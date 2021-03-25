﻿using System.Collections.Generic;
using SEE.DataModel;
using SEE.DataModel.DG;
using SEE.DataModel.DG.IO;
using SEE.Game;
using SEE.GO;
using SEE.Tools;
using SEE.Utils;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Implements the architectural mapping for the reflexion analysis. 
    /// This action assumes that it is attached to a game object representing
    /// the reflexion analysis during the game. 
    /// </summary>
    public class MappingAction : Observer, ReversibleAction
    {
        private const float SelectedAlpha = 0.8f;

        [Tooltip("The game object representing the architecture.")]
        public GameObject Architecture;

        [Tooltip("The game object representing the implementation.")]
        public GameObject Implementation;

        [Tooltip("The GXL file containing the mapping from implementation onto architecture entities.")]
        public string MappingFile;

        /// <summary>
        /// The graph renderer used to draw the city. There must be a component
        /// SEECity attached to the game object this MappingAction is attached to
        /// from which we derived its graph renderer.
        /// </summary>
        private GraphRenderer architectureGraphRenderer;

        /// <summary>
        /// The graph containing the mapping from implementation onto architecture entities.
        /// </summary>
        private Graph mapping;

        /// <summary>
        /// Returns a new instance of <see cref="MappingAction"/>.
        /// </summary>
        /// <returns>new instance of <see cref="MappingAction"/></returns>
        internal static ReversibleAction CreateReversibleAction()
        {
            return new MappingAction();
        }

        /// <summary>
        /// Returns a new instance of <see cref="MappingAction"/>.
        /// </summary>
        /// <returns>new instance</returns>
        public ReversibleAction NewInstance()
        {
            return CreateReversibleAction();
        }

        /// <summary>
        /// The graph containing the architecture.
        /// </summary>
        private Graph architecture;

        /// <summary>
        /// The graph containing the impementation.
        /// </summary>
        private Graph implementation;

        /// <summary>
        /// For the reflexion analysis.
        /// </summary>
        public Reflexion Reflexion { get; private set; }

        /// <summary>
        /// Materials for the decoration of reflexion edges.
        /// </summary>
        [Tooltip("Prefab for absences")]
        public GameObject AbsencePrefab;
        [Tooltip("Prefab for convergences")]
        public GameObject ConvergencePrefab;
        [Tooltip("Prefab for divergences")]
        public GameObject DivergencePrefab;

        private struct Selection
        {
            internal NodeRef nodeRef;
            internal InteractableObject interactableObject; // TODO(torben): it is time to combine NodeRefs and InteractableObjects or at least have some dictionary for them...
            // Rainer: note that gameObjects with an EdgeRef instead of NodeRef now may also have a InteractableObject component.
        }

        /// <summary>
        /// The current selection of a game object and its associated graph node.
        /// </summary>
        private Selection selection;

        /// <summary>
        /// Which kind of city we are currently focusing on.
        /// </summary>
        private enum HitCity
        {
            None,
            Architecture,
            Implementation
        }

        private struct _ActionState
        {
            internal bool copy;              // copy selected object (i.e., start mapping)
            internal bool paste;             // paste (map) copied object
            internal bool clearClipboard;    // whether the clipboard of copied nodes has been cleared
            internal bool save;              // whether the current mapping should be stored
            internal HitCity hitCity;        // which city we are currently focusing on
        }

        private _ActionState actionState;

        /// <summary>
        /// The game objects that have been copied to the clipboard via Ctrl-C.
        /// </summary>
        private readonly HashSet<Selection> objectsInClipboard = new HashSet<Selection>();

        /// <summary>
        /// Initializes this instance.
        /// See <see cref="ReversibleAction.Awake"/>.
        /// </summary>
        // Use this for initialization
        public void Awake()
        {
            if (Architecture == null)
            {
                Debug.LogWarning("No architecture city was specified for architectural mapping.\n");
            }
            else
            {
                architecture = SceneQueries.GetGraph(Architecture);
                if (architecture == null)
                {
                    Debug.LogWarning("The architecture city has no associated graph.\n");
                }
            }

            if (Implementation == null)
            {
                Debug.LogWarning("No implementation city was specified for architectural mapping.\n");
            }
            else
            {
                implementation = SceneQueries.GetGraph(Implementation);
                if (implementation == null)
                {
                    Debug.LogWarning("The implementation city has no associated graph.\n");
                }
            }

            if (string.IsNullOrEmpty(MappingFile))
            {
                Debug.LogWarning("A filename for the architectural mapping should be set. Continuing with an empty mapping. Mapping cannot be saved.\n");
                mapping = new Graph();
            }
            else
            {
                mapping = LoadMapping(MappingFile);
                if (mapping == null)
                {
                    Debug.LogErrorFormat("A GXL containing the mapping could not be loaded from {0}. We are using an empty mapping instead.\n",
                                         MappingFile);
                    mapping = new Graph();
                }
                else
                {
                    Debug.LogFormat("Mapping successfully loaded from {0}\n", MappingFile);
                }
            }

            if (AbsencePrefab == null)
            {
                Debug.LogErrorFormat("No material assigned for absences.\n");
            }
            if (ConvergencePrefab == null)
            {
                Debug.LogErrorFormat("No material assigned for convergences.\n");
            }
            if (DivergencePrefab == null)
            {
                Debug.LogErrorFormat("No material assigned for divergences.\n");
            }
            if (Architecture.TryGetComponent(out SEECity city))
            {
                architectureGraphRenderer = city.Renderer;
                if (architectureGraphRenderer == null)
                {
                    Debug.LogErrorFormat("The SEECity component attached to the object representing the architecture has no graph renderer.\n");                }
            }
            else
            {
                Debug.LogErrorFormat("The object representing the architecture has no SEECity component attached to it.\n");
            }
            //if (enabled)
            {
                Usage();
                SetupReflexionDecorator();
                SetupGameObjectMappings();
                SetupReflexion();
            }

            ActionState.OnStateChanged += OnStateChanged;
            if (Equals(ActionState.Value, ActionStateType.Map))
            {
                InteractableObject.AnySelectIn += AnySelectIn;
                InteractableObject.AnySelectOut += AnySelectOut;
            }
            else
            {
                // enabled = false;
            }
        }

        /// <summary>
        /// See <see cref="ReversibleAction.Start"/>.
        /// </summary>
        public void Start()
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// See <see cref="ReversibleAction.Stop"/>.
        /// </summary>
        public void Stop()
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// See <see cref="ReversibleAction.Undo"/>.
        /// </summary>
        public void Undo()
        {
            Debug.Log("UNDO MAPPING");
        }

        /// <summary>
        /// See <see cref="ReversibleAction.Redo"/>.
        /// </summary>
        public void Redo()
        {
            Debug.Log("REDO MAPPING");
        }

        private void OnStateChanged(ActionStateType value)
        {
            if (Equals(value, ActionStateType.Map))
            {
                InteractableObject.AnySelectIn += AnySelectIn;
                InteractableObject.AnySelectOut += AnySelectOut;
            }
            else
            {
                InteractableObject.AnySelectIn -= AnySelectIn;
                InteractableObject.AnySelectOut -= AnySelectOut;
            }
        }

        /// <summary>
        /// Used for the visualization and decoration of reflexion edges.
        /// </summary>
        private ReflexionDecorator decorator;

        /// <summary>
        /// Sets up the reflexion decorator.
        /// </summary>
        private void SetupReflexionDecorator()
        {
            Portal.GetDimensions(Architecture, out Vector2 leftFrontCorner, out Vector2 rightBackCorner);
            decorator = new ReflexionDecorator(AbsencePrefab, ConvergencePrefab, DivergencePrefab,
                                               leftFrontCorner, rightBackCorner);
        }

        /// <summary>
        /// Mapping of edge IDs onto game objects representing these edges in the architecture code city.
        /// </summary>
        private readonly Dictionary<string, GameObject> architectureEdges = new Dictionary<string, GameObject>();

        /// <summary>
        /// Mapping of node IDs onto game objects representing these nodes in the architecture code city.
        /// </summary>
        private readonly Dictionary<string, GameObject> architectureNodes = new Dictionary<string, GameObject>();

        private void SetupGameObjectMappings()
        {
            GatherNodesAndEdges(Architecture, architectureNodes, architectureEdges);
        }

        /// <summary>
        /// Adds all game nodes and edges that are reachable by <paramref name="rootGameObject"/> to
        /// <paramref name="nodes"/> or <paramref name="edges"/>, respectively, or by any of its
        /// descendants. Game objects representing either graph nodes or edges are recognized
        /// by either Tags.Node or Tags.Edge, respectively.
        /// </summary>
        /// <param name="rootGameObject">root object of the object hierarchy</param>
        /// <param name="nodes">where game objects representing graph nodes are to be added</param>
        /// <param name="edges">where game objects representing graph edges are to be added</param>
        private static void GatherNodesAndEdges(GameObject rootGameObject, IDictionary<string, GameObject> nodes, IDictionary<string, GameObject> edges)
        {
            switch (rootGameObject.tag)
            {
                case Tags.Edge when rootGameObject.TryGetComponent(out EdgeRef edgeRef):
                {
                    Edge edge = edgeRef.edge;
                    if (edge != null)
                    {
                        edges[edge.ID] = rootGameObject;
                    }
                    else
                    {
                        Debug.LogErrorFormat("Game-object edge {0} without an invalid graph edge reference.\n", rootGameObject.name);
                    }

                    break;
                }
                case Tags.Edge:
                {
                    Debug.LogErrorFormat("Game-object edge {0} without graph edge reference.\n", rootGameObject.name);
                    break;
                }
                case Tags.Node when rootGameObject.TryGetComponent(out NodeRef nodeRef):
                {
                    Node node = nodeRef.Value;
                    if (node != null)
                    {
                        nodes[node.ID] = rootGameObject;
                    }
                    else
                    {
                        Debug.LogErrorFormat("Game-object node {0} without an invalid graph node reference.\n", rootGameObject.name);
                    }

                    break;
                }
                case Tags.Node: Debug.LogErrorFormat("Game-object node {0} without graph node reference.\n", rootGameObject.name);
                    break;
            }

            foreach (Transform child in rootGameObject.transform)
            {
                GatherNodesAndEdges(child.gameObject, nodes, edges);
            }
        }

        /// <summary>
        /// Prints the keys for all actions.
        /// </summary>
        private static void Usage()
        {
            Debug.Log("Keys for architectural mapping:\n");
            Debug.LogFormat(" copy/remove selected implementation node to/from clipboard: Ctrl-{0}\n", KeyBindings.AddOrRemoveFromClipboard);
            Debug.LogFormat(" map nodes in clipboard onto selected architecture node: Ctrl-{0}\n", KeyBindings.PasteClipboard);
            Debug.LogFormat(" clear clipboard: Ctrl-{0}\n", KeyBindings.ClearClipboard);
            Debug.LogFormat(" save mapping to GXL file: Ctrl-{0}\n", KeyBindings.SaveArchitectureMapping);
        }

        /// <summary>
        /// Loads and returns the mapping from the given GXL <paramref name="mappingFile"/>.
        /// </summary>
        /// <param name="mappingFile">GXL file from which to load the mapping</param>
        /// <returns>the loaded graph or null</returns>
        private Graph LoadMapping(string mappingFile)
        {
            // Note: There are no hierarchical edges in a mapping graph.
            GraphReader graphReader = new GraphReader(mappingFile, new HashSet<string>());
            graphReader.Load();
            Graph graph = graphReader.GetGraph();

            HashSet<Edge> edgesToBeRemoved = new HashSet<Edge>();
            HashSet<Node> nodesToBeRemoved = new HashSet<Node>();

            foreach (Edge edge in graph.Edges())
            {
                if (edge.Type != "Maps_To")
                {
                    Debug.LogWarningFormat("Unexpected edge type {0} in mapping for edge {1}.\n", edge.Type, edge);
                    edgesToBeRemoved.Add(edge);
                }
                if (implementation.GetNode(edge.Source.ID) == null)
                {
                    Debug.LogWarning($"The mapping contains an implementation node {edge.Source.ID} (source) that is not in the implementation graph for maps-to edge {edge}.\n");
                    nodesToBeRemoved.Add(edge.Source);
                }
                if (architecture.GetNode(edge.Target.ID) == null)
                {
                    Debug.LogWarning($"The mapping contains an architecture node {edge.Target.ID} (target) that is not in the architecture graph for maps-to edge {edge}.\n");
                    nodesToBeRemoved.Add(edge.Target);
                }
            }
            foreach (Edge edge in edgesToBeRemoved)
            {
                graph.RemoveEdge(edge);
            }
            foreach (Node node in nodesToBeRemoved)
            {
                graph.RemoveNode(node);
            }
            return graph;
        }

        /// <summary>
        /// Saves the given <paramref name="mapping"/> in the file <paramref name="mappingFile"/> in GXL.
        /// </summary>
        /// <param name="mapping">the mapping to be saved</param>
        /// <param name="mappingFile">the GXL filename where to store the <paramref name="mapping"/></param>
        private static void SaveMapping(Graph mapping, string mappingFile)
        {
            if (!string.IsNullOrEmpty(mappingFile))
            {
                GraphWriter.Save(mappingFile, mapping, "Belongs_To");
                Debug.LogFormat("Mapping successfully saved in GXL file {0}\n", mappingFile);
            }
        }

        private struct SpinningCube
        {
            internal GameObject gameObject;
            internal MeshRenderer meshRenderer;
            internal float timer;
            internal Color c0;
            internal Color c1;
        }

        SpinningCube spinningCube;

        /// <summary>
        /// See <see cref="ReversibleAction.Update"/>.
        /// </summary>
        /// <returns>true if completed</returns>
        public bool Update()
        {
            // This script should be disabled, if the action state is not 'Map'
            if (!ActionState.Is(ActionStateType.Map))
            {
                InteractableObject.AnySelectIn -= AnySelectIn;
                InteractableObject.AnySelectOut -= AnySelectOut;
                return false;
            }

            //------------------------------------------------------------------------
            // ARCHITECTURAL MAPPING
            //------------------------------------------------------------------------

            bool result = false;

            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                if (Raycasting.RaycastGraphElement(out RaycastHit _, out GraphElementRef elementRef) == HitGraphElement.Node) 
                {
                    // Select, replace or map
                    NodeRef nodeRef = elementRef as NodeRef;

                    Assert.IsNotNull(nodeRef);
                    Assert.IsNotNull(nodeRef.Value);

                    if (nodeRef.Value.ItsGraph == implementation) // Set or replace implementation node
                    {
                        if (selection.interactableObject != null)
                        {
                            selection.interactableObject.SetSelect(false, true);
                        }
                        nodeRef.GetComponent<InteractableObject>().SetSelect(true, true);
                    }
                    else if (selection.nodeRef != null) // Create mapping
                    {
                        Node n0 = selection.nodeRef.Value;
                        Node n1 = nodeRef.Value;
                        if (Reflexion.Is_Explicitly_Mapped(n0))
                        {
                            Node mapped = Reflexion.Get_Mapping().GetNode(n0.ID);
                            Assert.IsTrue(mapped.Outgoings.Count == 1);
                            Reflexion.Delete_From_Mapping(mapped.Outgoings[0]);
                        }
                        Reflexion.Add_To_Mapping(n0, n1);
                        // mapping is completed
                        result = false;
                        selection.interactableObject.SetSelect(false, true);
                    }
                }
                else // Deselect
                {
                    selection.interactableObject?.SetSelect(false, true);
                }
            }

            if (spinningCube.gameObject != null)
            {
                const float PERIOD = 4.0f;
                spinningCube.timer += Time.deltaTime;
                while (spinningCube.timer > PERIOD)
                {
                    spinningCube.timer -= PERIOD;
                }
                float tPos = Mathf.Sin(2.0f * Mathf.PI * spinningCube.timer / PERIOD * 2.0f) * 0.5f + 0.5f; // y-range: [0.0, 1.0]
                float gr = 0.5f * MathExtensions.GoldenRatio;
                float ls = spinningCube.gameObject.transform.localScale.x;

                //TODO: Camera.main should be cached for Update(),
                // as it has CPU overhead comparable to GameObject.GetComponent
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out RaycastHit hit);

                spinningCube.gameObject.transform.position = hit.point + new Vector3(0.0f, gr * ls + tPos * gr * ls, 0.0f);
                spinningCube.gameObject.transform.rotation = Quaternion.AngleAxis(spinningCube.timer / PERIOD * 180.0f, Vector3.up);

                float tCol = Mathf.Sin(2.0f * Mathf.PI * spinningCube.timer / PERIOD) * 0.5f + 0.5f;
                spinningCube.meshRenderer.material.color = (1.0f - tCol) * spinningCube.c0 + tCol * spinningCube.c1;
            }

