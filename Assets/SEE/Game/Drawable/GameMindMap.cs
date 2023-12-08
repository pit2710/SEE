﻿using SEE.Game;
using SEE.Game.Drawable;
using SEE.Game.Drawable.ActionHelpers;
using SEE.Game.Drawable.Configurations;
using SEE.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Assets.SEE.Game.Drawable.GameDrawer;

namespace Assets.SEE.Game.Drawable
{
    /// <summary>
    /// This class provides methods to create or recover mind map nodes.
    /// </summary>
    public static class GameMindMap
    {
        /// <summary>
        /// The different kinds of a mind map node.
        /// </summary>
        [Serializable]
        public enum NodeKind
        {
            Theme,
            Subtheme,
            Leaf
        }

        /// <summary>
        /// Get a list of the different node kinds.
        /// </summary>
        /// <returns>A list of the node kinds</returns>
        public static List<NodeKind> GetNodeKinds()
        {
            return Enum.GetValues(typeof(NodeKind)).Cast<NodeKind>().ToList();
        }

        /// <summary>
        /// Setups a mind map node.
        /// It adds the <see cref="Tags.MindMapNode"/> to the node. 
        /// It also creates the border and the text for the node and disables the collider of that.
        /// The box collider of the node will be calculated on the border size.
        /// Additionally, the node receives an <see cref="MMNodeValueHolder"/> component.
        /// </summary>
        /// <param name="drawable">The drawable on that the node should be displayed.</param>
        /// <param name="name">The id of the node</param>
        /// <param name="prefix">The id prefix.</param>
        /// <param name="writtenText">The displayed text of the node</param>
        /// <param name="position">The position for the node</param>
        /// <param name="node">The created node.</param>
        private static void Setup(GameObject drawable, string name, string prefix, string writtenText, Vector3 position, out GameObject node)
        {
            if (name.Length > 4)
            {
                node = new(name);
            }
            else
            {
                node = new("");
                name = prefix + node.GetInstanceID() + DrawableHolder.GetRandomString(4);
                while (GameFinder.FindChild(drawable, name) != null)
                {
                    name = prefix + node.GetInstanceID() + DrawableHolder.GetRandomString(4);
                }
                node.name = name;
            }
            
            GameObject highestParent, attachedObjects;
            DrawableHolder.Setup(drawable, out highestParent, out attachedObjects);

            node.tag = Tags.MindMapNode;
            node.transform.SetParent(attachedObjects.transform);
            node.transform.rotation = attachedObjects.transform.rotation;
            node.transform.position = position;

            GameObject text = CreateText(drawable, position, writtenText, prefix);
            GameObject border = CreateMindMapBorder(drawable, position, text, prefix);

            text.transform.SetParent(node.transform);
            border.transform.SetParent(node.transform);
            node.transform.position = position - node.transform.forward * ValueHolder.distanceToDrawable.z *
                            border.GetComponent<OrderInLayerValueHolder>().GetOrderInLayer();
            node.AddComponent<OrderInLayerValueHolder>().SetOrderInLayer(border.GetComponent<OrderInLayerValueHolder>().GetOrderInLayer());
            border.transform.localPosition = Vector3.zero;
            border.GetComponent<OrderInLayerValueHolder>().SetOrderInLayer(0);
            text.GetComponent<TextMeshPro>().sortingOrder = node.GetComponent<OrderInLayerValueHolder>().GetOrderInLayer();

            text.GetComponent<MeshCollider>().enabled = false;
            border.GetComponent<MeshCollider>().enabled = false;

            BoxCollider box = node.AddComponent<BoxCollider>();
            box.size = GetBoxSize(border);

            node.AddComponent<MMNodeValueHolder>();            
        }

