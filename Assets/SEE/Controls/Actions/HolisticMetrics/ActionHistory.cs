using UnityEngine;

namespace SEE.Controls.Actions.HolisticMetrics
{
    /// <summary>
    /// This class records a history of holistic metric actions so we can revert them.
    /// </summary>
    internal static class ActionHistory
    {
        /// <summary>
        /// Points to the action last executed.
        /// </summary>
        private static Action lastAction;

        /// <summary>
        /// Adds a new action to the history after the action that the history currently points to. If any actions were
        /// reverted before, those will be forgotten.
        /// </summary>
        /// <param name="action">The action to add to the history</param>
        internal static void Add(Action action)
        {
            if (lastAction != null)
            {
                lastAction.Child = action;
                action.Parent = lastAction;
            }

            lastAction = action;
        }

        /// <summary>
        /// Undoes the last action.
        /// </summary>
        internal static void Undo()
        {
            if (lastAction != null)
            {
                lastAction.Undo();
                lastAction = lastAction.Parent;
            }
        }

        /// <summary>
        /// Redoes the action last undone.
        /// </summary>
        internal static void Redo()
        {
            if (lastAction != null && lastAction.Child != null)
            {
                lastAction.Child.Do();
                lastAction = lastAction.Child;
            }
        }
    }
}