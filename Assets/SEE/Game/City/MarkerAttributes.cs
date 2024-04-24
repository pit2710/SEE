using SEE.Utils.Config;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Game.City
{
    /// <summary>
    /// Attributes to mark changes.
    /// </summary>
    [Serializable]
    public class MarkerAttributes
    {
        /// <summary>
        /// Default constructor using the default attribute values.
        /// </summary>
        public MarkerAttributes() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="height">The height of posts used as markers.</param>
        /// <param name="width">The width (x and z lengths) of posts used as markers</param>
        /// <param name="addition">Color for beams of newly added nodes.</param>
        /// <param name="change">Color for beams of changed nodes.</param>
        /// <param name="deletion">Color for beams of newly deleted nodes.</param>
        public MarkerAttributes(float height, float width, Color addition, Color change, Color deletion)
        {
            MarkerHeight = height;
            MarkerWidth = width;
            AdditionBeamColor = addition;
            ChangeBeamColor = change;
            DeletionBeamColor = deletion;
        }

        /// <summary>
        /// The height of posts used as markers.
        /// </summary>
        [Tooltip("The height of posts used as markers for new, changed, and deleted elements (>=0).")]
        [Range(0, 1)]
        public float MarkerHeight = 0.2f;

        /// <summary>
        /// The width (x and z lengths) of posts used as markers.
        /// </summary>
        [Tooltip("The width (x and z lengths) of posts used as markers for new and deleted elements (>=0).")]
        [Range(0, 1)]
        public float MarkerWidth = 0.01f;

        /// <summary>
        /// Color for beams of newly added nodes.
        /// </summary>
        [Tooltip("The color of the beam for newly created nodes.")]
        public Color AdditionBeamColor = Color.green;

        /// <summary>
        /// Color for beams of changed nodes.
        /// </summary>
        [Tooltip("The color of the beam for changed nodes.")]
        public Color ChangeBeamColor = Color.yellow;

        /// <summary>
        /// Color for beams of newly deleted nodes.
        /// </summary>
        [Tooltip("The color of the beam for deleted nodes.")]
        public Color DeletionBeamColor = Color.black;

        #region Configuration file input/output
        //--------------------------------
        // Configuration file input/output
        //--------------------------------

        /// <summary>
        /// Label of attribute <see cref="MarkerHeight"/> in the configuration file.
        /// </summary>
        private const string markerHeightLabel = "MarkerHeight";
        /// <summary>
        /// Label of attribute <see cref="MarkerWidth"/> in the configuration file.
        /// </summary>
        private const string markerWidthLabel = "MarkerWidth";
        /// <summary>
        /// Label of attribute <see cref="AdditionBeamColor"/> in the configuration file.
        /// </summary>
        private const string additionBeamColorLabel = "AdditionBeamColor";
        /// <summary>
        /// Label of attribute <see cref="ChangeBeamColor"/> in the configuration file.
        /// </summary>
        private const string changeBeamColorLabel = "ChangeBeamColor";
        /// <summary>
        /// Label of attribute <see cref="DeletionBeamColor"/> in the configuration file.
        /// </summary>
        private const string deletionBeamColorLabel = "DeletionBeamColor";

        public void Save(ConfigWriter writer, string label)
        {
            writer.BeginGroup(label);
            writer.Save(MarkerHeight, markerHeightLabel);
            writer.Save(MarkerWidth, markerWidthLabel);
            writer.Save(AdditionBeamColor, additionBeamColorLabel);
            writer.Save(ChangeBeamColor, changeBeamColorLabel);
            writer.Save(DeletionBeamColor, deletionBeamColorLabel);
            writer.EndGroup();
        }

        public void Restore(Dictionary<string, object> attributes)
        {
            ConfigIO.Restore(attributes, markerHeightLabel, ref MarkerHeight);
            ConfigIO.Restore(attributes, markerWidthLabel, ref MarkerWidth);
            ConfigIO.Restore(attributes, additionBeamColorLabel, ref AdditionBeamColor);
            ConfigIO.Restore(attributes, changeBeamColorLabel, ref ChangeBeamColor);
            ConfigIO.Restore(attributes, deletionBeamColorLabel, ref DeletionBeamColor);
        }
        #endregion
    }
}
