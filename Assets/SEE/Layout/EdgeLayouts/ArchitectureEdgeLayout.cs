﻿using System.Collections.Generic;
using UnityEngine;

namespace SEE.Layout.EdgeLayouts
{
    /// <summary>
    /// Custom edge layout for the architecture graph.
    /// The layout creates plane two point lines between the center points of two nodes.
    /// </summary>
    public class ArchitectureEdgeLayout : IEdgeLayout
    {
        public ArchitectureEdgeLayout(bool edgesAboveBlocks, float minLevelDistance) : base(edgesAboveBlocks, minLevelDistance)
        {
            name = "Flat straight edge";
        }

        public override void Create(ICollection<ILayoutNode> nodes, ICollection<ILayoutEdge> edges)
        {
            MinMaxBlockY(nodes, out float minBlockY, out float maxBlockY, out float maxHeight);
            float edgeLevel = maxBlockY;
            foreach (ILayoutEdge edge in edges)
            {
                ILayoutNode source = edge.Source;
                ILayoutNode target = edge.Target;
                Vector3 start = source.Roof;
                Vector3 end = target.Roof;
                start.y = edgeLevel;
                end.y = edgeLevel;
                edge.Points = new Vector3[] {start, end};
            }
        }
    }
}