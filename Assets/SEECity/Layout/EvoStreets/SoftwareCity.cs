﻿using System;
using System.Collections.Generic;
using SEE.DataModel;
using UnityEngine;

namespace SEE.Layout.EvoStreets
{
    public class SoftwareCity
    {
        /// <summary>
        /// The leaf and inner nodes to be laid out.
        /// </summary>
        private List<GameObject> gameObjects = new List<GameObject>();

        /// <summary>
        /// Determines how to scale the node metrics.
        /// </summary>
        private IScale scaler;

        /// <summary>
        /// The settings to be considered for the layout.
        /// </summary>
        private GraphSettings graphSettings;

        /// <summary>
        /// The resulting layout.
        /// </summary>
        private Dictionary<GameObject, NodeTransform> layout_result;


        public Dictionary<GameObject, NodeTransform> GenerateCity(DataModel.Graph graph, IScale scaler, GraphSettings graphSettings)
        {
            layout_result = new Dictionary<GameObject, NodeTransform>();

            DateTime before = DateTime.Now;

            this.graphSettings = graphSettings;
            this.scaler = scaler;

            NodeFactory leafNodeFactory = new CubeFactory(); // FIXME
            float groundLevel = 0.0f; // FIXME

            gameObjects = AllNodes(graph);

            EvoStreetsNodeLayout evoStreetLayout = new EvoStreetsNodeLayout(groundLevel, leafNodeFactory, scaler, graphSettings);
            Dictionary<GameObject, NodeTransform> layout = evoStreetLayout.Layout(gameObjects);
            ENode rootNode = evoStreetLayout.GetRoot();

            //ENode rootNode = cityGenerator.GenerateCity(graph, scaler, graphSettings);
            bool rootStreetDimensionless = rootNode.Depth != 0;

            SwapZWithY(rootNode);

            GameObject gameObject = new GameObject();
            gameObject.tag = DataModel.Tags.Block;
            gameObject.name = "ROOT";
            SpawnNode(rootNode, gameObject);

            DateTime after = DateTime.Now;
            TimeSpan duration = after.Subtract(before);

            Debug.Log("Duration in milliseconds: " + duration.Milliseconds + "\n");

            return layout_result;
        }

        // FIXME: Can be removed later.
        private List<GameObject> AllNodes(Graph graph)
        {
            List<GameObject> gameObjects = new List<GameObject>();

            foreach (Node node in graph.Nodes())
            {
                GameObject gameObject = new GameObject(node.LinkName);
                NodeRef nodeRef = gameObject.AddComponent<NodeRef>();
                nodeRef.node = node;
            }
            return gameObjects;
        }

        /**
         * This fixes the fact that height in unity is the y component of a vector while in unreal it's the z component.
         */
        private void SwapZWithY(ENode node)
        {
            // Swap scale
            var origScaleZ = node.Scale.z;
            var origScaleX = node.Scale.x;
            node.Scale.x = node.Scale.y;
            node.Scale.y = origScaleZ;
            node.Scale.z = origScaleX;

            // Swap location
            var origLocationZ = node.Location.z;
            var origLocationX = node.Location.x;
            node.Location.x = node.Location.y;
            node.Location.y = origLocationZ;
            node.Location.z = origLocationX;

            foreach (var child in node.Children)
            {
                SwapZWithY(child);
            }
        }

        private NodeFactory leafNodeFactory = new CubeFactory();

        private GameObject NewLeaf(DataModel.Node node)
        {
            float metricMaximum = scaler.GetNormalizedMaximum(graphSettings.ColorMetric);

            int material = Mathf.RoundToInt(Mathf.Lerp(0.0f,
                                                       (float)(leafNodeFactory.NumberOfMaterials() - 1),
                                                       scaler.GetNormalizedValue(graphSettings.ColorMetric, node)
                                                                 / metricMaximum));
            return leafNodeFactory.NewBlock(material);
        }

        private void SpawnNode(ENode node, GameObject parentGameObject)
        {
            if (node == null)
            {
                Debug.LogAssertion("Node cannot be null");
                return;
            }

            if (node.IsHouse())
            {
                var spawnedHouse = SpawnHouse(node, parentGameObject, node.Location, node.Scale,
                    node.Rotation);

                // TODO: Root Point Calculation for RelationshipGenerator

                if (spawnedHouse == null)
                {
                    Debug.LogError($"House could not be spawned: {node}");
                    return;
                }

                gameObjects.Add(spawnedHouse);
            } // End isHouse

            if (node.IsStreet())
            {
                var spawnedStreet = SpawnStreet(node, parentGameObject, node.Location,
                    new Vector3(node.Scale.x, EvoStreetsNodeLayout.StreetHeight, node.Scale.z), node.Rotation);

                if (spawnedStreet == null)
                {
                    Debug.LogError($"Street could not be spawned: {node}");
                    return;
                }

                gameObjects.Add(spawnedStreet);

                foreach (var child in node.Children)
                {
                    SpawnNode(child, spawnedStreet);
                }

                // TODO: district generation
            } // End isStreet
        }

        private GameObject SpawnHouse(ENode node, GameObject parentGameObject,
                                      Vector3 nodeLocation, Vector3 nodeScale,
                                      float nodeRotationZ)
        {
            if (node == null)
            {
                Debug.LogError("Node must not be null");
                return null;
            }

            // TODO: instantiate some kind of actor/gameobj to display house
            var rotation = Quaternion.Euler(0, nodeRotationZ, 0); // this needs to be assigned to y due to difference between unity and unreal
            var o = NewLeaf(node.GraphNode);
            o.transform.localScale = nodeScale;
            o.transform.position = node.Location + graphSettings.origin;
            o.transform.rotation = rotation;
            o.name = (node.IsHouse() ? "House " : (node.IsStreet()) ? "Street " : "None ") + node.GraphNode.LinkName;
            AttachNode(o, node.GraphNode);
            layout_result[o] = new NodeTransform(o.transform.position, o.transform.localScale);

            return o;
        }

        /// <summary>
        /// Adds a NodeRef component to given game node referencing to given graph node.
        /// </summary>
        /// <param name="gameNode"></param>
        /// <param name="node"></param>
        protected void AttachNode(GameObject gameNode, DataModel.Node node)
        {
            NodeRef nodeRef = gameNode.AddComponent<NodeRef>();
            nodeRef.node = node;
        }

        private CubeFactory streetFactory = new CubeFactory();

        private GameObject SpawnStreet(ENode node, GameObject parentGameObject,
                                       Vector3 nodeLocation, Vector3 nodeScale,
                                       float nodeRotationZ)
        {
            if (node == null)
            {
                Debug.LogError("Node must not be null");
                return null;
            }

            // TODO: instantiate some kind of actor/gameobj to display street
            var rotation = Quaternion.Euler(0, nodeRotationZ, 0); // this needs to be assigned to y due to difference between unity and unreal
            var o = streetFactory.NewBlock(0);
            o.transform.position = node.Location + graphSettings.origin;
            o.transform.rotation = rotation;
            o.transform.localScale = nodeScale;
            o.name = (node.IsHouse() ? "House " : (node.IsStreet()) ? "Street " : "None ") + node.GraphNode.LinkName;
            AttachNode(o, node.GraphNode);
            layout_result[o] = new NodeTransform(o.transform.position, o.transform.localScale);

            return o;
        }
    }
}
