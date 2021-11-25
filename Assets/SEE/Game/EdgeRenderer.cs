﻿using SEE.DataModel.DG;
using SEE.Game.City;
using SEE.Layout;
using SEE.GO;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using SEE.Layout.EdgeLayouts;
using SEE.Utils;
using SEE.DataModel;

namespace SEE.Game
{
    /// <summary>
    /// Implements the functions of the <see cref="GraphRenderer"/> related to nodes.
    /// </summary>
    public partial class GraphRenderer
    {
        /// <summary>
        /// Creates and returns a new edge between <paramref name="from"/> and <paramref name="to"/>
        /// based on the current settings. A new edge will be added to the underlying graph, too.
        ///
        /// Note: A default edge layout will be used if no edge layout was chosen.
        ///
        /// Precondition: <paramref name="from"/> and <paramref name="to"/> must have a valid
        /// node reference. The corresponding graph nodes must be in the same graph.
        /// </summary>
        /// <param name="from">source of the new edge</param>
        /// <param name="to">target of the new edge</param>
        /// <param name="id">id of the new edge. If it is null or empty, a new id will be generated</param>
        /// <returns>the new edge</returns>
        /// <exception cref="Exception">thrown if <paramref name="from"/> or <paramref name="to"/>
        /// are not contained in any graph or contained in different graphs</exception>
        public GameObject DrawEdge(GameObject from, GameObject to, string id)
        {
            Node fromNode = from.GetNode();
            if (fromNode == null)
            {
                throw new Exception($"The source {from.name} of the edge is not contained in any graph.");
            }
            Node toNode = to.GetNode();
            if (toNode == null)
            {
                throw new Exception($"The target {to.name} of the edge is not contained in any graph.");
            }
            if (fromNode.ItsGraph != toNode.ItsGraph)
            {
                throw new Exception($"The source {from.name} and target {to.name} of the edge are in different graphs.");
            }

            // Creating the edge in the underlying graph
            Edge edge = string.IsNullOrEmpty(id) ? new Edge() : new Edge(id);
            edge.Source = fromNode;
            edge.Target = toNode;
            edge.Type = Graph.UnknownType; // FIXME: We need to set the type of the edge.

            Graph graph = fromNode.ItsGraph;
            graph.AddEdge(edge);
            // Save edge layout so that we can restore it if we need to select a default layout.
            EdgeLayoutKind savedEdgeLayout = settings.EdgeLayoutSettings.Kind;
            if (savedEdgeLayout == EdgeLayoutKind.None)
            {
                Debug.LogWarning($"An edge {edge.ID} from {fromNode.ID} to {toNode.ID} was added to the graph, but no edge layout was chosen.\n");
                // Select default layout
                settings.EdgeLayoutSettings.Kind = EdgeLayoutKind.Spline;
            }

            // Creating the game object representing the edge.
            // The edge layout will be calculated for the following gameNodes. This list will
            // contain the source and target of the edge but also all their ascendants. The
            // ascendants are needed for hierarchical layouts.
            HashSet<GameObject> gameNodes = new HashSet<GameObject>();
            // We add the descendants of the source and target nodes in case the edge layout is hierarchical.
            AddAscendants(from, gameNodes);
            AddAscendants(to, gameNodes);
            Dictionary<Node, ILayoutNode> to_layout_node = new Dictionary<Node, ILayoutNode>();
            // The layout nodes corresponding to those game nodes.
            ICollection<LayoutGameNode> layoutNodes = ToLayoutNodes(gameNodes, leafNodeFactory, innerNodeFactory, to_layout_node);

            LayoutGameNode fromLayoutNode = null; // layout node in layoutNodes corresponding to source node
            LayoutGameNode toLayoutNode = null;   // layout node in layoutNodes corresponding to target node
            // We need fromLayoutNode and toLayoutNode to create a single layout edge to be passed
            // to the edge layouter.
            foreach (LayoutGameNode layoutNode in layoutNodes)
            {
                //TODO: Should this be a ReferenceEquals() or Equals() comparison?
                if (layoutNode.ItsNode == fromNode)
                {
                    fromLayoutNode = layoutNode;
                }
                // note: fromNode = toNode is possible, hence, there is no 'else' here.
                if (layoutNode.ItsNode == toNode)
                {
                    toLayoutNode = layoutNode;
                }
            }
            Assert.IsNotNull(fromLayoutNode, $"source node {fromNode.ID} does not have a layout node.\n");
            Assert.IsNotNull(toLayoutNode, $"target node {toNode.ID} does not have a layout node.\n");
            // The single layout edge between source and target. We want the layout only for this edge.
            ICollection<LayoutEdge> layoutEdges = new List<LayoutEdge> { new LayoutEdge(fromLayoutNode, toLayoutNode, edge) };
            // Calculate the edge layout (for the single edge only).
            ICollection<GameObject> edges = EdgeLayout(layoutNodes, layoutEdges);
            GameObject resultingEdge = edges.FirstOrDefault();
            InteractionDecorator.PrepareForInteraction(resultingEdge);
            // The edge becomes a child of the root node of the game-node hierarchy
            GameObject codeCity = SceneQueries.GetCodeCity(from.transform).gameObject;
            GameObject rootNode = SceneQueries.GetCityRootNode(codeCity).gameObject;
            resultingEdge.transform.SetParent(rootNode.transform);
            // The portal of the new edge is inherited from the codeCity.
            Portal.SetPortal(root: codeCity, gameObject: resultingEdge);
            // Reset original edge layout.
            settings.EdgeLayoutSettings.Kind = savedEdgeLayout;
            return resultingEdge;
        }

