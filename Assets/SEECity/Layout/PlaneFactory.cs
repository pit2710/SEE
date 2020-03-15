﻿using SEE.DataModel;
using System;
using UnityEngine;

namespace SEE.Layout
{
    /// <summary>
    /// A factory for planes where blocks can be put on.
    /// </summary>
    internal class PlaneFactory
    {
        /// <summary>
        /// Returns a newly created plane at centerPosition with given color, width, depth, and height.
        /// </summary>
        /// <param name="centerPosition">center position of the plane</param>
        /// <param name="color">color of the plane</param>
        /// <param name="width">width of the plane (x axis)</param>
        /// <param name="depth">depth of the plane (z axis)</param>
        /// <param name="height">height of the plane (y axis)</param>
        /// <returns></returns>
        public static GameObject NewPlane(Vector3 centerPosition, Color color, float width, float depth, float height = 1.0f)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.tag = Tags.Decoration;
            plane.transform.position = centerPosition;

            Renderer planeRenderer = plane.GetComponent<Renderer>();
            planeRenderer.sharedMaterial = new Material(planeRenderer.sharedMaterial);
            planeRenderer.sharedMaterial.color = color;

            // Turn off reflection of plane
            planeRenderer.sharedMaterial.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            planeRenderer.sharedMaterial.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
            planeRenderer.sharedMaterial.SetFloat("_SpecularHighlights", 0.0f);
            // To turn reflection on again, use (_SPECULARHIGHLIGHTS_OFF and _GLOSSYREFLECTIONS_OFF
            // work as toggle, there is no _SPECULARHIGHLIGHTS_ON and _GLOSSYREFLECTIONS_ON):
            //planeRenderer.sharedMaterial.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            //planeRenderer.sharedMaterial.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
            //planeRenderer.sharedMaterial.SetFloat("_SpecularHighlights", 1.0f);

            // A plane is a flat square with edges ten units long oriented in the XZ plane of the local 
            // coordinate space. Thus, the mesh of a plane is 10 times larger than its scale factors for X and Y. 
            // When we want a plane to have width 12 units, we need to devide the scale for the width 
            // by 1.2.
            const float planeMeshFactor = 10.0f;
            Vector3 planeScale = new Vector3(width, height * planeMeshFactor, depth) / planeMeshFactor;
            plane.transform.localScale = planeScale;

            return plane;
        }

        /// <summary>
        /// Returns a newly created plane with the two corners leftFrontCorner = (x0, z0) 
        /// and rightBackCorner = (x1, z1).
        /// 
        /// with given color, width, depth, and height.
        /// Draws a plane in rectangle with  at ground level.
        /// 
        /// Preconditions: x0 < x1 and z0 < z1 (Exception is thrown otherwise)
        /// </summary>
        /// <param name="leftFrontCorner">2D co-ordinate of the left front corner</param>
        /// <param name="rightBackCorner">2D co-ordinate of the right back corner</param>
        /// <param name="groundLevel">y co-ordinate for the plane</param>
        /// <param name="color">color of the plane</param>
        /// <param name="height">height (thickness) of the plane</param>
        public static GameObject NewPlane(Vector2 leftFrontCorner, Vector2 rightBackCorner, float groundLevel, Color color, float height = 0.1f)
        {
            float width = Distance(leftFrontCorner.x, rightBackCorner.x);
            float depth = Distance(leftFrontCorner.y, rightBackCorner.y);

            Vector3 centerPosition = new Vector3(leftFrontCorner.x + width / 2.0f, groundLevel, leftFrontCorner.y + depth / 2.0f);
            return NewPlane(centerPosition, color, width, depth, height);
        }

        /// <summary>
        /// Returns the distance from v0 to v1.
        /// 
        /// Precondition: v1 > v0 (otherwise an Exception is thrown)
        /// </summary>
        /// <param name="v0">start position</param>
        /// <param name="v1">end position</param>
        /// <returns></returns>
        private static float Distance(float v0, float v1)
        {
            if (v1 <= v0)
            {
                Debug.AssertFormat(v1 > v0, "v1 > v0 expected. Actual v0 = {0}, v1 = {1}.\n", v0, v1);
                throw new Exception("v1 > v0 expected");
            }
            else
            {
                if (v0 < 0.0f)
                {
                    return v1 + Math.Abs(v0);
                }
                else
                {
                    return v1 - v0;
                }
            }
        }
    }
}
