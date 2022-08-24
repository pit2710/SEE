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
        /// Returns a new game edge for <paramref name="layoutGraphEdge"/>.
        /// </summary>
        /// <typeparam name="T">Type of node these edges connect to.</typeparam>
        /// <returns>new game edge</returns>
        private static GameObject NewGameEdge<T>(LayoutGraphEdge<T> layoutGraphEdge) where T : ILayoutNode
        {
            GameObject gameEdge = new GameObject
            {
                tag = Tags.Edge,
                isStatic = false,
                name = layoutGraphEdge.ItsEdge.ID
            };

            EdgeRef edgeRef = gameEdge.AddComponent<EdgeRef>();
            edgeRef.Value = layoutGraphEdge.ItsEdge;
            edgeRef.SourceNodeID = layoutGraphEdge.Source.ID;
            edgeRef.TargetNodeID = layoutGraphEdge.Source.ID;

            gameEdge.AddComponent<SEESpline>().Spline = layoutGraphEdge.Spline;

            GraphElementIDMap.Add(gameEdge);
            return gameEdge;
        }

        /// <summary>
        /// Creates and returns game objects for the given <paramref name="edges"/> among the given
        /// <paramref name="nodes"/>. An edge is drawn as a line using a LineRenderer (attached to
        /// the resulting edges). The line is generated by the edge layouter provided in the
        /// constructor.
        /// </summary>
        /// <typeparam name="T">Type of node these edges connect to.</typeparam>
        /// <param name="nodes">source and target nodes of the <paramref name="edges"/></param>
        /// <param name="edges">the layout edges for which to create game objects</param>
        /// <returns>game objects representing the <paramref name="edges"/></returns>
        public ICollection<GameObject> DrawEdges<T>(IEnumerable<T> nodes, ICollection<LayoutGraphEdge<T>> edges)
        where T : LayoutGameNode, IHierarchyNode<ILayoutNode>
        {
            List<GameObject> result = new List<GameObject>(edges.Count);
            if (edges.Count == 0)
            {
                return result;
            }

            layout.Create(nodes, edges);
            result.AddRange(edges.Select(NewGameEdgeWithLineRenderer));
            return result;
        }

        /// <summary>
        /// Creates and returns a game object representing <paramref name="layoutGraphEdge"/> where
        /// a <see cref="LineRenderer"/> has been added with default attribute values.
        /// the default
        /// </summary>
        /// <typeparam name="T">Type of node this edge connects to.</typeparam>
        /// <param name="layoutGraphEdge">the layout edge for which to create a game object</param>
        /// <returns>game object representing <paramref name="layoutGraphEdge"/></returns>
        private GameObject NewGameEdgeWithLineRenderer<T>(LayoutGraphEdge<T> layoutGraphEdge) where T : ILayoutNode
        {
            GameObject gameEdge = NewGameEdge(layoutGraphEdge);

            // Add a line renderer which serves as a preview in the Unity
            // editor. The line renderer will be replaced with a mesh
            // renderer at runtime (i.e., when starting the application).
            LineRenderer line = gameEdge.AddComponent<LineRenderer>();

            // Use sharedMaterial if changes to the original material
            // should affect all objects using this material;
            // renderer.material instead will create a copy of the
            // material and will not be affected by changes of the
            // original material.
            line.sharedMaterial = defaultLineMaterial;

            LineFactory.SetDefaults(line);
            LineFactory.SetWidth(line, edgeWidth);

            // If enabled, the lines are defined in world space. This
            // means the object's position is ignored and the lines are
            // rendered around world origin.
            line.useWorldSpace = false;

            // Draw spline as poly line.
            SEESpline spline = gameEdge.GetComponent<SEESpline>();
            Vector3[] positions = spline.PolyLine();
            line.positionCount = positions.Length; // number of vertices
            line.SetPositions(positions);

            return gameEdge;
        }
    }
}