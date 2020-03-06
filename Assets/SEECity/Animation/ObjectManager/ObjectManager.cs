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
    /// Implements the <see cref="AbstractObjectManager"/> by using a supplied
    /// AbstractSEECity to create its GameObjects.
    /// </summary>
    public class ObjectManager : AbstractObjectManager
    {
        /// <summary>
        /// The plane enclosing all game objects of the city.
        /// </summary>
        private GameObject currentPlane;

        /// <summary>
        /// A dictionary containing all created nodes that are currently in use. The set of
        /// nodes contained may be an accumulation of all nodes created and added by GetInnerNode()
        /// and GetLeaf() so far and not just those of one single graph in the graph series
        /// (unless a node was removed by RemoveNode() meanwhile).
        /// </summary>
        private readonly Dictionary<string, GameObject> nodes = new Dictionary<string, GameObject>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="renderer">the graph renderer used to create the game objects</param>
        public ObjectManager(GraphRenderer renderer) : base(renderer)
        {
        }

        /// <summary>
        /// Returns a list containing all created nodes that are in use.
        /// </summary>
        public override List<GameObject> GameObjects => nodes.Values.ToList();

        /// <summary>
        /// Returns a saved plane or generates a new one if it does not already exist. The resulting
        /// plane encloses all currently cached game objects of the city only if it was newly 
        /// generated. It may need to be adjusted if it was not newly generated. TODO.
        /// </summary>
        /// <param name="plane">the plane intended to enclose all game objects of the city</param>
        /// <returns>true if the plane already existed (thus, can be re-used) and false if it was newly created</returns>
        public override bool GetPlane(out GameObject plane)
        {
            bool hasPlane = currentPlane != null;
            if (!hasPlane)
            {
                currentPlane = GraphRenderer.NewPlane(GameObjects);
            }
            plane = currentPlane;
            return hasPlane;
        }

        /// <summary>
        /// Returns a saved GameObject for an inner node or creates and caches a new one if it does not 
        /// already exist. Scale and style of <paramref name="innerNode"/> remain unchanged.
        /// The given <paramref name="node"/> will be attached to <paramref name="leaf"/> and replaces
        /// its currently attached graph node.
        /// </summary>
        /// <param name="node">the inner node under which a GameObject may be stored</param>
        /// <param name="innerNode">the resulting GameObject associated to node or null if no GameObject 
        /// could be found or created</param>
        /// <returns>true if the GameObject already existed and false if it was newly created</returns>
        public override bool GetInnerNode(Node node, out GameObject innerNode)
        {
            node.AssertNotNull("node");

            if (nodes.TryGetValue(node.LinkName, out innerNode))
            {
                // The game object has already a node attached to it, but that
                // node is part of a different graph (i.e,, different revision).
                // That is why we replace the attached node by this node here.
                ReattachNode(innerNode, node);
                return true;
            }
            else
            {
                // NewInnerNode() will attach node to innerNode
                innerNode = GraphRenderer.NewInnerNode(node);
                // Note: The scale of innerNode will be adjusted later when we have the
                // layout. 
                // TODO: Inner nodes have a style, too, as much as leaves. We may need to
                // adjust that style, too, either here or (likely better) later when we 
                // apply the layout to inner nodes.
                return false;
            }
        }

        /// <summary>
        /// Returns a saved GameObject for a leaf node or creates and caches a new one if it does not 
        /// already exist.
        /// The resulting game object will have the dimensions and style according to the attributes of 
        /// the given <paramref name="node"/> even if the game node existed already. The position of the
        /// resulting game object is random. The reason for that is the fact that layouts do not change 
        /// the scale (well, some of them, for instance the TreeMap, may shrink or extend the scale by  
        /// a factor). Instead the node layouters need to know the scale of the nodes they are to layout 
        /// upfront. On the other hand, the layouts determine the positions.
        /// The given <paramref name="node"/> will be attached to <paramref name="leaf"/> and replaces
        /// its currently attached graph node.
        /// </summary>
        /// <param name="node">the leaf node under which a GameObject may be stored</param>
        /// <param name="leaf">the resulting GameObject associated to node or null if no GameObject 
        /// could be found or created</param>
        /// <returns>true if the GameObject already existed and false if it was newly created</returns>
        public override bool GetLeaf(Node node, out GameObject leaf)
        {
            node.AssertNotNull("node");

            if (nodes.TryGetValue(node.LinkName, out leaf))
            {
                // We are re-using an existing node, but that node's attributes
                // might have changed. That is why we need to adjust its scale
                // and color.

                // The game object has already a node attached to it, but that
                // node is part of a different graph (i.e,, different revision).
                // That is why we replace the attached node by this node here.
                ReattachNode(leaf, node);

                // Now after having attached the new node to the game object,
                // we must adjust the visual attributes of it according to the
                // newly attached node.
                GraphRenderer.AdjustVisualsOfBlock(leaf);
                return true;
            }
            else
            {
                // NewLeafNode() will set the scale and color of the leaf
                // and will also attach node to it.
                leaf = GraphRenderer.NewLeafNode(node);
                return false;
            }
        }

        /// <summary>
        /// Re-attaches the given <paramref name="node"/> to the given <paramref name="gameObject"/>,
        /// that is, the NodeRef component of <paramref name="gameObject"/> will refer to 
        /// <paramref name="node"/> afterwards.
        /// </summary>
        /// <param name="gameObject">the game object where the node is to be attached to</param>
        /// <param name="node">the node to be attached</param>
        private static void ReattachNode(GameObject gameObject, Node node)
        {
            NodeRef noderef = gameObject.GetComponent<NodeRef>();
            if (noderef == null)
            {
                // noderef should not be null
                Debug.LogErrorFormat("Re-used game object for node '{0}' does not have a graph node attached to it\n",
                                     node.LinkName);
                noderef = gameObject.AddComponent<NodeRef>();
            }
            noderef.node = node;
        }

        /// <summary>
        /// Removes a supplied node by using its Node.LinkName and returns
        /// the removed node, if some was removed.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public override bool RemoveNode(Node node, out GameObject gameObject)
        {
            node.AssertNotNull("node");

            var wasNodeRemoved = nodes.TryGetValue(node.LinkName, out gameObject);
            nodes.Remove(node.LinkName);
            return wasNodeRemoved;
        }

        /// <summary>
        /// Clears the internal lists containing the GameObjects,
        /// without destroing them.
        /// </summary>
        public override void Clear()
        {
            currentPlane = null;
            nodes.Clear();
        }
    }
}