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
using UnityEngine;

namespace SEE.Animation.Internal
{
    /// <summary>
    /// Data model containing all data generated for a graph loaded from a GXL file
    /// and its layout data.
    /// </summary>
    public class LaidOutGraph
    {
        /// <summary>
        /// The graph.
        /// </summary>
        private readonly Graph graph;
        /// <summary>
        /// The layout for the graph.
        /// </summary>
        private readonly Dictionary<GameObject, NodeTransform> layout;

        /// <summary>
        /// The graph.
        /// </summary>
        public Graph Graph => graph;

        /// <summary>
        /// The layout of the graph.
        /// </summary>
        public Dictionary<GameObject, NodeTransform> Layout => layout;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graph">the graph</param>
        /// <param name="layout">its layout out</param>
        public LaidOutGraph(Graph graph, Dictionary<GameObject, NodeTransform> layout)
        {
            this.graph = graph.AssertNotNull("graph");
            this.layout = layout.AssertNotNull("layout");
        }
    }
}