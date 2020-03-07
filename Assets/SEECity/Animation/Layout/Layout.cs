﻿//Copyright 2020 Florian Garbade

//Permission is hereby granted, free of charge, to any person obtaining a
//copy of this software and associated documentation files (the "Software"),
//to deal in the Software without restriction, including without limitation
//the rights to use, copy, modify, merge, publish, distribute, sublicense,
//and/or sell copies of the Software, and to permit persons to whom the Software
//is furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
//PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
//USE OR OTHER DEALINGS IN THE SOFTWARE.

using SEE.DataModel;
using SEE.Layout;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SEE.Animation.Internal
{
    /// <summary>
    /// A Layout uses a given NodeLayout to calculate the layout and save
    /// it for later use.
    /// </summary>
    public class Layout
    {
        /// <summary>
        /// The calculated NodeTransforms representing the layout.
        /// </summary>
        private readonly Dictionary<string, NodeTransform> nodeTransforms = new Dictionary<string, NodeTransform>();

        /// <summary>
        /// Returns a NodeTransform for a given node, using the Node.LinkName attribute
        /// according to the layout.
        /// </summary>
        /// <param name="node">node for which the NodeTransform is rquested</param>
        /// <returns>NodeTransform of given node</returns>
        public NodeTransform GetNodeTransform(Node node)
        {
            nodeTransforms.TryGetValue(node.LinkName, out NodeTransform nodeTransform);
            return nodeTransform;
        }

        /// <summary>
        /// Calculates the layout data using the given NodeLayout, IScale and ObjectManager for the given graph.
        /// </summary>
        /// <param name="objectManager"></param>
        /// <param name="scaler"></param>
        /// <param name="nodeLayout"></param>
        /// <param name="graph"></param>
        /// <param name="graphSettings"></param>
        public void Calculate(ObjectManager objectManager, IScale scaler, NodeLayout nodeLayout, Graph graph, SEECityEvolution graphSettings)
        {
            // The following code assumes that a leaf node remains a leaf across all
            // graphs of the graph series and an inner node remains an inner node.
            // This may not necessarily be true. For instance, a directory could 
            // get subdirectories in the course of the evolution.

            // Collecting all game objects corresponding to nodes of the given graph.
            // If node existed in a previous graph, we will re-use its corresponding
            // game object created earlier.
            var gameObjects = new List<GameObject>();
            graph.Traverse(
                rootNode =>
                {
                    // FIXME: In rare cases the root could be a leaf node.
                    objectManager.GetInnerNode(rootNode, out var inner);
                    gameObjects.Add(inner);
                },
                innerNode =>
                {
                    objectManager.GetInnerNode(innerNode, out var inner);
                    gameObjects.Add(inner);
                },
                leafNode =>
                {
                    objectManager.GetLeaf(leafNode, out var leaf);
                    /*
                    var size = new Vector3(
                        scaler.GetNormalizedValue(graphSettings.WidthMetric, leafNode),
                        scaler.GetNormalizedValue(graphSettings.HeightMetric, leafNode),
                        scaler.GetNormalizedValue(graphSettings.DepthMetric, leafNode)
                    );
                    objectManager.graphRenderer.SetSize(leaf, size);
                    */
                    gameObjects.Add(leaf);
                }
            );

            // Calculate the layout for the game objects.
            var layoutData = nodeLayout.Layout(gameObjects);

            // Apply the layout to the game objects.
            layoutData.Keys.ToList().ForEach(key =>
            {
                var node = key.GetComponent<NodeRef>().node;
                var nodeTransform = layoutData[key];
                if (node.IsLeaf())
                {
                    var size = new Vector3(
                        scaler.GetNormalizedValue(graphSettings.WidthMetric, node),
                        scaler.GetNormalizedValue(graphSettings.HeightMetric, node),
                        scaler.GetNormalizedValue(graphSettings.DepthMetric, node)
                    );
                    nodeTransform.scale = size;
                }
                nodeTransforms.Add(node.LinkName, nodeTransform);
            });
        }
    }
}