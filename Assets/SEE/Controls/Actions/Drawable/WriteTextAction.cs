﻿using Assets.SEE.Game;
using Assets.SEE.Game.Drawable;
using Assets.SEE.Game.UI.Drawable;
using RTG;
using SEE.Controls.Actions;
using SEE.DataModel.DG;
using SEE.Game;
using SEE.Game.Drawable.Configurations;
using SEE.Game.UI.Notification;
using SEE.Game.UI.PropertyDialog.Drawable;
using SEE.Net.Actions.Drawable;
using SEE.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        private class Memento
        {
            public GameObject drawable;
            public Text text;
            public Memento(GameObject drawable, Text text)
            {
                this.drawable = drawable;
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
        /// Enables the text menu
        /// </summary>
        public override void Awake()
        {
            if (firstStart)
            {
                TextMenu.enableTextMenu((color => ValueHolder.currentColor = color), ValueHolder.currentColor, true);
                GameObject.Find("UI Canvas").AddComponent<ValueResetter>().SetAllowedState(GetActionStateType());

                TextMenu.GetFontColorButton().onClick.AddListener(() =>
                {
                    TextMenu.AssignColorArea((color => ValueHolder.currentColor = color), ValueHolder.currentColor);
                });
                TextMenu.GetOutlineColorButton().onClick.AddListener(() =>
                {
                    if (ValueHolder.currentSecondColor == Color.clear)
                    {
                        ValueHolder.currentSecondColor = Random.ColorHSV();
                    }
                    if (ValueHolder.currentSecondColor.a == 0)
                    {
                        ValueHolder.currentSecondColor = new Color(ValueHolder.currentSecondColor.r, ValueHolder.currentSecondColor.g, ValueHolder.currentSecondColor.b, 255);
                    }
                    TextMenu.AssignColorArea((color => ValueHolder.currentSecondColor = color), ValueHolder.currentSecondColor);
                });
                TextMenu.AssignOutlineThickness((thickness => ValueHolder.currentOutlineThickness = thickness), ValueHolder.currentOutlineThickness);
                TextMenu.AssignFontSize(size => ValueHolder.currentFontSize = size, ValueHolder.currentFontSize);

                firstStart = false;
            }
            else
            {
                TextMenu.enableTextMenu(false);
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
                    case ProgressState.GettingPosition:
                        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) &&
                             Raycasting.RaycastAnythingBackface(out RaycastHit raycastHit) &&
                            (GameDrawableFinder.hasDrawable(raycastHit.collider.gameObject) || raycastHit.collider.gameObject.CompareTag(Tags.Drawable)))
                        {
                            drawable = raycastHit.collider.gameObject.CompareTag(Tags.Drawable) ?
                                raycastHit.collider.gameObject : GameDrawableFinder.FindDrawable(raycastHit.collider.gameObject);
                            position = raycastHit.point;
                            progress = ProgressState.GettingText;
                            writeTextDialog = new WriteEditTextDialog();
                            writeTextDialog.Open();
                        }
                        return false;

                    case ProgressState.GettingText:
                        if (writeTextDialog.GetUserInput(out string textOut))
                        {
                            if (textOut != null && textOut != "")
                            {
                                textObj = GameTexter.WriteText(drawable, textOut, position, ValueHolder.currentColor, ValueHolder.currentSecondColor,
                                    ValueHolder.currentOutlineThickness, ValueHolder.currentFontSize, ValueHolder.currentOrderInLayer, TextMenu.GetFontStyle());
                                new WriteTextNetAction(drawable.name, GameDrawableFinder.GetDrawableParentName(drawable), Text.GetText(textObj)).Execute();
                                memento = new Memento(drawable, Text.GetText(textObj));
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

                        if (writeTextDialog.WasCanceled())
                        {
                            progress = ProgressState.GettingPosition;
                        }
                        return false;
                    default:
                        return false;

                }
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
            TextMenu.disableTextMenu();
        }


        /// <summary>
        /// Reverts this action, i.e., deletes the text was was written on the drawable.
        /// </summary>
        public override void Undo()
        {
            base.Undo();
            GameObject obj = GameDrawableFinder.FindChild(memento.drawable, memento.text.id);
            new EraseNetAction(memento.drawable.name, GameDrawableFinder.GetDrawableParentName(memento.drawable), memento.text.id).Execute();
            Destroyer.Destroy(obj);
        }

        /// <summary>
        /// Repeats this action, i.e., writes the text again on the drawable.
        /// </summary>
        public override void Redo()
        {
            base.Redo();
            GameTexter.ReWriteText(memento.drawable, memento.text);
            new WriteTextNetAction(memento.drawable.name, GameDrawableFinder.GetDrawableParentName(memento.drawable), memento.text).Execute();

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
        /// <returns>an empty set</returns>
        public override HashSet<string> GetChangedObjects()
        {
            return new HashSet<string>();
        }
    }
}