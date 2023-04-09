﻿using DG.Tweening;
using SEE.DataModel.DG;
using SEE.Game.Operator;
using SEE.GO;
using SEE.Layout;
using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Game.Evolution
{
    /// <summary>
    /// Part of the <see cref="EvolutionRenderer"/> taking care of moving
    /// graph elements existing in the currently shown graph and the next
    /// one to their new position.
    /// The code following here implements phase (2).
    /// </summary>
    public partial class EvolutionRenderer
    {
        /// <summary>
        /// Implements the second phase in the transition from the <see cref="currentCity"/>
        /// to the <paramref name="nextCity"/>.
        /// In this phase, all existing nodes (nodes in both graphs, no matter whether they were
        /// changed or not) will be moved to their new location. When this phase has been
        /// completed, <see cref="Phase3AdjustExistingGraphElements"/> will be called eventually.
        ///
        /// Note: <paramref name="next"/> will be a graph for the previous revision of
        /// the currently drawn graph in the graph series when the evolution visualization
        /// is played backward.
        /// </summary>
        /// <param name="next">the next graph to be drawn</param>
        private void Phase2MoveExistingGraphElements(LaidOutGraph next)
        {
            /// We need to assign nextCity because the callbacks <see cref="RenderPlane"/>
            /// and <see cref="RenderExistingNode(Node)"/> will access it.
            nextCity = next;

            int existingElements = equalNodes.Count + changedNodes.Count + equalEdges.Count + changedEdges.Count;
            Debug.Log($"Phase2: Moving {existingElements} existing graph elements.\n");
            animationWatchDog.Await(existingElements, Phase3AdjustExistingGraphElements);
            if (existingElements > 0)
            {
                ISet<Edge> equalAndChangedEdges = new HashSet<Edge>(equalEdges);
                equalAndChangedEdges.UnionWith(changedEdges);
                CreateEdges(equalAndChangedEdges);
                SetUpEdgeAnimation(equalAndChangedEdges);

                equalNodes.ForEach(RenderExistingNode);
                changedNodes.ForEach(RenderExistingNode);
            }

            // Creates the game edges for all given graph edges and apply their layout.
            void CreateEdges(ISet<Edge> edges)
            {
                foreach (Edge edge in edges)
                {
                    objectManager.GetEdge(edge, out GameObject edgeObject);
                    if (edgeObject.TryGetComponent(out SEESpline spline))
                    {
                        spline.Spline = next.EdgeLayout[edge.ID].Spline;
                    }
                }
            }

            // Sets up the movement animation (edge morphism) of all given edges.
            void SetUpEdgeAnimation(ISet<Edge> edges)
            {
                // Create (or read from cache) the edge objects of the next
                // visible graph, update their spline, and make the objects
                // visible.
                if (currentCity != null)
                {
                    // We are transitioning from a previous graph, i.e., next is not
                    // the inital graph in the graph series.
                    foreach (Edge edge in edges)
                    {
                        if (!next.EdgeLayout.TryGetValue(edge.ID, out ILayoutEdge<ILayoutNode> newLayoutEdge))
                        {
                            Debug.LogWarning($"Missing layout for graph edge with id '{edge.ID}'; skipping it.\n");
                            continue;
                        }
                        if (currentCity.EdgeLayout.TryGetValue(edge.ID, out ILayoutEdge<ILayoutNode> oldLayoutEdge))
                        {
                            objectManager.GetEdge(edge, out GameObject gameEdge);
                            gameEdge.AddOrGetComponent<SplineMorphism>()
                                    .CreateTween(oldLayoutEdge.Spline, newLayoutEdge.Spline, AnimationLagPerPhase())
                                    .OnComplete(() => animationWatchDog.Finished()).Play();
                        }
                    }
                }
                else
                {
                    Debug.Log("No previous graph.\n");
                }
            }
        }

        /// <summary>
        /// Moves the game node corresponding to <paramref name="graphNode"/> (a node that exists
        /// in the current and next graph) to its new location according to <see cref="NextLayoutToBeShown"/>.
        /// </summary>
        /// <param name="graphNode">graph node whose corresponding game node is to be moved</param>
        private void RenderExistingNode(Node graphNode)
        {
            Assert.IsNotNull(graphNode);
            ILayoutNode layoutNode = NextLayoutToBeShown[graphNode.ID];
            // The game node representing the graphNode if there is any; null if there is none
            Node formerGraphNode = objectManager.GetNode(graphNode, out GameObject gameNode);
            Assert.IsTrue(gameNode.HasNodeRef());
            Assert.IsNotNull(formerGraphNode);

            // We want the animator to move each node separately, which is why we
            // remove each from the hierarchy; later the node hierarchy will be
            // re-established. It still needs to be a child of the code city,
            // however, because methods called in the course of the animation
            // will try to retrieve the code city from the game node.
            gameNode.transform.SetParent(gameObject.transform);
            MoveTo(gameNode, layoutNode);

            void MoveTo(GameObject gameNode, ILayoutNode layoutNode)
            {
                // currentGameNode is shifted to its new position through the animator.
                gameNode.AddOrGetComponent<NodeOperator>()
                         .MoveTo(layoutNode.CenterPosition, AnimationLagPerPhase(), updateEdges: false)
                         .SetOnComplete(animationWatchDog.Finished);
            }
        }

        /// <summary>
        /// Event function that adds the given <paramref name="gameNode"/>
        /// to <see cref="gameObject"/> as a child if <paramref name="gameNode"/>
        /// is a <see cref="GameObject"/> and has no parent yet. Informs
        /// <see cref="animationWatchDog"/> that this animation has finished.
        /// Called as a callback when the animation of new and existing
        /// nodes is finished; <see cref="RenderExistingNode(Node)"/>.
        /// </summary>
        /// <param name="gameNode">new or existing game object representing a graph node</param>
        private void OnAnimationNodeAnimationFinished(object gameNode)
        {
            if (gameNode is GameObject go)
            {
                graphRenderer.AdjustAntenna(go);
                markerFactory.AdjustMarkerY(go);

                if (go.transform.parent == null)
                {
                    /// We will just put this game object under <see cref="gameObject"/>
                    /// (the game object representing the city as a whole) as a child. When
                    /// the animation is over and all nodes have reached their destination,
                    /// <see cref="UpdateGameNodeHierarchy"/> will put this node to its
                    /// actual logical game-node parent.
                    go.transform.SetParent(gameObject.transform);
                }
            }
            animationWatchDog.Finished();
        }
    }
}