        /// <summary>
        /// Creates the text for the mind map node (description)
        /// </summary>
        /// <param name="drawable">The drawable on that the node should be displayed.</param>
        /// <param name="position">The position for the text.</param>
        /// <param name="writtenText">The text (description) for the node</param>
        /// <param name="prefix">The id prefix, necessary for the font size.</param>
        /// <returns>The created text.</returns>
        private static GameObject CreateText(GameObject drawable, Vector3 position, string writtenText, string prefix)
        {
            TMPro.FontStyles fontStyles = TMPro.FontStyles.Normal;
            float fontSize = 0.7f;

            if (prefix == ValueHolder.MindMapThemePrefix)
            {
                fontStyles = TMPro.FontStyles.Bold | TMPro.FontStyles.Underline;
                fontSize = 1f;
            }
            else if (prefix == ValueHolder.MindMapLeafPrefix)
            {
                fontSize = 0.5f;
            }

            GameObject text = GameTexter.WriteText(drawable, writtenText, position, Color.black, Color.clear,
                ValueHolder.standardTextOutlineThickness, fontSize, 0, fontStyles);

            return text;
        }

        /// <summary>
        /// Creates the border of the mind map node.
        /// Themes receive an ellipse shape, sub-themes a rectangle, and leaves receive an invisible ellipse shape.
        /// </summary>
        /// <param name="drawable">The drawable on that the node should be displayed.</param>
        /// <param name="position">The position for the border.</param>
        /// <param name="text">The text object, necessary for the width/height calculation</param>
        /// <param name="prefix">The id prefix</param>
        /// <returns>The created border</returns>
        private static GameObject CreateMindMapBorder(GameObject drawable, Vector3 position, GameObject text, string prefix)
        {
            GameObject shape;
            GameDrawer.LineKind lineKind = GameDrawer.LineKind.Solid;
            Color lineColor = Color.black;
            bool ellipse = false;
            switch(prefix)
            {
                case ValueHolder.MindMapThemePrefix:
                    ellipse = true;
                    break;
                case ValueHolder.MindMapLeafPrefix:
                    lineKind = GameDrawer.LineKind.Dashed;
                    lineColor = Color.clear;
                    ellipse = true;
                    break;
            }
            Vector3 convertedHitPoint = GameDrawer.GetConvertedPosition(drawable, position);
            Vector3[] positions = GetBorderPositions(ellipse, convertedHitPoint, text);
            shape = GameDrawer.DrawLine(drawable, "", positions, GameDrawer.ColorKind.Monochrome,
                        lineColor, ValueHolder.currentSecondaryColor, ValueHolder.standardLineThickness, true,
                        lineKind, ValueHolder.standardLineTiling, false);
            shape = GameDrawer.SetPivotShape(shape, convertedHitPoint);
            return shape;
        }

        /// <summary>
        /// Calculates the border positions.
        /// </summary>
        /// <param name="ellipse">True, if the node is a theme or a leaf</param>
        /// <param name="position">The position of the border.</param>
        /// <param name="text">The text object, necessary for the width/height calculation.</param>
        /// <returns>The calculated positions.</returns>
        private static Vector3[] GetBorderPositions(bool ellipse, Vector3 position, GameObject text)
        {
            if (ellipse)
            {
                return GameShapesCalculator.Ellipse(position,
                    text.GetComponent<RectTransform>().rect.width, text.GetComponent<RectTransform>().rect.height);
            }
            else
            {
                return GameShapesCalculator.MindMapRectangle(position,
                    text.GetComponent<RectTransform>().rect.width + 0.05f, text.GetComponent<RectTransform>().rect.height + 0.05f);
            }
        }

        /// <summary>
        /// Redraws the mind map node border.
        /// </summary>
        /// <param name="node">The node which border should be redrawed.</param>
        public static void ReDrawBorder(GameObject node)
        {
            MMNodeValueHolder valueHolder = node.GetComponent<MMNodeValueHolder>();
            bool ellipse = valueHolder.GetNodeKind() != NodeKind.Subtheme;
            GameObject nodeText = GameFinder.FindChildWithTag(node, Tags.DText);

            Vector3[] positions = GetBorderPositions(ellipse, Vector3.zero, nodeText);
            GameDrawer.Drawing(GameFinder.FindChildWithTag(node, Tags.Line), positions);
            ReDrawBranchLines(node);
        }

