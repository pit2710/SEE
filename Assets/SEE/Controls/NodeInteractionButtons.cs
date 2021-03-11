﻿using UnityEngine;
using UnityEngine.UI;
using SEE.Controls.Actions;
using System.Linq;
using SEE.Utils;

namespace SEE.Controls
{
    /// <summary>
    /// This component is added to the button of the adding-node canvas and the edit-node canvas.
    /// FIXME: What is the purpose of this component?
    /// </summary>
    public class NodeInteractionButtons : MonoBehaviour
    {
        /// <summary>
        /// The button on the adding-node canvas that is finishing the addition of a new node.
        /// </summary>
        public Button AddingButton;

        /// <summary>
        /// The button on the addNode canvas that is canceling the addition of a new node.
        /// </summary>
        public Button AddNodeCancel;

        /// <summary>
        /// The button on the editNode canvas that is canceling the editNode process.
        /// </summary>
        public Button EditNodeCancel;

        /// <summary>
        /// The button on the editNode canvas that is finishing the editNode process.
        /// </summary>
        public Button EditNodeButton;

        /// <summary>
        /// The player desktop that is the parent of the <see cref="AddNodeAction"/> 
        /// and <see cref="EditNodeAction"/>.
        /// </summary>
        private GameObject playerDesktop;

        /// <summary>
        /// The name of the GameObject that is the parent of the <see cref="AddNodeAction"/> 
        /// and <see cref="EditNodeAction"/>.
        /// </summary>
        private const string gameObjectName = "Player Desktop";

        /// <summary>
        /// The action which is currently executed.
        /// </summary>
        public static ReversibleAction addOrEditNode;

        /// <summary>
        /// Adds a listener to the button which calls a method when the button is pushed.
        /// </summary>   
        void Start()
        {
            AddingButton?.onClick?.AddListener(SetNextAddingNodeStep);
            EditNodeCancel?.onClick?.AddListener(EditIsCanceled);
            EditNodeButton?.onClick?.AddListener(EditNode);
            AddNodeCancel?.onClick?.AddListener(AddingIsCanceled);

            playerDesktop = GameObject.Find(gameObjectName);
        }

        /// <summary>
        /// Increases the progress enum in the <see cref="AddNodeAction"/> instance. 
        /// This results in the next step of addingNode.
        /// </summary>
        public void SetNextAddingNodeStep()
        {
            AddNodeAction addNodeAction = (AddNodeAction)addOrEditNode;
            addNodeAction.Progress = AddNodeAction.ProgressState.CanvasIsClosed;
        }

        /// <summary>
        /// Sets a bool in the <see cref="EditNodeAction"/> which closes the adding-node canvas.
        /// FIXME: There is no bool here. This comment must be fixed.
        /// </summary>
        public void EditIsCanceled()
        {
            EditNodeAction editNodeAction = (EditNodeAction)addOrEditNode;
            editNodeAction.EditProgress = EditNodeAction.ProgressState.EditIsCanceled;
        }

        /// <summary>
        /// Sets <see cref="EditNodeCanvasAction.EditNode"/> to true, which starts the edit process 
        /// and evaluation of the inputFields.
        /// </summary>
        public void EditNode()
        {
            EditNodeCanvasAction.EditNode = true;
        }

        /// <summary>
        /// Marks the action to add a new node as being canceled.
        /// </summary>
        public void AddingIsCanceled()
        {
            AddNodeAction addNodeAction = (AddNodeAction)addOrEditNode;
            addNodeAction.Progress = AddNodeAction.ProgressState.AddingIsCanceled;
        }
    }
}