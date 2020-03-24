﻿using System.Collections.Generic;
using UnityEngine;

namespace SEE.Layout
{
    /// <summary>
    ///  Defines the methods for all nodes to be laid out.
    /// </summary>
    public interface ILayoutNode
    {
        /// <summary>
        /// The parent of the node. Is null if the node is a root.
        /// </summary>
        ILayoutNode Parent { get; }

        /// <summary>
        /// The level of the node in the node hierarchy. A root node has
        /// level 0. For all other nodes, the level is the distance from
        /// the node to its root.
        /// </summary>
        int Level { get; set; }

        /// <summary>
        /// True if the given node is to be interpreted as a leaf by the layouter.
        /// 
        /// Note: Even leaves may have children. What to do with those is the decision of the
        /// layouter. It may or may not lay them out.
        /// </summary>
        bool IsLeaf { get; }

        /// <summary>
        /// A unique ID for a node.
        /// </summary>
        string LinkName { get; }

        /// <summary>
        /// Scale of a node.
        /// </summary>
        Vector3 Scale { get; set; }

        /// <summary>
        /// Center position of a node in world space.
        /// </summary>
        Vector3 CenterPosition { get; set; }

        /// <summary>
        /// Rotation around the y axis in degrees.
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// X-Z center position of the roof of the node in world space.
        /// </summary>
        Vector3 Roof { get; }

        /// <summary>
        /// X-Z center position of the ground of the node in world space.
        /// </summary>
        Vector3 Ground { get; }

        /// <summary>
        /// The set of immediate successors of this node.
        /// </summary>
        ICollection<ILayoutNode> Successors { get; }

        /// <summary>
        /// The set of children of this node. Note: Even nodes for which IsLeaf
        /// returns true, may still have children. Layouts may refuse to layout
        /// the children of a node for which IsLeaf returns true.
        /// </summary>
        /// <returns>children of this node</returns>
        IList<ILayoutNode> Children();
    }
}