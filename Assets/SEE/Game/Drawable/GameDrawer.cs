﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.SEE.Game;
using Assets.SEE.Game.Drawable;
using RTG;
using SEE.DataModel.DG;
using SEE.Game.City;
using SEE.GO;
using SEE.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SEE.Game
{
    /// <summary>
    /// Draws a new line on a drawable or deleting these again.
    /// </summary>
    public static class GameDrawer
    {
        /// <summary>
        /// The renderer used to draw the line.
        /// </summary>
        private static LineRenderer renderer;

        /// <summary>
        /// The collider of the line.
        /// </summary>
        private static MeshCollider meshCollider;

        private static GameObject line;

        private static void Setup(GameObject drawable, String name, Vector3[] positions, Color color, float thickness)
        {
            line = new (name);
            line.tag = Tags.Line;
            line.transform.parent = drawable.transform;
            renderer = line.AddComponent<LineRenderer>();
            meshCollider = line.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            renderer.sharedMaterial = GetMaterial(color);
            renderer.alignment = LineAlignment.TransformZ;
            renderer.startWidth = thickness;
            renderer.endWidth = renderer.startWidth;
            renderer.useWorldSpace = true;
            renderer.positionCount = positions.Length;
        }

        public static GameObject StartDrawing(GameObject drawable, Vector3[] positions, Color color, float thickness)
        {
            Setup(drawable, "", positions, color, thickness);
            line.name = "line" + line.GetInstanceID();
            renderer.sortingOrder = DrawableConfigurator.orderInLayer;
            DrawableConfigurator.orderInLayer++;
            
            return line;
        }

        /// <summary>
        ///  Draws the line given the <see cref="positions"/>.
        /// </summary>
        public static void Drawing(Vector3[] positions)
        {
            renderer.positionCount = positions.Length;
            renderer.SetPositions(positions);
        }

        public static void FinishDrawing()
        {
            Mesh mesh = new();
            renderer.BakeMesh(mesh, true);
            meshCollider.sharedMesh = mesh;
        }

        public static GameObject DrawLine(GameObject drawable, String name, Vector3[] positions, Color color, float thickness)
        {
            if (drawable.transform.Find(name) != null)
            {
                line = GameDrawableIDFinder.FindChild(drawable, name);
                renderer = line.GetComponent<LineRenderer>();
                meshCollider = line.GetComponent<MeshCollider>();
                Drawing(positions);
                FinishDrawing();
            }
            else
            {
                Setup(drawable, name, positions, color, thickness);
                renderer.SetPositions(positions);
                renderer.sortingOrder = DrawableConfigurator.orderInLayer;
                DrawableConfigurator.orderInLayer++;
                FinishDrawing();
            }
            return line;
        }

        public static GameObject ReDrawLine(GameObject drawable, String name, Vector3[] positions, Color color, float thickness, int orderInLayer)
        {
            Setup(drawable, name, positions, color, thickness);
            renderer.SetPositions(positions);
            renderer.sortingOrder = orderInLayer;
            FinishDrawing();

            return line;
        }

        public static int DifferentPositionCounter(Vector3[] positions)
        {
            List<Vector3> positionsList = new List<Vector3>(positions);
            return positionsList.Distinct().ToList().Count;
        }

        private static Material GetMaterial(Color color)
        {
            ColorRange colorRange = new ColorRange(color, color, 1);
            Materials materials = new Materials(Materials.ShaderType.DrawableLine, colorRange);
            Material material = materials.Get(0, 0);
            return material;
        }
    }
}
