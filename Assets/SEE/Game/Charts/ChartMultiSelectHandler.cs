﻿// Copyright 2020 Robert Bohnsack
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using SEE.Controls;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SEE.Game.Charts
{
    /// <summary>
    /// Handles selection of multiple markers in selection mode.
    /// </summary>
    public class ChartMultiSelectHandler : MonoBehaviour, IPointerDownHandler, IDragHandler,
        IPointerUpHandler
    {
        /// <summary>
        /// The rectangle used to visualize the selection process for the user.
        /// </summary>
        [SerializeField] protected RectTransform selectionRect;

        /// <summary>
        /// Needed for access to <see cref="Scripts.ChartContent.AreaSelection"/>.
        /// </summary>
        protected ChartContent chartContent;

        /// <summary>
        /// The position the user started the drag at.
        /// </summary>
        protected Vector3 startingPos;

        /// <summary>
        /// Assigns <see cref="chartContent"/>.
        /// </summary>
        private void Awake()
        {
            chartContent = transform.parent.GetComponent<ChartContent>();
        }

        public bool IsSelecting() => selectionRect.gameObject.activeSelf;

        protected void SortMinMax(Vector2 fst, Vector2 snd, out Vector2 min, out Vector2 max)
        {
            float x0, x1, y0, y1;

            if (fst.x < snd.x)
            {
                x0 = fst.x;
                x1 = snd.x;
            }
            else
            {
                x0 = snd.x;
                x1 = fst.x;
            }
            if (fst.y < snd.y)
            {
                y0 = fst.y;
                y1 = snd.y;
            }
            else
            {
                y0 = snd.y;
                y1 = fst.y;
            }

            min = new Vector2(x0, y0);
            max = new Vector2(x1, y1);
        }

        /// <summary>
        /// Activates and sets starting position of <see cref="selectionRect"/>.
        /// </summary>
        /// <param name="eventData">Contains the position data.</param>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            selectionRect.gameObject.SetActive(true);
            selectionRect.position = eventData.pressPosition;
            startingPos = selectionRect.position;
            selectionRect.sizeDelta = new Vector2(0.0f, 0.0f);
            if (!Input.GetKey(KeyCode.LeftControl))
            {
                foreach (InteractableObject interactableObject in FindObjectsOfType<InteractableObject>())
                {
                    interactableObject.SetHoverFlags(0, true);
                    interactableObject.SetSelect(false, true);
                }
            }
        }

        /// <summary>
        /// Resizes the <see cref="selectionRect"/> to make it span from <see cref="startingPos"/> to
        /// <see cref="PointerEventData.position"/>.
        /// </summary>
        /// <param name="eventData">Contains the position data.</param>
        public virtual void OnDrag(PointerEventData eventData)
        {
            bool negative = false;
            Vector3 lossyScale = selectionRect.lossyScale;
            Vector2 sizeDelta = Vector2.zero;

            if (eventData.position.x - startingPos.x < 0)
            {
                selectionRect.sizeDelta = new Vector2(
                    Mathf.Abs(eventData.position.x - startingPos.x) / selectionRect.lossyScale.x,
                    (eventData.position.y - startingPos.y) / lossyScale.y
                );
                sizeDelta = selectionRect.sizeDelta;
                selectionRect.position = new Vector3(
                    startingPos.x - sizeDelta.x / 2 * lossyScale.x,
                    startingPos.y + sizeDelta.y / 2 * lossyScale.y,
                    0
                );
                negative = true;
            }

            if (eventData.position.y - startingPos.y < 0)
            {
                if (negative)
                {
                    selectionRect.sizeDelta = new Vector2(
                        selectionRect.sizeDelta.x,
                        Mathf.Abs(eventData.position.y - startingPos.y) / selectionRect.lossyScale.y
                    );
                    selectionRect.position = new Vector3(
                        selectionRect.position.x,
                        startingPos.y - selectionRect.sizeDelta.y / 2 * lossyScale.y,
                        0
                    );
                }
                else
                {
                    selectionRect.sizeDelta = new Vector2(
                        (eventData.position.x - startingPos.x) / lossyScale.x,
                        Mathf.Abs(eventData.position.y - startingPos.y) / lossyScale.y
                    );
                    sizeDelta = selectionRect.sizeDelta;
                    selectionRect.position = new Vector3(
                        startingPos.x + sizeDelta.x / 2 * lossyScale.x,
                        startingPos.y - sizeDelta.y / 2 * lossyScale.y,
                        0
                    );
                    negative = true;
                }
            }

            if (!negative)
            {
                selectionRect.sizeDelta = new Vector2((eventData.position.x - startingPos.x) / lossyScale.x, (eventData.position.y - startingPos.y) / lossyScale.y);
                sizeDelta = selectionRect.sizeDelta;
                selectionRect.position = new Vector3(startingPos.x + sizeDelta.x / 2 * lossyScale.x, startingPos.y + sizeDelta.y / 2 * lossyScale.y, 0);
            }

            float x0, x1, y0, y1;
            if (startingPos.x < eventData.position.x)
            {
                x0 = startingPos.x;
                x1 = eventData.position.x;
            }
            else
            {
                x0 = eventData.position.x;
                x1 = startingPos.x;
            }
            if (startingPos.y < eventData.position.y)
            {
                y0 = startingPos.y;
                y1 = eventData.position.y;
            }
            else
            {
                y0 = eventData.position.y;
                y1 = startingPos.y;
            }

            SortMinMax(startingPos, eventData.position, out Vector2 min, out Vector2 max);
            chartContent.AreaHover(min, max);
        }

        /// <summary>
        /// Highlights all markers in <see cref="selectionRect"/> and deactivates it.
        /// </summary>
        /// <param name="eventData">Contains the position data.</param>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            chartContent.UnhoverAll();
            SortMinMax(startingPos, eventData.position, out Vector2 min, out Vector2 max);
            chartContent.AreaSelection(min, max);
            selectionRect.gameObject.SetActive(false);
        }
    }
}