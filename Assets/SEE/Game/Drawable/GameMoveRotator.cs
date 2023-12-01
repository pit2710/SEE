﻿using Assets.SEE.Game.UI.Drawable;
using RTG;
using SEE.Game;
using SEE.Game.Drawable;
using SEE.Net.Actions.Drawable;
using SEE.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

namespace Assets.SEE.Game.Drawable
{
    /// <summary>
    /// This class provides methods for moving and rotating objects
    /// </summary>
    public static class GameMoveRotator
    {
        /// <summary>
        /// Move an object (using its pivot point) by mouse.
        /// For moving it is necessary that the rotation of the object is zero.
        /// Because if they are not zero, the axes are rotated.
        /// And that would lead to incorrect movement.
        /// </summary>
        /// <param name="obj">The object that should be moved.</param>
        /// <param name="hitPoint">The mouse hit point.</param>
        /// <returns>The new position of the moved object</returns>
        public static Vector3 MoveObjectByMouse(GameObject obj, Vector3 hitPoint, bool includeChildren)
        {
            CheckPrepareNodeChilds(obj, includeChildren);
            Vector3 oldPos = obj.transform.localPosition;
            ///This is needed to ensure that the correct axes are being moved. A rotation changes the axis position.
            Vector3 localEulerAngles = obj.transform.localEulerAngles;
            obj.transform.localEulerAngles = Vector3.zero;
            Vector3 convertedHitPoint = GameFinder.GetHighestParent(obj).transform.InverseTransformPoint(hitPoint);
            convertedHitPoint -= obj.transform.forward * ValueHolder.distanceToDrawable.z * obj.GetComponent<OrderInLayerValueHolder>().GetOrderInLayer();
            Vector3 position = new Vector3(convertedHitPoint.x, convertedHitPoint.y, oldPos.z);
            obj.transform.localPosition = position;
            obj.transform.localEulerAngles = localEulerAngles;
            CheckPostProcessNode(obj, includeChildren);
            return position;
        }

        /// <summary>
        /// Move an object by keyboard or by the move menu.
        /// For moving it is necessary that the rotation of the object is zero.
        /// Because if they are not zero, the axes are rotated.
        /// And that would lead to incorrect movement.
        /// </summary>
        /// <param name="obj">The object that should be moved</param>
        /// <param name="key">The pressed key for the movement direction</param>
        /// <param name="speedUp">if true the speed is 0.01f. otherwise it's 0.001</param>
        /// <returns>The new position of the object</returns>
        public static Vector3 MoveObjectByKeyboard(GameObject obj, KeyCode key, bool speedUp, bool includeChildren)
        {
            CheckPrepareNodeChilds(obj, includeChildren);
            Vector3 newPosition = obj.transform.localPosition;
            Vector3 localEulerAngles = obj.transform.localEulerAngles;
            obj.transform.localEulerAngles = Vector3.zero;
            
            float multiplyValue = 0.001f;
            if (speedUp)
            {
                multiplyValue = 0.01f;
            }
            switch (key)
            {
                case KeyCode.LeftArrow:
                    newPosition -= Vector3.right * multiplyValue;
                    break;
                case KeyCode.RightArrow:
                    newPosition += Vector3.right * multiplyValue;
                    break;
                case KeyCode.UpArrow:
                    newPosition += Vector3.up * multiplyValue;
                    break;
                case KeyCode.DownArrow:
                    newPosition -= Vector3.up * multiplyValue;
                    break;
            }
            obj.transform.localPosition = newPosition;
            obj.transform.localEulerAngles = localEulerAngles;
            CheckPostProcessNode(obj, includeChildren);
            return newPosition;
        }

        /// <summary>
        /// Sets the given position to the object.
        /// It will needed for undo/redo.
        /// </summary>
        /// <param name="obj">The object that should be moved</param>
        /// <param name="position">The new position for the object.</param>
        public static void SetPosition(GameObject obj, Vector3 position, bool includeChildren)
        {
            CheckPrepareNodeChilds(obj, includeChildren);
            obj.transform.localPosition = position;
            CheckPostProcessNode(obj, includeChildren);
        }

        /// <summary>
        /// Moves an point of a line.
        /// It only works for the drawable type line.
        /// </summary>
        /// <param name="line">The line which holds the to moved point</param>
        /// <param name="Indices">The indices of the points which should be moved. (all indices have the same position)</param>
        /// <param name="point">The new point position</param>
        public static void MovePoint(GameObject line, List<int> Indices, Vector3 point)
        {
            LineRenderer renderer = line.GetComponent<LineRenderer>();
            foreach (int i in Indices)
            {
                float z = renderer.GetPosition(i).z;
                Vector3 newPoint = new Vector3(point.x, point.y, z);
                renderer.SetPosition(i, newPoint);
            }
            GameDrawer.RefreshCollider(line);
        }

        /// <summary>
        /// Rotates an object at its pivot point.
        /// It is necessary to refresh the object's collider, as it does not update itself. 
        /// </summary>
        /// <param name="obj">The object which should be rotated.</param>
        /// <param name="rotateDirection">The direction in which should be rotated.</param>
        /// <param name="degree">The new degree.</param>
        /// <returns>The new local euler angles of the object</returns>
        public static Vector3 RotateObject(GameObject obj, Vector3 rotateDirection, float degree, bool includeChildren)
        {
            CheckPrepareNodeChilds(obj, includeChildren);
            Transform transform = obj.transform;
            transform.Rotate(rotateDirection, degree, Space.Self);
            obj.GetComponent<Collider>().enabled = false;
            obj.GetComponent<Collider>().enabled = true;
            CheckPostProcessNode(obj, includeChildren);
            return obj.transform.localEulerAngles;
        }

