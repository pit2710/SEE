﻿using SEE.DataModel;
using SEE.Layout;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.GO
{
    public class EdgeFactory
    {
        public EdgeFactory(IEdgeLayout layout, float edgeWidth)
        {
            this.layout = layout;
            this.edgeWidth = edgeWidth;
        }

        /// <summary>
        /// Path to the material used for edges.
        /// </summary>
        protected const string materialPath = "Legacy Shaders/Particles/Additive";
        // protected const string materialPath = "BrickTextures/BricksTexture13/BricksTexture13";
        // protected const string materialPath = "Particles/Standard Surface";

        /// <summary>
        /// The material used for edges.
        /// </summary>
        protected readonly Material defaultLineMaterial = LineMaterial();

        /// <summary>
        /// Returns the default material for edges using the materialPath.
        /// </summary>
        /// <returns>default material for edges</returns>
        private static Material LineMaterial()
        {
            Material material = new Material(Shader.Find(materialPath));
            if (material == null)
            {
                Debug.LogError("Could not find material " + materialPath + "\n");
            }
            return material;
        }


        protected readonly float edgeWidth;

        protected readonly IEdgeLayout layout;

        /// <summary>
        /// Returns a new game edge.
        /// </summary>
        /// <returns>new game edge</returns>
        protected GameObject NewGameEdge(LayoutEdge layoutEdge)
        {
            GameObject gameEdge = new GameObject
            {
                tag = Tags.Edge,
                isStatic = true,
                name = "(" + layoutEdge.Source.LinkName + ", " + layoutEdge.Target.LinkName + ")"
            };
            // FIXME: gameEdge.AddComponent<EdgeRef>().edge = edge;
            return gameEdge;
        }

        public ICollection<GameObject> DrawEdges(ICollection<ILayoutNode> layoutNodes)
        {
            List<GameObject> result = new List<GameObject>();

            foreach (LayoutEdge layoutEdge in layout.GetLines(layoutNodes))
            {
                GameObject gameEdge = NewGameEdge(layoutEdge);
                result.Add(gameEdge);

                // gameEdge does not yet have a renderer; we add a new one
                LineRenderer line = gameEdge.AddComponent<LineRenderer>();
                // use sharedMaterial if changes to the original material should affect all
                // objects using this material; renderer.material instead will create a copy
                // of the material and will not be affected by changes of the original material
                line.sharedMaterial = defaultLineMaterial;

                LineFactory.SetDefaults(line);
                LineFactory.SetWidth(line, edgeWidth);

                // If enabled, the lines are defined in world space.
                // This means the object's position is ignored, and the lines are rendered around 
                // world origin.
                line.useWorldSpace = true;

                Vector3[] points = layoutEdge.Points;
                line.positionCount = points.Length; // number of vertices
                line.SetPositions(points);

                // FIXME
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
            return result;
        }
    }
}