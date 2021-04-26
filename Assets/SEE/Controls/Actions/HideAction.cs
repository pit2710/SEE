﻿using SEE.DataModel;
using SEE.DataModel.DG;
using SEE.Game;
using SEE.GO;
using SEE.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Action to hide/show the currently selected game object (edge or node).
    /// </summary>
    internal class HideAction : AbstractPlayerAction
    {
        /// <summary>
        /// The currently selected object (a node or edge).
        /// </summary>
        private GameObject selectedObject;

        /// <summary>
        /// The list of currently selected objects.
        /// </summary>
        private HashSet<GameObject> selectedObjects;

        /// <summary>
        /// The list of currently hidden objects.
        /// </summary>
        private ISet<GameObject> hiddenObjects = new HashSet<GameObject>();

        /// <summary>
        /// The list of objects whose visibility was changed in recent undo (needed for redo).
        /// </summary>
        private ISet<GameObject> undoneList = new HashSet<GameObject>();

        /// <summary>
        /// Specifies whether all objects of selected city should be hidden.
        /// </summary>
        private Boolean hideAll;

        /// <summary>
        /// Specifies whether selected objects should be hidden.
        /// </summary>
        private Boolean hideSelected;

        /// <summary>
        /// Specifies whether unselected objects should be hidden.
        /// </summary>
        private Boolean hideUnselected;

        /// <summary>
        /// Specifies whether incoming edges of selected node (including connected nodes) should be hidden.
        /// </summary>
        private Boolean hideIncoming;

        /// <summary>
        /// Specifies whether outgoing edges of selected node (including connected nodes) should be hidden.
        /// </summary>
        private Boolean hideOutgoing;

        /// <summary>
        /// Specifies whether all edges of selected node (including connected nodes) should be hidden.
        /// </summary>
        private Boolean hideAllEdgesOfSelected;

        /// <summary>
        /// The code city to perform actions on, only necessary for 
        /// </summary>
        SEECity CodeCity;

        enum EdgeSelector
        {
            Incomming,
            Outgoing
        }


        /// <summary>
        /// Returns a new instance of <see cref="HideAction"/>.
        /// </summary>
        /// <returns>new instance</returns>
        public static ReversibleAction CreateReversibleAction()
        {
            return new HideAction();
        }

        /// <summary>
        /// Returns a new instance of <see cref="HideAction"/>.
        /// </summary>
        /// <returns>new instance</returns>
        public override ReversibleAction NewInstance()
        {
            return CreateReversibleAction();
        }

        // Start is called before the first frame update
        public override void Start()
        {
            base.Stop();
            selectedObjects = new HashSet<GameObject>();
            InteractableObject.LocalAnySelectIn += LocalAnySelectIn;
            InteractableObject.LocalAnySelectOut += LocalAnySelectOut;
        }

        public override void Stop()
        {
            base.Stop();
            selectedObjects = null;
            InteractableObject.LocalAnySelectIn -= LocalAnySelectIn;
            InteractableObject.LocalAnySelectOut -= LocalAnySelectOut;
        }

        // Update is called once per frame
        public override bool Update()
        {
            if (hideAll)
            {
                if (HideAll())
                {
                    hadAnEffect = true;
                    return true;
                }
            } else if (hideSelected)
            {
                if (HideSelected())
                {
                    hadAnEffect = true;
                    return true;
                }
            } else if (hideUnselected)
            {
                if (HideUnselected())
                {
                    hadAnEffect = true;
                    return true;
                }
            } else if (hideOutgoing)
            {
                if (HideOutgoingEdges())
                {
                    hadAnEffect = true;
                    return true;
                }
            } else if (hideIncoming)
            {
                if (HideIncommingEdges())
                {
                    hadAnEffect = true;
                    return true;
                }
            } else if (hideAllEdgesOfSelected)
            {
                if (HideAllConnectedEdges())
                {
                    hadAnEffect = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Hides all selected objects when the enter key is pressed.
        /// </summary>
        /// <returns> true if all selected objects could be successfully hidden </returns>
        private bool HideSelected()
        {
            if (selectedObjects != null && selectedObjects.Count > 0 && Input.GetKey(KeyCode.Return))
            {
                foreach (GameObject g in selectedObjects)
                {
                    Assert.IsTrue(g.HasNodeRef() || g.HasEdgeRef());
                    if (g.CompareTag(Tags.Edge))
                    {
                        HideEdge(g);
                    }
                    else if (g.CompareTag(Tags.Node))
                    {
                        HideNodeIncludingConnectedEdges(g);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Hides all unselected objects when the enter key is pressed.
        /// </summary>
        /// <returns> true if all unselected objects could be successfully hidden </returns>
        private bool HideUnselected()
        {
            if (selectedObjects != null && selectedObjects.Count > 0 && selectedObject != null && Input.GetKey(KeyCode.Return))
            {
                GameObject city = selectedObject;
                while (!city.CompareTag(Tags.CodeCity))
                {
                    city = city.transform.parent.gameObject;
                }

                List<GameObject> nodesEdges = GetAllChildrenRecursively(city.transform, new List<GameObject>());

                List<GameObject> unselectedObjects = new List<GameObject>();

                foreach (GameObject g in nodesEdges)
                {
                    if (!selectedObjects.Contains(g) && !g.name.Equals("implementation"))
                    {
                        unselectedObjects.Add(g);
                    }
                }

                foreach (GameObject g in unselectedObjects)
                {
                    Assert.IsTrue(g.HasNodeRef() || g.HasEdgeRef());
                    if (g.CompareTag(Tags.Edge))
                    {
                        HideEdge(g);
                    }
                    else if (g.CompareTag(Tags.Node))
                    {
                        HideNodeIncludingConnectedEdges(g);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Hides an edge.
        /// </summary>
        /// <param name="edge"> edge to hide </param>
        /// <returns> true if edge was hidden </returns>
        private bool HideEdge(GameObject edge)
        {
            bool rendered = false;
            if (edge.TryGetComponent(out Renderer renderer))
            {
                rendered = renderer.enabled;
            }
            if (rendered)
            {
                hiddenObjects.Add(edge);
                GameObjectExtensions.SetVisibility(edge, false, true);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Hides a node including all the connected edges.
        /// </summary>
        /// <param name="node"> Node to hide </param>
        /// <returns> true if node was hidden successfully </returns>
        private bool HideNodeIncludingConnectedEdges(GameObject node)
        {
            if (node.TryGetComponent(out NodeRef nodeRef))
            {
                HashSet<String> edgeIDs = GetEdgeIds(nodeRef);

                foreach (GameObject edge in GameObject.FindGameObjectsWithTag(Tags.Edge))
                {
                    bool rendered = false;
                    if (edge.TryGetComponent(out Renderer renderer))
                    {
                        rendered = renderer.enabled;
                    }
                    if (edge.activeInHierarchy && edgeIDs.Contains(edge.name) && rendered)
                    {
                        hiddenObjects.Add(edge);
                        GameObjectExtensions.SetVisibility(edge, false, true);
                    }
                }
            }
            hiddenObjects.Add(node);
            GameObjectExtensions.SetVisibility(node, false, true);
            return true;
        }

        /// <summary>
        /// Hides all nodes and edges of the selected city.
        /// </summary>
        /// <returns> true if all nodes and edges could be successfully hidden </returns>
        private bool HideAll()
        {
            GameObject city = selectedObject;
            while (!city.CompareTag(Tags.CodeCity))
            {
                city = city.transform.parent.gameObject;
            }

            List<GameObject> nodesEdges = GetAllChildrenRecursively(city.transform, new List<GameObject>());
            
            foreach (GameObject g in nodesEdges)
            {
                GameObjectExtensions.SetVisibility(g, false, true);
                hiddenObjects.Add(g);
            }
            return true;
        }

        /// <summary>
        /// Recursive function to get all node and edge children of a game object.
        /// </summary>
        /// <param name="transform"> Transform of the game object </param>
        /// <param name="objectList"> Current list of all node and edge children </param>
        /// <returns> list of all node and edge children of a game object </returns>
        private List<GameObject> GetAllChildrenRecursively(Transform transform, List<GameObject> objectList)
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag(Tags.Node) || child.CompareTag(Tags.Edge))
                {
                    objectList.Add(child.gameObject);
                }
                if(child.childCount > 0)
                {
                    GetAllChildrenRecursively(child, objectList);
                }
            }
            return objectList;
        }

        /// <summary>
        /// Hides outgoing edges of currently selected node including the connected nodes.
        /// </summary>
        /// <returns> true if outgoing edges of currently selected node including the connected nodes were hidden </returns>
        private bool HideOutgoingEdges()
        {
            if (selectedObject != null && selectedObject.TryGetComponent(out NodeRef nodeRef))
            {

                HashSet<String> edgeIDs = new HashSet<string>();
                HashSet<String> nodeIDs = new HashSet<string>();

                foreach (Edge edge in nodeRef.Value.Outgoings)
                {
                    edgeIDs.Add(edge.ID);
                    nodeIDs.Add(edge.Target.ID);
                }

                foreach (GameObject edge in GameObject.FindGameObjectsWithTag(Tags.Edge))
                {
                            
                    if (edge.activeInHierarchy && edgeIDs.Contains(edge.name))
                    {
                        hiddenObjects.Add(edge);
                        GameObjectExtensions.SetVisibility(edge, false, true);
                    }
                }
                foreach (GameObject node in GameObject.FindGameObjectsWithTag(Tags.Node))
                {

                    if (node.activeInHierarchy && nodeIDs.Contains(node.name))
                    {
                        HideNodeIncludingConnectedEdges(node);
                    }
                }
                return true;
                }
            return false;   
        }

        /// <summary>
        /// Hides incoming edges of currently selected node including the connected nodes.
        /// </summary>
        /// <returns> true if incoming edges of currently selected node including the connected nodes were hidden</returns>
        private bool HideIncommingEdges()
        {
            if (selectedObject != null && selectedObject.TryGetComponent(out NodeRef nodeRef))
            {

                HashSet<String> edgeIDs = new HashSet<string>();
                HashSet<String> nodeIDs = new HashSet<string>();

                foreach (Edge edge in nodeRef.Value.Incomings)
                {
                    edgeIDs.Add(edge.ID);
                    nodeIDs.Add(edge.Source.ID);

                }

                foreach (GameObject edge in GameObject.FindGameObjectsWithTag(Tags.Edge))
                {
                            
                    if (edge.activeInHierarchy && edgeIDs.Contains(edge.name))
                    {
                        hiddenObjects.Add(edge);
                        GameObjectExtensions.SetVisibility(edge, false, true);
                    }
                }
                foreach (GameObject node in GameObject.FindGameObjectsWithTag(Tags.Node))
                {

                    if (node.activeInHierarchy && nodeIDs.Contains(node.name))
                    {
                        HideNodeIncludingConnectedEdges(node);
                    }
                }
                return true;
                }
            return false;         
        }

        /// <summary>
        /// Hides incoming edges of currently selected node including the connected nodes.
        /// </summary>
        /// <returns> true if incoming edges of currently selected node including the connected nodes were hidden </returns>
        private bool HideAllConnectedEdges()
        {
            return HideIncommingEdges() && HideOutgoingEdges();
        }


        /// <summary>
        /// Undoes the action.
        /// </summary>
        public override void Undo()
        {
            base.Undo();
            foreach (GameObject g in hiddenObjects)
            {
                GameObjectExtensions.SetVisibility(g, true, false);
                undoneList.Add(g);
            }
            hiddenObjects.Clear();
        }

        /// <summary>
        /// Redoes the action.
        /// </summary>
        public override void Redo()
        {
            base.Redo();
            foreach (GameObject g in undoneList)
            {
                GameObjectExtensions.SetVisibility(g, false, false);
                hiddenObjects.Add(g);
            }
            undoneList.Clear();
        }

        /// <summary>
        /// Selects source and target node of edge.
        /// </summary>
        /// <param name="edge"> edge to select source and target node of </param>
        private void SelectSourceAndTargetOfEdge(GameObject edge)
        {
            if(edge.TryGetComponent(out EdgeRef edgeRef))
            {
                string sourceID = edgeRef.edge.Source.ID;
                string targetID = edgeRef.edge.Target.ID;

                foreach (GameObject node in GameObject.FindGameObjectsWithTag(Tags.Node))
                {
                    if (node.name.Equals(sourceID))
                    {
                        if (node.TryGetComponent(out InteractableObject interactable))
                        {
                            interactable.SetSelect(true, true);
                        }
                    }
                    else if (node.name.Equals(targetID))
                    {
                        if (node.TryGetComponent(out InteractableObject interactable))
                        {
                            interactable.SetSelect(true, true);
                        }
                    }
                }
            }
        }


        private void SelectEdgesBetweenSubsetOfNodes(HashSet<GameObject> subset)
        { 
            if(subset != null && subset.Count > 0)
            {
                List<string> subsetNames = new List<string>();
                foreach (GameObject g in subset)
                {
                    subsetNames.Add(g.name);
                }
                foreach (GameObject edge in GameObject.FindGameObjectsWithTag(Tags.Edge))
                {
                    if (edge.TryGetComponent(out EdgeRef edgeRef))
                    {
                        if (subsetNames.Contains(edgeRef.edge.Source.ID) && subsetNames.Contains(edgeRef.edge.Target.ID))
                        {
                            if (edge.TryGetComponent(out InteractableObject interactable))
                            {
                                interactable.SetSelect(true, true);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the IDs of all incoming and outgoing edges for <paramref name="nodeRef"/>.
        /// </summary>
        /// <param name="nodeRef">node whose incoming and outgoing edges are requested</param>
        /// <returns>IDs of all incoming and outgoing edges</returns>
        private static HashSet<string> GetEdgeIds(NodeRef nodeRef)
        {
            HashSet<String> edgeIDs = new HashSet<string>();
            foreach (Edge edge in nodeRef.Value.Outgoings)
            {
                edgeIDs.Add(edge.ID);
            }
            foreach (Edge edge in nodeRef.Value.Incomings)
            {
                edgeIDs.Add(edge.ID);
            }
            return edgeIDs;
        }

        private void LocalAnySelectIn(InteractableObject interactableObject)
        {
            if (interactableObject != null)
            {
                selectedObject = interactableObject.gameObject;
                selectedObjects.Add(interactableObject.gameObject);
            }
        }

        private void LocalAnySelectOut(InteractableObject interactableObject)
        {
            if (selectedObjects.Contains(interactableObject.gameObject))
            {
                selectedObject = null;
                selectedObjects.Remove(interactableObject.gameObject);
            }
        }

        /// <summary>
        /// Returns the <see cref="ActionStateType"/> of this action.
        /// </summary>
        /// <returns><see cref="ActionStateType.Hide"/></returns>
        public override ActionStateType GetActionStateType()
        {
            return ActionStateType.Hide;
        }

    }
}