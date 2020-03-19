﻿using SEE.DataModel;
using SEE.GO;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Layout
{
    /// <summary>
    /// Yields a squarified treemap node layout according to the algorithm 
    /// described by Bruls, Huizing, van Wijk, "Squarified Treemaps".
    /// pp. 33-42, Eurographics / IEEE VGTC Symposium on Visualization, 2000.
    /// </summary>
    public class TreemapLayout : HierarchicalNodeLayout
    {
        /// <summary>
        /// Constructor. The width and depth are assumed to be in Unity units.
        /// </summary>
        /// <param name="groundLevel">the y co-ordinate setting the ground level; all nodes will be
        /// placed on this level</param>
        /// <param name="leafNodeFactory">the factory used to create leaf nodes</param>
        /// <param name="width">width of the rectangle in which to place all nodes in Unity units</param>
        /// <param name="depth">width of the rectangle in which to place all nodes in Unity units</param>
        public TreemapLayout(float groundLevel,
                             NodeFactory leafNodeFactory,
                             float width,
                             float depth)
        : base(groundLevel, leafNodeFactory)
        {
            name = "Treemap";
            this.width = width;
            this.depth = depth;
        }

        /// <summary>
        /// The width of the rectangle in which to place all nodes in Unity units.
        /// </summary>
        private readonly float width;

        /// <summary>
        /// The depth of the rectangle in which to place all nodes in Unity units.
        /// </summary>
        private readonly float depth;

        /// <summary>
        /// The node layout we compute as a result.
        /// </summary>
        private Dictionary<LayoutNode, NodeTransform> layout_result;

        /// <summary>
        /// The roots of the subtrees of the original graph that are to be laid out.
        /// A node is considered a root if it has either no parent in the original
        /// graph or its parent is not contained in the set of nodes to be laid out.
        /// </summary>
        private IList<LayoutNode> roots;

        public override Dictionary<GameObject, NodeTransform> Layout(ICollection<GameObject> gameNodes)
        {
            to_game_node = NodeMapping(gameNodes);
            return ToNodeTransformLayout(Layout(ToLayoutNodes(gameNodes)));
        }

        private Dictionary<LayoutNode, NodeTransform> Layout(ICollection<LayoutNode> layoutNodes)
        {
            layout_result = new Dictionary<LayoutNode, NodeTransform>();

            if (layoutNodes.Count == 0)
            {
                throw new Exception("No nodes to be laid out.");
            }
            else if (layoutNodes.Count == 1)
            {
                LayoutNode gameNode = layoutNodes.GetEnumerator().Current;
                layout_result[gameNode] = new NodeTransform(Vector3.zero, 
                                                            new Vector3(width, gameNode.GetSize().y, depth));
            }
            else
            {
                //CreateTree(to_game_node.Keys, out roots, out children);
                roots = GetRoots(layoutNodes);
                CalculateSize();
                CalculateLayout();
            }
            return layout_result;
        }

        /// <summary>
        /// Adds positioning and scaling to layout_result for all root nodes (nodes with no parent)
        /// within a rectangle whose center position is Vector3.zero and whose width and depth is 
        /// as specified by the constructor call. This function is then called recursively for the 
        /// children of each root (until leaves are reached).
        /// </summary>
        private void CalculateLayout()
        {
            if (roots.Count == 1)
            {
                LayoutNode root = roots[0];
                layout_result[root] = new NodeTransform(Vector3.zero, 
                                                        new Vector3(width, root.GetSize().y, depth));
                CalculateLayout(root.Children(), -width / 2.0f, -depth / 2.0f, width, depth);
            }
            else
            {
                CalculateLayout(roots, -width / 2.0f, -depth / 2.0f, width, depth);
            }
        }

        /// <summary>
        /// Adds positioning and scaling to layout_result for all given siblings (children of the same
        /// immediate parent in the node tree) within a rectangle with left front corner (x, z) and
        /// given width and depth. This function is then called recursively for the children of the
        /// given siblings.
        /// </summary>
        /// <param name="siblings">hildren of the same immediate parent in the node tree</param>
        /// <param name="x">x co-ordinate of the left front corner of the rectangle in which to place the nodes</param>
        /// <param name="z">z co-ordinate of the left front corner of the rectangle</param>
        /// <param name="width">width of the rectangle</param>
        /// <param name="depth">depth of the rectangle</param>
        private void CalculateLayout(ICollection<LayoutNode> siblings, float x, float z, float width, float depth)
        {
            List<RectangleTiling.NodeSize> sizes = GetSizes(siblings);
            float padding = Mathf.Min(width, depth) * 0.01f;
            List<RectangleTiling.Rectangle> rects = RectangleTiling.Squarified_Layout_With_Padding(sizes, x, z, width, depth, padding);
            Add_To_Layout(sizes, rects);

            foreach (LayoutNode node in siblings)
            {
                ICollection<LayoutNode> kids = node.Children();
                if (kids.Count > 0)
                {
                    // Note: nodeTransform.position is the center position, while 
                    // CalculateLayout assumes co-ordinates x and z as the left front corner
                    NodeTransform nodeTransform = layout_result[node];
                    CalculateLayout(kids, 
                                    nodeTransform.position.x - nodeTransform.scale.x / 2.0f, 
                                    nodeTransform.position.z - nodeTransform.scale.z / 2.0f,
                                    nodeTransform.scale.x, 
                                    nodeTransform.scale.z);
                }
            }
        }

        /// <summary>
        /// Calculates the size of all nodes. The size of a leaf is the maximum of 
        /// its width and depth. The size of an inner node is the sum of the sizes 
        /// of all its children.
        /// </summary>
        /// <returns>total size of all node</returns>
        private float CalculateSize()
        {
            float total_size = 0.0f;
            foreach(LayoutNode root in roots)
            {
                total_size += CalculateSize(root);
            }
            return total_size;
        }

        /// <summary>
        /// The size metric of each node. The area of the rectangle is proportional to a node's size.
        /// </summary>
        private Dictionary<LayoutNode, RectangleTiling.NodeSize> sizes = new Dictionary<LayoutNode, RectangleTiling.NodeSize>();

        /// <summary>
        /// Calculates the size of node and all its descendants. The size of a leaf
        /// is the maximum of its width and depth. The size of an inner node is the
        /// sum of the sizes of all its children.
        /// </summary>
        /// <param name="node">node whose size it to be determined</param>
        /// <returns>size of node</returns>
        private float CalculateSize(LayoutNode node)
        {
            if (node.IsLeaf())
            {
                // a leaf      
                Vector3 size = node.GetSize();
                // x and z lenghts may differ; we need to consider the larger value
                float result = Mathf.Max(size.x, size.z);
                sizes[node] = new RectangleTiling.NodeSize(node, result);
                return result;
            }
            else
            {
                float total_size = 0.0f;
                foreach (LayoutNode child in node.Children())
                {
                    total_size += CalculateSize(child);
                }
                sizes[node] = new RectangleTiling.NodeSize(node, total_size);
                return total_size;
            }
        }

        /// <summary>
        /// Returns the list of node area sizes; one for each node in nodes as
        /// defined in sizes.
        /// </summary>
        /// <param name="nodes">list of nodes whose sizes are to be determined</param>
        /// <returns>list of node area sizes</returns>
        private List<RectangleTiling.NodeSize> GetSizes(ICollection<LayoutNode> nodes)
        {
            List<RectangleTiling.NodeSize> result = new List<RectangleTiling.NodeSize>();
            foreach (LayoutNode node in nodes)
            {
                result.Add(sizes[node]);
            }
            return result;
        }

        /// <summary>
        /// Adds the transforms (position, scale) of the game objects (nodes) according to their
        /// corresponding rectangle in rects to layout_result. 
        /// 
        /// Precondition: For every i in the range of nodes: rects[i] is the transform
        /// corresponding to nodes[i].
        /// </summary>
        /// <param name="nodes">the game nodes</param>
        /// <param name="rects">their corresponding rectangle</param>
        private void Add_To_Layout
           (List<RectangleTiling.NodeSize> nodes,
            List<RectangleTiling.Rectangle> rects)
        {
            int i = 0;
            foreach (RectangleTiling.Rectangle rect in rects)
            {
                LayoutNode o = nodes[i].gameNode;
                Vector3 position = new Vector3(rect.x + rect.width / 2.0f, groundLevel, rect.z + rect.depth / 2.0f);
                Vector3 scale = new Vector3(rect.width, o.GetSize().y, rect.depth);
                layout_result[o] = new NodeTransform(position, scale);
                i++;
            }
        }
    }
}
