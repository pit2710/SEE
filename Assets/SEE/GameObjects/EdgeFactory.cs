﻿using System.Collections.Generic;
using System.Linq;
using SEE.DataModel;
using SEE.Game;
using SEE.Layout;
using SEE.Layout.EdgeLayouts;
using System.Collections.Generic;
using Valve.VR.InteractionSystem;
using System.Linq;
using UnityEngine;
using SEE.Controls;
using SEE.Controls.Actions;

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
        /// <param name="tubularSegments">The amount of segments of the tubular</param>
        /// <param name="radius">The radius of the tubular</param>
        /// <param name="radialSegments">The amount of radial segments of the tubular</param>
        public EdgeFactory(IEdgeLayout layout, float edgeWidth, int tubularSegments, float radius, int radialSegments)
        {
            this.layout = layout;
            this.edgeWidth = edgeWidth;
            this.tubularSegments = tubularSegments;
            this.radius = radius;
            this.radialSegments = radialSegments;
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
        /// The amount of segments of the tubular.
        /// </summary>
        private readonly int tubularSegments;

        /// <summary>
        /// The radius of the tubular.
        /// </summary>
        private readonly float radius;

        /// <summary>
        /// The amount of radial segments of the tubular.
        /// </summary>
        private readonly int radialSegments;

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

                Points p  = gameEdge.AddComponent<Points>();
                p.controlPoints = layoutEdge.ControlPoints;
                p.linePoints = layoutEdge.Points;
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

                MeshCollider meshCollider = gameEdge.AddComponent<MeshCollider>();

                // Build tubular mesh with Curve
                bool closed = false; // closed curve or not
                Mesh mesh = Tubular.Tubular.Build(new Curve.CatmullRomCurve(layoutEdge.Points.OfType<Vector3>().ToList()), tubularSegments, radius, radialSegments, closed);

                // visualize mesh
                MeshFilter filter = gameEdge.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                meshCollider.sharedMesh = mesh;

                //gameEdge.AddComponent<InteractionDecorator>;
                gameEdge.AddComponent<Interactable>();
                gameEdge.AddComponent<InteractableObject>();
                gameEdge.AddComponent<ShowHovering>();
                //gameEdge.AddComponent<ShowLabel>();
                gameEdge.AddComponent<ShowSelection>();
                //gameEdge.AddComponent<ShowGrabbing>();
            }
            return result;
        }

        /// <summary>
        /// Creates and returns game objects for the given <paramref name="edges"/> among the given 
        /// <paramref name="nodes"/>. An edge is calculated as a line without drawing it.
        // The line is generated by the edge layouter provided in the constructor.
        /// </summary>
        /// <param name="nodes">source and target nodes of the <paramref name="edges"/></param>
        /// <param name="edges">the layout edges for which to create game objects</param>
        /// <returns>game objects representing the <paramref name="edges"/></returns>
         public ICollection<GameObject> CalculateNewEdges(ICollection<ILayoutNode> nodes, ICollection<LayoutEdge> edges)
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

                Points p  = gameEdge.AddComponent<Points>();
                p.controlPoints = layoutEdge.ControlPoints;
                p.linePoints = layoutEdge.Points;
            }
            return result;
        }
    }
}