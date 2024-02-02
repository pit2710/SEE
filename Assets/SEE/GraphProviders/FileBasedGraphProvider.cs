﻿using SEE.Game.City;
using SEE.Utils.Config;
using SEE.Utils.Paths;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SEE.GraphProviders
{
    /// <summary>
    /// Common superclass of all graph providers which read their data from
    /// a single file.
    /// </summary>
    [Serializable]
    internal abstract class FileBasedGraphProvider : GraphProvider
    {
        /// <summary>
        /// The path to the file containing the additional data to be added to a graph.
        /// </summary>
        [Tooltip("Path to the input file."), HideReferenceObjectPicker]
        public FilePath Path = new();

        /// <summary>
        /// Checks whether the assumptions on <see cref="Path"/> and <paramref name="city"/> hold.
        /// If not, exceptions are thrown accordingly.
        /// </summary>
        /// <param name="city">to be checked</param>
        /// <exception cref="ArgumentException">thrown in case <see cref="Path"/>
        /// is undefined or does not exist or <paramref name="city"/> is null</exception>
        protected void CheckArguments(AbstractSEECity city)
        {
            if (string.IsNullOrEmpty(Path.Path))
            {
                throw new ArgumentException("Empty graph path.\n");
            }
            if (!File.Exists(Path.Path))
            {
                throw new ArgumentException($"File {Path.Path} does not exist.\n");
            }
            if (city == null)
            {
                throw new ArgumentException("The given city is null.\n");
            }
        }

        #region Config I/O

        /// <summary>
        /// The label for <see cref="Path"/> in the configuration file.
        /// </summary>
        private const string pathLabel = "path";

        protected override void SaveAttributes(ConfigWriter writer)
        {
            Path.Save(writer, pathLabel);
        }

        protected override void RestoreAttributes(Dictionary<string, object> attributes)
        {
            Path.Restore(attributes, pathLabel);
        }

        #endregion
    }
}
