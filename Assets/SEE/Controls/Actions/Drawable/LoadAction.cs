﻿using SEE.DataModel;
using SEE.DataModel.DG;
using SEE.Game;
using SEE.GO;
using SEE.Utils;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RootMotion.FinalIK.HitReaction;
using SEE.Net.Actions.Drawable;
using SEE.Net.Actions;
using Assets.SEE.Game.Drawable;
using Assets.SEE.Game;
using static SimpleFileBrowser.FileBrowser;
using SimpleFileBrowser;
using Assets.SEE.Game.UI.Drawable;
using UnityEngine.UI;
using SEE.Game.Drawable.Configurations;
using Text = SEE.Game.Drawable.Configurations.Text;

namespace SEE.Controls.Actions.Drawable
{
    /// <summary>
    /// Adds the <see cref="DrawableType"/> to the scene from one or more drawable configs saved in a file on the disk.
    /// </summary>
    public class LoadAction : AbstractPlayerAction
    {
        /// <summary>
        /// Represents how the file was loaded.
        /// LoadState.One is one drawbale in the file and it will be load the attached objects to that drawable.
        /// LoadState.OneSpecific is one drawbale in the file and it will be load the attached objects to a specific (maybe other) drawable.
        /// LoadState.More is for more drawbales in the file and it will be load the attached objects to that drawables.
        /// </summary>
        public enum LoadState
        {
            One,
            OneSpecific,
            More
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
            public readonly LoadState state;
            public DrawableConfig config;
            public GameObject specificDrawable;
            public DrawablesConfigs configs;

            public Memento(LoadState state)
            {
                this.state = state;
                this.specificDrawable = null;
                this.config = null;
                this.configs = null;
            }
        }

        /// <summary>
        /// The file path of the file that should be loaded.
        /// </summary>
        public static string filePath = "";

        /// <summary>
        /// This method manages the player's interaction with the mode <see cref="ActionStateType.Load"/>.
        /// </summary>
        /// <returns>Whether this Action is finished</returns>
        public override bool Update()
        {
            bool result = false;

            if (!Raycasting.IsMouseOverGUI())
            {
                if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
                {
                    GameObject.Find("UI Canvas").AddComponent<DrawableFileBrowser>().LoadDrawableConfiguration(LoadState.One);
                    memento = new Memento(LoadState.One);
                    currentState = ReversibleAction.Progress.InProgress;
                }

                if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift) && 
                    Raycasting.RaycastAnythingBackface(out RaycastHit raycastHit) &&
                    (raycastHit.collider.gameObject.CompareTag(Tags.Drawable) ||
                    GameDrawableFinder.hasDrawable(raycastHit.collider.gameObject)))
                {
                    GameObject.Find("UI Canvas").AddComponent<DrawableFileBrowser>().LoadDrawableConfiguration(LoadState.OneSpecific);
                    GameObject drawable = raycastHit.collider.gameObject.CompareTag(Tags.Drawable) ?
                        raycastHit.collider.gameObject : GameDrawableFinder.FindDrawable(raycastHit.collider.gameObject);
                    memento = new Memento(LoadState.OneSpecific);
                    memento.specificDrawable = drawable;
                    currentState = ReversibleAction.Progress.InProgress;
                }
                
                if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && !Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                {
                    GameObject.Find("UI Canvas").AddComponent<DrawableFileBrowser>().LoadDrawableConfiguration(LoadState.More);
                    memento = new Memento(LoadState.More);
                    currentState = ReversibleAction.Progress.InProgress;
                }

