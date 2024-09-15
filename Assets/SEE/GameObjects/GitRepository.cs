using System;
using System.Collections.Generic;
using SEE.Game.City;
using SEE.UI.RuntimeConfigMenu;
using SEE.Utils.Paths;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace SEE.GraphProviders
{
    /// <summary>
    /// Represents the needed information about a git repository for a <see cref="SEECityEvolution"/>.
    /// </summary>
    [Serializable]
    public class GitRepository
    {
        /// <summary>
        /// Used for the tab name in runtime config menu.
        /// </summary>
        private const string graphProviderFoldoutGroup = "Data";

        /// <summary>
        /// The path to the git repository.
        /// </summary>
        [ShowInInspector, Tooltip("Path to the git repository."), HideReferenceObjectPicker,
            RuntimeTab(graphProviderFoldoutGroup)]
        public DataPath RepositoryPath = new();

        /// <summary>
        /// The list of file globbings for file inclusion/exclusion.
        /// The key is the globbing pattern and the value is the inclusion status.
        /// If the latter is true, the pattern is included, otherwise it is excluded.
        /// </summary>
        [OdinSerialize]
        [ShowInInspector, ListDrawerSettings(ShowItemCount = true),
         Tooltip("Path globbings and whether they are inclusive (true) or exclusive (false)."),
            RuntimeTab(graphProviderFoldoutGroup),
         HideReferenceObjectPicker]
        public Dictionary<string, bool> PathGlobbing = new()
        {
            { "", false }
        };

    }
}
