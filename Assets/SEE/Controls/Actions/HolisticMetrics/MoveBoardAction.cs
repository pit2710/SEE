using SEE.Game.HolisticMetrics;
using SEE.Net.Actions.HolisticMetrics;
using UnityEngine;

namespace SEE.Controls.Actions.HolisticMetrics
{
    /// <summary>
    /// Each instance of this class manages one concrete move action of a metrics board from an old position and
    /// rotation to a new position and rotation. This should be used when moving a board.
    /// </summary>
    internal class MoveBoardAction
    {
        /// <summary>
        /// The name of the board that will be moved.
        /// </summary>
        private readonly string boardName;
        
        /// <summary>
        /// The old position of the board so we can revert this action.
        /// </summary>
        private readonly Vector3 oldPosition;

        /// <summary>
        /// The position to which the board is to be moved by this action.
        /// </summary>
        private readonly Vector3 newPosition;

        /// <summary>
        /// The old rotation that the board had, so we can revert this action.
        /// </summary>
        private readonly Quaternion oldRotation;

        /// <summary>
        /// The new rotation to which the board will be set.
        /// </summary>
        private readonly Quaternion newRotation;

        /// <summary>
        /// Initializes the fields of this instance.
        /// </summary>
        /// <param name="boardName">The name of the board; will be used to identify the board.</param>
        /// <param name="oldPosition">The position that the board is/was at before the moving.</param>
        /// <param name="newPosition">The position to move the board to.</param>
        /// <param name="oldRotation">The rotation the board is/was at before the moving.</param>
        /// <param name="newRotation">The rotation to rotate the board to.</param>
        internal MoveBoardAction(
            string boardName,
            Vector3 oldPosition,
            Vector3 newPosition,
            Quaternion oldRotation,
            Quaternion newRotation)
        {
            this.boardName = boardName;
            this.oldPosition = oldPosition;
            this.newPosition = newPosition;
            this.oldRotation = oldRotation;
            this.newRotation = newRotation;
        }

        /// <summary>
        /// Here we execute the moving action by moving the board to the new position on this client and all other
        /// clients. This method should only be called from the history and is also called from the base class of this
        /// class.
        /// </summary>
        internal void Do()
        {
            BoardsManager.Move(boardName, newPosition, newRotation);
            new MoveBoardNetAction(boardName, newPosition, newRotation).Execute();
        }

        /// <summary>
        /// Here we revert the moving action by moving the board to the old position on this client and all other
        /// clients. This method should only be called from the history.
        /// </summary>
        internal void Undo()
        {
            BoardsManager.Move(boardName, oldPosition, oldRotation);
            new MoveBoardNetAction(boardName, oldPosition, oldRotation).Execute();
        }
    }
}