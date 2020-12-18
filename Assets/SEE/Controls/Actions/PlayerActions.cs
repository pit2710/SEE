﻿using SEE.Game;
using SEE.Game.Charts;
using SEE.GO;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Actions of a player. To be attached to a game object representing a 
    /// player (desktop, VR, etc.).
    /// </summary>
    public class PlayerActions : MonoBehaviour
    {
        /// <summary>
        /// The possible states a player can be in. The state determines how the player
        /// reacts to events.
        /// </summary>
        private enum State
        {
            Browse,   // the user just browses the city; this is the default
            MoveNode, // a game node is being moved within its city
            MapNode,   // a game node is mapped from one city to another city
            NewNode, //a  game node is being created
            ScaleNode //a game node is scaled with mouse drag
        }


        /// <summary>
        /// If the State changes the PreState remembers the state before 
        /// </summary>
        private enum PreState
        {
            None,
            MoveNode,
            MapNode,
            NewNode,
            ScaleNode
        }
        GameObject nodeAction = null;


        /// <summary>
        /// The current state of the player.
        /// </summary>
        private State state = State.Browse;

        /// <summary>
        /// The last state of the Player
        /// </summary>
        private PreState preState = PreState.None;

        private void Update()
        {
            // If the local player presses U, we deselect all currently selected interactable objects.
            if (Input.GetKeyDown(KeyCode.U))
            {
                InteractableObject.UnselectAll(true);
                ChartManager.Instance.UnselectAll();
            }

            switch (state)
            {
                case State.MoveNode:
                    // an object must be selected; otherwise we cannot move it
                    if (selectedObject != null)
                    {
                        if (UserWantsToMove())
                        {
                            GameNodeMover.MoveTo(selectedObject);
                        }
                        else
                        {
                            // The selected object has reached its final destination.
                            // It needs to be placed there.
                            GameNodeMover.FinalizePosition(selectedObject);
                            selectedObject = null;
                        }
                    }
                    break;
                case State.NewNode:

                    //Ads the newComponent if not already done
                    if (!gameObject.GetComponent<DesktopNewNodeAction>())
                    {
                        gameObject.AddComponent<DesktopNewNodeAction>();
                    }
                    else //Sets the Hovered Object
                    { 
                        gameObject.GetComponent<DesktopNewNodeAction>().hoveredObject = hoveredObject;
                    }
                    break;
                case State.ScaleNode:
                    if (selectedObject != null && nodeAction == null )
                    {
                        nodeAction = selectedObject;
                        nodeAction.AddComponent<GameNodeScaleAction>();
                        preState = PreState.ScaleNode;
                    }
                    break;

                default:
                    switch (preState)
                    {
                        case PreState.ScaleNode:
                            nodeAction.GetComponent<GameNodeScaleAction>().removeScript();
                            nodeAction = null;
                            break;
                    }
                    break;
            }
        }

        // -------------------------------------------------------------
        // The callbacks from the circular menu to trigger state changes
        // -------------------------------------------------------------

        /// <summary>
        /// Changes the state to Browse. 
        /// 
        /// This method is called as a callback from the menu.
        /// </summary>
        public void Browse()
        {
            Enter(State.Browse);
        }

        /// <summary>
        /// Changes the state to MoveNode. 
        /// 
        /// This method is called as a callback from the menu.
        /// </summary>
        public void Move()
        {
            Enter(State.MoveNode);
        }

        /// <summary>
        /// Changes the state to MapNode. 
        /// 
        /// This method is called as a callback from the menu.
        /// </summary>
        public void Map()
        {
            Enter(State.MapNode);
        }

        /// <summary>
        /// Changes the state to NewNode. 
        /// 
        /// This method is called as a callback from the menu.
        /// </summary>
        public void NewNode()
        {
            Enter(State.NewNode);
        }

        /// <summary>
        /// Changes th state to ScaleNode
        /// 
        /// This method is called as a callback from the menu.
        /// </summary>
        public void ScaleNode()
        {
            Enter(State.ScaleNode);
        }
        /// <summary>
        /// If <paramref name="newState"/> is different from the current state,
        /// <see cref="Cancel"/> is called and <paramref name="newState"/> is
        /// entered.
        /// </summary>
        /// <param name="newState">new state to be entered</param>
        private void Enter(State newState)
        {
            if (state != newState)
            {
                Cancel();
                state = newState;
            }
        }

        /// <summary>
        /// Cancels the current action before the next new state is entered.
        /// This method can implement the "last wishes" of a running action.
        /// </summary>
        private void Cancel()
        {
            switch (state)
            {
                case State.Browse:
                    // nothing to be done
                    break;
                case State.MapNode:
                    break;
                case State.MoveNode:
                    break;
                case State.NewNode:
                 
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        }

        // ------------------------------------------------------------
        // The management of the currently selected interactable object
        // ------------------------------------------------------------

        /// <summary>
        /// The currently selected object. May be null if none is selected.
        /// Do not use this attribute directly. Use <see cref="SelectedObject"/>
        /// instead.
        /// </summary>
        private GameObject selectedObject;

        // ------------------------------------------------------------------------------
        // Events triggered by interactable objects when they are selected, hovered over,
        // or grabbed.
        // ------------------------------------------------------------------------------

        /// <summary>
        /// Assigns the value of given <paramref name="selection"/> to
        /// <see cref="SelectedObject"/>.
        /// 
        /// Called by an interactable object when it is selected (only once when the
        /// selection starts).
        /// </summary>
        /// <param name="selection">the selected interactable object</param>
        public void SelectOn(GameObject selection)
        {
            selectedObject = selection;
        }

        /// <summary>
        /// Resets <see cref="SelectedObject"/> to null.
        /// 
        /// Called by an interactable object when it is unselected (only once when the
        /// selection ends).
        /// </summary>
        /// <param name="selection">the interactable object no longer selected</param>
        public void SelectOff(GameObject selection)
        {
            selectedObject = null;
        }

        /// <summary>
        /// The interactable object that is currently being hovered over.
        /// </summary>
        private GameObject hoveredObject;

        /// <summary>
        /// Assigns the value of given <paramref name="hovered"/> to
        /// <see cref="hoveredObject"/>.
        /// 
        /// Called by an interactable object when it is being hovered over
        /// (only once when the hovering starts).
        /// </summary>
        /// <param name="hovered">the hovered interactable object</param>
        public void HoverOn(GameObject hovered)
        {
            hoveredObject = hovered;
        }

        /// <summary>
        /// Resets <see cref="hoveredObject"/> to null.
        /// 
        /// Called by an interactable object when it is no longer being hovered over
        /// (only once when the hovering ends).
        /// </summary>
        /// <param name="hovered">the interactable object no longer hovered</param>
        public void HoverOff(GameObject hovered)
        {
            hoveredObject = null;
        }

        /// <summary>
        /// Called by an interactable object when it is being grabbed
        /// (only once when the grabbing begins).
        /// </summary>
        /// <param name="grabbed">the grabbed interactable object</param>
        public void GrabOn(GameObject grabbed)
        {
            // currently empty
        }

        /// <summary>
        /// Called by an interactable object when it is no longer being grabbed
        /// (only once when the grabbing ends).
        /// </summary>
        /// <param name="grabbed">the interactable object no longer grabbed</param>
        public void GrabOff(GameObject grabbed)
        {
            // currently empty
        }

        // -------------------------------------------------------------
        // User input
        // -------------------------------------------------------------

        /// <summary>
        /// True iff the user expresses that the moving action should start or continue.
        /// The expression depends upon the environment (desktop, VR, etc.).
        /// </summary>
        /// <returns>user wants to move a selected object</returns>
        private static bool UserWantsToMove()
        {
            // FIXME: We need to an interaction for VR, too.
            // We move the node while the left mouse button is pressed.
            return Input.GetMouseButton(0);
        }
    }
}