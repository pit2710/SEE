using System;
using System.Linq;
using DG.Tweening;
using SEE.GO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SEE.Game.UI.CodeWindow
{
    /// <summary>
    /// Represents a movable, scrollable window containing source code.
    /// The source code may either be entered manually or read from a file.
    /// </summary>
    public partial class CodeWindow : BaseWindow<CodeWindow.CodeWindowValues>
    {
        /// <summary>
        /// The text displayed in the code window.
        /// </summary>
        private string Text;

        /// <summary>
        /// TextMeshPro component containing the code.
        /// </summary>
        private TextMeshProUGUI TextMesh;

        /// <summary>
        /// Path to the file whose content is displayed in this code window.
        /// May be <c>null</c> if the code window was filled using <see cref="EnterFromText"/> instead.
        /// </summary>
        public string FilePath;

        /// <summary>
        /// Whether code issues should be downloaded and added to the shown code.
        /// </summary>
        public bool ShowIssues;

        /// <summary>
        /// The line that was marked (1-indexed). Unlike <see cref="VisibleLine"/>,
        /// this line is independent of scrolling.
        /// </summary>
        private int markedLine = 1;

        /// <summary>
        /// The solution path used for the IDE integration.
        /// </summary>
        public string SolutionPath;

        /// <summary>
        /// Size of the font used in the code window.
        /// </summary>
        public float FontSize = 20f;

        /// <summary>
        /// An event which gets called whenever the scrollbar is used to scroll to a different line.
        /// Will be called after the scroll is completed.
        /// </summary>
        public readonly UnityEvent ScrollEvent = new();

        /// <summary>
        /// Number of lines within the file.
        /// </summary>
        private int lines;

        /// <summary>
        /// Path to the code window content prefab.
        /// </summary>
        private const string CODE_WINDOW_PREFAB = "Prefabs/UI/CodeWindowContent";

        /// <summary>
        /// Whether the full text of the code window should be transmitted instead of just the filename.
        /// </summary>
        private const bool SYNC_FULL_TEXT = false;

        /// <summary>
        /// Visually marks the line at the given <paramref name="lineNumber"/> and scrolls to it.
        /// Will also unmark any other line. Sets <see cref="markedLine"/> to
        /// <paramref name="lineNumber"/>.
        /// </summary>
        /// <param name="line">The line number of the line to mark and scroll to (1-indexed)</param>
        private void MarkLine(int lineNumber)
        {
            markedLine = lineNumber;
            string[] allLines = TextMesh.text.Split('\n').Select(x => x.EndsWith("</mark>") ? x.Substring(16, x.Length - 16 - 7) : x).ToArray();
            string markLine = $"<mark=#ff000044>{allLines[lineNumber - 1]}</mark>\n";
            TextMesh.text = string.Join("", allLines.Select(x => x + "\n").Take(lineNumber - 1).Append(markLine)
                                                    .Concat(allLines.Select(x => x + "\n").Skip(lineNumber).Take(lines - lineNumber - 2)));
        }

        #region Visible Line Calculation

        /// <summary>
        /// The line we're scrolling towards at the moment.
        /// Will be 0 if we're not scrolling towards anything.
        /// </summary>
        private int ScrollingTo;

        /// <summary>
        /// Number of "excess lines" within this code window.
        /// Excess lines are defined as lines which can't be accessed by the scrollbar, so
        /// they're all lines which are visible when scrolling to the lowest point of the window (except for the
        /// first line, as that one is still accessible by the scrollbar).
        /// In our case, this can be calculated by <c>ceil(window_height/line_height)</c>.
        /// </summary>
        private int excessLines;

        /// <summary>
        /// Holds the desired visible line before <see cref="Start"/> is called, because <see cref="scrollbar"/> will
        /// be undefined until then.
        /// </summary>
        private float PreStartLine = 1;

        /// <summary>
        /// The line currently at the top of the window.
        /// Will scroll smoothly to the line when changed and mark it visually.
        /// While scrolling to a line, this returns the line we're currently scrolling to.
        /// If a line outside the range of available lines is set, the highest available line number is used instead.
        /// </summary>
        /// <remarks>Only a fully visible line counts. If a line is partially obscured, the next line number
        /// will be returned.</remarks>
        public int VisibleLine
        {
            get => ScrollingTo > 0 ? ScrollingTo : Mathf.CeilToInt(visibleLine) + 1;
            set
            {
                if (value > lines || value < 1)
                {
                    Debug.LogError($"Specified line number {value} is outside the range of lines 1-{lines}. "
                                   + $"Using maximum line number {lines} instead.");
                    value = lines;
                }

                // If this is called before Start() has been called, scrollbar will be null, so we have to cache
                // the desired visible line.
                if (!HasStarted)
                {
                    PreStartLine = value;
                }
                else
                {
                    // Animate scroll
                    ScrollingTo = value;
                    DOTween.Sequence().Append(DOTween.To(() => visibleLine, f => visibleLine = f, value - 1, 1f))
                           .AppendCallback(() => ScrollingTo = 0);

                    // FIXME: TMP bug: Large files cause issues with highlighting text. This is just a workaround.
                    // See https://github.com/uni-bremen-agst/SEE/issues/250#issuecomment-819653373
                    if (Text.Length < 16382)
                    {
                        MarkLine(value);
                    }

                    ScrollEvent.Invoke();
                }
            }
        }

        /// <summary>
        /// The line currently at the top of the window.
        /// Will immediately set the line.
        /// Note that the line here is 0-indexed, as opposed to <see cref="VisibleLine"/>, which is 1-indexed.
        /// </summary>
        private float visibleLine
        {
            get => HasStarted ? (1 - scrollRect.verticalNormalizedPosition) * (lines - 1 - excessLines) : PreStartLine;
            set
            {
                if (value > lines - 1 || value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                // If this is called before Start() has been called, scrollbar will be null, so we have to cache
                // the desired visible line.
                if (!HasStarted)
                {
                    PreStartLine = value;
                }
                else
                {
                    scrollRect.verticalNormalizedPosition = 1 - value / (lines - 1 - excessLines);
                }
            }
        }

        #endregion

        #region Value Object

        protected override void InitializeFromValueObject(CodeWindowValues valueObject)
        {
            if (valueObject.Path != null)
            {
                EnterFromFile(valueObject.Path);
            }
            else if (valueObject.Text != null)
            {
                EnterFromText(valueObject.Text.Split('\n'));
            }
            else
            {
                throw new ArgumentException("Invalid value object. Either FilePath or Text must not be null.");
            }
            VisibleLine = valueObject.VisibleLine;
        }

        /// <summary>
        /// Generates and returns a <see cref="CodeWindowValues"/> struct for this code window.
        /// </summary>
        /// <param name="fulltext">Whether the whole text should be included. Iff false, the filename will be saved
        /// instead of the text.</param>
        /// <returns>The newly created <see cref="CodeWindowValues"/>, matching this class</returns>
        public override CodeWindowValues ToValueObject()
        {
            string attachedTo = gameObject.name;
            return SYNC_FULL_TEXT
                ? new CodeWindowValues(Title, VisibleLine, attachedTo, Text)
                : new CodeWindowValues(Title, VisibleLine, attachedTo, path: FilePath);
        }

        /// <summary>
        /// Represents the values of a code window needed to re-create its content.
        /// Used for serialization when sending a <see cref="CodeWindow"/> over the network.
        /// </summary>
        [Serializable]
        public class CodeWindowValues: WindowValues
        {
            /// <summary>
            /// Text of the code window. May be <c>null</c>, in which case <see cref="Path"/> is not <c>null</c>.
            /// </summary>
            [field: SerializeField]
            public string Text { get; private set; }

            /// <summary>
            /// Path to the file displayed in the code window. May be <c>null</c>, in which case <see cref="Text"/> is not
            /// <c>null</c>.
            /// </summary>
            [field: SerializeField]
            public string Path { get; private set; }

            /// <summary>
            /// The line number which is currently visible in / at the top of the code window.
            /// </summary>
            [field: SerializeField]
            public int VisibleLine { get; private set; }

            /// <summary>
            /// Creates a new CodeWindowValues object from the given parameters.
            /// Note that either text or Path must not be <c>null</c>.
            /// </summary>
            /// <param name="title">The title of the code window.</param>
            /// <param name="visibleLine">The line currently at the top of the code window which is fully visible.</param>
            /// <param name="attachedTo">Name of the game object the code window is attached to.</param>
            /// <param name="text">The text of the code window. May be <c>null</c>, in which case
            /// <paramref name="path"/> may not be.</param>
            /// <param name="path">The path to the file which should be displayed in the code window.
            /// May be <c>null</c>, in which case <paramref name="text"/> may not.</param>
            /// <exception cref="ArgumentException">Thrown when both <paramref name="path"/> and
            /// <paramref name="text"/> are <c>null</c>.</exception>
            internal CodeWindowValues(string title, int visibleLine, string attachedTo = null, string text = null, string path = null) : base(title, attachedTo)
            {
                if (text == null && path == null)
                {
                    throw new ArgumentException("Either text or filename must not be null!");
                }

                Text = text;
                Path = path;
                VisibleLine = visibleLine;
            }
        }

        #endregion
    }
}