#if false
            bool leftControl = LeftControlPressed();

            actionState.save = leftControl && Input.GetKeyDown(SaveKey);
            actionState.copy = leftControl && Input.GetKeyDown(CopyKey);
            actionState.paste = leftControl && Input.GetKeyDown(PasteKey);
            actionState.clearClipboard = leftControl && Input.GetKeyDown(ClearKey);

            // We can copy only from the implementation city and if there is a selected object.
            if (actionState.copy && actionState.hitCity == HitCity.Implementation && selection.go != null)
            {
                if (objectsInClipboard.Contains(selection))
                {
                    Debug.LogFormat("Removing node {0} from clipboard\n", selection.go.name);
                    objectsInClipboard.Remove(selection);
                }
                else
                {
                    Debug.LogFormat("Copying node {0} to clipboard\n", selection.go.name);
                    objectsInClipboard.Add(selection);
                }
            }
            if (actionState.clearClipboard)
            {
                Debug.Log("Node clipboard has been cleared.\n");
                objectsInClipboard.Clear();
            }
            // We can paste only into the architecture city and if we have a selected object as a target
            if (actionState.paste && actionState.hitCity == HitCity.Architecture && selection.go != null)
            {
                MapClipboardContent(selection);
            }
            // Save the mapping if requested.
            if (actionState.save && (actionState.hitCity == HitCity.Implementation || actionState.hitCity == HitCity.Implementation))
            {
                SaveMapping(mapping, MappingFile);
            }
