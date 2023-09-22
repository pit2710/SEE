﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SEE.DataModel.DG;
using SEE.Utils;
using Newtonsoft.Json;

namespace SEE.Net.Dashboard.Model.Issues
{
    /// <summary>
    /// An issue representing cyclic dependencies.
    /// </summary>
    [Serializable]
    public class CycleIssue : Issue
    {
        /// <summary>
        /// The type of the relation between source and target
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public readonly string DependencyType;

        /// <summary>
        /// The source entity
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public readonly string SourceEntity;

        /// <summary>
        /// The source entity type
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public readonly string SourceEntityType;

        /// <summary>
        /// The source filename
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public readonly string SourcePath;

        /// <summary>
        /// The source line number
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public readonly int SourceLine;

        /// <summary>
        /// The internal name of the corresponding entity
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public readonly string SourceLinkName;

        /// <summary>
        /// The target entity
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public readonly string TargetEntity;

        /// <summary>
        /// The target entity type
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public readonly string TargetEntityType;

        /// <summary>
        /// The target filename
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public readonly string TargetPath;

        /// <summary>
        /// The target line number
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public readonly int TargetLine;

        /// <summary>
        /// The internal name of the corresponding entity
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public readonly string TargetLinkName;

        public CycleIssue()
        {
            // Necessary for generics shenanigans in IssueRetriever.
        }

        [JsonConstructor]
        protected CycleIssue(string dependencyType, string sourceEntity, string sourceEntityType,
                             string sourcePath, int sourceLine, string sourceLinkName, string targetEntity,
                             string targetEntityType, string targetPath, int targetLine, string targetLinkName)
        {
            this.DependencyType = dependencyType;
            this.SourceEntity = sourceEntity;
            this.SourceEntityType = sourceEntityType;
            this.SourcePath = sourcePath;
            this.SourceLine = sourceLine;
            this.SourceLinkName = sourceLinkName;
            this.TargetEntity = targetEntity;
            this.TargetEntityType = targetEntityType;
            this.TargetPath = targetPath;
            this.TargetLine = targetLine;
            this.TargetLinkName = targetLinkName;
        }

        public override async UniTask<string> ToDisplayString()
        {
            string explanation = await DashboardRetriever.Instance.GetIssueDescription($"CY{ID}");
            return "<style=\"H2\">Cyclic dependency</style>"
                   + $"\nSource: {SourcePath} ({SourceEntityType}), Line {SourceLine}\n".WrapLines(WrapAt)
                   + $"\nTarget: {TargetPath} ({TargetEntityType}), Line {TargetLine}\n".WrapLines(WrapAt)
                   + $"\n{explanation.WrapLines(WrapAt)}";
        }

        public override string IssueKind => "CY";

        public override NumericAttributeNames AttributeName => NumericAttributeNames.Cycle;

        public override IEnumerable<SourceCodeEntity> Entities => new[]
        {
            new SourceCodeEntity(SourcePath, SourceLine, null, SourceEntity),
            new SourceCodeEntity(TargetPath, TargetLine, null, TargetEntity)
        };
    }
}