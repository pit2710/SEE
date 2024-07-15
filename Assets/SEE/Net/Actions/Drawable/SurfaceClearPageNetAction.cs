﻿using SEE.Game.Drawable;
using SEE.Game.Drawable.Configurations;
using SEE.Game.Drawable.ValueHolders;

namespace SEE.Net.Actions.Drawable
{/// <summary>
 /// This class is responsible for clear a page of a drawable on all clients.
 /// </summary>
    public class SurfaceClearPageNetAction : DrawableNetAction
    {
        /// <summary>
        /// The config of the drawable that should be synchronized.
        /// </summary>
        public DrawableConfig Config;

        /// <summary>
        /// The page to be cleared.
        /// </summary>
        public int Page;

        /// <summary>
        /// The constructor of this action. All it does is assign the value you pass it to a field.
        /// </summary>
        /// <param name="config">The current <see cref="DrawableConfig"/>.</param>
        /// <param name="page">The page to be removed.</param>
        public SurfaceClearPageNetAction(DrawableConfig config, int page) : base(config.ID, config.ParentID)
        {
            Config = config;
            Page = page;
        }

        /// <summary>
        /// Synchronize the current order in layer of the host on each client.
        /// </summary>
        protected override void ExecuteOnClient()
        {
            if (!IsRequester())
            {
                base.ExecuteOnClient();
                GameDrawableManager.DeleteTypesFromPage(Surface, Page);
            }
        }
    }
}