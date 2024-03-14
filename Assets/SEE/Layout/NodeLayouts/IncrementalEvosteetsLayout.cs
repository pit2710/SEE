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
            //List<Node> nodes = layoutNodes.Select(n => nodeMap[n.ID]).ToList();
            //// check if the old layout can be used to lay out siblings.


            IncrementalEvostreets.Rectangle rectangle = new IncrementalEvostreets.Rectangle(x: -width / 2.0f, z: -depth / 2.0f, width, depth);
         
            if (oldLayout == null)

            {
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
        /// Calculates the layout for <paramref name="siblings"/> so that they fit in <paramref name="rectangle"/>.
        /// Works recursively on the children of each sibling.
        /// Adds the actual layout to <see cref="layoutResult"/>
        /// </summary>
        /// <param name="siblings">nodes with same parent (or roots)</param>
        /// <param name="rectangle">area to place siblings</param>
        private void CalculateLayout(ICollection<ILayoutNode> siblings, IncrementalEvostreets.Rectangle rectangle) //
        {
          
            List<Node> nodes = siblings.Select(n => nodeMap[n.ID]).ToList();
            // check if the old layout can be used to lay out siblings.
            if (oldLayout == null
                || NumberOfOccurrencesInOldGraph(nodes) <= 4
                || ParentsInOldGraph(nodes).Count != 1)
            {
                // Dissect.Apply(nodes, rectangle);
           

            }
            else
            {
     

                  ApplyIncrementalLayout(nodes, rectangle);
            }

            AddToLayout(nodes);

            //foreach (ILayoutNode node in siblings)
            //{
            //    ICollection<ILayoutNode> children = node.Children();
            //    if (children.Count <= 0)
            //    {
            //        continue;
            //    }

            //    //EvoStreets.Rectangle childRectangle = nodeMap[node.ID].Rectangle;
            //    //CalculateLayout(children, childRectangle);
            //}
        }

        // <summary>
        // Calculates a stable layout for <paramref name = "nodes" />.
        // </ summary >
        // < param name="nodes">nodes to be laid out</param>
        // <param name = "rectangle" > rectangle in which the nodes should be laid out</param>
        private void ApplyIncrementalLayout(List<Node> nodes, IncrementalEvostreets.Rectangle rectangle)
        {
            // oldNodes are not only the siblings that are in the old graph and in the new one,
            // but all siblings in old graph. Note that there is exactly one single parent (because of the if-clause),
            // but this parent can be null if children == roots
            ILayoutNode oldILayoutParent = ParentsInOldGraph(nodes).First();
            ICollection<ILayoutNode> oldILayoutSiblings =
                oldILayoutParent == null ? oldLayout.Roots : oldILayoutParent.Children();
            List<Node> oldNodes = oldILayoutSiblings.Select(n => oldLayout.nodeMap[n.ID]).ToList();

            SetupNodeLists(nodes, oldNodes,
                out List<Node> workWith,
                out List<Node> nodesToBeDeleted,
                out List<Node> nodesToBeAdded);

          IncrementalEvostreets.Rectangle oldRectangle = IncrementalEvostreets.Utils.CreateParentRectangle(oldNodes);
            IncrementalEvostreets.Utils.TransformRectangles(workWith,
                oldRectangle: oldRectangle,
                newRectangle: rectangle);

            foreach (Node obsoleteNode in nodesToBeDeleted)
            {
                LocalMoves.DeleteNode(obsoleteNode);
                workWith.Remove(obsoleteNode);
            }

            CorrectAreas.Correct(workWith, settings);
            foreach (Node nodeToBeAdded in nodesToBeAdded)
            {
                LocalMoves.AddNode(workWith, nodeToBeAdded);
                workWith.Add(nodeToBeAdded);
            }

            CorrectAreas.Correct(workWith, settings);

            LocalMoves.LocalMovesSearch(workWith, settings);
        }

        /// <summary>
        /// Calculates 3 lists of <see cref="Node"/>s to transform stepwise the old layout
        /// to a new one.
        /// </summary>
        /// <param name="nodes">siblings from the current layout</param>
        /// <param name="oldNodes">corresponding siblings from the old layout</param>
        /// <param name="workWith">a copy of the old layout with nodes from the new one</param>
        /// <param name="nodesToBeDeleted">artificial nodes with no equivalent ILayoutNode, part of workWith</param>
        /// <param name="nodesToBeAdded">nodes that are not in workWith, but in nodes</param>
        private static void SetupNodeLists(
            List<Node> nodes,
            List<Node> oldNodes,
            out List<Node> workWith,
            out List<Node> nodesToBeDeleted,
            out List<Node> nodesToBeAdded)
        {
            //  [         oldNodes            ]--------------   <- nodes of OLD layout
            //  --------------[              nodes          ]   <- nodes of new layout
            //  [ toBeDeleted ]---------------[  toBeAdded  ]   <- nodes of new layout
            //  [         workWith            ]--------------   <- nodes of new layout
            //                                                     designed to be changed over time to nodes

            // get nodes from old layout and copy their rectangles
            // setup workWith and nodesToBeDeleted
            workWith = new List<Node>();
            nodesToBeDeleted = new List<Node>();
            foreach (Node oldNode in oldNodes)
            {
                Node newNode = nodes.Find(x => x.ID.Equals(oldNode.ID));
                if (newNode == null)
                {
                    // create an artificial node that has no corresponding ILayoutNode in this layout.
                    // they are designed to be deleted but it's necessary to copy the layout of oldNodes.
                    newNode = new Node(oldNode.ID);
                    nodesToBeDeleted.Add(newNode);
                }

                workWith.Add(newNode);
                newNode.Rectangle = oldNode.Rectangle.Clone();
            }

            IncrementalEvostreets.Utils.CloneSegments(
                from: oldNodes,
                to: workWith.ToDictionary(n => n.ID, n => n));

            List<Node> workWithAlias = workWith;
            nodesToBeAdded = nodes.Where(n => !workWithAlias.Contains(n)).ToList();
        }

        /// <summary>
        /// Returns a collection of all nodes of <see cref="oldLayout"/> that are parent to a node
        /// with the same id as a node in <paramref name="nodes"/>. The result will contain null
        /// if there is a root in the old layout with an equivalent node in <paramref name="nodes"/>
        /// </summary>
        /// <param name="nodes">nodes in this graph</param>
        /// <returns>Collection of parent nodes from <see cref="oldLayout"/>.</returns>
        private ICollection<ILayoutNode> ParentsInOldGraph(IEnumerable<Node> nodes)
        {
            Assert.IsNotNull(oldLayout);
            HashSet<ILayoutNode> parents = new();
            foreach (Node node in nodes)
            {
                if (oldLayout.iLayoutNodeMap.TryGetValue(node.ID, out ILayoutNode oldNode))
                {
                    parents.Add(oldNode.Parent);
                }
            }

            return parents;
        }

        /// <summary>
        /// The number of nodes that are also present in <see cref="oldLayout"/>.
        /// </summary>
        /// <param name="nodes">the nodes to look up</param>
        /// <returns>The number of occurrences in the last graph.</returns>
        private int NumberOfOccurrencesInOldGraph(IEnumerable<Node> nodes)
        {
            Assert.IsNotNull(oldLayout);
            return nodes.Count(n => oldLayout.iLayoutNodeMap.ContainsKey(n.ID));
        }

        /// <summary>
        /// Adds the result of the layout calculation to <see cref="layoutResult"/>.
        /// Applies padding to the result.
        /// </summary>
        /// <param name="nodes">nodes with calculated layout</param>
        private void AddToLayout(IEnumerable<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                float absolutePadding = settings.PaddingMm / 1000;
                IncrementalEvostreets.Rectangle rectangle = node.Rectangle;

                ILayoutNode layoutNode = iLayoutNodeMap[node.ID];

                if (rectangle.Width - absolutePadding <= 0 ||
                    rectangle.Depth - absolutePadding <= 0)
                {
                    absolutePadding = 0;
                }

                Vector3 position = new Vector3(
                    (float)(rectangle.X + rectangle.Width / 2.0d),
                    GroundLevel,
                    (float)(rectangle.Z + rectangle.Depth / 2.0d));
                Vector3 scale = new Vector3(
                    (float)(rectangle.Width - absolutePadding),
                    layoutNode.LocalScale.y,
                    (float)(rectangle.Depth - absolutePadding));



                layoutResult[layoutNode] = new NodeTransform(position, scale);
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