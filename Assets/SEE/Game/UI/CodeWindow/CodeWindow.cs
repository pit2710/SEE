using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SEE.Game.UI.CodeWindow
{
    /// <summary>
    /// Represents a movable, scrollable window containing source code.
    /// The source code may either be entered manually or read from a file.
    /// </summary>
    public partial class CodeWindow: PlatformDependentComponent
    {
        /// <summary>
        /// The text displayed in the code window.
        /// </summary>
        private string Text;

        /// <summary>
        /// The title (e.g. filename) for the code window.
        /// </summary>
        public string Title;

        /// <summary>
        /// The GameObject which serves as the anchor, i.e., the GameObject above which the code window should float.
        /// </summary>
        public GameObject Anchor;

        /// <summary>
        /// Distance (in meters) with which the code windows should be positioned above the <see cref="Anchor"/>.
        /// </summary>
        public float AnchorDistance = 0.5f;

        /// <summary>
        /// Size of the font used in the code window.
        /// </summary>
        public float FontSize = 20f;

        /// <summary>
        /// Resolution of the code window. By default, this is set to a resolution of 800x600.
        /// </summary>
        public Vector2 Resolution = new Vector2(800, 400);

        /// <summary>
        /// How wide the canvas should be in the world (in meters). Default is one meter.
        /// This will not affect the <see cref="Resolution"/>, the canvas will simply be scaled appropriately.
        /// </summary>
        public float WorldWidth = 1;

        /// <summary>
        /// GameObject containing the code canvas, which in turn contains this code window.
        /// </summary>
        private GameObject CodeCanvas;

        /// <summary>
        /// Name for the code windows group game object.
        /// </summary>
        private const string CODE_WINDOWS_NAME = "Code Windows";

        /// <summary>
        /// Path to the code canvas prefab.
        /// </summary>
        private const string CODE_CANVAS_PREFAB = "Prefabs/UI/CodeCanvas";

        /// <summary>
        /// Populates the code window with the contents of the given file.
        /// This will overwrite any existing text.
        /// </summary>
        /// <param name="text">An array of lines to use for the code window.</param>
        /// <exception cref="ArgumentException">If <paramref name="text"/> is empty or <c>null</c></exception>
        public void EnterFromText(string[] text)
        {
            if (text == null || text.Length <= 0)
            {
                throw new ArgumentException("Given text must not be empty or null.\n");
            }

            int neededPadding = $"{text.Length}".Length;
            Text = "";
            for (int i = 0; i < text.Length; i++)
            {
                // Add whitespace next to line number so it's consistent
                Text += string.Join("", Enumerable.Repeat(" ", neededPadding-$"{i}".Length));
                // Line number will be typeset in yellow to distinguish it from the rest
                Text += $"<color=\"yellow\">{i}</color> <noparse>{text[i]}</noparse>\n";
            }
        }

        /// <summary>
        /// Populates the code window with the contents of the given file.
        /// This will overwrite any existing text.
        /// </summary>
        /// <param name="filename">The filename for the file to read.</param>
        public void EnterFromFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException($"Couldn't find file '{filename}'.");
            }

            EnterFromText(File.ReadAllLines(filename));
        }
    }
}