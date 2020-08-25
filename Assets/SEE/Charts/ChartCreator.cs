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

using UnityEngine;

namespace SEE.Charts.Scripts
{
    /// <summary>
    ///     Used to create new charts.
    /// </summary>
    public class ChartCreator : MonoBehaviour
    {
        /// <summary>
        ///     The prefab for a new chart.
        /// </summary>
        [SerializeField] private GameObject chartPrefab;

        /// <summary>
        ///     The <see cref="Canvas" /> on which the chart is created.
        /// </summary>
        [SerializeField] private Transform chartsCanvas;

        /// <summary>
        ///     Initializes the new chart as GameObject.
        /// </summary>
        public void CreateChart()
        {
            Instantiate(chartPrefab, chartsCanvas).GetComponent<ChartContent>();
        }

        /// <summary>
        ///     Deactivates the chart canvas.
        /// </summary>
        public void CloseCharts()
        {
            gameObject.SetActive(false);
        }
    }
}