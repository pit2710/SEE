﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SEE.DataModel.DG;
using SEE.Game;
using System;
using SEE.Controls.Actions;

namespace SEE.Controls {

    public  class DesktopNewNodeAction : MonoBehaviour
    {
        /// <summary>
        /// The Code City in wich the Node should be Placed
        /// </summary>
        SEECity city = null;

        /// <summary>
        /// The New GameObject
        /// </summary>
        GameObject GONode = null;

        /// <summary>
        /// Set by the GUI if a Inner Node should be Created
        /// </summary>
        bool is_innerNode = false;

        /// <summary>
        /// The Meta infos from the new Node, seted by the GUI
        /// 1. ID, 2. SourceName, 3. Type
        /// </summary>
        Tuple<String, String, String> nodeMetrics = null;

        /// <summary>
        /// The Object that the Cursor hovers over
        /// </summary>
        public GameObject hoveredObject = null;

        public void Start()
        {
            
        }

        public void Update()
        {
            if (city == null)
            {
                //City Selection
                selectCity();
            }
            else
            { 
                //Gets the Metrics for the new Node if no
                if(nodeMetrics == null)
                {
                    getMetrics();
                } else
                {
                    //Creates the new node, important check if the metrics have been set before!
                    if (GONode == null)
                    {
                        NewNode();
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            if (!Place())
                            {
                                Destroy(GONode);
                            }
                            GONode = null;
                            city = null;
                            nodeMetrics = null;
                        }
                        else
                        {
                            GameNodeMover.MoveTo(GONode);
                        }
                    }
                }

                
                
            }
        }

        /// <summary>
        /// Selects the City with hovering. Sets the City Object on Click on a GameObject
        /// </summary>
        private void selectCity()
        {
          if(hoveredObject != null &&Input.GetMouseButtonDown(0))
            {
                //Gets the SEECity from the hoverdObject
                SceneQueries.GetCodeCity(hoveredObject.transform)?.gameObject.TryGetComponent<SEECity>(out city);

                
            }
        }
        
        /// <summary>
        /// Sets the Metrics from the GUI
        /// </summary>
        private void getMetrics()
        {
            //FIXME WITH GUI THE NODE ID MUST BE UNIQUE SO MAYBE YOU NEED TO CHECK THE GUI ENTRY AND THE ALREDY EXISTING NODES
            System.Random rnd = new System.Random();
            //YOU CANT MODIFY THE VALUES OF A TUPLE, SO YOU NEED TO CREATE A NEW ONE IF YOU WANT TO MODIFY
            nodeMetrics =new Tuple<string, string, string>( "TEST-NODE" + rnd.Next(0, 999999999), "TEST-NODE" + rnd.Next(0, 999999999),"TEST NODE");

        }


        /// <summary>
        /// Creates a New Node
        /// </summary>
        /// <returns>New Node as GameObject</returns>
        private  void NewNode()
        { 
            GameObject gameNode;
            Node node = new Node();

            //Set the metrics of the new node
            node.ID = nodeMetrics.Item1;
            node.SourceName = nodeMetrics.Item2;
            node.Type = nodeMetrics.Item3;
            
            //Ads the new Node to the City Graph
            city.LoadedGraph.AddNode(node);

            //Redraw the node Graph
            city.LoadedGraph.FinalizeNodeHierarchy();
            
            //gets the renderer
            GraphRenderer graphRenderer = city.Renderer;

            if (is_innerNode)
            {
                gameNode = graphRenderer.NewInnerNode(node); 
            }
            else
            {
                gameNode = graphRenderer.NewLeafNode(node);
            }

            //Sets the The GONode so the main work can continue;
            GONode =  gameNode;
        }

       

        /// <summary>
        /// Places a node on call and checks if the city is the preselected one
        /// </summary>
        /// <returns> True if the Action performed well | false if the object should be destroyed </returns>
        private bool Place()
        {
            SEECity cityTmp = null;
            if (hoveredObject != null)
            {

                //checks if the currently hovered object is part of the preselected city
                GameObject tmp = SceneQueries.GetCodeCity(hoveredObject.transform)?.gameObject;
                tmp.TryGetComponent<SEECity>(out cityTmp);
                if (city.Equals(cityTmp))
                {
                    GameNodeMover.FinalizePosition(GONode);
                    city.LoadedGraph.FinalizeNodeHierarchy();
                    return true;
                }
                
            }
            return false;
        }
        
    }
}
