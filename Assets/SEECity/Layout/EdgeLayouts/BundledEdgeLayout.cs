﻿using System.Collections.Generic;
using UnityEngine;
using System;

namespace SEE.Layout
{
    /// <summary>
    /// Draws edges as hierarchically bundled edges. 
    /// 
    /// See D. Holten, "Hierarchical Edge Bundles: Visualization of Adjacency Relations in Hierarchical Data" 
    /// in IEEE Transactions on Visualization and Computer Graphics, vol. 12, no. 5, pp. 741-748, Sept.-Oct. 2006.
    /// </summary>
    public class BundledEdgeLayout : IEdgeLayout
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="edgesAboveBlocks">if true, edges are drawn above nodes, otherwise below</param>
        /// <param name="tension">strength of the tension for bundling edges; must be in the range [0,1]</param>
        public BundledEdgeLayout(bool edgesAboveBlocks, float tension = 0.85f)
            : base(edgesAboveBlocks)
        {
            name = "Hierarchically Bundled";
            Debug.Assert(0.0f <= tension && tension <= 1.0f);
            this.tension = tension;
        }

        /// <summary>
        /// Determines the strength of the tension for bundling edges. This value may
        /// range from 0.0 (straight lines) to 1.0 (maximal bundling along the spline).
        /// </summary>
        private float tension = 0.85f; // 0.85 is the value recommended by Holten

        /// <summary>
        /// Returns hierarchically bundled splines for all edges among the given <paramref name="layoutNodes"/>.
        /// 
        /// </summary>
        /// <param name="layoutNodes">nodes whose connecting edges are to be drawn</param>
        /// <returns>hierarchically bundled splines</returns>
        public override ICollection<LayoutEdge> Create(ICollection<ILayoutNode> layoutNodes)
        {
            ICollection<LayoutEdge> layout = new List<LayoutEdge>();

            ICollection<ILayoutNode> roots = GetRoots(layoutNodes);
            maxLevel = GetMaxLevel(roots, -1);

            MinMaxBlockY(layoutNodes, out float minY, out float maxY, out float maxHeight);
            levelDistance = Math.Max(levelDistance, maxHeight / 5.0f);
            minimalDistanceToGround = (edgesAboveBlocks ? maxY : minY) + levelDistance;            

            LCAFinder<ILayoutNode> lca = new LCAFinder<ILayoutNode>(roots);

            foreach (ILayoutNode source in layoutNodes)
            {
                foreach (ILayoutNode target in source.Successors)
                {
                    layout.Add(new LayoutEdge(source, target, GetLinePoints(source, target, lca, maxLevel)));
                }
            }
            return layout;
        }

        /// <summary>
        /// The maximal level of the node hierarchy of the graph. The first level is 0.
        /// Thus, this value is greater or equal to zero. It is zero if we have only roots.
        /// </summary>
        private int maxLevel = 0;

        /// <summary>
        /// Returns the maximal tree level of the given <paramref name="nodes"/>, that is, the 
        /// longest path from a leaf to any node in <paramref name="nodes"/>.
        /// </summary>
        /// <param name="nodes">nodes whose maximal level is to be determined</param>
        /// <param name="currentLevel">the current level of all <paramref name="nodes"/></param>
        /// <returns>maximal tree level</returns>
        private int GetMaxLevel(ICollection<ILayoutNode> nodes, int currentLevel)
        {
            int max = currentLevel + 1;
            foreach (ILayoutNode node in nodes)
            {
                max = Math.Max(max, GetMaxLevel(node.Children(), currentLevel + 1));
            }
            return max;
        }

