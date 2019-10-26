﻿using SEE.DataModel;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Layout
{
    public class ManhattenLayout : INodeLayout
    {
        public ManhattenLayout(string widthMetric, string heightMetric, string breadthMetric, 
                               SerializableDictionary<string, IconFactory.Erosion> issueMap,
                               BlockFactory blockFactory,
                               IScale scaler,
                               bool showErosions)
            : base(widthMetric, heightMetric, breadthMetric, issueMap, blockFactory, scaler, showErosions)
        {
            name = "Manhattan";
        }

        // The minimal length of any axis (width, breadth, height) of a block.
        // Must not exceed 1.0f.
        protected const float minimalLength = 0.1f;

        // The maximal height of all blocks. If edges are drawn above the blocks, they
        // will be drawn somewhat above this value relative to the blocks ground.
        private float maxBlockHeight = 0.0f;

        /// <summary>
        /// The position and scaling of a node game object as determined by a node layout.
        /// </summary>
        /// Note: Structs are value types and are copied on assignment. 
        public struct NodeTransform
        {
            public NodeTransform(Vector3 position, Vector3 scale)
            {
                this.position = position;
                this.scale = scale;
            }
            public Vector3 position;
            public Vector3 scale;
        }

        private Dictionary<Node, GameObject> CreateGameObjects(IList<Node> nodes)
        {
            Dictionary<Node, GameObject> result = new Dictionary<Node, GameObject>();

            foreach (Node node in nodes)
            {
                // We only draw leaves.
                if (node.IsLeaf())
                {
                    GameObject block = blockFactory.NewBlock();
                    block.name = node.LinkName;
                    
                    AttachNode(block, node);
                    // Scaled metric values for the dimensions.
                    Vector3 scale = new Vector3(scaler.GetNormalizedValue(node, widthMetric),
                                                scaler.GetNormalizedValue(node, heightMetric),
                                                scaler.GetNormalizedValue(node, breadthMetric));

                    // Scale according to the metrics.
                    blockFactory.ScaleBlock(block, scale);

                    if (showErosions)
                    {
                        AddErosionIssues(block);
                    }

                    result[node] = block;
                }
            }
            return result;
        }

        public Dictionary<GameObject, NodeTransform> Layout(Dictionary<Node, GameObject> gameNodes)
        {
            Dictionary<GameObject, NodeTransform> result = new Dictionary<GameObject, NodeTransform>();

            int numberOfBuildingsPerRow = (int)Mathf.Sqrt(gameNodes.Count);
            int column = 0;
            int row = 1;
            const float distanceBetweenBuildings = 1.0f;
            float maxZ = 0.0f;         // maximal depth of a building in a row
            float maxZinFirstRow = 0.0f; // the value of maxZ in the first row
            float positionX = 0.0f;    // co-ordinate in a column of the grid
            float positionZ = 0.0f;    // co-ordinate in a row of the grid
            float maxPositionX = 0.0f; // maximal value of any positionX

            // Draw all nodes on a grid. position
            foreach (var gameNode in gameNodes)
            {
                Node node = gameNode.Key;
                GameObject block = gameNode.Value;

                column++;
                if (column > numberOfBuildingsPerRow)
                {
                    // exceeded length of the square => start a new row
                    if (row == 1)
                    {
                        // we are about to start the first column in the second row;
                        // thus, we have seen a blocks in the first row and can set
                        // maxZinFirstRow accordingly
                        maxZinFirstRow = maxZ;
                    }
                    row++;
                    column = 1;
                    positionZ += maxZ + distanceBetweenBuildings;
                    maxZ = 0.0f;
                    if (positionX > maxPositionX)
                    {
                        maxPositionX = positionX;
                    }
                    positionX = 0.0f;
                }

                // size is independent of the sceneNode
                Vector3 size = blockFactory.GetSize(block);
                if (size.z > maxZ)
                {
                    maxZ = size.z;
                }
                if (size.y > maxBlockHeight)
                {
                    maxBlockHeight = size.y;
                }

                positionX += size.x / 2.0f;
                // The position is the center of a GameObject. We want all GameObjects
                // be placed at the same ground level 0.
                result[block] = new NodeTransform(new Vector3(positionX, groundLevel, positionZ), Vector3.one);

                positionX += size.x / 2.0f + distanceBetweenBuildings;

            }
            return result;
        }

        public void Apply(Dictionary<GameObject, NodeTransform> layout)
        {
            foreach (var entry in layout)
            {
                GameObject block = entry.Key;
                NodeTransform transform = entry.Value;
                // Note: We need to first scale a block and only then set its position
                // because the scaling behavior differs between Cubes and CScape buildings.
                // Cubes scale from its center up and downward, which CScape buildings
                // scale only up.
                blockFactory.ScaleBlock(block, transform.scale);
                blockFactory.SetGroundPosition(block, transform.position);
            }
        }

        public override void Draw(Graph graph)
        {
            gameNodes = CreateGameObjects(graph.Nodes());
            Dictionary<GameObject, NodeTransform> layout = Layout(gameNodes);
            Apply(layout);
            BoundingBox(gameNodes.Values, out Vector2 leftFrontCorner, out Vector2 rightBackCorner);
            PlaneFactory.NewPlane(leftFrontCorner, rightBackCorner, groundLevel, Color.gray);
        }

        private void BoundingBox(ICollection<GameObject> gameNodes, out Vector2 leftLowerCorner, out Vector2 rightUpperCorner)
        {
            if (gameNodes.Count == 0)
            {
                leftLowerCorner = Vector2.zero;
                rightUpperCorner = Vector2.zero;
            }
            else
            {
                leftLowerCorner = new Vector2(Mathf.Infinity, Mathf.Infinity);
                rightUpperCorner = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);

                foreach (GameObject go in gameNodes)
                {
                    // Note: go.transform.position denotes the center of the object
                    Vector3 extent = blockFactory.GetSize(go) / 2.0f;
                    Vector3 position = blockFactory.GetCenterPosition(go);
                    {
                        // x co-ordinate of lower left corner
                        float x = position.x - extent.x;
                        if (x < leftLowerCorner.x)
                        {
                            leftLowerCorner.x = x;
                        }
                    }
                    {
                        // z co-ordinate of lower left corner
                        float z = position.z - extent.z;
                        if (z < leftLowerCorner.y)
                        {
                            leftLowerCorner.y = z;
                        }
                    }
                    {   // x co-ordinate of upper right corner
                        float x = position.x + extent.x;
                        if (x > rightUpperCorner.x)
                        {
                            rightUpperCorner.x = x;
                        }
                    }
                    {
                        // z co-ordinate of upper right corner
                        float z = position.z + extent.z;
                        if (z > rightUpperCorner.y)
                        {
                            rightUpperCorner.y = z;
                        }
                    }
                }
            }
        }
    }
}