#endif
            return result;
        }

        /// <summary>
        /// Maps all nodes in <code>objectsInClipboard</code> <onto <paramref name="target"/>.
        /// Assumption: all nodes in objectsInClipboard are implementation nodes.
        /// </summary>
        /// <param name="target">architecture node to be mapped on</param>
        private void MapClipboardContent(Selection target)
        {
            foreach (Selection implementation in objectsInClipboard)
            {
                if (!Reflexion.Is_Explicitly_Mapped(implementation.nodeRef.Value))
                {
                    Debug.LogFormat("Mapping {0} -> {1}.\n", implementation.nodeRef.name, target.nodeRef.name);
                    Reflexion.Add_To_Mapping(from: implementation.nodeRef.Value, to: target.nodeRef.Value);
                }
                else
                {
                    Debug.LogWarningFormat("Node {0} is already explicitly mapped.\n", implementation.nodeRef.name);
                }
            }
            objectsInClipboard.Clear();
        }

        /// <summary>
        /// Whether the left control key was pressed.
        /// </summary>
        /// <returns>true if the left control key was pressed</returns>
        private static bool LeftControlPressed()
        {
            // Control key capturing does not really work well in the editor.
            bool leftControl = false;
#if UNITY_EDITOR
            leftControl = true;
#else
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                leftControl = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                leftControl = false;
            }