        /// <summary>
        /// Adds <paramref name="node"/> and all its transitive parent game objects tagged by
        /// Tags.Node to <paramref name="gameNodes"/>.
        /// </summary>
        /// <param name="node">the game objects whose ascendant game nodes are to be added to <paramref name="gameNodes"/></param>
        /// <param name="gameNodes">where to add the ascendants</param>
        private static void AddAscendants(GameObject node, HashSet<GameObject> gameNodes)
        {
            GameObject cursor = node;
            while (cursor != null && cursor.CompareTag(Tags.Node))
            {
                gameNodes.Add(cursor);
                cursor = cursor.transform.parent.gameObject;
            }
        }

        /// <summary>
        /// Applies the edge layout according to the user's choice (settings) for
        /// all edges in between nodes in <paramref name="gameNodes"/>. The resulting
        /// edges are added to <paramref name="parent"/> as children.
        /// </summary>
        /// <param name="gameNodes">the subset of nodes for which to draw the edges</param>
        /// <param name="parent">the object the new edges are to become children of</param>
        /// <param name="draw">Decides whether the edges should only be calculated, or whether they should also be drawn.</param>
        /// <returns>all game objects created to represent the edges; may be empty</returns>
        public ICollection<GameObject> EdgeLayout(ICollection<GameObject> gameNodes, GameObject parent, bool draw = true)
        {
            return EdgeLayout(ToLayoutNodes(gameNodes), parent, draw);
        }

        /// <summary>
        /// Applies the edge layout according to the the user's choice (settings) for
        /// all edges in between nodes in <paramref name="gameNodes"/>. The resulting
        /// edges are added to <paramref name="parent"/> as children.
        /// </summary>
        /// <param name="gameNodes">the subset of nodes for which to draw the edges</param>
        /// <param name="parent">the object the new edges are to become children of</param>
        /// <param name="draw">decides whether the edges should only be calculated, or whether they should also be drawn.</param>
        /// <returns>all game objects created to represent the edges; may be empty</returns>
        private ICollection<GameObject> EdgeLayout(ICollection<LayoutGameNode> gameNodes, GameObject parent, bool draw = true)
        {
            ICollection<GameObject> result = EdgeLayout(gameNodes, ConnectingEdges(gameNodes.Cast<AbstractLayoutNode>().ToList()), draw);
            AddToParent(result, parent);
            return result;
        }

        /// <summary>
        /// Returns the connecting edges among <paramref name="layoutNodes"/> laid out by the
        /// selected edge layout.
        /// If <paramref name="layoutNodes"/> is null or empty or if no layout was selected
        /// by the user, the empty collection is returned.
        /// </summary>
        /// <param name="layoutNodes">nodes whose connecting edges are to be laid out</param>
        /// <returns>laid out edges</returns>
        public ICollection<LayoutEdge> LayoutEdges(ICollection<ILayoutNode> layoutNodes)
        {
            if (layoutNodes == null || layoutNodes.Count == 0)
            {
                // no nodes, no edges, no layout
                return new List<LayoutEdge>();
            }
            IEdgeLayout layout = GetEdgeLayout();
            if (layout == null)
            {
                // No layout selected, no edges will be created.
                return new List<LayoutEdge>();
            }
            else
            {
                ICollection<LayoutEdge> edges = ConnectingEdges(layoutNodes.Cast<AbstractLayoutNode>().ToList());
                layout.Create(layoutNodes, edges.Cast<ILayoutEdge>().ToList());
                return edges;
            }
        }

