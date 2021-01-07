﻿using SEE.DataModel.DG;
using SEE.GO;
using SEE.Utils;
using UnityEngine;

namespace SEE.Game
{
    /// <summary>
    /// Allows to move game nodes (game objects representing a graph node).
    /// </summary>
    public static class GameNodeMover
    {
        /// <summary>
        /// The speed by which to move a selected object.
        /// </summary>
        public static float MovingSpeed = 1.0f;

        /// <summary>
        /// Moves the given <paramref name="movingObject"/> on a sphere around the
        /// camera. The radius sphere of this sphere is the original distance
        /// from the <paramref name="movingObject"/> to the camera. The point
        /// on that sphere is determined by a ray driven by the user hitting
        /// this sphere. The speed of travel is defind by <see cref="MovingSpeed"/>.
        /// 
        /// This method is expected to be called at every Update().
        /// </summary>
        /// <param name="movingObject">the object to be moved.</param>
        public static void MoveTo(GameObject movingObject)
        {
            float step = MovingSpeed * Time.deltaTime;
            Vector3 target = TipOfRayPosition(movingObject);
            movingObject.transform.position = Vector3.MoveTowards(movingObject.transform.position, target, step);
        }

        /// <summary>
        /// Finalizes the final position of the <paramref name="movingObject"/>.
        /// </summary>
        /// <param name="movingObject"></param>
        public static void FinalizePosition(GameObject movingObject)
        {
            // The underlying graph node of the moving object.
            Node movingNode = movingObject.GetComponent<NodeRef>().Value;
            // The new parent of the movingNode in the underlying graph.
            Node newGraphParent = null;
            // The new parent of the movingNode in the game-object hierarchy.
            GameObject newGameParent = null;
            // The new position of the movingNode in world space.
            Vector3 newPosition = Vector3.negativeInfinity;

            // Note that the order of the results of RaycastAll() is undefined.
            // Hence, we need to identify the node in the node hierarchy that
            // is at the lowest level in the tree (more precisely, the one with
            // the greatest value of the node attribute Level; Level counting
            // starts at the root and increases downward into the tree).            
            foreach (RaycastHit hit in Physics.RaycastAll(UserPointsTo()))
            {
                // Must be different from the movingObject itself
                if (hit.collider.gameObject != movingObject)
                {
                    NodeRef nodeRef = hit.transform.GetComponent<NodeRef>();
                    // Is it a node at all and if so, are they in the same graph?
                    if (nodeRef != null && nodeRef.Value.ItsGraph == movingNode.ItsGraph)
                    {
                        // update newParent when we found a node deeper into the tree
                        if (newGraphParent == null || nodeRef.Value.Level > newGraphParent.Level)
                        {
                            newGraphParent = nodeRef.Value;
                            newGameParent = hit.collider.gameObject;
                            newPosition = hit.point;
                        }
                    }
                }
            }

            if (newGraphParent != null)
            {
                movingObject.transform.position = newPosition;
                if (movingNode.Parent != newGraphParent)
                {
                    movingNode.Reparent(newGraphParent);
                    PutOn(movingObject, newGameParent);
                }
            }
            else
            {
                Debug.Log("Final destination canceled.\n");
            }
        }

        /// <summary>
        /// Puts <paramref name="child"/> on top of <paramref name="parent"/>
        /// and makes <paramref name="child"/> a child of <paramref name="parent"/>
        /// in the game-object hierarchy.
        /// </summary>
        /// <param name="child">child</param>
        /// <param name="parent">parent</param>
        private static void PutOn(GameObject child, GameObject parent)
        {
            // FIXME: child may not actually fit into parent, in which we should 
            // downscale it until it fits
            Vector3 childCenter = child.transform.position;
            float parentRoof = parent.transform.position.y + parent.transform.lossyScale.y / 2;
            childCenter.y = parentRoof + child.transform.lossyScale.y / 2;
            child.transform.position = childCenter;
            child.transform.SetParent(parent.transform);
        }

        // -------------------------------------------------------------
        // User input
        // -------------------------------------------------------------

        /// <summary>
        /// A ray from the user.
        /// </summary>
        /// <returns>ray from the user</returns>
        private static Ray UserPointsTo()
        {
            // FIXME: We need to an interaction for VR, too.
            return MainCamera.Camera.ScreenPointToRay(Input.mousePosition);
        }

        /// <summary>
        /// Returns the position of the tip of the ray drawn from the camera towards
        /// the position the user is currently pointing to. The distance of that 
        /// point along this ray is the distance between the camera from which the
        /// ray originated and the position of the given <paramref name="selectedObject"/>.
        /// 
        /// That means, the selected object moves on a sphere around the camera
        /// at the distance of the selected object.
        /// </summary>
        /// <param name="selectedObject">the selected object currently moved around</param>
        /// <returns>tip of the ray</returns>
        private static Vector3 TipOfRayPosition(GameObject selectedObject)
        {
            Ray ray = UserPointsTo();
            return ray.GetPoint(Vector3.Distance(ray.origin, selectedObject.transform.position));
        }
    }
}