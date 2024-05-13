﻿using SEE.DataModel.DG;
using SEE.Game.City;
using SEE.Utils.Config;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SEE.GraphProviders
{
    /// <summary>
    /// SingleGraphProvider is a graph provider for returning a single graph.
    ///
    /// This kind of graph provider is used in <see cref="SEECity"/>.
    /// It can also be used with a <see cref="SingleGraphPipelineProvider"/> to create a pipeline
    /// </summary>
    public abstract class SingleGraphProvider : GraphProvider<Graph, GraphProviderKind>
    {
        /// <summary>
        /// Restores the settings from <paramref name="attributes"/> under the key <paramref name="label"/>.
        /// The latter must be the label under which the settings were grouped, i.e., the same
        /// value originally passed in <see cref="Save(ConfigWriter, string)"/>.
        /// </summary>
        /// <param name="attributes">dictionary of attributes from which to retrieve the settings</param>
        /// <param name="label">the label for the settings (a key in <paramref name="attributes"/>)</param>
        public static SingleGraphProvider Restore(Dictionary<string, object> attributes, string label)
        {
            if (attributes.TryGetValue(label, out object dictionary))
            {
                Dictionary<string, object> values = dictionary as Dictionary<string, object>;
                return RestoreProvider(values);
            }
            else
            {
                throw new Exception($"A graph provider could not be found under the label {label}.");
            }
        }


        /// <summary>
        /// Restores the graph provider's attributes from <paramref name="values"/>.
        /// The parameter <paramref name="values"/> is assumed to already be the list
        /// of values. No further label look-up will be needed.
        /// </summary>
        /// <param name="values">list of values to be used to restore a graph provider</param>
        /// <returns>the resulting graph provider that was restored; it will have the
        /// <paramref name="values"/></returns>
        /// <exception cref="Exception">thrown in case the values are malformed</exception>
        /// <remarks>This method just creates a new instance using the 'kind' attribute
        /// via the <see cref="GraphProviderFactory"/>. The actual restoration of that
        /// new instance's attributes ist deferred to the subclasses, which need to
        /// implement <see cref="RestoreAttributes(Dictionary{string, object})"/>.</remarks>
        protected internal static SingleGraphProvider RestoreProvider(Dictionary<string, object> values)
        {
            GraphProviderKind kind = GraphProviderKind.SinglePipeline;
            if (ConfigIO.RestoreEnum(values, kindLabel, ref kind))
            {
                SingleGraphProvider provider = GraphProviderFactory.NewSingleGraphProviderInstance(kind);
                provider.RestoreAttributes(values);
                return provider;
            }
            else
            {
                throw new Exception($"Specification of graph provider is malformed: label {kindLabel} is missing.");
            }
        }
    }

    public abstract class MultiGraphProvider : GraphProvider<List<Graph>, MultiGraphProviderKind>
    {
        /// <summary>
        /// Restores the settings from <paramref name="attributes"/> under the key <paramref name="label"/>.
        /// The latter must be the label under which the settings were grouped, i.e., the same
        /// value originally passed in <see cref="Save(ConfigWriter, string)"/>.
        /// </summary>
        /// <param name="attributes">dictionary of attributes from which to retrieve the settings</param>
        /// <param name="label">the label for the settings (a key in <paramref name="attributes"/>)</param>
        public static MultiGraphProvider Restore(Dictionary<string, object> attributes, string label)
        {
            if (attributes.TryGetValue(label, out object dictionary))
            {
                Dictionary<string, object> values = dictionary as Dictionary<string, object>;
                return RestoreProvider(values);
            }
            else
            {
                throw new Exception($"A graph provider could not be found under the label {label}.");
            }
        }


        /// <summary>
        /// Restores the graph provider's attributes from <paramref name="values"/>.
        /// The parameter <paramref name="values"/> is assumed to already be the list
        /// of values. No further label look-up will be needed.
        /// </summary>
        /// <param name="values">list of values to be used to restore a graph provider</param>
        /// <returns>the resulting graph provider that was restored; it will have the
        /// <paramref name="values"/></returns>
        /// <exception cref="Exception">thrown in case the values are malformed</exception>
        /// <remarks>This method just creates a new instance using the 'kind' attribute
        /// via the <see cref="GraphProviderFactory"/>. The actual restoration of that
        /// new instance's attributes ist deferred to the subclasses, which need to
        /// implement <see cref="RestoreAttributes(Dictionary{string, object})"/>.</remarks>
        protected internal static MultiGraphProvider RestoreProvider(Dictionary<string, object> values)
        {
            MultiGraphProviderKind kind = MultiGraphProviderKind.MultiPipeline;
            if (ConfigIO.RestoreEnum(values, kindLabel, ref kind))
            {
                MultiGraphProvider provider = GraphProviderFactory.NewMultiGraphProviderInstance(kind);
                provider.RestoreAttributes(values);
                return provider;
            }
            else
            {
                throw new Exception($"Specification of graph provider is malformed: label {kindLabel} is missing.");
            }
        }
    }

    /// <summary>
    /// The GraphProvider class is an abstract base class that provides a framework
    /// for creating and managing graph data. Concrete subclasses of GraphProvider
    /// are responsible for providing a graph based on the input graph by
    /// implementing the method <see cref="ProvideAsync(Graph, AbstractSEECity)"/>.
    /// </summary>
    /// <typeparam name="T">The type of data which should be provided (e.g <see cref="Graph"/> for simple CodeCities or <see cref="Graph}"/> for evolution cities)</typeparam>
    public abstract class GraphProvider<T, Kind> where Kind : Enum
    {
        protected const string GraphProviderFoldoutGroup = "Data";

        /// <summary>
        /// Yields a new graph based on the input <paramref name="graph"/>.
        /// The input <paramref name="graph"/> may be empty. Subclasses are free to
        /// ignore the parameter <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph">input graph</param>
        /// <param name="city">settings possibly necessary to provide a graph</param>
        /// <param name="changePercentage">callback to report progress from 0 to 1</param>
        /// <param name="token">cancellation token</param>
        /// <returns>provided graph based on <paramref name="graph"/></returns>
        public abstract UniTask<T> ProvideAsync(T graph, AbstractSEECity city,
            Action<float> changePercentage = null,
            CancellationToken token = default);

        /// <summary>
        /// Saves the settings in the configuration file.
        ///
        /// Because we have different types of graph providers, a key-value
        /// pair will be stored first that specifies the type of provider.
        /// Only then, its attributes follow. For instance, a <see cref="GXLSingleGraphProvider"/>
        /// would be emitted as follows:
        ///
        /// {
        ///   kind : "GXL";
        ///   path : {
        ///      Root : "Absolute";
        ///      RelativePath : "";
        ///      AbsolutePath : "mydir/myfile.gxl";
        /// };
        ///
        /// where 'kind' is the label for the kind of graph provider and "GXL" in this
        /// example would be used for a <see cref="GXLSingleGraphProvider"/>. What value is
        /// used for 'kind' is decided by <see cref="GraphProviderFactory"/>.
        /// </summary>
        /// <param name="writer">to be used for writing the settings</param>
        /// <param name="label">the outer label grouping the settings</param>
        public void Save(ConfigWriter writer, string label)
        {
            writer.BeginGroup(label);
            writer.Save(GetKind().ToString(), kindLabel);
            SaveAttributes(writer);
            writer.EndGroup();
        }

        /// <summary>
        /// Returns the kind of graph provider.
        /// </summary>
        /// <returns>kind of graph provider</returns>
        public abstract Kind GetKind();

        /// <summary>
        /// Subclasses must implement this so save their attributes. This class takes
        /// care only to begin and end the grouping and to emit the key-value pair
        /// for the 'kind'.
        /// </summary>
        /// <param name="writer">to be used for writing the settings</param>
        protected abstract void SaveAttributes(ConfigWriter writer);


        /// <summary>
        /// Must be implemented by subclasses to restore their attributes.
        /// </summary>
        /// <param name="attributes">attributes that should be restored</param>
        protected abstract void RestoreAttributes(Dictionary<string, object> attributes);

        /// <summary>
        /// The label for kind of graph provider in the configuration file.
        /// </summary>
        protected const string kindLabel = "kind";
    }
}