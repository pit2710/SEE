using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LibGit2Sharp;
using SEE.DataModel.DG;
using SEE.DataModel.DG.IO.Git;
using SEE.Game.City;
using SEE.GameObjects;
using SEE.UI.Notification;
using SEE.UI.RuntimeConfigMenu;
using SEE.Utils;
using SEE.Utils.Config;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace SEE.GraphProviders
{
    /// <summary>
    /// This provider analyses all branches of a given git repository specified in <see cref="VCSCity.VCSPath"/> within the given time range (<see cref="BranchCity.Date"/>).
    ///
    /// This provider will collect all commits from the latest to the last one before <see cref="BranchCity.Date"/>.
    ///
    /// The collected metrics are:
    /// <list type="bullet">
    /// <item>Metric.File.Commits</item>
    /// <item>Metric.File.AuthorsNumber</item>
    /// <item>Metric.File.Churn</item>
    /// <item>Metric.File.CoreDevs</item>
    /// </list>
    /// </summary>
    [Serializable]
    public class AllGitBranchesSingleGraphProvider : SingleGraphProvider
    {
        #region Attributes

        /// <summary>
        /// The List of filetypes that get included/excluded.
        /// </summary>
        [OdinSerialize]
        [ShowInInspector, ListDrawerSettings(ShowItemCount = true),
                         Tooltip("Paths and their inclusion/exclusion status."), RuntimeTab(GraphProviderFoldoutGroup),
                         HideReferenceObjectPicker]
        public IDictionary<string, bool> PathGlobbing = new Dictionary<string, bool>()
                         {
                             { "**/*", true }
                         };

        /// <summary>
        /// This option fill simplify the graph with <see cref="GitFileMetricsGraphGenerator.DoSimplyfiGraph"/> and combine directories.
        /// </summary>
        [OdinSerialize][ShowInInspector] public bool SimplifyGraph = false;

        /// <summary>
        /// Specifies if SEE should automatically fetch for new commits in the repository <see cref="RepositoryData"/>.
        ///
        /// This will append the path of this repo to <see cref="GitPoller"/>.
        ///
        /// Note: the repository must be fetch-able without any credentials since we cant store them securely yet.
        /// </summary>
        [OdinSerialize][ShowInInspector] public bool AutoFetch = false;

        /// <summary>
        /// The interval in seconds in which git fetch should be called.
        /// </summary>
        [OdinSerialize, ShowInInspector, EnableIf(nameof(AutoFetch)), Range(5, 200)] public int PollingInterval = 5;

        /// <summary>
        /// If file changes where picked up by the <see cref="GitPoller"/> the affected files will be marked.
        /// This filed specifies, for how long these markers should appear.
        /// </summary>
        [OdinSerialize, ShowInInspector, EnableIf(nameof(AutoFetch)), Range(5, 200)] public int MarkerTime = 10;

        #endregion

        #region Constants

        /// <summary>
        /// Label for serializing the <see cref="PathGlobbing"/> field.
        /// </summary>
        private const string pathGlobbingLabel = "PathGlobbing";

        /// <summary>
        /// Label for serializing the <see cref="SimplifyGraph"/> field.
        /// </summary>
        private const string simplifyGraphLabel = "SimplifyGraph";

        /// <summary>
        /// Label for serializing the <see cref="AutoFetch"/> field.
        /// </summary>
        private const string autoFetchLabel = "AutoFetch";

        #endregion

        #region Methods

        /// <summary>
        /// Checks if all attributes are set correctly.
        /// Otherwise, an exception is thrown.
        /// </summary>
        /// <param name="branchCity">The <see cref="BranchCity"/> where this provider was executed.</param>
        /// <exception cref="ArgumentException">If one attribute is not set correctly.</exception>
        private void CheckAttributes(BranchCity branchCity)
        {
            if (branchCity.Date == "" || !DateTime.TryParseExact(branchCity.Date, "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _))
            {
                throw new ArgumentException("Date is not set or cant be parsed");
            }

            if (branchCity.VCSPath.Path == "" || !Directory.Exists(branchCity.VCSPath.Path))
            {
                throw new ArgumentException("Repository path is not set or does not exists");
            }
        }

        /// <summary>
        /// Returns or adds the <see cref="GitPoller"/> component to the current gameobject/code city <paramref name="city"/>.
        /// </summary>
        /// <param name="city">The code city where the <see cref="GitPoller"/> component should be attached.</param>
        /// <returns>The <see cref="GitPoller"/> component</returns>
        private GitPoller GetOrAddGitPollerComponent(SEECity city)
        {
            if (city.TryGetComponent(out GitPoller poller))
            {
                return poller;
            }

            GitPoller newPoller = city.gameObject.AddComponent<GitPoller>();
            newPoller.CodeCity = city;
            newPoller.PollingInterval = PollingInterval;
            newPoller.MarkerTime = MarkerTime;
            return newPoller;
        }

        /// <summary>
        /// Provides the graph of the git history.
        /// </summary>
        /// <param name="graph">The graph of the previous provider.</param>
        /// <param name="city">The city where the graph should be displayed.</param>
        /// <param name="changePercentage">The current status of the process.</param>
        /// <param name="token">Can be used to cancel the action.</param>
        /// <returns>The graph generated from the git repository <see cref="RepositoryData"/>.</returns>
        public override async UniTask<Graph> ProvideAsync(Graph graph, AbstractSEECity city,
            Action<float> changePercentage = null,
            CancellationToken token = default)
        {
            if (city is not BranchCity branchCity)
            {
                throw new ArgumentException("Only a Branch city is supported");
            }

            CheckAttributes(branchCity);

            Graph task = await UniTask.RunOnThreadPool(() => GetGraph(graph, changePercentage, branchCity),
                cancellationToken: token);
            if (AutoFetch)
            {
                if (city is not BranchCity seeCity)
                {
                    ShowNotification.Warn("Can't enable auto fetch",
                        "Automatically fetching git repos is only supported in SEECity");
                    return task;
                }

                // Only add the poller when in playing mode
                if (Application.isPlaying)
                {
                    GitPoller poller = GetOrAddGitPollerComponent(seeCity);
                    poller.WatchedRepositories.Add(branchCity.VCSPath.Path);
                }
            }

            return task;
        }

        /// <summary>
        /// Calculates and returns the actual graph.
        ///
        /// This method will collect all commit from all branches which are not older than <see cref="Date"/>.
        /// Then from all these commits the metrics are calculated with <see cref="GitFileMetricProcessor.ProcessCommit(LibGit2Sharp.Commit,LibGit2Sharp.Patch)"/>.
        /// </summary>
        /// <param name="graph">The input graph.</param>
        /// <param name="changePercentage">The current status of the process.</param>
        /// <param name="branchCity">The <see cref="BranchCity"/> from which the provider was called.</param>
        /// <returns>The generated output graph.</returns>
        private Graph GetGraph(Graph graph, Action<float> changePercentage, BranchCity branchCity)
        {
            graph.BasePath = branchCity.VCSPath.Path;
            string[] pathSegments = graph.BasePath.Split(Path.DirectorySeparatorChar);

            string repositoryName = pathSegments[^1];

            GraphUtils.NewNode(graph, repositoryName, GraphUtils.RepositoryTypeName, pathSegments[^1]);

            // Assuming that CheckAttributes() was already executed so that the date string is not empty nor malformed.
            DateTime timeLimit = DateTime.ParseExact(branchCity.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            using (Repository repo = new Repository(graph.BasePath))
            {
                CommitFilter filter;

                filter = new CommitFilter { IncludeReachableFrom = repo.Branches };

                IEnumerable<Commit> commitList = repo.Commits
                    .QueryBy(filter)
                    .Where(commit =>
                        DateTime.Compare(commit.Author.When.Date, timeLimit) > 0)
                    // Filter out merge commits
                    .Where(commit => commit.Parents.Count() <= 1);

                // select all files of this repo
                IEnumerable<string> files = repo.Branches
                    .SelectMany(x => VCSGraphProvider.ListTree(x.Tip.Tree))
                    .Distinct();

                GitFileMetricProcessor metricProcessor = new(repo, PathGlobbing, files);

                int counter = 0;
                int commitLength = commitList.Count();
                foreach (Commit commit in commitList)
                {
                    metricProcessor.ProcessCommit(commit);
                    changePercentage?.Invoke(Mathf.Clamp((float)counter / commitLength, 0, 0.98f));
                    counter++;
                }

                metricProcessor.CalculateTruckFactor();
                GitFileMetricsGraphGenerator.FillGraphWithGitMetrics(metricProcessor, graph, repositoryName,
                    SimplifyGraph);
                changePercentage(1f);
            }

            return graph;
        }

        /// <summary>
        /// Returns the kind of this provider.
        /// </summary>
        /// <returns>Returns <see cref="SingleGraphProviderKind.GitAllBranches"/>.</returns>
        public override SingleGraphProviderKind GetKind()
        {
            return SingleGraphProviderKind.GitAllBranches;
        }

        /// <summary>
        /// Saves the attributes of this provider to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="ConfigWriter"/> to save the attributes to.</param>
        protected override void SaveAttributes(ConfigWriter writer)
        {
            writer.Save(PathGlobbing as Dictionary<string, bool>, pathGlobbingLabel);
            writer.Save(SimplifyGraph, simplifyGraphLabel);
            writer.Save(AutoFetch, autoFetchLabel);
        }

        /// <summary>
        /// Restores the attributes of this provider from <paramref name="attributes"/>.
        /// </summary>
        /// <param name="attributes">The attributes to restore from.</param>
        protected override void RestoreAttributes(Dictionary<string, object> attributes)
        {
            bool simplifyGraph = SimplifyGraph;
            ConfigIO.Restore(attributes, simplifyGraphLabel, ref simplifyGraph);
            SimplifyGraph = simplifyGraph;
            bool autoFetch = AutoFetch;
            ConfigIO.Restore(attributes, autoFetchLabel, ref autoFetch);
            AutoFetch = autoFetch;
            IDictionary<string, bool> pathGlob = PathGlobbing;
            ConfigIO.Restore(attributes, "PathGlobing", ref pathGlob);

        }

        #endregion
    }
}
