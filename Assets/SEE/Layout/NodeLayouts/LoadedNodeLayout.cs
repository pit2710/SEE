﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SEE.DataModel.DG;
using SEE.Layout.IO;
using SEE.Layout.NodeLayouts.Cose;
using SEE.Utils;
using UnityEngine;

namespace SEE.Layout.NodeLayouts
{
    /// <summary>
    /// A layout that is read from a layout file.
    /// </summary>
    public class LoadedNodeLayout : HierarchicalNodeLayout
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="groundLevel">the y co-ordinate setting the ground level; all nodes will be
        /// placed on this level</param>
        /// <param name="filename">the name of file from which to read the layout information</param>
        public LoadedNodeLayout(float groundLevel, string filename)
          : base(groundLevel)
        {
            Name = "Loaded Layout";
            this.filename = filename;
        }

        /// <summary>
        /// The name of the layout file from which to read the layout information.
        /// </summary>
        private readonly string filename;

        /// <summary>
        /// See <see cref="HierarchicalNodeLayout.Layout()"/>.
        /// Note: The layout may not only be returned but also applied depending on the implementation
        /// of <see cref="ILayoutNode"/>.
        /// </summary>
        /// <param name="layoutNodes">nodes to be laid out</param>
        /// <returns>resulting layout</returns>
        public override Dictionary<ILayoutNode, NodeTransform> Layout(IEnumerable<ILayoutNode> layoutNodes)
        {
            Dictionary<ILayoutNode, NodeTransform> result = new();
            if (File.Exists(filename))
            {
                IList<ILayoutNode> layoutNodeList = layoutNodes.ToList();
                if (Filenames.HasExtension(filename, Filenames.GVLExtension))
                {
                    new GVLReader(filename, layoutNodeList.Cast<IGameNode>().ToList(), GroundLevel, new SEELogger());
                    // The elements in layoutNodeList will be stacked onto each other starting at groundLevel.
                }
                else if (Filenames.HasExtension(filename, Filenames.SLDExtension))
                {
                    SLDReader.Read(filename, layoutNodeList.Cast<IGameNode>().ToList());
                    // The elements in layoutNodeList will have the y position as it was saved in the file.
                }
                else
                {
                    throw new Exception($"Unknown layout file format for file extension of {filename}.");
                }

                // Apply the layout for the result.
                foreach (ILayoutNode node in layoutNodeList)
                {
                    Vector3 position = node.CenterPosition;
                    Vector3 absoluteScale = node.AbsoluteScale;
                    // Note: The node transform's y co-ordinate of the position is interpreted
                    // as the ground of the object. We need to adjust it accordingly.
                    position.y -= absoluteScale.y / 2.0f;
                    result[node] = new NodeTransform(position, absoluteScale);
                }
            }
            else
            {
                Debug.LogError($"Layout file {filename} does not exist. No layout could be loaded.\n");
            }
            return result;
        }

        public override Dictionary<ILayoutNode, NodeTransform> Layout(ICollection<ILayoutNode> layoutNodes, ICollection<Edge> edges, ICollection<SublayoutLayoutNode> sublayouts)
        {
            throw new NotImplementedException();
        }

        public override bool UsesEdgesAndSublayoutNodes()
        {
            return false;
        }
    }
}
