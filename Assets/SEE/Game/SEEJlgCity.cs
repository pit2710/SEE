﻿
using Assets.SEE.DataModel;
using Assets.SEE.DataModel.IO;
using SEE.DataModel;
using System;
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

        public override void LoadData()
        {
            base.LoadData();
            LoadJLG();
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadJLG()
        {
            if (string.IsNullOrEmpty(jlgPath))
            {
                Debug.LogError("Path to JLG source file cannot be empty.\n");
            }
            else if (!File.Exists(jlgPath))
            {
                Debug.LogError("Source file does not exist at that path.\n");
            }
            else
            {
                JLGParser jlgParser = new JLGParser(jlgPath);
                GameObject jlgVisualisationGameObject = new GameObject();
                jlgVisualisationGameObject.transform.parent = transform;
                jlgVisualisationGameObject.name = Tags.JLGVisualisation;
                jlgVisualisationGameObject.tag = Tags.JLGVisualisation;

                ParsedJLG parsedJLG = jlgParser.Parse();
                jlgVisualisationGameObject.AddComponent<Runtime.JLGVisualizer>().parsedJLG = parsedJLG;
            }

        }
    }
}
