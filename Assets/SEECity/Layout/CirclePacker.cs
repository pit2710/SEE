﻿using System.Collections.Generic;
using UnityEngine;
using System;

namespace SEE.Layout
{

    /// <summary>
    /// A Circle can be used by the <see cref="CirclePacker"/>.
    /// </summary>
    public struct Circle
    {
        // The position of the transform will be changed by the circle packer.
        public Transform transform;

        public float radius;

        /// <summary>
        /// Creates a new circle at position within given transform and with given radius.
        /// </summary>
        /// <param name="transform">The transform with the position of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        public Circle(Transform transform, float radius)
        {
            this.transform = transform;
            this.radius = radius;
        }

        /// <summary>
        /// For debugging.
        /// </summary>
        /// <returns>string representation of the circle</returns>
        public override string ToString()
        {
            return "(center= " + transform.localPosition.ToString() + ", radius=" + radius + ")";
        }
    }
    
    /// <summary>
    /// This class holds a list of <see cref="Circle"/>-Objects and can pack them closely.
    /// The original source can be found <see href="https://www.codeproject.com/Articles/42067/D-Circle-Packing-Algorithm-Ported-to-Csharp">HERE</see>.
    /// </summary>
    public static class CirclePacker
    {
        /// <summary>
        /// Packs the <paramref name="circles"/> as close together within reasonable time.
        /// 
        /// Important note: the order of circles may have changed afterward.
        /// </summary>
        /// <param name="circles">The circles to be packed.</param>
        /// <param name="out_outer_radius">The radius of the appoximated minimal enclosing circle.</param>
        public static void Pack(List<Circle> circles, out float out_outer_radius)
        {
            Vector3 center = Vector3.zero;

            //if (circles.Count == 0)
            //{
            //    center = Vector3.zero;
            //    out_outer_radius = 0.0f;
            //}
            //else if (circles.Count == 1)
            //{
            //    center = Vector3.zero;
            //    circles[0].Transform.position = Vector3.zero;
            //    out_outer_radius = circles[0].Radius;
            //}
            //else if (circles.Count == 2)
            //{
            //    float r0 = circles[0].Radius;
            //    float r1 = circles[1].Radius;
            //    Debug.Assert(r0 >= r1);
            //    circles[0].Transform.position = Vector3.zero;
            //    circles[1].Transform.position = circles[0].Transform.position + (r0 + r1) * Vector3.right;
            //    // distance between centers of both circles (they are to touch each other)
            //    EnclosingCircleIntersectingCircles(circles, out center, out out_outer_radius);

            //    //{
            //    //    List<MyCircle> debugCircles = new List<MyCircle>();
            //    //    debugCircles.AddRange(circles);
            //    //    debugCircles.Add(new MyCircle(null, center, out_outer_radius));
            //    //    DrawCircles(debugCircles);
            //    //}
            //}
            //else
            {
                out_outer_radius = 0.0f;
                // Sort circles descendingly based on radius
                circles.Sort(Comparator);
                float last_out_radius = Mathf.Infinity;
                int max_iterations = 100; // FIXME: What would be a suitable number of maximal iterations? circles.Count?
                for (int it = 0; it < max_iterations; it++)
                {
                    // Each step draws all pairs of circles closer together.
                    for (int i = 0; i < circles.Count - 1; i++)
                    {
                        for (int j = i + 1; j < circles.Count; j++)
                        {
                            if (i == j)
                                continue;

                            Vector3 ab = circles[j].transform.localPosition - circles[i].transform.localPosition;
                            ab.y = 0.0f;
                            float r = circles[i].radius + circles[j].radius;
                            // Length squared = (dx * dx) + (dy * dy);
                            float d = Mathf.Max(0.0f, Vector3.SqrMagnitude(ab));

                            if (d < (r * r) - 0.01)
                            {
                                ab.Normalize();
                                ab *= (float)((r - Math.Sqrt(d)) * 0.5f);
                                circles[j].transform.localPosition += ab;
                                circles[i].transform.localPosition -= ab;
                            }
                        }
                    }

                    SmallestEnclosingCircle(circles, out Vector3 out_center, out float out_radius);
                    out_outer_radius = out_radius;
                    center = out_center;

                    float improvement = out_radius / last_out_radius;
                    if (last_out_radius != Mathf.Infinity && improvement < 1.01f)
                    {
                        // If the degree of improvement falls below 1%, we will stop.
                        Debug.LogFormat("Minor improvement of {0} after {1} iterations.\n", improvement, it);
                        break;
                    }
                    //else
                    //{
                    //    Debug.LogFormat("Improvement: {0}\n", improvement);
                    //}
                    last_out_radius = out_radius;
                }


            }
            // Clients of CirclePacker assume that all co-ordinates of children are relative to Vector3.zero.
            // SmallestEnclosingCircle() may have given us a different center. That is why we need to make
            // adjustments here by subtracting center as delivered by SmallestEnclosingCircle().
            for (int i = 0; i < circles.Count; i++)
            {
                circles[i].transform.localPosition -= center;
            }
        }

