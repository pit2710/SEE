﻿using SEE.Game;
using SEE.Utils;
using System.Collections.Generic;
using UnityEngine;
using SEE.Net.Actions.Drawable;
using Assets.SEE.Game.Drawable;
using SEE.Game.Drawable.Configurations;
using TextConf = SEE.Game.Drawable.Configurations.TextConf;
using SEE.Game.Drawable;
using Michsky.UI.ModernUIPack;
using SEE.GO;
using HighlightPlus;
using SEE.Game.UI.Notification;
using UnityEngine.Events;
using Assets.SEE.Game.UI.Drawable;

namespace SEE.Controls.Actions.Drawable
{
    /// <summary>
    /// Adds the <see cref="DrawableType"/> to the scene from one or more drawable configs saved in a file on the disk.
    /// </summary>
    public class LoadAction : AbstractPlayerAction
    {
        /// <summary>
        /// Represents how the file was loaded.
        /// LoadState.Regular is load the drawable(s) from file one-to-one into the same drawable.
        /// LoadState.Specific load the drawable(s) from the given file to one specific drawable.
        /// </summary>
        public enum LoadState
        {
            Regular,
            Specific
        }
        /// <summary>
        /// Saves all the information needed to revert or repeat this action.
        /// </summary>
        private Memento memento;

        /// <summary>
        /// This struct can store all the information needed to revert or repeat a <see cref="LoadAction"/>
        /// </summary>
        private class Memento
        {
            /// <summary>
            /// The load state of the action
            /// </summary>
            public readonly LoadState state;
            /// <summary>
            /// The specific chosen drawable (needed for LoadState.Specific)
            /// </summary>
            public DrawableConfig specificDrawable;
            /// <summary>
            /// The drawable configurations.
            /// </summary>
            public DrawablesConfigs configs;

            public List<DrawableConfig> addedDrawables;

            /// <summary>
            /// The constructor, which simply assigns its only parameter to a field in this class.
            /// </summary>
            /// <param name="state">The kind how the file was loaded.</param>
            public Memento(LoadState state)
            {
                this.state = state;
                this.specificDrawable = null;
                this.configs = null;
                this.addedDrawables = new();
            }
        }

        /// <summary>
        /// Ensures that per click is only saved once.
        /// </summary>
        private bool clicked = false;

        /// <summary>
        /// The selected drawable for specific loading.
        /// </summary>
        private GameObject selectedDrawable;

        /// <summary>
        /// The instance for the drawable file browser
        /// </summary>
        private DrawableFileBrowser browser;

        /// <summary>
        /// Creates the load menu and adds the neccressary Handler for the buttons.
        /// </summary>
        public override void Awake()
        {
            UnityAction loadButtonCall = () =>
            {
                if (browser == null || (browser != null && !browser.IsOpen()))
                {
                    browser = GameObject.Find("UI Canvas").AddOrGetComponent<DrawableFileBrowser>();
                    browser.LoadDrawableConfiguration(LoadState.Regular);
                    memento = new Memento(LoadState.Regular);
                }
            };
            UnityAction loadSpecificButtonCall = () =>
            {
                if (browser == null || (browser != null && !browser.IsOpen()))
                {
                    if (selectedDrawable != null)
                    {
                        browser = GameObject.Find("UI Canvas").AddOrGetComponent<DrawableFileBrowser>();
                        browser.LoadDrawableConfiguration(LoadState.Specific);
                        memento = new Memento(LoadState.Specific);
                    } else
                    {
                        ShowNotification.Warn("No drawable selected.", "Select a drawable to load specific.");
                    }
                }
            };

            LoadMenu.Enable(loadButtonCall, loadSpecificButtonCall);
        }

        /// <summary>
        /// Stops the <see cref="LoadAction"/>.
        /// Destroys the load menu and if there are still an activ
        /// highlight effect
        /// </summary>
        public override void Stop()
        {
            LoadMenu.Disable();

            if (selectedDrawable != null && selectedDrawable.GetComponent<HighlightEffect>() != null)
            {
                Destroyer.Destroy(selectedDrawable.GetComponent<HighlightEffect>());
            }
        }

