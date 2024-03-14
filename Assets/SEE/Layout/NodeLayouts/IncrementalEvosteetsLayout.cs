using SEE.DataModel.DG;
using SEE.Layout.NodeLayouts.Cose;
using SEE.Layout.NodeLayouts.IncrementalEvostreets;
using System;
using System.Collections.Generic;
using System.Linq;
using SEE.Game.City;
using UnityEngine;
using UnityEngine.Assertions;
using Node = SEE.Layout.NodeLayouts.IncrementalEvostreets.Node;
using SEE.Layout.NodeLayouts.EvoStreets;

namespace SEE.Layout.NodeLayouts
{
    public class IncrementalEvostreetsLayout : HierarchicalNodeLayout, IIncrementalNodeLayout
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="groundLevel">the y co-ordinate setting the ground level</param>
        /// <param name="width">width of the rectangle in which to place all nodes in Unity units</param>
        /// <param name="depth">width of the rectangle in which to place all nodes in Unity units</param>
        /// <param name="settings">the settings for the layout</param>
        public IncrementalEvostreetsLayout(float groundLevel,
            float width,
            float depth,
            IncrementalEvostreetsAttributes settings)
            : base(groundLevel)
        {
            Name = "IncrementalEvostreets";
            this.width = width;
            this.depth = depth;
            this.settings = settings;      
        }

        static int layoutLevel = -1;

        /// <summary>
        /// <see cref="CalculateStreetWidth(IList{ILayoutNode})"/> determines a statistical
        /// parameter of the widths and depths of all leaf nodes (the average) and adjusts
        /// this statistical parameter by multiplying it with this factor <see cref="streetWidthPercentage"/>.
        /// </summary>
        private const float streetWidthPercentage = 0.3f;

        /// <summary>
        /// Is used to calculate the offset between buildings as this factor multiplied by
        /// the absolute street width for the root node.
        /// </summary>
        private const float offsetBetweenBuildingsPercentage = 0.3f;

        /// <summary>
        /// The height (y co-ordinate) of game objects (inner tree nodes) represented by streets.
        /// The actual value used will be multiplied by leafNodeFactory.Unit.
        /// </summary>
        private readonly float streetHeight = 0.0001f;

        /// <summary>
        /// The adjustable parameters for the layout.
        /// </summary>
        private readonly IncrementalEvostreetsAttributes settings;

        /// <summary>
        /// The width of the rectangle in which to place all nodes in Unity units.
        /// </summary>
        private readonly float width;

        /// <summary>
        /// The depth of the rectangle in which to place all nodes in Unity units.
        /// </summary>
        private readonly float depth;

        /// <summary>
        /// The layout computed by this layouter. It needs to be kept for the next layout.
        /// If the there is no old layout, it can be null.
        /// </summary>
        private readonly Dictionary<ILayoutNode, NodeTransform> layoutResult = new();

        /// <summary>
        /// A map to find a node (fast) by its ID
        /// </summary>
        private readonly Dictionary<string, Node> nodeMap = new();

        /// <summary>
        /// A map to find a ILayoutNode (fast) by its ID
        /// </summary>
        private readonly Dictionary<string, ILayoutNode> iLayoutNodeMap = new();

        /// <summary>
        /// The layout of the last revision in the evolution. Can be null.
        /// </summary>
        private IncrementalEvostreetsLayout oldLayout;

        /// <summary>
        /// Property for <see cref="oldLayout"/>
        /// </summary>
        /// <exception cref="ArgumentException">throws exception if the set <see cref="IIncrementalNodeLayout"/>
        /// is not a <see cref="IncrementalEvostreetsLayout"/>.</exception>
        public IIncrementalNodeLayout OldLayout
        {
            set
            {
                if (value is IncrementalEvostreetsLayout layout)
                {
                    this.oldLayout = layout;
                }
                else
                {
                    throw new ArgumentException(
                        "Predecessor of IncrementalEvosteetsLayout was not an IncrementalEvosteetsLayout.");
                }
            }
        }
        public override Dictionary<ILayoutNode, NodeTransform> Layout(IEnumerable<ILayoutNode> layoutNodes)
        {

            //Set the layoutlevel +1 for each created Layout
            layoutLevel += 1;

            //UnityEngine.Debug.LogWarning($"LayoutLevel:{layoutLevel}");
            List<ILayoutNode> layoutNodesList = layoutNodes.ToList();
            if (!layoutNodesList.Any())
            {
                throw new ArgumentException("No nodes to be laid out.");
            }            
         
            if (oldLayout == null)
            {
                IncrementalEvostreets.Rectangle rectangle = new IncrementalEvostreets.Rectangle(x: -width / 2.0f, z: -depth / 2.0f, width, depth);
                this.Roots = LayoutNodes.GetRoots(layoutNodesList);
                InitNodes();
                LayoutDescriptor treeDescriptor;
                treeDescriptor.StreetWidth = CalculateStreetWidth(layoutNodes.ToList());
                treeDescriptor.OffsetBetweenBuildings = treeDescriptor.StreetWidth * offsetBetweenBuildingsPercentage;
                ILayoutNode root = Roots.FirstOrDefault();
                ENode rootNode = GenerateHierarchy(root);
                treeDescriptor.MaximalDepth = MaxDepth(root);
                treeDescriptor.StreetWidth = CalculateStreetWidth(layoutNodes.ToList());
                rootNode.SetSize(Orientation.East, treeDescriptor);
                rootNode.SetLocation(Orientation.East, new Location(0, 0));
                               
                rootNode.ToLayout(layoutResult, GroundLevel + (float)(20 * layoutLevel), streetHeight);

                foreach (KeyValuePair<ILayoutNode, NodeTransform> keyValuePair in layoutResult)
                {
                    UnityEngine.Debug.Log($"Initial: {keyValuePair.Key},{keyValuePair.Value}\n");
                }
            }
            else
            {
                // Note: The ILayoutNodes in input parameter layoutNodes are not the same as the ILayoutNodes
                // in oldLayout.layoutResult. We will get always fresh ILayoutNodes in each call to this
                // method.
                foreach (ILayoutNode newerNode in layoutNodes)
                {
                    // Check whether the node is in the old layout. We can do that based on the ID.
                    // FIXME: This is a very inefficient way to find the node in the old layout.
                    // Every such lookup is O(n) in the number of nodes in the old layout.
                    ILayoutNode olderNode = oldLayout.layoutResult.Keys.FirstOrDefault(l => l.ID == newerNode.ID);
                    if (olderNode != null)
                    {
                        // An older node existed in the old layout, so we can re-use its position and scale.
                        // FIXME: We cannot use olderNode.AbsoluteScale, because newNodes has a different scale.
                        layoutResult[newerNode] = new NodeTransform(olderNode.CenterPosition, olderNode.AbsoluteScale);
                    }
                    else
                    {
                        // If the node is not in the old layout, create a new layout for it.
                        // FIXME: We need to find a suitable position and scale for the new node based on the
                        // old layout.
                        Debug.LogWarning($"Node {newerNode.ID} not found in the old layout. We just place it in the center.");
                        // FIXME: We are using layoutLevel for the y-coordinate. This is used only to let the
                        // new nodes stick out. This must be fixed.
                        layoutResult[newerNode] = new NodeTransform(new Vector3(0, layoutLevel, 0), newerNode.LocalScale);
                    }                    
                }
                // FIXME: We do not handle deleted nodes. The loop above only handles new and existing nodes.
            }
            return layoutResult;
        }

