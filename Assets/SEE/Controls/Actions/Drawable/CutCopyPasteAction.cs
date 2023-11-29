﻿using Assets.SEE.Game.Drawable;
using Assets.SEE.Game.UI.Drawable;
using Michsky.UI.ModernUIPack;
using SEE.Game;
using SEE.Game.Drawable;
using SEE.Game.Drawable.Configurations;
using SEE.GO;
using SEE.Net.Actions.Drawable;
using SEE.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Controls.Actions.Drawable
{
    /// <summary>
    /// This action provides the cut and copy functionality for drawable types.
    /// </summary>
    public class CutCopyPasteAction : AbstractPlayerAction
    {
        /// <summary>
        /// Holds the current progress state.
        /// </summary>
        private ProgressState progressState = ProgressState.SelectObject;

        /// <summary>
        /// The progress states of the <see cref="CutCopyPasteAction"/>
        /// </summary>
        private enum ProgressState
        {
            SelectObject,
            CutCopyPaste,
            OpenSelectParentMenu,
            SelectParent,
            Finish
        }

        /// <summary>
        /// Holds the current state.
        /// </summary>
        private CutCopy state = CutCopy.None;

        /// <summary>
        /// The state of cut or copy is chosen.
        /// </summary>
        private enum CutCopy
        {
            None,
            Cut,
            Copy
        }

        /// <summary>
        /// This struct can store all the information needed to revert or repeat a <see cref="CutCopyPasteAction"/>
        /// </summary>
        private Memento memento;

        /// <summary>
        /// This struct can store all the information needed to revert or repeat a <see cref="CutCopyPasteAction"/>
        /// </summary>
        private struct Memento
        {
            /// <summary>
            /// The old values of the drawable type object.
            /// </summary>
            public readonly DrawableType oldValueHolder;
            /// <summary>
            /// The new values of the drawable type object.
            /// </summary>
            public readonly DrawableType newValueHolder;
            /// <summary>
            /// The drawable on that the drawable type object was displayed.
            /// </summary>
            public readonly DrawableConfig oldDrawable;
            /// <summary>
            /// The drawable on that the drawable type object is now displayed.
            /// </summary>
            public readonly DrawableConfig newDrawable;
            /// <summary>
            /// Holds the original configurations for the children and branches of an mind map node.
            /// </summary>
            public DrawableConfig oldNodesHolder;
            /// <summary>
            /// Holds the new configurations for the children and branches of an mind map node.
            /// </summary>
            public DrawableConfig newNodesHolder;
            /// <summary>
            /// The old branch line config, will needed for restore the optical data of it.
            /// </summary>
            public LineConf oldBranchLineConfig;
            /// <summary>
            /// The state if it was copied or cutted.
            /// </summary>
            public readonly CutCopy state;

            /// <summary>
            /// The constructor, which simply assigns its only parameter to a field in this class.
            /// </summary>
            /// <param name="oldValueHolder">The old values of the drawable type object.</param>
            /// <param name="newValueHolder">The new edited values of the drawable type object.</param>
            /// <param name="oldDrawable">The drawable on that the drawable type object was displayed.</param>
            /// <param name="newDrawable">The drawable on that the drawable type object is displayed.</param>
            /// <param name="cutCopy">The stat if it was cutted or copied.</param>
            public Memento(DrawableType oldValueHolder,
                DrawableType newValueHolder, GameObject oldDrawable, GameObject newDrawable, CutCopy cutCopy)
            {
                this.oldValueHolder = oldValueHolder;
                this.newValueHolder = newValueHolder;
                this.oldDrawable = DrawableConfigManager.GetDrawableConfig(oldDrawable);
                this.newDrawable = DrawableConfigManager.GetDrawableConfig(newDrawable);
                state = cutCopy;
                oldNodesHolder = null;
                newNodesHolder = null;
                oldBranchLineConfig = null;
            }
        }

        /// <summary>
        /// The prefab of the cut copy paste menu.
        /// </summary>
        private const string cutCopyPasteMenuPrefab = "Prefabs/UI/Drawable/CutCopyPaste";
        /// <summary>
        /// The instance of the cut copy paste menu
        /// </summary>
        private GameObject cutCopyPasteMenu;

        /// <summary>
        /// The new created object.
        /// </summary>
        private GameObject newObject;

        /// <summary>
        /// The selected drawable type object that should be cut or copied.
        /// </summary>
        private GameObject selectedObj;

        /// <summary>
        /// The id of the old selected object of the last run.
        /// </summary>
        private static string oldSelectedId;

        /// <summary>
        /// Bool that represents that the left mouse button was released after finish.
        /// It is necessary to prevent the previously selected object from being accidentally selected again. 
        /// After the action has successfully completed, it starts again, allowing for the selection of a new object. 
        /// This option enables the immediate selection of another object while holding down the mouse button.
        /// </summary>
        private static bool mouseWasReleased = true;

        /// <summary>
        /// The old values of the selected drawable type.
        /// </summary>
        private DrawableType oldValueHolder;

        /// <summary>
        /// The new values of the selected drawable type.
        /// </summary>
        private DrawableType newValueHolder;

        /// <summary>
        /// The drawable where the selected object was displayed.
        /// </summary>
        private GameObject oldDrawable;

        /// <summary>
        /// The drawable where the new object is displayed.
        /// </summary>
        private GameObject newDrawable;

        /// <summary>
        /// The configuration that includes the node children and their branch lines.
        /// Only necessary for interacting with a <see cref="MindMapNodeConf"/>
        /// </summary>
        private DrawableConfig oldNodesBranchLineHolder = null;
        /// <summary>
        /// The configuration that includes the changed node children and their branch lines.
        /// Only necessary for interacting with a <see cref="MindMapNodeConf"/>
        /// </summary>
        private DrawableConfig newNodesBranchLineHolder = null;
        /// <summary>
        /// The configuration of the old branch line to parent.
        /// </summary>
        private LineConf oldBranchLineConf = null;
        /// <summary>
        /// Query that ensures the optical data from the previous branch line is taken over.
        /// </summary>
        private bool editToOldBranchLine = false;

        /// <summary>
        /// Resets the old selected object, if the action state will leave.
        /// </summary>
        public static void Reset()
        {
            oldSelectedId = "";
            mouseWasReleased = true;
        }

        /// <summary>
        /// Deactivates the blink effect if, it is still active and destroys cut copy paste menu.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            if (selectedObj != null && selectedObj.GetComponent<BlinkEffect>() != null)
            {
                selectedObj.GetComponent<BlinkEffect>().Deactivate();
            }

            if (cutCopyPasteMenu != null)
            {
                Destroyer.Destroy(cutCopyPasteMenu);
            }
        }

        /// <summary>
        /// Adds the necessary Handler for the cut, copy and paste button.
        /// </summary>
        /// <param name="menu">The instance of the cut copy paste menu</param>
        private void SetupButtons(GameObject menu)
        {
            Transform content = menu.transform.Find("Content");
            ButtonManagerBasic cut = content.Find("Cut").GetComponent<ButtonManagerBasic>();
            ButtonManagerBasic copy = content.Find("Copy").GetComponent<ButtonManagerBasic>();

            cut.clickEvent.AddListener(() =>
            {
                state = CutCopy.Cut;
            });

            copy.clickEvent.AddListener(() =>
            {
                state = CutCopy.Copy;
            });
        }

        /// <summary>
        /// This method manages the player's interaction with the mode <see cref="ActionStateType.CutCopyPaste"/>.
        /// It allows to cut or copy drawable type objects and paste them on a specific position on a specific drawbale.
        /// </summary>
        /// <returns>Whether this Action is finished</returns>
        public override bool Update()
        {
            if (!Raycasting.IsMouseOverGUI())
            {
                switch (progressState)
                {
                    /// Block for selecting the drawable type object to copy/cut.
                    case ProgressState.SelectObject:
                        SelectObject();
                        break;

                    /// Block in which the object is duplicated at the desired location, and the original may be deleted if necessary.
                    case ProgressState.CutCopyPaste:
                        CutCopyPaste();
                        break;

                    /// Block in which the select parent menu will be open.
                    case ProgressState.OpenSelectParentMenu:
                        OpenSelectParent();
                        break;

                    /// Block in which it will wait for the user selection.
                    case ProgressState.SelectParent:
                        SelectParent();
                        break;

                    /// Block to finish this action.
                    case ProgressState.Finish:
                        mouseWasReleased = false;
                        memento = new Memento(oldValueHolder, newValueHolder, oldDrawable, newDrawable, state);
                        memento.oldNodesHolder = oldNodesBranchLineHolder;
                        memento.newNodesHolder = newNodesBranchLineHolder;
                        memento.oldBranchLineConfig = oldBranchLineConf;
                        currentState = ReversibleAction.Progress.Completed;
                        return true;

                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Allows the user to choose a Drawable Type Object for copying/cutting.
        /// It's activate the blink effect and enables the cut copy paste menu.
        /// </summary>
        private void SelectObject()
        {
            if (Input.GetMouseButtonUp(0) && selectedObj == null)
            {
                mouseWasReleased = true;
            }
            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && selectedObj == null &&
                Raycasting.RaycastAnything(out RaycastHit raycastHit) &&
                (oldSelectedId == "" || oldSelectedId != raycastHit.collider.gameObject.name ||
                    oldSelectedId == raycastHit.collider.gameObject.name && mouseWasReleased)
                && Tags.DrawableTypes.Contains(raycastHit.collider.gameObject.tag))
            {
                selectedObj = raycastHit.collider.gameObject;
                oldDrawable = GameFinder.GetDrawable(selectedObj);
                oldSelectedId = selectedObj.name;
                oldValueHolder = DrawableType.Get(selectedObj);

                if (selectedObj.CompareTag(Tags.MindMapNode))
                {
                    oldNodesBranchLineHolder = GameMindMap.SummarizeSelectedNodeIncChildren(selectedObj);
                    newNodesBranchLineHolder = GameMindMap.SummarizeSelectedNodeIncChildren(selectedObj);

                }

                BlinkEffect effect = selectedObj.AddOrGetComponent<BlinkEffect>();
                effect.SetAllowedActionStateType(GetActionStateType());

                if (GameObject.Find("UI Canvas").GetComponent<ValueResetter>() == null)
                {
                    GameObject.Find("UI Canvas").AddComponent<ValueResetter>().SetAllowedState(GetActionStateType());
                }
                cutCopyPasteMenu = PrefabInstantiator.InstantiatePrefab(cutCopyPasteMenuPrefab,
                            GameObject.Find("UI Canvas").transform, false);
                SetupButtons(cutCopyPasteMenu);

            }
            if (Input.GetMouseButtonUp(0) && selectedObj != null)
            {
                progressState = ProgressState.CutCopyPaste;
            }
        }

        /// <summary>
        /// Provides the interaction for cut/copy/paste.
        /// </summary>
        private void CutCopyPaste()
        {
            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && selectedObj != null &&
                 selectedObj.GetComponent<BlinkEffect>() != null && state != CutCopy.None &&
                 Raycasting.RaycastAnything(out RaycastHit hit) &&
                 (hit.collider.gameObject.CompareTag(Tags.Drawable) ||
                     GameFinder.hasDrawable(hit.collider.gameObject)))
            {
                Destroyer.Destroy(cutCopyPasteMenu);
                selectedObj.GetComponent<BlinkEffect>().Deactivate();
                Vector3 newPosition = hit.point;
                newDrawable = hit.collider.gameObject.CompareTag(Tags.Drawable) ?
                        hit.collider.gameObject : GameFinder.GetDrawable(hit.collider.gameObject);
                switch (DrawableType.Get(selectedObj))
                {
                    case LineConf:
                        ProcessLine(newPosition);
                        break;
                    case TextConf:
                        ProcessText(newPosition);
                        break;
                    case ImageConf:
                        ProcessImage(newPosition);
                        break;
                    case MindMapNodeConf:
                        ProcessMindMapNode(newPosition);
                        break;
                }
                newValueHolder = DrawableType.Get(newObject);
                Cut();
                if (newObject.CompareTag(Tags.MindMapNode) &&
                    newObject.GetComponent<MMNodeValueHolder>().GetNodeKind() != GameMindMap.NodeKind.Theme)
                {
                    progressState = ProgressState.OpenSelectParentMenu;
                }
            }

            if (Input.GetMouseButtonUp(0) && state != CutCopy.None && newObject != null && progressState == ProgressState.CutCopyPaste)
            {
                progressState = ProgressState.Finish;
            }

            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && state == CutCopy.None)
            {
                SetToInitialPosition();
            }
        }

        /// <summary>
        /// Resets to the initial position.
        /// </summary>
        private void SetToInitialPosition()
        {
            Destroyer.Destroy(cutCopyPasteMenu);
            if (selectedObj != null && selectedObj.GetComponent<BlinkEffect>() != null)
            {
                selectedObj.GetComponent<BlinkEffect>().Deactivate();
            }
            selectedObj = null;
            mouseWasReleased = false;
            editToOldBranchLine = false;
            state = CutCopy.None;
            progressState = ProgressState.SelectObject;
        }

        /// <summary>
        /// Deletes the drawable type objects that are cut after pasting.
        /// </summary>
        private void Cut()
        {
            if (state == CutCopy.Cut)
            {
                if (selectedObj.CompareTag(Tags.MindMapNode))
                {
                    MMNodeValueHolder valueHolder = selectedObj.GetComponent<MMNodeValueHolder>();
                    if (valueHolder.GetParent() != null)
                    {
                        valueHolder.GetParent().GetComponent<MMNodeValueHolder>().RemoveChild(selectedObj);
                        new MindMapRemoveChildNetAction(oldDrawable.name, GameFinder.GetDrawableParentName(oldDrawable),
                            MindMapNodeConf.GetNodeConf(selectedObj)).Execute();
                    }

                    if (valueHolder.GetParentBranchLine() != null)
                    {
                        new EraseNetAction(oldDrawable.name, GameFinder.GetDrawableParentName(oldDrawable), valueHolder.GetParentBranchLine().name).Execute();
                        Destroyer.Destroy(valueHolder.GetParentBranchLine());
                    }
                    foreach (KeyValuePair<GameObject, GameObject> pair in valueHolder.GetAllChildren())
                    {
                        new EraseNetAction(oldDrawable.name, GameFinder.GetDrawableParentName(oldDrawable), pair.Value.name).Execute();
                        Destroyer.Destroy(pair.Value);

                        new EraseNetAction(oldDrawable.name, GameFinder.GetDrawableParentName(oldDrawable), pair.Key.name).Execute();
                        Destroyer.Destroy(pair.Key);
                    }
                }
                new EraseNetAction(oldDrawable.name, GameFinder.GetDrawableParentName(oldDrawable), selectedObj.name).Execute();
                Destroyer.Destroy(selectedObj);
            }
        }

        /// <summary>
        /// Cuts or copy the line to the chosen position.
        /// </summary>
        /// <param name="newPosition">The new position for the line.</param>
        private void ProcessLine(Vector3 newPosition)
        {
            LineConf lineConf = LineConf.GetLine(selectedObj);
            lineConf.id = "";
            newObject = GameDrawer.ReDrawLine(newDrawable, lineConf);
            newObject.transform.position = newPosition - newObject.transform.forward * ValueHolder.distanceToDrawable.z * lineConf.orderInLayer;
            new DrawOnNetAction(newDrawable.name, GameFinder.GetDrawableParentName(newDrawable), LineConf.GetLine(newObject)).Execute();
        }

        /// <summary>
        /// Cuts or copy the text to the chosen position.
        /// </summary>
        /// <param name="newPosition">The new position for the text.</param>
        private void ProcessText(Vector3 newPosition)
        {
            TextConf textConf = TextConf.GetText(selectedObj);
            textConf.id = "";
            newObject = GameTexter.ReWriteText(newDrawable, textConf);
            newObject.transform.position = newPosition - newObject.transform.forward * ValueHolder.distanceToDrawable.z * textConf.orderInLayer;
            new WriteTextNetAction(newDrawable.name, GameFinder.GetDrawableParentName(newDrawable), TextConf.GetText(newObject)).Execute();
        }

        /// <summary>
        /// Cuts or copy the image to the chosen position.
        /// </summary>
        /// <param name="newPosition">The new position for the image.</param>
        private void ProcessImage(Vector3 newPosition)
        {
            ImageConf imageConf = ImageConf.GetImageConf(selectedObj);
            imageConf.id = "";
            newObject = GameImage.RePlaceImage(newDrawable, imageConf);
            newObject.transform.position = newPosition - newObject.transform.forward * ValueHolder.distanceToDrawable.z * imageConf.orderInLayer;
            new AddImageNetAction(newDrawable.name, GameFinder.GetDrawableParentName(newDrawable), ImageConf.GetImageConf(newObject)).Execute();
        }

        /// <summary>
        /// Cuts or copy the node and their children to the chosen position.
        /// </summary>
        /// <param name="newPosition">The new position for the node.</param>
        private void ProcessMindMapNode(Vector3 newPosition)
        {
            if (selectedObj.GetComponent<MMNodeValueHolder>().GetParentBranchLine() != null)
            {
                oldBranchLineConf = LineConf.GetLine(selectedObj.GetComponent<MMNodeValueHolder>().GetParentBranchLine());
            }
            newNodesBranchLineHolder.MindMapNodeConfigs[0].branchLineToParent = "";
            newNodesBranchLineHolder.MindMapNodeConfigs[0].parentNode = "";
            GameMindMap.RenameMindMap(newNodesBranchLineHolder, GameFinder.GetAttachedObjectsObject(newDrawable));

            foreach (DrawableType type in newNodesBranchLineHolder.GetAllDrawableTypes())
            {
                DrawableType.Restore(type, newDrawable);
            }

            Vector3 newLocalPosition = GameFinder.GetHighestParent(newDrawable).transform.InverseTransformPoint(newPosition);
            newLocalPosition = new Vector3(newLocalPosition.x, newLocalPosition.y, selectedObj.transform.localPosition.z);
            newObject = GameFinder.FindChild(newDrawable, newNodesBranchLineHolder.MindMapNodeConfigs[0].id);
            GameMoveRotator.SetPosition(newObject, newLocalPosition, true);
            new MoveNetAction(newDrawable.name, GameFinder.GetDrawableParentName(newDrawable), newObject.name, newLocalPosition, true).Execute();
        }

        /// <summary>
        /// Open's the select parent menu and switches the state to select parent.
        /// </summary>
        private void OpenSelectParent()
        {
            GameObject newAttachedObjects = GameFinder.GetAttachedObjectsObject(newDrawable);
            if (newAttachedObjects != null)
            {
                MindMapParentSelectionMenu.EnableForEditing(newAttachedObjects, newObject, MindMapNodeConf.GetNodeConf(newObject), null, true);
                progressState = ProgressState.SelectParent;
            }
        }

        /// <summary>
        /// Waits for user selection. 
        /// If the node cannot be added because the selected node 
        /// is not a theme and the chosen drawable does not have a theme 
        /// that can be used as a parent node, the action is canceled and reset.
        /// </summary>
        private void SelectParent()
        {
            if (MindMapParentSelectionMenu.TryGetParent(out GameObject parent))
            {
                if (newValueHolder is MindMapNodeConf conf)
                {
                    conf.parentNode = parent.name;
                    GameObject branchLineToParent = parent.GetComponent<MMNodeValueHolder>().GetChildren()[newObject];
                    conf.branchLineToParent = branchLineToParent.name;
                    if (oldBranchLineConf != null)
                    {
                        GameEdit.ChangeLine(branchLineToParent, oldBranchLineConf);
                        new EditLineNetAction(newDrawable.name, GameFinder.GetDrawableParentName(newDrawable), LineConf.GetLine(branchLineToParent)).Execute();
                    }
                    newNodesBranchLineHolder = GameMindMap.SummarizeSelectedNodeIncChildren(newObject);
                }
                progressState = ProgressState.Finish;
            } else if (!MindMapParentSelectionMenu.IsActive())
            {
                foreach (DrawableType type in newNodesBranchLineHolder.GetAllDrawableTypes())
                {
                    GameObject typeObject = GameFinder.FindChild(newDrawable, type.id);
                    new EraseNetAction(newDrawable.name, GameFinder.GetDrawableParentName(newDrawable), typeObject.name).Execute();
                    Destroyer.Destroy(typeObject);
                }

                if(state == CutCopy.Cut)
                {
                    foreach(DrawableType type in oldNodesBranchLineHolder.GetAllDrawableTypes())
                    {
                        DrawableType.Restore(type, oldDrawable);
                    }
                    if (oldBranchLineConf != null)
                    {
                        GameObject branchLineToParent = GameFinder.FindChild(oldDrawable, oldValueHolder.id).GetComponent<MMNodeValueHolder>().GetParentBranchLine();
                        GameEdit.ChangeLine(branchLineToParent, oldBranchLineConf);
                        new EditLineNetAction(newDrawable.name, GameFinder.GetDrawableParentName(newDrawable), LineConf.GetLine(branchLineToParent)).Execute();
                    }
                }
                SetToInitialPosition();
            } else
            {
                if (oldBranchLineConf != null && !editToOldBranchLine)
                {
                    GameObject branchLineToParent = newObject.GetComponent<MMNodeValueHolder>().GetParentBranchLine();
                    GameEdit.ChangeLine(branchLineToParent, oldBranchLineConf);
                    new EditLineNetAction(newDrawable.name, GameFinder.GetDrawableParentName(newDrawable), LineConf.GetLine(branchLineToParent)).Execute();
                    editToOldBranchLine = true;
                }
            }
            
        }

        /// <summary>
        /// Reverts this action, i.e., restores the old object and destroyes the new one.
        /// </summary>
        public override void Undo()
        {
            base.Undo();
            if (memento.state == CutCopy.Cut)
            {
                GameObject oldDrawable = memento.oldDrawable.GetDrawable();
                
                if (memento.oldValueHolder is MindMapNodeConf)
                {
                    foreach(DrawableType type in memento.oldNodesHolder.GetAllDrawableTypes())
                    {
                        DrawableType.Restore(type, oldDrawable);
                    }
                    if (memento.oldBranchLineConfig != null)
                    {
                        GameObject oldObject = GameFinder.FindChild(oldDrawable, memento.oldValueHolder.id);
                        if (oldObject.GetComponent<MMNodeValueHolder>().GetParentBranchLine() != null)
                        {
                            GameObject branchLineToParent = oldObject.GetComponent<MMNodeValueHolder>().GetParentBranchLine();
                            GameEdit.ChangeLine(branchLineToParent, memento.oldBranchLineConfig);
                            new EditLineNetAction(oldDrawable.name, GameFinder.GetDrawableParentName(oldDrawable), LineConf.GetLine(branchLineToParent)).Execute();
                        }
                    }
                } else
                {
                    DrawableType.Restore(memento.oldValueHolder, oldDrawable);
                }
            }

            GameObject newObject = GameFinder.FindChild(memento.newDrawable.GetDrawable(), memento.newValueHolder.id);
            if (newObject.CompareTag(Tags.MindMapNode))
            {
                MMNodeValueHolder valueHolder = newObject.GetComponent<MMNodeValueHolder>();
                if (valueHolder.GetParentBranchLine() != null)
                {
                    new EraseNetAction(memento.newDrawable.ID, memento.newDrawable.ParentID, valueHolder.GetParentBranchLine().name).Execute();
                    Destroyer.Destroy(valueHolder.GetParentBranchLine());

                    valueHolder.GetParent().GetComponent<MMNodeValueHolder>().RemoveChild(newObject);
                    new MindMapRemoveChildNetAction(memento.newDrawable.ID, memento.newDrawable.ParentID, MindMapNodeConf.GetNodeConf(newObject)).Execute();
                }
                foreach (DrawableType type in memento.newNodesHolder.GetAllDrawableTypes())
                {
                    new EraseNetAction(memento.newDrawable.ID, memento.newDrawable.ParentID, type.id).Execute();
                    Destroyer.Destroy(GameFinder.FindChild(memento.newDrawable.GetDrawable(), type.id));
                }
            }
            new EraseNetAction(memento.newDrawable.ID, memento.newDrawable.ParentID, newObject.name).Execute();
            Destroyer.Destroy(newObject);
        }

        /// <summary>
        /// Repeats this action, i.e., create again the new object and deletes the old one if it was cutted.
        /// </summary>
        public override void Redo()
        {
            base.Redo();
            GameObject newDrawable = memento.newDrawable.GetDrawable();
            
            if (memento.newValueHolder is MindMapNodeConf)
            {
                foreach (DrawableType type in memento.newNodesHolder.GetAllDrawableTypes())
                {
                    DrawableType.Restore(type, newDrawable);
                }
                DrawableType.Restore(memento.newValueHolder, newDrawable);
                if (memento.oldBranchLineConfig != null)
                {
                    GameObject newObject = GameFinder.FindChild(newDrawable, memento.newValueHolder.id);
                    if (newObject.GetComponent<MMNodeValueHolder>().GetParentBranchLine() != null)
                    {
                        GameObject branchLineToParent = newObject.GetComponent<MMNodeValueHolder>().GetParentBranchLine();
                        GameEdit.ChangeLine(branchLineToParent, memento.oldBranchLineConfig);
                        new EditLineNetAction(newDrawable.name, GameFinder.GetDrawableParentName(newDrawable), LineConf.GetLine(branchLineToParent)).Execute();
                    }
                }
            } else
            {
                DrawableType.Restore(memento.newValueHolder, newDrawable);
            }

            if (memento.state == CutCopy.Cut)
            {
                GameObject oldObject = GameFinder.FindChild(memento.oldDrawable.GetDrawable(), memento.oldValueHolder.id);
                if (oldObject.CompareTag(Tags.MindMapNode))
                {
                    MMNodeValueHolder valueHolder = oldObject.GetComponent<MMNodeValueHolder>();
                    if (valueHolder.GetParentBranchLine() != null)
                    {
                        new EraseNetAction(memento.oldDrawable.ID, memento.oldDrawable.ParentID, valueHolder.GetParentBranchLine().name).Execute();
                        Destroyer.Destroy(valueHolder.GetParentBranchLine());

                        valueHolder.GetParent().GetComponent<MMNodeValueHolder>().RemoveChild(oldObject);
                        new MindMapRemoveChildNetAction(memento.oldDrawable.ID, memento.oldDrawable.ParentID, MindMapNodeConf.GetNodeConf(oldObject)).Execute();
                    }
                    foreach (DrawableType type in memento.oldNodesHolder.GetAllDrawableTypes())
                    {
                        new EraseNetAction(memento.oldDrawable.ID, memento.oldDrawable.ParentID, type.id).Execute();
                        Destroyer.Destroy(GameFinder.FindChild(memento.oldDrawable.GetDrawable(), type.id));
                    }
                }
                new EraseNetAction(memento.oldDrawable.ID, memento.oldDrawable.ParentID, oldObject.name).Execute();
                Destroyer.Destroy(oldObject);

            }
        }


        /// <summary>
        /// A new instance of <see cref="CutCopyPasteAction"/>.
        /// See <see cref="ReversibleAction.CreateReversibleAction"/>.
        /// </summary>
        /// <returns>new instance of <see cref="CutCopyPasteAction"/></returns>
        public static ReversibleAction CreateReversibleAction()
        {
            return new CutCopyPasteAction();
        }

        /// <summary>
        /// A new instance of <see cref="CutCopyPasteAction"/>.
        /// See <see cref="ReversibleAction.NewInstance"/>.
        /// </summary>
        /// <returns>new instance of <see cref="CutCopyPasteAction"/></returns>
        public override ReversibleAction NewInstance()
        {
            return CreateReversibleAction();
        }

        /// <summary>
        /// Returns the <see cref="ActionStateType"/> of this action.
        /// </summary>
        /// <returns><see cref="ActionStateType.CutCopyPaste"/></returns>
        public override ActionStateType GetActionStateType()
        {
            return ActionStateTypes.CutCopyPaste;
        }

        /// <summary>
        /// The set of IDs of all gameObjects changed by this action.
        /// <see cref="ReversibleAction.GetActionStateType"/>
        /// Because this action does not actually change any game object, 
        /// an empty set is always returned.
        /// </summary>
        /// <returns>The object id of the changed object.</returns>
        public override HashSet<string> GetChangedObjects()
        {
            return new HashSet<string>
                {
                    memento.oldValueHolder.id,
                    memento.newValueHolder.id
                };
        }
    }
}