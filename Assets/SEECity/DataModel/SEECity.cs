﻿using SEE.DataModel;
using SEE.Layout;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SEE
{
    /// <summary>
    /// Settings of the graph data needed at runtime. A SEECity is intended to
    /// be added to a game object in the scene as a component. That game object
    /// is the representation of a graph including the settings that have lead
    /// to its visualization.
    /// </summary>
    public class SEECity : MonoBehaviour
    {
        /// <summary>
        /// The graph that is visualized in the scene and whose settings are 
        /// managed here.
        /// </summary>
        private Graph graph = null;

        /// <summary>
        /// The graph underlying this SEE city. May be null.
        /// the element is currently not in a graph.
        /// </summary>
        public Graph ItsGraph
        {
            get => graph;
            set => graph = value;
        }

        /// <summary>
        /// The prefix of the absolute paths for the GXL and CSV data.
        /// </summary>        
        public string pathPrefix = null;

        /// Clone graph with one directory and two files contained therein.
        //public string gxlPath = "..\\Data\\GXL\\two_files.gxl";
        //public string csvPath = "..\\Data\\GXL\\two_files.csv";

        /// Clone graph with one directory and three files contained therein.
        //public string gxlPath = "..\\Data\\GXL\\three_files.gxl";
        //public string csvPath = "..\\Data\\GXL\\three_files.csv";

        /// Very tiny clone graph with single root, one child as a leaf and 
        /// two more children with two children each to experiment with.
        //public string gxlPath = "..\\Data\\GXL\\micro_clones.gxl";
        //public string csvPath = "..\\Data\\GXL\\micro_clones.csv";

        /// Tiny clone graph with single root to experiment with.
        //public string gxlPath = "..\\Data\\GXL\\minimal_clones.gxl";
        //public string csvPath = "..\\Data\\GXL\\minimal_clones.csv";

        /// Tiny clone graph with single roots to check edge bundling.
        //public string gxlPath = "..\\Data\\GXL\\controlPoints.gxl";
        //public string csvPath = "..\\Data\\GXL\\controlPoints.csv";

        // Smaller clone graph with single root (Linux directory "fs").
        //public string gxlPath = "..\\Data\\GXL\\linux-clones\\fs.gxl";
        //public string csvPath = "..\\Data\\GXL\\linux-clones\\fs.csv";

        // Smaller clone graph with single root (Linux directory "net").

        /// <summary>
        /// The relative path for the GXL file containing the graph data.
        /// </summary>
        public string gxlPath = "..\\Data\\GXL\\linux-clones\\net.gxl";
        /// <summary>
        /// The relative path for the CSV file containing the node metrics.
        /// </summary>
        public string csvPath = "..\\Data\\GXL\\linux-clones\\net.csv";

        // Larger clone graph with single root (Linux directory "drivers"): 16.920 nodes, 10583 edges.
        //public string gxlPath = "..\\Data\\GXL\\linux-clones\\drivers.gxl";
        //public string csvPath = "..\\Data\\GXL\\linux-clones\\drivers.csv";

        // Medium size include graph with single root (OpenSSL).
        //public string gxlPath = "..\\Data\\GXL\\OpenSSL\\openssl-include.gxl";
        //public string csvPath = "..\\Data\\GXL\\OpenSSL\\openssl-include.csv";

        /// <summary>
        /// Returns the concatenation of pathPrefix and gxlPath. That is the complete
        /// absolute path to the GXL file containing the graph data.
        /// </summary>
        /// <returns>concatenation of pathPrefix and gxlPath</returns>
        public string GXLPath()
        {
            return pathPrefix + gxlPath;
        }

        /// <summary>
        /// Returns the concatenation of pathPrefix and csvPath. That is the complete
        /// absolute path to the CSV file containing the additional metric values.
        /// </summary>
        /// <returns>concatenation of pathPrefix and csvPath</returns>
        public string CSVPath()
        {
            return pathPrefix + csvPath;
        }

        /// <summary>
        /// The center origin where the graph should be placed in the world scene.
        /// </summary>
        public Vector3 origin = Vector3.zero;

        /// <summary>
        /// The names of the edge types of hierarchical edges.
        /// </summary>
        public HashSet<string> HierarchicalEdges = Hierarchical_Edge_Types();

        /// <summary>
        /// The names of the edge types of hierarchical edges.
        /// </summary>
        public static HashSet<string> Hierarchical_Edge_Types()
        {
            HashSet<string> result = new HashSet<string>
            {
                "Enclosing",
                "Belongs_To",
                "Part_Of",
                "Defined_In"
            };
            return result;
        }

        //---------------------------------
        // Visual attributes of a leaf node
        //---------------------------------
        /// <summary>
        /// The attribute name of the metric to be used for the width of a building (x co-ordinate).
        /// </summary>
        public string WidthMetric = "Metric.Number_of_Tokens";
        /// <summary>
        /// The attribute name of the metric to be used for the height of a building (y co-ordinate).
        /// </summary>
        public string HeightMetric = "Metric.Clone_Rate";
        /// <summary>
        /// The attribute name of the metric to be used for the breadth of a building (y co-ordinate).
        /// </summary>
        public string DepthMetric = "Metric.LOC";
        /// <summary>
        /// The attribute name of the metric to be used for determining the color gradient of leaf nodes.
        /// </summary>
        public string ColorMetric = "Metric.Complexity";

        /// <summary>
        /// This parameter determines the minimal width, breadth, and height of each block
        /// representing a graph node visually. Must not be greater than MaximalBlockLength.
        /// </summary>
        public float MinimalBlockLength = 0.1f;

        /// <summary>
        /// This parameter determines the maximal width, breadth, and height of each block
        /// representing a graph node visually. Must not be smaller than MinimalBlockLength.
        /// </summary>
        public float MaximalBlockLength = 100.0f;

        /// <summary>
        /// How leaf graph nodes should be depicted.
        /// </summary>
        public enum LeafNodeKinds
        {
            Blocks,
            Buildings,
        }

        /// <summary>
        /// How inner graph nodes should be depicted.
        /// </summary>
        public enum InnerNodeKinds
        {
            Blocks,
            Rectangles,
            Donuts,
            Circles,
            Empty,
            Cylinders,
        }

        /// <summary>
        /// What kinds of game objects are to be created for leaf nodes in the graph.
        /// </summary>
        public LeafNodeKinds LeafObjects;

        /// <summary>
        /// What kinds of game objects are to be created for inner graph nodes.
        /// </summary>
        public InnerNodeKinds InnerNodeObjects;

        /// <summary>
        /// The kinds of node layouts available.
        /// </summary>
        public enum NodeLayouts
        {
            EvoStreets,
            Balloon,
            FlatRectanglePacking,
            Treemap,
            CirclePacking,
            Manhattan,
        }

        /// <summary>
        /// The kinds of edge layouts available.
        /// </summary>
        public enum EdgeLayouts
        {
            None = 0,        // no edges are to be drawn
            Straight = 1,
            Spline = 2,
            Bundling = 3
        }

        /// <summary>
        /// The layout that should be used for nodes.
        /// </summary>
        public NodeLayouts NodeLayout;

        /// <summary>
        /// The layout that should be used for edges.
        /// </summary>
        public EdgeLayouts EdgeLayout;

        /// <summary>
        /// Whether ZScore should be used for normalizing node metrics. If false, linear interpolation
        /// for range [0, max-value] is used, where max-value is the maximum value of a metric.
        /// </summary>
        public bool ZScoreScale = true;

        /// <summary>
        /// The width of the line representing edges.
        /// </summary>
        public float EdgeWidth = 0.3f;

        /// <summary>
        /// Whether erosions should be visible above blocks.
        /// </summary>
        public bool ShowErosions = false;

        /// <summary>
        /// Orientation of the edges; 
        /// if false, the edges are drawn below the houses;
        /// if true, the edges are drawn above the houses;
        /// </summary>
        public bool EdgesAboveBlocks = true;

        //--------------------------------------------------------
        // Software erosion issues shown as icons above leaf nodes
        //--------------------------------------------------------

        /// <summary>
        /// The attribute name of the metric representing architecture violations.
        /// </summary>
        public string ArchitectureIssue = "Metric.Architecture_Violations";
        /// <summary>
        /// The attribute name of the metric representing duplicated code.
        /// </summary>
        public string CloneIssue = "Metric.Clone";
        /// <summary>
        /// The attribute name of the metric representing cylces.
        /// </summary>
        public string CycleIssue = "Metric.Cycle";
        /// <summary>
        /// The attribute name of the metric representing dead code.
        /// </summary>
        public string Dead_CodeIssue = "Metric.Dead_Code";
        /// <summary>
        /// The attribute name of the metric representing metric violations.
        /// </summary>
        public string MetricIssue = "Metric.Metric";
        /// <summary>
        /// The attribute name of the metric representing code-style violations.
        /// </summary>
        public string StyleIssue = "Metric.Style";
        /// <summary>
        /// The attribute name of the metric representing other kinds of violations.
        /// </summary>
        public string UniversalIssue = "Metric.Universal";

        /// <summary>
        /// Returns all attribute names of the different kinds of software erosions.
        /// </summary>
        /// <returns>all attribute names of the different kinds of software erosions</returns>
        public IList<string> AllLeafIssues()
        {
            List<string> result = new List<string>()
               {
                  ArchitectureIssue,
                  CloneIssue,
                  CycleIssue,
                  Dead_CodeIssue,
                  MetricIssue,
                  StyleIssue,
                  UniversalIssue
               };
            return result;
        }

        //-----------------------------------------------------------------------
        // Software erosion issues shown as icons on Donut charts for inner nodes
        //-----------------------------------------------------------------------
        /// <summary>
        /// The attribute name of the metric representing the sum of all architecture violations
        /// for an inner node.
        /// </summary>
        public string ArchitectureIssue_SUM = "Metric.Architecture_Violations_SUM";
        /// <summary>
        /// The attribute name of the metric representing the sum of all clones
        /// for an inner node.
        /// </summary>
        public string CloneIssue_SUM = "Metric.Clone_SUM";
        /// <summary>
        /// The attribute name of the metric representing the sum of all cycles
        /// for an inner node.
        /// </summary>
        public string CycleIssue_SUM = "Metric.Cycle_SUM";
        /// <summary>
        /// The attribute name of the metric representing the sum of all dead entities
        /// for an inner node.
        /// </summary>
        public string Dead_CodeIssue_SUM = "Metric.Dead_Code_SUM";
        /// <summary>
        /// The attribute name of the metric representing the sum of all metric violations
        /// for an inner node.
        /// </summary>
        public string MetricIssue_SUM = "Metric.Metric_SUM";
        /// <summary>
        /// The attribute name of the metric representing the sum of all style violations
        /// for an inner node.
        /// </summary>
        public string StyleIssue_SUM = "Metric.Style_SUM";
        /// <summary>
        /// The attribute name of the metric representing the sum of all other kinds of
        /// software erosions for an inner node.
        /// </summary>
        public string UniversalIssue_SUM = "Metric.Universal_SUM";

        /// <summary>
        /// Returns all attribute names of the different kinds of software erosions for inner
        /// nodes (the sums of their descendants).
        /// </summary>
        /// <returns>all attribute names of the different kinds of software erosions for inner nodes</returns>
        public IList<string> AllInnerNodeIssues()
        {
            List<string> result = new List<string>()
               {
                  ArchitectureIssue_SUM,
                  CloneIssue_SUM,
                  CycleIssue_SUM,
                  Dead_CodeIssue_SUM,
                  MetricIssue_SUM,
                  StyleIssue_SUM,
                  UniversalIssue_SUM
               };
            return result;
        }

        /// <summary>
        /// The metric to be put in the inner circle of a Donut chart.
        /// </summary>
        public string InnerDonutMetric = "Metric.IssuesTotal";

        /// <summary>
        /// Yields a mapping of all node attribute names that define erosion issues 
        /// for leaf nodes in the GXL file onto the icons to be used for visualizing them.
        /// </summary>
        /// <returns>mapping of all node attribute names for leaves onto icon ids</returns>
        public SerializableDictionary<string, IconFactory.Erosion> LeafIssueMap()
        {
            SerializableDictionary<string, IconFactory.Erosion> result = new SerializableDictionary<string, IconFactory.Erosion>
            {
                { ArchitectureIssue, IconFactory.Erosion.Architecture_Violation },
                { CloneIssue, IconFactory.Erosion.Clone },
                { CycleIssue, IconFactory.Erosion.Cycle },
                { Dead_CodeIssue, IconFactory.Erosion.Dead_Code },
                { MetricIssue, IconFactory.Erosion.Metric },
                { StyleIssue, IconFactory.Erosion.Style },
                { UniversalIssue, IconFactory.Erosion.Universal }
            };
            return result;
        }

        /// <summary>
        /// Loads the graph data from the GXL file with GXLPath() and the metrics
        /// from the CSV file with CSVPath().
        /// </summary>
        public void LoadGraph()
        {
            if (string.IsNullOrEmpty(GXLPath()))
            {
                Debug.LogError("Empty graph path.\n");
            }
            else
            {
                SEE.Performance p = SEE.Performance.Begin("loading graph data from " + GXLPath());
                GraphReader graphCreator = new GraphReader(GXLPath(), HierarchicalEdges, new SEELogger());
                graphCreator.Load();
                graph = graphCreator.GetGraph();
                graph.CalculateLevels();
                p.End();
                Debug.Log("Number of nodes loaded: " + graph.NodeCount + "\n");
                Debug.Log("Number of edges loaded: " + graph.EdgeCount + "\n");
                LoadMetrics();
                DrawGraph();
            }
        }

        /// <summary>
        /// Draws the graph.
        /// Precondition: The graph and its metrics have been loaded.
        /// </summary>
        private void DrawGraph()
        {
            if (ReferenceEquals(ItsGraph, null))
            {
                Debug.LogError("No graph loaded.\n");
            }
            else
            {
                GraphRenderer renderer = new GraphRenderer(this);
                // We assume here that this SEECity instance was added to a game object as
                // a component. The inherited attribute gameObject identifies this game object.
                renderer.Draw(ItsGraph, gameObject);
                // If CScape buildings are used, the scale of the world is larger and, hence, the camera needs to move faster.
                // We may have cities with blocks and cities with CScape buildings in the same scene.
                // We cannot simply alternate the speed each time when a graph is loaded.
                // Cameras.AdjustCameraSpeed(renderer.Unit());
            }
        }

        /// <summary>
        /// Loads the metrics from CSVPath() and aggregates and adds them to the graph.
        /// Precondition: graph must have been loaded before.
        /// </summary>
        private void LoadMetrics()
        {
            int numberOfErrors = MetricImporter.Load(ItsGraph, CSVPath());
            if (numberOfErrors > 0)
            {
                Debug.LogErrorFormat("CSV file {0} has {1} many errors.\n", CSVPath(), numberOfErrors);
            }
            {
                MetricAggregator.AggregateSum(ItsGraph, AllLeafIssues().ToArray<string>());
                // Note: We do not want to compute the derived metric editorSettings.InnerDonutMetric
                // when we have a single root node in the graph. This metric will be used to define the color
                // of inner circles of Donut charts. Because the color is a linear interpolation of the whole
                // metric value range, the inner circle would always have the maximal value (it is the total
                // sum over all) and hence the maximal color gradient. The color of the other nodes would be
                // hardly distinguishable. 
                // FIXME: We need a better solution. This is a kind of hack.
                MetricAggregator.DeriveSum(ItsGraph, AllInnerNodeIssues().ToArray<string>(), InnerDonutMetric, true);
            }
        }

        /// <summary>
        /// Destroys the underlying graph and all game objects visualizing information about it.
        /// </summary>
        public void DeleteGraph()
        {
            // Delete all children.
            // Note: foreach (GameObject child in transform)... would not work;
            // we really need to collect all children first and only then can destroy each.
            foreach (GameObject child in AllChildren())
            {
                Destroyer.DestroyGameObject(child);
            }
            // Delete the underlying graph.
            if (graph != null)
            {
                graph.Destroy();
            }
            graph = null;
        }

        /// <summary>
        /// Returns all immediate children of the game object this SEECity is attached to.
        /// </summary>
        /// <returns>immediate children of the game object this SEECity is attached to</returns>
        private List<GameObject> AllChildren()
        {
            List<GameObject> result = new List<GameObject>();
            foreach (Transform child in transform)
            {
                result.Add(child.gameObject);
            }
            return result;
        }
    }
}
