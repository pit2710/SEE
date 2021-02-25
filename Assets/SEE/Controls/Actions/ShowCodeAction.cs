﻿using SEE.Game.UI.CodeWindow;
using SEE.GO;
using UnityEngine;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Action to display the source code of the currently selected node using <see cref="CodeWindow"/>s.
    /// </summary>
    internal class ShowCodeAction : MonoBehaviour
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
        private readonly ActionStateType ThisActionState = ActionStateType.ShowCode;

        /// <summary>
        /// The currently displayed <see cref="CodeWindow"/>.
        /// <c>null</c> if no code window is currently displayed.
        /// </summary>
        private CodeWindow codeWindow;

        /// <summary>
        /// The selected node.
        /// </summary>
        private NodeRef selectedNode;

        /// <summary>
        /// The currently selected node.
        /// This is a cached version of <see cref="selectedNode"/> and used to determine
        /// whether we need to change which code window is currently displayed.
        /// </summary>
        private NodeRef currentlySelectedNode;

        /// <summary>
        /// The selected node's filename.
        /// </summary>
        private string selectedPath;

        private void Start()
        {
            // An anonymous delegate is registered for the event <see cref="ActionState.OnStateChanged"/>.
            // This delegate will be called from <see cref="ActionState"/> upon every
            // state changed where the passed parameter is the newly entered state.
            ActionState.OnStateChanged += (ActionStateType newState) =>
            {
                // Is this our action state where we need to do something?
                if (Equals(newState, ThisActionState))
                {
                    // The MonoBehaviour is enabled and Update() will be called by Unity.
                    enabled = true;
                    InteractableObject.LocalAnySelectIn += LocalAnySelectIn;
                    InteractableObject.LocalAnySelectOut += LocalAnySelectOut;
                }
                else
                {
                    // The MonoBehaviour is disabled and Update() no longer be called by Unity.
                    enabled = false;
                    InteractableObject.LocalAnySelectIn -= LocalAnySelectIn;
                    InteractableObject.LocalAnySelectOut -= LocalAnySelectOut;
                }
            };
            enabled = ActionState.Is(ThisActionState);
        }

        private void Update()
        {
            // This script should be disabled if the action state is not this action's type
            if (!ActionState.Is(ThisActionState))
            {
                // The MonoBehaviour is disabled and Update() no longer be called by Unity.
                enabled = false;
                InteractableObject.LocalAnySelectIn -= LocalAnySelectIn;
                InteractableObject.LocalAnySelectOut -= LocalAnySelectOut;
                return;
            }
            
            if (!Equals(selectedNode?.Value, currentlySelectedNode?.Value))
            {
                // Hide code window in old selection
                if (currentlySelectedNode?.TryGetComponent(out codeWindow) ?? false)
                {
                    codeWindow.Show(false);
                }
                currentlySelectedNode = selectedNode;
                // If nothing is selected, there's nothing more we need to do
                if (selectedNode == null)
                {
                    return;
                }

                // Create new code window for active selection, or use existing one
                if (!selectedNode.TryGetComponent(out codeWindow))
                {
                    GameObject selectedGO = selectedNode.gameObject;
                    codeWindow = selectedNode.gameObject.AddComponent<CodeWindow>();
                    codeWindow.Anchor = selectedGO;
                    codeWindow.Title = selectedNode.Value.SourceName;
                    if (!selectedNode.Value.TryGetString("Source.File", out string selectedFile))
                    {
                        Debug.LogError("Source.Path was set, but Source.File was not. Can't show code window.\n");
                        return;
                    }
                    codeWindow.EnterFromFile($"{selectedPath}{selectedFile}");  // selectedPath has trailing /

                    //TODO: Set font size etc per SEECity settings
                }
                codeWindow.Show(true);
            }
        }

        private void LocalAnySelectIn(InteractableObject interactableObject)
        {
            if (!interactableObject.gameObject.TryGetComponent(out selectedNode) 
                || !selectedNode.Value.TryGetString("Source.Path", out selectedPath))
            {
                    selectedPath = null;
                    selectedNode = null;
            }
        }

        private void LocalAnySelectOut(InteractableObject interactableObject)
        {
            selectedPath = null;
            selectedNode = null;
        }
    }
}
