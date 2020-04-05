﻿using System.Collections.Generic;
using UnityEngine;
using System;

namespace SEE.Layout
{
    /// <summary>
    /// Draws edges as hierarchically bundled edges.
    /// </summary>
    public class BundledEdgeLayout : IEdgeLayout
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="edgesAboveBlocks">if true, edges are drawn above nodes, otherwise below</param>
        public BundledEdgeLayout(bool edgesAboveBlocks)
            : base(edgesAboveBlocks)
        {
            name = "Hierarchically Bundled";
        }

        /// <summary>
        /// The maximal level of the node hierarchy of the graph. The first level is 0.
        /// Thus, this value is greater or equal to zero. It is zero if we have only roots.
        /// </summary>
        private int maxLevel = 0;

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
        /// Yields the list of points for a spline along the node hierarchy.
        /// If source equals target, a self loop is generated atop of the node.
        /// If source and target have no common ancestor, the path starts at source
        /// and ends at target and reaches through the point on half distance between 
        /// these two nodes, but at the top-most edge height (given by maxDepth).
        /// If source and target are siblings (immediate ancestors of the same parent
        /// node), the control points are all on ground level froum source to target
        /// via their parent.
        /// Otherwise, the points are chosen along the path from the source
        /// node to their lowest common ancestor and then down again to the target 
        /// node. The height of each such points is proportional to the level 
        /// of the node hierarchy. The higher the node in the hierarchy on this path,
        /// the higher the points.
        /// </summary>
        /// <param name="source">starting node</param>
        /// <param name="target">ending node</param>
        /// <param name="lcaFinder">to retrieve the lowest common ancestor of source and target</param>
        /// <param name="maxLevel">the maximal depth of the node hierarchy</param>
        /// <returns>points to draw a spline between source and target</returns>
        private Vector3[] GetLinePoints(ILayoutNode source, ILayoutNode target, LCAFinder<ILayoutNode> lcaFinder, int maxLevel)
        { 
            if (source == target)
            {
                return LinePoints.BSplineLinePoints(SelfLoop(source));
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
                    Debug.LogError("Undefined lowest common ancestor for "
                        + source.LinkName + " and " + target.LinkName + "\n");
                    return BetweenTrees(source, target, maxLevel);
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
                    //Debug.Assert(sourceToLCA.Length > 1);
                    //Debug.Assert(sourceToLCA[0] == source);
                    //Debug.Assert(sourceToLCA[sourceToLCA.Length - 1] == lcaNode);

                    ILayoutNode[] targetToLCA = Ancestors(target, lca);
                    //Debug.Assert(targetToLCA.Length > 1);
                    //Debug.Assert(targetToLCA[0] == target);
                    //Debug.Assert(targetToLCA[targetToLCA.Length - 1] == lcaNode);

                    Array.Reverse(targetToLCA, 0, targetToLCA.Length);

                    // Note: lcaNode is included in both paths
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
                        //Debug.LogFormat("maxDepth = {0}\n", maxDepth);
                        // Concatenate both paths.
                        // We have sufficient many control points without the duplicated LCA,
                        // hence, we can remove the duplicate
                        ILayoutNode[] fullPath = new ILayoutNode[sourceToLCA.Length + targetToLCA.Length - 1];
                        sourceToLCA.CopyTo(fullPath, 0);
                        // copy without the first element
                        for (int i = 1; i < targetToLCA.Length; i++)
                        {
                            fullPath[sourceToLCA.Length + i - 1] = targetToLCA[i];
                        }
                        // Calculate control points along the node hierarchy 
                        Vector3[] controlPoints = new Vector3[fullPath.Length];
                        controlPoints[0] = source.Roof;
                        for (int i = 1; i < fullPath.Length - 1; i++)
                        {
                            // We consider the height of intermediate nodes.
                            // Note that a root has level 0 and the level is increased along 
                            // the childrens' depth. That is why we need to choose the height
                            // as a measure relative to maxDepth.
                            // TODO: Do we really want the center position here?
                            controlPoints[i] = fullPath[i].CenterPosition
                                               + GetLevelHeight(fullPath[i].Level) * Vector3.up;
                        }
                        controlPoints[controlPoints.Length - 1] = target.Roof;
                        return Layout.LinePoints.BSplineLinePoints(controlPoints);
                    }
                }
            }
        }

        /// <summary>
        /// Returns four control points for an edge from <paramref name="source"/> to <paramref name="target"/>.
        /// The first control point is the center of the roof of <paramref name="source"/> and the last
        /// control point the center of the roof of <paramref name="target"/>. The second and third control point
        /// is the position in between <paramref name="source"/> and <paramref name="target"/> where
        /// the y co-ordinate is specified by <paramref name="yLevel"/>. That means an edge between the nodes is drawn
        /// as a direct spline on the shortest path between the two nodes from roof to roof. Thus, no hierarchical
        /// bundling is applied.
        /// </summary>
        /// <param name="source">the object where to start the edge</param>
        /// <param name="target">the object where to end the edge</param>
        /// <param name="yLevel">the y co-ordinate of the two middle control points</param>
        /// <returns>control points for a direct spline between the two nodes</returns>
        private Vector3[] DirectSpline(ILayoutNode source, ILayoutNode target, float yLevel)
        {
            Vector3 start = source.Roof;
            Vector3 end = target.Roof;
            // position in between start and end
            Vector3 middle = Vector3.Lerp(start, end, 0.5f);
            middle.y = yLevel;
            return LinePoints.SplineLinePoints(start, middle, end);
        }

        /// <summary>
        /// Dumps given control points for debugging.
        /// </summary>
        /// <param name="controlPoints">control points to be emitted</param>
        private void Dump(Vector3[] controlPoints)
        {
            int i = 0;
            foreach (Vector3 cp in controlPoints)
            {
                Debug.LogFormat("controlpoint[{0}] = {1}\n", i, cp);
                i++;
            }
        }

        /// <summary>
        /// Returns the path from child to ancestor in the tree including
        /// the child and the ancestor.
        /// Assertations on result: path[0] = child and path[path.Length-1] = ancestor.
        /// If child = ancestor, path[0] = child = path[path.Length-1] = ancestor.
        /// Precondition: child has ancestor.
        /// </summary>
        /// <param name="child">from where to start</param>
        /// <param name="ancestor">where to stop</param>
        /// <returns>path from child to ancestor in the tree</returns>
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
        /// The number of Unity units per level of the hierarchy for the height of control points.
        /// Its value must be greater than zero.
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
        /// which is perfectly acceptable. In such cases, the returned value will be just one 
        /// levelDistance above those for normal root nodes: 
        ///     minimalDistanceToGround + (maxLevel + 1) * levelDistance
        /// If level == maxLevel, minimalDistanceToGround will be returned.
        /// In all other cases the returned value is guaranteed to be greater than minimalDistanceToGround.
        /// </summary>
        /// <param name="level">node hierarchy level</param>
        /// <returns>y co-ordinate for control points; always >= 0</returns>
        private float GetLevelHeight(int level)
        {
            return minimalDistanceToGround + (maxLevel - level) * levelDistance;
        }

        /// <summary>
        /// Yields points of a spline for a self loop at a node. The first  
        /// point is the front left corner of the roof of <paramref name="node"/>
        /// and the last point is its opposite back right roof corner. Thus, the 
        /// edge is diagonal across the roof. The peak of the spline is in the
        /// middle of the begin and end where the y co-ordinate of that peak
        /// is levelDistance above the roof or ground, respectively.
        /// </summary>
        /// <param name="node">node whose self loop control points are required</param>
        /// <returns>control points forming a self loop above the node</returns>
        private Vector3[] SelfLoop(ILayoutNode node)
        {
            Vector3 roofCenter = node.Roof;
            Vector3 extent = node.Scale / 2.0f;
            Vector3 start = new Vector3(roofCenter.x - extent.x, roofCenter.y, roofCenter.z - extent.z);
            Vector3 end = new Vector3(roofCenter.x + extent.x, roofCenter.y, roofCenter.z + extent.z);
            Vector3 middle = roofCenter;
            middle.y += levelDistance;
            return LinePoints.SplineLinePoints(start, middle, end);
        }

        /// <summary>
        /// Yields line points for two nodes that do not have a common ancestor in the
        /// node hierarchy. This may occur when we have multiple roots in the graph, that
        /// is, the node hierarchy is a forrest and not just a single tree. In this case,
        /// we want the spline to reach above all other splines of nodes having a common
        /// ancestor. 
        /// The first and last points are the respective roofs/grounds of source and target
        /// node. The peak point of the direct spline lies in between the two nodes with 
        /// respect to the x and z axis; its height (y axis) is the highest hierarchical level, 
        /// that is, one levelDistance above all other edges within the same trees.
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
