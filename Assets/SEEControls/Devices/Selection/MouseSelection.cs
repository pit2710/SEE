﻿using UnityEngine;

namespace SEE.Controls.Devices
{
    /// <summary>
    /// A selection device using input from a mouse or touch screen yielding 2D co-ordinates 
    /// for selected points on the screen. A user can hit the selection mouse button
    /// or touch the screen with a finger or pen.
    /// </summary>
    public class MouseSelection: Selection
    {
        [Tooltip("The index of the mouse button needed to be pressed to change the viewpoint (0 = left, 1 = right).")]
        public int MouseButton = 0;

        public override Vector3 Direction => Input.mousePosition;

        public override bool Activated => Input.GetMouseButton(MouseButton);        

        public override Vector3 Position => Input.mousePosition;
    }
}