﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SEE.DataModel.DG;

namespace SEE.Net.Dashboard.Model.Issues
{
    /// <summary>
    /// Contains information about an issue in the source code.
    /// </summary>
    [Serializable]
    public abstract class Issue
    {
        // A note: Due to how the JSON serializer works with inheritance, fields in here can't be readonly.

        /// <summary>
        /// Represents the state of an issue.
        /// </summary>
        public enum IssueState
        {
            added, changed, removed // note: names must be lowercase for the serialization to work
        }

        /// <summary>
        /// A kind-wide Id identifying the issue across analysis versions
        /// </summary>
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int ID;

        /// <summary>
        /// In diff-queries, this indicates whether the issue is “Removed”,
        /// i.e. contained in the base-version but not any more in the current version or “Added”,
        /// i.e. it was not contained in the base-version but is contained in the current version
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "state", Required = Required.Default)]
        public IssueState State;

        /// <summary>
        /// Whether or not the issue is suppressed or disabled via a control comment.
        /// </summary>
        /// <remarks>
        /// This column is only available for projects where importing of suppressed issues is configured
        /// </remarks>
        [JsonProperty(PropertyName = "suppressed", Required = Required.Default)]
        public bool Suppressed;

        /// <summary>
        /// The justification provided in the source-code via a control comment
        /// </summary>
        [JsonProperty(PropertyName = "justification", Required = Required.AllowNull)]
        public string Justification;

        /// <summary>
        /// Tags that are attached to the issue.
        /// </summary>
        [JsonProperty(PropertyName = "tag", Required = Required.Default)]
        public IList<IssueTag> Tag;

        /// <summary>
        /// The comments that were made on the issue.
        /// </summary>
        /// <remarks>
        /// This column is not available for CSV output.
        /// This column is not returned by default and must be explicitly requested via the “columns” parameter.
        /// </remarks>
        [JsonProperty(PropertyName = "comments", Required = Required.Default)]
        public IList<IssueComment> Comments;

        /// <summary>
        /// The dashboard users associated with the issue via VCS blaming, CI path mapping,
        /// CI user name mapping and dashboard user name mapping.
        /// </summary>
        [JsonProperty(PropertyName = "owners", Required = Required.Always)]
        public IList<UserRef> Owners;

        protected Issue()
        {
            // Necessary for inheritance with Newtonsoft.Json to work properly
        }

        public Issue(int id, IssueState state, bool suppressed, string justification,
                     IList<IssueTag> tag, IList<IssueComment> comments, IList<UserRef> owners)
        {
            ID = id;
            State = state;
            Suppressed = suppressed;
            Justification = justification;
            Tag = tag;
            Comments = comments;
            Owners = owners;
        }

        /// <summary>
        /// An issue tag as returned by the Issue-List API.
        /// </summary>
        [Serializable]
        public readonly struct IssueTag
        {
            /// <summary>
            /// Use this for displaying the tag.
            /// </summary>
            [JsonProperty(PropertyName = "tag", Required = Required.Always)]
            public readonly string Tag;

            /// <summary>
            /// An RGB hex color in the form #RRGGBB directly usable by css.
            /// The colors are best suited to draw a label on bright background and to contain white letters for labeling.
            /// </summary>
            [JsonProperty(PropertyName = "color", Required = Required.Always)]
            public readonly string Color;

            public IssueTag(string tag, string color)
            {
                Tag = tag;
                Color = color;
            }
        }

        /// <summary>
        /// Describes an issue comment.
        /// </summary>
        [Serializable]
        public readonly struct IssueComment
        {
            /// <summary>
            /// The loginname of the user that created the comment.
            /// </summary>
            [JsonProperty(PropertyName = "username", Required = Required.Always)]
            public readonly string Username;

            /// <summary>
            /// The recommended display name of the user that wrote the comment.
            /// </summary>
            [JsonProperty(PropertyName = "userDisplayName", Required = Required.Always)]
            public readonly string UserDisplayName;

            /// <summary>
            /// The Date when the comment was created.
            /// </summary>
            [JsonProperty(PropertyName = "date", Required = Required.Always)]
            public readonly DateTime Date;

            /// <summary>
            /// The Date when the comment was created for UI-display.
            /// It is formatted as a human-readable string relative to query time, e.g. 2 minutes ago.
            /// </summary>
            [JsonProperty(PropertyName = "displayDate", Required = Required.Always)]
            public readonly string DisplayDate;

            /// <summary>
            /// The comment text.
            /// </summary>
            [JsonProperty(PropertyName = "text", Required = Required.Always)]
            public readonly string Text;

            /// <summary>
            /// The id for comment deletion.
            /// When the requesting user is allowed to delete the comment,
            /// contains an id that can be used to mark the comment as deleted using another API.
            /// </summary>
            /// <remarks>
            /// This is never set when the Comment is returned as the result of an Issue-List query.
            /// </remarks>
            [JsonProperty(PropertyName = "commentDeletionId", Required = Required.Default)]
            public readonly string CommentDeletionId;

            public IssueComment(string username, string userDisplayName, DateTime date,
                                string displayDate, string text, string commentDeletionId)
            {
                Username = username;
                UserDisplayName = userDisplayName;
                Date = date;
                DisplayDate = displayDate;
                Text = text;
                CommentDeletionId = commentDeletionId;
            }
        }

        /// <summary>
        /// Number of characters to wrap the string in <see cref="ToDisplayStringAsync"/> at.
        /// </summary>
        protected const int WrapAt = 120;

        /// <summary>
        /// Returns a string suitable for display in a TextMeshPro which describes this issue.
        /// TextMeshPro's rich tags are used in here, so the string shouldn't be displayed elsewhere.
        /// </summary>
        /// <returns>A string describing this issue which is suitable for display in a TextMeshPro</returns>
        public abstract UniTask<string> ToDisplayStringAsync();

        /// <summary>
        /// The kind of issue this is.
        /// Usually an abbreviation of the type of the issue, e.g. MV for Metric Violation Issues.
        /// </summary>
        public abstract string IssueKind { get; }

        /// <summary>
        /// The numeric attribute name for this issue kind.
        /// </summary>
        public abstract NumericAttributeNames AttributeName { get; }

        /// <summary>
        /// The entities this issue references.
        /// May be empty if all referenced entities don't have a path.
        /// </summary>
        public abstract IEnumerable<SourceCodeEntity> Entities { get; }
    }
}
