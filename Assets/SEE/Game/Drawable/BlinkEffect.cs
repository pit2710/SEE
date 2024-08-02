﻿using HighlightPlus;
using SEE.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SEE.Game.Drawable
{
    /// <summary>
    /// Component that is required to show which object has been selected.
    /// It makes the respective object blink.
    /// </summary>
    public class BlinkEffect : MonoBehaviour
    {
        /// <summary>
        /// Whether the loop is active.
        /// </summary>
        private bool loopOn;

        /// <summary>
        /// The renderer of the attached game object.
        /// </summary>
        private new Renderer renderer;

        /// <summary>
        /// The renderers of the attached game object (for mind map nodes)
        /// </summary>
        private List<Renderer> renderers;

        /// <summary>
        /// The canvas of the attached game object.
        /// </summary>
        private Canvas canvas;

        /// <summary>
        /// The highlight effect of the attached game object.
        /// </summary>
        private HighlightEffect highlight;

        /// <summary>
        /// Executed as long as the Blink Effect Component is active.
        /// It ensures that the corresponding renderer/canvas/highlight effect
        /// is toggled on and off, thus creating a blinking effect.
        /// </summary>
        /// <returns>Nothing, only the seconds to wait.</returns>
        public IEnumerator Blink()
        {
            while (loopOn)
            {
                if (renderer != null)
                {
                    /// Makes the renderer blink.
                    renderer.enabled = false;
                    yield return new WaitForSeconds(0.2f);
                    renderer.enabled = true;
                    yield return new WaitForSeconds(0.5f);
                }
                else if (renderers != null)
                {
                    /// Makes the renderers blink.
                    foreach (Renderer renderer in renderers)
                    {
                        renderer.enabled = false;
                    }
                    yield return new WaitForSeconds(0.2f);
                    foreach (Renderer renderer in renderers)
                    {
                        renderer.enabled = true;
                    }
                    yield return new WaitForSeconds(0.5f);
                }
                else if (canvas != null)
                {
                    /// Makes the canvas blink.
                    canvas.enabled = false;
                    yield return new WaitForSeconds(0.2f);
                    canvas.enabled = true;
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    /// Makes the highlight blink.
                    highlight.enabled = false;
                    yield return new WaitForSeconds(0.2f);
                    highlight.enabled = true;
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        /// <summary>
        /// Deactivates the blink effect.
        /// It enables the renderer, the canvas, or the child renderers
        /// (depending on what is present).
        /// If a highlight effect was used, it will be destroyed.
        /// Subsequently, the Blink Effect Component is destroyed.
        /// </summary>
        public void Deactivate()
        {
            loopOn = false;
            if (renderer != null)
            {
                renderer.enabled = true;
            }
            else if (renderers != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
                }
            }
            else if (canvas != null)
            {
                canvas.enabled = true;
            }
            else
            {
                Destroyer.Destroy(highlight);
            }
            Destroy(this);
        }

        /// <summary>
        /// Deactivate the blink effect of the given object.
        /// </summary>
        /// <param name="obj">The object which blink effect should be deactivated.</param>
        public static void Deactivate(GameObject obj)
        {
            if (obj != null && obj.GetComponent<BlinkEffect>() != null)
            {
                obj.GetComponent<BlinkEffect>().Deactivate();
            }
        }

        /// <summary>
        /// Executed upon assigning the component.
        /// It searches for a renderer, child renderers, or a highlight effect.
        /// If none of these components are present, a highlight effect is created,
        /// and then the blink loop is initiated.
        /// </summary>
        private void Start()
        {
            GameObject obj = gameObject;

            if (renderer == null && obj.GetComponent<Renderer>() != null
                && obj.GetComponentsInChildren<Renderer>().Length == 1)
            {
                /// Sets the renderer if available.
                renderer = obj.GetComponent<Renderer>();
            }
            else if (obj.GetComponentsInChildren<Renderer>().Length > 0)
            {
                /// Sets the renderers if available.
                renderers = obj.GetComponentsInChildren<Renderer>().ToList();
            }
            else if (obj.GetComponent<Canvas>() != null)
            {
                /// Sets the canvas if available.
                /// Needed for an image.
                canvas = obj.GetComponent<Canvas>();
            }
            else if (obj.GetComponent<HighlightEffect>() != null)
            {
                /// Sets the highlight if available.
                highlight = obj.GetComponent<HighlightEffect>();
            }
            else
            {
                /// Creates a highlight effect, if none of the other cases apply.
                highlight = GameHighlighter.EnableGlowOutline(obj);
            }
            loopOn = true;
            StartCoroutine(Blink());
        }

        /// <summary>
        /// Removes the renderer of the fill out.
        /// </summary>
        /// <param name="obj">The object which has a fill out.</param>
        public static void RemoveFillOutFromEffect(GameObject obj)
        {
            if (obj != null && obj.GetComponent<BlinkEffect>() != null)
            {
                GameObject fillOut = GameFinder.FindChild(obj, ValueHolder.FillOut);
                BlinkEffect effect = obj.GetComponent<BlinkEffect>();
                if (fillOut != null)
                {
                    effect.renderers.Remove(fillOut.GetComponent<Renderer>());
                }
            }
        }

        /// <summary>
        /// Adds the renderer of the fill out.
        /// </summary>
        /// <param name="obj">The object which has a fill out.</param>
        public static void AddFillOutToEffect(GameObject obj)
        {
            if (obj != null && obj.GetComponent<BlinkEffect>() != null)
            {
                GameObject fillOut = GameFinder.FindChild(obj, ValueHolder.FillOut);
                BlinkEffect effect = obj.GetComponent<BlinkEffect>();
                if (fillOut != null && fillOut.GetComponent<Renderer>() != null)
                {
                    if (effect.renderers != null)
                    {
                        effect.renderers.Add(fillOut.GetComponent<Renderer>());
                    } 
                    else if (effect.renderer != null)
                    {
                        effect.Deactivate();
                        obj.AddComponent<BlinkEffect>();
                    }
                }
            }
        }
    }
}
