using Assets.SEE.Game;
using SEE.Controls;
using SEE.Controls.Actions;
using SEE.DataModel.DG;
using SEE.Game;
using SEE.GO;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Net
{
    /// <summary>
    /// This class is responsible for deleting a node or edge via network from one client to all others and 
    /// to the server. 
    /// </summary>
    public class UndoDeleteNetAction : AbstractAction
    {
        // Note: All attributes are made public so that they will be serialized
        // for the network transfer.

        /// <summary>
        /// The unique name of the gameObject of a node or edge that needs to be deleted.
        /// </summary>
        public string GameObjectID;

        public GameObject garbageCan;

        public String parentID;

        public Graph graph; 

        /// <summary>
        /// Creates a new DeleteNetAction.
        /// </summary>
        /// <param name="gameObjectID">the unique name of the gameObject of a node or edge 
        /// that has to be deleted</param>
        public UndoDeleteNetAction(string gameObjectID,  String parentID) : base()
        {
            this.GameObjectID = gameObjectID;
            garbageCan = GameObject.Find("garbageCan");
            this.parentID = parentID;
        }

        /// <summary>
        /// Things to execute on the server (none for this class). Necessary because it is abstract
        /// in the superclass.
        /// </summary>
        protected override void ExecuteOnServer()
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Deletes the game object identified by <see cref="GameObjectID"/> on each client.
        /// </summary>
        protected override void ExecuteOnClient()
        {
            if (!IsRequester())
            {
                Debug.Log("run");
                GameObject gameObject = GameObject.Find(GameObjectID);
                GameObject rootNode =  GameObject.Find(parentID);


                Debug.Log(rootNode.name);

                Debug.Log(rootNode.ItsGraph());
                Debug.Log(rootNode.GetGraph());
               
                if (rootNode.HasNodeRef())
                {
                    graph = rootNode.ItsGraph();
                    Debug.Log(graph +  "graph");

                    // oder Graph �ber city holen 
                    //parentOfNode.ContainingCity()
                    // city.loaded graph
                }
                else if (rootNode.HasEdgeRef()) 
                {
                    graph = rootNode.GetGraph();
                    // oder Graph �ber city holen 
                    //parentOfNode.ContainingCity()
                    // city.loaded graph
                    Debug.Log(graph + " graphEdgeRef");
                }

                if (gameObject != null)
                {

                    if (gameObject.HasEdgeRef())
                    {


                        PlayerSettings.GetPlayerSettings().StartCoroutine(AnimationsOfDeletion.DelayEdges(gameObject));
                        if (gameObject.TryGetComponentOrLog(out EdgeRef edgeReference))
                        {
                            try
                            {
                                 graph.AddEdge(edgeReference.Value);
                            }
                            catch (Exception e)
                            {
                                Debug.Log("edgeReference canot be added");
                            }
                        }
                    }
                        if (gameObject.HasNodeRef())
                        {

                            List<GameObject> removeFromGarbage = new List<GameObject>();
                            removeFromGarbage.Add(gameObject);
                            PlayerSettings.GetPlayerSettings().StartCoroutine(AnimationsOfDeletion.RemoveNodeFromGarbage(new List<GameObject>(removeFromGarbage)));
                            Node node = gameObject.GetNode();
                            //GameNodeAdder.AddNodeToGraph(parentOfNode.GetNode(),gameObject.GetNode() );

                            Debug.Log(graph);
                            Debug.Log("adding node to graph");
                            graph.AddNode(node);
                            graph.FinalizeNodeHierarchy();
                            node.ItsGraph = graph;
                         
                            
                        }
                    }


                    else
                    {
                        throw new System.Exception($"There is no game object with the ID {GameObjectID}");
                    }
                }
            }
        }
    }