#endif
            return leftControl;
        }

        private void SetupReflexion()
        {
            Reflexion = new Reflexion(implementation, architecture, mapping);
            Reflexion.Register(this);
            // An initial run is necessary to set up the necessary data structures.
            Reflexion.Run();
        }

        private void AnySelectIn(InteractableObject interactableObject, bool isOwner)
        {
            Assert.IsNull(selection.nodeRef);
            Assert.IsNull(selection.interactableObject);

            spinningCube.gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spinningCube.gameObject.name = "MappingAction.spinningCube";
            // Note: This will make raycasting ignore this object. Physics.IgnoreRaycastLayer contains the wrong value (water mask)!
            spinningCube.gameObject.layer = 2;
            float scale = 0.1f * Implementation.GetComponent<GO.Plane>().MinLengthXZ;
            spinningCube.gameObject.transform.localScale = new Vector3(scale, scale, scale);

            spinningCube.meshRenderer = spinningCube.gameObject.GetComponent<MeshRenderer>();
            spinningCube.meshRenderer.material = new Material(interactableObject.GetComponent<MeshRenderer>().material);
            spinningCube.meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            spinningCube.meshRenderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay;
            Portal.SetInfinitePortal(spinningCube.gameObject);

            spinningCube.timer = -Time.deltaTime;

            spinningCube.c0 = spinningCube.meshRenderer.material.color;
            spinningCube.c0.a = SelectedAlpha;
            spinningCube.c1 = spinningCube.c0 + new Color(0.2f, 0.2f, 0.2f, 0.0f);

            selection.nodeRef = interactableObject.GetComponent<NodeRef>();
            selection.interactableObject = interactableObject;
            SetAlpha(selection.nodeRef, SelectedAlpha);
        }

        private void AnySelectOut(InteractableObject interactableObject, bool isOwner)
        {
            Assert.IsNotNull(selection.nodeRef);
            Assert.IsNotNull(selection.interactableObject);

            Destroyer.DestroyGameObject(spinningCube.gameObject);
#if UNITY_EDITOR
            spinningCube.gameObject = null;
            spinningCube.meshRenderer = null;
            spinningCube.timer = 0.0f;
            spinningCube.c0 = new Color();
            spinningCube.c1 = new Color();
#endif

            SetAlpha(selection.nodeRef, 1.0f);
            selection.interactableObject = null;
            selection.nodeRef = null;
        }

        private void SetAlpha(NodeRef nodeRef, float alpha)
        {
            MeshRenderer meshRenderer = nodeRef.GetComponent<MeshRenderer>();
            Color color = meshRenderer.material.color;
            color.a = alpha;
            meshRenderer.material.color = color;
        }

        /// <summary>
        /// Called by incremental reflexion for every change in the reflexion model
        /// by way of the observer protocol as a callback. Dispatches the event to
        /// the appropriate handling function.
        /// 
        /// </summary>
        /// <param name="changeEvent">additional information about the change in the reflexion model</param>
        public void Update(ChangeEvent changeEvent)
        {
            switch (changeEvent)
            {
                case EdgeChange changedEvent: HandleEdgeChange(changedEvent);
                    break;
                case PropagatedEdgeAdded changedEvent: HandlePropagatedEdgeAdded(changedEvent);
                    break;
                case PropagatedEdgeRemoved changedEvent: HandlePropagatedEdgeRemoved(changedEvent);
                    break;
                case MapsToEdgeAdded changedEvent: HandleMapsToEdgeAdded(changedEvent);
                    break;
                case MapsToEdgeRemoved changedEvent: HandleMapsToEdgeRemoved(changedEvent);
                    break;
                default: Debug.LogErrorFormat("UNHANDLED CALLBACK: {0}\n", changeEvent);
                    break;
            }
        }

        /// <summary>
        /// <see cref="ReversibleAction.HadEffect"/>
        /// </summary>
        /// <returns>true if this action has had already some effect that would need to be undone</returns>
        public bool HadEffect()
        {
            return false; // FIXME
        }

        /// <summary>
        /// Handles every state change of an existing edge.
        /// </summary>
        /// <param name="edgeChange"></param>
        private void HandleEdgeChange(EdgeChange edgeChange)
        {
            Debug.LogFormat("edge of type {0} from {1} to {2} changed its state from {3} to {4}.\n",
                            edgeChange.edge.Type, edgeChange.edge.Source.ID, edgeChange.edge.Target.ID,
                            edgeChange.oldState, edgeChange.newState);

            // Possible edge changes:
            //  for specified architecture dependencies
            //    specified          => {absent, allowed_absent, convergent}
            //    absent             => {allowed_absent, convergent}
            //    allowed_absent     => {allowed, convergent}
            //    convergent         => {absent, allowed_absent}
            //  for implementation dependencies propagated to the architecture
            //    undefined          => {allowed, divergent, implicitly_allowed}
            //    allowed            => {divergent, implicitly_allowed}
            //    divergent          => {{allowed, implicitly_allowed}
            //    implicitly_allowed => {allowed, divergent}

            if (architectureEdges.TryGetValue(edgeChange.edge.ID, out GameObject gameEdge))
            {

                if (edgeChange.oldState != edgeChange.newState)
                {
                    switch (edgeChange.oldState)
                    {
                        //--------------------------------------
                        // Changes for architecture dependencies
                        //--------------------------------------
                        case State.specified:
                            // nothing to be done
                            break;
                        case State.absent:
                            decorator.UndecorateAbsence(gameEdge);
                            break;
                        case State.allowed_absent:
                            decorator.UndecorateAllowedAbsence(gameEdge);
                            break;
                        case State.convergent:
                            decorator.UndecorateConvergence(gameEdge);
                            break;

                        //-----------------------------------------------------------------------
                        // changes for implementation dependencies propagated to the architecture
                        //-----------------------------------------------------------------------
                        case State.divergent:
                            decorator.UndecorateDivergence(gameEdge);
                            break;
                        case State.allowed:
                            decorator.UndecorateAllowed(gameEdge);
                            break;
                        case State.implicitly_allowed:
                            decorator.UndecorateImplicitlyAllowed(gameEdge);
                            break;
                        default:
                            Debug.LogErrorFormat("UNHANDLED PREVIOUS EDGE STATE: {0}\n", edgeChange.oldState);
                            break;
                    }

                    switch (edgeChange.newState)
                    {
                        //--------------------------------------
                        // Changes for architecture dependencies
                        //--------------------------------------
                        case State.specified:
                            // nothing to be done
                            break;
                        case State.absent:
                            decorator.DecorateAbsence(gameEdge);
                            break;
                        case State.allowed_absent:
                            decorator.DecorateAllowedAbsence(gameEdge);
                            break;
                        case State.convergent:
                            decorator.DecorateConvergence(gameEdge);
                            break;

                        //-----------------------------------------------------------------------
                        // changes for implementation dependencies propagated to the architecture
                        //-----------------------------------------------------------------------
                        case State.divergent:
                            decorator.DecorateDivergence(gameEdge);
                            break;
                        case State.allowed:
                            decorator.DecorateAllowed(gameEdge);
                            break;
                        case State.implicitly_allowed:
                            decorator.DecorateImplicitlyAllowed(gameEdge);
                            break;
                        default:
                            Debug.LogErrorFormat("UNHANDLED NEW EDGE STATE: {0}\n", edgeChange.oldState);
                            break;
                    }
                }
            }
            else
            {
                Debug.LogErrorFormat("Edge {0} is unknown.\n", edgeChange.edge.ID);
            }
        }

        private void HandlePropagatedEdgeRemoved(PropagatedEdgeRemoved propagatedEdgeRemoved)
        {
            Debug.Log(propagatedEdgeRemoved.ToString());
        }

        private void HandlePropagatedEdgeAdded(PropagatedEdgeAdded propagatedEdgeAdded)
        {
            Debug.Log(propagatedEdgeAdded.ToString());

            Edge edge = propagatedEdgeAdded.propagatedEdge;
            GameObject source = architectureNodes[edge.Source.ID];
            GameObject target = architectureNodes[edge.Target.ID];
            List<GameObject> nodes = new List<GameObject> { source, target };
            // FIXME: Continue here.
            //ICollection<GameObject> edges = architectureGraphRenderer.EdgeLayout(nodes);
        }

        private void HandleMapsToEdgeAdded(MapsToEdgeAdded mapsToEdgeAdded)
        {
            Debug.Log(mapsToEdgeAdded.ToString());
        }

        private void HandleMapsToEdgeRemoved(MapsToEdgeRemoved mapsToEdgeRemoved)
        {
            Debug.Log(mapsToEdgeRemoved.ToString());
        }

        /// <summary>
        /// Returns the <see cref="ActionStateType"/> of this action.
        /// </summary>
        /// <returns>the <see cref="ActionStateType"/> of this action</returns>
        public ActionStateType GetActionStateType()
        {
            return ActionStateType.Map;
        }
    }
}