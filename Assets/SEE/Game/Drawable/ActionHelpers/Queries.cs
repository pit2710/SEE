﻿using SEE.Game.Drawable;
using SEE.Game;
using UnityEngine;

namespace Assets.SEE.Game.Drawable.ActionHelpers
{
    /// <summary>
    /// This class provides various reused queries for the drawable actions.
    /// </summary>
    public static class Queries
    {
        /// <summary>
        /// Registers the users left mouse button input.
        /// </summary>
        /// <returns>true if the user uses the left mouse button.</returns>
        public static bool LeftMouseInteraction()
        {
            return (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0));
        }

        /// <summary>
        /// Registers the users left mouse down input.
        /// </summary>
        /// <returns>True if the user uses left mouse down.</returns>
        public static bool LeftMouseDown() 
        {
            return Input.GetMouseButtonDown(0);
        }

        /// <summary>
        /// Registers the users right mouse button input.
        /// </summary>
        /// <returns>true if the user uses the left mouse button.</returns>
        public static bool RightMouseInteraction()
        {
            return (Input.GetMouseButton(1) || Input.GetMouseButtonDown(1));
        }

        /// <summary>
        /// Registers the uses mouse button up input (release the selected button).
        /// </summary>
        /// <param name="state">The mouse button which should be observed.</param>
        /// <returns>true if the user releases the selected mouse button.</returns>
        public static bool MouseUp(MouseButton state)
        {
            return Input.GetMouseButtonUp((int)state);
        }

        /// <summary>
        /// Registers the uses mouse button input (button holded).
        /// </summary>
        /// <param name="state">The mouse button which should be observed.</param>
        /// <returns>true if the user holds the selected mouse button.</returns>
        public static bool MouseHold(MouseButton state)
        {
            return Input.GetMouseButton((int)state);
        }

        /// <summary>
        /// Registers the uses mouse down button input (button down).
        /// </summary>
        /// <param name="state">The mouse down button which should be observed.</param>
        /// <returns>true if the user press the selected mouse button.</returns>
        public static bool MouseDown(MouseButton state)
        {
            return Input.GetMouseButtonDown((int)state);
        }

        /// <summary>
        /// Checks if the given drawable surface object is the same object as the other one.
        /// </summary>
        /// <param name="drawable">The drawable surface to be checked.</param>
        /// <param name="other">>The other object.</param>
        /// <returns>True if the drawable surface is the same as the other object.</returns>
        public static bool SameDrawableSurface(GameObject drawable, GameObject other)
        {
           return drawable != null && GameFinder.GetDrawable(other).Equals(drawable);
        }

        /// <summary>
        /// Checks if the given drawable surface is null or the same object as the other <see cref="GameObject"/>.
        /// </summary>
        /// <param name="drawable">The drawable surface to be checked.</param>
        /// <param name="other">The other object.</param>
        /// <returns>True if the drawable surface is null or the same as the other object.</returns>
        public static bool DrawableSurfaceNullOrSame(GameObject drawable, GameObject other)
        {
            return drawable == null || SameDrawableSurface(drawable, other);
        }
    }
}