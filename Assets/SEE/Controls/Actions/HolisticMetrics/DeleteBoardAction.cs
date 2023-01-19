using System.Collections.Generic;
using SEE.Game.HolisticMetrics;
using SEE.Game.HolisticMetrics.Components;
using SEE.Net.Actions.HolisticMetrics;
using SEE.Utils;
using UnityEngine;

namespace SEE.Controls.Actions.HolisticMetrics
{
    /// <summary>
    /// This class manages the delete action deleting one metrics board. When deleting a board, you should use this
    /// class.
    /// </summary>
    internal class DeleteBoardAction : AbstractPlayerAction
    {
        private Memento memento;
        
        private struct Memento
        {
            /// <summary>
            /// The entire configuration of the board for creating it again when the player wants to undo the action.
            /// </summary>
            public readonly BoardConfig boardConfig;

            public Memento(BoardConfig boardConfig)
            {
                this.boardConfig = boardConfig;
            }
        }

        public override bool Update()
        {
            if (Input.GetMouseButtonDown(0) && Raycasting.RaycastAnything(out RaycastHit raycastHit))
            {
                WidgetsManager widgetsManager = raycastHit.transform.GetComponent<WidgetsManager>();
                if (widgetsManager == null)
                {
                    return false;
                }

                foreach (string name in BoardsManager.GetNames())
                {
                    if (widgetsManager.name.Equals(name)) // TODO: Might need to attach collider to board prefab
                    {
                        memento = new Memento(ConfigManager.GetBoardConfig(widgetsManager));
                        Redo();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Creates the deleted board again from the saved board config.
        /// </summary>
        public override void Undo()
        {
            BoardsManager.Create(memento.boardConfig);
            new CreateBoardNetAction(memento.boardConfig).Execute();
        }
        
        /// <summary>
        /// Deletes the board (again).
        /// </summary>
        public override void Redo()
        {
            BoardsManager.Delete(memento.boardConfig.Title);
            new DeleteBoardNetAction(memento.boardConfig.Title).Execute();
        }
        
        /// <summary>
        /// Returns a new instance of <see cref="DeleteBoardAction"/>.
        /// </summary>
        /// <returns>new instance</returns>
        public static ReversibleAction CreateReversibleAction()
        {
            return new DeleteBoardAction();
        }
        
        public override ReversibleAction NewInstance()
        {
            return CreateReversibleAction();
        }
        
        /// <summary>
        /// Returns the ID (name) of the metrics board that has been deleted by this action.
        /// </summary>
        /// <returns></returns>
        public override HashSet<string> GetChangedObjects()
        {
            return new HashSet<string> { memento.boardConfig.Title };
        }

        public override ActionStateType GetActionStateType()
        {
            return ActionStateType.DeleteBoard;
        }
    }
}