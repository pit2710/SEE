﻿using Assets.SEE.Game.Drawable;
using Assets.SEE.Game;
using SEE.Controls.Actions.Drawable;
using UnityEngine;

namespace SEE.Net.Actions.Drawable
{
    /// <summary>
    /// This class is responsible for changing the order in layer (<see cref="EditLineAction"/>) of a line on all clients.
    /// </summary>
    public class EditLineLayerNetAction : AbstractNetAction
    {
        /// <summary>
        /// The id of the drawable on which the object is located
        /// </summary>
        public string DrawableID;
        /// <summary>
        /// The id of the drawable parent
        /// </summary>
        public string ParentDrawableID;
        /// <summary>
        /// The id of the line that should be changed
        /// </summary>
        public string LineName;
        /// <summary>
        /// The new order in layer for the line.
        /// </summary>
        public int OrderInLayer;

        /// <summary>
        /// The constructor of this action. All it does is assign the value you pass it to a field.
        /// </summary>
        /// <param name="drawableID">The id of the drawable on which the object is located.</param>
        /// <param name="parentDrawableID">The id of the drawable parent.</param>
        /// <param name="lineName">The id of the line that should be changed.</param>
        /// <param name="thickness">The new order in layer for the line.</param>
        public EditLineLayerNetAction(string drawableID, string parentDrawableID, string lineName, int oderInLayer) : base()
        {
            DrawableID = drawableID;
            ParentDrawableID = parentDrawableID;
            LineName = lineName;
            this.OrderInLayer = oderInLayer;
        }

        /// <summary>
        /// Things to execute on the server (none for this class). Necessary because it is abstract
        /// in the superclass.
        /// </summary>
        protected override void ExecuteOnServer()
        {

        }

        /// <summary>
        /// Changes the order in layer of the given line on each client.
        /// </summary>
        /// <exception cref="System.Exception">will be thrown, if the <see cref="DrawableID"/> or <see cref="LineName"/> don't exists.</exception>
        protected override void ExecuteOnClient()
        {
            if (!IsRequester())
            {
                if (!IsRequester())
                {
                    GameObject drawable = GameDrawableFinder.Find(DrawableID, ParentDrawableID);
                    if (drawable != null && GameDrawableFinder.FindChild(drawable, LineName) != null)
                    {
                        GameEditLine.ChangeLayer(GameDrawableFinder.FindChild(drawable, LineName), OrderInLayer);
                    }
                    else
                    {
                        throw new System.Exception($"There is no drawable with the ID {DrawableID} or line with the ID {LineName}.");
                    }
                }
            }
        }
    }
}