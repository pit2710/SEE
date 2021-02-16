﻿using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// An abstract superclass of all PlayerActions such as NewNodeAction, ScaleNodeAction, EditNodeAction and AddEdgeAction.
    /// The most important attribute for all of them is the hoveredObject, which will be instantiated and updated by LocalAnyHoverIn and LocalAnyHoverOut.
    /// </summary>
    public abstract class AbstractPlayerAction : MonoBehaviour
    {
        /// <summary>
        /// The gameObject that contains the CanvasGenerator and the actual CanvasObject.
        /// </summary>
        protected GameObject canvasObject;

        /// <summary>
        /// The object that the cursor hovers over.
        /// </summary>
        protected GameObject hoveredObject = null;

        /// <summary>
        /// The current name of the gameObject that contains the canvas operations and components.
        /// </summary>
        protected readonly string nameOfCanvasObject = "CanvasObject";

        /// <summary>
        /// True if the active script is already initialized, else false.
        /// </summary>
        protected bool instantiated = false;

        /// <summary>
        /// Finds the GameObject that contains the CanvasOperations and components
        /// and saves it in the <see cref="canvasObject"/>.
        /// </summary>
        /// <returns>true if the <see cref="canvasObject"/> could be found</returns>
        protected bool InitializeCanvasObject()
        {
            canvasObject = GameObject.Find(nameOfCanvasObject);
            return canvasObject != null;
        }

        /// <summary>
        /// Sets <see cref="hoveredObject"/> to given <paramref name="interactableObject"/>.
        /// Will be called while any <see cref="InteractableObject"/> is being hovered over.
        /// </summary>
        /// <param name="interactableObject">new value for <see cref="hoveredObject"/></param>
        protected void LocalAnyHoverIn(InteractableObject interactableObject)
        {
            Debug.LogFormat("{0}.LocalAnyHoverIn({1})\n",
                            this.GetType().FullName, 
                            interactableObject == null ? "NULL" : interactableObject.name);
            try
            {
                if (interactableObject.gameObject != hoveredObject)
                {
                    Assert.IsNull(hoveredObject);
                    hoveredObject = interactableObject.gameObject;
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("{0}.LocalAnyHoverIn throws {1}. [hoveredObject: {2}. interactableObject: {3}.\n",
                                      this.GetType().FullName,
                                      e.Message, hoveredObject == null ? "NULL" : hoveredObject.name, interactableObject.name);
                // FIXME: There are AssertionExceptions 
            }
        }

        /// <summary>
        /// Sets <see cref="hoveredObject"/> to <code>null</code>.
        /// Will be called whenever any <see cref="InteractableObject"/> is no longer being hovered over.
        /// </summary>
        /// <param name="interactableObject">object no longer be hovered over (ignored here)</param>
        protected void LocalAnyHoverOut(InteractableObject interactableObject)
        {
            Debug.LogFormat("{0}.LocalAnyHoverOut({1})\n",
                            this.GetType().FullName,
                            interactableObject == null ? "NULL" : interactableObject.name);
            try
            {
                Assert.IsTrue(hoveredObject == interactableObject.gameObject);
                hoveredObject = null;
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("{0}.LocalAnyHoverOut throws {1}. [hoveredObject: {2}. interactableObject: {3}.\n",
                                     this.GetType().FullName,
                                     e.Message, hoveredObject == null ? "NULL" : hoveredObject.name, interactableObject.name);
                // FIXME: There are AssertionExceptions 
            }
        }
    }
}
