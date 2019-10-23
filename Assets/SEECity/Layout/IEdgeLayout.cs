﻿using SEE.DataModel;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Layout
{
    public abstract class IEdgeLayout
    {
        public IEdgeLayout(BlockFactory blockFactory, float edgeWidth, bool edgesAboveBlocks)
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
        protected readonly BlockFactory blockFactory;

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
        protected readonly static Material defaultLineMaterial = LineMaterial();

        /// <summary>
        /// The width of every edge.
        /// </summary>
        protected readonly float edgeWidth;

        // A mapping of graph nodes onto the game objects representing them visually in the scene
        protected Dictionary<Node, GameObject> gameNodes = new Dictionary<Node, GameObject>();

        /// <summary>
        /// Adds all graph nodes contained in any of the game nodes given to gameNodes mapping.
        /// 
        /// Precondition: every game node N in nodes must have a component NodeRef; otherwise
        /// an exception is thrown.
        /// </summary>
        /// <param name="nodes">list of game nodes whose contained graph nodes are to be mapped</param>
        protected void SetGameNodes(IList<GameObject> nodes)
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
        public abstract void DrawEdges(Graph graph, IList<GameObject> nodes);

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
    }
}
