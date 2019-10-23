﻿using SEE.DataModel;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Layout
{

    /// <summary>
    /// This layout packs circles closely together to decrease total area of city.
    /// </summary>
    public class CirclePackingLayout : ILayout
    {
        /*
         * CIRCLE PACKING LAYOUT
         * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        */

        private GameObject RootNodes;
        private GameObject RootEdges;

        private readonly string[] InnerNodeMetrics;

        public static Vector3 LevelUnit;

        public CirclePackingLayout(bool showEdges,
                             string widthMetric, string heightMetric, string breadthMetric,
                             SerializableDictionary<string, IconFactory.Erosion> issueMap,
                             string[] innerNodeMetrics,
                             BlockFactory blockFactory,
                             IScale scaler,
                             float edgeWidth,
                             bool showErosions,
                             bool edgesAboveBlocks,
                             bool showDonuts)
        : base(showEdges, widthMetric, heightMetric, breadthMetric, issueMap, blockFactory, scaler, edgeWidth, showErosions, edgesAboveBlocks)
        {
            name = "Circle Packing";
            ShowDonuts = showDonuts;
            InnerNodeMetrics = innerNodeMetrics;
        }

        private readonly bool ShowDonuts;

        /*
         * NODES
         * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        */

        protected override void DrawNodes(Graph graph)
        {
            LevelUnit = Vector3.zero;
            graph.SortHierarchyByName();
            RootNodes = new GameObject("Nodes");
            RootNodes.tag = Tags.Node;
            List<Node> roots = graph.GetRoots();
            DrawNodes(RootNodes, roots, out float out_radius);
            DrawPlane(RootNodes, out_radius);
        }

        private void DrawNodes(GameObject parent, List<Node> nodes, out float out_radius)
        {
            List<Circle> circles = new List<Circle>(nodes.Count);

            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];

                GameObject gameObject;

                float radius;
                if (node.IsLeaf())
                {
                    gameObject = DrawLeaf(node, out float out_leaf_radius);
                    radius = out_leaf_radius;
                }
                else
                {
                    gameObject = new GameObject(node.LinkName);      
                    DrawNodes(gameObject, node.Children(), out float out_nodes_radius);
                    radius = out_nodes_radius;
                }
                gameObject.tag = Tags.Node;
                gameObject.AddComponent<NodeRef>().node = node;
                gameObject.transform.parent = parent.transform;

                float radians = ((float)i / (float)nodes.Count) * (2.0f * Mathf.PI);
                gameObject.transform.localPosition = new Vector3(Mathf.Cos(radians), 0.0f, Mathf.Sin(radians)) * radius;
                gameObject.transform.position = gameObject.transform.position + new Vector3(0.0f, 0.1f, 0.0f);
                circles.Add(new Circle(gameObject.transform, radius));
            }

            Vector3 position = parent.transform.position;
            parent.transform.position = position;

            CirclePacker.Pack(circles, out float out_outer_radius);
            if (circles.Count > 1)
            {
                DrawOutline(parent, ref out_outer_radius);
            }
            out_radius = out_outer_radius;
        }

        private GameObject DrawLeaf(Node node, out float out_leaf_radius)
        {
            GameObject block = blockFactory.NewBlock();
            gameNodes[node] = block;
            
            block.name = node.LinkName + " Block";
            blockFactory.ScaleBlock(block, GetScale(node));
            Vector3 size = blockFactory.GetSize(block);
            blockFactory.SetLocalPosition(block, new Vector3(0.0f, size.y / 2.0f, 0.0f));
            out_leaf_radius = Mathf.Sqrt(size.x * size.x + size.z * size.z);

            if (showErosions)
            {
                AddErosionIssues(node);
            }
    
            LevelUnit.y = Mathf.Max(LevelUnit.y, size.y);

            return block;
        }

        private void DrawOutline(GameObject parent, ref float radius)
        {
            if (ShowDonuts)
            {
                AddDonut(parent, ref radius);
            }
            else
            {
                AttachCircleLine(parent, ref radius);
            }
        }

        // FIXME: Unify with BallonLayout.AttachCircleLine
        private void AttachCircleLine(GameObject parent, ref float radius)
        {
            GameObject circle = new GameObject(parent.name + " Border");
            circle.tag = Tags.Node;
            circle.transform.parent = parent.transform;

            // Number of line segments constituting the circle
            const int segments = 360;

            LineRenderer line = circle.AddComponent<LineRenderer>();

            LineFactory.SetDefaults(line);
            LineFactory.SetColor(line, Color.white);

            // line width is relative to the radius
            float lineWidth = radius / 100.0f;
            LineFactory.SetWidth(line, lineWidth);

            // We want to set the points of the circle lines relative to the game object.
            line.useWorldSpace = false;

            line.sharedMaterial = new Material(defaultLineMaterial);

            line.positionCount = segments + 1;
            const int pointCount = segments + 1; // add extra point to make startpoint and endpoint the same to close the circle
            Vector3[] points = new Vector3[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                float rad = Mathf.Deg2Rad * (i * 360f / segments);
                points[i] = new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);
            }
            line.SetPositions(points);
        }

        // FIXME: Unify with BallonLayout.AddDonut
        private void AddDonut(GameObject parent, ref float radius)
        {
            GameObject donut = new GameObject(parent.name + " Donut");
            donut.tag = Tags.Node;
            donut.transform.parent = parent.transform;

            float innerValue = UnityEngine.Random.Range(0.0f, 1.0f);
            float m1 = UnityEngine.Random.Range(0.0f, 50.0f);
            float m2 = UnityEngine.Random.Range(0.0f, 90.0f);
            float m3 = UnityEngine.Random.Range(0.0f, 150.0f);
            float m4 = UnityEngine.Random.Range(0.0f, 200.0f);

            const float innerScale = 0.95f;
            radius += (1.0f - innerScale) * radius;
            new DonutFactory(InnerNodeMetrics).DonutChart(donut, radius, innerValue, new float[] { m1, m2, m2, m3 }, innerScale);
        }

        private Vector3 GetScale(Node node)
        {
            return new Vector3(scaler.GetNormalizedValue(node, widthMetric), 
                               scaler.GetNormalizedValue(node, heightMetric), 
                               scaler.GetNormalizedValue(node, breadthMetric)); ;
        }

        private void DrawPlane(GameObject parent, float maxRadius)
        {
            const float enlargementFactor = 1.12f;

            // We put the circle into a square somewhat larger than what is necessary
            float widthAndDepth = 2.0f * maxRadius * enlargementFactor;

            GameObject plane = PlaneFactory.NewPlane(parent.transform.position, Color.gray, widthAndDepth, widthAndDepth);
            plane.transform.parent = parent.transform;
        }

        /*
         * EDGES
         * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        */

        protected override void DrawEdges(Graph graph)
        {
            RootEdges = new GameObject("Edges");
            RootEdges.tag = Tags.Edge;

            List<Edge> edges = graph.Edges();

            Material edgeMaterial = new Material(defaultLineMaterial);
            if (edgeMaterial == null)
            {
                Debug.LogError("Could not find material " + materialPath + "\n");
                return;
            }

            for (int i = 0; i < edges.Count; i++)
            {
                Edge edge = edges[i];
                Node source = edge.Source;
                Node target = edge.Target;
                Vector3 sourcePosition = blockFactory.Roof(gameNodes[source]);
                Vector3 targetPosition = blockFactory.Roof(gameNodes[target]);

                GameObject gameObject = new GameObject(edge.Type + "(" + source.LinkName + ", " + target.LinkName + ")");
                gameObject.tag = Tags.Edge;
                gameObject.AddComponent<EdgeRef>().edge = edge;
                gameObject.transform.parent = RootEdges.transform;

                Vector3[] controlPoints = new Vector3[] {
                    sourcePosition,
                    Vector3.Lerp(sourcePosition, targetPosition, 0.3f) + LevelUnit,
                    Vector3.Lerp(sourcePosition, targetPosition, 0.7f) + LevelUnit,
                    targetPosition
                };
                BSplineFactory.Draw(gameObject, controlPoints, edgeWidth, edgeMaterial);
            }
        }

    }

}// namespace SEE.Layout