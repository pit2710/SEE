﻿using SEE.Game;
using System.Collections;
using UnityEngine;
using SEE.Game.Drawable.Configurations;

namespace Assets.SEE.Game.Drawable
{
    /// <summary>
    /// This class represents a value holder component for the order in layer value of the <see cref="DrawableType"/>drawable types.
    /// </summary>
    public class OrderInLayerValueHolder : MonoBehaviour
    {
        /// <summary>
        /// The holded order of the drawable type.
        /// </summary>
        private int orderInLayer;

        /// <summary>
        /// Sets the given order.
        /// </summary>
        /// <param name="order">The given order that should be set.</param>
        public void SetOrderInLayer(int order)
        {
            this.orderInLayer = order;
        }

        /// <summary>
        /// Gets the current order of the drawable type.
        /// </summary>
        /// <returns></returns>
        public int GetOrderInLayer()
        {
            return orderInLayer;
        }
    }
}