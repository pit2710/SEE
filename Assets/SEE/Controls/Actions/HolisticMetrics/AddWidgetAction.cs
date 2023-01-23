using System;
using System.Collections.Generic;
using SEE.Game.HolisticMetrics;
using SEE.Game.HolisticMetrics.Components;
using SEE.Game.UI.PropertyDialog.HolisticMetrics;
using SEE.Net.Actions.HolisticMetrics;
using SEE.Utils;
using UnityEngine;

namespace SEE.Controls.Actions.HolisticMetrics
{
    /// <summary>
    /// This class manages the creation of a holistic metrics widget. It is needed so we can also revert the deletion.
    /// </summary>
    internal class AddWidgetAction : AbstractPlayerAction
    {
        private bool gotPosition;
        
        private Memento memento;

        private struct Memento
        {
            /// <summary>
            /// The name of the board on which to create the widget.
            /// </summary>
            internal readonly string boardName;

            /// <summary>
            /// The configuration of the widget that knows how the widget should be created.
            /// </summary>
            internal readonly WidgetConfig config;

            /// <summary>
            /// Assigns the configuration of the widget to create and the name of the board on which to create it to fields
            /// of this class.
            /// </summary>
            /// <param name="boardName">The name of the board on which to create the widget</param>
            /// <param name="config">The configuration; this is how the widget will be configured</param>
            internal Memento(string boardName, WidgetConfig config)
            {
                this.boardName = boardName;
                this.config = config;
            }
        }

        public override void Start()
        {
            BoardsManager.AddWidgetAdders();
        }

        public override bool Update()
        {
            if (!gotPosition)
            {
                if (BoardsManager.GetWidgetAdditionPosition(out string boardName, out Vector3 position))
                {
                    WidgetConfig config = new WidgetConfig { Position = position, ID = Guid.NewGuid() };
                    memento = new Memento(boardName, config);
                    new AddWidgetDialog().Open();
                    gotPosition = true;
                }

                return false;
            }

            if (AddWidgetDialog.GetConfig(out string metric, out string widget))
            {
                memento.config.MetricType = metric;
                memento.config.WidgetName = widget;
                Redo();
                return true;
            }

            return false;
        }

        public override void Stop()
        {
            WidgetAdder.Stop();
        }

        /// <summary>
        /// Deletes the widget from the board on all clients.
        /// </summary>
        public override void Undo()
        {
            WidgetsManager widgetsManager = BoardsManager.Find(memento.boardName);
            if (widgetsManager != null)
            {
                widgetsManager.Delete(memento.config.ID);
                new DeleteWidgetNetAction(memento.boardName, memento.config.ID).Execute();
            }
            else
            {
                Debug.LogError($"No board found with the name {memento.boardName} for deleting the widget.\n");
            }
        }
        
        /// <summary>
        /// Creates the new widget as configured, on all clients.
        /// </summary>
        public override void Redo()
        {
            WidgetsManager widgetsManager = BoardsManager.Find(memento.boardName);
            if (widgetsManager != null)
            {
                widgetsManager.Create(memento.config);
                new CreateWidgetNetAction(memento.boardName, memento.config).Execute();
            }
            else
            {
                Debug.LogError($"No board found with the name {memento.boardName} for adding the widget.\n");
            }
        }

        /// <summary>
        /// Returns a new instance of <see cref="AddWidgetAction"/>.
        /// </summary>
        /// <returns>new instance</returns>
        public static ReversibleAction CreateReversibleAction()
        {
            return new AddWidgetAction();
        }
        
        public override ReversibleAction NewInstance()
        {
            return CreateReversibleAction();
        }

        public override HashSet<string> GetChangedObjects()
        {
            return new HashSet<string> { memento.boardName, memento.config.ID.ToString() };
        }

        public override ActionStateType GetActionStateType()
        {
            return ActionStateType.AddWidget;
        }
    }
}