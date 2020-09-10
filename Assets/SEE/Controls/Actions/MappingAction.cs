﻿using SEE.DataModel;
using SEE.DataModel.IO;
using SEE.Game;
using SEE.GO;
using SEE.Tools.Architecture;
using SEE.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Controls
{
    /// <summary>
    /// Implements the architectural mapping for the reflexion analysis.
    /// </summary>
    public class MappingAction : CityAction, Observer
    {
        private const KeyCode SaveKey  = KeyCode.S;
        private const KeyCode CopyKey  = KeyCode.C;
        private const KeyCode PasteKey = KeyCode.V;
        private const KeyCode ClearKey = KeyCode.X;

        [Tooltip("The game object representing the architecture.")]
        public GameObject Architecture;

        [Tooltip("The game object representing the implementation.")]
        public GameObject Implementation;

        [Tooltip("The GXL file containing the mapping from implementation onto architecture entities.")]
        public string MappingFile;

        /// <summary>
        /// The graph containing the mapping from implementation onto architecture entities.
        /// </summary>
        private Graph mapping;

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
        private Reflexion reflexion;

        /// <summary>
        /// Materials for the decoration of reflexion edges.
        /// </summary>
        [Tooltip("Prefab for absencenes")]
        public GameObject AbsencePrefab;
        [Tooltip("Prefab for convergences")]
        public GameObject ConvergencePrefab;
        [Tooltip("Prefab for divergences")]
        public GameObject DivergencePrefab;

        private struct Selection
        {
            /// <summary>
            /// The currently selected game object. May be null if none has been selected
            /// or in case of deselection.
            /// </summary>
            internal GameObject gameNode;

            /// <summary>
            /// The graph node currently selected as retrieved from gameNode.
            /// If gameNode is null, graphNode is null, too. If gameNode
            /// is not null but has no attached node reference, graphNode is null, too.
            /// </summary>
            internal Node graphNode;
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

        private struct ActionState
        {
            internal bool copy;              // copy selected object (i.e., start mapping)
            internal bool paste;             // paste (map) copied object
            internal bool clearClipboard;    // whether the clipboard of copied nodes has been cleared
            internal bool save;              // whether the current mapping should be stored
            internal HitCity hitCity;        // which city we are currently focusing on
        }

        private ActionState actionState;

        /// <summary>
        /// The game objects that have been copied to the clipboard via Ctrl-C.
        /// </summary>
        private HashSet<Selection> objectsInClipboard = new HashSet<Selection>();

        // Use this for initialization
        void Start()
        {
            if (Architecture == null)
            {
                Debug.LogWarning("No architecture city was specified for architectural mapping.\n");                
                this.enabled = false;
            }
            else
            {
                architecture = GetGraph(Architecture);
                if (architecture == null)
                {
                    Debug.LogWarning("The architecture city has no associated graph.\n");
                    this.enabled = false;
                }
            }

            if (Implementation == null)
            {
                Debug.LogWarning("No implementation city was specified for architectural mapping.\n");
                this.enabled = false;
            }
            else
            {
                implementation = GetGraph(Implementation);
                if (implementation == null)
                {
                    Debug.LogWarning("The implementation city has no associated graph.\n");
                    this.enabled = false;
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
                this.enabled = false;
            }
            if (ConvergencePrefab == null)
            {
                Debug.LogErrorFormat("No material assigned for convergences.\n");
                this.enabled = false;
            }
            if (DivergencePrefab == null)
            {
                Debug.LogErrorFormat("No material assigned for divergences.\n");
                this.enabled = false;
            }

            if (this.enabled)
            {
                Usage();
                SetupGameObjectMappings();
                SetupReflexion();
                SetupReflexionDecorator();
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
        private Dictionary<string, GameObject> architectureEdges = new Dictionary<string, GameObject>();

        /// <summary>
        /// Mapping of node IDs onto game objects representing these nodes in the architecture code city.
        /// </summary>
        private Dictionary<string, GameObject> architectureNodes = new Dictionary<string, GameObject>();

        private void SetupGameObjectMappings()
        {
            GatherNodesAndEdges(Architecture, architectureNodes, architectureEdges);
        }

        /// <summary>
        /// Adds all game nodes and edges that are reachable by <paramref name="gameObject"/> to
        /// <paramref name="nodes"/> or <paramref name="edges"/>, respectively, or by any of its
        /// descendants. Game objects representing either graph nodes or edges are recognized
        /// by either Tags.Node or Tags.Edge, respectively.
        /// </summary>
        /// <param name="gameObject">root object of the object hierarchy</param>
        /// <param name="nodes">where game objects representing graph nodes are to be added</param>
        /// <param name="edges">where game objects representing graph edges are to be added</param>
        private void GatherNodesAndEdges(GameObject gameObject, Dictionary<string, GameObject> nodes, Dictionary<string, GameObject> edges)
        {
            if (gameObject.tag == Tags.Edge)
            {
                if (gameObject.TryGetComponent<EdgeRef>(out EdgeRef edgeRef))
                {
                    Edge edge = edgeRef.edge;
                    if (edge != null)
                    {
                        edges[edge.ID] = gameObject;
                    }
                    else
                    {
                        Debug.LogErrorFormat("Game-object edge {0} without an invalid graph edge reference.\n", gameObject.name);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Game-object edge {0} without graph edge reference.\n", gameObject.name);
                }
            }
            else if (gameObject.tag == Tags.Node)
            {
                if (gameObject.TryGetComponent<NodeRef>(out NodeRef nodeRef))
                {
                    Node node = nodeRef.node;
                    if (node != null)
                    {                        
                        nodes[node.ID] = gameObject;
                    }
                    else
                    {
                        Debug.LogErrorFormat("Game-object node {0} without an invalid graph node reference.\n", gameObject.name);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Game-object node {0} without graph node reference.\n", gameObject.name);
                }
            }
            foreach (Transform child in gameObject.transform)
            {
                GatherNodesAndEdges(child.gameObject, nodes, edges);
            }
        }

        /// <summary>
        /// Prints the keys for all actions.
        /// </summary>
        private static void Usage()
        {
            Debug.Log("Keys for architecutural mapping:\n");
            Debug.LogFormat(" copy/remove selected implementation node to/from clipboard: Ctrl-{0}\n", CopyKey);
            Debug.LogFormat(" map nodes in clipboard onto selected architecture node: Ctrl-{0}\n", PasteKey);
            Debug.LogFormat(" clear clipboard: Ctrl-{0}\n", ClearKey);
            Debug.LogFormat(" save mapping to GXL file: Ctrl-{0}\n", SaveKey);
        }

        /// <summary>
        /// Loads and returns the mapping from the given GXL <paramref name="mappingFile"/>.
        /// </summary>
        /// <param name="mappingFile"></param>
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
                    Debug.LogWarningFormat("Unexpected edge type {0} in mapping for edge {1}.\n", edge.Type, edge.ToString());
                    edgesToBeRemoved.Add(edge);
                }
                if (implementation.GetNode(edge.Source.ID) == null)
                {
                    Debug.LogWarningFormat("The mapping contains an implementation node that is not in the implementation graph.\n", edge.Source.ID);
                    nodesToBeRemoved.Add(edge.Source);
                }
                if (architecture.GetNode(edge.Target.ID) == null)
                {
                    Debug.LogWarningFormat("The mapping contains an architecture node that is not in the architecture graph.\n", edge.Source.ID);
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

        // Update is called once per frame
        void Update()
        {
            //------------------------------------------------------------------------
            // ARCHITECTURAL MAPPING
            //------------------------------------------------------------------------

            // We check whether we are focusing on the code city this NavigationAction is attached to.
            RaycastHit[] hits = Raycasting.SortedHits();
            // If we don't hit anything or if we hit anything (including another code city 
            // that is different from the code city this NavigationAction is attached to),
            // we will not process any user input.
            if (hits.Length == 0)
            {
                // nothing hit; we are not focusing on anything
                //Debug.Log("nothing hit\n");
                return;
            }
            else
            {
                Transform firstHit = GetHitCity(hits[0].transform);
                if (firstHit == null)
                {
                    // Neither the architecture nor the implementation city was hit.
                    //Debug.Log("neither architecture nor implementation was hit\n");
                    return;
                }
                else if (firstHit.gameObject == Architecture)
                {
                    //Debug.Log("hit architecture\n");
                    actionState.hitCity = HitCity.Architecture;
                }
                else if (firstHit.gameObject == Implementation)
                {
                    //Debug.Log("hit implementation\n");
                    actionState.hitCity = HitCity.Implementation;
                }
                else
                {
                    //Debug.LogFormat("hit some other city {0}\n", firstHit.name);
                    return;
                }
            }

            // Selection of an object
            if (Input.GetMouseButtonDown(0))  // left mouse button
            {
                bool selected = false;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.gameObject.TryGetComponent<NodeRef>(out NodeRef nodeRef))
                    {
                        selection.gameNode = hit.transform.gameObject;
                        selection.graphNode = nodeRef.node;
                        selected = true;
                        break;
                    }
                }
                if (!selected)
                {
                    selection.gameNode = null;
                    selection.graphNode = null;
                }
            }

            bool leftControl = LeftControlPressed();


            actionState.save = leftControl && Input.GetKeyDown(SaveKey);
            actionState.copy = leftControl && Input.GetKeyDown(CopyKey);
            actionState.paste = leftControl && Input.GetKeyDown(PasteKey);
            actionState.clearClipboard = leftControl && Input.GetKeyDown(ClearKey);

            //if (actionState.copy || actionState.paste || actionState.clearClipboard)
            //{
            //    Debug.LogFormat("copy={0} paste={1} clearClipboard={2} selectedObject={3}\n",
            //                    actionState.copy, actionState.paste, actionState.clearClipboard,
            //                    selection.gameNode != null ? selection.gameNode.name : "NONE");
            //}
            // We can copy only from the implementation city and if there is a selected object.
            if (actionState.copy && actionState.hitCity == HitCity.Implementation && selection.gameNode != null)
            {
                if (objectsInClipboard.Contains(selection))
                {
                    Debug.LogFormat("Removing node {0} from clipboard\n", selection.gameNode.name);
                    objectsInClipboard.Remove(selection);
                }
                else
                {
                    Debug.LogFormat("Copying node {0} to clipboard\n", selection.gameNode.name);
                    objectsInClipboard.Add(selection);
                }
            }            
            if (actionState.clearClipboard)
            {
                Debug.Log("Node clipboard has been cleared.\n");
                objectsInClipboard.Clear();
            }
            // We can paste only into the architecture city and if we have a selected object as a target
            if (actionState.paste && actionState.hitCity == HitCity.Architecture && selection.gameNode != null)
            {
                MapClipboardContent(selection);
            }
            // Save the mapping if requested.
            if (actionState.save && (actionState.hitCity == HitCity.Implementation || actionState.hitCity == HitCity.Implementation))
            {
                SaveMapping(mapping, MappingFile);
            }
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
                if (!reflexion.Is_Mapper(implementation.graphNode))
                {
                    Debug.LogFormat("Mapping {0} -> {1}.\n", implementation.gameNode.name, target.gameNode.name);
                    reflexion.Add_To_Mapping(from: implementation.graphNode, to: target.graphNode);
                }
                else
                {
                    Debug.LogWarningFormat("Node {0} is already explicitly mapped..\n", implementation.gameNode.name);
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
            reflexion = new Reflexion(implementation, architecture, mapping);
            reflexion.Register(this);
            // An initial run is necessary to set up the necessary data structures.
            reflexion.Run();
        }

        /// <summary>
        /// Called by incremental reflexion for every change in the reflexion model
        /// by way of the observer protocol as a callback. Dispatches the event to
        /// the approriate handling function.
        /// </summary>
        /// <param name="changeEvent">additional information about the change in the reflexion model</param>
        public void Update(ChangeEvent changeEvent)
        {
            if (changeEvent is EdgeChange)
            {
                HandleEdgeChange(changeEvent as EdgeChange);

            }
            else if (changeEvent is PropagatedEdgeAdded)
            {
                HandlePropagatedEdgeAdded(changeEvent as PropagatedEdgeAdded);
            }
            else if (changeEvent is PropagatedEdgeRemoved)
            {
                HandlePropagatedEdgeRemoved(changeEvent as PropagatedEdgeRemoved);
            }
            else if (changeEvent is MapsToEdgeAdded)
            {
                HandleMapsToEdgeAdded(changeEvent as MapsToEdgeAdded);
            }
            else if (changeEvent is MapsToEdgeRemoved)
            {
                HandleMapsToEdgeRemoved(changeEvent as MapsToEdgeRemoved);
            }
            else
            {
                Debug.LogErrorFormat("UNHANDLED CALLBACK: {0}\n", changeEvent.ToString());
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

            GameObject gameEdge = architectureEdges[edgeChange.edge.ID];

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
               
                switch (edgeChange.oldState)
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
            List<GameObject> nodes = new List<GameObject> { source, target};
            // FIXME
            //GraphRenderer graphRenderer;
            //ICollection<GameObject> edges = graphRenderer.EdgeLayout(nodes);
        }

        private void HandleMapsToEdgeAdded(MapsToEdgeAdded mapsToEdgeAdded)
        {
            Debug.Log(mapsToEdgeAdded.ToString());
        }

        private void HandleMapsToEdgeRemoved(MapsToEdgeRemoved mapsToEdgeRemoved)
        {
            Debug.Log(mapsToEdgeRemoved.ToString());
        }
    }
}