        /// <summary>
        /// Applies the edge layout according to the the user's choice (settings).
        /// </summary>
        /// <param name="gameNodes">the set of layout nodes for which to create game edges</param>
        /// <param name="layoutEdges">the edges to be laid out</param>
        /// <param name="draw">Decides whether the edges should only be calculated, or whether they should also be drawn.</param>
        /// <returns>all game objects created to represent the edges; may be empty</returns>
        private ICollection<GameObject> EdgeLayout(ICollection<LayoutGameNode> gameNodes, ICollection<LayoutEdge> layoutEdges, bool draw = true)
        {
            IEdgeLayout layout = GetEdgeLayout();
            if (layout == null)
            {
                // No layout selected, no edges will be created.
                return new List<GameObject>();
            }
#if UNITY_EDITOR
            Performance p = Performance.Begin("edge layout " + layout.Name);
#endif
            EdgeFactory edgeFactory = new EdgeFactory(
                layout,
                settings.EdgeLayoutSettings.EdgeWidth,
                settings.EdgeSelectionSettings.TubularSegments,
                settings.EdgeSelectionSettings.Radius,
                settings.EdgeSelectionSettings.RadialSegments,
                settings.EdgeSelectionSettings.AreSelectable);
            // The resulting game objects representing the edges.
            ICollection<GameObject> result;
            // Calculate only
            if (!draw)
            {
                result = edgeFactory.CalculateNewEdges(gameNodes.Cast<ILayoutNode>().ToList(), layoutEdges);
            }
            // Calculate and draw edges
            else
            {
                result = edgeFactory.DrawEdges(gameNodes.Cast<ILayoutNode>().ToList(), layoutEdges);
                InteractionDecorator.PrepareForInteraction(result);
                AddLOD(result);
            }

#if UNITY_EDITOR
            p.End();
            Debug.Log($"Calculated \"  {settings.EdgeLayoutSettings.Kind} \" edge layout for {gameNodes.Count}"
                      + $" nodes and {result.Count} edges in {p.GetElapsedTime()} [h:m:s:ms].\n");
#endif
            return result;
        }

        /// <summary>
        /// Yields the edge layout as specified in the <see cref="settings"/>.
        /// </summary>
        /// <returns>specified edge layout</returns>
        private IEdgeLayout GetEdgeLayout()
        {
            float minimalEdgeLevelDistance = 2.5f * settings.EdgeLayoutSettings.EdgeWidth;
            bool edgesAboveBlocks = settings.EdgeLayoutSettings.EdgesAboveBlocks;
            float rdp = settings.EdgeLayoutSettings.RDP;
            switch (settings.EdgeLayoutSettings.Kind)
            {
                case EdgeLayoutKind.Straight:
                    return new StraightEdgeLayout(edgesAboveBlocks, minimalEdgeLevelDistance);
                case EdgeLayoutKind.Spline:
                   return new SplineEdgeLayout(edgesAboveBlocks, minimalEdgeLevelDistance, rdp);
                case EdgeLayoutKind.Bundling:
                    return new BundledEdgeLayout(edgesAboveBlocks, minimalEdgeLevelDistance, settings.EdgeLayoutSettings.Tension, rdp);
                case EdgeLayoutKind.None:
                    // nothing to be done
                    return null;
                default:
                    throw new Exception("Unhandled edge layout " + settings.EdgeLayoutSettings.Kind);
            }
        }

        /// <summary>
        /// Returns the list of layout edges for all edges in between <paramref name="gameNodes"/>.
        /// </summary>
        /// <param name="gameNodes">set of game nodes whose connecting edges are requested</param>
        /// <returns>list of layout edges/returns>
        private static ICollection<LayoutEdge> ConnectingEdges(ICollection<AbstractLayoutNode> gameNodes)
        {
            ICollection<LayoutEdge> edges = new List<LayoutEdge>();
            Dictionary<Node, AbstractLayoutNode> map = NodeToGameNodeMap(gameNodes);

            foreach (AbstractLayoutNode source in gameNodes)
            {
                Node sourceNode = source.ItsNode;

                foreach (Edge edge in sourceNode.Outgoings)
                {
                    Node target = edge.Target;
                    edges.Add(new LayoutEdge(source, map[target], edge));
                }
            }
            return edges;
        }
    }
}
