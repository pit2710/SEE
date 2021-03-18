﻿using System.Collections.Generic;
using SEE.DataModel;
using SEE.DataModel.DG;
using SEE.DataModel.DG.IO;
using SEE.Game;
using SEE.GO;
using SEE.Layout;
using SEE.Layout.EdgeLayouts;
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
    public class MappingAction : CityAction, Observer
    {
        /// <summary>
        /// Which kind of city we are currently focusing on.
        /// </summary>
        private enum HitCity
        {
            None,
            Architecture,
            Implementation
        }

        private struct Selection
        {
            internal NodeRef nodeRef;
            internal InteractableObject interactableObject; // TODO(torben): it is time to combine NodeRefs and InteractableObjects or at least have some dictionary for them...
            internal Material material;
            internal Color initialColor;
            internal Color highlightColor;
            // Rainer: note that gameObjects with an EdgeRef instead of NodeRef now may also have a InteractableObject component.
        }

        private struct _ActionState
        {
            internal bool copy;              // copy selected object (i.e., start mapping)
            internal bool paste;             // paste (map) copied object
            internal bool clearClipboard;    // whether the clipboard of copied nodes has been cleared
            internal bool save;              // whether the current mapping should be stored
            internal HitCity hitCity;        // which city we are currently focusing on
        }

        private struct SpinningCubeData
        {
            internal GameObject go;
            internal float timer;
        }

        private const float HighlightLoopTimeInSeconds = 2.0f;
        private const float SpinningCursorCubeLoopTimeInSeconds = 4.0f; // TODO(torben): maybe only use one timer for everything
        private const uint NumEdgeCubes = 1;
        private const float MaxVelocity = 0.003f; // TODO(torben): this should be relative and be adapted to the table size!

        [Tooltip("The game object representing the architecture.")]
        public GameObject Architecture;

        [Tooltip("The game object representing the implementation.")]
        public GameObject Implementation;

        [Tooltip("The GXL file containing the mapping from implementation onto architecture entities.")]
        public string MappingFile;

        [Tooltip("Prefab for absences")]
        public GameObject AbsencePrefab;

        [Tooltip("Prefab for convergences")]
        public GameObject ConvergencePrefab;

        [Tooltip("Prefab for divergences")]
        public GameObject DivergencePrefab;

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
        /// The graph containing the architecture.
        /// </summary>
        private Graph archGraph;

        /// <summary>
        /// The graph containing the impementation.
        /// </summary>
        private Graph implGraph;

        /// <summary>
        /// For the reflexion analysis.
        /// </summary>
        public Reflexion Reflexion { get; private set; }

        /// <summary>
        /// Used for the visualization and decoration of reflexion edges.
        /// </summary>
        private ReflexionDecorator decorator;

        /// <summary>
        /// The current selection of a game object and its associated graph node.
        /// </summary>
        private Selection selection;

        private _ActionState actionState;

        private float highlightTimer;
        private Material materialCopy; // This mateiral has no portal
        private SpinningCubeData spinningCubeData;

        private Vector3 target;
        private bool hasTarget;
        private Vector3[] velocities = new Vector3[NumEdgeCubes];
        private GameObject[] edgeGOs = new GameObject[NumEdgeCubes];
        private MeshRenderer[] edgeMeshRenderers = new MeshRenderer[NumEdgeCubes];

        LineRenderer activeMappingEdge = null;

        /// <summary>
        /// The game objects that have been copied to the clipboard via Ctrl-C.
        /// </summary>
        private readonly HashSet<Selection> objectsInClipboard = new HashSet<Selection>();

        /// <summary>
        /// Mapping of edge IDs onto game objects representing these edges in the architecture code city.
        /// </summary>
        private readonly Dictionary<string, GameObject> architectureEdges = new Dictionary<string, GameObject>();

        /// <summary>
        /// Mapping of node IDs onto game objects representing these nodes in the architecture code city.
        /// </summary>
        private readonly Dictionary<string, GameObject> architectureNodes = new Dictionary<string, GameObject>();

        private void Start()
        {
            if (!Assertions.DisableOnCondition(this, Architecture == null, "No architecture city was specified for architectural mapping."))
            {
                archGraph = SceneQueries.GetGraph(Architecture);
                Assertions.DisableOnCondition(this, archGraph == null, "The architecture city has no associated graph.");
            }
            if (!Assertions.DisableOnCondition(this, Implementation == null, "No implementation city was specified for architectural mapping."))
            {
                implGraph = SceneQueries.GetGraph(Implementation);
                Assertions.DisableOnCondition(this, implGraph == null, "The implementation city has no associated graph.");
            }

            if (string.IsNullOrEmpty(MappingFile))
            {
                Debug.LogWarning("A filename for the architectural mapping should be set. Continuing with an empty mapping. Mapping cannot be saved.");
                mapping = new Graph();
            }
            else
            {
                mapping = LoadMapping(MappingFile);
                if (mapping == null)
                {
                    Debug.LogErrorFormat("A GXL containing the mapping could not be loaded from {0}. We are using an empty mapping instead.", MappingFile);
                    mapping = new Graph();
                }
                else
                {
                    Debug.LogFormat("Mapping successfully loaded from {0}\n", MappingFile);
                }
            }

            Assertions.DisableOnCondition(this, AbsencePrefab == null, "No material assigned for absences.");
            Assertions.DisableOnCondition(this, ConvergencePrefab == null, "No material assigned for convergences.");
            Assertions.DisableOnCondition(this, DivergencePrefab == null, "No material assigned for divergences.");

            if (!Assertions.DisableOnCondition(this, !Architecture.TryGetComponent(out SEECity city), "The object representing the architecture has no SEECity component attached to it."))
            {
                architectureGraphRenderer = city.Renderer;
                Assertions.DisableOnCondition(this, architectureGraphRenderer == null, "The SEECity component attached to the object representing the architecture has no graph renderer.");
            }

            if (enabled)
            {
                // Setup reflexion decorator
                Portal.GetDimensions(Architecture, out Vector2 leftFrontCorner, out Vector2 rightBackCorner);
                decorator = new ReflexionDecorator(AbsencePrefab, ConvergencePrefab, DivergencePrefab, leftFrontCorner, rightBackCorner);

                // Setup game object mappings
                GatherNodesAndEdges(Architecture, architectureNodes, architectureEdges);

                // Setup reflexion
                Reflexion = new Reflexion(implGraph, archGraph, mapping);
                Reflexion.Register(this);
                // An initial run is necessary to set up the necessary data structures.
                Reflexion.Run();
            }

            ActionState.OnStateChanged += OnStateChanged;
            if (!Assertions.DisableOnCondition(this, !Equals(ActionState.Value, ActionStateType.Map)))
            {
                InteractableObject.AnyHoverIn += AnyHoverIn;
                InteractableObject.AnyHoverOut += AnyHoverOut;
                InteractableObject.AnySelectIn += AnySelectIn;
                InteractableObject.AnySelectOut += AnySelectOut;
            }

            CubeFactory cubeFactory = new CubeFactory(Materials.ShaderType.Opaque, new ColorRange(Color.red, Color.red, 1));
            for (uint i = 0; i < NumEdgeCubes; i++)
            {
                edgeGOs[i] = cubeFactory.NewBlock(0, 0);
                edgeGOs[i].layer = 2; // Note: This will make raycasting ignore this object. Physics.IgnoreRaycastLayer contains the wrong value (water mask)!
                edgeGOs[i].transform.position = Vector3.zero;
                edgeGOs[i].transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
                edgeMeshRenderers[i] = edgeGOs[i].GetComponent<MeshRenderer>();

                edgeGOs[i].SetActive(false);
            }
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
                if (implGraph.GetNode(edge.Source.ID) == null)
                {
                    Debug.LogWarning($"The mapping contains an implementation node {edge.Source.ID} (source) that is not in the implementation graph for maps-to edge {edge}.\n");
                    nodesToBeRemoved.Add(edge.Source);
                }
                if (archGraph.GetNode(edge.Target.ID) == null)
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

        private LineRenderer CreateEdge(Node from, Node to)
        {
            LineRenderer result = null;

            // TODO(torben): This is way to inefficient to create an edge!!!
            NodeRef nr0 = NodeRef.Get(from);
            NodeRef nr1 = NodeRef.Get(to);
            AbstractSEECity settings = Implementation.GetComponent<SEECity>().Renderer.GetSettings();
            float minimalEdgeLevelDistance = 2.5f * settings.EdgeWidth;

            IEdgeLayout edgeLayout = new SplineEdgeLayout(settings.EdgesAboveBlocks, minimalEdgeLevelDistance, settings.RDP);
            NodeFactory nodeFactory = new CubeFactory(Materials.ShaderType.Opaque, new ColorRange(Color.white, Color.white, 1));
            EdgeFactory factory = new EdgeFactory(edgeLayout, settings.EdgeWidth, settings.TubularSegments, settings.Radius, settings.RadialSegments, settings.isEdgeSelectable);

            Dictionary<Node, ILayoutNode> to_layout_node = new Dictionary<Node, ILayoutNode>();
            ILayoutNode fromLayoutNode = new GameNode(to_layout_node, nr0.gameObject, nodeFactory);
            ILayoutNode toLayoutNode = new GameNode(to_layout_node, nr1.gameObject, nodeFactory);
            LayoutEdge layoutEdge = new LayoutEdge(fromLayoutNode, toLayoutNode, new Edge(from, to));

            ICollection<ILayoutNode> nodes = new List<ILayoutNode> { fromLayoutNode, toLayoutNode };
            ICollection<LayoutEdge> edges = new List<LayoutEdge> { layoutEdge };

            IEnumerator<GameObject> enumerator = factory.DrawEdges(nodes, edges).GetEnumerator();
            enumerator.MoveNext();
            result = enumerator.Current.GetComponent<LineRenderer>();
            LineFactory.SetColor(result, new Color(1.0f, 1.0f, 1.0f, 0.2f));

            return result;
        }

        private void Update()
        {
            // This script should be disabled, if the action state is not 'Map'
            if (!ActionState.Is(ActionStateType.Map))
            {
                enabled = false;
                InteractableObject.AnyHoverIn -= AnyHoverIn;
                InteractableObject.AnyHoverOut -= AnyHoverOut;
                InteractableObject.AnySelectIn -= AnySelectIn;
                InteractableObject.AnySelectOut -= AnySelectOut;
                return;
            }

            highlightTimer += Time.deltaTime;
            if (highlightTimer >= HighlightLoopTimeInSeconds)
            {
                highlightTimer -= HighlightLoopTimeInSeconds;
            }

            //------------------------------------------------------------------------
            // ARCHITECTURAL MAPPING
            //------------------------------------------------------------------------

            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                if (Raycasting.RaycastGraphElement(out RaycastHit _, out GraphElementRef elementRef) == HitGraphElement.Node) 
                {
                    // Select, replace or map
                    NodeRef nodeRef = elementRef as NodeRef;

                    Assert.IsNotNull(nodeRef);
                    Assert.IsNotNull(nodeRef.Value);

                    if (nodeRef.Value.ItsGraph == implGraph) // Set or replace implementation node
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
                        selection.interactableObject.SetSelect(false, true);
                    }
                }
                else // Deselect
                {
                    selection.interactableObject?.SetSelect(false, true);
                }
            }

            if (selection.material)
            {
                float t = -Mathf.Cos((2.0f * Mathf.PI * highlightTimer) / HighlightLoopTimeInSeconds) * 0.5f + 0.5f;
                SetColor(t);
            }

            if (spinningCubeData.go != null)
            {
                spinningCubeData.timer += Time.deltaTime;
                while (spinningCubeData.timer > SpinningCursorCubeLoopTimeInSeconds)
                {
                    spinningCubeData.timer -= SpinningCursorCubeLoopTimeInSeconds;
                }
                float tPos = Mathf.Sin(2.0f * Mathf.PI * spinningCubeData.timer / SpinningCursorCubeLoopTimeInSeconds * 2.0f) * 0.5f + 0.5f; // y-range: [0.0, 1.0]
                float gr = 0.5f * MathExtensions.GoldenRatio;
                float ls = spinningCubeData.go.transform.localScale.x;

                //TODO: Camera.main should be cached for Update(),
                // as it has CPU overhead comparable to GameObject.GetComponent
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out RaycastHit hit);

                spinningCubeData.go.transform.position = hit.point + new Vector3(0.0f, gr * ls + tPos * gr * ls, 0.0f);
                spinningCubeData.go.transform.rotation = Quaternion.AngleAxis(spinningCubeData.timer / SpinningCursorCubeLoopTimeInSeconds * 180.0f, Vector3.up);

                float tCol = Mathf.Sin(2.0f * Mathf.PI * spinningCubeData.timer / SpinningCursorCubeLoopTimeInSeconds) * 0.5f + 0.5f;
            }

            // Apply forces
            bool hasOrigin = selection.nodeRef != null;
            if (hasOrigin && hasTarget)
            {
                for (uint i = 0; i < NumEdgeCubes; i++)
                {
                    // Apply forces to velocity
                    Vector3 toTarget = target - edgeGOs[i].transform.position;
                    float sqrMagnitude = toTarget.sqrMagnitude;
                    if (sqrMagnitude > 0.001f) // Move towards target
                    {
                        Vector3 direction = toTarget / Mathf.Sqrt(sqrMagnitude);
                        Vector3 force = 0.04f * direction;
                        Vector3 friction = PhysicsUtil.Friction(velocities[i], 2.0f);
                        Vector3 acceleration = force + friction;
                        velocities[i] += acceleration * Time.deltaTime; // TODO(torben): put in fixed update! use: Time.fixedDeltaTime
                    }
                    else // Reset
                    {
                        edgeGOs[i].transform.position = selection.nodeRef.transform.position;
                        velocities[i] = new Vector3(
                            Random.Range(-MaxVelocity, MaxVelocity),
                            Random.Range(0.0f, MaxVelocity),
                            Random.Range(-MaxVelocity, MaxVelocity));
                    }
                }
            }
            else // Only friction
            {
                for (uint i = 0; i < NumEdgeCubes; i++)
                {
                    Vector3 acceleration = PhysicsUtil.Friction(velocities[i], 2.0f);
                    velocities[i] += acceleration * Time.deltaTime; // TODO(torben): put in fixed update! use: Time.fixedDeltaTime
                }
            }

            // Clamp and apply velocity
            for (uint i = 0; i < NumEdgeCubes; i++)
            {
                velocities[i] = PhysicsUtil.ClampVelocity(velocities[i], MaxVelocity);
                edgeGOs[i].transform.position += velocities[i];
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

        private void OnStateChanged(ActionStateType value)
        {
            if (Equals(value, ActionStateType.Map))
            {
                InteractableObject.AnyHoverIn += AnyHoverIn;
                InteractableObject.AnyHoverOut += AnyHoverOut;
                InteractableObject.AnySelectIn += AnySelectIn;
                InteractableObject.AnySelectOut += AnySelectOut;
                enabled = true;
            }
            else
            {
                InteractableObject.AnyHoverIn -= AnyHoverIn;
                InteractableObject.AnyHoverOut -= AnyHoverOut;
                InteractableObject.AnySelectIn -= AnySelectIn;
                InteractableObject.AnySelectOut -= AnySelectOut;
                enabled = false; // We don't want to waste CPU time, if Update() doesn't do anything
            }
        }

        private void AnyHoverIn(InteractableObject interactableObject, bool isOwner)
        {
            if (interactableObject.TryGetComponent(out NodeRef to) && interactableObject != selection.interactableObject) // TODO(torben): only, if the interactableObject is from architecture! @ArchInteract
            {
                if (selection.nodeRef)
                {
                    target = interactableObject.transform.position;
                    hasTarget = true;
                    Assert.IsNull(activeMappingEdge);
                    activeMappingEdge = CreateEdge(selection.nodeRef.Value, to.Value);
                }
            }
        }

        private void AnyHoverOut(InteractableObject interactableObject, bool isOwner)
        {
            hasTarget = false;
            if (activeMappingEdge)
            {
                Destroy(activeMappingEdge.gameObject);
                activeMappingEdge = null;
            }
        }

        private void AnySelectIn(InteractableObject interactableObject, bool isOwner)
        {
            Assert.IsNull(selection.nodeRef);
            Assert.IsNull(selection.interactableObject);

            // Spinning cube above cursor

            spinningCubeData.go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spinningCubeData.go.name = "MappingAction.spinningCube";
            spinningCubeData.go.layer = 2; // Note: This will make raycasting ignore this object. Physics.IgnoreRaycastLayer contains the wrong value (water mask)!
            float scale = 0.1f * Implementation.GetComponent<GO.Plane>().MinLengthXZ;
            spinningCubeData.go.transform.localScale = new Vector3(scale, scale, scale);

            materialCopy = new Material(interactableObject.GetComponent<MeshRenderer>().material);
            spinningCubeData.go.GetComponent<MeshRenderer>().material = materialCopy;
            Portal.SetInfinitePortal(spinningCubeData.go);

            spinningCubeData.timer = -Time.deltaTime;

            spinningCubeData.go.SetActive(false);

            // Select and highlight object

            highlightTimer = 0.0f;
            selection.nodeRef = interactableObject.GetComponent<NodeRef>();
            selection.interactableObject = interactableObject;
            selection.material = interactableObject.GetComponent<MeshRenderer>().material;

            Color color0 = selection.material.color;
            Vector3 c0 = ((Vector4)color0).XYZ();
            Vector3 c1 = Vector3.one - c0;
            Vector3 d = c1 - c0;
            float sqrMag = d.sqrMagnitude;
            const float MaxMag = 0.5f;
            const float MaxSqrMag = MaxMag * MaxMag;
            if (sqrMag > MaxSqrMag) // Keep the difference in colors somewhat reasonable
            {
                d /= Mathf.Sqrt(sqrMag) / MaxMag;
                c1 = c0 + d;
            }

            selection.initialColor = new Color(c0.x, c0.y, c0.z, color0.a);
            //selection.highlightColor = new Color(c1.x, c1.y, c1.z, color0.a * (color0.a >= 0.5f ? 0.5f : 2.0f));
            selection.highlightColor = new Color(c1.x, c1.y, c1.z, color0.a);

            // Edge visualization

            for (uint i = 0; i < NumEdgeCubes; i++)
            {
                edgeGOs[i].transform.position = selection.nodeRef.transform.position;
                edgeMeshRenderers[i].material = materialCopy;
            }
        }

        private void AnySelectOut(InteractableObject interactableObject, bool isOwner)
        {
            Assert.IsNotNull(selection.nodeRef);
            Assert.IsNotNull(selection.interactableObject);
            Assert.IsNotNull(interactableObject.GetComponent<MeshRenderer>());
            Assert.IsNotNull(interactableObject.GetComponent<MeshRenderer>());

            Destroy(spinningCubeData.go);
#if UNITY_EDITOR
            spinningCubeData.go = null;
            spinningCubeData.timer = 0.0f;
#endif

            highlightTimer = 0.0f;
            selection.material.color = selection.initialColor;
            selection.nodeRef = null;
            selection.interactableObject = null;
            selection.material = null;
        }

        private void SetColor(float t)
        {
            selection.material.color = Color.Lerp(selection.initialColor, selection.highlightColor, t);
            materialCopy.color = selection.material.color;
        }

        /// <summary>
        /// Called by incremental reflexion for every change in the reflexion model
        /// by way of the observer protocol as a callback. Dispatches the event to
        /// the appropriate handling function.
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

            const float Coeff = 4.0f;

            Color initialColor = activeMappingEdge.startColor;
            Color highlightColor = new Color(initialColor.r, initialColor.g, initialColor.b, Mathf.Min(1.0f, Coeff * initialColor.a));
            Color finalColor = new Color(initialColor.r, initialColor.g, initialColor.b, Mathf.Max(0.05f, initialColor.a / Coeff));

            EdgeAnimator.Create(activeMappingEdge.gameObject, initialColor, highlightColor, finalColor, 1.0f, 3.0f);
            activeMappingEdge = null;
        }

        private void HandleMapsToEdgeRemoved(MapsToEdgeRemoved mapsToEdgeRemoved)
        {
            Debug.Log(mapsToEdgeRemoved.ToString());
        }
    }
}