﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SEE.DataModel;
using SEE;

namespace SEE.Layout
{
    public class CoseNodeSublayoutValues
    {
        /// <summary>
        /// the bounds of the node relative to its sublayout root 
        /// </summary>
        public Rect relativeRect = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Indicates whether the node is a sublayout root
        /// </summary>
        private bool isSubLayoutRoot = false;

        /// <summary>
        /// Indicates whether the node is a sublayout node
        /// </summary>
        private bool isSubLayoutNode = false;

        /// <summary>
        /// The root node of the sublayout the node is part of 
        /// </summary>
        private CoseNode subLayoutRoot = null;

        /// <summary>
        /// 
        /// </summary>
        private CoseSublayout sublayout;

        public bool IsSubLayoutRoot { get => isSubLayoutRoot; set => isSubLayoutRoot = value; }
        public bool IsSubLayoutNode { get => isSubLayoutNode; set => isSubLayoutNode = value; }
        public CoseNode SubLayoutRoot { get => subLayoutRoot; set => subLayoutRoot = value; }
        public CoseSublayout Sublayout { get => sublayout; set => sublayout = value; }



        /// <summary>
        /// updates the relative bounding rect
        /// </summary>
        /// <param name="left">the left position</param>
        /// <param name="right">the right position</param>
        /// <param name="top">the top position</param>
        /// <param name="bottom">the bottom position</param>
        public void UpdateRelativeRect(float left, float right, float top, float bottom)
        {
            relativeRect = new Rect(left, top, right - left, bottom - top);
        }
    }
}

