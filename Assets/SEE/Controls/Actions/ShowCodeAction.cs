﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using SEE.UI.Window.CodeWindow;
using SEE.UI.Notification;
using SEE.GO;
using SEE.Net.Actions;
using SEE.Utils;
using UnityEngine;
using SEE.DataModel.DG;
using System;
using SEE.UI.Window;
using SEE.Utils.History;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Action to display the source code of the currently selected node using <see cref="CodeWindow"/>s.
    /// </summary>
    internal class ShowCodeAction : AbstractPlayerAction
    {
        /// <summary>
        /// Manager object which takes care of the player selection menu and window space dictionary for us.
        /// </summary>
        private WindowSpaceManager spaceManager;

        /// <summary>
        /// Action responsible for synchronizing the window spaces across the network.
        /// </summary>
        private SyncWindowSpaceAction syncAction;

        public override HashSet<string> GetChangedObjects()
        {
            // Changes to the window space are handled and synced by us separately, so we won't include them here.
            return new HashSet<string>();
        }

        public override ActionStateType GetActionStateType() => ActionStateTypes.ShowCode;

        /// <summary>
        /// Returns a new instance of <see cref="ShowCodeAction"/>.
        /// </summary>
        /// <returns>new instance</returns>
        public static IReversibleAction CreateReversibleAction() => new ShowCodeAction();

        public override IReversibleAction NewInstance() => CreateReversibleAction();

        public override void Awake()
        {
            // In case we do not have an ID yet, we request one.
            if (ICRDT.GetLocalID() == 0)
            {
                new NetCRDT().RequestID();
            }
            spaceManager = WindowSpaceManager.ManagerInstance;
        }

        public override void Start()
        {
            syncAction = new SyncWindowSpaceAction();
            spaceManager.OnActiveWindowChanged.AddListener(UpdateSpace);
            spaceManager[WindowSpaceManager.LocalPlayer].OnWindowAdded.AddListener(w =>
            {
                if (w is CodeWindow codeWindow)
                {
                    codeWindow.ScrollEvent.AddListener(UpdateSpace);
                }
            });
            return;

            void UpdateSpace()
            {
                syncAction.UpdateSpace(spaceManager[WindowSpaceManager.LocalPlayer]);
            }
        }

        /// <summary>
        /// If the gameObject associated with graphElementRef has already a CodeWindow
        /// attached to it, that CodeWindow will be returned. Otherwise a new CodeWindow
        /// will be attached to the gameObject associated with graphElementRef and returned.
        /// The title for the newly created CodeWindow will be GetName(graphElement).
        /// </summary>
        /// <param name="graphElementRef">The graph element to get the CodeWindow for</param>
        /// <param name="filename">The filename to use for the CodeWindow title</param>
        private static CodeWindow GetOrCreateCodeWindow(GraphElementRef graphElementRef, string filename)
        {
            // Create new window for active selection, or use existing one
            if (!graphElementRef.TryGetComponent(out CodeWindow codeWindow))
            {
                codeWindow = graphElementRef.gameObject.AddComponent<CodeWindow>();

                codeWindow.Title = GetName(graphElementRef.Elem);
                // If SourceName differs from Source.File (except for its file extension), display both
                if (!codeWindow.Title.Replace(".", "").Equals(filename.Split('.').Reverse().Skip(1)
                                                                      .Aggregate("", (acc, s) => s + acc)))
                {
                    codeWindow.Title += $" ({filename})";
                }
            }
            return codeWindow;
        }

        /// <summary>
        /// Returns the filename and the absolute platform-specific path of
        /// given graphElement.
        /// </summary>
        /// <param name="graphElement">The graph element to get the filename and path for</param>
        /// <returns>filename and absolute path</returns>
        /// <exception cref="InvalidOperationException">
        /// If the given graphElement has no filename or the path does not exist
        /// </exception>
        private static (string filename, string absolutePlatformPath) GetPath(GraphElement graphElement)
        {
            string filename = graphElement.Filename();
            if (filename == null)
            {
                string message = $"Selected {GetName(graphElement)} has no filename.";
                ShowNotification.Error("No filename", message, log: false);
                throw new InvalidOperationException(message);
            }
            string absolutePlatformPath = graphElement.AbsolutePlatformPath();
            if (!File.Exists(absolutePlatformPath))
            {
                string message = $"Path {absolutePlatformPath} of selected {GetName(graphElement)} does not exist.";
                ShowNotification.Error("Path does not exist", message, log: false);
                throw new InvalidOperationException(message);
            }
            if ((File.GetAttributes(absolutePlatformPath) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                string message = $"Path {absolutePlatformPath} of selected {GetName(graphElement)} is a directory.";
                ShowNotification.Error("Path is a directory", message, log: false);
                throw new InvalidOperationException(message);
            }
            return (filename, absolutePlatformPath);
        }

        /// <summary>
        /// Returns a human-readable representation of given graphElement.
        /// </summary>
        /// <param name="graphElement">The graph element to get the name for</param>
        /// <returns>human-readable name</returns>
        private static string GetName(GraphElement graphElement)
        {
            return graphElement.ToShortString();
        }

        /// <summary>
        /// Returns a new CodeWindow showing a unified diff for the given Clone edge.
        /// We are assuming that the edge has type Clone.
        /// </summary>
        /// <param name="edgeRef">The edge to get the CodeWindow for</param>
        /// <returns>new CodeWindow showing a unified diff</returns>
        /// <exception cref="InvalidOperationException">If the given edge is not a proper Clone edge</exception>
        public static CodeWindow ShowUnifiedDiff(EdgeRef edgeRef)
        {
            Edge edge = edgeRef.Value;
            (string sourceFilename, string sourceAbsolutePlatformPath) = GetPath(edge.Source);
            (string _, string targetAbsolutePlatformPath) = GetPath(edge.Target);
            int sourceStartLine = GetAttribute(edge, "Clone.Source.Start.Line");
            int sourceEndLine = GetAttribute(edge, "Clone.Source.End.Line");
            int targetStartLine = GetAttribute(edge, "Clone.Target.Start.Line");
            int targetEndLine = GetAttribute(edge, "Clone.Target.End.Line");

            string[] diff = TextualDiff.Diff(sourceAbsolutePlatformPath, sourceStartLine, sourceEndLine,
                                             targetAbsolutePlatformPath, targetStartLine, targetEndLine);
            CodeWindow codeWindow = GetOrCreateCodeWindow(edgeRef, sourceFilename);
            codeWindow.EnterFromText(diff, true);
            codeWindow.ScrolledVisibleLine = 1;
            return codeWindow;

            // Returns the value of the edge's integer attribute.
            // If none exists, the user will be notified and an exception will be thrown.
            static int GetAttribute(Edge edge, string attribute)
            {
                if (edge.TryGetInt(attribute, out int value))
                {
                    return value;
                }
                else
                {
                    string message = $"Selected {GetName(edge)} has no attribute {attribute}.";
                    ShowNotification.Error("No attribute", message, log: false);
                    throw new InvalidOperationException(message);
                }
            }
        }

        /// <summary>
        /// Returns a CodeWindow showing the code range of the given graph element
        /// retrieved from a file. The path of the file is retrieved from
        /// the absolute path as specified by the graph element's source location
        /// attributes.
        /// </summary>
        /// <param name="graphElementRef">The graph element to get the CodeWindow for</param>
        /// <returns>new CodeWindow showing the code range of the given graph element</returns>
        public static CodeWindow ShowCode(GraphElementRef graphElementRef)
        {
            GraphElement graphElement = graphElementRef.Elem;
            // File name of source code file to read from it
            (string filename, string absolutePlatformPath) = GetPath(graphElement);
            CodeWindow codeWindow = GetOrCreateCodeWindow(graphElementRef, filename);
            codeWindow.EnterFromFile(absolutePlatformPath);

            // Pass line number to automatically scroll to it, if it exists
            if (graphElement.SourceLine() is { } line)
            {
                codeWindow.ScrolledVisibleLine = line;
            }

            return codeWindow;
        }

        public override bool Update()
        {
            // Only allow local player to open new code windows
            if (spaceManager.CurrentPlayer == WindowSpaceManager.LocalPlayer
                && SEEInput.Select()
                && Raycasting.RaycastGraphElement(out RaycastHit _, out GraphElementRef graphElementRef) != HitGraphElement.None)
            {
                // If nothing is selected, there's nothing more we need to do
                if (graphElementRef == null)
                {
                    return false;
                }

                // Edges of type Clone will be handled differently. For these, we will be
                // showing a unified diff.
                CodeWindow codeWindow = graphElementRef is EdgeRef { Value: { Type: "Clone" } } edgeRef
                    ? ShowUnifiedDiff(edgeRef)
                    : ShowCode(graphElementRef);

                // Add code window to our space of code window, if it isn't in there yet
                WindowSpace manager = spaceManager[WindowSpaceManager.LocalPlayer];
                if (!manager.Windows.Contains(codeWindow))
                {
                    manager.AddWindow(codeWindow);
                }
                manager.ActiveWindow = codeWindow;
                // TODO (#669): Set font size etc in settings (maybe, or maybe that's too much)
            }

            return false;
        }
    }
}
