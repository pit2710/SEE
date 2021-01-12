﻿using System.Collections.Generic;
using System.Linq;
using SEE.DataModel;
using SEE.Game;
using SEE.Layout;
using SEE.Layout.EdgeLayouts;
using UnityEngine;

namespace SEE.GO
{
    /// <summary>
    /// A factory to create game objects for laid out edges.
    /// </summary>
    public class EdgeFactory
    {
        /// <summary>
        /// Constructor.
        /// Saves the <paramref name="layout"/> to be used and the requested <paramref name="edgeWidth"/>
        /// and creates the materials for the edges, but does not actually create any edges.
        /// </summary>
        /// <param name="layout">the edge layouter used to calculate the line for the edges</param>
        /// <param name="edgeWidth">the width of the line for the edges</param>
        public EdgeFactory(IEdgeLayout layout, float edgeWidth)
        {
            this.layout = layout;
            this.edgeWidth = edgeWidth;
            defaultLineMaterial = Materials.New(Materials.ShaderType.TransparentLine, Color.white);
        }

        /// <summary>
        /// Path to the material used for edges.
        /// </summary>
        private const string materialPath = "Hidden/Internal-Colored";

        /// <summary>
        /// The material used for edges.
        /// </summary>
        protected readonly Material defaultLineMaterial;

        /// <summary>
        /// The width of the line for the created edges, given in the constructor.
        /// </summary>
        private readonly float edgeWidth;

        /// <summary>
        /// The edge layouter used to generate the line for the edges, given in the constructor.
        /// </summary>
        private readonly IEdgeLayout layout;

        /// <summary>
        /// Returns a new game edge.
        /// </summary>
        /// <returns>new game edge</returns>
        private GameObject NewGameEdge(LayoutEdge layoutEdge)
        {
            GameObject gameEdge = new GameObject
            {
                tag = Tags.Edge,
                isStatic = false,
                name = layoutEdge.ItsEdge.ID
            };
            gameEdge.AddComponent<EdgeRef>().edge = layoutEdge.ItsEdge;
            return gameEdge;
        }

        /// <summary>
        /// Creates and returns game objects for the given <paramref name="edges"/> among the given 
        /// <paramref name="nodes"/>. An edge is drawn as a line using a LineRenderer (attached to 
        /// the resulting edges). The line is generated by the edge layouter provided in the
        /// constructor.
        /// </summary>
        /// <param name="nodes">source and target nodes of the <paramref name="edges"/></param>
        /// <param name="edges">the layout edges for which to create game objects</param>
        /// <returns>game objects representing the <paramref name="edges"/></returns>
        public ICollection<GameObject> DrawEdges(ICollection<ILayoutNode> nodes, ICollection<LayoutEdge> edges)
        {
            List<GameObject> result = new List<GameObject>(edges.Count);
            if (edges.Count == 0)
            {
                return result;
            }
            layout.Create(nodes, edges.Cast<ILayoutEdge>().ToList());
            foreach (LayoutEdge layoutEdge in edges)
            {
                GameObject gameEdge = NewGameEdge(layoutEdge);
                result.Add(gameEdge);

                // gameEdge does not yet have a renderer; we add a new one
                LineRenderer line = gameEdge.AddComponent<LineRenderer>();
                // use sharedMaterial if changes to the original material should affect all
                // objects using this material; renderer.material instead will create a copy
                // of the material and will not be affected by changes of the original material
                line.sharedMaterial = defaultLineMaterial;

                LineFactory.SetDefaults(line);
                LineFactory.SetWidth(line, edgeWidth);

                // If enabled, the lines are defined in world space.
                // This means the object's position is ignored, and the lines are rendered around 
                // world origin.
                line.useWorldSpace = false;

                Vector3[] points = layoutEdge.Points;
                line.positionCount = points.Length; // number of vertices
                line.SetPositions(points);

                // FIXME
                // put a capsule collider around the straight main line
                // (the one from points[1] to points[2]
                // FIXME: The following works only for straight lines with at least
                // three points, but a layoutEdge can have fewer lines and generally
                // is not a line in the first place. We need a better approach to
                // make edges selectable. For the time being, this code will be
                // disabled.
                //CapsuleCollider capsule = gameEdge.AddComponent<CapsuleCollider>();
                //capsule.radius = Math.Max(line.startWidth, line.endWidth) / 2.0f;
                //capsule.center = Vector3.zero;
                //capsule.direction = 2; // Z-axis for easier "LookAt" orientation
                //capsule.transform.position = points[1] + (points[2] - points[1]) / 2;
                //capsule.transform.LookAt(points[1]);
                //capsule.height = (points[2] - points[1]).magnitude;
            }
            return result;
        }
    }
}