        /// <summary>
        /// Gets the box collider size for the mind map node.
        /// </summary>
        /// <param name="shape">The mind map border.</param>
        /// <returns>The vector3 that represents the size for the box collider.</returns>
        private static Vector3 GetBoxSize(GameObject shape)
        {
            LineRenderer renderer = shape.GetComponent<LineRenderer>();
            Vector3[] rendererPos = new Vector3[renderer.positionCount];
            renderer.GetPositions(rendererPos);
            float[] xFloats = ConvertVector3ArrayToFloatArray(rendererPos, true);
            float x = Mathf.Abs(xFloats.Min()) + xFloats.Max() + 0.02f;

            float[] yFloats = ConvertVector3ArrayToFloatArray(rendererPos, false);
            float y = Mathf.Abs(yFloats.Min()) + yFloats.Max() + 0.01f;
            float z = Mathf.Abs(shape.transform.localPosition.z);
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Converts an axis of a Vector3 array to a float array.
        /// The z axis will be ignored because it is always zero.
        /// </summary>
        /// <param name="positions">The vector3 array which holds the positions</param>
        /// <param name="xValue">true, if the x axis should be converted. Otherwise it will convert the y axis.</param>
        /// <returns>The float array with the chosen converted axis.</returns>
        private static float[] ConvertVector3ArrayToFloatArray(Vector3[] positions, bool xValue)
        {
            float[] arr = new float[positions.Length];
            if (xValue)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    arr[i] = positions[i].x;
                }
            }
            else
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    arr[i] = positions[i].y;
                }
            }
            return arr;
        }

        /// <summary>
        /// Creates a mind map node.
        /// </summary>
        /// <param name="drawable">The drawable on that the node should be displayed.</param>
        /// <param name="prefix">The id prefix for the node.</param>
        /// <param name="writtenText">The text (description) of the node.</param>
        /// <param name="position">The position for the node.</param>
        /// <returns>The created node.</returns>
        public static GameObject Create(GameObject drawable, string prefix, string writtenText, Vector3 position)
        {
            Setup(drawable, "", prefix, writtenText, position, out GameObject node);
            return node;
        }

        /// <summary>
        /// Extract the id of a drawable type name.
        /// </summary>
        /// <param name="name">The drawable type name from which the ID should be extracted.</param>
        /// <returns>The extracted ID.</returns>
        public static string GetIDofName(string name)
        {
            return name.Split('-')[1];
        }

        /// <summary>
        /// Creates/Recreates a branch line between the node and the parent.
        /// If the parameter name is not empty, an attempt is made to redraw the branch line.
        /// The order of the branch line is a sequence lower than the lower sequence of both nodes (node/parent).
        /// The mesh collider of the branch line will be deactivated.
        /// In the <see cref="MMNodeValueHolder"/> component of the node, 
        /// the branch line is added as the parent branch line and the parent as parent.
        /// The <see cref="MMNodeValueHolder"/> component of the parent, adds the node and the branch line as children.
        /// </summary>
        /// <param name="node">The child node</param>
        /// <param name="parent">The parent node</param>
        /// <param name="name">The branch line id, empty if it's a new branch line.</param>
        /// <returns>The created branch line.</returns>
        public static GameObject CreateBranchLine(GameObject node, GameObject parent, string name = "")
        {
            Vector3 endPoint = NearestPoints.GetNearestPoint(parent, node.transform.position);
            Vector3 startPoint = NearestPoints.GetNearestPoint(node, endPoint);
            Vector3[] positions = new Vector3[2];
            positions[0] = startPoint;
            positions[1] = endPoint;
            GameFinder.GetHighestParent(node).transform.InverseTransformPoints(positions);

            GameObject drawable = GameFinder.GetDrawable(node);
            if (name == "")
            {
                name = ValueHolder.MindMapBranchLine + "-" + GetIDofName(parent.name) + "-" + GetIDofName(node.name);
            }
            GameObject branchLine = GameDrawer.DrawLine(drawable, name, positions, GameDrawer.ColorKind.Monochrome,
                        Color.black, ValueHolder.currentSecondaryColor, ValueHolder.standardLineThickness, true,
                        LineKind.Solid, ValueHolder.standardLineTiling, false);
            int order = GetBranchLineOrder(node, parent);
            GameLayerChanger.Decrease(branchLine, order, false);

            MMNodeValueHolder parentValueHolder = parent.GetComponent<MMNodeValueHolder>();
            parentValueHolder.AddChild(node, branchLine);

            MMNodeValueHolder nodeValueHolder = node.GetComponent<MMNodeValueHolder>();
            nodeValueHolder.SetParent(parent, branchLine);
            nodeValueHolder.SetLayer(parentValueHolder.GetLayer() + 1);
            branchLine.GetComponent<MeshCollider>().enabled = false;
            return branchLine;
        }

        /// <summary>
        /// Calculates the order for the branch line.
        /// The order of the branch line is a sequence lower than the lower sequence of both nodes (node/parent).
        /// </summary>
        /// <param name="node">The child node.</param>
        /// <param name="parent">The parent node.</param>
        /// <returns>The calculated order</returns>
        private static int GetBranchLineOrder(GameObject node, GameObject parent)
        {
            int order;
            if (node.GetComponent<OrderInLayerValueHolder>().GetOrderInLayer() > parent.GetComponent<OrderInLayerValueHolder>().GetOrderInLayer())
            {

                order = parent.GetComponent<OrderInLayerValueHolder>().GetOrderInLayer() - 1;
            }
            else
            {
                order = node.GetComponent<OrderInLayerValueHolder>().GetOrderInLayer() - 1;
            }
            if (order < 0)
            {
                order = 0;
            }
            return order;
        }

        /// <summary>
        /// Redraws the branch line to the parent node.
        /// </summary>
        /// <param name="node">The node which parent branch line should be redrawed.</param>
        public static void ReDrawParentBranchLine(GameObject node)
        {
            if (node.CompareTag(Tags.MindMapNode))
            {
                MMNodeValueHolder valueHolder = node.GetComponent<MMNodeValueHolder>();
                if (valueHolder.GetParentBranchLine() != null)
                {
                    GameObject parent = valueHolder.GetParent();
                    CreateBranchLine(node, parent, valueHolder.GetParentBranchLine().name);
                }
            }
        }

        /// <summary>
        /// Redraws the branch lines of a node.
        /// Includes the parent branch line and 
        /// the branch lines to the children of the given node.
        /// </summary>
        /// <param name="node">The node which branch lines should be redrawed.</param>
        /// <returns>true, if the given node has a <see cref="Tags.MindMapNode"/> and the redraw was successfully.</returns>
        public static bool ReDrawBranchLines(GameObject node)
        {
            if (node.CompareTag(Tags.MindMapNode))
            {
                MMNodeValueHolder valueHolder = node.GetComponent<MMNodeValueHolder>();
                ReDrawParentBranchLine(node);
                if (valueHolder.GetChildren().Count > 0)
                {
                    foreach (KeyValuePair<GameObject, GameObject> pair in valueHolder.GetChildren())
                    {
                        CreateBranchLine(pair.Key, node, pair.Value.name);
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Provides the changing of the parent.
        /// If the newly chosen parent is different from the previous one, 
        /// and if the validity check returns a positive result, the following will happen:
        /// - It removes the node of the children list of the old parent 
        ///   and destroys the old parent branch line.
        /// - Create a new branch line to the new chosen parent.
        ///   (<see cref="CreateBranchLine"/>)
        /// </summary>
        /// <param name="node">The child node</param>
        /// <param name="parent">The new chosen parent node</param>
        public static void ChangeParent(GameObject node, GameObject parent)
        {
            MMNodeValueHolder nodeValueHolder = node.GetComponent<MMNodeValueHolder>();
            if (nodeValueHolder.GetParent() != parent && CheckValidParentChange(node, parent))
            {
                if (nodeValueHolder.GetParent() != null)
                {
                    nodeValueHolder.GetParent().GetComponent<MMNodeValueHolder>().RemoveChild(node);
                }
                LineConf oldBranchLine = null;
                if (nodeValueHolder.GetParentBranchLine() != null)
                {
                    oldBranchLine = LineConf.GetLine(nodeValueHolder.GetParentBranchLine());
                }
                Destroyer.Destroy(nodeValueHolder.GetParentBranchLine());
                GameObject newBranchLine = CreateBranchLine(node, parent, "");
                if (oldBranchLine != null)
                {
                    GameEdit.ChangeLine(newBranchLine, oldBranchLine);
                }
            }
        }

        /// <summary>
        /// Validity check for the change of parent.
        /// The check prevents the formation of a cycle.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool CheckValidParentChange(GameObject node, GameObject parent, bool result = true)
        {
            MMNodeValueHolder valueHolder = node.GetComponent<MMNodeValueHolder>();
            if (node == parent)
            {
                result = false;
            }
            foreach (KeyValuePair<GameObject, GameObject> pair in valueHolder.GetChildren())
            {
                result = result && CheckValidParentChange(pair.Key, parent, result);
            }
            return result;
        }

        /// <summary>
        /// Provides the changing of the node kind.
        /// If the newly chosen node kind is different from the previous one, 
        /// and if the validity check returns a positive result, the following will happen:
        /// - If the new node kind is a theme, the old parent branch line will 
        ///   be deleted and the parent will set to null.
        /// - It adjusts the font size, font styles, border shape, and line kind of the borders. 
        /// - If the node kind will switched from leaf to another node kind the border will be black.
        /// - The prefix of the node id will change to the new selected node kind prefix.
        /// - The branch lines will be redrawed.
        /// </summary>
        /// <param name="node">The node which should change the node kind.</param>
        /// <param name="newNodeKind">The new node kind for the node.</param>
        /// <param name="borderConf">Optional parameter: To make the border look like the old one.</param>
        /// <returns>The new node kind of the node.</returns>
        public static NodeKind ChangeNodeKind(GameObject node, NodeKind newNodeKind, LineConf borderConf = null)
        {
            MMNodeValueHolder nodeValueHolder = node.GetComponent<MMNodeValueHolder>();
            GameObject nodeText = GameFinder.FindChildWithTag(node, Tags.DText);
            GameObject nodeBorder = GameFinder.FindChildWithTag(node, Tags.Line);
            LineConf border = LineConf.GetLine(nodeBorder);

            if (nodeValueHolder.GetNodeKind() != newNodeKind && CheckValidNodeKindChange(node, newNodeKind, nodeValueHolder.GetNodeKind()))
            {
                bool ellipse = false;
                switch(newNodeKind)
                {
                    case NodeKind.Theme:
                        if (nodeValueHolder.GetParent() != null)
                        {
                            nodeValueHolder.GetParent().GetComponent<MMNodeValueHolder>().RemoveChild(node);
                        }
                        Destroyer.Destroy(nodeValueHolder.GetParentBranchLine());
                        nodeValueHolder.SetParent(null, null);
                        ellipse = true;
                        GameEdit.ChangeFontStyles(nodeText, FontStyles.Bold | FontStyles.Underline);
                        GameEdit.ChangeFontSize(nodeText, 1.0f);
                        GameDrawer.ChangeLineKind(nodeBorder, LineKind.Solid, ValueHolder.standardLineTiling);
                        GameEdit.ChangePrimaryColor(nodeBorder, Color.black);
                        break;
                    case NodeKind.Subtheme:
                        GameEdit.ChangeFontStyles(nodeText, FontStyles.Normal);
                        GameEdit.ChangeFontSize(nodeText, 0.7f);
                        GameDrawer.ChangeLineKind(nodeBorder, LineKind.Solid, ValueHolder.standardLineTiling);
                        GameEdit.ChangePrimaryColor(nodeBorder, Color.black);
                        break;
                    case NodeKind.Leaf:
                        ellipse = true;
                        GameEdit.ChangeFontStyles(nodeText, FontStyles.Normal);
                        GameEdit.ChangeFontSize(nodeText, 0.5f);
                        GameDrawer.ChangeLineKind(nodeBorder, LineKind.Dashed25, ValueHolder.standardLineTiling);
                        GameEdit.ChangePrimaryColor(nodeBorder, Color.clear);
                        break;
                }
                ChangeName(node, newNodeKind);
                DisableTextAndBorderCollider(node);
                Vector3[] positions = GetBorderPositions(ellipse, Vector3.zero, nodeText);
                GameDrawer.Drawing(nodeBorder, positions);
                if (newNodeKind != NodeKind.Leaf && borderConf != null)
                {
                    GameEdit.ChangeLine(nodeBorder, borderConf);
                }

                nodeValueHolder.SetNodeKind(newNodeKind);
                ReDrawBranchLines(node);
            }
            return nodeValueHolder.GetNodeKind();
        }

        /// <summary>
        /// Changes the prefix of a node.
        /// </summary>
        /// <param name="node">The node which id should be changed.</param>
        /// <param name="newNodeKind">The new node kind, which prefix should be used.</param>
        private static void ChangeName(GameObject node, NodeKind newNodeKind)
        {
            NodeKind old = node.GetComponent<MMNodeValueHolder>().GetNodeKind();
            node.name = node.name.Replace(GetPrefix(old), GetPrefix(newNodeKind));

        }

        /// <summary>
        /// Gets the prefix of a given node kind.
        /// </summary>
        /// <param name="nodeKind">The chosen node kind which prefix should be returned.</param>
        /// <returns>The node kind prefix.</returns>
        private static string GetPrefix(NodeKind nodeKind)
        {
            switch (nodeKind)
            {
                case NodeKind.Theme:
                    return ValueHolder.MindMapThemePrefix;
                case NodeKind.Subtheme:
                    return ValueHolder.MindMapSubthemePrefix;
                case NodeKind.Leaf:
                    return ValueHolder.MindMapLeafPrefix;
            }
            return "";
        }

        /// <summary>
        /// Disables the text and border mesh collider of a node.
        /// </summary>
        /// <param name="node">The node which text and border collider should be disabled.</param>
        public static void DisableTextAndBorderCollider(GameObject node)
        {
            GameFinder.FindChildWithTag(node, Tags.Line).GetComponent<Collider>().enabled = false;
            GameFinder.FindChildWithTag(node, Tags.DText).GetComponent<Collider>().enabled = false;
        }

        /// <summary>
        /// Checks the validity and possibility <see cref="CheckChangeIsPosible"/> of the node kind change.
        /// </summary>
        /// <param name="node">The node which node kind should be changed</param>
        /// <param name="newNodeKind">The new node kind</param>
        /// <param name="oldNodeKind">The old node kind</param>
        /// <returns>the result of the check.</returns>
        public static bool CheckValidNodeKindChange(GameObject node, NodeKind newNodeKind, NodeKind oldNodeKind)
        {
            MMNodeValueHolder valueHolder = node.GetComponent<MMNodeValueHolder>();
            if (oldNodeKind == NodeKind.Theme)
            {
                return ((newNodeKind == NodeKind.Leaf && valueHolder.GetChildren().Count == 0)
                            || newNodeKind == NodeKind.Subtheme) && CheckChangeIsPosible(node);
            }
            if (oldNodeKind == NodeKind.Subtheme)
            {
                return newNodeKind == NodeKind.Theme || (newNodeKind == NodeKind.Leaf && valueHolder.GetChildren().Count == 0);
            }
            return true;
        }

        /// <summary>
        /// Checks if a node kind change from theme to subtheme is possible.
        /// For this, another theme node must exist on the drawable 
        /// that is considered as a new parent.
        /// </summary>
        /// <param name="selectedNode">The selected node</param>
        /// <returns>The result of the check</returns>
        private static bool CheckChangeIsPosible(GameObject selectedNode)
        {
            GameObject attacheds = GameFinder.GetAttachedObjectsObject(selectedNode);
            List<GameObject> nodes = GameFinder.FindAllChildrenWithTag(attacheds, Tags.MindMapNode);
            foreach (GameObject node in nodes)
            {
                if (node.GetComponent<MMNodeValueHolder>().GetNodeKind() == NodeKind.Theme && CheckValidParentChange(selectedNode, node))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Recreates a mind map node
        /// </summary>
        /// <param name="drawable">The drawable on that the node should be displayed.</param>
        /// <param name="parent">The parent mind map node</param>
        /// <param name="name">The id of the node</param>
        /// <param name="textConf">The text configuration for the text (description) of the node</param>
        /// <param name="borderConf">The line configuration for the node border</param>
        /// <param name="position">The position for the node.</param>
        /// <param name="scale">The scale for the node</param>
        /// <param name="eulerAngles">The euler angles for the node.</param>
        /// <param name="order">The order for the node</param>
        /// <param name="nodeKind">The node kind for the node</param>
        /// <param name="branchToParentName">The branch line name</param>
        /// <returns>The created mind map node.</returns>
        private static GameObject ReCreate(GameObject drawable, GameObject parent, string name, 
            TextConf textConf, LineConf borderConf, Vector3 position, Vector3 scale, 
            Vector3 eulerAngles, int order, NodeKind nodeKind, string branchToParentName)
        {
            if (order >= ValueHolder.currentOrderInLayer)
            {
                ValueHolder.currentOrderInLayer = order + 1;
            }
            GameObject createdNode;
            if (GameFinder.FindChild(drawable, name) != null)
            {
                createdNode = GameFinder.FindChild(drawable, name);
            }
            else
            {
                Setup(drawable, name, GetPrefix(nodeKind), textConf.text, drawable.transform.TransformPoint(position), out GameObject node);
                Destroyer.Destroy(GameFinder.FindChildWithTag(node, Tags.Line));
                Destroyer.Destroy(GameFinder.FindChildWithTag(node, Tags.DText));
                createdNode = node;
            }

            GameObject border = GameDrawer.ReDrawLine(drawable, borderConf);
            textConf.orderInLayer = order;
            GameObject text = GameTexter.ReWriteText(drawable, textConf);
            text.GetComponent<OrderInLayerValueHolder>().SetOrderInLayer(0);

            border.transform.SetParent(createdNode.transform);
            text.transform.SetParent(createdNode.transform);
            text.GetComponent<MeshCollider>().enabled = false;
            border.GetComponent<MeshCollider>().enabled = false;
            border.transform.localPosition = Vector3.zero;
            text.transform.localPosition = Vector3.zero;

            BoxCollider box = createdNode.GetComponent<BoxCollider>();
            box.size = GetBoxSize(border);

            createdNode.transform.localScale = scale;
            createdNode.transform.localEulerAngles = eulerAngles;
            createdNode.transform.localPosition = position;
            createdNode.GetComponent<OrderInLayerValueHolder>().SetOrderInLayer(order);
            if (parent != null)
            {
                CreateBranchLine(createdNode, parent, branchToParentName);
            }
            return createdNode;
        }

        /// <summary>
        /// Recreates a mind map node.
        /// </summary>
        /// <param name="drawable">The drawable on that the node should be displayed.</param>
        /// <param name="conf">The node configuration for restore.</param>
        /// <returns>The mind map node.</returns>
        public static GameObject ReCreate(GameObject drawable, MindMapNodeConf conf)
        {
            GameObject parent = null;
            if (GameFinder.GetAttachedObjectsObject(drawable) != null) 
            {
                parent = GameFinder.FindChild(GameFinder.GetAttachedObjectsObject(drawable), conf.parentNode);
            }
            return ReCreate(drawable,
                parent,
                conf.id,
                conf.textConf,
                conf.borderConf,
                conf.position,
                conf.scale,
                conf.eulerAngles,
                conf.orderInLayer,
                conf.nodeKind,
                conf.branchLineToParent);
        }

        /// <summary>
        /// Renames the mind map nodes and branchlines.
        /// </summary>
        /// <param name="config">The drawable configuration that holds the nodes and branch lines.</param>
        /// <param name="attachedObject">The attached objects object where the drawable types should be placed.</param>
        public static void RenameMindMap(DrawableConfig config, GameObject attachedObject)
        {
            if (attachedObject != null)
            {
                Dictionary<string, string> nameDictionary = new();
                Dictionary<string, string> idDictionary = new();
                foreach (MindMapNodeConf node in config.MindMapNodeConfigs)
                {
                    RenameNode(node, attachedObject, nameDictionary, idDictionary);
                }
                foreach (LineConf branchLine in config.LineConfigs)
                {
                    if (branchLine.id.StartsWith(ValueHolder.MindMapBranchLine))
                    {
                        RenameBranchLine(branchLine, idDictionary);
                    }
                }
            }
        }

        /// <summary>
        /// Renames a mind map node.
        /// </summary>
        /// <param name="conf">The node configuration that should be renamed</param>
        /// <param name="attachedObjects">The attached objects object where the drawable types should be placed.</param>
        /// <param name="nameDictionary">The dictionary that holds the old name and the new name</param>
        /// <param name="idDictionary">Dictionary that holds the old id's and the new names.</param>
        private static void RenameNode(MindMapNodeConf conf, GameObject attachedObjects, Dictionary<string, string> nameDictionary, Dictionary<string, string> idDictionary)
        {
            string prefix = GetPrefix(conf.nodeKind); ;

            if (GameFinder.FindChild(attachedObjects, conf.id) != null)
            {
                string id = DrawableHolder.GetRandomString(8);
                string newName = prefix + id;
                while (GameFinder.FindChild(attachedObjects, newName) != null)
                {
                    id = DrawableHolder.GetRandomString(8);
                    newName = prefix + id;
                }
                nameDictionary.Add(conf.id, newName);
                idDictionary.Add(GetIDofName(conf.id), newName);
                conf.id = newName;
                conf.borderConf.id = ValueHolder.LinePrefix + id;
                conf.textConf.id = ValueHolder.TextPrefix + id;
                if (conf.parentNode != "")
                {
                    conf.parentNode = nameDictionary[conf.parentNode];
                    conf.branchLineToParent = ValueHolder.MindMapBranchLine + "-" + GetIDofName(conf.parentNode) + "-" + GetIDofName(conf.id);
                    if (conf.branchLineConf != null)
                    {
                        conf.branchLineConf.id = conf.branchLineToParent;
                    }
                }
            }
        }

        /// <summary>
        /// Renames a branch line.
        /// </summary>
        /// <param name="conf">The branch line config</param>
        /// <param name="idDictionary">Dictionary that holds the old id's and the new names.</param>
        private static void RenameBranchLine(LineConf conf, Dictionary<string, string> idDictionary)
        {
            string prefix = ValueHolder.MindMapBranchLine;
            string[] splitted = conf.id.Split("-");

            string newParentID = splitted[1];
            string nPID;
            if (idDictionary.TryGetValue(newParentID, out nPID))
            {
                newParentID = GetIDofName(nPID);
            }
            
            string newChildID = splitted[2];
            string nCID;
            if (idDictionary.TryGetValue(newChildID, out nCID))
            {
                newChildID = GetIDofName(nCID);
            }
            conf.id = prefix + "-" + newParentID +"-" + newChildID;
        }

        /// <summary>
        /// Summerize the selected node, including children and branch lines, into a DrawableConfig.
        /// </summary>
        /// <param name="node">The selected node</param>
        /// <returns>A drawable configuration that only contains the selected node with children and branch lines.</returns>
        public static DrawableConfig SummarizeSelectedNodeIncChildren(GameObject node)
        {
            if (node.CompareTag(Tags.MindMapNode))
            {
                DrawableConfig conf = DrawableConfigManager.GetDrawableConfig(GameFinder.GetDrawable(node));
                conf.TextConfigs.Clear();
                conf.ImageConfigs.Clear();
                List<LineConf> selectedBranchLines = new();
                List<MindMapNodeConf> selectedNodes = new();
                selectedNodes.Add(MindMapNodeConf.GetNodeConf(node));
                MMNodeValueHolder valueHolder = node.GetComponent<MMNodeValueHolder>();
                foreach (KeyValuePair<GameObject, GameObject> pair in valueHolder.GetAllChildren())
                {
                    selectedNodes.Add(MindMapNodeConf.GetNodeConf(pair.Key));
                    selectedBranchLines.Add(LineConf.GetLine(pair.Value));
                }
                conf.LineConfigs = selectedBranchLines;
                conf.MindMapNodeConfigs = selectedNodes;
                return conf;
            }
            return null;
        }
    }
}