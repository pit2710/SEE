﻿using System.Collections.Generic;
using SEE.Controls;
using SEE.DataModel;
using SEE.DataModel.DG;
using SEE.GO;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Game
{
    /// <summary>
    /// Provides queries on game objects in the current scence at run-time.
    /// </summary>
    internal class SceneQueries
    {
        /// <summary>
        /// Returns all game objects in the current scene tagged by Tags.Node and having 
        /// a valid reference to a graph node.
        /// </summary>
        /// <returns>all game objects representing graph nodes in the scene</returns>
        public static ICollection<GameObject> AllGameNodesInScene(bool includeLeaves, bool includeInnerNodes)
        {
            List<GameObject> result = new List<GameObject>();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Tags.Node))
            {
                if (go.TryGetComponent<NodeRef>(out NodeRef nodeRef))
                {
                    Node node = nodeRef.node;
                    if (node != null)
                    {
                        if ((includeLeaves && node.IsLeaf()) || (includeInnerNodes && !node.IsLeaf()))
                        {
                            result.Add(go);
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("Game node {0} has a null node reference.\n", go.name);
                    }
                }
                else
                {
                    Debug.LogWarningFormat("Game node {0} without node reference.\n", go.name);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the roots of all graphs currently represented by any of the <paramref name="gameNodes"/>.
        /// 
        /// Precondition: Every game object in <paramref name="gameNodes"/> must be tagged by
        /// Tags.Node and have a valid graph node reference.
        /// </summary>
        /// <param name="gameNodes">game nodes whose roots are to be returned</param>
        /// <returns>all root nodes in the scene</returns>
        public static ICollection<Node> GetRoots(ICollection<GameObject> gameNodes)
        {
            List<Node> result = new List<Node>();
            foreach (Graph graph in GetGraphs(gameNodes))
            {
                result.AddRange(graph.GetRoots());
            }
            return result;
        }

        /// <summary>
        /// Returns all graphs currently represented by any of the <paramref name="gameNodes"/>.
        /// 
        /// Precondition: Every game object in <paramref name="gameNodes"/> must be tagged by
        /// Tags.Node and have a valid graph node reference.
        /// </summary>
        /// <param name="gameNodes">game nodes whose graph is to be returned</param>
        /// <returns>all graphs in the scene</returns>
        public static HashSet<Graph> GetGraphs(ICollection<GameObject> gameNodes)
        {
            HashSet<Graph> result = new HashSet<Graph>();
            foreach (GameObject go in gameNodes)
            {
                result.Add(go.GetComponent<NodeRef>().node.ItsGraph);
            }
            return result;
        }

        /// <summary>
        /// True if <paramref name="gameNode"/> represents a leaf in the graph.
        /// 
        /// Precondition: <paramref name="gameNode"/> has a NodeRef component attached to it
        /// that is a valid graph node reference.
        /// </summary>
        /// <param name="gameNode"></param>
        /// <returns>true if <paramref name="gameNode"/> represents a leaf in the graph</returns>
        public static bool IsLeaf(GameObject gameNode)
        {
            return gameNode.GetComponent<NodeRef>()?.node?.IsLeaf() ?? false;
        }

        /// <summary>
        /// True if <paramref name="gameNode"/> represents an inner node in the graph.
        /// 
        /// Precondition: <paramref name="gameNode"/> has a NodeRef component attached to it
        /// that is a valid graph node reference.
        /// </summary>
        /// <param name="gameNode"></param>
        /// <returns>true if <paramref name="gameNode"/> represents an inner node in the graph</returns>
        public static bool IsInnerNode(GameObject gameNode)
        {
            return gameNode.GetComponent<NodeRef>()?.node?.IsInnerNode() ?? false;
        }

        /// <summary>
        /// Returns the Source.Name attribute of <paramref name="gameNode"/>. 
        /// If <paramref name="gameNode"/> has no valid node reference, the name
        /// of <paramref name="gameNode"/> is returned instead.
        /// </summary>
        /// <param name="gameNode"></param>
        /// <returns>source name of <paramref name="gameNode"/></returns>
        public static string SourceName(GameObject gameNode)
        {
            if (gameNode.TryGetComponent<NodeRef>(out NodeRef nodeRef))
            {
                if (nodeRef.node != null)
                {
                    return nodeRef.node.SourceName;
                }
            }
            return gameNode.name;
        }

        /// <summary>
        /// Returns first child of <paramref name="codeCity"/> tagged by Tags.Node. 
        /// If <paramref name="codeCity"/> is a node representing a code city,
        /// the result is considered the root of the graph.
        /// </summary>
        /// <param name="codeCity">object representing a code city</param>
        /// <returns>game object representing the root of the graph or null if there is none</returns>
        public static Transform GetCityRootNode(GameObject codeCity)
        {            
            foreach (Transform child in codeCity.transform)
            {
                if (child.CompareTag(Tags.Node))
                {
                    return child.transform;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the root game object that represents a code city as a whole
        /// along with the settings (layout information etc.). In other words,
        /// we simply return the top-most transform in the game-object hierarchy.
        /// That top-most object must be tagged by Tags.CodeCity. If it is,
        /// it will be returned. If not, null will be returned.
        /// </summary>
        /// <param name="transform">transform at which to start the search</param>
        /// <returns>top-most transform in the game-object hierarchy tagged by 
        /// Tags.CodeCity or null</returns>
        public static Transform GetCodeCity(Transform transform)
        {

            Transform result = transform;
            if (PlayerSettings.GetInputType() == PlayerSettings.PlayerInputType.HoloLens)
            {
                // If the MRTK is enabled, the cities will be part of a CityCollection, so we can't simply use the root.
                // In this case, we actually have to traverse the tree up until the Tags match.
                
                while (cursor != null)
                {
                    if (cursor.CompareTag(Tags.CodeCity))
                    {
                        return cursor;
                    }
                    cursor = cursor.parent;
                }
                return cursor;
            }
            result = transform.root;
	    if (result.CompareTag(Tags.CodeCity))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Equivalent to: GetCityRootNode(gameObject).GetComponent<NodeRef>.node.
        /// </summary>
        /// <param name="codeCity">object representing a code city</param>
        /// <returns>the root node of the graph or null if there is none</returns>
        public static Node GetCityRootGraphNode(GameObject codeCity)
        {
            Transform transform = GetCityRootNode(codeCity);
            if (transform == null)
            {
                return null;
            }

            NodeRef nodeRef = transform.GetComponent<NodeRef>();
            if (nodeRef == null)
            {
                return null;
            }

            return nodeRef.node;
        }

        /// <summary>
        /// Returns the graph of the root node of <paramref name="codeCity"/> assumed
        /// to represent a code city. Equivalent to: GetCityRootGraphNode(gameObject).ItsGraph.
        /// </summary>
        /// <param name="codeCity">object representing a code city</param>
        /// <returns>the graph represented by <paramref name="codeCity"/> or null</returns>
        public static Graph GetGraph(GameObject codeCity)
        {
            Node root = GetCityRootGraphNode(codeCity);

            return root?.ItsGraph;
        }
    }
}