        /// <summary>
        /// Sets the given z euler angle to the object.
        /// It rotates the z axis.
        /// Will needed for undo/redo.
        /// </summary>
        /// <param name="obj">The object which should be rotated.</param>
        /// <param name="localEulerAngleZ">The new z degree</param>
        public static void SetRotate(GameObject obj, float localEulerAngleZ, bool includeChildren)
        {
            CheckPrepareNodeChilds(obj, includeChildren, true);
            Transform transform = obj.transform;
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, localEulerAngleZ);
            CheckPostProcessNode(obj, includeChildren);
        }

        /// <summary>
        /// Sets the given y euler angle to the object.
        /// It rotates the y axis.
        /// Will needed for mirror an image.
        /// </summary>
        /// <param name="obj">The image object which should be mirrored.</param>
        /// <param name="localEulerAngleY">The new degree for the y axis</param>
        public static void SetRotateY(GameObject obj, float localEulerAngleY)
        {
            Transform transform = obj.transform;
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, localEulerAngleY, transform.localEulerAngles.z);
        }

        /// <summary>
        /// This method prepares a mind map node so that 
        /// an action including the children can be performed. 
        /// For this purpose, the child nodes are added to the parent node.
        /// </summary>
        /// <param name="obj">The parent node</param>
        /// <param name="rotationSetMode">true, if this method will be called from rotation action.</param>
        private static void PrepareNodeChilds(GameObject obj, bool rotationSetMode = false)
        {
            if (obj.CompareTag(Tags.MindMapNode))
            {
                MMNodeValueHolder valueHolder = obj.GetComponent<MMNodeValueHolder>();
                foreach(KeyValuePair<GameObject, GameObject> pair in valueHolder.GetAllChildren())
                {
                    if (rotationSetMode)
                    {
                        pair.Key.transform.localEulerAngles = obj.transform.localEulerAngles;
                    }
                    pair.Key.transform.SetParent(obj.transform);
                    if (pair.Key.GetComponent<Rigidbody>() == null)
                    {
                        pair.Key.AddComponent<Rigidbody>().isKinematic = true;
                        pair.Key.AddComponent<CollisionController>();
                    }
                }
            }
        }

        /// <summary>
        /// Checks if preparing child nodes is necessary, and if not, 
        /// deletes all rigid bodies and collision controllers except those of the selected nodes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="includeChildren"></param>
        /// <param name="rotationSetMode"></param>
        private static void CheckPrepareNodeChilds(GameObject obj, bool includeChildren, bool rotationSetMode = false)
        {
            if (includeChildren)
            {
                PrepareNodeChilds(obj, true);
            } else
            {
                if (obj.CompareTag(Tags.MindMapNode))
                {
                    MMNodeValueHolder valueHolder = obj.GetComponent<MMNodeValueHolder>();
                    GameObject drawable = GameFinder.GetDrawable(obj);
                    string drawableParentName = GameFinder.GetDrawableParentName(drawable);
                    new RbAndCCDestroyerNetAction(drawable.name, drawableParentName, obj.name).Execute();
                    foreach(KeyValuePair<GameObject, GameObject> pair in valueHolder.GetAllChildren())
                    {
                        if (pair.Key.GetComponent<Rigidbody>() != null)
                        {
                            Destroyer.Destroy(pair.Key.GetComponent<Rigidbody>());
                            Destroyer.Destroy(pair.Key.GetComponent<CollisionController>());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method separates the child nodes from the parent node 
        /// after <see cref="PrepareNodeChilds"/> has been called. 
        /// The children are assigned to the original "AttachedObjects" 
        /// object of the respective drawable.
        /// </summary>
        /// <param name="obj">The parent node</param>
        private static void PostProcessNode(GameObject obj)
        {
            if (obj.CompareTag(Tags.MindMapNode))
            {
                GameMindMap.ReDrawParentBranchLine(obj);
                GameObject attachedObject = GameFinder.GetAttachedObjectsObject(obj);
                MMNodeValueHolder v = obj.GetComponent<MMNodeValueHolder>();
                foreach (KeyValuePair<GameObject, GameObject> pair in v.GetAllChildren())
                {
                    pair.Key.transform.SetParent(attachedObject.transform);
                    pair.Value.transform.SetParent(attachedObject.transform);
                    GameMindMap.ReDrawParentBranchLine(pair.Key);
                }
            }
        }

        private static void CheckPostProcessNode(GameObject obj, bool includeChildren)
        {
            if (includeChildren)
            {
                PostProcessNode(obj);
            }
            else
            {
                if (obj.CompareTag(Tags.MindMapNode))
                {
                    GameMindMap.ReDrawBranchLines(obj);
                }
            }
        }

        /// <summary>
        /// Destroys all rigid bodies and collision controllers of all children of the selected node.
        /// </summary>
        /// <param name="node">The selected node</param>
        public static void DestroyRigidBodysAndCollisionControllersOfChildren(GameObject node)
        {
            if (node.CompareTag(Tags.MindMapNode))
            {
                MMNodeValueHolder valueHolder = node.GetComponent<MMNodeValueHolder>();
                foreach(KeyValuePair<GameObject, GameObject> pair in valueHolder.GetAllChildren())
                {
                    if (pair.Key.GetComponent<Rigidbody>() != null)
                    {
                        Destroyer.Destroy(pair.Key.GetComponent<Rigidbody>());
                    }
                    if (pair.Key.GetComponent<CollisionController>() != null)
                    {
                        Destroyer.Destroy(pair.Key.GetComponent<CollisionController>());
                    }
                }
            }
        }
    }
}