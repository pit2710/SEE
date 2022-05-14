﻿namespace SEE.GO
{
    /// <summary>
    /// What kind of input devices the player uses.
    /// </summary>
    public enum PlayerInputType
    {
        DesktopPlayer = 0,      // player for desktop and mouse input
        TouchGamepadPlayer = 1, // player for touch devices or gamepads using InControl
        VRPlayer = 2,           // player for virtual reality devices
        // HoloLensPlayer = 3,     // player for mixed reality devices. Not supported anymore.
        None = 4,               // no player at all
    }
}
