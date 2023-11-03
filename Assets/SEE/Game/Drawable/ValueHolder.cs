﻿using SEE.Controls.Actions;
using SEE.Game.Drawable;
using SEE.GO;
using SEE.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.SEE.Game.Drawable
{
    /// <summary>
    /// This class holds the current values and the constants for drawables.
    /// </summary>
    public static class ValueHolder
    {
        #region current values
        /// <summary>
        /// The current chosen primary color for drawing.
        /// </summary>
        public static Color currentPrimaryColor { get; set; }

        /// <summary>
        /// The current chosen secondary color for drawing.
        /// </summary>
        public static Color currentSecondaryColor { get; set; }

        /// <summary>
        /// The current chosen thickness for drawing.
        /// </summary>
        public static float currentThickness { get; set; }

        /// <summary>
        /// The current chosen thickness for text outline.
        /// </summary>
        public static float currentOutlineThickness { get; set; }

        /// <summary>
        /// The current chosen text font size.
        /// </summary>
        public static float currentFontSize { get; set; }

        /// <summary>
        /// The current chosen line kind for drawing.
        /// </summary>
        public static GameDrawer.LineKind currentLineKind { get; set; }

        /// <summary>
        /// The current chosen color kind for drawing.
        /// </summary>
        public static GameDrawer.ColorKind currentColorKind { get; set; }

        /// <summary>
        /// The current chosen tiling for drawing a dashed line kind.
        /// </summary>
        public static float currentTiling { get; set; }

        /// <summary>
        /// The current order in layer value.
        /// </summary>
        public static int currentOrderInLayer { get; set; }
        #endregion

        #region prefixes
        /// <summary>
        /// The prefix of a line object name.
        /// </summary>
        public const string LinePrefix = "Line";

        /// <summary>
        /// The prefix of a text object name.
        /// </summary>
        public const string TextPrefix = "Text";

        /// <summary>
        /// The prefix of a image object name.
        /// </summary>
        public const string ImagePrefix = "Image";

        /// <summary>
        /// The prefix of a sticky notes.
        /// </summary>
        public const string StickyNotePrefix = "StickyNote";

        /// <summary>
        /// The prefix of a drawable holder object.
        /// </summary>
        public const string DrawableHolderPrefix = "DrawableHolder";

        /// <summary>
        /// The name of the attached objects object.
        /// </summary>
        public const string AttachedObject = "AttachedObjects";
        #endregion

        /// <summary>
        /// The distance to a drawable that is used by default to place objects.
        /// </summary>
        public static readonly Vector3 distanceToDrawable = new(0, 0, 0.0001f);

        /// <summary>
        /// The path to the folder drawable folder of the saved files. This is saved in a field because multiple
        /// methods of this class and other classes use it.
        /// </summary>
        public static readonly string drawablePath = Application.persistentDataPath + "/Drawable/";

        /// <summary>
        /// The path to the drawable image folder. This is saved in a field because multiple
        /// methods of this class and other classes use it.
        /// </summary>
        public static readonly string imagePath = drawablePath + "Image/";

        /// <summary>
        /// The direction's for moving. Will needed for sticky note menu.
        /// </summary>
        public enum MoveDirection
        {
            Left,
            Right,
            Up,
            Down,
            Forward,
            Back
        }

        /// <summary>
        /// The constructor, it loads the default values for the current values.
        /// </summary>
        static ValueHolder()
        {
            currentPrimaryColor = UnityEngine.Random.ColorHSV();
            currentSecondaryColor = Color.clear;
            currentOutlineThickness = 0.4f;
            currentFontSize = 0.5f;
            currentThickness = 0.01f;
            currentLineKind = GameDrawer.LineKind.Solid;
            currentColorKind = GameDrawer.ColorKind.Monochrome;
            currentTiling = 1f;
            currentOrderInLayer = 1;
        }
    }
}