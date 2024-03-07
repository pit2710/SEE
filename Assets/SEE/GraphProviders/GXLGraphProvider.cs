﻿using SEE.DataModel.DG;
using SEE.DataModel.DG.IO;
using SEE.Game.City;
using SEE.Utils;
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using SEE.Utils.Paths;

namespace SEE.GraphProviders
{
    /// <summary>
    /// A graph provider reading a graph from a GXL file.
    /// </summary>
    [Serializable]
    public class GXLGraphProvider : FileBasedGraphProvider
    {
        /// <summary>
        /// Reads and returns a graph from a GXL file with <see cref="Path"/> where
        /// the <see cref="AbstractSEECity.HierarchicalEdges"/> of <paramref name="city"/>
        /// specifies the hierarchical edges in the GXL file to be interested for nesting
        /// nodes and <see cref="AbstractSEECity.SourceCodeDirectory"/>
        /// of <paramref name="city"/> determins the base of the resulting graph.
        /// The loaded graph will have that value for its <see cref="Graph.BasePath"/>.
        /// It will be used to turn relative file-system paths into absolute ones. It should be chosen as
        /// the root directory in which the source code can be found.
        /// </summary>
        /// <param name="graph">input graph (currently ignored)</param>
        /// <param name="city">where the <see cref="AbstractSEECity.HierarchicalEdges"/>
        /// and <see cref="AbstractSEECity.SourceCodeDirectory"/> will be retrieved</param>
        /// <returns>loaded graph</returns>
        /// <exception cref="ArgumentException">thrown in case <see cref="Path"/>
        /// is undefined or does not exist or <paramref name="city"/> is null</exception>
        /// <exception cref="NotImplementedException">thrown if <paramref name="graph"/>
        /// has nodes; this case is currently not yet handled</exception>
        public override async UniTask<Graph> ProvideAsync(Graph graph, AbstractSEECity city)
        {
            CheckArguments(city);
            return await GraphReader.LoadAsync(Path, city.HierarchicalEdges, city.SourceCodeDirectory.Path);
        }

        public override GraphProviderKind GetKind()
        {
            return GraphProviderKind.GXL;
        }
    }
}
