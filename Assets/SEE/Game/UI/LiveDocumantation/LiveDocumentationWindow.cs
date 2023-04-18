using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using JetBrains.Annotations;
using SEE.Controls;
using SEE.DataModel.DG;
using SEE.Game.UI.Notification;
using SEE.Game.UI.Window;
using SEE.Utils;
using TMPro;
using UnityEngine;

namespace SEE.Game.UI.LiveDocumantation
{
    /// <summary>
    /// This class represents a LiveDocumentation window.
    /// 
    /// In this window the Name of the Class, the documentation and all class methods are shown.
    /// 
    /// 
    /// The following fields must be set:
    /// <ul>
    ///     <li><see cref="ClassName"/> </li>
    ///     <li><see cref="BasePath"/> </li>
    ///     <li><see cref="RelativePath"/> </li>
    /// </ul>
    /// Otherwise an error is displayed and the window is not rendereed.
    /// 
    /// </summary>
    public class LiveDocumentationWindow : BaseWindow
    {
        /// <summary>
        /// Path to the prefab
        /// </summary>
        private const string PREFAB_NAME = "Prefabs/UI/LiveDocumentation/LiveDocumentation";

        private const string ClassDocumentationPath = "ClassDocumentation/Viewport/Content/ClassDoc";
        private const string ClassMemberListPath = "ClassMembers/Scroll Area/List";

        /// <summary>
        /// Text mesh for the (shortened) class name
        /// </summary>
        private TextMeshProUGUI ClassNameField;

        private GameObject ClassMembersList;

        /// <summary>
        /// Text mesh for the class documentation
        ///
        /// The Path inside the prefab should be "LiveDocumentation/ClassDocumentation/Viewport/Content/ClassDoc"
        /// </summary>
        private TextMeshProUGUI ClassDocumentation;

        private Dictionary<String, Node> NamespaceCache = new();


        /// <summary>
        /// The name of the class
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// The base path of the folder containing the source code files
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// The relative Path
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// The graph which the node the user has clicked on belongs to.
        /// This node is the node which documentation is displayed in this window.
        ///
        /// The Graph is needed to find a corresponding node when the user clicked on a link
        /// </summary>
        public Graph Graph;

        private WindowSpaceManager spaceManager;

        private bool SpaceManagerContainsWindow(string filePath, out BaseWindow win)
        {
            List<LiveDocumentationWindow> matchingWindows = spaceManager[WindowSpaceManager.LOCAL_PLAYER].Windows
                .OfType<LiveDocumentationWindow>()
                .Where(x => x.RelativePath == filePath).ToList();


            if (matchingWindows.Count == 0)
            {
                win = null;
                return false;
            }

            win = matchingWindows[0];
            return true;
        }

        /// <summary>
        /// Enables and disables automatic line breaks in the LiveDocumentation window text fields.
        ///
        /// TODO: Needs to be implemented
        /// </summary>
        public bool AutoLineBreaksEnabled { get; set; } = true;

        /// <summary>
        /// The <see cref="LiveDocumentationBuffer"/> which contains the documentation of the class including links
        /// </summary>
        public LiveDocumentationBuffer DocumentationBuffer { get; set; }

        public IList<LiveDocumentationBuffer> ClassMembers { get; set; } = new List<LiveDocumentationBuffer>();


        /// <summary>
        /// Checks if all the necessary fields are set in the class
        /// </summary>
        /// <returns>Returns true when all fields are set. Otherwise false</returns>
        private bool CheckNecessaryFields() =>
            ClassName != null && BasePath != null && RelativePath != null && Graph != null;

        /// <summary>
        /// Adds a new Class member to the ClassMember section in the LiveDocumentation Window.
        ///
        /// Currently ClassMembers are represented by a <see cref="LiveDocumentationBuffer"/>.
        /// In this buffer all information and links of the method signature is stored. 
        /// </summary>
        /// <param name="buffer"></param>
        private void AddClassMember(LiveDocumentationBuffer buffer)
        {
            // Creating a new GameObject and naming it
            GameObject classMem = new GameObject();
            classMem.name = "Item";

            // Adding some other Components
            classMem.AddComponent<CanvasRenderer>();
            RectTransform rt = classMem.AddComponent<RectTransform>();

            //Adding the ClassMember component
            ClassMember cm = classMem.AddComponent<ClassMember>();
            cm.Text = buffer.PrintBuffer();

            // Setting the correct anchor point (upper left corner) for the new game object
            classMem.transform.parent = ClassMembersList.transform;
            rt.localScale = new Vector3(1, 1, 1);
            // This will set the 
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);

            cm.OnLinkClicked += OnLinkClicked;
        }


