﻿using UnityEngine;
using UnityEngine.UI;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Creates a clone of a canvas-prefab for adding a new node. Extracts these values from the canvas 
    /// after closing it.
    /// </summary>
    public class AddingNodeCanvasAction : NodeCanvasAction
    {
        /// <summary>
        /// The directory of the AddingNodeCanvas prefab.
        /// </summary>
        private static readonly string prefabDirectory = "Prefabs/NewNodeCanvas";

        void Start()
        {
            /// Note: It is important that the Prefab is contained in the Resources folder to use the 
            /// Resources.Load method.
            InstantiatePrefab(prefabDirectory);
            canvas.transform.SetParent(gameObject.transform);
        }

        /// <summary>
        /// Extracts the given node name, the node type and whether it is an inner node or a leaf from the canvas.
        /// Therefore, it extracts the string from the InputFields on the prefab.
        /// Note: The sequences of the extracted arrays are based on the sequence of the components in the prefab.
        /// </summary>
        public void GetNodeMetrics()
        {
            // FIXME: this part has to be removed by the new UI Team
            AddingNodeCanvasAction script = gameObject.GetComponent<AddingNodeCanvasAction>();

            Component[] c = script.canvas.GetComponentsInChildren<InputField>();
            InputField inputname = (InputField)c[0];
            InputField inputtype = (InputField)c[1];

            Component toggleGroup = script.canvas.GetComponentInChildren<ToggleGroup>();
            Toggle[] toggles = toggleGroup.GetComponentsInChildren<Toggle>();

            if (toggles[0].isOn)
            {
                NewNodeAction.IsInnerNode = true;
            }
            if (toggles[1].isOn)
            {
                NewNodeAction.IsInnerNode = false;
            }
            string inputNodename = inputname.text;
            string inputNodetype = inputtype.text;
            //until here 

            NewNodeAction.Nodename = inputNodename;
            NewNodeAction.Nodetype = inputNodetype;
        }
    }
}