        /// <summary>
        /// This method manages the player's interaction with the mode <see cref="ActionStateType.Load"/>.
        /// </summary>
        /// <returns>Whether this Action is finished</returns>
        public override bool Update()
        {
            bool result = false;

            if (!Raycasting.IsMouseOverGUI())
            {
                /// This block marks the selected drawable and adds it to a list. If it has already been selected, it is removed from the list, and the marking is cleared.
                /// For execution, no open file browser should exist.
                if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
                    && !clicked &&
                    Raycasting.RaycastAnything(out RaycastHit hit) &&
                    (GameFinder.hasDrawable(hit.collider.gameObject) || hit.collider.gameObject.CompareTag(Tags.Drawable))
                    && (browser == null || (browser != null && !browser.IsOpen())))
                {
                    clicked = true;
                    GameObject drawable = hit.collider.gameObject.CompareTag(Tags.Drawable) ?
                        hit.collider.gameObject : GameFinder.GetDrawable(hit.collider.gameObject);

                    if (drawable.GetComponent<HighlightEffect>() == null)
                    {
                        if (selectedDrawable != null && selectedDrawable.GetComponent<HighlightEffect>() != null)
                        {
                            Destroyer.Destroy(selectedDrawable.GetComponent<HighlightEffect>());
                        }
                        selectedDrawable = drawable;
                        HighlightEffect effect = selectedDrawable.AddComponent<HighlightEffect>();
                        effect.highlighted = true;
                        effect.previewInEditor = false;
                        effect.outline = 0;
                        effect.glowQuality = HighlightPlus.QualityLevel.Highest;
                        effect.glow = 1.0f;
                        effect.glowHQColor = Color.yellow;
                        effect.overlay = 1.0f;
                        effect.overlayColor = Color.magenta;
                    }
                    else
                    {
                        Destroyer.Destroy(drawable.GetComponent<HighlightEffect>());
                        selectedDrawable = null;
                    }
                }

                /// Needed for select more drawables to save.
                if (Input.GetMouseButtonUp(0))
                {
                    clicked = false;
                }
                /// This block will be executed when a file was successfully chosen.
                if (browser != null && browser.TryGetFilePath(out string filePath) && memento != null)
                {
                    switch (memento.state)
                    {
                        /// This block loads one drawable onto the specific chosen drawable.
                        case LoadState.Specific:
                            memento.specificDrawable = DrawableConfigManager.GetDrawableConfig(selectedDrawable);
                            DrawablesConfigs configsSpecific = DrawableConfigManager.LoadDrawables(new FilePath(filePath));
                            foreach (DrawableConfig drawableConfig in configsSpecific.Drawables)
                            {
                                Restore(memento.specificDrawable.GetDrawable(), drawableConfig);
                            }
                            memento.configs = configsSpecific;
                            currentState = ReversibleAction.Progress.Completed;
                            result = true;
                            break;
                        /// This block loads one or more drawables onto the drawables of the configuration.
                        case LoadState.Regular:
                            DrawablesConfigs configs = DrawableConfigManager.LoadDrawables(new FilePath(filePath));
                            foreach (DrawableConfig drawableConfig in configs.Drawables)
                            {
                                GameObject drawableOfFile = GameFinder.FindDrawable(drawableConfig.ID, drawableConfig.ParentID);
                                /// If the drawable does not exist it will be spawned as a sticky note.
                                if (drawableOfFile == null)
                                {
                                    memento.addedDrawables.Add(drawableConfig);
                                    GameObject stickyNote = GameStickyNoteManager.Spawn(drawableConfig);
                                    drawableOfFile = GameFinder.GetDrawable(stickyNote);
                                    new StickyNoteSpawnNetAction(drawableConfig).Execute();
                                }
                                Restore(drawableOfFile, drawableConfig);
                            }
                            memento.configs = configs;
                            currentState = ReversibleAction.Progress.Completed;
                            result = true;
                            break;
                    }
                } 
            }
            return result;
        }

        /// <summary>
        /// Restores all the objects of the configuration.
        /// </summary>
        /// <param name="drawable">The drawable on that the config should restore.</param>
        /// <param name="config">The configuration which holds the drawable type configuration to restore.</param>
        private void Restore(GameObject drawable, DrawableConfig config)
        {
            GameObject attachedObject = GameFinder.GetAttachedObjectsObject(drawable);
            if (attachedObject != null)
            {
                GameMindMap.RenameMindMap(config, attachedObject);
            }
            foreach (DrawableType type in config.GetAllDrawableTypes())
            {
                if (attachedObject != null && type is not MindMapNodeConf)
                {
                    CheckAndChangeID(type, attachedObject, DrawableType.GetPrefix(type));
                }
                DrawableType.Restore(type, drawable);
            }
        }

        /// <summary>
        /// When the id of the given config already exist on the drawable, the id will be changed.
        /// </summary>
        /// <param name="conf">The configuration to restore.</param>
        /// <param name="attachedObjects">The objects that are attached on a drawable</param>
        /// <param name="prefix">The prefix for the drawable type object.</param>
        private void CheckAndChangeID (DrawableType conf, GameObject attachedObjects, string prefix)
        {
            if (GameFinder.FindChild(attachedObjects, conf.id) != null && !conf.id.Contains(ValueHolder.MindMapBranchLine))
            {
                string newName = prefix + "-" + DrawableHolder.GetRandomString(8);
                while (GameFinder.FindChild(attachedObjects, newName) != null)
                {
                    newName = prefix + "-" + DrawableHolder.GetRandomString(8);
                }
                conf.id = newName;
            }
        }


