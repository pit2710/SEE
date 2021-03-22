﻿using SEE.DataModel;
using SEE.DataModel.DG;
using SEE.Game;
using SEE.GO;
using SEE.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Action to delete the currently selected game object (edge or node).
    /// </summary>
    internal class DeleteAction : AbstractPlayerAction
    {
        /// <summary>
        /// Returns a new instance of <see cref="DeleteAction"/>.
        /// </summary>
        /// <returns>new instance</returns>
        public static ReversibleAction CreateReversibleAction()
        {
            return new DeleteAction();
        }

        /// <summary>
        /// Returns a new instance of <see cref="DeleteAction"/>.
        /// </summary>
        /// <returns>new instance</returns>
        public override ReversibleAction NewInstance()
        {
            return CreateReversibleAction();
        }

        /// <summary>
        /// The currently selected object (a node or edge) to be deleted.
        /// </summary>
        private GameObject selectedObject;

        /// <summary>
        /// The waiting time of the animation for moving a node into a garbage can from over the garbage can.
        /// </summary>
        private const float TimeToWait = 1f;

        /// <summary>
        /// The animation time of the animation of moving a node to the top of the garbage can.
        /// </summary>
        private const float TimeForAnimation = 1f;

        /// <summary>
        /// Contains all nodes and edges deleted as explicitly requested by the user.
        /// As a consequence of deleting a node, its ancestores along with their incoming and outgoing
        /// edges may be deleted implicitly, too. All of these are kept in <see cref="DeletedNodes"/>
        /// and <see cref="DeletedEdges"/>. Yet, if we need to redo a deletion, we need to remember
        /// the explicitly deleted objects.
        /// </summary>
        private ISet<GameObject> explicitlyDeletedNodesAndEdges = new HashSet<GameObject>();

        /// <summary>
        /// A history of all nodes and the graph where they were attached to, deleted by this action.
        /// </summary>
        private Dictionary<GameObject, Graph> DeletedNodes { get; set; } = new Dictionary<GameObject, Graph>();

        /// <summary>
        /// A history of the old positions of the nodes deleted by this action.
        /// </summary>
        private Dictionary<GameObject, Vector3> OldPositions = new Dictionary<GameObject, Vector3>();

        /// <summary>
        /// A history of all edges and the graph where they were attached to, deleted by this action.
        /// </summary>
        private Dictionary<GameObject, Graph> DeletedEdges { get; set; } = new Dictionary<GameObject, Graph>();

        /// <summary>
        /// The name of the garbage can gameObject.
        /// </summary>
        private const string GarbageCanName = "GarbageCan";

        /// <summary>
        /// The garbage can the deleted nodes will be moved to. It is the object named 
        /// <see cref="GarbageCanName"/>.
        /// </summary>
        private GameObject garbageCan;

        /// <summary>
        /// True, if the moving process of a node to the garbage can is running, else false.
        /// Avoids multiple calls of coroutine.
        /// </summary>
        private bool isRunning = false;

        public override void Awake()
        {
            garbageCan = GameObject.Find(GarbageCanName);
        }

        public override void Start()
        {
            base.Stop();
            Debug.Log("Start\n");
            InteractableObject.LocalAnySelectIn += LocalAnySelectIn;
            InteractableObject.LocalAnySelectOut += LocalAnySelectOut;
        }

        public override void Stop()
        {
            base.Stop();
            Debug.Log("Stop\n");
            InteractableObject.LocalAnySelectIn -= LocalAnySelectIn;
            InteractableObject.LocalAnySelectOut -= LocalAnySelectOut;
        }
        /// <summary>
        /// See <see cref="ReversibleAction.Update"/>.
        /// </summary>
        /// <returns>true if completed</returns>
        public override bool Update()
        {
            // Delete a gameobject and all its children and incoming and outgoing edges.
            if (selectedObject != null && !isRunning)
            {
                Assert.IsTrue(selectedObject.HasNodeRef() || selectedObject.HasEdgeRef());
                explicitlyDeletedNodesAndEdges.Add(selectedObject);
                DeleteSelectedObject(selectedObject);               
                DumpStatus();
                // the selected objects are deleted and this action is done now
                return true; 
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes given <paramref GameObject="selectedObject"/> assumed to be either an
        /// edge or node. If it represents a node, the incoming and outgoing edges and
        /// its ancestors will be removed, too. For the possibility of an undo, the deleted objects will be saved. 
        /// 
        /// Precondition: <paramref name="selectedObject"/> != null.
        /// </summary>
        /// <param GameObject="selectedObject">selected GameObject that along with its children should be removed</param>
        public void DeleteSelectedObject(GameObject selectedObject)
        {
            if (selectedObject.CompareTag(Tags.Edge))
            {
                DeleteEdge(selectedObject);
            }
            else if (selectedObject.CompareTag(Tags.Node))
            {
                if (selectedObject.GetNode().IsRoot())
                {
                    Debug.LogError("A root shall not be deleted.\n");
                }
                else
                {
                    // The selectedObject (a node) and its ancestors are not deleted immediately. Instead we
                    // will run an animation that moves them into a garbage bin. Only when they arrive there,
                    // we will actually delete them.
                    // FIXME: Shouldn't the edges be moved to the garbage bin, too?
                    PlayerSettings.GetPlayerSettings().StartCoroutine(this.MoveNodeToGarbage(selectedObject.AllAncestors()));
                }
            }
            // FIXME:(Thore) NetAction is no longer up to date
            new DeleteNetAction(selectedObject.name).Execute(null);
        }

        private void DumpStatus()
        {
            Debug.Log($"Explicitly deleted elements: {explicitlyDeletedNodesAndEdges.Count}.\n");
            foreach (GameObject deleted in explicitlyDeletedNodesAndEdges)
            {
                Debug.Log($"Explicitly deleted {deleted.name}\n");
            }
            Debug.Log($"Implicitly deleted nodes: {DeletedNodes.Count}.\n");
            Debug.Log($"Implicitly deleted edges: {DeletedEdges.Count}.\n");
        }

        /// <summary>
        /// Undoes this DeleteAction.
        /// </summary>
        public override void Undo()
        {
            // Re-add all nodes to their graphs.
            foreach (KeyValuePair<GameObject, Graph> nodeGraphPair in DeletedNodes)
            {
                if (nodeGraphPair.Key.TryGetComponentOrLog(out NodeRef nodeRef))
                {
                    if (!nodeGraphPair.Value.Contains(nodeRef.Value))
                    {
                        nodeGraphPair.Value.AddNode(nodeRef.Value);
                    }
                }
            }
            // Re-add all edges to their graphs.
            foreach (KeyValuePair<GameObject, Graph> edgeGraphPair in DeletedEdges)
            {
                if (edgeGraphPair.Key.TryGetComponentOrLog(out EdgeRef edgeReference))
                {
                    edgeGraphPair.Value.AddEdge(edgeReference.edge);
                    edgeGraphPair.Key.SetVisibility(true, false);
                }
            }            
            PlayerSettings.GetPlayerSettings().StartCoroutine(this.RemoveNodeFromGarbage(new List<GameObject>(DeletedNodes.Keys)));
        }

        /// <summary>
        /// Redoes this DeleteAction.
        /// </summary>
        public override void Redo()
        {
            foreach (GameObject gameObject in explicitlyDeletedNodesAndEdges)
            {
                DeleteSelectedObject(gameObject);
            }
        }

        /// <summary>
        /// Moves all nodes in <paramref name="deletedNodes"/> to the garbage can
        /// using an animation. When they finally arrive there, they will be 
        /// deleted. 
        /// 
        /// Assumption: <paramref name="deletedNodes"/> contains all nodes in a subtree
        /// of the game-node hierarchy. All of them represent graph nodes.
        /// </summary>
        /// <param name="deletedNodes">the deleted nodes which will be moved to the garbage can.</param>
        /// <returns>the waiting time between moving deleted nodes over the garbage can and then into the garbage can</returns>
        private IEnumerator MoveNodeToGarbage(IList<GameObject> deletedNodes)
        {
            isRunning = true;
            // We need to reset the portal of all all deletedNodes so that we can move
            // them to the garbage bin. Otherwise they will become invisible if they 
            // leave their portal.
            foreach (GameObject deletedNode in deletedNodes)
            {
                if (!DeletedNodes.ContainsKey(deletedNode))
                {
                    Portal.SetInfinitePortal(deletedNode);
                }
            }
            MarkAsDeleted(deletedNodes);
            foreach (GameObject deletedNode in deletedNodes)
            {
                Tweens.Move(deletedNode, new Vector3(garbageCan.transform.position.x, garbageCan.transform.position.y + 1.4f, garbageCan.transform.position.z), TimeForAnimation);
            }

            yield return new WaitForSeconds(TimeToWait);

            foreach (GameObject deletedNode in deletedNodes)
            {
                Tweens.Move(deletedNode, new Vector3(garbageCan.transform.position.x, garbageCan.transform.position.y, garbageCan.transform.position.z), TimeForAnimation);
            }

            yield return new WaitForSeconds(TimeToWait);
            isRunning = false;
            InteractableObject.UnselectAll(true);
        }

        /// <summary>
        /// Removes all given nodes from the garbage can and back into the city.
        /// </summary>
        /// <param name="deletedNode">The nodes to be removed from the garbage-can</param>
        /// <returns>the waiting time between moving deleted nodes from the garbage-can and then to the city</returns>
        private IEnumerator RemoveNodeFromGarbage(IList<GameObject> deletedNodes)
        {
            isRunning = true;
            // up, out of the garbage can
            foreach (GameObject deletedNode in deletedNodes)
            {
                Tweens.Move(deletedNode, new Vector3(garbageCan.transform.position.x, garbageCan.transform.position.y + 1.4f, garbageCan.transform.position.z), TimeForAnimation);
            }

            yield return new WaitForSeconds(TimeToWait);

            // back to the original position
            foreach (GameObject node in deletedNodes)
            {
                Tweens.Move(node, OldPositions[node], TimeForAnimation);
            }

            yield return new WaitForSeconds(TimeToWait);
            OldPositions.Clear();
            DeletedNodes.Clear();
            DeletedEdges.Clear();
            isRunning = false;
            InteractableObject.UnselectAll(true);
        }

        /// <summary>
        /// Marks the given <paramref name="gameNodesToDelete"/> as deleted, i.e.,
        /// 1) removes the associated nodes represented by thos <paramref name="gameNodesToDelete"/> 
        ///    from their graph
        /// 2) removes the incoming and outgoing edges of <paramref name="gameNodesToDelete"/>
        ///    from their graph and makes those invisible
        /// 
        /// Assumption: <paramref name="gameNodesToDelete"/> contains all nodes in a subtree
        /// of the game-node hierarchy. All of them represent graph nodes.
        /// </summary>
        /// <param name="gameNodesToDelete">all deleted objects of the last operation</param>
        private void MarkAsDeleted(IList<GameObject> gameNodesToDelete)
        {           
            ISet<GameObject> edgesInScene = new HashSet<GameObject>(GameObject.FindGameObjectsWithTag(Tags.Edge));

            // First identify all incoming and outgoing edges for all nodes in gameNodesToDelete
            HashSet<GameObject> implicitlyDeletedEdges = new HashSet<GameObject>();
            foreach (GameObject deletedGameNode in gameNodesToDelete)
            {
                if (deletedGameNode.TryGetComponentOrLog(out NodeRef nodeRef))
                {
                    ISet<string> attachedEdges = nodeRef.GetEdgeIds();

                    foreach (GameObject edge in edgesInScene)
                    {
                        if (edge.activeInHierarchy && attachedEdges.Contains(edge.name))
                        {
                            // We will not immediately delete this edge here, because it may be an
                            // edge inbetween two nodes both contained in gameNodesToDelete, in which
                            // case it will show up as an incoming and outgoing edge.
                            implicitlyDeletedEdges.Add(edge);
                        }
                    }                    
                }
            }

            // Now delete the incoming and outgoing edges.
            foreach (GameObject implicitlyDeletedEdge in implicitlyDeletedEdges)
            {
                DeleteEdge(implicitlyDeletedEdge);
            }

            // Finally, we remove the nodes themselves.
            foreach (GameObject deletedGameNode in gameNodesToDelete)
            {
                DeleteNode(deletedGameNode);
            }
            selectedObject = null;
        }

        /// <summary>
        /// Deletes the given <paramref name="gameNode"/>, that is, it will remove
        /// the associated Node it from its graph. The <paramref name="gameNode"/>
        /// itself is not deleted or made invisible (because it will be needed during 
        /// the animation).
        /// 
        /// Precondition: <paramref name="gameNode"/> must have an <see cref="NodeRef"/>
        /// attached to it.
        /// </summary>
        /// <param name="gameNode">a game object representing an edge</param>
        private void DeleteNode(GameObject gameNode)
        {
            if (gameNode.TryGetComponentOrLog(out NodeRef nodeRef))
            {
                OldPositions[gameNode] = gameNode.transform.position;
                Graph graph = nodeRef.Value.ItsGraph;
                DeletedNodes[gameNode] = graph;
                graph.RemoveNode(nodeRef.Value);
            }
        }

        /// <summary>
        /// Deletes the given <paramref name="gameEdge"/>, that is, it will make it
        /// invisible and remove it from its graph.
        /// 
        /// Precondition: <paramref name="gameEdge"/> must have an <see cref="EdgeRef"/>
        /// attached to it.
        /// </summary>
        /// <param name="gameEdge">a game object representing an edge</param>
        private void DeleteEdge(GameObject gameEdge)
        {
            if (gameEdge.TryGetComponentOrLog(out EdgeRef edgeRef))
            {
                gameEdge.SetVisibility(false, true);
                Graph graph = edgeRef.edge.ItsGraph;
                DeletedEdges[gameEdge] = graph;
                graph.RemoveEdge(edgeRef.edge);
            }
        }

        private void LocalAnySelectIn(InteractableObject interactableObject)
        {
            // FIXME: For an unknown reason, the mouse events in InteractableObject will be
            // triggered twice per frame, which causes this method to be called twice.
            // We need to further investigate this issue.
            // Assert.IsNull(selectedObject);
            selectedObject = interactableObject.gameObject;
        }

        private void LocalAnySelectOut(InteractableObject interactableObject)
        {
            // FIXME: For an unknown reason, the mouse events in InteractableObject will be
            // triggered twice per frame, which causes this method to be called twice.
            // We need to further investigate this issue.
            // Assert.IsTrue(selectedObject == interactableObject.gameObject);
            selectedObject = null;
        }
    }
}
