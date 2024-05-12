using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LibGit2Sharp;
using SEE.DataModel.DG;
using SEE.Game.City;
using SEE.UI.RuntimeConfigMenu;
using SEE.Utils;
using SEE.Utils.Config;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace SEE.GraphProviders
{
    [Serializable]
    public class AllBranchGitProvider : GitRepositoryProvider<Graph>
    {
        /// <summary>
        /// The date limit until commits should be analysed
        /// </summary>
        [OdinSerialize]
        [ShowInInspector, InspectorName("Date Limit"),
         Tooltip("The date until commits should be analysed (DD/MM/YYYY)"), RuntimeTab(GraphProviderFoldoutGroup)]
        public string Date = "";


        [OdinSerialize] [ShowInInspector] public int AuthorThreshhold { get; set; } = 1;

        [OdinSerialize] [ShowInInspector] public int CommitThreshhold { get; set; } = 1;
        [OdinSerialize] [ShowInInspector] public bool SimplifyGraph { get; set; }

        [OdinSerialize] [ShowInInspector] public bool AutoFetch { get; set; }

        #region Constants

        private const string NumberOfAuthorsMetricName = "Metric.File.AuthorsNumber";

        private const string NumberOfCommitsMetricName = "Metric.File.Commits";

        private const string NumberOfFileChurnMetricName = "Metric.File.Churn";

        private const string TruckFactorMetricName = "Metric.File.TruckFactor";

        /// <summary>
        /// Used in the calculation of the truck factor.
        ///
        /// Specifies the minimum ratio of the file churn the core devs should be responsible for 
        /// </summary>
        private const float TruckFactorCoreDevRatio = 0.8f;

        #endregion

        public override UniTask<Graph> ProvideAsync(Graph graph, AbstractSEECity city,
            Action<float> changePercentage = null,
            CancellationToken token = default)
        {
            UniTask<Graph> graphTask = UniTask.FromResult(GetGraph(graph));

            return graphTask;
        }

        private Graph GetGraph(Graph graph)
        {
            graph.BasePath = RepositoryPath.Path;
            string[] pathSegments = RepositoryPath.Path.Split(Path.DirectorySeparatorChar);

            string repositoryName = pathSegments[^1];

            GraphUtils.NewNode(graph, repositoryName, "Repository", pathSegments[^1]);

            DateTime timeLimit = DateTime.ParseExact(Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            IEnumerable<string> includedFiles = PathGlobbing
                .Where(path => path.Value)
                .Select(path => path.Key);

            IEnumerable<string> excludedFiles = PathGlobbing
                .Where(path => !path.Value)
                .Select(path => path.Key);

            using (var repo = new Repository(RepositoryPath.Path))
            {
                IEnumerable<Commit> commitList = repo.Commits
                    .QueryBy(new CommitFilter { IncludeReachableFrom = repo.Branches })
                    .Where(commit => DateTime.Compare(commit.Author.When.Date, timeLimit) > 0)
                    // Filter out merge commits
                    .Where(commit => commit.Parents.Count() == 1);

                Dictionary<string, GitFileMetricsCollector> fileMetrics = new();

                foreach (var commit in commitList)
                {
                    var changedFilesPath = repo.Diff.Compare<Patch>(commit.Tree, commit.Parents.First().Tree);


                    foreach (var changedFile in changedFilesPath)
                    {
                        string filePath = changedFile.Path;
                        if (!includedFiles.Contains(Path.GetExtension(filePath)) ||
                            excludedFiles.Contains(Path.GetExtension(filePath)))
                        {
                            continue;
                        }

                        if (!fileMetrics.ContainsKey(filePath))
                        {
                            fileMetrics.Add(filePath,
                                new GitFileMetricsCollector(1, new HashSet<string> { commit.Author.Email },
                                    changedFile.LinesAdded + changedFile.LinesDeleted));

                            fileMetrics[filePath].AuthorsChurn.Add(commit.Author.Email,
                                changedFile.LinesAdded + changedFile.LinesDeleted);
                        }
                        else
                        {
                            fileMetrics[filePath].NumberOfCommits += 1;
                            fileMetrics[filePath].Authors.Add(commit.Author.Email);
                            fileMetrics[filePath].Churn += changedFile.LinesAdded + changedFile.LinesDeleted;
                            fileMetrics[filePath].AuthorsChurn.GetOrAdd(commit.Author.Email, 0);
                            fileMetrics[filePath].AuthorsChurn[commit.Author.Email] +=
                                (changedFile.LinesAdded + changedFile.LinesDeleted);
                        }
                    }
                }

                foreach (var file in fileMetrics)
                {
                    file.Value.TruckFactor = CalculateTruckFactor(file.Value.AuthorsChurn);
                }

                foreach (var file in fileMetrics)
                {
                    Node n = GraphUtils.GetOrAddNode(file.Key, graph.GetNode(repositoryName), graph);
                    n.SetInt(NumberOfAuthorsMetricName, file.Value.Authors.Count);
                    n.SetInt(NumberOfCommitsMetricName, file.Value.NumberOfCommits);
                    n.SetInt(NumberOfFileChurnMetricName, file.Value.Churn);
                    n.SetInt(TruckFactorMetricName, file.Value.TruckFactor);
                
                }

                if (SimplifyGraph)
                {
                    foreach (var child in graph.GetRoots().First().Children().ToList())
                    {
                        DoSimplyfiGraph(child, graph);
                    }
                }
            }

            return graph;
        }


        /// <summary>
        /// Calculates the truck factor with a LOC-based heuristic algorithm by Yamashita et al. cited by. Ferreira et. al
        ///
        /// Soruce/Math: https://doi.org/10.1145/2804360.2804366, https://doi.org/10.1007/s11219-019-09457-2
        /// </summary>
        /// <returns></returns>
        private static int CalculateTruckFactor(Dictionary<string, int> developersChurn)
        {
            int totalChurn = developersChurn.Select(x => x.Value).Sum();

            HashSet<string> coreDevs = new();

            float cumulativeRatio = 0;
            // Sorting devs by their number of changed files 
            List<string> sortedDevs =
                developersChurn
                    .OrderByDescending(x => x.Value)
                    .Select(x => x.Key)
                    .ToList();
            // Selecting the coreDevs which are responsible for at least 80% of the total churn of a file
            while (cumulativeRatio <= TruckFactorCoreDevRatio)
            {
                string dev = sortedDevs.First();
                float devRatio = (float)developersChurn[dev] / totalChurn;
                cumulativeRatio += devRatio;
                coreDevs.Add(dev);
                sortedDevs.Remove(dev);
            }

            return coreDevs.Count;
        }

        private void DoSimplyfiGraph(Node root, Graph g)
        {
            if (root.Children().ToList().All(x => x.Type != "file"))
            {
                foreach (var child in root.Children().ToList())
                {
                    child.Reparent(root.Parent);
                    // child.ID = root.ID + Path.AltDirectorySeparatorChar + child.ID;
                    DoSimplyfiGraph(child, g);

                    //root.Children().Remove(root);
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
                    DoSimplyfiGraph(node, g);
                }
            }
        }

        // private Node GetNode(string path, Graph graph, Node current = null)
        // {
        //     string[] pathSplit = path.Split(Path.AltDirectorySeparatorChar);
        //     if (pathSplit.Length == 1)
        //     {
        //         return current.Children().First(x => x.ID == pathSplit.First());
        //     }
        // }


        public override GraphProviderKind GetKind()
        {
            return GraphProviderKind.VCS;
        }

        protected override void SaveAttributes(ConfigWriter writer)
        {
            Dictionary<string, bool> pathGlobbing = string.IsNullOrEmpty(PathGlobbing.ToString()) ? null : PathGlobbing;
            writer.Save(pathGlobbing, pathGlobbingLabel);
        }

        private const string pathGlobbingLabel = "PathGlobbing";

        protected override void RestoreAttributes(Dictionary<string, object> attributes)
        {
            throw new System.NotImplementedException();
        }
    }

    public class GitFileMetricsCollector
    {
        public int NumberOfCommits { get; set; }

        public HashSet<string> Authors { get; set; }

        public Dictionary<string, int> AuthorsChurn { get; set; }

        public int TruckFactor { get; set; }

        /// <summary>
        /// Total sum of changed lines (added and removed)
        /// </summary>
        public int Churn { get; set; }

        public GitFileMetricsCollector(int numberOfCommits, HashSet<string> authors, int churn)
        {
            NumberOfCommits = numberOfCommits;
            Authors = authors;
            Churn = churn;
            AuthorsChurn = new();
            TruckFactor = 0;
        }
    }
}