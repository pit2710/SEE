﻿using SEE.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO flo doc:
/// </summary>
public abstract class AbstractCCAAnimator
{
    /// <summary>
    /// TODO flo: doc
    /// </summary>
    public const int DefaultAnimationTime = 2;

    private float _maxAnimationTime;
    private bool _animationsDisabled = false;

    /// <summary>
    /// Defines the maximum time, in seconds, the animation is allowed to take.
    /// </summary>
    public float MaxAnimationTime { get => _maxAnimationTime; set => _maxAnimationTime = value; }

    /// <summary>
    /// TODO flo doc:
    /// </summary>
    public bool AnimationsDisabled { get => _animationsDisabled; set => _animationsDisabled = value; }

    /// <summary>
    /// TODO flo doc:
    /// </summary>
    /// <param name="maxAnimationTime">The maximum time the animation is allowed to run.</param>
    public AbstractCCAAnimator(float maxAnimationTime = DefaultAnimationTime)
    {
        this.MaxAnimationTime = maxAnimationTime;
    }

    /// <summary>
    /// TODO flo doc:
    /// </summary>
    /// <param name="node"></param>
    /// <param name="gameObject"></param>
    /// <param name="position"></param>
    /// <param name="scale"></param>
    /// <param name="callback"></param>
    public void AnimateTo(Node node, GameObject gameObject, Vector3 position, Vector3 scale, Action<object> callback = null)
    {
        node.AssertNotNull("node");
        gameObject.AssertNotNull("gameObject");
        position.AssertNotNull("position");
        scale.AssertNotNull("scale");

        if (AnimationsDisabled)
        {
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale;
            callback?.Invoke(gameObject);
        }
        else if(callback == null)
        {
            AnimateToInternal(node, gameObject, position, scale);
        }
        else
        {
            AnimateToAndInternal(node, gameObject, position, scale, ((MonoBehaviour)callback.Target).gameObject, callback.Method.Name);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="gameObject"></param>
    /// <param name="position"></param>
    /// <param name="scale"></param>
    protected abstract void AnimateToInternal(Node node, GameObject gameObject, Vector3 position, Vector3 scale);

    /// <summary>
    /// TODO flo doc:
    /// </summary>
    /// <param name="node"></param>
    /// <param name="gameObject"></param>
    /// <param name="position"></param>
    /// <param name="scale"></param>
    /// <param name="callback"></param>
    protected abstract void AnimateToAndInternal(Node node, GameObject gameObject, Vector3 position, Vector3 scale, GameObject callBackTarget, string callbackName);
}
