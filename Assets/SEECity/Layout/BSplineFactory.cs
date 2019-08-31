﻿using System.Collections.Generic;
using UnityEngine;

namespace SEE.Layout
{
    /// <summary>
    /// Creates BSplines using control points.
    /// </summary>
    class BSplineFactory
    {
        /// <summary>
        /// Serializes the co-ordinates of all given vectors as a list.
        /// E.g., The list {(1.0, 2.0, 3.0), (4.0, 5.0, 6.0)} is serialized 
        /// into {1.0, 2.0, 3.0, 4.0, 5.0, 6.0}.
        /// </summary>
        /// <param name="vectors">vectors to be serialized</param>
        /// <returns>serialized coordindates of given vectors</returns>
        private static IList<double> VectorsToList(IList<Vector3> vectors)
        {
            List<double> result = new List<double>();
            foreach (Vector3 vector in vectors)
            {
                result.Add(vector.x);
                result.Add(vector.y);
                result.Add(vector.z);
            }
            return result;
        }

        /// <summary>
        /// Deserializes the given co-oordindates back into 3D vectors.
        /// E.g., The list {1.0, 2.0, 3.0, 4.0, 5.0, 6.0} is deserialized 
        /// into {(1.0, 2.0, 3.0), (4.0, 5.0, 6.0)}.
        /// </summary>
        /// <param name="values">co-ordinates to be deserialized</param>
        /// <returns>Deserialized vectors having the given co-ordinates</returns>
        /*
        private static IList<Vector3> ListToVectors(IList<double> values)
        {
            List<Vector3> result = new List<Vector3>();

            int i = 0;
            // Random value; this value will not never be added based on the
            // logic of the loop, but the compiler forces us to initialize v.
            Vector3 v = Vector3.zero; 
            foreach (double value in values)
            {
                switch(i)
                    {
                    case 0:
                        v = new Vector3();
                        v.x = (float)value;
                        i++;
                        break;
                    case 1:
                        v.y = (float)value;
                        i++;
                        break;
                    case 2:
                        v.z = (float)value;
                        result.Add(v);
                        i = 0;
                        break;
                }
            }
            return result;
        }
        */

        /// <summary>
        /// Deserializes the given co-oordindates back into 3D vectors.
        /// E.g., The list [1.0, 2.0, 3.0, 4.0, 5.0, 6.0] is deserialized 
        /// into [(1.0, 2.0, 3.0), (4.0, 5.0, 6.0)].
        /// </summary>
        /// <param name="values">co-ordinates to be deserialized</param>
        /// <returns>Deserialized vectors having the given co-ordinates</returns>
        private static Vector3[] ListToVectors(IList<double> values)
        {
            Vector3[] result = new Vector3[values.Count / dimensions];

            int i = 0;
            int next = 0;
            // Random value; this value will not never be added based on the
            // logic of the loop, but the compiler forces us to initialize v.
            Vector3 v = Vector3.zero;
            foreach (double value in values)
            {
                switch (i % dimensions)
                {
                    case 0:
                        v = new Vector3();
                        v.x = (float)value;
                        break;
                    case 1:
                        v.y = (float)value;
                        break;
                    case 2:
                        v.z = (float)value;
                        result[next] = v;
                        next++;
                        break;
                }
                i++;
            }
            return result;
        }

        private const int dimensions = 3;

        private static TinySpline.BSpline Spline(IList<Vector3> controlPoints)
        {
            // Create a cubic spline with 7 control points in 3D using
            // a clamped knot vector. This call is equivalent to:
            // BSpline spline = new BSpline(7, 2, 3, BSplineType.CLAMPED);
            TinySpline.BSpline spline = new TinySpline.BSpline(7, dimensions);

            // Setup control points.
            IList<double> ctrlp = spline.controlPoints;
            return spline;
        }

        public static void Draw(GameObject edge, Vector3[] controlPoints, Material material = null)
        {
            // Create a cubic spline with control points in 3D using a clamped knot vector.
            TinySpline.BSpline spline = new TinySpline.BSpline((uint)controlPoints.Length, dimensions);

            // Setup control points.
            spline.controlPoints = VectorsToList(controlPoints);

            IList<double> list = spline.buckle(0.5f).sample();

            Vector3[] splinePoints = ListToVectors(list);

            LineRenderer line = edge.GetComponent<LineRenderer>();
            if (line == null)
            {
                // edge does not yet have a renderer; we add a new one
                line = edge.AddComponent<LineRenderer>();
            }
            line.useWorldSpace = true;
            if (material != null)
            {
                // use sharedMaterial if changes to the original material should affect all
                // objects using this material; renderer.material instead will create a copy
                // of the material and will not be affected by changes of the original material
                line.sharedMaterial = material;
            }
            line.positionCount = splinePoints.Length; // number of vertices       
            line.SetPositions(splinePoints);
            LineFactory.SetDefaults(line);
            LineFactory.SetWidth(line, 0.1f);
            LineFactory.SetColors(line);
        }

        public static void Foo()
        {
            Vector3[] controlPoints = new Vector3[] { new Vector3(2.0f, 5.0f, 2.0f),
                                               new Vector3(5.0f, 5.0f, 3.0f),
                                               new Vector3(8.0f, 5.0f, 10.0f),
                                               new Vector3(2.0f, 5.0f, 19.0f),
                                               new Vector3(-10.0f, 5.0f, 23.0f),
                                               new Vector3(-22.0f, 5.0f, 5.0f)};

            GameObject edge = new GameObject();
            edge.name = "BSpline";
            string materialPath = "Legacy Shaders/Particles/Additive";
            // string materialPath = "BrickTextures/BricksTexture13/BricksTexture13";
            // string materialPath = "Particles/Standard Surface";

            //Material newMat = Resources.Load<Material>(materialPath);
            Material newMat = new Material(Shader.Find(materialPath));
            if (newMat == null)
            {
                Debug.LogError("Could not find material " + materialPath + "\n");
                return;
            }
            Draw(edge, controlPoints, newMat);
        }
    }
}
