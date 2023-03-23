using SEE.DataModel.DG;
using SEE.Layout.NodeLayouts.Cose;
using SEE.Layout.NodeLayouts.TreeMap;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Layout.NodeLayouts
{
    /// <summary>
    /// </summary>
    public class IncrementalTreemapLayout : HierarchicalNodeLayout
    {
        /// <summary>
        /// Constructor. The width and depth are assumed to be in Unity units.
        /// </summary>
        /// <param name="groundLevel">the y co-ordinate setting the ground level; all nodes will be
        /// placed on this level</param>
        /// <param name="width">width of the rectangle in which to place all nodes in Unity units</param>
        /// <param name="depth">width of the rectangle in which to place all nodes in Unity units</param>
        public IncrementalTreemapLayout(float groundLevel,
                             float width,
                             float depth)
        : base(groundLevel)
        {
            name = "IncrementalTreemap";
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
        private Dictionary<ILayoutNode, NodeTransform> layout_result;

        /// <summary>
        /// Return a treemap layout where are all nodes are fit into a given rectangle
        /// with the width and depth passed to the constructor. The width and depth of
        /// the original layout nodes will be scaled to fit into the rectangle, but
        /// the height will remain the same as in the input.
        /// </summary>
        /// <param name="layoutNodes">nodes to be laid out</param>
        /// <returns>treemap layout scaled in x and z axes</returns>
        public override Dictionary<ILayoutNode, NodeTransform> Layout(T, , int d, int c)
        {
            return layout_result;
        }

        /// <summary>
        /// Adds positioning and scales to <see cref="layout_result"/> for all root nodes (nodes with no parent)
        /// within a rectangle whose center position is Vector3.zero and whose width and depth is
        /// as specified by the constructor call. This function is then called recursively for the
        /// children of each root (until leaves are reached).
        /// </summary>
        private void CalculateLayout()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds positioning and scaling to layout_result for all given siblings (children of the same
        /// immediate parent in the node tree) within a rectangle with left front corner (x, z) and
        /// given width and depth. This function is then called recursively for the children of the
        /// given siblings.
        /// </summary>
        /// <param name="siblings">children of the same immediate parent in the node tree</param>
        /// <param name="x">x co-ordinate of the left front corner of the rectangle in which to place the nodes</param>
        /// <param name="z">z co-ordinate of the left front corner of the rectangle</param>
        /// <param name="width">width of the rectangle in which to fit the nodes</param>
        /// <param name="depth">depth of the rectangle in which to fit the nodes</param>
        private void CalculateLayout(ICollection<ILayoutNode> siblings, float x, float z, float width, float depth)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Some padding will be added between nodes. That padding depends upon the minimum
        /// of the width and depth of a node, multiplied by this factor.
        /// </summary>
        private const float PaddingFactor = 0.05f;

        /// <summary>
        /// The minimal padding between nodes in absolute (world space) terms.
        /// </summary>
        private const float MinimimalAbsolutePadding = 0.01f;

        /// <summary>
        /// The maximal padding between nodes in absolute (world space) terms.
        /// </summary>
        private const float MaximalAbsolutePadding = 0.1f;

        /// <summary>
        /// Returns the padding to be added between neighboring nodes (on a per node basis,
        /// i.e., the actual padding is the sum of the padding of two neighboring nodes).
        /// </summary>
        /// <param name="width">the width of the node</param>
        /// <param name="depth">the depth of the node</param>
        /// <returns>padding to be added</returns>
        private static float Padding(float width, float depth)
        {
            return Mathf.Clamp(Mathf.Min(width, depth) * PaddingFactor, MinimimalAbsolutePadding, MaximalAbsolutePadding);
        }

        /// <summary>
        /// Calculates the size of all nodes. The size of a leaf is the maximum of
        /// its width and depth. The size of an inner node is the sum of the sizes
        /// of all its children.
        ///
        /// The sizes of all <see cref="roots"/> and all their descendants are
        /// stored in <see cref="sizes"/>.
        /// </summary>
        /// <returns>total size of all node</returns>
        private float CalculateSize()
        {
            float total_size = 0.0f;
            foreach (ILayoutNode root in roots)
            {
                total_size += CalculateSize(root);
            }
            return total_size;
        }

        /// <summary>
        /// The size metric of each node. The area of the rectangle is proportional to a node's size.
        /// </summary>
        private readonly Dictionary<ILayoutNode, RectangleTiling.NodeSize> sizes
            = new Dictionary<ILayoutNode, RectangleTiling.NodeSize>();

        /// <summary>
        /// Calculates the size of node and all its descendants. The size of a leaf
        /// is the maximum of its width and depth. The size of an inner node is the
        /// sum of the sizes of all its children.
        ///
        /// The size of <see cref="node"/> and all its descendants is stored in <see cref="sizes"/>.
        /// </summary>
        /// <param name="node">node whose size it to be determined</param>
        /// <returns>size of <see cref="node"/></returns>
        private float CalculateSize(ILayoutNode node)
        {
            if (node.IsLeaf)
            {
                // a leaf
                Vector3 size = node.LocalScale;
                // x and z lengths may differ; we need to consider the larger value
                float result = Mathf.Max(size.x, size.z);
                sizes[node] = new RectangleTiling.NodeSize(node, result);
                return result;
            }
            else
            {
                float total_size = 0.0f;
                foreach (ILayoutNode child in node.Children())
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
        private List<RectangleTiling.NodeSize> GetSizes(ICollection<ILayoutNode> nodes)
        {
            List<RectangleTiling.NodeSize> result = new List<RectangleTiling.NodeSize>();
            foreach (ILayoutNode node in nodes)
            {
                result.Add(sizes[node]);
            }
            return result;
        }

        /// <summary>
        /// Adds the transforms (position, scale) of the game objects (nodes) according to their
        /// corresponding rectangle in rects to <see cref="layout_result"/>.
        ///
        /// The x and z co-ordinates for the resulting <see cref="NodeTransform"/> are determined
        /// by the rectangles, but the y co-ordinate is the original value of the input
        /// <see cref="ILayoutNode"/> (local scale).
        ///
        /// Precondition: For every i in the range of nodes: rects[i] is the transform
        /// corresponding to nodes[i].
        /// </summary>
        /// <param name="nodes">the game nodes</param>
        /// <param name="rects">their corresponding rectangle</param>
        private void AddToLayout
           (List<RectangleTiling.NodeSize> nodes,
            List<RectangleTiling.Rectangle> rects)
        {
            int i = 0;
            foreach (RectangleTiling.Rectangle rect in rects)
            {
                ILayoutNode o = nodes[i].gameNode;
                Vector3 position = new Vector3(rect.x + rect.width / 2.0f, groundLevel, rect.z + rect.depth / 2.0f);
                Vector3 scale = new Vector3(rect.width, o.LocalScale.y, rect.depth);
                Assert.AreEqual(o.AbsoluteScale, o.LocalScale, $"{o.ID}: {o.AbsoluteScale} != {o.LocalScale}");
                layout_result[o] = new NodeTransform(position, scale);
                i++;
            }
        }

        public override Dictionary<ILayoutNode, NodeTransform> Layout
            (ICollection<ILayoutNode> layoutNodes, ICollection<Edge> edges,
             ICollection<SublayoutLayoutNode> sublayouts)
        {
            throw new NotImplementedException();
        }

        public override bool UsesEdgesAndSublayoutNodes()
        {
            return false;
        }
    }
}