        /// <summary>
        /// Yields all nodes in <paramref name="layoutNodes"/> that do not have a parent,
        /// i.e., are root nodes.
        /// </summary>
        /// <param name="layoutNodes">list of nodes</param>
        /// <returns>all root nodes in <paramref name="layoutNodes"/></returns>
        private ICollection<ILayoutNode> GetRoots(ICollection<ILayoutNode> layoutNodes)
        {
            ICollection<ILayoutNode> result = new List<ILayoutNode>();
            foreach (ILayoutNode layoutNode in layoutNodes)
            {
                if (layoutNode.Parent == null)
                {
                    result.Add(layoutNode);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the path from <paramref name="child"/> to <paramref name="child"/> in the 
        /// node hierarchy including the child and the ancestor.
        /// Assertations on result: path[0] = child and path[path.Length-1] = ancestor.
        /// If child = ancestor, path[0] = child = path[path.Length-1] = ancestor.
        /// Precondition: child has ancestor.
        /// </summary>
        /// <param name="child">from where to start</param>
        /// <param name="ancestor">where to stop</param>
        /// <returns>path from child to ancestor in the node hierarchy</returns>
        private ILayoutNode[] Ancestors(ILayoutNode child, ILayoutNode ancestor)
        {
            int childLevel = child.Level;
            int ancestorLevel = ancestor.Level;
            // Note: roots have level 0, lower nodes have a level greater than 0;
            // thus, childLevel >= ancestorLevel

            // if ancestorLevel = childLevel, then path.Count = 1
            ILayoutNode[] path = new ILayoutNode[childLevel - ancestorLevel + 1];
            ILayoutNode cursor = child;
            int i = 0;
            while (true)
            {
                path[i] = cursor;
                if (cursor == ancestor)
                {
                    break;
                }
                cursor = cursor.Parent;
                i++;
            }
            return path;
        }

        /// <summary>
        /// Yields the list of points for a spline along the node hierarchy.
        /// If source equals target, a self loop is generated atop of the node.
        /// If source and target have no common LCA, the path starts at source
        /// and ends at target and reaches through the point on half distance between 
        /// these two nodes, but at the top-most edge height (given by maxLevel).
        /// If source and target are siblings (immediate ancestors of the same parent
        /// node), a direct spline is drawn between them.
        /// Otherwise, the control points of the splines are chosen along the node hierarchy 
        /// path from the source node to their lowest common ancestor and then down again 
        /// to the target node. The height of each such points is proportional to the level 
        /// of the node hierarchy. The higher the node in the hierarchy on this path,
        /// the higher the points.
        /// </summary>
        /// <param name="source">starting node</param>
        /// <param name="target">ending node</param>
        /// <param name="lcaFinder">to retrieve the lowest common ancestor of source and target</param>
        /// <param name="maxLevel">the maximal level of the node hierarchy</param>
        /// <returns>points to draw a spline between source and target</returns>
        private Vector3[] GetLinePoints
            (ILayoutNode source, 
             ILayoutNode target, 
             LCAFinder<ILayoutNode> lcaFinder, 
             int maxLevel)
        { 
            if (source == target)
            {
                return SelfLoop(source);
            }
            else
            {
                // Lowest common ancestor
                ILayoutNode lca = lcaFinder.LCA(source, target);
                if (lca == null)
                {
                    // This should never occur if we have a single root node, but may happen if
                    // there are multiple roots, in which case nodes in different trees of this
                    // forrest do not have a common ancestor.
                    Debug.LogWarning("Undefined lowest common ancestor for "
                        + source.LinkName + " and " + target.LinkName + "\n");
                    return BetweenTrees(source, target);
                }
                else if (lca == source || lca == target)
                {
                    // The edge is along a node hierarchy path.
                    // We will create a direct spline from source to target at the lowest level.
                    return DirectSpline(source, target, GetLevelHeight(maxLevel));
                }
                else
                {
                    // assert: sourceObject != targetObject
                    // assert: lcaObject != null
                    // because the edges are only between leaves:
                    // assert: sourceObject != lcaObject
                    // assert: targetObject != lcaObject

                    ILayoutNode[] sourceToLCA = Ancestors(source, lca);
                    ILayoutNode[] targetToLCA = Ancestors(target, lca);

                    Array.Reverse(targetToLCA, 0, targetToLCA.Length);

                    // Note: lca is included in both paths
                    if (sourceToLCA.Length == 2 && targetToLCA.Length == 2)
                    {
                        // source and target are siblings in the same subtree at the same level.
                        // We assume that those nodes are close to each other for all hierarchical layouts,
                        // which is true for EvoStreets, Balloon, TreeMap, and CirclePacking. If all edges 
                        // between siblings were led over one single control point, they would often take 
                        // a detour even though the nodes are close by. The detour would make it difficult 
                        // to follow the edges visually.
                        return DirectSpline(source, target, GetLevelHeight(maxLevel));
                    }
                    else
                    {
                        // Concatenate both paths.
                        // We have sufficient many control points without the duplicated LCA,
                        // hence, we can remove the duplicated LCA
                        ILayoutNode[] fullPath = new ILayoutNode[sourceToLCA.Length + targetToLCA.Length - 1];
                        sourceToLCA.CopyTo(fullPath, 0);
                        // copy without the first element
                        for (int i = 1; i < targetToLCA.Length; i++)
                        {
                            fullPath[sourceToLCA.Length + i - 1] = targetToLCA[i];
                        }
                        // Calculate control points along the node hierarchy 
                        Vector3[] controlPoints = new Vector3[fullPath.Length];
                        controlPoints[0] = edgesAboveBlocks ? source.Roof : source.Ground;
                        Vector3 direction = edgesAboveBlocks ? Vector3.up : Vector3.down;
                        for (int i = 1; i < fullPath.Length - 1; i++)
                        {
                            // We consider the height of intermediate nodes.
                            // Note that a root has level 0 and the level is increased along 
                            // the childrens' depth. That is why we need to choose the height
                            // as a measure relative to maxLevel.
                            // TODO: Do we really want the center position here?
                            controlPoints[i] = fullPath[i].CenterPosition
                                               + GetLevelHeight(fullPath[i].Level) * direction;
                        }
                        controlPoints[controlPoints.Length - 1] = edgesAboveBlocks ? target.Roof : target.Ground;
                        return LinePoints.BSplineLinePoints(controlPoints, tension);
                    }
                }
            }
        }

        /// <summary>
        /// Returns points of a direct spline for an edge from <paramref name="source"/> to <paramref name="target"/>.
        /// The first point is the center of the roof/ground of <paramref name="source"/> and the last
        /// point the center of the roof/ground of <paramref name="target"/>. The middle peak point
        /// is the position in between <paramref name="source"/> and <paramref name="target"/> where
        /// the y co-ordinate is specified by <paramref name="yLevel"/>. If edges should be drawn below
        /// blocks, we use the negative value of <paramref name="yLevel"/> for the y co-ordinate.
        /// 
        /// That means, an edge between the nodes is drawn as a direct spline on the shortest path 
        /// between the two nodes from roof/ground to roof/ground. Thus, no hierarchical bundling is applied.
        /// </summary>
        /// <param name="source">the node where to start the edge</param>
        /// <param name="target">the node where to end the edge</param>
        /// <param name="yLevel">the y co-ordinate of the two middle control points; must be positive</param>
        /// <returns>control points for a direct spline between the two nodes</returns>
        private Vector3[] DirectSpline(ILayoutNode source, ILayoutNode target, float yLevel)
        {
            Vector3 start = edgesAboveBlocks ? source.Roof : source.Ground;
            Vector3 end   = edgesAboveBlocks ? target.Roof : target.Ground;
            // position in between start and end
            Vector3 middle = Vector3.Lerp(start, end, 0.5f);
            middle.y = edgesAboveBlocks ? yLevel : -yLevel;
            return LinePoints.SplineLinePoints(start, middle, end);
        }

        /// <summary>
        /// The number of Unity units per level of the hierarchy for the height of control points.
        /// Its value must be greater than zero. It will be set relative to the maximal height
        /// of the nodes whose edge are to be laid out (in Create()). The value set here is the minimum value.
        /// </summary>
        private float levelDistance = 1.0f;

        /// <summary>
        /// The minimal y co-ordinate for all hierarchical control points at level 2 and above.
        /// Control points at level 0 (self loops) will be handled separately: self loops will be
        /// drawn from corner to corner on the roof or ground, respectively, depending upon 
        /// edgesAboveBlocks. Likewise, edges for nodes that are siblings in the hierarchy will be
        /// drawn as simple splines from roof to roof or ground to ground of the two blocks, respectively,
        /// on their shortest path. The y co-ordinate of inner control points of all other edges will be 
        /// at minimalDistanceToGround or above with respect to the ground. See GetLevelHeight() for 
        /// more details.
        /// Its value is never negative. It will be set in Create().
        /// </summary>
        private float minimalDistanceToGround = 0.0f;

        /// <summary>
        /// Returns the y co-ordinate for control points of nodes at the given <paramref name="level"/>
        /// of the node hierarchy. Root nodes are assumed to have level 0. There may be node hierarchies
        /// that are actually forrests rather than simple trees. In such cases, the lowest common ancestor
        /// of nodes in different trees does not exist and -1 will be passed as <paramref name="level"/>,
        /// which is perfectly acceptable. In such cases, the returned value will be just one levelDistance 
        /// above those for normal root nodes: 
        ///     minimalDistanceToGround + (maxLevel + 1) * levelDistance
        /// If level = maxLevel, minimalDistanceToGround will be returned.
        /// In all other cases the returned value is guaranteed to be greater than minimalDistanceToGround.
        /// </summary>
        /// <param name="level">node hierarchy level</param>
        /// <returns>y co-ordinate for control points; always >= 0</returns>
        private float GetLevelHeight(int level)
        {
            return minimalDistanceToGround + (maxLevel - level) * levelDistance;
        }

        /// <summary>
        /// Yields points of a spline for a self loop at a node. The first point
        /// is the front left corner of the roof/ground of <paramref name="node"/>
        /// and the last point is its opposite back right roof/ground corner. Thus, the 
        /// edge is diagonal across the roof/ground. The peak of the spline is in the
        /// middle of the start and end, where the y co-ordinate of that peak
        /// is levelDistance above the roof or below the ground, respectively.
        /// </summary>
        /// <param name="node">node whose self loop line points are required</param>
        /// <returns>line points forming a self loop above <paramref name="node"/></returns>
        private Vector3[] SelfLoop(ILayoutNode node)
        {
            // center area (roof or ground)
            Vector3 center = edgesAboveBlocks ? node.Roof : node.Ground;
            Vector3 extent = node.Scale / 2.0f;
            // left front corner of center area
            Vector3 start = new Vector3(center.x - extent.x, center.y, center.z - extent.z);
            // right back corner of center area
            Vector3 end = new Vector3(center.x + extent.x, center.y, center.z + extent.z);
            Vector3 middle = center;
            middle.y += edgesAboveBlocks? levelDistance : -levelDistance;
            return LinePoints.SplineLinePoints(start, middle, end);
        }

        /// <summary>
        /// Yields line points for two nodes that do not have a common ancestor in the
        /// node hierarchy. This may occur when we have multiple roots in the graph, that
        /// is, the node hierarchy is a forest and not just a single tree. In this case,
        /// we want the spline to reach above/below all other splines of nodes having a common
        /// ancestor. 
        /// The first and last points are the respective roofs/grounds of <paramref name="source"/>
        /// and <paramref name="target"/>. The peak point of the direct spline lies in between the 
        /// two nodes with respect to the x and z axis; its height (y axis) is the highest 
        /// hierarchical level, that is, one levelDistance above all other edges within the same 
        /// trees.
        /// </summary>
        /// <param name="source">start of edge (in one tree)</param>
        /// <param name="target">end of the edge (in another tree)</param>
        /// <returns>line points for two nodes without common ancestor</returns>
        private Vector3[] BetweenTrees(ILayoutNode source, ILayoutNode target)
        {
            return DirectSpline(source, target, GetLevelHeight(-1));
        }
    }
}
