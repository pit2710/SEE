﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Layout
{
    /// <summary>
    /// Allows one to decorate game objects with rectangle lines.
    /// </summary>
    internal class RectangleDecorator
    {
        /// <summary>
        /// Constructor for creating rectangle lines
        /// </summary>
        /// <param name="nodeFactory">the factory that was used to create the objects for which to draw a rectangle line</param>
        /// <param name="color">the color for the rectangle line</param>
        public RectangleDecorator(NodeFactory nodeFactory, Color color)
        {
            this.nodeFactory = nodeFactory;
            this.color = color;
        }

        /// <summary>
        /// The factory that was used to create the objects for which to draw a rectangle line.
        /// </summary>
        private NodeFactory nodeFactory;

        /// <summary>
        /// The color for the rectangle line.
        /// </summary>
        private Color color;

        /// <summary>
        /// The material we use for the rectangle line. It is the same for all rectangle lines
        /// to reduce the number of drawing calls.
        /// </summary>
        private Material material = new Material(LineFactory.DefaultLineMaterial);

        /// <summary>
        /// Attaches a deccorators to rectangle line to all game nodes. 
        /// </summary>
        /// <param name="gameNodes">list of game nodes for which to create rectangle lines</param>
        public void Add(ICollection<GameObject> gameNodes)
        {
            foreach (GameObject gameNode in gameNodes)
            {
                float lineWidth = 0.7f;
                AttachRectangleLine(gameNode, lineWidth);
            }
        }

        /// <summary>
        /// The thickness of the line is specified by <paramref name="lineWidth"/>.
        /// </summary>
        /// <param name="gameObject">the game object the rectangle is to be drawn</param>
        /// <param name="lineWidth">thickness of the rectangle line</param>
        private void AttachRectangleLine(GameObject gameObject, float lineWidth)
        {
            LineRenderer line;
            line = gameObject.GetComponent<LineRenderer>();
            if (line == null)
            {
                line = gameObject.AddComponent<LineRenderer>();
            }

            LineFactory.SetDefaults(line);
            LineFactory.SetColor(line, color);
            LineFactory.SetWidth(line, 0.7f);

            line.useWorldSpace = false;

            line.sharedMaterial = material;
        }
    }
}
