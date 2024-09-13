﻿using SEE.UI.RuntimeConfigMenu;
using SEE.Utils.Config;
using SEE.Utils.Paths;
using SEE.VCS;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Game.City
{
    /// <summary>
    /// A VCS based city.
    ///
    /// This class alone should not be used directly but rather with its inheritors:
    /// <list type="bullet">
    /// <item>
    /// <see cref="CommitCity"/>
    /// </item>
    /// <item>
    /// <see cref="BranchCity"/>
    /// </item>
    /// </list>
    ///
    /// </summary>
    public class VCSCity : SEECity
    {
        /// <summary>
        /// Name of the Inspector foldout group for the version control system
        /// (VCS) setttings.
        /// </summary>
        protected const string VCSFoldoutGroup = "VCS";

        /// <summary>
        /// The version control system identifier, to get the source code from both revision.
        /// </summary>
        [ShowInInspector, Tooltip("Version control system. Currently only Git is supported."),
            TabGroup(VCSFoldoutGroup), RuntimeTab(VCSFoldoutGroup)]
        internal VCSKind VersionControlSystem = VCSKind.Git;

        /// <summary>
        /// The path to the VCS containing the two revisions to be compared.
        /// </summary>
        [ShowInInspector, Tooltip("VCS path"),
            TabGroup(VCSFoldoutGroup), RuntimeTab(VCSFoldoutGroup)]
        public DataPath VCSPath = new();


        #region Config I/O

        //--------------------------------
        // Configuration file input/output
        //--------------------------------

        /// <summary>
        /// Label of attribute <see cref="VCSPath"/> in the configuration file.
        /// </summary>
        private const string vcsPathLabel = "VCSPath";

        /// <summary>
        /// Label of attribute <see cref="VersionControlSystem"/> in the configuration file.
        /// </summary>
        private const string versionControlSystemLabel = "VersionControlSystem";

        protected override void Save(ConfigWriter writer)
        {
            base.Save(writer);
            writer.Save(VersionControlSystem.ToString(), versionControlSystemLabel);
            VCSPath.Save(writer, vcsPathLabel);
        }

        protected override void Restore(Dictionary<string, object> attributes)
        {
            base.Restore(attributes);
            ConfigIO.RestoreEnum(attributes, versionControlSystemLabel, ref VersionControlSystem);
            VCSPath.Restore(attributes, vcsPathLabel);
        }

        #endregion Config I/O
    }
}
