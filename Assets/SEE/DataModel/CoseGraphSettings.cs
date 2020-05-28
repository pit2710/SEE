﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SEE.Layout;
using static SEE.Game.AbstractSEECity;

namespace SEE.DataModel
{
    /// <summary>
    /// This class holds all settings for the cose layout
    /// </summary>
    public class CoseGraphSettings
    {
        /// <summary>
        /// The ideal Length of the edge
        /// </summary>
        public int EdgeLength = CoseLayoutSettings.Edge_Length;

        /// <summary>
        /// If true the edge length is calcualte with the feature: use smart ideal edge calculation
        /// </summary>
        public bool UseSmartIdealEdgeCalculation = CoseLayoutSettings.Use_Smart_Ideal_Edge_Calculation;

        /// <summary>
        /// If true the feature: use smart multilevel calculation is used, the edge length ajusts for each level
        /// </summary>
        public bool UseSmartMultilevelScaling = CoseLayoutSettings.Use_Smart_Multilevel_Calculation;

        /// <summary>
        /// the factor by which the edge length of intergraph edges is enlarged
        /// </summary>
        public float PerLevelIdealEdgeLengthFactor = CoseLayoutSettings.Per_Level_Ideal_Edge_Length_Factor;

        /// <summary>
        /// if true the feature: smart repulsion range calculation is used (Grid variant)
        /// </summary>
        public bool UseSmartRepulsionRangeCalculation = CoseLayoutSettings.Use_Smart_Repulsion_Range_Calculation;

        /// <summary>
        /// the strength of the gravity (root graph)
        /// </summary>
        public float GravityStrength = CoseLayoutSettings.Gravity_Strength;

        /// <summary>
        /// strength of the gravity in compound nodes (not root graph)
        /// </summary>
        public float CompoundGravityStrength = CoseLayoutSettings.Compound_Gravity_Strength;

        /// <summary>
        /// the repulsion strength
        /// </summary>
        public float RepulsionStrength = CoseLayoutSettings.Repulsion_Strength;

        /// <summary>
        /// if true the feature: multilevel scaling is used
        /// </summary>
        public bool multiLevelScaling = CoseLayoutSettings.Multilevel_Scaling;

        /// <summary>
        /// key: dir ids, value: bool, if true the dir is a layouted by a sublayout
        /// </summary>
        public Dictionary<string, bool> ListDirToggle = new Dictionary<string, bool>();

        /// <summary>
        ///  key: dir ids, value: the nodelayout
        /// </summary>
        public Dictionary<string, NodeLayouts> DirNodeLayout = new Dictionary<string, NodeLayouts>();

        /// <summary>
        /// key: dir ids, value: the inner node kind
        /// </summary>
        public Dictionary<string, InnerNodeKinds> DirShape = new Dictionary<string, InnerNodeKinds>();

        /// <summary>
        /// a list of root dirs from the current graph
        /// </summary>
        public List<Node> rootDirs = new List<Node>();

        /// <summary>
        /// key: dir ids, value: bool, if true the dir is shown in the foldout, if false the sectioon feldout is collapsed 
        /// </summary>
        public Dictionary<string, bool> show = new Dictionary<string, bool>();

        /// <summary>
        /// if true the potalgorithm is used
        /// </summary>
        public bool useOptAlgorithm = false;

        /// <summary>
        /// if true is listing of dirs with posiible nodelayouts and inner node kinds is shown
        /// </summary>
        public bool showGraphListing = true;

        /// <summary>
        /// the nodetypes
        /// </summary>
        public Dictionary<string, bool> loadedForNodeTypes = new Dictionary<string, bool>();

        /// <summary>
        /// is true the parameter edgeLength and repulsion strength are calculated automatically
        /// </summary>
        public bool useCalculationParameter = false;

        /// <summary>
        /// is true the parameter edgeLength and repulsion strength are calculated automatically and are iteratily changed till a goog layout is found
        /// </summary>
        public bool useItertivCalclation = false; 
    }
}

