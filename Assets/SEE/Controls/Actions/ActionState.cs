﻿namespace SEE.Controls.Actions
{
    /// <summary>
    /// Provides user action that depend upon a particular state the user can be in. 
    /// A user state determines what kinds of actions are triggered for a given
    /// interaction.
    /// </summary>
    public static class ActionState
    {
        /// <summary>
        /// The type of a state-based action.
        /// </summary>
        public enum Type
        {
            Move = 0,      // the user wants to move a node
            Rotate = 1,    // the user wants to rotate a node
            Map = 2,       // the user wants to map an implementation node onto an architecture node (reflexion analysis)
            NewEdge = 3,   // the user wants to draw an edge between nodes
            NewNode = 4,   // the user wants to add a new node
            EditNode = 5,  // the user wants to edit an existing node
            ScaleNode = 6, // the user wants to scale an existing node
            Delete = 7,    // the user wants to delete a node or edge
            Hide = 8,      // the user wants to hide or show a node or edge
        }

        private static Type value = 0;
        /// <summary>
        /// The type of the state-based action. Upon changing this type,
        /// the event <see cref="OnStateChangedFn"/> will be triggered with
        /// the currently set action type.
        /// </summary>
        public static Type Value
        {
            get => value;
            set
            {
                if (ActionState.value != value)
                {
                    ActionState.value = value;
                    OnStateChanged?.Invoke(ActionState.value);
                }
            }
        }

        /// <summary>
        /// Whether the given type of the state-based action is currently active.
        /// </summary>
        /// <param name="value">The type to check</param>
        /// <returns><code>true</code> if the given type if currently active,
        /// <code>false</code> otherwise.</returns>
        public static bool Is(Type value)
        {
            return ActionState.value == value;
        }

        /// <summary>
        /// A delegate to be called upon a change of the action state. 
        /// </summary>
        /// <param name="value">the new action state</param>
        public delegate void OnStateChangedFn(Type value);
        /// <summary>
        /// Event that is triggered when the action is assigned a new action state to.
        /// </summary>
        public static event OnStateChangedFn OnStateChanged;
    }
}