        /// <summary>
        /// Creates the ENode tree hierarchy starting at given root node. The root has
        /// depth 0.
        /// </summary>
        /// <param name="root">root of the hierarchy</param>
        /// <returns>root ENode</returns>
        private static ENode GenerateHierarchy(ILayoutNode root, int depth = 0)
        {
            ENode result = ENodeFactory.Create(root);
            result.TreeDepth = depth;
            if (result is EInner inner)
            {
                foreach (ILayoutNode child in root.Children())
                {
                    inner.AddChild(GenerateHierarchy(child, depth + 1));
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a <see cref="Node"/> for each <see cref="ILayoutNode"/>
        /// and sets the <see cref="Node.DesiredSize"/>.
        /// Fills the <see cref="nodeMap"/> and the <see cref="iLayoutNodeMap"/>.
        /// </summary>
        private void InitNodes()
        {
            float totalSize = Roots.Sum(InitNode);

            // adjust the absolute size to the rectangle of the layout
            float adjustFactor = (width * depth) / totalSize;
            foreach (Node node in nodeMap.Values)
            {
                node.DesiredSize *= adjustFactor;
                node.DesiredSize *= adjustFactor;
            }
        }

        /// <summary>
        /// Creates a <see cref="Node"/> for the given <see cref="ILayoutNode"/> <paramref name="node"/>
        /// and continues recursively with the children of the ILayoutNode <paramref name="node"/>.
        /// Extends both <see cref="nodeMap"/> and <see cref="iLayoutNodeMap"/> by the node.
        /// </summary>
        /// <param name="node">node of the layout</param>
        /// <returns>the absolute size of the node</returns>
        private float InitNode(ILayoutNode node)
        {
            Node newNode = new(node.ID);
            nodeMap.Add(node.ID, newNode);
            iLayoutNodeMap.Add(node.ID, node);

            if (node.IsLeaf)
            {
                // x and z lengths may differ; we need to consider the larger value
                float size = Mathf.Max(node.LocalScale.x, node.LocalScale.z);
                newNode.DesiredSize = size;
                return size;
            }
            else
            {
                float totalSize = node.Children().Sum(InitNode);
                newNode.DesiredSize = totalSize;
                return totalSize;
            }
        }

        /// <summary>
        /// Returns the width of the street for the root as a percentage <see cref="streetWidthPercentage"/>
        /// of the average of all widths and depths of leaf nodes in <paramref name="layoutNodes"/>.
        /// </summary>
        /// <param name="layoutNodes">the nodes to be laid out</param>
        /// <returns>width of street for the root</returns>
        private static float CalculateStreetWidth(IList<ILayoutNode> layoutNodes)
        {
            float result = 0;
            int numberOfLeaves = 0;
            foreach (ILayoutNode node in layoutNodes)
            {
                if (node.IsLeaf)
                {
                    numberOfLeaves++;
                    result += node.AbsoluteScale.x > node.AbsoluteScale.z ? node.AbsoluteScale.x : node.AbsoluteScale.z;
                }
            }
            UnityEngine.Assertions.Assert.IsTrue(numberOfLeaves > 0);
            result /= numberOfLeaves;
            // result is now the average length over all widths and depths of all leaf nodes.
            return result * streetWidthPercentage;
        }

        public override Dictionary<ILayoutNode, NodeTransform> Layout
            (ICollection<ILayoutNode> layoutNodes, ICollection<Edge> edges,
             ICollection<SublayoutLayoutNode> sublayouts)
        {
            // Must not be implemented because UsesEdgesAndSublayoutNodes() returns false
            // and this method should never be called.
            throw new NotImplementedException();
        }

        public override bool UsesEdgesAndSublayoutNodes()
        {
            return false;
        }
    }
}