        /// <summary>
        /// Destroyes the objects, which was loaded from the configuration.
        /// </summary>
        /// <param name="attachedObjects"></param>
        /// <param name="config"></param>
        private void DestroyLoadedObjects(GameObject attachedObjects, DrawableConfig config)
        {
            if (attachedObjects != null)
            {
                GameObject drawable = GameFinder.GetDrawable(attachedObjects);
                string drawableParentName = GameFinder.GetDrawableParentName(drawable);

                foreach (DrawableType type in config.GetAllDrawableTypes())
                {
                    GameObject typeObj = GameFinder.FindChild(attachedObjects, type.id);
                    if (typeObj != null)
                    {
                        new EraseNetAction(drawable.name, drawableParentName, typeObj.name).Execute();
                        Destroyer.Destroy(typeObj);
                    }
                }
            }
        }

        /// <summary>
        /// Reverts this instance of the action, i.e., deletes the attached object that was loaded from the file.
        /// </summary>
        public override void Undo()
        {
            base.Undo();
            switch (memento.state)
            {
                case LoadState.Specific:
                    GameObject attachedObjs = GameFinder.GetAttachedObjectsObject(memento.specificDrawable.GetDrawable());
                    foreach (DrawableConfig config in memento.configs.Drawables)
                    {
                        DestroyLoadedObjects(attachedObjs, config);
                    }
                    break;
                case LoadState.Regular:
                    foreach (DrawableConfig config in memento.configs.Drawables)
                    {
                        if (memento.addedDrawables.Contains(config))
                        {
                            GameObject drawable = GameFinder.FindDrawable(config.ID, config.ParentID);
                            new StickyNoteDeleterNetAction(GameFinder.GetHighestParent(drawable).name).Execute();
                            Destroyer.Destroy(GameFinder.GetHighestParent(drawable));
                        }
                        else
                        {
                            GameObject drawable = GameFinder.FindDrawable(config.ID, config.ParentID);
                            GameObject attachedObj = GameFinder.GetAttachedObjectsObject(drawable);
                            DestroyLoadedObjects(attachedObj, config);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Repeats this action, i.e., loades the configuration again.
        /// </summary>
        public override void Redo()
        {
            base.Redo();
            switch (memento.state)
            {
                case LoadState.Specific:
                    GameObject specificDrawable = memento.specificDrawable.GetDrawable();
                    foreach (DrawableConfig config in memento.configs.Drawables)
                    {
                        Restore(specificDrawable, config);
                    }
                    break;
                case LoadState.Regular:
                    foreach (DrawableConfig config in memento.configs.Drawables)
                    {
                        GameObject drawable = GameFinder.FindDrawable(config.ID, config.ParentID);
                        if (drawable == null)
                        {
                            drawable = GameFinder.GetDrawable(GameStickyNoteManager.Spawn(config));
                            new StickyNoteSpawnNetAction(config).Execute();
                        }
                        Restore(drawable, config);
                    }
                    break;
            }
        }

        /// <summary>
        /// A new instance of <see cref="LoadAction"/>.
        /// See <see cref="ReversibleAction.CreateReversibleAction"/>.
        /// </summary>
        /// <returns>new instance of <see cref="LoadAction"/></returns>
        public static ReversibleAction CreateReversibleAction()
        {
            return new LoadAction();
        }

        /// <summary>
        /// A new instance of <see cref="LoadAction"/>.
        /// See <see cref="ReversibleAction.NewInstance"/>.
        /// </summary>
        /// <returns>new instance of <see cref="LoadAction"/></returns>
        public override ReversibleAction NewInstance()
        {
            return CreateReversibleAction();
        }

        /// <summary>
        /// Returns the <see cref="ActionStateType"/> of this action.
        /// </summary>
        /// <returns><see cref="ActionStateType.Load"/></returns>
        public override ActionStateType GetActionStateType()
        {
            return ActionStateTypes.Load;
        }

        /// <summary>
        /// The set of IDs of all gameObjects changed by this action.
        /// <see cref="ReversibleAction.GetActionStateType"/>
        /// Because this action does not actually change any game object, 
        /// an empty set is always returned.
        /// </summary>
        /// <returns>an empty set</returns>
        public override HashSet<string> GetChangedObjects()
        {
            return new HashSet<string>();
        }
    }
}
