﻿using System;
using SEE.DataModel.DG;
using SEE.Game.City;
using SEE.GO;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Game
{
    /// <summary>
    /// Creates new game objects representing graph nodes or deleting these again,
    /// respectively.
    /// </summary>
    public static class GameNodeAdder
    {
        /// <summary>
        /// Creates and returns a new graph node with a random unique ID,
        /// an empty source name, and an unknown node type. This node is
        /// not yet in any graph.
        /// </summary>
        /// <param name="nodeID">the unique ID of the new node; if null or empty, an empty ID will be used</param>
        /// <param name="type">The type of the new node;if null or empty, the type <see cref="Graph.UnknownType"/> is used</param>
        /// <param name="sourceName">the source name of the new node; if null or empty, an empty string is used</param>
        /// <returns>new graph node</returns>
        private static Node NewGraphNode(string nodeID, string type = null, string sourceName = null )
        {
            string SourceName = string.IsNullOrEmpty(sourceName) ? string.Empty : sourceName;
            string Type = string.IsNullOrEmpty(type) ? Graph.UnknownType : type;
            string ID = string.IsNullOrEmpty(nodeID) ? Guid.NewGuid().ToString() : nodeID;
            return new Node()
            {
                ID = ID,
                SourceName = SourceName,
                Type = Type
            };
        }

        /// <summary>
        /// Adds a <paramref name="node"/> as a child of <paramref name="parent"/> to the
        /// graph containing <paramref name="parent"/> with a random unique ID.
        ///
        /// If node has no ID yet (null or empty), a random unique ID will be used. If it has
        /// an ID, that ID will be kept. In case this ID is not unique, an exception will
        /// be thrown.
        ///
        /// Precondition: <paramref name="parent"/> must not be null, neither may
        /// <paramref name="parent"/> and <paramref name="node"/> be equal; otherwise an exception
        /// will be thrown.
        /// </summary>
        /// <param name="parent">The node that should be the parent of <paramref name="node"/></param>
        /// <param name="node">The node to add to the graph</param>
        private static void AddNodeToGraph(Node parent, Node node)
        {
            //TODO could be done way easier
            if (parent == null)
            {
                Graph parentGraph = parent.ItsGraph;
                if (parentGraph == null)
                {
                    throw new Exception("Parent must be in a graph.");
                }
                if (string.IsNullOrEmpty(node.ID))
                {
                    // Loop until the node.ID is unique within the graph.
                    node.ID = Guid.NewGuid().ToString();
                    while (parentGraph.GetNode(node.ID) != null)
                    {
                        node.ID = Guid.NewGuid().ToString();
                    }
                }
                parentGraph.AddNode(node);
            }
            else if (parent == node)
            {
                throw new Exception("Node must not be its own parent.");
            }
            else
            {
                Graph graph = parent.ItsGraph;
                if (graph == null)
                {
                    throw new Exception("Parent must be in a graph.");
                }
                if (string.IsNullOrEmpty(node.ID))
                {
                    // Loop until the node.ID is unique within the graph.
                    node.ID = Guid.NewGuid().ToString();
                    while (graph.GetNode(node.ID) != null)
                    {
                        node.ID = Guid.NewGuid().ToString();
                    }
                }
                // Note: ReflexionGraph.AddNode(node) determines the subgraph where node should be
                // added via its parent. That means, the parent of node must be set before it can
                // be called.
                parent.AddChild(node);
                graph.AddNode(node);
            }
        }

        /// <summary>
        /// Creates and returns a new game node as a child of <paramref name="parent"/> at the
        /// given <paramref name="worldSpacePosition"/> with the given <paramref name="worldSpaceScale"/>.
        ///
        /// Precondition: <paramref name="parent"/> must have a valid node reference
        /// and must be contained in a code city.
        ///
        /// Postcondition: The returned child is an immediate child of <paramref name="parent"/> in the
        /// game object hierarchy and in the underlying graph.
        /// </summary>
        /// <param name="parent">parent of the new node</param>
        /// <param name="worldSpacePosition">the position in world space for the center point of the new game node</param>
        /// <param name="worldSpaceScale">the scale in world space of the new game node</param>
        /// <param name="nodeID">the unique ID of the new node; if null or empty, a random ID will be used</param>
        /// <returns>new child game node>/returns>
        /// <exception cref="Exception">thrown if <paramref name="parent"/> has no valid node reference
        /// or is not contained in a code city</exception>
        public static GameObject AddChild(GameObject parent, Vector3 worldSpacePosition, Vector3 worldSpaceScale, string nodeID = null)
        {
            GameObject result = AddChild(parent, nodeID);
            // Resetting the parent to null temporarily so that there is no difference between
            // local scale and world-space scale.
            result.transform.SetParent(null);
            // result is just created, hence, we do not need a NodeOperator to position and scale it.
            result.transform.position = worldSpacePosition;
            result.transform.localScale = worldSpaceScale;
            result.transform.SetParent(parent.transform);
            return result;
        }

        /// <summary>
        /// Creates and returns a new game node as a child of <paramref name="parent"/>.
        /// The world-space position and scale of the result will be the world-space
        /// position and scale of <paramref name="parent"/>.
        ///
        /// Precondition: <paramref name="parent"/> must have a valid node reference
        /// and must be contained in a code city.
        ///
        /// Postcondition: The returned child is an immediate child of <paramref name="parent"/> in the
        /// game object hierarchy and in the underlying graph.
        /// </summary>
        /// <param name="parent">parent of the new node</param>
        /// <param name="nodeID">the unique ID of the new node; if null or empty, a random ID will be used</param>
        /// <returns>new child game node>/returns>
        /// <exception cref="Exception">thrown if <paramref name="parent"/> has no valid node reference
        /// or is not contained in a code city</exception>
        public static GameObject AddChild(GameObject parent, string nodeID = null)
        {
            SEECity city = parent.ContainingCity() as SEECity;
            if (city != null)
            {
                Node node = NewGraphNode(nodeID);
                AddNodeToGraph(parent.GetNode(), node);
                GameObject result = city.Renderer.DrawNode(node);
                result.transform.position = parent.transform.position;
                result.transform.localScale = parent.transform.lossyScale;
                result.transform.SetParent(parent.transform);
                Portal.SetPortal(city.gameObject, gameObject: result);
                return result;
            }
            else
            {
                throw new Exception($"Parent node {parent.FullName()} is not contained in a code city.");
            }
        }

        /// <summary>
        /// Creates and returns a new game node as a child of <paramref name="parent"/> having a <see cref="NodeRef"/>
        /// component referencing <see cref="Node"/> at the given <paramref name="position"/>
        /// with the given <paramref name="worldScale"/>
        /// Precondition: <paramref name="parent"/> must have a valid node reference.
        /// </summary>
        /// <param name="parent">Parent of the new node.</param>
        /// <param name="position">The position in world space for the center point of the new game node</param>
        /// <param name="worldScale">The scale in world space of the new game node</param>
        /// <param name="nodeID">The unique id of the new node; if null or empty, a random ID will be used</param>
        /// <param name="nodeType">The type of the new graph node. Must not be null</param>
        /// <returns>The new child game node or null if none could be created.</returns>
        public static GameObject AddArchitectureNode(GameObject parent, Vector3 position, Vector3 worldScale,
            string nodeType, string nodeID = null)
        {
            SEECityArchitecture city = parent.ContainingArchitectureCity();
            if (city != null)
            {
                parent.TryGetNode(out Node parentNode);
                Assert.IsNotNull(nodeType);
                //Generate the default source name for this new architecture node.
                string sourceName = "arch_" + nodeType + "_" + city.NODE_COUNTER++;
                Node node = NewGraphNode(nodeID, nodeType, sourceName);
                AddNodeToGraph(parentNode, node);
                GameObject result = city.Renderer.DrawNode(node);
                result.transform.localScale = worldScale;
                result.transform.position = position;
                result.transform.SetParent(parent.transform);
                city.Renderer.RefreshNodeStyle(city.gameObject, SceneQueries.AllGameNodesInScene(true, true));
                return result;
            }
            return null;

        }

        /// <summary>
        /// Inverse operation of <see cref="Add(GameObject, Vector3, Vector3, string)"/>.
        /// Removes the given <paramref name="gameNode"/> from the scene and its associated
        /// graph node from its graph.
        ///
        /// Notes:
        ///
        /// <paramref name="gameNode"/> is not actually destroyed.
        ///
        /// If <paramref name="gameNode"/> represents an inner node of the node
        /// hierarchy, its ancestors will not be deleted.
        ///
        /// Precondition: <paramref name="gameNode"/> must have a valid NodeRef; otherwise
        /// an exception will be thrown.
        /// </summary>
        /// <param name="gameNode">game node to be removed</param>
        public static void Remove(GameObject gameNode)
        {
            Node node = gameNode.GetNode();
            Graph graph = node.ItsGraph;
            graph.RemoveNode(node);
        }
    }
}
