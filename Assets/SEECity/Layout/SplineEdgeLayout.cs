﻿using System.Collections.Generic;
using SEE.DataModel;
using SEE.GO;
using UnityEngine;

namespace SEE.Layout
{
    /// <summary>
    /// Draws edges as splines with three control points between either the roof or ground of
    /// game objects.
    /// </summary>
    public class SplineEdgeLayout : IEdgeLayout
    {
        public SplineEdgeLayout(NodeFactory blockFactory, float edgeWidth, bool edgesAboveBlocks) : base(blockFactory, edgeWidth, edgesAboveBlocks)
        {
        }

        public override ICollection<GameObject> DrawEdges(Graph graph, ICollection<GameObject> nodes)
        {
            List<GameObject> result = new List<GameObject>();

            Material edgeMaterial = new Material(defaultLineMaterial);
            if (edgeMaterial == null)
            {
                Debug.LogError("Could not find material " + materialPath + "\n");
                return result;
            }

            SetGameNodes(nodes);

            foreach (Edge edge in graph.ConnectingEdges(gameNodes.Keys))
            {
                Node sourceObject = edge.Source;
                Node targetObject = edge.Target;

                GameObject gameEdge = NewGameEdge(edge);

                Vector3 start = edgesAboveBlocks ? blockFactory.Roof(gameNodes[sourceObject])
                                                 : blockFactory.Ground(gameNodes[sourceObject]);
                Vector3 end = edgesAboveBlocks ? blockFactory.Roof(gameNodes[targetObject])
                                               : blockFactory.Ground(gameNodes[targetObject]);

                LineFactory.Draw(gameEdge, 
                                 LinePoints.SplineLinePoints(start, end, edgesAboveBlocks), 
                                 edgeWidth * blockFactory.Unit, 
                                 edgeMaterial);
                result.Add(gameEdge);
            }
            return result;
        }

        internal override ICollection<LayoutEdge> GetLines(ICollection<ILayoutNode> layoutNodes)
        {
            ICollection<LayoutEdge> layout = new List<LayoutEdge>();
            foreach (ILayoutNode source in layoutNodes)
            {
                foreach (ILayoutNode target in source.Successors)
                {
                    // define the points along the line
                    Vector3 start;
                    Vector3 end;
                    if (edgesAboveBlocks)
                    {
                        start = source.Roof;
                        end = target.Roof;
                    }
                    else
                    {
                        start = source.Ground;
                        end = target.Ground;
                    }
                    layout.Add(new LayoutEdge(source, target, LinePoints.SplineLinePoints(start, end, edgesAboveBlocks)));
                }
            }
            return layout;
        }
    }
}
