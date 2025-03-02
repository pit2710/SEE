﻿using System;
using System.Collections.Generic;
using SEE.Utils.History;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// An abstract superclass of all PlayerActions such as NewNodeAction, ScaleNodeAction, EditNodeAction and AddEdgeAction.
    /// The most important attribute for all of them is the hoveredObject, which will be instantiated and updated by LocalAnyHoverIn and LocalAnyHoverOut.
    /// </summary>
    public abstract class AbstractPlayerAction : IReversibleAction
    {
        /// <summary>
        /// The unique ID of an action.
        /// </summary>
        private readonly Guid id = Guid.NewGuid();

        /// <summary>
        /// The object that the cursor hovers over.
        /// </summary>
        protected GameObject HoveredObject = null;

        /// <summary>
        /// The current state of the action as specified by <see cref="IReversibleAction.Progress"/>.
        /// </summary>
        protected IReversibleAction.Progress CurrentState = IReversibleAction.Progress.NoEffect;

        /// <summary>
        /// The undo operation which has to be implemented specifically by subclasses
        /// to revert the effect of an executed action.
        /// See <see cref="IReversibleAction.Undo"/>.
        /// </summary>
        public virtual void Undo()
        {
            Assert.IsTrue(CurrentState == IReversibleAction.Progress.InProgress
                || CurrentState == IReversibleAction.Progress.Completed);
            // intentionally left blank; can be overridden by subclasses
        }

        /// <summary>
        /// The redo operation which has to be implemented specifically by subclasses
        /// to revert the effect of an undone action, in other words, to return to
        /// the state at the point in time when <see cref="Undo"/> was called.
        /// See <see cref="IReversibleAction.Redo"/>.
        /// </summary>
        public virtual void Redo()
        {
            Assert.IsTrue(CurrentState == IReversibleAction.Progress.Completed);
            // intentionally left blank; can be overridden by subclasses
        }

        /// <summary>
        /// Returns the <see cref="ActionStateType"/> of the specific action.
        /// </summary>
        /// <returns>the <see cref="ActionStateType"/> of the specific action</returns>
        public abstract ActionStateType GetActionStateType();

        /// <summary>
        /// Will be called once when the action is started for the
        /// first time. Intended for initialization purposes.
        /// See <see cref="IReversibleAction.Awake"/>.
        /// </summary>
        public virtual void Awake()
        {
            // intentionally left blank; can be overridden by subclasses
        }

        /// <summary>
        /// Will be called after <see cref="Awake"/> and then again whenever the
        /// action is re-enabled (<see cref="Stop"/> was called before then).
        /// See <see cref="IReversibleAction.Start"/>.
        /// </summary>
        public virtual void Start()
        {
            // intentionally left blank; can be overridden by subclasses
        }

        /// <summary>
        /// Will be called upon every frame when this action is being executed.
        /// See <see cref="IReversibleAction.Update"/>.
        /// </summary>
        /// <returns>true if action is completed</returns>
        public abstract bool Update();

        /// <summary>
        /// Returns a new instance of the same type as this particular type of ReversibleAction.
        /// </summary>
        /// <returns>new instance</returns>
        public abstract IReversibleAction NewInstance();

        /// <summary>
        /// Will be called when another action is to be executed. This signals that
        /// the action is to be put on hold. No <see cref="Update"/> will occur
        /// while on hold. It may be re-enabled by <see cref="Start"/> again.
        /// </summary>
        public virtual void Stop()
        {
            // intentionally left blank; can be overridden by subclasses
        }

        /// <summary>
        /// Sets <see cref="HoveredObject"/> to given <paramref name="interactableObject"/>.
        /// Will be called while any <see cref="InteractableObject"/> is being hovered over.
        /// </summary>
        /// <param name="interactableObject">new value for <see cref="HoveredObject"/></param>
        protected void LocalAnyHoverIn(InteractableObject interactableObject)
        {
            HoveredObject = interactableObject.gameObject;
        }

        /// <summary>
        /// Sets <see cref="HoveredObject"/> to <code>null</code>.
        /// Will be called whenever any <see cref="InteractableObject"/> is no longer being hovered over.
        /// </summary>
        /// <param name="interactableObject">object no longer be hovered over (ignored here)</param>
        protected void LocalAnyHoverOut(InteractableObject interactableObject)
        {
            HoveredObject = null;
        }

        /// <summary>
        /// Returns the current state of the action indicating whether it has had an effect
        /// that may need to be undone and whether it is still ongoing.
        /// Implements <see cref="IReversibleAction.CurrentProgress"/>.
        /// </summary>
        /// <returns>the current state of the action</returns>
        public IReversibleAction.Progress CurrentProgress() => CurrentState;

        /// <summary>
        /// Returns the IDs of all gameObjects manipulated by the specific action.
        /// </summary>
        /// <returns>All IDs of manipulated gameObjects</returns>
        public abstract HashSet<string> GetChangedObjects();

        /// <summary>
        /// A getter for the ID of this action.
        /// </summary>
        /// <returns>The ID of this action as a string</returns>
        public string GetId() => id.ToString();
    }
}
