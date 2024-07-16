﻿using SEE.Game.Drawable;
using SEE.Game.Drawable.Configurations;
using UnityEngine;

namespace SEE.Net.Actions.Drawable
{
    /// <summary>
    /// This class is reponsible for change the drawable lighting on all clients.
    /// </summary>
    public class DrawableChangeLightingNetAction : AbstractNetAction
    {
        /// <summary>
        /// The drawable that should be changed.
        /// </summary>
        public DrawableConfig DrawableConf;

        /// <summary>
        /// The constructor of this action. All it does is assign the value you pass it to a field.
        /// </summary>
        public DrawableChangeLightingNetAction(DrawableConfig config)
        {
            DrawableConf = config;
        }

        /// <summary>
        /// Changes the lighting of the drawable on each client.
        /// </summary>
        public override void ExecuteOnClient()
        {
            GameObject surface = GameFinder.FindDrawableSurface(DrawableConf.ID, DrawableConf.ParentID);
            GameDrawableManager.ChangeLighting(surface.transform.parent.gameObject, DrawableConf.Lighting);
        }
    }
}