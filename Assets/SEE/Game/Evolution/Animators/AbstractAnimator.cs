﻿//Copyright 2020 Florian Garbade

//Permission is hereby granted, free of charge, to any person obtaining a
//copy of this software and associated documentation files (the "Software"),
//to deal in the Software without restriction, including without limitation
//the rights to use, copy, modify, merge, publish, distribute, sublicense,
//and/or sell copies of the Software, and to permit persons to whom the Software
//is furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
//PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
//USE OR OTHER DEALINGS IN THE SOFTWARE.

using SEE.Layout;
using SEE.Utils;
using System;
using UnityEngine;

namespace SEE.Game.Evolution
{
    /// <summary>
    /// The kind of differences for an animated object.
    /// </summary>
    public enum Difference
    {
        None,    // if there is no difference whatsoever
        Added,   // if the element was added
        Changed, // if the element was changed
        Deleted, // if the element was deleted
    }

    /// <summary>
    /// An abstract animator that makes it simple to swap an existing type of animation.
    /// For example there could be a RotationAnimator and BounceAnimator for different
    /// kind of states.
    /// </summary>
    public abstract class AbstractAnimator
    {
        /// <summary>
        /// Defines the default time an animation takes in seconds.
        /// </summary>
        public const int DefaultAnimationTime = 2;

        /// <summary>
        /// Defines the maximum time an animation is allowed to take in seconds.
        /// </summary>
        private float _maxAnimationTime;
        /// <summary>
        /// If true animations are skipped and the new values are applied instantly.
        /// </summary>
        private bool _animationsDisabled = false;

        /// <summary>
        /// Defines the maximum time an animation is allowed to take in seconds.
        /// </summary>
        public float MaxAnimationTime { get => _maxAnimationTime; set => _maxAnimationTime = value; }

        /// <summary>
        /// If set to true animations are skipped and the new values are applied instantly.
        /// </summary>
        public bool AnimationsDisabled { get => _animationsDisabled; set => _animationsDisabled = value; }

        /// <summary>
        /// Creates a new animator with a given maximal animation time.
        /// </summary>
        /// <param name="maxAnimationTime">The maximum time the animation is allowed to run.</param>
        public AbstractAnimator(float maxAnimationTime = DefaultAnimationTime)
        {
            MaxAnimationTime = maxAnimationTime;
        }

        /// <summary>
        /// Animates the node transformation of given <paramref name="gameObject"/>. If needed, a <paramref name="callback"/> 
        /// that is invoked with <paramref name="gameObject"/> as a parameter when the animation is finished can be passed.
        /// 
        /// Precondition: the caller of this method AnimateTo() is a Monobehaviour instance attached to a game object.
        /// 
        /// Let C be the caller (a Monobehaviour) of AnimateTo(). Then the actual callback will be as follows: 
        ///   O.<paramref name="callback"/>(<paramref name="gameObject"/>) where O is the game object caller C 
        ///   is attached to (its gameObject).
        /// </summary>
        /// <param name="gameObject">game object to be animated</param>
        /// <param name="nodeTransform">the node transformation to be applied</param>
        /// <param name="difference">whether the node attached to <paramref name="gameObject"/> was added, 
        /// modified, or deleted w.r.t. to the previous graph</param>
        /// <param name="callback">an optional callback</param>
        public void AnimateTo(GameObject gameObject, ILayoutNode nodeTransform, Difference difference, Action<object> callback = null)
        {
            gameObject.AssertNotNull("gameObject");
            nodeTransform.AssertNotNull("nodeTransform");

            if (AnimationsDisabled)
            {
                // Note: nodeTransform.position.y denotes the ground of the game object, not
                // the center as normally in Unity. The following position is the one in
                // Unity's terms where the y co-ordinate denotes the center.
                Vector3 position = nodeTransform.CenterPosition;
                position.y += nodeTransform.LocalScale.y / 2;
                gameObject.transform.position = position;
                gameObject.transform.localScale = nodeTransform.LocalScale;
                callback?.Invoke(gameObject);
            }
            else if (callback == null)
            {
                AnimateToInternalWithCallback(gameObject, nodeTransform, difference, null, "", null);
            }
            else
            {
                AnimateToInternalWithCallback(gameObject, nodeTransform, difference, ((MonoBehaviour)callback.Target).gameObject, callback.Method.Name, callback);
            }
        }

        /// <summary>
        /// Abstract method called by <see cref="AnimateTo"/> for an animation with a callback. At the
        /// end of the animation, the method <paramref name="callbackName"/> will be called for the
        /// game object <paramref name="callBackTarget"/> with <paramref name="gameObject"/> as 
        /// parameter if <paramref name="callBackTarget"/> is not null. If <paramref name="callBackTarget"/>
        /// equals null, no callback happens.
        /// </summary>
        /// <param name="gameObject">game object to be animated</param>
        /// <param name="nodeTransform">the node transformation to be applied</param>
        /// <param name="difference">whether the node attached to <paramref name="gameObject"/> was added, 
        /// modified, or deleted w.r.t. to the previous graph</param>
        /// <param name="callBackTarget">an optional game object that should receive the callback</param>
        /// <param name="callbackName">the method name of the callback</param>
        protected abstract void AnimateToInternalWithCallback
            (GameObject gameObject,
             ILayoutNode nodeTransform,
             Difference difference,
             GameObject callBackTarget,
             string callbackName,
             Action<object> callback);
    }
}