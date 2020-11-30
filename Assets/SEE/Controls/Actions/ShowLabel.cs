﻿using SEE.DataModel.DG;
using SEE.Game;
using SEE.GO;
using SEE.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR.InteractionSystem;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Shows the source name of the hovered object as a text label above the 
    /// object. In between that label and the game object, a connecting bar
    /// will be shown.
    /// </summary>
    public class ShowLabel : InteractableObjectHoveringAction
    {
        /// <summary>
        /// Sets <see cref="isLeaf"/> and <see cref="city"/>.
        /// </summary>
        protected override void Awake()           
        {
            base.Awake();  // Must be called to register for the hovering events.
            isLeaf = SceneQueries.IsLeaf(gameObject);
            GameObject codeCityObject = SceneQueries.GetCodeCity(gameObject.transform)?.gameObject;
            Assert.IsTrue(codeCityObject != null);
            codeCityObject.TryGetComponent(out city);
        }

        /// <summary>
        /// True if this node is a leaf. This value is cached to avoid frequent retrievals.
        /// </summary>
        private bool isLeaf;

        /// <summary>
        /// The text label that's displayed above the object when the user hovers over it.
        /// Will be <code>null</code> when the label is not currently being displayed.
        /// This nodeLabel will contain a TextMeshPro component for the label text and a
        /// LineRenderer that connects the labeled object and the label text visually.
        /// </summary>
        private GameObject nodeLabel;

        /// <summary>
        /// Settings for the visualization of the node.
        /// </summary>
        private AbstractSEECity city;

        /// <summary>
        /// Returns true iff labels are enabled for this node type.
        /// </summary>
        /// <returns>true iff labels are enabled for this node type</returns>
        private bool LabelsEnabled()
        {
            return isLeaf && city.ShowLabel || !isLeaf && city.InnerNodeShowLabel;
        }

        /// <summary>
        /// Creates a text label above the object with its node's SourceName if the label doesn't exist yet.
        /// </summary>
        /// <param name="isOwner">true if a local user initiated this call</param>
        protected override void On(bool isOwner)
        {
            if (!LabelsEnabled())
            {
                return;  // If labels are disabled, we don't need to do anything
            }

            // If label already exists, nothing needs to be done
            if (nodeLabel != null || !gameObject.TryGetComponent(out NodeRef nodeRef))
            {
                return;
            }

            Node node = nodeRef.node;
            if (node == null)
            {
                return;
            }

            // Add text
            Vector3 position = gameObject.transform.position;
            position.y += isLeaf ? city.LeafLabelDistance : city.InnerNodeLabelDistance;
            nodeLabel = TextFactory.GetTextWithSize(node.SourceName, position,
                isLeaf ? city.LeafLabelFontSize : city.InnerNodeLabelFontSize, textColor: Color.black);
            nodeLabel.name = "Label " + node.SourceName;
            nodeLabel.transform.SetParent(gameObject.transform);

            // Add connecting line between "roof" of object and text
            Vector3 labelPosition = nodeLabel.transform.position;
            Vector3 nodeTopPosition = gameObject.transform.position;
            nodeTopPosition.y = BoundingBox.GetRoof(new List<GameObject> { gameObject });
            labelPosition.y -= nodeLabel.GetComponent<TextMeshPro>().textBounds.extents.y;
            LineFactory.Draw(nodeLabel, new[] { nodeTopPosition, labelPosition }, 0.01f,
                Materials.New(Materials.ShaderType.TransparentLine, Color.black.ColorWithAlpha(0.9f)));

            Portal.SetInfinitePortal(nodeLabel);
        }

        /// <summary>
        /// Destroys the text label above the object if it exists.
        /// 
        ///  <seealso cref="On"/>
        /// </summary>
        /// <param name="isOwner">true if a local user initiated this call</param>
        protected override void Off(bool isOwner)
        {
            // If labels are disabled, we don't need to do anything
            if (LabelsEnabled() && nodeLabel != null)
            {
                Destroyer.DestroyGameObject(nodeLabel);
            }
        }
    }
}