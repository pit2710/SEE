﻿using System.Collections.Generic;
using UnityEngine;

namespace SEE.Layout
{
    /// <summary>
    /// Draws edges as splines with three control points between either the roof or ground of
    /// game objects.
    /// </summary>
    public class SplineEdgeLayout : IEdgeLayout
    {
        /// <summary>
        /// Constructor.
        /// 
        /// Parameter <paramref name="rdp"/> specifies the extent the polylines of the generated
        /// splines are simplified. Neighboring line points whose distances fall below 
        /// <paramref name="rdp"/> (with respect to the line drawn between their neighbors) will 
        /// be removed. The greater the value is, the more aggressively points are removed 
        /// (note: values greater than one are fine). A positive value close to zero results 
        /// in a line with little to no reduction. A negative value is treated as 0. A value 
        /// of zero has no effect.
        /// </summary>
        /// <param name="edgesAboveBlocks">if true, edges are drawn above nodes, otherwise below</param>
        /// <param name="minLevelDistance">the minimal distance between different edge levels</param>
        /// <param name="rdp">epsilon parameter of the Ramer–Douglas–Peucker algorithm</param>
        public SplineEdgeLayout(bool edgesAboveBlocks, float minLevelDistance, float rdp = 0.0f) 
            : base(edgesAboveBlocks, minLevelDistance)
        {
            name = "Splines";
            this.rdp = rdp;
        }

        /// <summary>
        /// Determines to which extent the polylines of the generated splines are simplified.
        /// </summary>
        private float rdp = 0.0f; // 0.0f means no simplification

        public override ICollection<ILayoutEdge> Create(ICollection<ILayoutNode> layoutNodes)
        {
            ICollection<ILayoutEdge> layout = new List<ILayoutEdge>();
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
                    layout.Add(new ILayoutEdge(source, target,
                               Simplify(LinePoints.SplineLinePoints(start, end, edgesAboveBlocks, minLevelDistance), rdp)));
                }
            }
            return layout;
        }
    }
}