        /// <summary>
        /// Finds smallest circle that encloses <paramref name="circles"/>. To improve
        /// performance, <paramref name="circles"/> should be already sorted by radius in
        /// descending order.
        /// 
        /// The original sources can be found
        /// <see href="https://gist.github.com/mbostock/29c534ff0b270054a01c">HERE</see>/> and
        /// <see href="http://www.sunshine2k.de/coding/java/Welzl/Welzl.html">HERE</see>/>.
        /// </summary>
        /// 
        /// <param name="circles">The circles to be enclosed.</param>
        /// 
        /// <param name="out_center">The center of <paramref name="circles"/> enclosing
        /// circle.</param>
        /// 
        /// <param name="out_radius">The radius of <paramref name="circles"/> enclosing
        /// circle.</param>
        private static void SmallestEnclosingCircle(List<Circle> circles, out Vector3 out_center, out float out_radius)
        {
            SmallestEnclosingCircleImpl(new List<Circle>(circles), new List<Circle>(), out Vector3 center, out float radius);
            out_center = center;
            out_radius = radius;
        }

        /// <summary>
        /// Implementation of
        /// <see cref="SmallestEnclosingCircle(List{Circle}, out Vector3, out float)"/>
        /// .
        /// </summary>
        /// 
        /// <param name="circles">The circles to be enclosed.</param>
        /// 
        /// <param name="borderCircles">The circles that currently represent the border.
        /// <code>borderCircles.Count</code> is always less than or equal to 3.</param>
        /// 
        /// <param name="out_center">The center of <paramref name="borderCircles"/> enclosing
        /// circle.</param>
        /// 
        /// <param name="out_radius">The radius of <paramref name="borderCircles"/> enclosing
        /// circle.</param>
        private static void SmallestEnclosingCircleImpl(List<Circle> circles, List<Circle> borderCircles, out Vector3 out_center, out float out_radius)
        {
            out_center = Vector3.zero;
            out_radius = 0.0f;

            if (circles.Count == 0 || borderCircles.Count > 0 && borderCircles.Count > 3)
            {
                switch (borderCircles.Count)
                {
                    case 1:
                        {
                            out_center = borderCircles[0].transform.position;
                            out_radius = borderCircles[0].radius;
                            break;
                        }
                    case 2:
                        {
                            CircleIntersectingTwoCircles(borderCircles[0], borderCircles[1], out Vector3 out_center_trivial, out float out_radius_trivial);
                            out_center = out_center_trivial;
                            out_radius = out_radius_trivial;
                            break;
                        }
                    case 3:
                        {
                            CircleIntersectingThreeCircles(borderCircles[0], borderCircles[1], borderCircles[2], out Vector3 out_center_trivial, out float out_radius_trivial);
                            out_center = out_center_trivial;
                            out_radius = out_radius_trivial;
                            break;
                        }
                }
                out_center.y = 0.0f;

                if (circles.Count == 0)
                {
                    return;
                }
            }

            // This is the smallest circle, if circles are sorted by descending radius
            int smallestCircleIndex = circles.Count - 1;
            Circle smallestCircle = circles[smallestCircleIndex];

            List<Circle> cmc = new List<Circle>(circles);
            cmc.RemoveAt(smallestCircleIndex);

            SmallestEnclosingCircleImpl(cmc, borderCircles, out Vector3 out_center_cmc, out float out_radius_cmc);

            if (!CircleContainsCircle(out_center_cmc, out_radius_cmc, smallestCircle))
            {
                List<Circle> bcpc = new List<Circle>(borderCircles);
                bcpc.Add(smallestCircle);

                SmallestEnclosingCircleImpl(cmc, bcpc, out Vector3 out_center_cmc_bcpc, out float out_radius_cmc_bcpc);

                out_center = out_center_cmc_bcpc;
                out_radius = out_radius_cmc_bcpc;
            }
            else
            {
                out_center = out_center_cmc;
                out_radius = out_radius_cmc;
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if circle with <paramref name="position"/> and
        /// <paramref name="radius"/> contains <paramref name="circle"/>.
        /// </summary>
        /// 
        /// <param name="position">Position of containing circle.</param>
        /// <param name="radius">Radius of containing circle.</param>
        /// <param name="circle">Contained circle.</param>
        /// <returns></returns>
        private static bool CircleContainsCircle(Vector3 position, float radius, Circle circle)
        {
            var xc0 = position.x - circle.transform.position.x;
            var yc0 = position.z - circle.transform.position.z;
            return Mathf.Sqrt(xc0 * xc0 + yc0 * yc0) < radius - circle.radius + float.Epsilon;
        }

        /// <summary>
        /// Calculates smallest enclosing circle of <paramref name="c1"/> and
        /// <paramref name="c2"/>.
        /// </summary>
        /// 
        /// <param name="c1">First circle.</param>
        /// <param name="c2">Second circle.</param>
        /// <param name="out_center">Center of smallest enclosing circle.</param>
        /// <param name="out_radius">Radius of smallest enclosing circle.</param>
        private static void CircleIntersectingTwoCircles(Circle c1, Circle c2, out Vector3 out_center, out float out_radius)
        {
            Vector3 c12 = c2.transform.position - c1.transform.position;
            float r12 = c2.radius - c1.radius;
            float l = c12.magnitude;
            out_center = (c1.transform.position + c2.transform.position + c12 / l * r12) / 2.0f;
            out_radius = (l + c1.radius + c2.radius) / 2.0f;
        }

        /// <summary>
        /// Calculates smallest enclosing circle of <paramref name="c1"/>,
        /// <paramref name="c2"/> and <paramref name="c3"/>.
        /// </summary>
        /// 
        /// <param name="c1">First circle.</param>
        /// <param name="c2">Second circle.</param>
        /// <param name="c3">Third circle.</param>
        /// <param name="out_center">Center of smallest enclosing circle.</param>
        /// <param name="out_radius">Radius of smallest enclosing circle.</param>
        private static void CircleIntersectingThreeCircles(Circle c1, Circle c2, Circle c3, out Vector3 out_center, out float out_radius)
        {
            Vector2 p0 = new Vector2(c1.transform.position.x, c1.transform.position.z);
            Vector2 p1 = new Vector2(c2.transform.position.x, c2.transform.position.z);
            Vector2 p2 = new Vector2(c3.transform.position.x, c3.transform.position.z);

            float r0 = c1.radius;
            float r1 = c2.radius;
            float r2 = c3.radius;

            Vector2 a0 = 2.0f * (p0 - p1);
            float a1 = 2.0f * (r1 - r0);
            float a2 = p0.SqrMagnitude() - r0 * r0 - p1.SqrMagnitude() + r1 * r1;

            Vector2 b0 = 2.0f * (p0 - p2);
            float b1 = 2.0f * (r2 - r0);
            float b2 = p0.SqrMagnitude() - r0 * r0 - p2.SqrMagnitude() + r2 * r2;

            float det = b0.x * a0.y - a0.x * b0.y;

            float cx = (a0.y * b2 - b0.y * a2) / det - p1.x;
            float cy = -(a0.x * b2 - b0.x * a2) / det - p1.y;
            float dx = (b0.y * a1 - a0.y * b1) / det;
            float dy = -(b0.x * a1 - a0.x * b1) / det;

            float e1 = dx * dx + dy * dy - 1.0f;
            float e2 = 2.0f * (cx * dx + cy * dy + r1);
            float e3 = cx * cx + cy * cy - r1 * r1;

            out_radius = (-e2 - Mathf.Sqrt(e2 * e2 - 4.0f * e1 * e3)) / (2.0f * e1);
            out_center = new Vector3(cx + dx * out_radius + p1.x, 0.0f, cy + dy * out_radius + p1.y);
        }

        /// <summary>
        /// Compares <paramref name="c1"/> and <paramref name="c2"/> by radius (descending).
        /// </summary>
        /// <param name="c1">First circle.</param>
        /// <param name="c2">Second circle.</param>
        /// <returns></returns>
        private static int Comparator(Circle c1, Circle c2)
        {
            float r1 = c1.radius;
            float r2 = c2.radius;
            if (r1 < r2)
                return 1;
            else if (r1 > r2)
                return -1;
            else return 0;
        }
    }

}// namespace SEE.Layout