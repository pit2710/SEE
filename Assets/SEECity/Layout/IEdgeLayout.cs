﻿using SEE.DataModel;
using SEE.GO;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Layout
{
    public abstract class IEdgeLayout
    {
        public IEdgeLayout(NodeFactory blockFactory, float edgeWidth, bool edgesAboveBlocks)
        {
            this.blockFactory = blockFactory;
            this.edgeWidth = edgeWidth;
            this.edgesAboveBlocks = edgesAboveBlocks;
        }

        /// <summary>
        /// Name of the layout.
        /// </summary>
        protected string name = "";

        /// <summary>
        /// A factory to create visual representations of graph nodes (e.g., cubes or CScape buildings).
        /// </summary>
        protected readonly NodeFactory blockFactory;

        /// <summary>
        /// Orientation of the edges; 
        /// if false, the edges are drawn below the houses;
        /// if true, the edges are drawn above the houses;
        /// </summary>
        protected readonly bool edgesAboveBlocks;

        /// <summary>
        /// Path to the material used for edges.
        /// </summary>
        protected const string materialPath = "Legacy Shaders/Particles/Additive";
        // protected const string materialPath = "BrickTextures/BricksTexture13/BricksTexture13";
        // protected const string materialPath = "Particles/Standard Surface";

        /// <summary>
        /// The material used for edges.
        /// </summary>
        protected readonly Material defaultLineMaterial = LineMaterial();

        /// <summary>
        /// The width of every edge.
        /// </summary>
        protected readonly float edgeWidth;

        // A mapping of graph nodes onto the game objects representing them visually in the scene
        protected Dictionary<Node, GameObject> gameNodes = new Dictionary<Node, GameObject>();

        internal abstract ICollection<LayoutEdge> GetLines(ICollection<ILayoutNode> layoutNodes);

        /// <summary>
        /// Adds all graph nodes contained in any of the game nodes given to gameNodes mapping.
        /// 
        /// Precondition: every game node N in nodes must have a component NodeRef; otherwise
        /// an exception is thrown.
        /// </summary>
        /// <param name="nodes">list of game nodes whose contained graph nodes are to be mapped</param>
        protected void SetGameNodes(ICollection<GameObject> nodes)
        {
            foreach (GameObject gameNode in nodes)
            {
                NodeRef nodeRef = gameNode.GetComponent<NodeRef>();
                if (nodeRef != null)
                {
                    gameNodes[nodeRef.node] = gameNode;
                }
                else
                {
                    throw new System.Exception("game node without graph node component");
                }
            }
        }

        /// <summary>
        /// The unique name of a layout.
        /// </summary>
        public string Name
        {
            get => name;
        }

        /// <summary>
        /// Creates the GameObjects representing the edges of the graph.
        /// The graph must have been loaded before via Load().
        /// Intended to be overriden by subclasses.
        /// </summary>
        /// <param name="graph">graph whose edges are to be drawn</param>
        /// <param name="nodes">the list of nodes whose edges are to be drawn</param>
        /// <returns>all game objects created to represent the edges</returns>
        public abstract ICollection<GameObject> DrawEdges(Graph graph, ICollection<GameObject> nodes);

        /// <summary>
        /// Returns the default material for edges using the materialPath.
        /// </summary>
        /// <returns>default material for edges</returns>
        private static Material LineMaterial()
        {
            Material material = new Material(Shader.Find(materialPath));
            if (material == null)
            {
                Debug.LogError("Could not find material " + materialPath + "\n");
            }
            return material;
        }

        /// <summary>
        /// Yields the greatest and smallest y co-ordinate and the maximal height of all <paramref name="nodes"/> given.
        /// 
        /// Precondition: <paramref name="nodes"/> is not empty.
        /// </summary>
        /// <param name="nodes">list of nodes whose greatest and smallest y co-ordinate is required</param>
        /// <param name="minY">smallest y co-ordinate</param>
        /// <param name="maxY">highest x co-ordinate</param>
        /// <param name="maxHeight">maximal height of nodes</param>
        protected void MinMaxBlockY(ICollection<GameObject> nodes, out float minY, out float maxY, out float maxHeight)
        {
            maxY = Mathf.NegativeInfinity;
            minY = Mathf.Infinity;
            maxHeight = 0.0f;
            foreach (GameObject node in nodes)
            {
                float y = blockFactory.Roof(node).y;
                if (y > maxY)
                {
                    maxY = y;
                }
                else if (y < minY)
                {
                    minY = y;
                }
                float h = blockFactory.GetSize(node).y;
                if (h > maxHeight)
                {
                    maxHeight = h;
                }
            }
        }

        /// <summary>
        /// Yields the greatest and smallest y co-ordinate and the maximal height of all <paramref name="nodes"/> given.
        /// 
        /// Precondition: <paramref name="nodes"/> is not empty.
        /// </summary>
        /// <param name="nodes">list of nodes whose greatest and smallest y co-ordinate is required</param>
        /// <param name="minY">smallest y co-ordinate</param>
        /// <param name="maxY">highest x co-ordinate</param>
        /// <param name="maxHeight">maximal height of nodes</param>
        protected void MinMaxBlockY(ICollection<ILayoutNode> nodes, out float minY, out float maxY, out float maxHeight)
        {
            maxY = Mathf.NegativeInfinity;
            minY = Mathf.Infinity;
            maxHeight = 0.0f;
            foreach (ILayoutNode node in nodes)
            {
                float y = node.Roof.y;
                if (y > maxY)
                {
                    maxY = y;
                }
                else if (y < minY)
                {
                    minY = y;
                }
                float h = node.Scale.y;
                if (h > maxHeight)
                {
                    maxHeight = h;
                }
            }
        }

        /// <summary>
        /// Returns a new game edge.
        /// </summary>
        /// <param name="edge">graph edge for which to create the game edge</param>
        /// <returns>new game edge</returns>
        protected GameObject NewGameEdge(Edge edge)
        {
            GameObject gameEdge = new GameObject
            {
                tag = Tags.Edge,
                isStatic = true,
                name = edge.Type + "(" + edge.Source.LinkName + ", " + edge.Target.LinkName + ")"
            };
            gameEdge.AddComponent<EdgeRef>().edge = edge;
            return gameEdge;
        }
    }
}
