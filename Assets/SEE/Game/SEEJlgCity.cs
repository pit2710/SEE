﻿// Copyright 2020 Lennart Kipka
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using SEE.DataModel;
using System.IO;
using UnityEngine;

namespace SEE.Game
{
    public class SEEJlgCity : SEECity
    {
        /// <summary>
        /// The full path to the jlg source file.
        /// </summary>
        public string jlgPath;

        /// <summary>
        /// Returns the concatenation of pathPrefix and jlgPath. That is the complete
        /// absolute path to the JLG file containing the runtime trace data.
        /// </summary>
        /// <returns>concatenation of pathPrefix and jlgPath</returns>
        public string JLGPath()
        {
            return PathPrefix + jlgPath;
        }

        public override void LoadData()
        {
            base.LoadData();
            LoadJLG();
        }

        /// <summary>
        /// Loads the data from the given jlg file into a parsedJLG object and gives the object to a GameObject, that has a component to visualize it in the running game.
        /// </summary>
        private void LoadJLG()
        {
            string path = JLGPath();

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Path to JLG source file must not be empty.\n");
            }
            else if (!File.Exists(path))
            {
                Debug.LogErrorFormat("Source file does not exist at that path {0}.\n", path);
            }
            else
            {
                GameObject jlgVisualisationGameObject = new GameObject();
                jlgVisualisationGameObject.transform.parent = transform;
                jlgVisualisationGameObject.name = Tags.JLGVisualization;
                jlgVisualisationGameObject.tag = Tags.JLGVisualization;

                jlgVisualisationGameObject.AddComponent<Runtime.JLGVisualizer>().jlgFilePath = path;
            }

        }
    }
}
