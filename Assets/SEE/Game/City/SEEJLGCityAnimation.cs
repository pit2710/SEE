﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SEE.Controls;
using SEE.Controls.KeyActions;
using SEE.DataModel.DG;
using SEE.Game.Runtime;
using SEE.GO;
using SEE.Runtime.JLG;
using SEE.Utils;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SEE.Game.City
{
    /// <summary>
    /// Configuration of a code city for the visualization of dynamic data in
    /// traced at the level of statements.
    ///
    /// This part contains the animation code.
    /// </summary>
    public partial class SEEJlgCity
    {
        //------------------------------------
        // Public configuration attributes
        //------------------------------------

        /// <summary>
        /// The class the breakpoint is contained in.
        /// </summary>
        [Tooltip("The class the breakpoint is contained in."), FoldoutGroup(jlgFoldoutGroup)]
        public string BreakpointClass = "classname";

        /// <summary>
        /// The source line of the breakpoint.
        /// </summary>
        [Tooltip("The source line of the breakpoint."), FoldoutGroup(jlgFoldoutGroup)]
        public int BreakpointLine = 0;

        /// <summary>
        /// The distance between the code city and the source-code window
        /// in Unity units.
        /// </summary>
        [Tooltip("The distance between the code city and the source-code window."),
         FoldoutGroup(jlgFoldoutGroup)]
        public float DistanceAboveCity = 0.3f;

        /// <summary>
        /// The distance between the line connecting the code city and the source-code window
        /// in Unity units.
        /// </summary>
        [Tooltip("The distance between the line connecting the code city and the source-code window."),
         FoldoutGroup(jlgFoldoutGroup)]
        public float DistanceBehindCity = 0.3f;

        /// <summary>
        /// The width of the line connecting the source-code window and the game objects
        /// whose source code is currently shown.
        /// </summary>
        [Tooltip("The width of the line connecting the source-code window and the game objects whose source code is currently shown."),
         FoldoutGroup(jlgFoldoutGroup)]
        public float LineWidth = 0.01f;

        /// <summary>
        /// If true, we always move to the next/previous call statement in the execution
        /// (depending upon whether the execution is forward or backward, respectively),
        /// that is, only interprocedural control flow will be shown.
        /// </summary>
        [Tooltip("If true, we always move to the next/previous call statement in the execution"
                 + " (depending upon whether the execution is forward or backward, respectively),"
                 + " that is, only interprocedural control flow will be shown."),
         FoldoutGroup(jlgFoldoutGroup)]
        public bool ShowOnlyCalls = false;

        //-------------------------------------------------------
        // Private attributes not saved in the configuration file
        //-------------------------------------------------------

        private const string jlgFoldoutGroup = "JLG Settings";

        /// <summary>
        /// This name will be added at the end of every game object representing a
        /// source-code viewer for an executed node.
        /// </summary>
        private const string fileContentNamePostfix = "FileContent";

        /// <summary>
        /// A ParsedJLG object that contains a parsed JLG file. This object contains all
        /// information needed for the visualization of a debugging process.
        /// </summary>
        //[NonSerialized, OdinSerialize]
        private ParsedJLG parsedJLG;

        /// <summary>
        /// The statement counter represents the index of the current active statement,
        /// that is, the one whose execution is currently visualized.
        /// All indices can be found in this.parsedJLG.allStatements.
        /// The total number of indices is this.parsedJLG.allStatements.Count.
        /// Must always stay in the range 0 <= statementCounter <= parsedJLG.AllStatements.Count - 1
        /// </summary>
        private StatementCounter statementCounter;
        private struct StatementCounter
        {
            public StatementCounter(uint initialValue, uint maxValue)
            {
                value = initialValue;
                this.maxValue = maxValue;
            }
            private uint value;
            private readonly uint maxValue;

            public int Value
            {
                get => (int)value;
                set
                {
                    if (value >= 0 && value <= maxValue)
                    {
                        this.value = (uint)value;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException($"Does not hold: 0 <= {value} <= {maxValue}");
                    }
                }
            }

            public void Increase(uint delta = 1)
            {
                value = (uint)Mathf.Min(value + delta, maxValue);
            }

            public void Decrease(uint delta = 1)
            {
                value = (uint)Mathf.Max(0, (int)value - (int)delta);
            }

            public bool LowerBound()
            {
                return value == 0;
            }

            public bool UpperBound()
            {
                return value == maxValue;
            }
        }

        /// <summary>
        /// This field saves the text that is displayed when a interaction key is hit.
        /// </summary>
        private string labelText = "";

        /// <summary>
        /// Time value in seconds. At this point in time (running time) the next or
        /// previous statement will be visualized, depending on the playing direction.
        /// </summary>
        private float nextUpdateTime = 1.0f;

        /// <summary>
        /// How long the currently a GUI message telling which button was pressed should
        /// be shown.
        /// </summary>
        private float showLabelUntil = 0f;

        /// <summary>
        /// Time in seconds until the next statement is to be shown.
        /// </summary>
        private float updateInterval = 1.0f;

        /// <summary>
        /// True is visualisation is running in automatic mode; false if in manual mode.
        /// In automatic mode, the execution is played automatically at a particular
        /// period. In manual mode, the user needs to press keys to move to the next
        /// or previous statement to be visualized.
        /// </summary>
        private bool inAutomaticMode = false;

        /// <summary>
        /// Describes the direction, in which the visualisation is running. True for forward, false for backwards.
        /// </summary>
        private bool playingForward = true;

        /// <summary>
        /// This saves the direction in which the statement executed last was visualized.
        /// True means forward, false means backward.
        /// </summary>
        private bool lastDirectionWasForward = true;

        /// <summary>
        /// The GameObject that represents the class the current statement belongs to.
        /// </summary>
        private GameObject currentGO;

        /// <summary>
        /// A list of all active GameObjects that are tagged with 'Node'.
        /// </summary>
        private Dictionary<string, GameObject> nodesGOs;

        /// <summary>
        /// Stack of all existing FileContent text windows.
        /// </summary>
        private Stack<GameObject> textWindows = new();

        /// <summary>
        /// Stack of all existing function calls.
        /// </summary>
        private Stack<GameObject> functionCalls = new();

        /// <summary>
        /// Reads the JLG data from <see cref="JLGPath"/>, initializes <see cref="parsedJLG"/>,
        /// <see cref="statementCounter"/>, <see cref="nodesGOs"/>, and <see cref="currentGO"/>.
        /// In case of any error, this components disables itself.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            JLGParser jlgParser = new(JLGPath.Path);
            parsedJLG = jlgParser.Parse();

            if (parsedJLG == null)
            {
                enabled = false;
                Debug.LogError("Parsed JLG is null!");
            }
            else if (parsedJLG.AllStatements.Count == 0)
            {
                enabled = false;
                Debug.LogWarning("[JLG] There are no statements to be executed.\n");
            }
            else
            {
                statementCounter = new StatementCounter(0, (uint)(parsedJLG.AllStatements.Count - 1));
                nodesGOs = GetNodes();
                Debug.Log($"[JLG] Number of nodes contained in {gameObject.name} is {nodesGOs.Count}\n");
                // Sets the currentGO to be the node representing the Class of the first Statement in preparation.
                if (nodesGOs.Count == 0)
                {
                    enabled = false;
                    Debug.LogError("[JLG] There are no nodes. Your first need to create a code city with at least one node.\n");
                }
                else if (GetNodeForStatement(statementCounter.Value) == null)
                {
                    enabled = false;
                    Debug.LogError($"[JLG] Node for statement counter {statementCounter} is missing. Check whether the correct GXL is loaded.\n");
                }
                else
                {
                    currentGO = GetNodeForStatement(statementCounter.Value);
                    currentGO.GetComponentInChildren<MeshRenderer>().material.color = new Color(0.219f, 0.329f, 0.556f, 1f);
                    GenerateScrollableTextWindow();
                    Debug.Log("[JLG] Started.");
                }
            }
        }

        /// <summary>
        /// Returns all transitive children of <see cref="gameObject"/> tagged by <see cref="Tags.Node"/>.
        /// </summary>
        /// <returns>all ancestors representing nodes</returns>
        private Dictionary<string, GameObject> GetNodes()
        {
            Dictionary<string, GameObject> result = new();
            foreach(GameObject node in gameObject.AllDescendants(Tags.Node))
            {
                result[node.name] = node;
            }
            return result;
        }

        /// <summary>
        /// Reacts to the user's interactions.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            // Update Visualisation every 'interval' seconds.
            if (Time.time >= nextUpdateTime)
            {
                if (inAutomaticMode)
                {
                    UpdateVisualization();
                }
                nextUpdateTime += updateInterval;
            }

            bool userIsHoveringCity = UserIsHoveringCity();
            // Controls
            if (userIsHoveringCity && SEEInput.ToggleAutoPlay())
            {
                // Toggling automatic/manual execution mode.
                inAutomaticMode = !inAutomaticMode;
                showLabelUntil = Time.time + 1f;
                if (inAutomaticMode)
                {
                    labelText = "Play";
                    ToggleTextWindows();
                }
                else
                {
                    labelText = "Pause";
                }
            }
            if (Input.GetMouseButtonDown(0) && !Raycasting.IsMouseOverGUI())
            {
                // The user can select a node. This turns off the automatic mode
                // and the source code of that node is shown.
                GameObject clickedGO = MouseClickHitActiveNode();
                if (clickedGO != null)
                {
                    if (inAutomaticMode)
                    {
                        inAutomaticMode = false;
                        showLabelUntil = Time.time + 1f;
                        labelText = "Paused";
                    }
                    ActivateNodeTextWindow(clickedGO);
                }
            }
            if (userIsHoveringCity && SEEInput.ToggleExecutionOrder())
            {
                // Reversing the order of execution.
                updateInterval = 1;
                showLabelUntil = Time.time + 1f;

                playingForward = !playingForward;
                if (playingForward)
                {
                    lastDirectionWasForward = true;
                    statementCounter.Increase(); // This need to be called because the statement counter
                                                 // was increased/decreased in the last UpdateVisualization call already.
                                                 // This prevents bugs.
                    MoveForwardToNextStatement();
                    labelText = "Forward";
                }
                else
                {
                    lastDirectionWasForward = false;
                    statementCounter.Decrease();  // Ditto.
                    MoveBackwardToPreviousStatement();
                    labelText = "Rewind";
                }
            }
            if (inAutomaticMode)
            {
                // automatic mode
                if (userIsHoveringCity && SEEInput.IncreaseAnimationSpeed())
                {
                    SpeedUp();
                    showLabelUntil = Time.time + 1f;
                    labelText = "Speed x" + 1f / updateInterval;
                }
                if (userIsHoveringCity && SEEInput.DecreaseAnimationSpeed())
                {
                    SlowDown();
                    showLabelUntil = Time.time + 1f;
                    labelText = "Speed x" + 1f / updateInterval;
                }
            }
            else
            {
                // manual mode
                if (userIsHoveringCity && SEEInput.ExecuteToBreakpoint())
                {
                    showLabelUntil = Time.time + 1f;
                    labelText = "Jumping to Breakpoint...";
                    if (JumpToNextBreakpointHit())
                    {
                        showLabelUntil = Time.time + 1f;
                        labelText = "Jumped to Breakpoint";
                    }
                    else
                    {
                        showLabelUntil = Time.time + 1f;
                        labelText = "Could not find Breakpoint";
                    }
                }
                if (userIsHoveringCity && SEEInput.Previous())
                {
                    OneStep(false);
                    lastDirectionWasForward = false;
                }
                if (userIsHoveringCity && SEEInput.Next())
                {
                    OneStep(true);
                    lastDirectionWasForward = true;
                }
                if (userIsHoveringCity && SEEInput.FirstStatement())
                {
                    ResetComplete();
                }
            }

            // Only update the Spheres cause this visualization uses its own highlighting for the buildings.
            foreach (GameObject f in functionCalls)
            {
                f.GetComponent<FunctionCallSimulator>().UpdateSpheres();
            }
        }

        /// <summary>
        /// Displays a messages (<see cref="labelText"/>) that indicates what button was pressed.
        /// </summary>
        void OnGUI()
        {
            if (Time.time < showLabelUntil)
            {
                // left upper corner
                GUI.Label(new Rect(Screen.width / 96, Screen.height / 96, Screen.width / 3, Screen.height / 16), labelText);
            }
        }

        /// <summary>
        /// This Method updates the visualization for one Step.
        /// </summary>
        private void UpdateVisualization()
        {
            //Debug.Log($"[JLG] Current statement: {statementCounter.Value}\n");
            try
            {
                CheckCurrentGO();

                if (!TextWindowForNodeExists(currentGO))
                {
                    GenerateScrollableTextWindow();
                }
                ToggleTextWindows();

                UpdateStacks();

                if (playingForward)
                {
                    NextStatement();
                }
                else
                {
                    PreviousStatement();
                }
            }
            catch (MalformedStatement e)
            {
                Debug.LogError($"[JLG] Statement {statementCounter} is malformed and will be skipped: {e.Message}.\n");
                if (playingForward)
                {
                    MoveForwardToNextStatement();
                }
                else
                {
                    MoveBackwardToPreviousStatement();
                }
            }
        }

        /// <summary>
        /// Checks if <see cref="currentGO"/> is the game object of the currently
        /// statement (indexed by <see cref="statementCounter"/>). If not, <see cref="currentGO"/>
        /// is updated to the next game objects containing the statement we are currently
        /// executing. If we switch from game node to another one, that means we have
        /// an interprocedural control flow in which case we create a visualization of
        /// a call.
        ///
        /// Note: This will generate a visual representation of a call if we hit an
        /// entry statement. If we instead hit an exit statement, that means we would
        /// leave the callee. This case will be handled later in <see cref="UpdateStacks"/>.
        /// </summary>
        private void CheckCurrentGO()
        {
            if (!NodeRepresentsStatementLocation(statementCounter.Value, currentGO))
            {
                GameObject nodeForStatement = GetNodeForStatement(statementCounter.Value);
                if (playingForward && statementCounter.Value > 0
                    && parsedJLG.AllStatements[statementCounter.Value].StatementType == StatementKind.Entry)
                {
                    CreateFunctionCall(currentGO, nodeForStatement);
                }
                currentGO.GetComponentInChildren<MeshRenderer>().material.color = new Color(0.530f, 0.637f, 0.858f, 1f);
                currentGO = nodeForStatement;
                currentGO.GetComponentInChildren<MeshRenderer>().material.color = new Color(0.219f, 0.329f, 0.556f, 1f);
            }
        }

        /// <summary>
        /// Activates the FileContent game object of a given node and disables all others.
        /// </summary>
        /// <param name="nodeGameObject"></param>
        private void ActivateNodeTextWindow(GameObject nodeGameObject)
        {
            if (textWindows.Count != 0)
            {
                foreach (GameObject go in textWindows)
                {
                    go.SetActive(go.name == nodeGameObject.name + fileContentNamePostfix);
                }
            }
        }

        /// <summary>
        /// Checks if a mouse click hit a Node game object. Returns GameObject or Null if no object was hit.
        /// </summary>
        /// <returns>hit game object</returns>
        private GameObject MouseClickHitActiveNode()
        {
            Ray cameraMouseRay = MainCamera.Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraMouseRay, out RaycastHit hit))
            {
                if (hit.transform && TextWindowForNodeExists(hit.transform.gameObject))
                {
                    return hit.transform.gameObject;
                }
            }
            return null;
        }

        /// <summary>
        /// Using the GenerateFunctionCalls method from Runtime.cs but with some adjustments so it better works in this Visualization. Only using the spheres
        /// and saving the complete FunctionCall Gameobject instead of just the actual FunctionCallSimulator component.
        /// </summary>
        /// <param name="currentGO"></param>
        /// <param name="destination"></param>
        private void CreateFunctionCall(GameObject currentGO, GameObject destination)
        {
            GameObject fCGO = new GameObject("FunctionCall: " + currentGO.name + " call " + destination.name, typeof(FunctionCallSimulator))
            {
                tag = Tags.FunctionCall
            };
            FunctionCallSimulator sim = fCGO.GetComponent<FunctionCallSimulator>();
            sim.Initialize(currentGO, destination, 1f);
            functionCalls.Push(fCGO);
        }

        /// <summary>
        /// Checks, if there is an existing scrollable text window for a given <paramref name="node"/>.
        /// </summary>
        /// <param name="node">a game object representing a graph node</param>
        /// <returns>true if there is an existing scrollable text window for a given <paramref name="node"/></returns>
        private bool TextWindowForNodeExists(GameObject node)
        {
            return textWindows.Count != 0 && textWindows.Any(go => go.name.Equals(node.name + fileContentNamePostfix));
        }

        /// <summary>
        /// The prefab to be instantiated for the composite object containing the variables
        /// and source code window.
        /// </summary>
        private const string tracingDisplayPrefab = "TracingDisplay";

        /// <summary>
        /// Name of the scroll container where the source code currently being executed
        /// is shown. This name is used in the prefab <see cref="tracingDisplayPrefab"/>.
        /// </summary>
        private const string scrollContainerFileContent = "ScrollContainerFileContent";

        /// <summary>
        /// This method generates a new ScrollableTextMeshProWindow above the code city.
        /// This method assumes that the game object this JLGVisualizer is attached to is
        /// an immediate child of the code-city object. Hence, the parent of
        /// <see cref="gameObject"/> identifies the code-city object. Also it fills the
        /// Textfield with the written code saved in the file, that belongs to this node
        /// aka the "FileContent". Lastly it creates a visual line between the
        /// TextWindow and the <see cref="gameObject"/>.
        /// </summary>
        private void GenerateScrollableTextWindow()
        {
            // The tracing display consists of two parts: (1) a window where the source code of
            // the currently executed code is shown and (2) a window where the values of the
            // currently visible variables at that source-code location are shown.
            GameObject tracingDisplay = Instantiate((GameObject)Resources.Load(tracingDisplayPrefab),
                                                Vector3.zero,
                                                rotation: Quaternion.identity);
            tracingDisplay.name = currentGO.name + fileContentNamePostfix;
            ///set canvas order in layer to textwindows.count so that the text windows can be rendered in front of each other
            tracingDisplay.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = GetFileContentForNode(currentGO);

            float textWindowHeight = GetWindowHeight(tracingDisplay);

            {
                // Sets the position of the tracingDisplay relative to transform;
                // transform is the game object this component is attached to,
                // i.e., a game object representing a code city.
                Vector3 cityExtent = transform.transform.lossyScale / 2.0f;
                Vector3 windowPosition = transform.position;
                windowPosition.y += DistanceAboveCity + textWindowHeight / 2;
                windowPosition.z += cityExtent.z + DistanceBehindCity;
                tracingDisplay.transform.position = windowPosition;
            }

            textWindows.Push(tracingDisplay);

            {
                Transform codeWindow = GetCodeWindow(tracingDisplay);

                ///add line between Class and FileContentWindow
                LineRenderer line = new GameObject(currentGO.name + "FileContentConnector").AddComponent<LineRenderer>();
                line.positionCount = 2;
                line.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(0.219f, 0.329f, 0.556f, 1f) };
                line.startWidth = LineWidth;
                line.endWidth = LineWidth;
                line.useWorldSpace = true;
                Vector3 startPosition = codeWindow.transform.position;
                startPosition.y -= textWindowHeight / 2;
                line.SetPosition(0, startPosition);
                line.SetPosition(1, currentGO.transform.position);
                line.transform.SetParent(tracingDisplay.transform);
            }
        }

        /// <summary>
        /// Returns the <see cref="Transform"/> of the immediate child of <paramref name="tracingDisplay"/>
        /// where the source code is shown.
        ///
        /// Assumption: <paramref name="tracingDisplay"/> has an immediate child named
        /// <see cref="scrollContainerFileContent"/>.
        /// </summary>
        /// <param name="tracingDisplay">composite game object representing the tracing display
        /// consisting of a source-code window and window showing the variable values</param>
        /// <returns>the scroll container where the source code is displayed</returns>
        /// <exception cref="Exception">thrown if <paramref name="tracingDisplay"/> does not have
        /// an immediate child named <see cref="scrollContainerFileContent"/></exception>
        private static Transform GetCodeWindow(GameObject tracingDisplay)
        {
            Transform codeWindow = tracingDisplay.transform.Find(scrollContainerFileContent);
            if (codeWindow == null)
            {
                throw new Exception($"Text window {tracingDisplay.FullName()} has no child {scrollContainerFileContent}.");
            }
            else
            {
                return codeWindow;
            }
        }

        /// <summary>
        /// Returns the height of the given <paramref name="tracingDisplay"/>.
        /// The <paramref name="tracingDisplay"/> is assumed to have a <see cref="RectTransform"/>
        /// attached.
        /// </summary>
        /// <param name="tracingDisplay">text windows whose height is to be determined</param>
        /// <returns>height of <paramref name="tracingDisplay"/></returns>
        /// <exception cref="Exception">thrown if <paramref name="tracingDisplay"/> does
        /// not have a <see cref="RectTransform"/></exception>
        private static float GetWindowHeight(GameObject tracingDisplay)
        {
            Transform codeWindow = GetCodeWindow(tracingDisplay);

            if (codeWindow.TryGetComponent(out RectTransform rectTransform))
            {
                // Each corner provides its world space value. The returned array of 4 vertices
                // is clockwise. It starts bottom left and rotates to top left, then top right,
                // and finally bottom right. Note that bottom left, for example, is an (x, y, z)
                // vector with x being left and y being bottom.
                Vector3[] corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);
                // corners[1] is top left corner
                // corners[0] is bottom left corner
                return corners[1].y - corners[0].y;
            }
            else
            {
                throw new Exception($"Text window {codeWindow.gameObject.FullName()} does not have a {nameof(RectTransform)}.");
            }
        }

        /// <summary>
        /// This method finds the path to the file of a given node and returns the content of the file.
        /// If the source code cannot be found, the single line "1. // empty" will be returned.
        /// Precondition: <paramref name="go"/> represents a Java class.
        /// </summary>
        /// <param name="go">the game object representing the class whose source code is to be returned</param>
        /// <returns>the file content of the corresponding source-code file (line numbers are appended)</returns>
        private static string GetFileContentForNode(GameObject go)
        {
            if (go.TryGetNode(out Node node))
            {
                string path = node.AbsolutePlatformPath();

                if (File.Exists(path))
                {
                    int lineNumber = 1;
                    string output = "";
                    foreach (string lineContent in File.ReadLines(path))
                    {
                        output += lineNumber + ". " + lineContent + Environment.NewLine;
                        lineNumber++;
                    }
                    return output;
                }
                else
                {
                    Debug.LogError($"Source code file {path} not found for game object {go.name}.\n");
                    return "1. // empty" + Environment.NewLine;
                }
            }
            else
            {
                Debug.LogError($"Game object {go.FullName()} has no valid {nameof(NodeRef)}.\n");
                return "1. // empty" + Environment.NewLine;
            }
        }

        /// <summary>
        /// Gets a GameObject tagged with Node from this objects list of GameObjects (nodesGos) that matches
        /// the location of the statement represented by <paramref name="index"/>.
        /// A node matches the location of a statement if the class name of the location equals
        /// the node's name.
        /// </summary>
        /// <param name="index">the index of the statement in this <see cref="parsedJLG.allstatements"/></param>
        /// <returns>Node for Statement, if exists, else null.</returns>
        private GameObject GetNodeForStatement(int index)
        {
            string searchedName = parsedJLG.GetStatementLocationString(index);
            return nodesGOs.TryGetValue(searchedName, out GameObject result) ? result : null;
        }

        /// <summary>
        /// Gets the FileContent GameObject of a given Node, if it exists in text windows. Otherwise returns null.
        /// </summary>
        /// <param name="classGO">given node</param>
        /// <returns>FileContent for node</returns>
        private GameObject GetFileContentGOForNode(GameObject classGO)
        {
            return textWindows.FirstOrDefault(fc => fc.name.Equals(classGO.name + fileContentNamePostfix));
        }

        /// <summary>
        /// Returns true if <see cref="go"/> is a node and the class it is representing is the class of
        /// the given statement.
        /// </summary>
        /// <param name="stmtIndex">index of the Java statement in parsedJLGs JavaStatement list</param>
        /// <param name="go">the GameObject</param>
        /// <returns>whether <paramref name="go"/> represent the class at given <paramref name="stmtIndex"/></returns>
        private bool NodeRepresentsStatementLocation(int stmtIndex, GameObject go)
        {
            return parsedJLG.GetStatementLocationString(stmtIndex) == go.name && go.CompareTag(Tags.Node);
        }

        /// <summary>
        /// Visualizes the current statement and then increases statementCounter by 1.
        /// </summary>
        private void NextStatement()
        {
            HighlightCurrentLineFadePrevious();

            // Generate the info text in the smaller text window for the current statement. The info text is built
            // in the parsedJLG object and then returned to the TMPro text component.
            GameObject fileContent = GetFileContentGOForNode(currentGO);
            fileContent.transform.GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text
                = parsedJLG.CreateStatementInfoString(statementCounter.Value, true);

            if (statementCounter.UpperBound())
            {
                const string begin = "End of execution trace reached.";
                if (KeyBindings.TryGetKeyCode(KeyAction.Previous, out KeyCode keyCode))
                {
                    Debug.Log(begin + $" Press '{keyCode}' to start playing backward.\n");
                }
                else
                {
                    Debug.Log(begin + "\n");
                }
                inAutomaticMode = false;
                playingForward = false;
            }
            else
            {
                MoveForwardToNextStatement();
            }
        }

        /// <summary>
        /// Visualizes the current statement and then decreases the statementCounter by 1.
        /// </summary>
        private void PreviousStatement()
        {
            HighlightCurrentLineFadePreviousReverse();

            // Generate the info text in the smaller text window for the current statement.
            // The info text is built in the parsedJLG object and then returned to the
            // TMPro text component.
            GameObject fileContent = GetFileContentGOForNode(currentGO);
            fileContent.transform.GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text
                = parsedJLG.CreateStatementInfoString(statementCounter.Value, false);

            if (statementCounter.LowerBound())
            {
                const string begin = "Start of execution trace reached.";
                if (KeyBindings.TryGetKeyCode(KeyAction.Next, out KeyCode keyCode))
                {
                    Debug.Log(begin + $" Press '{keyCode}' to start playing forward.\n");
                }
                else
                {
                    Debug.Log(begin + "\n");
                }
                inAutomaticMode = false;
                playingForward = true;
            }
            else
            {
                MoveBackwardToPreviousStatement();
            }
        }

        /// <summary>
        /// If <see cref="ShowOnlyCalls"/> is true, the <see cref="statementCounter"/> is increased
        /// until either the very last statement or an interprocedural control-flow
        /// statement (entry or exit) is reached. If <see cref="ShowOnlyCalls"/> is false,
        /// the <see cref="statementCounter"/> is increased by one.
        /// </summary>
        private void MoveForwardToNextStatement()
        {
            statementCounter.Increase();
            if (ShowOnlyCalls)
            {
                while (!statementCounter.UpperBound() && !CallOrReturnReached(statementCounter.Value))
                {
                    statementCounter.Increase();
                }
            }
        }

        /// <summary>
        /// If <see cref="ShowOnlyCalls"/> is true, the <see cref="statementCounter"/> is decreased
        /// until either the very first statement or an interprocedural control-flow
        /// statement (entry or exit) is reached. If <see cref="ShowOnlyCalls"/> is false,
        /// the <see cref="statementCounter"/> is decreased by one.
        /// </summary>
        private void MoveBackwardToPreviousStatement()
        {
            statementCounter.Decrease();
            if (ShowOnlyCalls)
            {
                while (!statementCounter.LowerBound() && !CallOrReturnReached(statementCounter.Value))
                {
                    statementCounter.Decrease();
                }
            }
        }

        /// <summary>
        /// True if the statement at <paramref name="stmtIndex"/> is an <see cref="StatementKind.Entry"/>
        /// or <see cref="StatementKind.Exit"/>.
        /// </summary>
        /// <param name="stmtIndex">index of the statement</param>
        /// <returns>true for interprocedural control-flow statements</returns>
        private bool CallOrReturnReached(int stmtIndex)
        {
            StatementKind statement = parsedJLG.AllStatements[stmtIndex].StatementType;
            return statement == StatementKind.Exit || statement == StatementKind.Entry;
        }

        /// <summary>
        /// Activates the text window of the currentGO and disables all other.
        /// </summary>
        private void ToggleTextWindows()
        {
            foreach (GameObject go in textWindows)
            {
                // text window of currentGO is always active
                go.SetActive(go.name == currentGO.name + fileContentNamePostfix);
            }
        }

        /// <summary>
        /// This method updates all stacks that are need for the visualization, depending on the play direction.
        /// The stacks modified here are: functionCalls and parsedJLG.ReturnValues.
        /// The returnValues stack is filled inside the parsedJLG Object when play direction is true/Forward.
        /// </summary>
        private void UpdateStacks()
        {
            if (playingForward)
            {
                // Executing forward.

                // If previous statement exited a class, metaphorically remove the statements class from the callstack
                // by disabling its functionCall and coloring it back to normal.
                // FIXME: statementCounter.Value - 1 makes no sense if ShowOnlyCalls.
                if (statementCounter.Value > 0 // not at the first statement
                        && parsedJLG.AllStatements[statementCounter.Value - 1].StatementType == StatementKind.Exit)
                {
                    // caller

                    GameObject nodeForPreviousStatement = GetNodeForStatement(statementCounter.Value - 1);

                    if (currentGO != nodeForPreviousStatement)
                    {
                        GameObject functionCall = FindFunctionCallForGameObjects(nodeForPreviousStatement, currentGO, true);
                        if (functionCall != null)
                        {
                            functionCall.SetActive(false); // only disable, so it can be enabled when the visualization is running backwards
                            nodeForPreviousStatement.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                        }
                    }
                }
            }
            else
            {
                // Executing backward.

                bool exitingCalledFunction = parsedJLG.AllStatements[statementCounter.Value].StatementType == StatementKind.Exit;
                if (exitingCalledFunction)
                {
                    parsedJLG.ReturnValues.Pop(); // remove return value from stack, that is returned in the exit statement.
                }

                GameObject nodeForNextStatement = statementCounter.Value < parsedJLG.AllStatements.Count - 1 ? GetNodeForStatement(statementCounter.Value + 1) : null;
                if (exitingCalledFunction && currentGO != nodeForNextStatement && nodeForNextStatement != null)
                {
                    GameObject functionCall = FindFunctionCallForGameObjects(currentGO, nodeForNextStatement, false);
                    //Check for null because sometimes, this can break if the play direction is changed a lot in a short time.
                    if (functionCall != null)
                    {
                        functionCall.SetActive(true);
                    }
                }
                // If previous statement entered a class, metaphorically remove the class from the callstack by destroying its text window and its FunctionCallSimulator.
                // Looking for an at statementCounter+1 "entry statement", because the visualisation is running backwards.
                else if (statementCounter.Value < parsedJLG.AllStatements.Count - 1
                    && parsedJLG.AllStatements[statementCounter.Value + 1].StatementType == StatementKind.Entry
                    && currentGO != nodeForNextStatement)
                {
                    nodeForNextStatement.GetComponentInChildren<MeshRenderer>().material.color = new Color(1f, 0f, 0f, 1f);
                    if (functionCalls.Count > 0 &&
                        functionCalls.Peek().name.Equals("FunctionCall: " + currentGO.name + " call " + nodeForNextStatement.name))
                    {
                        Destroyer.Destroy(functionCalls.Pop());
                    }
                }
            }
        }

        /// <summary>
        /// Returns the latest <paramref name="active"/> FunctionCall that matches the two given GameObjects.
        /// </summary>
        /// <param name="dstGO">destination of the call</param>
        /// <param name="srcGO">source of the call</param>
        /// <param name="active">whether the returned function should be active</param>
        /// <returns>function call</returns>
        private GameObject FindFunctionCallForGameObjects(GameObject dstGO, GameObject srcGO, bool active)
        {
            // The enumerator of a stack iterates from the top element to the very first element added to the stack.
            return functionCalls.FirstOrDefault(go => go.activeSelf == active &&
                                                      go.GetComponent<FunctionCallSimulator>().Source == srcGO &&
                                                      go.GetComponent<FunctionCallSimulator>().Destination == dstGO);
        }

        /// <summary>
        /// Highlights the current line and fades the previous five lines bit by bit back to white.
        /// This is called when play direction is forward/true.
        /// </summary>
        private void HighlightCurrentLineFadePrevious()
        {
            GameObject currentFileContentGO = GetFileContentGOForNode(currentGO);

            //cancel the method if there was no FileContent GameObject found. This should never happen though.
            if (currentFileContentGO == null)
            {
                return;
            }

            string fileContent = currentFileContentGO.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text;
            string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            lines = FadePreviousLines(lines);


            ///For some Reason this is sometimes false and it would throw an error.
            if (lines.Length - 1 > parsedJLG.AllStatements[statementCounter.Value].LineAsInt())
            {
                string currentLineString = lines[parsedJLG.AllStatements[statementCounter.Value].LineAsInt() - 1];
                ///strip currentline of previous highlighting, if it has it
                if (currentLineString.StartsWith("<color=#"))
                {
                    ///remove color tag at start
                    currentLineString = currentLineString.Substring(currentLineString.IndexOf('>') + 1);
                    ///remove color tag at end
                    currentLineString = currentLineString.Remove(currentLineString.LastIndexOf('<'));
                }
                ///highlight currentline, LineAsInt -1, because lines array starts counting at 0 and Classlines start at 1.
                lines[parsedJLG.AllStatements[statementCounter.Value].LineAsInt() - 1] = "<color=#5ACD5A>" + currentLineString + "</color>";

                ///return lines array back to a single string and then save the new highlighted string in the GameObject.
                fileContent = string.Join(Environment.NewLine, lines);
                currentFileContentGO.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = fileContent;

                ///Scroll the Scroll rect so the current line is visible. This is not optimal yet, since sometimes it shows the current line right at the top of the Textwindow and not in the middle.
                currentFileContentGO.transform.GetChild(0).GetComponent<ScrollRect>().verticalNormalizedPosition = 1 - parsedJLG.AllStatements[statementCounter.Value].LineAsInt() / (float)lines.Length;
            }
        }

        /// <summary>
        /// Highlights the current line and unfades previously faded lines back to white. Can only actively highlight the current line.
        /// This is called when play direction is backwards/false.
        /// </summary>
        private void HighlightCurrentLineFadePreviousReverse()
        {
            GameObject currentFileContentGO = GetFileContentGOForNode(currentGO);

            //cancel the method if there was no FileContent GameObject found. This should never happen though.
            if (currentFileContentGO == null)
            {
                return;
            }

            string fileContent = currentFileContentGO.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text;
            string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            lines = UnfadePreviousLines(lines);

            ///For some Reason this is sometimes false and it would throw an error.
            if (lines.Length - 1 > parsedJLG.AllStatements[statementCounter.Value].LineAsInt())
            {
                lines[parsedJLG.AllStatements[statementCounter.Value].LineAsInt() - 1] = "<color=#5ACD5A>" + lines[parsedJLG.AllStatements[statementCounter.Value].LineAsInt() - 1] + "</color>";

                fileContent = string.Join(Environment.NewLine, lines);
                currentFileContentGO.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = fileContent;

                currentFileContentGO.transform.GetChild(0).GetComponent<ScrollRect>().verticalNormalizedPosition = 1 - parsedJLG.AllStatements[statementCounter.Value].LineAsInt() / (float)lines.Length;
            }
        }

        /// <summary>
        /// This Method reverses the highlighting of lines by coloring them all white again.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private static string[] UnfadePreviousLines(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("<color="))
                {
                    if (lines[i].StartsWith("<color=#5ACD5A>"))
                    {
                        lines[i] = lines[i].Replace("<color=#5ACD5A>", "");
                        lines[i] = lines[i].Replace("</color>", "");
                    }
                    else if (lines[i].StartsWith("<color=#A5CDA5>"))
                    {
                        lines[i] = lines[i].Replace("<color=#A5CDA5>", "");
                        lines[i] = lines[i].Replace("</color>", "");
                    }
                    else if (lines[i].StartsWith("<color=#78CD78>"))
                    {
                        lines[i] = lines[i].Replace("<color=#78CD78>", "");
                        lines[i] = lines[i].Replace("</color>", "");
                    }
                    else if (lines[i].StartsWith("<color=#96CD96>"))
                    {
                        lines[i] = lines[i].Replace("<color=#96CD96>", "");
                        lines[i] = lines[i].Replace("</color>", "");
                    }
                    else if (lines[i].StartsWith("<color=#B4CDB4>"))
                    {
                        lines[i] = lines[i].Replace("<color=#B4CDB4>", "");
                        lines[i] = lines[i].Replace("</color>", "");
                    }
                }
            }
            return lines;
        }

        /// <summary>
        /// This Method fades highlighted lines in 5 steps back to white.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private static string[] FadePreviousLines(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("<color="))
                {
                    if (lines[i].StartsWith("<color=#5ACD5A>"))
                    {
                        lines[i] = lines[i].Replace("<color=#5ACD5A>", "<color=#78CD78>");
                    }
                    else if (lines[i].StartsWith("<color=#78CD78>"))
                    {
                        lines[i] = lines[i].Replace("<color=#78CD78>", "<color=#96CD96>");
                    }
                    else if (lines[i].StartsWith("<color=#96CD96>"))
                    {
                        lines[i] = lines[i].Replace("<color=#96CD96>", "<color=#A5CDA5>");
                    }
                    else if (lines[i].StartsWith("<color=#A5CDA5>"))
                    {
                        lines[i] = lines[i].Replace("<color=#A5CDA5>", "<color=#B4CDB4>");
                    }
                    else
                    {
                        lines[i] = lines[i].Replace("<color=#B4CDB4>", "");
                        lines[i] = lines[i].Replace("</color>", "");
                    }
                }
            }
            return lines;
        }

        /// <summary>
        /// This Method jumps to the next Breakpoint. If the Breakpoint is more
        /// than 200 Statements ahead, it only visualizes the last 150 steps.
        /// </summary>
        /// <returns></returns>
        private bool JumpToNextBreakpointHit()
        {
            playingForward = true;
            if (BreakpointLine <= 0)
            {
                return false;
            }
            if (string.IsNullOrEmpty(BreakpointClass))
            {
                return false;
            }
            JavaStatement js = parsedJLG.AllStatements[statementCounter.Value];
            int currentStatementCounter = statementCounter.Value;
            // Search for the next breakpoint.
            while (!(js.LineAsInt() == BreakpointLine && parsedJLG.GetStatementLocationString(currentStatementCounter).Contains(BreakpointClass)))
            {
                currentStatementCounter++;
                if (currentStatementCounter < parsedJLG.AllStatements.Count)
                {
                    js = parsedJLG.AllStatements[currentStatementCounter];
                }
                else
                {
                    return false;
                }
            }

            if (currentStatementCounter <= 300 || (currentStatementCounter - statementCounter.Value) < 200)
            {
                while (statementCounter.Value <= currentStatementCounter)
                {
                    UpdateVisualization();
                }
            }
            else if ((currentStatementCounter - statementCounter.Value) > 200)
            {
                statementCounter.Value = currentStatementCounter - 150;
                parsedJLG.AllStatements.RemoveRange(0, statementCounter.Value - 1);
                ResetVisualization();
                while (statementCounter.Value <= 151)
                {
                    UpdateVisualization();
                }
            }
            return true;
        }

        /// <summary>
        /// Adjusts the <see cref="statementCounter"/> to refer to the next statement
        /// to be executed and then triggers the visualization.
        /// </summary>
        /// <param name="direction">the direction where we should be executing,
        /// true for forward, false for backward</param>
        private void OneStep(bool direction)
        {
            bool savedDirection = playingForward;
            playingForward = direction;

            if (direction != lastDirectionWasForward)
            {
                // We have changed the direction since the last execution.
                // The statement at the statementCounter is the one to be executed next.
                // It has not been visualized yet. Its visualization would be done in
                // the call to UpdateVisualization() below.
                if (direction)
                {
                    // The new direction is forward. The previous direction was backward.
                    // The statementCounter is one before the statement just executed.
                    // The next statement we must execute is the one immediately
                    // after the last executed statement. Hence, we need to move the
                    // statementCounter two steps forward.
                    statementCounter.Increase(); // Move one step forward
                    // Move one more step forward (and possibly more to reach the next interprocedural
                    // control flow statement if only the calls are to be shown).
                    MoveForwardToNextStatement();

                }
                else
                {
                    // The new direction is backward. The previous direction was forward.
                    // The statementCounter is one behind the statement just executed.
                    // The next statement we must execute is the one immediately
                    // before the last executed statement. Hence, we need to move the
                    // statementCounter two steps back.
                    statementCounter.Decrease(); // Move one step back
                    // Move one more step back (and possibly more to reach the next interprocedural
                    // control flow statement if only the calls are to be shown).
                    MoveBackwardToPreviousStatement();
                }
            }
            UpdateVisualization();
            playingForward = savedDirection;
        }

        /// <summary>
        /// Speeds up the playing speed of the visualization by x2 until a max of updating every 0.03125 seconds or 32 statements per second.
        /// </summary>
        private void SpeedUp()
        {
            if (updateInterval > 0.03125)
            {
                nextUpdateTime = nextUpdateTime - updateInterval + (updateInterval / 2);
                updateInterval /= 2;
            }
        }

        /// <summary>
        /// Slows down the playing speed of the visualization by x2 until a minimum of updating every 8 seconds or 1 statement every 8 seconds.
        /// </summary>
        private void SlowDown()
        {
            if (updateInterval < 8)
            {
                nextUpdateTime = nextUpdateTime - updateInterval + (updateInterval * 2);
                updateInterval *= 2;
            }
        }

        /// <summary>
        /// This method resets the visualization by destroying all text windows and functionCalls and setting the StatementCounter to 0.
        /// </summary>
        private void ResetVisualization()
        {
            foreach (GameObject go in textWindows)
            {
                Destroyer.Destroy(go);
            }
            textWindows = new Stack<GameObject>();
            foreach (GameObject go in functionCalls)
            {
                Destroyer.Destroy(go);
            }
            functionCalls = new Stack<GameObject>();
            statementCounter.Value = 0;
        }

        /// <summary>
        /// This Method does a complete reset by calling ResetVisualization() and reparsing the JLG-File by calling Start().
        /// </summary>
        private void ResetComplete()
        {
            ResetVisualization();
            Start();
            foreach (GameObject go in nodesGOs.Values)
            {
                go.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            }
        }
    }
}
