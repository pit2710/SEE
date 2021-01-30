﻿using UnityEngine;
using SEE.Game;
using SEE.GO;
using System;
using UnityEngine.Assertions;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Action to create an edge between two selected nodes.
    /// </summary>
    public class AddEdgeAction : MonoBehaviour
    {
        /// <summary>
        /// Start() will register an anonymous delegate of type 
        /// <see cref="ActionState.OnStateChangedFn"/> on the event
        /// <see cref="ActionState.OnStateChanged"/> to be called upon every
        /// change of the action state, where the newly entered state will
        /// be passed as a parameter. The anonymous delegate will compare whether
        /// this state equals <see cref="ThisActionState"/> and if so, execute
        /// what needs to be done for this action here. If that parameter is
        /// different from <see cref="ThisActionState"/>, this action will
        /// put itself to sleep. 
        /// Thus, this action will be executed only if the new state is 
        /// <see cref="ThisActionState"/>.
        /// </summary>
        const ActionState.Type ThisActionState = ActionState.Type.DrawEdge;

        /// <summary>
        /// The currently hovered object.
        /// </summary>
        private GameObject hoveredObject;

        /// <summary>
        /// The source for the edge to be drawn.
        /// </summary>
        private GameObject from;

        /// <summary>
        /// The target of the edge to be drawn.
        /// </summary>
        private GameObject to;

        private void Start()
        {
            // An anonymous delegate is registered for the event <see cref="ActionState.OnStateChanged"/>.
            // This delegate will be called from <see cref="ActionState"/> upon every
            // state changed where the passed parameter is the newly entered state.
            ActionState.OnStateChanged += (ActionState.Type newState) =>
            {
                // Is this our action state where we need to do something?
                if (newState == ThisActionState)
                {
                    // The monobehaviour is enabled and Update() will be called by Unity.
                    enabled = true;
                    InteractableObject.LocalAnyHoverIn += LocalAnyHoverIn;
                    InteractableObject.LocalAnyHoverOut += LocalAnyHoverOut;
                }
                else
                {
                    // The monobehaviour is diabled and Update() no longer be called by Unity.
                    enabled = false;
                    InteractableObject.LocalAnyHoverIn -= LocalAnyHoverIn;
                    InteractableObject.LocalAnyHoverOut -= LocalAnyHoverOut;
                }
            };
            enabled = ActionState.Is(ThisActionState);
        }

        private void Update()
        {
            Assert.IsTrue(ActionState.Is(ThisActionState));

            // Assigning the game objects to be connected.
            // Checking whether the two game objects are not null and whether they are 
            // actually nodes.
            if (Input.GetMouseButtonDown(0) && hoveredObject != null)
            {
                Assert.IsTrue(hoveredObject.HasNodeRef());
                if (from == null)
                {
                    from = hoveredObject;
                }
                else if (to == null)
                {
                    to = hoveredObject;
                }
            }
            // Note: from == to may be possible.
            if (from != null && to != null)
            {
                Transform cityObject = SceneQueries.GetCodeCity(from.transform);
                if (cityObject != null)
                {
                    if (cityObject.TryGetComponent(out SEECity city))
                    {
                        try
                        {
                            city.Renderer.DrawEdge(from, to);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"The new edge from {from.name} to {to.name} could not be created: {e.Message}.\n");
                        }
                        from = null;
                        to = null;
                    }
                }
            }
            // Adding the key "F1" in order to delete the selected gameobjects.
            if (Input.GetKeyDown(KeyCode.F1))
            {
                from = null;
                to = null;
            }
        }

        private void LocalAnyHoverIn(InteractableObject interactableObject)
        {
            try
            {
                Assert.IsNull(hoveredObject);
                hoveredObject = interactableObject.gameObject;
            }
            catch
            {
                //There are AssertionExceptions 
            }
        }

        private void LocalAnyHoverOut(InteractableObject interactableObject)
        {
            try
            {
                Assert.IsTrue(hoveredObject == interactableObject.gameObject);
                hoveredObject = null;
            }
            catch
            {
                //There are AssertionExceptions
            }
        }
    }
}