                if (!filePath.Equals("") && memento != null)
                {
                    switch(memento.state)
                    {
                        case LoadState.One:
                            DrawableConfig config = DrawableConfigManager.LoadDrawable(new FilePath(filePath));
                            GameObject drawable = GameDrawableFinder.Find(config.DrawableName, config.DrawableParentName);
                            Restore(drawable, config);
                            memento.config = config;
                            currentState = ReversibleAction.Progress.Completed;
                            result = true;
                            break;
                        case LoadState.OneSpecific:
                            DrawableConfig configSpecific = DrawableConfigManager.LoadDrawable(new FilePath(filePath));
                            Restore(memento.specificDrawable, configSpecific);
                            memento.config = configSpecific;
                            currentState = ReversibleAction.Progress.Completed;
                            result = true;
                            break;
                        case LoadState.More:
                            DrawablesConfigs configs = DrawableConfigManager.LoadDrawables(new FilePath(filePath));
                            foreach (DrawableConfig drawableConfig in configs.Drawables)
                            {
                                GameObject drawableOfFile = GameDrawableFinder.Find(drawableConfig.DrawableName, drawableConfig.DrawableParentName);
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
        /// <param name="drawable"></param>
        /// <param name="config"></param>
        private void Restore(GameObject drawable, DrawableConfig config)
        {
            RestoreLines(drawable, config.LineConfigs);
            RestoreTexts(drawable, config.TextConfigs);
        }

        /// <summary>
        /// This method restores the lines of the drawable configuration to the given drawable.
        /// </summary>
        /// <param name="drawable">The drawable on which the lines should be restored.</param>
        /// <param name="lines">The lines to be restored.</param>
        private void RestoreLines(GameObject drawable, List<Line> lines)
        {
            foreach (Line line in lines)
            {
                GameDrawer.ReDrawLine(drawable, line);
                new DrawOnNetAction(drawable.name, GameDrawableFinder.GetDrawableParentName(drawable), line).Execute();
            }
        }

        private void RestoreTexts(GameObject drawable, List<Text> texts)
        {
            foreach(Text text in texts)
            {
                GameTexter.ReWriteText(drawable, text);
                new WriteTextNetAction(drawable.name, GameDrawableFinder.GetDrawableParentName(drawable), text).Execute();
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
                GameObject drawable = GameDrawableFinder.FindDrawable(attachedObjects);
                string drawableParentName = GameDrawableFinder.GetDrawableParentName(drawable);
                List<DrawableType> allConfigs = new(config.LineConfigs);
                allConfigs.AddRange(config.TextConfigs); //TODO ADD THE OTHER DRAWABLE TYPES Configs!

                foreach (DrawableType type in allConfigs)
                {
                    GameObject typeObj = GameDrawableFinder.FindChild(attachedObjects, type.id);
                    new EraseNetAction(drawable.name, drawableParentName, typeObj.name).Execute();
                    Destroyer.Destroy(typeObj);
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
                case LoadState.One:
                    GameObject drawable = GameDrawableFinder.Find(memento.config.DrawableName, memento.config.DrawableParentName);
                    GameObject attachedObjects = GameDrawableFinder.GetAttachedObjectsObject(drawable);
                    DestroyLoadedObjects(attachedObjects, memento.config);
                    break;
                case LoadState.OneSpecific:
                    GameObject attachedObjs = GameDrawableFinder.GetAttachedObjectsObject(memento.specificDrawable);
                    DestroyLoadedObjects(attachedObjs, memento.config);
                    break;
                case LoadState.More:
                    foreach(DrawableConfig config in memento.configs.Drawables)
                    {
                        GameObject d = GameDrawableFinder.Find(config.DrawableName, config.DrawableParentName);
                        GameObject aO = GameDrawableFinder.GetAttachedObjectsObject(d);
                        DestroyLoadedObjects(aO, config);
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
                case LoadState.One:
                    GameObject drawable = GameDrawableFinder.Find(memento.config.DrawableName, memento.config.DrawableParentName);
                    Restore(drawable, memento.config);
                    break;
                case LoadState.OneSpecific:
                    Restore(memento.specificDrawable, memento.config);
                    break;
                case LoadState.More:
                    foreach(DrawableConfig config in memento.configs.Drawables)
                    {
                        GameObject d = GameDrawableFinder.Find(config.DrawableName, config.DrawableParentName);
                        Restore(d, config);
                    }
                    break;
            }
        }

        /// <summary>
        /// Stops the <see cref="LoadAction"/>. Resets the file path.
        /// </summary>
        public override void Stop()
        {
            filePath = "";
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
