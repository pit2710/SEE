using System;
using System.Linq;
using MoreLinq.Extensions;
using SEE.Utils;

namespace SEE.DataModel.DG.IO.Git
{
    /// <summary>
    /// Generates a <see cref="Graph"/> from the metrics of a <see cref="GitFileMetricRepository"/> instance.
    /// </summary>
    public static class GitFileMetricsGraphGenerator
    {
        #region Constants

        /// <summary>
        /// The name of the number of authors metric
        /// </summary>
        private const string NumberOfAuthorsMetricName = "Metric.File.AuthorsNumber";

        /// <summary>
        /// The name of the number of commits metric
        /// </summary>
        private const string NumberOfCommitsMetricName = "Metric.File.Commits";

        /// <summary>
        /// The name of the churn metric
        /// </summary>
        private const string NumberOfFileChurnMetricName = "Metric.File.Churn";

        /// <summary>
        /// The Name of the number of coredevs metric
        /// </summary>
        private const string TruckFactorMetricName = "Metric.File.CoreDevs";

        #endregion

        /// <summary>
        /// Fills and adds all files and their metrics from <paramref name="metricRepository"/> to the passed graph <paramref name="initialGraph"/>.
        ///
        /// </summary>
        /// <param name="metricRepository">The metrics to add</param>
        /// <param name="initialGraph">The initial graph where the files and metrics should be generated</param>
        /// <param name="repositoryName">The name of the repository</param>
        /// <param name="simplifyGraph">If the final graph should be simplified</param>
        public static void FillGraphWithGitMetrics(GitFileMetricRepository metricRepository, Graph initialGraph,
            string repositoryName, bool simplifyGraph)
        {
            FillGraphWithGitMetrics(metricRepository, initialGraph, repositoryName, simplifyGraph, "");
        }

        /// <summary>
        /// Fills and adds all files and their metrics from <paramref name="metricRepository"/> to the passed graph <paramref name="initialGraph"/>.
        ///
        /// </summary>
        /// <param name="metricRepository">The metrics to add</param>
        /// <param name="initialGraph">The initial graph where the files and metrics should be generated</param>
        /// <param name="repositoryName">The name of the repository</param>
        /// <param name="simplifyGraph">If the final graph should be simplified</param>
        /// <param name="idSuffix">A suffix to add to all nodes. This can be used when the same repository is loaded in two code cities at the same time</param>
        public static void FillGraphWithGitMetrics(GitFileMetricRepository metricRepository, Graph initialGraph,
            string repositoryName, bool simplifyGraph, string idSuffix)
        {
            if (initialGraph == null || metricRepository == null)
            {
                return;
            }

            foreach (var file in metricRepository.FileToMetrics)
            {
                Node n = GraphUtils.GetOrAddNode(file.Key, initialGraph.GetNode(repositoryName + idSuffix),
                    initialGraph,
                    idSuffix: idSuffix);
                n.SetInt(NumberOfAuthorsMetricName, file.Value.Authors.Count);
                n.SetInt(NumberOfCommitsMetricName, file.Value.NumberOfCommits);
                n.SetInt(NumberOfFileChurnMetricName, file.Value.Churn);
                n.SetInt(TruckFactorMetricName, file.Value.TruckFactor);
                if (file.Value.Authors.Any())
                {
                    n.SetString("Metric.File.Authors", String.Join(',', file.Value.Authors));
                }

                foreach (var authorChurn in file.Value.AuthorsChurn)
                {
                    n.SetInt(NumberOfFileChurnMetricName + ":" + authorChurn.Key, authorChurn.Value);
                }
            }

            if (simplifyGraph)
            {
                foreach (var child in initialGraph.GetRoots()[0].Children().ToList())
                {
                    DoSimplifyGraph(child, initialGraph);
                }
            }
        }

        /// <summary>
        /// Simplifies a given graph by combining common directories.
        ///
        /// If a directory has only other directories as children, their paths will be combined.
        /// For instance the file structure:
        /// <code>
        /// root/
        ///├─ dir1/
        ///│  ├─ dir2/
        ///│  │  ├─ dir6/
        ///│  ├─ dir3/
        ///│  │  ├─ file1.md
        ///│  │  ├─ dir5/
        ///  ├─ dir4/
        /// </code>
        /// will become:
        /// <code>
        ///root/
        /// ├─ dir1/dir2/dir6/
        /// ├─ dir1/dir4/
        /// ├─ dir1/dir3/
        /// │  ├─ file1.md
        /// │  ├─ dir5/
        /// </code>
        ///
        /// </summary>
        /// <param name="root">The root element of the graph to analyse from</param>
        /// <param name="g">The <see cref="Graph"/> itself. Needed for cleaning up</param>
        private static void DoSimplifyGraph(Node root, Graph g)
        {
            if (root.Children().ToList().TrueForAll(x => x.Type != "file") && root.Children().Any())
            {
                foreach (var child in root.Children().ToList())
                {
                    child.Reparent(root.Parent);
                    DoSimplifyGraph(child, g);
                }

                if (g.ContainsNode(root))
                {
                    g.RemoveNode(root);
                }
            }
            else
            {
                foreach (var node in root.Children().Where(x => x.Type == "directory").ToList())
                {
                    DoSimplifyGraph(node, g);
                }
            }
        }
    }
}
