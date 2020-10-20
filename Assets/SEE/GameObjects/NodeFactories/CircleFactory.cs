﻿using SEE.Game;
using UnityEngine;

namespace SEE.GO
{
    /// <summary>
    /// A factory for circle inner game objects.
    /// </summary>
    public class CircleFactory : LineInnerNodeFactory
    {
        /// <summary>
        /// Constructor allowing to set the initial unit for the width of the lines that render this inner node.
        /// Every line width passed as a parameter to methods of this class will be multiplied by this factor
        /// for the actual rendering.
        /// </summary>
        /// <param name="colorRange">the color range of the created objects</param>
        /// <param name="unit">initial unit for the width of all lines</param>
        public CircleFactory(ColorRange colorRange, float unit)
            : base(colorRange, unit)
        {
            material = Materials.New(Materials.ShaderType.TransparentLine, colorRange.upper);
        }

        /// <summary>
        /// The material we use for the circle lines.
        /// </summary>
        private readonly Material material;

        /// <summary>
        /// The default radius of a circle if none is given.
        /// </summary>
        private const float defaultRadius = 0.5f;

        public override GameObject NewBlock(int index = 0, int renderQueueOffset = 0)
        {
            GameObject result = new GameObject();
            AttachCircleLine(result, defaultRadius, Unit * defaultLineWidth, material.color);
            result.AddComponent<MeshCollider>();
            return result;
        }

        /// <summary>
        /// Attaches a circle line on given circle object.
        /// </summary>
        /// <param name="circle">object to which to attach the circle line</param>
        /// <param name="radius">radius of the circle</param>
        /// <param name="lineWidth">width of the circle line</param>
        /// <param name="color">color of the circle line</param>
        private void AttachCircleLine(GameObject circle, float radius, float lineWidth, Color color)
        {
            // Number of line segments constituting the circle
            const int segments = 360;

            LineRenderer line = circle.AddComponent<LineRenderer>();

            LineFactory.SetDefaults(line);
            LineFactory.SetColor(line, color);
            LineFactory.SetWidth(line, lineWidth);

            // We want to set the points of the circle lines relative to the game object.
            // If the containing object moves, the line renderer should move along with it.
            line.useWorldSpace = false;

            // All circles lines have the same material to reduce the number of drawing calls.
            line.sharedMaterial = material;

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

        /// <summary>
        /// Returns the size of the game object generated by this factory.
        /// Precondition: The given game object must have been generated by this factory.
        /// </summary>
        /// <param name="gameObject">game object whose size is to be returned</param>
        /// <returns>size of the block</returns>
        public override Vector3 GetSize(GameObject gameObject)
        {
            // Nodes represented by cubes have a renderer from which we can derive the
            // extent.
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                // IMPORTANT NOTE: For some unknown strange reason, the extent of circles
                // is actually double the actual space it consumes. That is why we devide
                // it by two here.
                return renderer.bounds.size / 2.0f;
            }
            else
            {
                Debug.LogErrorFormat("Node {0} (tag: {1}) without renderer.\n", gameObject.name, gameObject.tag);
                return Vector3.one;
            }
        }
    }
}