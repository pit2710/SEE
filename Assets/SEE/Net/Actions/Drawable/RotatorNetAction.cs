﻿using SEE.Net.Actions;
using UnityEngine;
using SEE.Controls.Actions.Drawable;
using SEE.Game.Drawable;

namespace SEE.Net.Actions.Drawable
{
    /// <summary>
    /// This class is responsible for changing the rotation (<see cref="MoveRotatorAction"/>) of an object on all clients.
    /// </summary>
    public class RotatorNetAction : AbstractNetAction
    {
        /// <summary>
        /// The id of the drawable on which the object is located.
        /// </summary>
        public string DrawableID;
        /// <summary>
        /// The id of the drawable parent.
        /// </summary>
        public string ParentDrawableID;
        /// <summary>
        /// The id of the object that should be changed.
        /// </summary>
        public string ObjectName;
        /// <summary>
        /// The direction by which the object should be rotated (forward or back).
        /// </summary>
        public Vector3 Direction;
        /// <summary>
        /// The degree by which the object should be rotated.
        /// </summary>
        public float Degree;
        /// <summary>
        /// The bool for include children, only necressary for mind map nodes.
        /// </summary>
        public bool IncludeChildren;
        /// <summary>
        /// The position of the object.
        /// </summary>
        public Vector3 ObjectPosition;

        /// <summary>
        /// The constructor of this action. All it does is assign the value you pass it to a field.
        /// </summary>
        /// <param name="drawableID">The id of the drawable on which the object is located.</param>
        /// <param name="parentDrawableID">The id of the drawable parent.</param>
        /// <param name="objectName">The id of the object that should be changed.</param>
        /// <param name="direction">The direction by which the object should be rotated.</param>
        /// <param name="degree">The value by which the object should be rotated</param>
        /// <param name="includeChildren">Option for mind map nodes, if children should also rotated.</param>
        public RotatorNetAction(string drawableID, string parentDrawableID, string objectName, Vector3 direction, float degree, bool includeChildren) : base()
        {
            DrawableID = drawableID;
            ParentDrawableID = parentDrawableID;
            ObjectName = objectName;
            Direction = direction;
            Degree = degree;
            IncludeChildren = includeChildren;
        }

        /// <summary>
        /// The constructor of this action. All it does is assign the value you pass it to a field.
        /// Used for undo / redo of <see cref="MoveRotatorAction"/> 
        /// </summary>
        /// <param name="drawableID">The id of the drawable on which the object is located.</param>
        /// <param name="parentDrawableID">The id of the drawable parent.</param>
        /// <param name="objectName">The id of the object that should be changed.</param>
        /// <param name="localEulerAnlgeZ">The value to which the object should be rotated</param>
        /// <param name="includeChildren">Option for mind map nodes, if children should also rotated.</param>
        public RotatorNetAction(string drawableID, string parentDrawableID, string objectName, float localEulerAnlgeZ, bool includeChildren) : base()
        {
            DrawableID = drawableID;
            ParentDrawableID = parentDrawableID;
            ObjectName = objectName;
            Degree = localEulerAnlgeZ;
            Direction = Vector3.zero;
            IncludeChildren = includeChildren;
        }

        /// <summary>
        /// Things to execute on the server (none for this class). Necessary because it is abstract
        /// in the superclass.
        /// </summary>
        protected override void ExecuteOnServer()
        {

        }

        /// <summary>
        /// Changes the rotation of the given object on each client.
        /// </summary>
        /// <exception cref="System.Exception">will be thrown, if the <see cref="DrawableID"/> or <see cref="ObjectName"/> don't exists.</exception>
        protected override void ExecuteOnClient()
        {
            if (!IsRequester())
            {
                if (!IsRequester())
                {
                    GameObject drawable = GameFinder.FindDrawable(DrawableID, ParentDrawableID);
                    if (drawable != null && GameFinder.FindChild(drawable, ObjectName) != null)
                    {
                        GameObject child = GameFinder.FindChild(drawable, ObjectName);
                        if (Direction != Vector3.zero)
                        {
                            GameMoveRotator.RotateObject(child, Direction, Degree, IncludeChildren);
                        } else
                        {
                            GameMoveRotator.SetRotate(child, Degree, IncludeChildren);
                        }
                    }
                    else
                    {
                        throw new System.Exception($"There is no drawable with the ID {DrawableID} or line with the ID {ObjectName}.");
                    }
                }
            }
        }
    }
}