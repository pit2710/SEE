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
    /// A city for the differences between two revisions of a software
    /// stored in a version control system (VCS).
    /// </summary>
    public class DiffCity : SEECity
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
        public DirectoryPath VCSPath = new();

        /// <summary>
        /// The VCS identifier for the revision that constitutes the baseline of the
        /// comparison (the 'old' revision).
        /// </summary>
        [ShowInInspector, Tooltip("Old revision"),
            TabGroup(VCSFoldoutGroup), RuntimeTab(VCSFoldoutGroup)]
        public string OldRevision = string.Empty;

        /// <summary>
        /// The VCS identifier for the revision that constitutes the new revision
        /// against which the <see cref="OldRevision"/> is to be compared.
        /// </summary>
        [ShowInInspector, Tooltip("New revision"),
            TabGroup(VCSFoldoutGroup), RuntimeTab(VCSFoldoutGroup)]
        public string NewRevision = string.Empty;

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

        /// <summary>
        /// Label of attribute <see cref="RepositoryPath"/> in the configuration file.
        /// </summary>
        private const string repositoryPathLabel = "RepositoryPath";

        /// <summary>
        /// Label of attribute <see cref="OldRevision"/> in the configuration file.
        /// </summary>
        private const string oldRevisionLabel = "OldRevision";

        /// <summary>
        /// Label of attribute <see cref="NewRevision"/> in the configuration file.
        /// </summary>
        private const string newRevisionLabel = "NewRevision";

        protected override void Save(ConfigWriter writer)
        {
            base.Save(writer);
            writer.Save(VersionControlSystem.ToString(), versionControlSystemLabel);
            VCSPath.Save(writer, vcsPathLabel);
            writer.Save(OldRevision, oldRevisionLabel);
            writer.Save(NewRevision, newRevisionLabel);
        }

        protected override void Restore(Dictionary<string, object> attributes)
        {
            base.Restore(attributes);
            ConfigIO.RestoreEnum(attributes, versionControlSystemLabel, ref VersionControlSystem);
            VCSPath.Restore(attributes, vcsPathLabel);
            ConfigIO.Restore(attributes, oldRevisionLabel, ref OldRevision);
            ConfigIO.Restore(attributes, newRevisionLabel, ref NewRevision);
        }

        #endregion Config I/O
    }
}
