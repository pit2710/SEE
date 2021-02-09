﻿using UnityEngine;
using SEE.DataModel.DG;
using SEE.Game;
using System;
using SEE.GO;
using System.Collections.Generic;
using System.Linq;
using SEE.Utils;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Action to create a new node for a selected city.
    /// </summary>
    public class NewNodeAction : AbstractPlayerAction
    {
        /// <summary>
        /// Start() will register an anonymous delegate of type 
        /// <see cref="ActionState.OnStateChangedFn"/> on the event
        /// <see cref="ActionState.OnStateChanged"/> to be called upon every
        /// change of the action state, where the newly entered state will
        /// be passed as a parameter. The anonymous delegate will compare whether
        /// this state equals <see cref="ThisActionState"/> and if so, execute
        /// what needs to be done for this action here. If that parameter is
        /// different from <see cref="ThisActionState"/>, this action will
        /// put itself to sleep. 
        /// Thus, this action will be executed only if the new state is 
        /// <see cref="ThisActionState"/>.
        /// </summary>
        const ActionState.Type ThisActionState = ActionState.Type.NewNode;

        private SEECity city = null;
        /// <summary>
        /// The Code City in wich the node should be placed.
        /// </summary>
        public SEECity City { get => city; set => city = value; }

        /// <summary>
        /// The new GameObject which contains the new node.
        /// </summary>
        public GameObject GONode = null;
        
        private static bool isInnerNode = false;
        /// <summary>
        /// True if the node to be created is an inner node.
        /// </summary>
        public static bool IsInnerNode { get => isInnerNode; set => isInnerNode = value; }

        private static string nodeName;
        /// <summary>
        /// The name of a new node given in the input field.
        /// </summary>
        public static string Nodename { get => nodeName; set => nodeName = value; }

        private static string nodeType;
        /// <summary>
        /// The type of a new node given in the input field.
        /// </summary>
        public static string Nodetype { get => nodeType; set => nodeType = value; }

        private Color defaultHoverCityColor = Color.green;
        /// <summary>
        /// Colour the hovered city is dyed when hovered or selected, set green by default 
        /// </summary>
        public Color DefaultHoverCityColor { get => defaultHoverCityColor; set => defaultHoverCityColor = value; }

        private Color alternativeHoverCityColor = Color.black;
        /// <summary>
        /// Colour the hovered city is dyed to when hovered or selected in case the default colour is occupied, 
        /// set black by default
        /// </summary>
        public Color AlternativeHoverCityColor { get => alternativeHoverCityColor; set => alternativeHoverCityColor = value; }

        private String nodeID;
        /// <summary>
        /// The node id of the new node.
        /// </summary>
        public String NodeID { get => nodeID; set => nodeID = value; }

        /// <summary>
        /// A list of the hovered GameObjects are stored in. FIXME: Grammar is wrong.
        /// </summary>
        private List<GameObject> hoveredObjectList = new List<GameObject>();

        /// <summary>
        /// A list of the colors of the hovered GameObjects are stored in. FIXME: Grammar is wrong.
        /// </summary>
        private List<Color> listOfColors = new List<Color>();

        /// <summary>
        /// The median of the lossyscale of the graph's leaves.
        /// </summary>
        private Vector3 medianOfLeaves;

        /// <summary>
        /// The median of the lossyscale of the graph's inner nodes.
        /// </summary>
        private Vector3 medianOfInnerNodes;

        /// <summary>
        /// The colour of the new graph leaves, set red by default. 
        /// To be changed when the metrics are considered.
        /// </summary>
        private static readonly Color leafColor = Color.red;

        /// <summary>
        /// The colour of the new inner nodes, set white by default. 
        /// To be changed when the metrics are considered
        /// </summary>
        private static readonly Color innerNodeColor = Color.white;

        /// <summary>
        /// A list of roots stores at least the single root of a graph, more in the special case the 
        /// graph has more than one root.
        /// </summary>
        private IList<GameObject> listOfRoots = null;

        /// <summary>
        /// True, if allNodesOfScene() is called, else false.
        /// </summary>
        private bool nodesLoaded = false;

        /// <summary>
        /// The current hovered city, which has to be colored while selecting a city-process for creating a new node.
        /// </summary>
        private SEECity cityToDye = null;

        /// <summary>
        /// To use the hovered object later
        /// </summary>
        private GameObject rndObjectInCity;

        /// <summary>
        /// The position of the graphs root.
        /// </summary>
        public Vector3 rootPostion;

        private bool network = false;
        // FIXME: Comment is missing.
        public bool Network { get => network; set => network = value; }

        public enum ProgressState
        {
            NoCitySelected,
            CityIsSelected,
            WaitingForValues,
            CanvasIsClosed,
            ValuesAreGiven,
            AddingIsCanceled
        }

        private ProgressState progress = ProgressState.NoCitySelected;
        /// <summary>
        /// The specific state of the progress of adding a node to the implementation.
        /// </summary>
        public ProgressState Progress { get => progress; set => progress = value; }

        public void Start()
        {
            listOfRoots = new List<GameObject>();
            InitializeCanvasObject();
            // An anonymous delegate is registered for the event <see cref="ActionState.OnStateChanged"/>.
            // This delegate will be called from <see cref="ActionState"/> upon every
            // state changed where the passed parameter is the newly entered state.
            ActionState.OnStateChanged += (ActionState.Type newState) =>
            {
                // Is this our action state where we need to do something?
                if (newState == ThisActionState)
                {
                    // The monobehaviour is enabled and Update() will be called by Unity.
                    if (!network)
                    {
                        enabled = true;
                    }
                    InteractableObject.LocalAnyHoverIn += LocalAnyHoverIn;
                    InteractableObject.LocalAnyHoverOut += LocalAnyHoverOut;
                    if (!instantiated)
                    {
                        instantiated = true;
                    }
                }
                else
                {
                    if (!network)
                    {
                        // The monobehaviour is diabled and Update() no longer be called by Unity.
                        enabled = false;
                    }
                    canvasObject.GetComponent<CanvasGenerator>().DestroyAddNodeCanvas();
                    Undye();
                    instantiated = false;
                    InteractableObject.LocalAnyHoverIn -= LocalAnyHoverIn;
                    InteractableObject.LocalAnyHoverOut -= LocalAnyHoverOut;
                    hoveredObject = null;

                }
            };
            enabled = ActionState.Is(ThisActionState);
        }

        /// <summary>
        /// The update-method interacts in dependency of the progress-state. (sequencial series)
        /// NoCitySelected: Searching for a selected city, where a node can be added
        /// CityIsSelected: Opens a canvas-object where values for the node can be added
        /// WaitingForValues: This State is active while the canvas is open, but the button "AddNode" on it is not pushed
        /// CanvasIsClosed: Closes the canvas-object after extracting the values for the creation of a node. This state is reached by pushing the "AddNode"-Button
        /// ValuesAreGiven: Moves the node and waits for a mouseInput to place the node, if its inside of the previous chosen city
        /// AddingIsCanceled: Removes all attributes and states and resets the progress-state to noCitySelected
        /// 
        /// </summary>
        public void Update()
        {
            // FIXME: Is this comment correct? I can't see where the new node is created.
            // Removes the canvasObject and extracts the values from it to create a new node with 
            // these params.
            void RemoveCanvas()
            {
                CanvasGenerator canvas = canvasObject.GetComponent<CanvasGenerator>();
                canvasObject.GetComponent<AddingNodeCanvasAction>().GetNodeMetrics();
                canvas.DestroyAddNodeCanvas();
            }

            switch (Progress)
            {
                case ProgressState.NoCitySelected:
                    SelectCity();
                    if (city != null && !network)
                    {
                        Progress = ProgressState.CityIsSelected;
                    }
                    break;

                case ProgressState.CityIsSelected:
                    OpenDialog();
                    Progress = ProgressState.WaitingForValues;
                    break;

                case ProgressState.CanvasIsClosed:
                    RemoveCanvas();
                    Progress = ProgressState.ValuesAreGiven;
                    break;

                case ProgressState.ValuesAreGiven:
                    if (GONode == null)
                    {
                        NodeID = RandomStrings.Get();
                        NewNode();
                        new NewNodeNetAction(rndObjectInCity.name, isInnerNode, nodeID, GONode.transform.position, GONode.transform.lossyScale, "", false, true, false).Execute(null);
                        nodesLoaded = false;
                        GameNodeMover.MoveTo(GONode);
                        new NewNodeNetAction(rndObjectInCity.name, isInnerNode, NodeID, GONode.transform.position, GONode.transform.lossyScale, "", false, false, false).Execute(null);

                    }
                    else
                    {
                        GameNodeMover.MoveTo(GONode);
                        new NewNodeNetAction(rndObjectInCity.name, isInnerNode, nodeID, GONode.transform.position, GONode.transform.lossyScale, "", false, false, false).Execute(null);
                        if (Input.GetMouseButtonDown(0))
                        {
                            Place();
                        }
                    }
                    break;

                case ProgressState.AddingIsCanceled:
                    RemoveCanvas();
                    city = null;
                    Progress = ProgressState.NoCitySelected;
                    instantiated = false;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Selects the city with hovering. Sets the city object on click on a GameObject.
        /// While there is no city selected by mouse click, the rootList of the current hovered city will 
        /// be colored in a different color to support the selection process visually.
        /// </summary>
        private void SelectCity()
        {
            // The case the user hovers an object and has hovered objects before. The former colors 
            // of the specific objects are set again.
            if ((hoveredObject != null && hoveredObjectList.Count > 0
                && hoveredObject.gameObject.GetComponent<Renderer>().material.color != defaultHoverCityColor)
                || (hoveredObject == null && hoveredObjectList.Count > 0))
            {
                Undye();
            }
            if (hoveredObject != null)
            {
                if (!nodesLoaded)
                {
                    rndObjectInCity = hoveredObject;
                    SceneQueries.GetCodeCity(hoveredObject.transform)?.gameObject.TryGetComponent<SEECity>(out cityToDye);
                    GetNodesOfScene();
                }
                // FIXME: What will happen if cityToDye is undefined? Could that ever happen?
                foreach (GameObject go in listOfRoots)
                {
                    if (go.GetComponent<NodeRef>().Value.ItsGraph == cityToDye.LoadedGraph)
                    {
                        ChangeColor(go);
                    }
                }
            }
            nodesLoaded = false;
            if (hoveredObject != null && Input.GetMouseButtonDown(0))
            {
                //Gets the SEECity from the hoverdObject
                SceneQueries.GetCodeCity(hoveredObject.transform)?.gameObject.TryGetComponent<SEECity>(out city);

                foreach (GameObject root in listOfRoots)
                {
                    if (root.GetComponent<NodeRef>().Value.ItsGraph == city.LoadedGraph)
                    {
                        ChangeColor(root);
                    }
                }
            }
        }

        /// <summary>
        /// Opens a dialog-canvas where the user can insert some node-metrics. Therefore, a CanvasGenerator-script-component 
        /// will be added to this gameObject which contains the canvas as a gameObject-instance of a prefab.
        /// </summary>
        public void OpenDialog()
        {
            canvasObject.GetComponent<CanvasGenerator>().InstantiateAddingNodeCanvas();
        }

        /// <summary>
        /// Undyes the current color of the object, i.e., changes the color of to its original color.
        /// </summary>
        public void Undye()
        {
            int count = 0;
            foreach (GameObject GO in hoveredObjectList)
            {
                GO.gameObject.GetComponent<Renderer>().material.color = listOfColors.ElementAt(count);
                count++;
            }
            listOfColors.Clear();
            hoveredObjectList.Clear();
        }

        /// <summary>
        /// Dyes the hoveredObject either in the defaulthoverCitycolor or in the alternativeHoverCityColor in case
        /// the object is already dyed in that color.
        /// The colors are about to be set by the user itself in the inspector or via GUI.
        /// </summary>
        /// <param name="colorOfCity">the default-color of the city.</param>
        /// <param name="objectToDye"> the object which will get dyed</param>
        private void ChangeColor(GameObject objectToDye)
        {
            Color newCityColor = defaultHoverCityColor;

            if (!(hoveredObjectList.Contains(objectToDye)))
            {
                hoveredObjectList.Add(objectToDye);
            }
            listOfColors.Add(objectToDye.gameObject.GetComponent<Renderer>().material.color);
            objectToDye.gameObject.GetComponent<Renderer>().material.color = newCityColor;
        }

        /// <summary>
        /// Adds a node to the loadedGraph and creates its ID. Repeats the process in case the generated 
        /// ID is not unique.
        /// </summary>
        /// <param name="node">The node to add to the graph</param>
        private void AddNode(Node node)
        {
            try
            {
                city.LoadedGraph.AddNode(node);
            }
            catch (Exception)
            {
                node.ID = RandomStrings.Get();
                AddNode(node);
                return;
            }
        }

        /// <summary>
        /// Creates a new node. First, it is created with default-values which will be replaced by inputValues 
        /// by the user if they are given. Sets this node in the hierachy of the graph. 
        /// Its important to set the id, city and isInnnerNode first
        /// </summary>
        public void NewNode()
        {
            GameObject gameNode;
            System.Random rnd = new System.Random();
            Node node = new Node
            {
                // Set the attributes of the new node.
                ID = NodeID,
                SourceName = NodeID,
                Type = Graph.UnknownType
            };

            AddNode(node);
            //Redraw the node Graph
            city.LoadedGraph.FinalizeNodeHierarchy();

            if (isInnerNode)
            {
                gameNode = city.Renderer.NewInnerNode(node);
                GONode = gameNode;
                GONode.transform.localScale = medianOfInnerNodes;
                GONode.gameObject.GetComponent<Renderer>().material.color = innerNodeColor;
            }
            if (!isInnerNode)
            {
                gameNode = city.Renderer.NewLeafNode(node);
                GONode = gameNode;
                GONode.transform.localScale = medianOfLeaves;
                GONode.gameObject.GetComponent<Renderer>().material.color = leafColor;
            }

            GONode.transform.position = rootPostion;
            GONode.gameObject.GetComponent<Collider>().enabled = false;
            GameNodeMover.MoveTo(GONode);
        }

        /// <summary>
        /// Places a node and checks if the city is the preselected one. Before this node will be placed, 
        /// the default values will be replaced by the user's input, if it is given and they are not only 
        /// whitespaces. 
        /// </summary>
        private void Place()
        {
            Node node = GONode.GetComponent<NodeRef>().Value;

            if (nodeName.Trim().Length > 0)
            {
                node.SourceName = nodeName;
            }
            if (nodeType.Trim().Length > 0)
            {
                node.Type = nodeType;
            }

            // Is the currently hovered object is part of the preselected city?
            GameObject codeCity = SceneQueries.GetCodeCity(hoveredObject.transform)?.gameObject;
            if (codeCity != null && codeCity.TryGetComponent<AbstractSEECity>(out AbstractSEECity hoveredCity))
            {
                if (city.Equals(hoveredCity))
                {
                    GONode.gameObject.GetComponent<Collider>().enabled = true;
                    // FIXME: GameNodeMover is obsolete.
                    GameNodeMover.FinalizePosition(GONode, GONode.transform.position);
                    new EditNodeNetAction(node.SourceName, node.Type, GONode.name).Execute(null);
                    new NewNodeNetAction(rndObjectInCity.name, isInnerNode, NodeID, GONode.transform.position, GONode.transform.lossyScale, GONode.transform.parent.gameObject.name, true, false, false).Execute(null);
                }
                else
                {
                    new NewNodeNetAction(rndObjectInCity.name, isInnerNode, NodeID, GONode.transform.position, GONode.transform.lossyScale, "", true, false, true).Execute(null);
                    Destroy(GONode);
                }
                Progress = ProgressState.NoCitySelected;
                GONode = null;
                city = null;
                nodesLoaded = false;
                cityToDye = null;
            }
        }

        /// <summary>
        /// Gets all nodes of the scene and saves them in collections - seperated one for the graph's leaves
        /// and one for the graph's inner nodes.
        /// Furthermore the root or if there are more than one, the roots will be determined and stored as well 
        /// in a list. 
        /// Finally, the median of the lossyscale of all nodes will be calculated in order to 
        /// determine a default size, which can be use when creating new nodes, either leaves or inner nodes.
        /// </summary>
        public void GetNodesOfScene()
        {           
            ICollection<GameObject> allLeavesInScene = SceneQueries.AllGameNodesInScene(true, false);
            ICollection<GameObject> allInnerNodesInScene = SceneQueries.AllGameNodesInScene(false, true);

            List<Node> rootList = cityToDye.LoadedGraph.GetRoots();

            // It is nessecary to find the GameObjects that are containing the specific root nodes.
            listOfRoots = RootSearch(allInnerNodesInScene, allLeavesInScene, rootList);

            List<Vector3> innerNodeSize = ListOfLossyscale(allInnerNodesInScene);

            medianOfLeaves = Medians.Median(ListOfLossyscale(allLeavesInScene));
            medianOfInnerNodes = Medians.Median(innerNodeSize);

            // In the special case there are no inner nodes, the median of the graph's only leaf is set 
            // as a default value for any inner node that might be created.
            if (innerNodeSize.Count == 0)
            {
                medianOfInnerNodes = medianOfLeaves;
            }

            // if, for any reason, the calulated median vector is the null vector, we adjust the new node's 
            // lossyscale size by 40% of the norm vector.
            if (medianOfInnerNodes == new Vector3(0,0,0))
            {
                medianOfInnerNodes = new Vector3(0.4f, 0.4f, 0.4f);
            }

            nodesLoaded = true;
        }

        /// <summary>
        /// Iterates the list of gameobjects and adds the lossyscale of the given objects to a list. 
        /// </summary>
        /// <param name="pListOfGameObjects">A List of GameObjects</param>
        /// <returns>Returns a  vector list filled with the lossyscale of the param pListOfGameObject or 
        /// null in case the list is empty or the given object is null</returns>
        private List<Vector3> ListOfLossyscale(ICollection<GameObject> pListOfGameObjects)
        {
            if (pListOfGameObjects == null | pListOfGameObjects.Count == 0)
            {
                return null;
            }

            List<Vector3> lossyScaleList = new List<Vector3>();

            foreach (GameObject go in pListOfGameObjects)
            {
                //to specify if the specific node belongs to the specific graph. 
                if (go.GetComponent<NodeRef>().Value.ItsGraph == cityToDye.LoadedGraph)
                {
                    lossyScaleList.Add(go.transform.lossyScale);
                }
            }
            return lossyScaleList;
        }

        /// <summary>
        /// Search and compare all rootNode GameObjects in the given lists of Gameobjects. 
        /// Special cases for the root-search: 
        /// There is no node in the scene -> root = null
        /// There is just one node in the scene -> root is this node
        /// There is exactly one root in the graph
        /// There are multipleRoots in the graph.
        /// </summary>
        /// <param name="listofLeaves">A collection of all leaves in a loaded scene</param>
        /// <param name="listOfInnerNodes">A collection of all InnerNodes in a loaded scene</param>
        /// <param name="pListofRoots">A list of all root-nodes in a loaded scene</param>
        /// <returns>A list with all root-GameObjects in the loaded scene ; Postcondition : list might be null </returns>
        private IList<GameObject> RootSearch
            (ICollection<GameObject> listOfInnerNodes, 
             ICollection<GameObject> listofLeaves, 
             IList<Node> pListofRoots)
        {            
            if (listofLeaves.Count == 0 && listOfInnerNodes.Count == 0)
            {
                return null;
            }
            IList<GameObject> listOfRoots = new List<GameObject>();
            // Special case the graph only consists of one leaf, i.e. the root.
            if (listofLeaves.Count == 1 && listOfInnerNodes.Count == 0)
            {
                listOfRoots.Add(listofLeaves.ElementAt(0));
                return listOfRoots;
            }
          
            // There might be more than one root, so we have to compare each of them.
            foreach (Node root in pListofRoots)
            {
                Node rootOfCity = root;
                Node rootTmp;

                foreach (GameObject rootSearchItem in listOfInnerNodes)
                {
                    rootTmp = rootSearchItem.GetComponent<NodeRef>().Value;
                    if (rootTmp != null && rootTmp.IsRoot() && rootTmp == rootOfCity)
                    {
                        listOfRoots.Add(rootSearchItem);
                        rootPostion = rootSearchItem.transform.position;
                    }
                }                
            }

            return listOfRoots;
        }

        /// <summary>
        /// For Network Use Only, places the new node on all other clients. Additonaly it reenables the collider of the node after placement.
        /// </summary>
        /// <param name="position"> The position of the new node</param>
        /// <param name="parentID">The id of the new GameObject</param>
        /// <param name="scale">the size of the new GameObject</param>
        public void NetworkPlaceNode(Vector3 position, Vector3 scale, string parentID)
        {            
            GONode.SetScale(scale);
            // FIXME: GameNodeMover will be removed.
            GameNodeMover.NetworkFinalizeNodePosition(GONode, parentID, position);
            GONode.gameObject.GetComponent<Collider>().enabled = true;
            GONode = null;
        }

        /// <summary>
        /// Setter for the Static isInnerNode-variable
        /// </summary>
        /// <param name="isInnerNode"></param>
        public void SetIsInnerNode(bool isInnerNode)
        {
            IsInnerNode = isInnerNode;
        }
    }
}
