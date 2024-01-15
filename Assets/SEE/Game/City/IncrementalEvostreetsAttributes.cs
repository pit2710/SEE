using System;
using System.Collections.Generic;
using System.ComponentModel;
using SEE.Utils.Config;
using Sirenix.OdinInspector;
using UnityEngine;
namespace SEE.Game.City
{
    /// <summary>
    /// The settings for <see cref="Layout.NodeLayouts.IncrementalEvostreetsLayout"/>.
    /// </summary>
    [Serializable]
    public class IncrementalEvostreetsAttributes : ConfigIO.IPersistentConfigItem
    {
        /// <summary>
        /// The depth of the local moves search.
        /// </summary>
        [SerializeField]
        [Range(0, 5)]
        [Tooltip("The maximal depth for local moves algorithm. Increase for higher visual quality, " +
                 "decrease for higher stability and to save runtime")]
        public int LocalMovesDepth = 3;

        /// <summary>
        /// The maximal branching factor of the local moves search.
        /// </summary>
        [SerializeField]
        [Range(1, 10)]
        [Tooltip("The maximal branching factor for local moves algorithm.  Increase for higher visual quality, " +
                 "decrease for higher stability and to save runtime")]
        public int LocalMovesBranchingLimit = 4;

        /// <summary>
        /// Defines the specific p norm used in the local moves algorithm. See here:
        /// <see cref="Layout.NodeLayouts.IncrementalEvostreets.LocalMoves.AspectRatiosPNorm"/>.
        ///
        /// Notice:
        /// The kind of p norm changes which layout is considered to have the greatest visual quality.
        /// For example with p=1 (Manhattan Norm) the algorithm would
        /// minimize the sum of aspect ratios, while with p=infinity (Chebyshev Norm)
        /// the algorithm would minimize the maximal aspect ratio over the layout nodes.
        /// The other p norms range between these extremes.
        ///
        /// Needs therefor a mapping from <see cref="PNormRange"/> to a double value p, which is realized with the
        /// property <see cref="PNorm"/>.
        /// </summary>
        [SerializeField]
        [Tooltip("Norm for the visual quality of a set of nodes, " +
                 "larger p values lead to stronger penalties for larger deviations in aspect ratio of single nodes.")]
        private PNormRange pNorm = PNormRange.P2Euclidean;

        /// <summary>
        /// The absolute padding between neighboring nodes so that they can be distinguished (in millimeters).
        /// </summary>
        [SerializeField]
        [Range(0.1f, 100f)]
        [LabelText("Padding (mm)")]
        [Tooltip("The distance between two neighbour nodes in mm")]
        public float PaddingMm = 5f;

        /// <summary>
        /// The maximal error for the method
        /// <see cref="Layout.NodeLayouts.IncrementalTreeMap.CorrectAreas.GradientDecent"/> as power of 10.
        /// </summary>
        [SerializeField]
        [Range(-7, -2)]
        [LabelText("Gradient Descent Precision (10^n)")]
        [Tooltip("The maximal error for the gradient descent method as power of 10")]
        public int GradientDescentPrecision = -4;

        /// <summary>
        /// Maps <see cref="pNorm"/> to a double.
        /// </summary>
        public double PNorm => pNorm switch
        {
            (PNormRange.P1Manhattan) => 1d,
            (PNormRange.P2Euclidean) => 2d,
            (PNormRange.P3) => 3d,
            (PNormRange.P4) => 4d,
            (PNormRange.PInfinityChebyshev) => double.PositiveInfinity,
            _ => throw new InvalidEnumArgumentException("Unrecognized PNormRange value.")
        };

        public void Save(ConfigWriter writer, string label)
        {
            writer.BeginGroup(label);
            writer.Save(LocalMovesDepth, localMovesDepthLabel);
            writer.Save(LocalMovesBranchingLimit, localMovesBranchingLimitLabel);
            writer.Save(pNorm.ToString(), pNormLabel);
            writer.Save(GradientDescentPrecision, gradientDescentPrecisionLabel);
            writer.Save(PaddingMm, paddingLabel);
            writer.EndGroup();
        }

        public bool Restore(Dictionary<string, object> attributes, string label)
        {
            if (!attributes.TryGetValue(label, out object dictionary))
            {
                return false;
            }
            Dictionary<string, object> values = dictionary as Dictionary<string, object>;
            bool result = ConfigIO.Restore(values, localMovesDepthLabel, ref LocalMovesDepth);
            result |= ConfigIO.Restore(values, localMovesBranchingLimitLabel, ref LocalMovesBranchingLimit);
            result |= ConfigIO.RestoreEnum(values, pNormLabel, ref pNorm);
            result |= ConfigIO.Restore(values, gradientDescentPrecisionLabel, ref GradientDescentPrecision);
            result |= ConfigIO.Restore(values, paddingLabel, ref PaddingMm);
            return result;
        }

        /// <summary>
        /// Configuration label for <see cref="LocalMovesDepth"/>.
        /// </summary>
        private const string localMovesDepthLabel = "LocalMovesDepth";
        /// <summary>
        /// Configuration label for <see cref="LocalMovesBranchingLimit"/>.
        /// </summary>
        private const string localMovesBranchingLimitLabel = "LocalMovesBranchingLimit";
        /// <summary>
        /// Configuration label for <see cref="PNorm"/>.
        /// </summary>
        private const string pNormLabel = "PNorm";
        /// <summary>
        /// Configuration label for <see cref="GradientDescentPrecision"/>.
        /// </summary>
        private const string gradientDescentPrecisionLabel = "GradientDescentPrecision";
        /// <summary>
        /// Configuration label for <see cref="PaddingMm"/>.
        /// </summary>
        private const string paddingLabel = "Padding";
    }



}     