﻿using SEE.Game;
using SEE.Game.Drawable;
using SEE.Game.Drawable.Configurations;
using SEE.Game.UI.Drawable;
using SEE.Game.UI.Menu.Drawable;
using SEE.Game.UI.Notification;
using SEE.Game.UI.PropertyDialog.Drawable;
using SEE.Net.Actions.Drawable;
using SEE.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Controls.Actions.Drawable
{
    /// <summary>
    /// Adds a text to a drawable.
    /// </summary>
    public class WriteTextAction : AbstractPlayerAction
    {
        /// <summary>
        /// Value that represents the first start of this action.
        /// </summary>
        public static bool firstStart = true;

        /// <summary>
        /// The game object that holds the TextMeshPro component.
        /// </summary>
        private GameObject textObj;

        /// <summary>
        /// The drawable on that the text should be displayed.
        /// </summary>
        private GameObject drawable;

        /// <summary>
        /// The position on the drawable where the text should be displayed.
        /// </summary>
        private Vector3 position;

        /// <summary>
        /// Saves all the information needed to revert or repeat this action.
        /// </summary>
        private Memento memento;

        /// <summary>
        /// This field can hold a reference to the dialog that the player will see in the process of executing this.
        /// </summary>
        private WriteEditTextDialog writeTextDialog;

        /// <summary>
        /// Indicates how far this instance has progressed in write a text on a drawable.
        /// </summary>
        private ProgressState progress = ProgressState.GettingPosition;

        /// <summary>
        /// Represents the different stages of progress of this action.
        /// </summary>
        private enum ProgressState
        {
            GettingPosition,
            GettingText
        }

        /// <summary>
        /// This struct can store all the information needed to revert or repeat a <see cref="WriteTextAction"/>
        /// </summary>
        private struct Memento
        {
            /// <summary>
            /// The drawable on that the text should be displayed
            /// </summary>
            public DrawableConfig drawable;

            /// <summary>
            /// The written text.
            /// </summary>
            public TextConf text;

            /// <summary>
            /// The constructor, which simply assigns its only parameter to a field in this class.
            /// </summary>
            /// <param name="drawable">The drawable on that the text should be displayed.</param>
            /// <param name="text">The written text</param>
            public Memento(GameObject drawable, TextConf text)
            {
                this.drawable = DrawableConfigManager.GetDrawableConfig(drawable);
                this.text = text;
            }
        }

        /// <summary>
        /// Resets the action values.
        /// </summary>
        public static void Reset()
        {
            firstStart = true;
        }

        /// <summary>
        /// Enables the text menu.
        /// When it starts the first time, it will add necessary listeners.
        /// </summary>
        public override void Awake()
        {
            if (firstStart)
            {
                GameObject.Find("UI Canvas").AddComponent<ValueResetter>()
                    .SetAllowedState(GetActionStateType());
                TextMenu.EnableForWriting();
                firstStart = false;
            }
            else
            {
                TextMenu.Enable(false);
            }
        }

        /// <summary>
        /// This method manages the player's interaction with the mode <see cref="ActionStateType.WriteText"/>.
        /// </summary>
        /// <returns>Whether this Action is finished</returns>
        public override bool Update()
        {
            if (!Raycasting.IsMouseOverGUI())
            {
                switch (progress)
                {
                    /// Block for getting the text position.
                    case ProgressState.GettingPosition:
                        GettingPosition();
                        return false;

                    /// Block for getting the text.
                    case ProgressState.GettingText:
                        return GettingText();
                    default:
                        return false;

                }
            }
            return false;
        }

        /// <summary>
        /// Identifying where the text should be placed. 
        /// The <see cref="WriteEditTextDialog"/> is then opened.
        /// The user can enter the text in it.
        /// Changes the progress state to <see cref="ProgressState.GettingText"/>
        /// </summary>
        private void GettingPosition()
        {
            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
                && Raycasting.RaycastAnything(out RaycastHit raycastHit)
                && (GameFinder.hasDrawable(raycastHit.collider.gameObject)
                    || raycastHit.collider.gameObject.CompareTag(Tags.Drawable)))
            {
                drawable = raycastHit.collider.gameObject.CompareTag(Tags.Drawable) ?
                    raycastHit.collider.gameObject : GameFinder.GetDrawable(raycastHit.collider.gameObject);
                position = raycastHit.point;
                progress = ProgressState.GettingText;
                writeTextDialog = new WriteEditTextDialog();
                writeTextDialog.Open();
            }
        }

        /// <summary>
        /// Gets the user input for the drawable text and create the drawable text if the text is not empty.
        /// Because the mesh renderer of TextMeshPro takes some time to generate the mesh, 
        /// the mesh collider of the text must be refreshed.
        /// When the text for the drawable text is not empty the action will be finished.
        /// 
        /// If the dialog was canceled or the user input is empty, then the action is resets.
        /// </summary>
        /// <returns>Whatever the success of creating is.</returns>
        private bool GettingText()
        {
            if (writeTextDialog.GetUserInput(out string textOut))
            {
                if (textOut != null && textOut != "")
                {
                    textObj = GameTexter.WriteText(drawable, textOut, position, 
                        ValueHolder.currentPrimaryColor, ValueHolder.currentSecondaryColor,
                        ValueHolder.currentOutlineThickness, ValueHolder.currentFontSize, 
                        ValueHolder.currentOrderInLayer, TextMenu.GetFontStyle());
                    new WriteTextNetAction(drawable.name, GameFinder.GetDrawableParentName(drawable), 
                        TextConf.GetText(textObj)).Execute();
                    memento = new Memento(drawable, TextConf.GetText(textObj));
                    GameTexter.RefreshMeshCollider(textObj);
                    currentState = ReversibleAction.Progress.Completed;
                    return true;
                }
                else
                {
                    ShowNotification.Warn("Empty text", "The text to write is empty. Please add one.");
                    progress = ProgressState.GettingPosition;
                    return false;
                }
            }
            /// If the dialog was canceled the action will starts from beginning.
            if (writeTextDialog.WasCanceled())
            {
                progress = ProgressState.GettingPosition;
            }
            return false;
        }

        /// <summary>
        /// Stops the <see cref="WriteTextAction"/>.
        /// Refreshes the mesh collider of the text.
        /// It is necessary because the MeshRenderer needs some time to generate and deploy the mesh.
        /// </summary>
        public override void Stop()
        {
            TextMenu.Disable();
        }


        /// <summary>
        /// Reverts this action, i.e., deletes the text was was written on the drawable.
        /// </summary>
        public override void Undo()
        {
            base.Undo();
            GameObject obj = GameFinder.FindChild(memento.drawable.GetDrawable(), memento.text.id);
            new EraseNetAction(memento.drawable.ID, memento.drawable.ParentID, memento.text.id).Execute();
            Destroyer.Destroy(obj);
        }

        /// <summary>
        /// Repeats this action, i.e., writes the text again on the drawable.
        /// </summary>
        public override void Redo()
        {
            base.Redo();
            GameTexter.ReWriteText(memento.drawable.GetDrawable(), memento.text);
            new WriteTextNetAction(memento.drawable.ID, memento.drawable.ParentID, memento.text).Execute();

        }

        /// <summary>
        /// A new instance of <see cref="WriteTextAction"/>.
        /// See <see cref="ReversibleAction.CreateReversibleAction"/>.
        /// </summary>
        /// <returns>new instance of <see cref="WriteTextAction"/></returns>
        public static ReversibleAction CreateReversibleAction()
        {
            return new WriteTextAction();
        }

        /// <summary>
        /// A new instance of <see cref="WriteTextAction"/>.
        /// See <see cref="ReversibleAction.NewInstance"/>.
        /// </summary>
        /// <returns>new instance of <see cref="WriteTextAction"/></returns>
        public override ReversibleAction NewInstance()
        {
            return CreateReversibleAction();
        }

        /// <summary>
        /// Returns the <see cref="ActionStateType"/> of this action.
        /// </summary>
        /// <returns><see cref="ActionStateType.WriteText"/></returns>
        public override ActionStateType GetActionStateType()
        {
            return ActionStateTypes.WriteText;
        }

        /// <summary>
        /// The set of IDs of all gameObjects changed by this action.
        /// <see cref="ReversibleAction.GetActionStateType"/>
        /// Because this action does not actually change any game object, 
        /// an empty set is always returned.
        /// </summary>
        /// <returns>the id of the created drawable text</returns>
        public override HashSet<string> GetChangedObjects()
        {
            return new HashSet<string> { memento.text.id };
        }
    }
}