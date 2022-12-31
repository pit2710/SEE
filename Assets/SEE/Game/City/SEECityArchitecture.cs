﻿using System.IO;
using SEE.DataModel.DG;
using SEE.DataModel.DG.IO;
using SEE.Game.Architecture;
using SEE.Layout.IO;
using SEE.Utils;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Game.City
{

    /// <summary>
    /// Manages settings for modelling software architectures for the reflexion analysis.
    /// </summary>
    public class SEECityArchitecture : SEECity
    {
        /// <summary>
        /// The accessor for the <see cref="ArchitectureRenderer"/>. If none exist, a new one will be created.
        /// </summary>
        public new ArchitectureRenderer Renderer
        {
            get
            {
                if (architectureRenderer == null)
                {
                    architectureRenderer = new ArchitectureRenderer(this, LoadedGraph);
                }
                return architectureRenderer;
            }
        }

        /// <summary>
        /// The <see cref="ArchitectureRenderer"/> instance.
        /// </summary>
        private ArchitectureRenderer architectureRenderer;

        /// <summary>
        /// Path to the layout file of the architecture.
        /// </summary>
        [Tooltip("The path to the layout file used for this architectural graph visualization")]
        public FilePath ArchitectureLayoutPath = new FilePath();

        /// <summary>
        /// Settings holders for the architecture elements.
        /// </summary>
        public readonly ArchitectureElementSettings[] ArchitectureElementSettings = new ArchitectureElementSettings[]
        {
            new ArchitectureElementSettings(ArchitectureElementType.Cluster),
            new ArchitectureElementSettings(ArchitectureElementType.Component)
        };

        /// <summary>
        /// Counts the newly created nodes. Used as an suffix for the default node name.
        /// </summary>
        public int NODE_COUNTER = 0;

        /// <summary>
        /// Loads the architecture graph from the GXL File with GXLPath.
        /// DrawGraph() must be used to actually render the graph afterwards.
        /// </summary>
        public void LoadGraph()
        {
            if (string.IsNullOrEmpty(GXLPath.Path))
            {
                Debug.LogError("Empty graph path.\n");
                return;
            }

            if (LoadedGraph != null)
            {
                Reset();
            }
            LoadedGraph = LoadGraph(GXLPath.Path);
        }

        /// <summary>
        /// Saves the architecture graph to the GXL file with GXLPath.
        /// </summary>
        public void SaveGraph()
        {
            if (string.IsNullOrEmpty(GXLPath.Path))
            {
                Debug.LogError("Empty graph path.\n");
                return;
            }

            if (LoadedGraph != null)
            {
                foreach (string hierarchicalEdge in HierarchicalEdges)
                {
                    GraphWriter.Save(GXLPath.Path, LoadedGraph, hierarchicalEdge);
                    break;
                }
            }
        }

        protected override void Awake()
        {
            string filename = GXLPath.Path;
            if (LoadedGraph != null)
            {
                Debug.Log("SEECityArchitecture.Awake: graph is already loaded.\n");
            }
            else if (!string.IsNullOrEmpty(filename))
            {
                LoadedGraph = LoadGraph(filename);
                // Make sure that the artificial root node is added again.
                if (LoadedGraph != null)
                {
                    SetNodeEdgeRefs(LoadedGraph, gameObject);
                }
                else
                {
                    Debug.LogError($"SEECityArchitecture.Awake: Could not laod GXL file {filename} of architecture {name}.\n");
                }
            }
            else
            {
                Debug.LogError($"SEECityArchitecture.Awake: GXL file of architecture {name} is undefined.\n");
            }
        }

        /// <summary>
        /// Saves the graph layout and the underlaying graph to disk. The files are named by the given architecture name.
        /// The Layout is stored as SLD file, while the graph is stored as GXL.
        /// </summary>
        /// <param name="name">the architecture name.</param>
        public void SaveLayoutAndGraph(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("Architecture name is empty!.\n");
                return;
            }

            string gxlFilePath = GenerateFileName(name, Filenames.GXLExtension);
            string sldFilePath = GenerateFileName(name, Filenames.SLDExtension);
            SLDWriter.Save(sldFilePath, AllNodeDescendants(gameObject));
            foreach (string hierarchicalEdge in HierarchicalEdges)
            {
                GraphWriter.Save(gxlFilePath, LoadedGraph, hierarchicalEdge);
                break;
            }
        }

        /// <summary>
        /// Generates the filename for saving the architecture graph and its layout.
        /// If the the output folder does not exist yet, it will be created.
        /// </summary>
        /// <param name="name">The name of the architecture.</param>
        /// <param name="extension">The file extension to use.</param>
        /// <returns></returns>
        private string GenerateFileName(string name, string extension)
        {
            string subFolder = Application.streamingAssetsPath + "\\Architecture\\Output";
            if (!Directory.Exists(subFolder))
                Directory.CreateDirectory(subFolder);
            return subFolder + $"\\{name}{extension}";
        }

        /// <summary>
        /// Does a fresh Draw call for the graph.
        /// </summary>
        public new void ReDrawGraph()
        {
            if (LoadedGraph == null)
            {
                Debug.LogError("Cant redraw graph! No graph was loaded.\n");
                return;
            }
            DeleteGraphGameObjects();
            DrawGraph();
        }

        /// <summary>
        /// Creates a new empty graph for architecture modelling.
        /// </summary>
        public void NewGraph()
        {
            Assert.IsNull(LoadedGraph);
            LoadedGraph = new Graph("Architecture");
            architectureRenderer = new ArchitectureRenderer(this, LoadedGraph);
            architectureRenderer.PrepareNewArchitectureGraph(gameObject);
        }

        /// <summary>
        /// Renders the loaded architecture graph.
        /// </summary>
        public new void DrawGraph()
        {
            Assert.IsNotNull(LoadedGraph);
            architectureRenderer = new ArchitectureRenderer(this, LoadedGraph);
            architectureRenderer.Draw(gameObject);
        }
    }
}