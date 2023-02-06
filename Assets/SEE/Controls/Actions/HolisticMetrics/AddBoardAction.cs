using System.Collections.Generic;
using SEE.Game.HolisticMetrics;
using SEE.Game.HolisticMetrics.ActionHelpers;
using SEE.Game.UI.HolisticMetrics;
using SEE.Game.UI.PropertyDialog.HolisticMetrics;
using SEE.Net.Actions.HolisticMetrics;
using SEE.Utils;
using UnityEngine;

namespace SEE.Controls.Actions.HolisticMetrics
{
    /// <summary>
    /// This action manages the creation of a specific metrics board.
    /// </summary>
    internal class AddBoardAction : AbstractPlayerAction
    {
        /// <summary>
        /// Path to the prefab that lets the player rotate the dummy metrics board he sees at one stage of adding a new
        /// board to the scene. This is a little window with a slider (the slider controls the rotation) and a button to
        /// confirm the rotation.
        /// </summary>
        private const string boardRotatorPath = "Prefabs/UI/MetricsBoardRotation";

        /// <summary>
        /// This field can hold a reference to the dialog that the player will see in the process of executing this
        /// action.
        /// </summary>
        private AddBoardDialog addBoardDialog;
        
        /// <summary>
        /// Indicates how far this instance has progressed in adding a new metrics board to the scene.
        /// </summary>
        private ProgressState progress = ProgressState.GettingPosition;
        
        /// <summary>
        /// Saves all the information needed to revert or repeat this action.
        /// </summary>
        private Memento memento;
        
        /// <summary>
        /// Represents the different stages of progress of this action.
        /// </summary>
        private enum ProgressState
        {
            GettingPosition,
            GettingRotation,
            GettingName
        }
        
        /// <summary>
        /// This struct can store all the information needed to revert or repeat an <see cref="AddBoardAction"/>.
        /// </summary>
        private struct Memento
        {
            /// <summary>
            /// The configuration of the board to create/that has been created.
            /// </summary>
            internal readonly BoardConfig boardConfig;

            /// <summary>
            /// Creates this action. That does not execute it, it only prepares it.
            /// </summary>
            /// <param name="boardConfig">The configuration of the board to create.</param>
            internal Memento(BoardConfig boardConfig)
            {
                this.boardConfig = boardConfig;
            }    
        }
        
        /// <summary>
        /// Sets up the scene to listen for a mouse click on the floor.
        /// </summary>
        public override void Start()
        {
            BoardAdder.Init();
        }

        /// <summary>
        /// This method manages the player's interaction with the mode <see cref="ActionStateType.AddBoard"/>.
        /// </summary>
        /// <returns>Whether this Action is finished</returns>
        public override bool Update()
        {
            switch (progress)
            {
                case ProgressState.GettingPosition:
                    if (BoardAdder.GetPosition(out Vector3 position))
                    {
                        progress = ProgressState.GettingRotation;
                        memento = new Memento(new BoardConfig { Position = position });
                        GameObject slider = PrefabInstantiator.InstantiatePrefab(boardRotatorPath, 
                            GameObject.Find("UI Canvas").transform, false);
                        slider.GetComponent<AddBoardSliderController>().Setup(position);
                    }

                    return false;
                case ProgressState.GettingRotation:
                    if (AddBoardSliderController.GetRotation(out Quaternion rotation))
                    {
                        memento.boardConfig.Rotation = rotation;
                        BoardAdder.Stop();
                        progress = ProgressState.GettingName;
                        addBoardDialog = new AddBoardDialog();
                        addBoardDialog.Open();
                    }

                    return false;
                case ProgressState.GettingName:
                    if (addBoardDialog.GetName(out string name))
                    {
                        memento.boardConfig.Title = name;
                        BoardsManager.Create(memento.boardConfig);
                        new CreateBoardNetAction(memento.boardConfig).Execute();
                        currentState = ReversibleAction.Progress.Completed;
                        return true;
                    }

                    if (addBoardDialog.WasCanceled())
                    {
                        progress = ProgressState.GettingPosition;
                        BoardAdder.Init();
                    }

                    return false;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Marks the BoardAdder component as "to be deleted".
        /// </summary>
        public override void Stop()
        {
            BoardAdder.Stop();
        }

        /// <summary>
        /// Deletes the board that was created.
        /// </summary>
        public override void Undo()
        {
            base.Undo();
            BoardsManager.Delete(memento.boardConfig.Title);
            new DeleteBoardNetAction(memento.boardConfig.Title).Execute();
        }

        /// <summary>
        /// This method (re-)executes the action, i.e. creates the board from the given configuration.
        /// </summary>
        public override void Redo()
        {
            base.Redo();
            BoardsManager.Create(memento.boardConfig);
            new CreateBoardNetAction(memento.boardConfig).Execute();
        }
        
        /// <summary>
        /// Returns a new instance of <see cref="AddBoardAction"/>.
        /// </summary>
        /// <returns>The new instance</returns>
        public static ReversibleAction CreateReversibleAction()
        {
            return new AddBoardAction();
        }
        
        /// <summary>
        /// Returns a new instance of <see cref="AddBoardAction"/>.
        /// </summary>
        /// <returns>The new instance</returns>
        public override ReversibleAction NewInstance()
        {
            return CreateReversibleAction();
        }
        
        /// <summary>
        /// The name of the new board.
        /// </summary>
        /// <returns>A HashSet with one string in it which is the name of the new board.</returns>
        public override HashSet<string> GetChangedObjects()
        {
            return new HashSet<string> { memento.boardConfig.Title };
        }

        /// <summary>
        /// Returns the <see cref="ActionStateType"/> of this class.
        /// </summary>
        /// <returns><see cref="ActionStateType.AddBoard"/></returns>
        public override ActionStateType GetActionStateType()
        {
            return ActionStateType.AddBoard;
        }
    }
}