        protected override void StartDesktop()
        {
            spaceManager = WindowSpaceManager.ManagerInstance;
            if (!CheckNecessaryFields())
            {
                Debug.LogError("Some fields are not set; cant load LiveDocumentation");
                return;
            }

            base.StartDesktop();
            var c = Canvas.GetComponent<Camera>();
            GameObject livedoc =
                PrefabInstantiator.InstantiatePrefab(PREFAB_NAME, Window.transform.Find("Content"), false);
            livedoc.name = "LiveDocumentation";


            Window.transform.Find("Dragger/Title").gameObject.GetComponent<TextMeshProUGUI>().text =
                "LiveDocumentation";


            // Initializing Unity Components 
            ClassNameField = livedoc.transform.Find("ClassName/Viewport/Content/ClassName").gameObject
                .GetComponent<TextMeshProUGUI>();

            ClassDocumentation =
                livedoc.transform.Find(ClassDocumentationPath).gameObject.GetComponent<TextMeshProUGUI>();


            ClassMembersList = livedoc.transform.Find(ClassMemberListPath).gameObject;

            // Setting the classname.
            ClassNameField.text = ClassName;

            // Try setting the documentation 
            ClassDocumentation.text = DocumentationBuffer?.PrintBuffer() ?? "NO DOCS AVAILABLE";
            //     GameObject livedoc =
            //         PrefabInstantiator.InstantiatePrefab("Prefabs/UI/LiveDocumentation/ClassMember", ClassMembers.transform, false);

            ClassDocumentation.ForceMeshUpdate();

            foreach (var item in ClassMembers)
            {
                AddClassMember(item);
            }
        }


        public override void RebuildLayout()
        {
            // Nothing to be done yet
        }

        protected override void InitializeFromValueObject(WindowValues valueObject)
        {
            throw new NotImplementedException();
        }

        public override void UpdateFromNetworkValueObject(WindowValues valueObject)
        {
            throw new NotImplementedException();
        }

        public override WindowValues ToValueObject()
        {
            return new LiveDocumentationValues("Live doc", gameObject.name);
        }

        public class LiveDocumentationValues : WindowValues
        {
            public LiveDocumentationValues(string title, string attachedTo = null) : base(title, attachedTo)
            {
            }
        }

        /// <summary>
        /// Finds a node given from a clicked filename.
        /// </summary>
        /// <param name="filename">The filename to find a corresponding node for.</param>
        /// <returns>The node or null if none could be found.</returns>
        [CanBeNull]
        private Node FindNodeWithPath(string filename, Node currentNode)
        {
            //See if an item was found in the cache.
            if (NamespaceCache.ContainsKey(filename))
            {
                return NamespaceCache[filename];
            }

                // Iterate through each node in the code city.
                foreach (var item in Graph.Nodes())
                {
                    //Collect all namespaces
                    //TODO replace using with namespaces.
                    var filePath = item.AbsolutePlatformPath();
                    var input = File.ReadAllText(filePath);
                    var lexer = new CSharpUsingGrammarLexer(new AntlrInputStream(input));
                    var tokens = new CommonTokenStream(lexer);
                    tokens.Fill();

                    var parser = new CSharpUsingGrammarParser(tokens);
                    var namespaces = parser.start().usingDirective(0).children;

                    foreach (var parsedNamespace in namespaces)
                    {
                        if (parsedNamespace.GetText().Equals(filename))
                        {
                            NamespaceCache[filename] = item;
                            return item;
                        }
                    }
                }

            return null;
        }

        /// <summary>
        /// Called when a user has clicked on a link
        /// </summary>
        /// <param name="linkPath"></param>
        public void OnLinkClicked(string linkPath)
        {
            Node nodeOfLink = FindNodeWithPath(linkPath);
            if (nodeOfLink == null)
            {
                ShowNotification.Error("Cant open link", "The class can't be found");
                return;
            }

            if (!SpaceManagerContainsWindow(linkPath, out BaseWindow w))
            {
                LiveDocumentationWindow newWin = nodeOfLink.GameObject().AddComponent<LiveDocumentationWindow>();
                var filenames = linkPath.Split("\\");
                newWin.ClassName = filenames[filenames.Length - 1].Split(".")[0];
                newWin.Title = linkPath;
                newWin.BasePath = BasePath;
                newWin.RelativePath = linkPath;
                newWin.Graph = Graph;

                LiveDocumentationBuffer buffer = new LiveDocumentationBuffer();
                buffer.Add(new LiveDocumentationBufferText(
                    "Dies ist eine Test documentation für die andere Klasse "));
                buffer.Add(new LiveDocumentationLink("src/C2.cs", "CS2.cs"));

                newWin.DocumentationBuffer = buffer;
                //  newWin.DocumentationBuffer = buffer;
                // Add code window to our space of code window, if it isn't in there yet


                spaceManager[WindowSpaceManager.LOCAL_PLAYER].AddWindow(newWin);
                spaceManager[WindowSpaceManager.LOCAL_PLAYER].ActiveWindow = newWin;
            }
            else
            {
                spaceManager[WindowSpaceManager.LOCAL_PLAYER].ActiveWindow = w;
            }
        }

        protected override void UpdateDesktop()
        {
            // When the user clicked in the LiveDocumentation window
            if (Input.GetMouseButtonDown(0))
            {
                int link = TMP_TextUtilities.FindIntersectingLink(ClassDocumentation, Input.mousePosition, null);

                if (link != -1)
                {
                    string linkId = ClassDocumentation.textInfo.linkInfo[link].GetLinkID().ToString();
                    OnLinkClicked(linkId);
                }
            }
        }
    }
}