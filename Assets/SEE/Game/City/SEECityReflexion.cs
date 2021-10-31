using System;
using System.Collections.Generic;
using System.Linq;
using SEE.DataModel.DG;
using SEE.Utils;
using UnityEngine;

namespace SEE.Game.City
{
    /// <summary>
    /// A code city that supports architectural mappings from
    /// implementation nodes onto architecture nodes.
    /// </summary>
    public class SEECityReflexion : SEECity
    {
        /// <summary>
        /// The path to the GXL file containing the implementation graph data.
        /// </summary>
        public DataPath GxlImplementationPath = new DataPath();

        /// <summary>
        /// The path to the GXL file containing the architecture graph data.
        /// </summary>
        public DataPath GxlArchitecturePath = new DataPath();

        /// <summary>
        /// The path to the GXL file containing the mapping graph data.
        /// </summary>
        public DataPath GxlMappingPath = new DataPath();

        /// <summary>
        /// Name of this code city.
        /// </summary>
        public string CityName = "Reflexion Analysis";

        /// <summary>
        /// First, if a graph was already loaded, everything will be reset by calling <see cref="Reset"/>.
        /// Second, the graph data from the three GXL files are loaded. The loaded graph is available
        /// in <see cref="LoadedGraph"/> afterwards.
        ///
        /// This method loads only the data, but does not actually render the graph.
        /// </summary>
        public override void LoadData()
        {
            if (string.IsNullOrEmpty(GxlArchitecturePath.Path))
            {
                Debug.LogError("Architecture graph path is empty.\n");
            }
            else if (string.IsNullOrEmpty(GxlImplementationPath.Path))
            {
                Debug.LogError("Implementation graph path is empty.\n");
            }
            else
            {
                if (LoadedGraph != null)
                {
                    Reset();
                }

                Graph ArchitectureGraph = LoadGraph(GxlArchitecturePath.Path);
                Graph ImplementationGraph = LoadGraph(GxlImplementationPath.Path);
                Graph MappingGraph;
                if (string.IsNullOrEmpty(GxlMappingPath.Path))
                {
                    Debug.LogWarning("Mapping graph path is empty. Will create new mapping from scratch.\n");
                    MappingGraph = new Graph();
                }
                else
                {
                    MappingGraph = LoadGraph(GxlMappingPath.Path);
                }

                LoadedGraph = GenerateFullGraph(ArchitectureGraph, ImplementationGraph, MappingGraph, CityName);
                Debug.Log($"Loaded graph {LoadedGraph.Name}");
            }
        }

        /// <summary>
        /// Generates the full graph from the three sub-graphs <see cref="ImplementationGraph"/>,
        /// <see cref="ArchitectureGraph"/> and <see cref="MappingGraph"/> by combining them into one, returning
        /// the result. Note that the name of the three graphs may be modified.
        /// 
        /// Pre-condition: <see cref="ImplementationGraph"/>, <see cref="ArchitectureGraph"/> and
        /// <see cref="MappingGraph"/> are not <c>null</c> (i.e. have been loaded).
        /// </summary>
        /// <returns>Full graph obtained by combining architecture, implementation and mapping</returns>
        private static Graph GenerateFullGraph(Graph ArchitectureGraph, Graph ImplementationGraph, Graph MappingGraph, 
                                               string Name)
        {
            if (ImplementationGraph == null || ArchitectureGraph == null || MappingGraph == null)
            {
                throw new ArgumentException("All three sub-graphs must be loaded before generating "
                                            + "the full graph.");
            }

            // We set the name for the implementation graph, because its name will be used for the merged graph.
            ImplementationGraph.Name = Name;

            // We merge architecture and implementation first.
            // If there are duplicate IDs, try to remedy this by appending a suffix to the ID.
            List<string> graphsOverlap = GraphsOverlap(ImplementationGraph, ArchitectureGraph);
            string archSuffix = null;
            if (graphsOverlap.Count > 0)
            {
                archSuffix = "-A";
                Debug.LogWarning($"Overlapping graph elements found, will append '{archSuffix}' suffix."
                                 + $"Offending elements: {string.Join(", ", graphsOverlap)}");
            }
            Graph mergedGraph = ImplementationGraph.MergeWith(ArchitectureGraph, archSuffix);
            
            // Then we add the mappings, again checking if any IDs overlap.
            graphsOverlap = GraphsOverlap(mergedGraph, MappingGraph);
            string mapSuffix = null;
            if (graphsOverlap.Count > 0)
            {
                mapSuffix = "-M";
                Debug.LogWarning($"Overlapping graph elements found, will append '{mapSuffix}' suffix."
                                 + $"Offending elements: {string.Join(", ", graphsOverlap)}");
            }
            return mergedGraph.MergeWith(MappingGraph, mapSuffix);

            
            #region Local Functions

            // Returns any intersecting elements (node IDs, edge IDs) between the two given graphs.
            List<string> GraphsOverlap(Graph aGraph, Graph anotherGraph) => NodeIntersection(aGraph, anotherGraph).Concat(EdgeIntersection(aGraph, anotherGraph)).ToList();

            // Returns the intersection of the node IDs of the two graphs.
            IEnumerable<string> NodeIntersection(Graph aGraph, Graph anotherGraph) => aGraph.Nodes().Select(x => x.ID).Intersect(anotherGraph.Nodes().Select(x => x.ID));

            // Returns the intersection of the edge IDs of the two graphs.
            IEnumerable<string> EdgeIntersection(Graph aGraph, Graph anotherGraph) => aGraph.Edges().Select(x => x.ID).Intersect(anotherGraph.Edges().Select(x => x.ID));
            
            #endregion
        }

        private static (Graph, Graph, Graph) DisassembleFullGraph(Graph FullGraph)
        {
            //TODO: How do we differentiate between the three graphs, assuming all three can be freely edited/appended?
            throw new NotImplementedException();
        }

        public override void SaveData()
        {
            //TODO
            throw new NotImplementedException();
        }

        //------------------------------------------------
        // TODO: Anything below this line not yet updated.
        //------------------------------------------------

        //--------------------------------
        // Configuration file input/output
        //--------------------------------

        protected override void Save(ConfigWriter writer)
        {
            base.Save(writer);
            // If any attribute is added to this class that should be contained in the
            // configuration file, then do not forget to add the necessary
            // statements here.
        }

        protected override void Restore(Dictionary<string, object> attributes)
        {
            base.Restore(attributes);
            // If any attribute is added to this class that should be restored from the
            // configuration file, then do not forget to add the necessary
            // statements here.
        }
    }
}