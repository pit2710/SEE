﻿using SEE.Game;
using SEE.Game.Drawable;
using SEE.Game.Drawable.Configurations;
using SEE.Game.Drawable.ValueHolders;
using SEE.Net.Actions.Drawable;
using UnityEngine;

namespace SEE.UI.Drawable
{
    /// <summary>
    /// The border trigger controller ensures that the 
    /// <see cref="DrawableType"/> objects stay within the drawables 
    /// and moves them in the respective direction when necessary.
    /// </summary>
    public class BorderTriggerController : MonoBehaviour
    {
        /// <summary>
        /// Will be executed when a collision stay.
        /// It moves the collision object back into the drawable area.
        /// </summary>
        /// <param name="other">The object that causes the collision.</param>
        private void OnTriggerStay(Collider other)
        {
            if (Tags.DrawableTypes.Contains(other.gameObject.tag)
                && GameFinder.GetHighestParent(gameObject)
                    .Equals(GameFinder.GetHighestParent(other.gameObject)))
            {
                GameObject drawable = GameFinder.GetDrawable(other.gameObject);
                string drawableParentName = GameFinder.GetDrawableParentName(drawable);

                /// Block for Mind Map Nodes, they could include children, 
                /// which is why they are considered particularly.
                if (other.gameObject.CompareTag(Tags.MindMapNode))
                {
                    Rigidbody[] bodys = GameFinder.GetAttachedObjectsObject(other.gameObject)
                        .GetComponentsInChildren<Rigidbody>();
                    MMNodeValueHolder valueHolder = other.gameObject.GetComponent<MMNodeValueHolder>();
                    /// A Rigidbody is only assigned when the object needs to be moved, rotated, or scaled.
                    foreach (Rigidbody body in bodys)
                    {
                        if (body.gameObject == other.gameObject
                            || valueHolder.GetAllChildren().ContainsKey(body.gameObject)
                            || valueHolder.GetAllParentAncestors().Contains(body.gameObject))
                        {
                            MoveBack(body.gameObject, drawable, drawableParentName);
                        }
                    }
                }
                else
                { /// For all other <see cref="DrawableType"/> move the object back into the drawable area.
                    MoveBack(other.gameObject, drawable, drawableParentName);
                }
            }
        }

        /// <summary>
        /// This method moves the object back.
        /// </summary>
        /// <param name="objToMove">The object to move</param>
        /// <param name="drawable">The drawable of the object.</param>
        /// <param name="drawableParentName">The parent name of the drawable.</param>
        private void MoveBack(GameObject objToMove, GameObject drawable, string drawableParentName)
        {
            Transform transform = objToMove.transform;
            /// This is needed to ensure that the correct axes are being moved. A rotation changes the axis position.
            Vector3 eulerAngles = transform.localEulerAngles;
            transform.localEulerAngles = Vector3.zero;

            /// The fast moving speed will be chosen here.
            /// Regular movement might take too long under certain circumstances.
            float moveValue = ValueHolder.MoveFast;

            Vector3 newPosition = transform.localPosition;
            /// Calculation of the new position, depending on which border registers a collision.
            switch (tag)
            {
                case Tags.Top:
                    newPosition -= Vector3.up * moveValue;
                    break;
                case Tags.Bottom:
                    newPosition += Vector3.up * moveValue;
                    break;
                case Tags.Left:
                    newPosition += Vector3.right * moveValue;
                    break;
                case Tags.Right:
                    newPosition -= Vector3.right * moveValue;
                    break;
                default:
                    break;
            }
            /// Restores the old rotation.
            transform.localEulerAngles = eulerAngles;

            /// Sets the new position.
            GameMoveRotator.SetPosition(objToMove, newPosition, false);
            new MoveNetAction(drawable.name, drawableParentName, objToMove.name, newPosition, false).Execute();
        }
    }
}