﻿using Assets.SEE.Game;
using SEE.Game;
using SEE.Utils;
using UnityEngine;

namespace SEE.Net.Actions
{
    /// <summary>
    /// This class propagates a <see cref="DeleteAction"/> to all clients in the network.
    /// </summary>
    public class LineEraseNetAction : AbstractNetAction
    {
        public string DrawableID;
        public string ParentDrawableID;
        public string LineName;

        /// <summary>
        /// Creates a new FastEraseNetAction.
        /// </summary>
        /// <param name="gameObjectID">the unique name of the gameObject of a line
        /// that has to be deleted</param>
        public LineEraseNetAction(string drawableID, string parentDrawableID, string lineName) : base()
        {
            DrawableID = drawableID;
            ParentDrawableID = parentDrawableID;
            LineName = lineName;
        }

        /// <summary>
        /// Things to execute on the server (none for this class). Necessary because it is abstract
        /// in the superclass.
        /// </summary>
        protected override void ExecuteOnServer()
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Deletes the game object identified by <see cref="GameObjectID"/> on each client.
        /// </summary>
        protected override void ExecuteOnClient()
        {
            if (!IsRequester())
            {
                GameObject drawable = GameDrawableIDFinder.Find(DrawableID,ParentDrawableID);
                if (drawable != null && GameDrawableIDFinder.FindChild(drawable, LineName) != null)
                {
                    Destroyer.Destroy(GameDrawableIDFinder.FindChild(drawable, LineName));
                }
                else
                {
                    throw new System.Exception($"There is no drawable with the ID {DrawableID} or line with the ID {LineName}.");
                }
            }
        }
    }
}
