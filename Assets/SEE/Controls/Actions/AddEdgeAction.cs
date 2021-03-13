﻿using System;
using System.Collections.Generic;
using SEE.Game;
using SEE.GO;
using SEE.Utils;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Action to create an edge between two selected nodes.
    /// </summary>
    public class AddEdgeAction : AbstractPlayerAction
    {
        /// <summary>
        /// The source for an edge to be drawn.
        /// </summary>
        private GameObject from;

        /// <summary>
        /// The target of the edge to be drawn.
        /// </summary>
        private GameObject to;

        /// <summary>
        /// The Objects which are needed to create a new edge:
        /// The source, the target and the city where the edge will be attached to.
        /// </summary>
        private List<Tuple<GameObject, GameObject, SEECity>> edgesToBeDrawn = new List<Tuple<GameObject, GameObject, SEECity>>();

        /// <summary>
        /// All createdEdges by this action.
        /// </summary>
        private List<GameObject> createdEdges = new List<GameObject>();

        /// <summary>
        /// The names of the generated edges.
        /// </summary>
        private List<string> edgeNames = new List<string>();

        public override void Start()
        {
            InteractableObject.LocalAnyHoverIn += LocalAnyHoverIn;
            InteractableObject.LocalAnyHoverOut += LocalAnyHoverOut;
        }

        public override void Update()
        {
            // Assigning the game objects to be connected.
            // Checking whether the two game objects are not null and whether they are 
            // actually nodes.
            if (Input.GetMouseButtonDown(0) && hoveredObject != null)
            {
                Assert.IsTrue(hoveredObject.HasNodeRef());
                if (from == null)
                {
                    from = hoveredObject;
                }
                else if (to == null)
                {
                    to = hoveredObject;
                }
            }
            // Note: from == to may be possible.
            if (from != null && to != null)
            {
                Transform cityObject = SceneQueries.GetCodeCity(from.transform);
                if (cityObject != null)
                {
                    if (cityObject.TryGetComponent(out SEECity city))
                    {
                        try
                        {
                            GameObject addedEdge = city.Renderer.DrawEdge(from, to);
                            edgesToBeDrawn.Add(new Tuple<GameObject, GameObject, SEECity>(from, to, city));
                            createdEdges.Add(addedEdge);
                            new AddEdgeNetAction(from.name, to.name).Execute();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"The new edge from {from.name} to {to.name} could not be created: {e.Message}.\n");
                        }
                        from = null;
                        to = null;
                    }
                }
            }
            // Adding the key "F1" in order to delete the selected GameObjects.
            if (Input.GetKeyDown(KeyCode.F1))
            {
                from = null;
                to = null;
            }
        }

        /// <summary>
        /// Undoes this AddEdgeActíon
        /// </summary>
        public override void Undo()
        {
            DeleteAction deleteAction = new DeleteAction();
            foreach (GameObject edge in createdEdges)
            {
                deleteAction.DeleteSelectedObject(edge);
                edgeNames.Add(edge.name);
                Destroyer.DestroyGameObject(edge);
            }
        }

        /// <summary>
        /// Redoes this AddEdgeAction
        /// </summary>
        public override void Redo()
        {
            // Hint: The ID´s of the new edges are generated by a randomize function.
            // That means, that the redo currently creates new edges, which are not the same
            // as the previous created nodes, because their id´s are different. 
            // Question: Should this ID be overwritten by the previous ID? - it is not the same as the gameObject-name.
            createdEdges.Clear();
            for(int i = 0; i < edgesToBeDrawn.Count; i++)
            {
                Tuple<GameObject, GameObject, SEECity> edgeToBeDrawn = edgesToBeDrawn[i];
                GameObject redoneEdge = edgeToBeDrawn.Item3.Renderer.DrawEdge(edgeToBeDrawn.Item1, edgeToBeDrawn.Item2);
                redoneEdge.name = (edgeNames[i]);
                createdEdges.Add(redoneEdge);
            }
        }

        /// <summary>
        /// Returns a new instance of <see cref="AddEdgeAction"/>.
        /// </summary>
        /// <returns></returns>
        public static ReversibleAction CreateReversibleAction()
        {
            return new AddEdgeAction();
        }
    }
}