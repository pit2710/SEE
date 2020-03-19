﻿using System;
using System.Collections.Generic;
using SEE.DataModel;
using UnityEngine;

namespace SEE.Layout
{
    /// <summary>
    /// Draws edges as straight lines at either above or below the game nodes.
    /// </summary>
    public class StraightEdgeLayout : IEdgeLayout
    {
        public StraightEdgeLayout(NodeFactory blockFactory, float edgeWidth, bool edgesAboveBlocks) 
            : base(blockFactory, edgeWidth, edgesAboveBlocks)
        {
        }

        public override ICollection<GameObject> DrawEdges(Graph graph, ICollection<GameObject> nodes)
        {
            List<GameObject> result = new List<GameObject>();

            SetGameNodes(nodes);
            float maxBlockHeight = GetMaxBlockHeight(nodes);

            // The offset of the edges above or below the ground chosen relative 
            // to the height of the largest block.
            // This offset is used to draw the line somewhat below
            // or above the house (depending on the orientation).
            float offset = blockFactory.Unit; // must be positive
            // The level at which edges are drawn. This value is used only if the
            // edges are to be drawn above the blocks.
            float edgeLevel = maxBlockHeight + offset;

            //Material newMat = Resources.Load<Material>(materialPath);
            Material newMat = new Material(defaultLineMaterial);
            if (newMat == null)
            {
                Debug.LogError("Could not find material " + materialPath + "\n");
                return result;
            }

            foreach (Edge edge in graph.ConnectingEdges(gameNodes.Keys))
            {
                Node source = edge.Source;
                Node target = edge.Target;
                if (source != null && target != null)
                {
                    GameObject gameEdge = NewGameEdge(edge);
                    result.Add(gameEdge);

                    // gameEdge does not yet have a renderer; we add a new one
                    LineRenderer line = gameEdge.AddComponent<LineRenderer>();
                    // use sharedMaterial if changes to the original material should affect all
                    // objects using this material; renderer.material instead will create a copy
                    // of the material and will not be affected by changes of the original material
                    line.sharedMaterial = defaultLineMaterial;

                    LineFactory.SetDefaults(line);
                    LineFactory.SetWidth(line, edgeWidth * blockFactory.Unit);

                    // If enabled, the lines are defined in world space.
                    // This means the object's position is ignored, and the lines are rendered around 
                    // world origin.
                    line.useWorldSpace = true;

                    GameObject sourceObject = gameNodes[source];
                    GameObject targetObject = gameNodes[target];

                    {
                        // define the points along the line
                        Vector3 sourceCenterToBorder = blockFactory.GetSize(sourceObject) / 2.0f;
                        Vector3 targetCenterToBorder = blockFactory.GetSize(targetObject) / 2.0f;
                        line.positionCount = 4; // number of vertices
                        Vector3[] points = new Vector3[line.positionCount];

                        if (edgesAboveBlocks)
                        {
                            points[0] = blockFactory.Roof(sourceObject);
                            points[3] = blockFactory.Roof(targetObject);

                            points[1] = blockFactory.Ground(sourceObject);
                            points[1].y = edgeLevel;
                            points[2] = blockFactory.Ground(targetObject);
                            points[2].y = edgeLevel;
                        }
                        else
                        {
                            points[0] = blockFactory.Ground(sourceObject);
                            points[3] = blockFactory.Ground(targetObject);

                            // position below/above starting position
                            points[1] = points[0];
                            points[1].y -= offset;

                            // position below/above ending position
                            points[2] = points[3];
                            points[2].y -= offset;
                        }

                        line.SetPositions(points);

                        // put a capsule collider around the straight main line
                        // (the one from points[1] to points[2]
                        CapsuleCollider capsule = gameEdge.AddComponent<CapsuleCollider>();
                        capsule.radius = Math.Max(line.startWidth, line.endWidth) / 2.0f;
                        capsule.center = Vector3.zero;
                        capsule.direction = 2; // Z-axis for easier "LookAt" orientation
                        capsule.transform.position = points[1] + (points[2] - points[1]) / 2;
                        capsule.transform.LookAt(points[1]);
                        capsule.height = (points[2] - points[1]).magnitude;
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Edge of type {0} has a missing source or target.\n", edge.Type);
                }
            }
            return result;
        }
    }
}
