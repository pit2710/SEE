﻿//Copyright 2020 Florian Garbade

//Permission is hereby granted, free of charge, to any person obtaining a
//copy of this software and associated documentation files (the "Software"),
//to deal in the Software without restriction, including without limitation
//the rights to use, copy, modify, merge, publish, distribute, sublicense,
//and/or sell copies of the Software, and to permit persons to whom the Software
//is furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
//PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
//USE OR OTHER DEALINGS IN THE SOFTWARE.

using SEE.DataModel;
using SEE.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace SEE.Animation.Internal
{
    /// <summary>
    /// Renders the evolution of the graph series through animations. Incrementally updates
    /// the graph (removal/addition of nodes/edges).
    /// 
    /// Note: The renderer is a MonoBehaviour, thus, will be added as a component to a game
    /// object. As a consequence, a constructor will not be called and is meaningless.
    /// </summary>
    public class EvolutionRenderer : MonoBehaviour
    {
        /// <summary>
        /// The city evolution to be drawn by this renderer.
        /// </summary>
        private SEECityEvolution city;

        /// <summary>
        /// The graph renderer used to draw a single graph and the later added nodes and edges.
        /// This attribute will be set in the setter of the attribute CityEvolution because it
        /// depends upon the city, which is set by this setter.
        /// </summary>
        private GraphRenderer graphRenderer;

        /// <summary>
        /// The manager of the game objects created for the city.         
        /// This attribute will be set in the setter of the attribute CityEvolution because it
        /// depends upon the graphRenderer, which in turn depends upon the city, which is set by 
        /// this setter.
        /// </summary>
        private AbstractObjectManager objectManager;

        /// <summary>
        /// The city evolution to be drawn by this renderer.
        /// </summary>
        public SEECityEvolution CityEvolution
        {
            get => city;
            set {
                city = value;
                // A constructor is meaningless for a class that derives from MonoBehaviour.
                // So we cannot make the following assignment in the constructor. Neither
                // can we assign this value at the declaration of graphRenderer because
                // we need the city argument, which comes only later. Anyhow, whenever we
                // assign a new city, we also need a new graph renderer for that city.
                // So in fact this is the perfect place to assign graphRenderer.
                graphRenderer = new GraphRenderer(city);
                objectManager = new ObjectManager(graphRenderer);
            }
        }

        /// <summary>
        /// Shortest time period in which an animation can be run.
        /// </summary>
        private float MinimalWaitTimeForNextRevision = 0.1f;

        /// <summary>
        /// An event fired upon the start of an animation.
        /// </summary>
        public readonly UnityEvent AnimationStartedEvent = new UnityEvent();

        /// <summary>
        /// An event fired upon the end of an animation.
        /// </summary>
        public readonly UnityEvent AnimationFinishedEvent = new UnityEvent();

        /// <summary>
        /// A SimpleAnimator used for animation.
        /// </summary>
        protected readonly AbstractAnimator SimpleAnim = new SimpleAnimator();

        /// <summary>
        /// A MoveAnimator used for move animations.
        /// </summary>
        protected readonly AbstractAnimator MoveAnim = new MoveAnimator();

        /// <summary>
        /// Whether the animation is still ongoing.
        /// </summary>
        private bool _isStillAnimating = false;

        /// <summary>
        /// True if animation is still ongoing.
        /// </summary>
        public bool IsStillAnimating { get => _isStillAnimating; set => _isStillAnimating = value; }

        /// <summary>
        /// The collection of registered <see cref="AbstractAnimator"/> to be updated
        /// automatically for changes during the animation time period.
        /// </summary>
        private readonly List<AbstractAnimator> animators = new List<AbstractAnimator>();

        private float _animationTime = AbstractAnimator.DefaultAnimationTime;

        /// <summary>
        /// Maximal time of the lifetime of animation after they started.
        /// </summary>
        public float AnimationTime
        {
            get => _animationTime;
            set
            {
                if (value >= 0)
                {
                    _animationTime = value;
                    animators.ForEach(animator =>
                    {
                        animator.MaxAnimationTime = value;
                        animator.AnimationsDisabled = value == 0;
                    });
                }
            }
        }

        /// <summary>
        /// The city (graph + layout) currently shown.
        /// </summary>
        private LaidOutGraph _currentCity;
        /// <summary>
        /// The underlying graph of the city currently shown.
        /// </summary>
        protected Graph CurrentGraphShown => _currentCity?.Graph;
        /// <summary>
        /// The layout of the city currently shown.
        /// </summary>
        protected Dictionary<GameObject, NodeTransform> CurrentLayoutShown => _currentCity?.Layout;

        /// <summary>
        /// The city (graph + layout) to be shown next.
        /// </summary>
        private LaidOutGraph _nextCity;

        /// <summary>
        /// The next city (graph + layout) to be shown. 
        /// Note: 'next' does not necessarily mean that it is a graph coming later in the
        /// series of the graph evolution. It just means that this is the next graph to
        /// be shown. If the user goes backward in time, _nextCity is actually an older
        /// graph.
        /// </summary>
        protected Graph NextGraphToBeShown => _nextCity?.Graph;
        /// <summary>
        /// The layout of _nextGraph.
        /// </summary>
        protected Dictionary<GameObject, NodeTransform> NextLayoutToBeShown => _nextCity?.Layout;

        /// <summary>
        /// Allows the comparison of two instances of <see cref="Node"/>.
        /// </summary>
        private readonly NodeEqualityComparer nodeEqualityComparer = new NodeEqualityComparer();

        /// <summary>
        /// Allows the comparison of two instances of <see cref="Edge"/>.
        /// </summary>
        private readonly EdgeEqualityComparer edgeEqualityComparer = new EdgeEqualityComparer();

        private AbstractObjectManager _objectManager;

        protected enum GraphDirection { First, Next, Previous };

        protected SEECityEvolution City => city;

        /// <summary>
        /// Constructor
        /// </summary>
        public EvolutionRenderer()
        {
            RegisterAllAnimators(animators);
        }

        private Dictionary<Graph, Dictionary<GameObject, NodeTransform>> Layouts { get; }
             =  new Dictionary<Graph, Dictionary<GameObject, NodeTransform>>();

        /// <summary>
        /// Calculates the layout data for <paramref name="graph"/> using the graphRenderer.
        /// All the game objects created for the nodes of <paramref name="graph"/> will
        /// be created by the objectManager, thus, be available for later use. The layout
        /// is not actually applied.
        /// </summary>
        /// <param name="graph">graph for which the layout is to be calculated</param>
        /// <returns>the node layout for all nodes in <paramref name="graph"/></returns>
        private Dictionary<GameObject, NodeTransform> CalculateLayout(Graph graph)
        {
            // The following code assumes that a leaf node remains a leaf across all
            // graphs of the graph series and an inner node remains an inner node.
            // This may not necessarily be true. For instance, a empty directory could 
            // get subdirectories in the course of the evolution.

            // Collecting all game objects corresponding to nodes of the given graph.
            // If node existed in a previous graph, we will re-use its corresponding
            // game object created earlier.
            var gameObjects = new List<GameObject>();

            // the layout to be applied
            NodeLayout nodeLayout = graphRenderer.GetLayout();

            // gather all nodes for the layout
            bool isHierarchicalLayout = nodeLayout.IsHierarchical();
            foreach (Node node in graph.Nodes())
            {
                if (isHierarchicalLayout || node.IsLeaf())
                {
                    // All layouts (flat and hierarchical ones) must be able to handle leaves; 
                    // hence, leaves can be added at any rate. For a hierarchical layout, we 
                    // need to add the game objects for inner nodes, too. To put it differently,
                    // inner nodes are added only if we apply a hierarchical layout.
                    objectManager.GetNode(node, out var gameNode);
                    gameObjects.Add(gameNode);
                }
            }

            // Calculate the layout for the game objects.
            return nodeLayout.Layout(gameObjects);

            // Note: The game objects for leaf nodes are already properly sized by the call to 
            // objectManager.GetNode() above. Yet, inner nodes are generally not sized by
            // the layout and there may be layouts that may shrinked leaf nodes. For instance,
            // TreeMap shrinks leaves so that they fit into the available space.
            // Anyhow, we do not need to apply the layout already now. That can be deferred
            // to the point in time when the city is actually visualized. Here, we just calculate
            // the layout for every graph in the graph series for later use.
        }

        /// <summary>
        /// Creates the layouts for all given <paramref name="graphs"/>. This will
        /// also create all necessary game objects -- even those game objects that are
        /// not present in the first graph in this list.
        /// </summary>
        public void CalculateGraphLayouts(List<Graph> graphs)
        {
            // Determine the layouts of all loaded graphs upfront.
            var p = Performance.Begin("Layouting all graphs");
            graphs.ForEach(graph =>
            {
                Layouts[graph] = CalculateLayout(graph);
            });
            p.End();
        }

        /// <summary>
        /// Displays the given graph instantly if all animations are finished; that is,
        /// no animation are run. The graph is drawn from scratch.
        /// </summary>
        /// <param name="graph">graph to be drawn initially</param>
        public void DisplayInitialGraph(LaidOutGraph graph)
        {
            graph.AssertNotNull("loadedGraph");

            if (IsStillAnimating)
            {
                Debug.LogWarning("Graph changes are blocked while animations are running.");
                return;
            }
            _currentCity = graph;
            _nextCity = graph; // FIXME: Not needed?
            RenderGraph(true);
        }

        /// <summary>
        /// Retrieves the pre-computed stored layout for given <paramref name="graph"/>
        /// in output parameter <paramref name="layout"/> if one can be found. If a
        /// layout was actually found, true is returned; otherwise false.
        /// </summary>
        /// <param name="graph">the graph for which to determine the layout</param>
        /// <param name="layout">the retrieved layout or null</param>
        /// <returns></returns>
        public bool TryGetLayout(Graph graph, out Dictionary<GameObject, NodeTransform> layout)
        {
            return Layouts.TryGetValue(graph, out layout);
        }

        /// <summary>
        /// Starts the animations to transition from the current to the next graph.
        /// </summary>
        /// <param name="current">the currently shown graph</param>
        /// <param name="next">the next graph to be shown</param>
        public void TransitionToNextGraph(LaidOutGraph current, LaidOutGraph next)
        {
            current.AssertNotNull("current");
            next.AssertNotNull("next");

            if (IsStillAnimating)
            {
                Debug.LogError("Graph changes are not allowed while animations are running.");
                return;
            }

            _currentCity = current;
            _nextCity = next;
            RenderGraph(false);
        }

        /// <summary>
        /// Renders the animation from CurrentGraphShown to NextGraphToBeShown if <paramref name="asNew"/>
        /// is false; otherwise the graph is drawn from scratch.
        /// </summary>
        /// <param name="asNew">if true, graph is drawn from scratch; otherwise the differences
        /// from CurrentGraphShown to NextGraphToBeShown are animated</param>
        private void RenderGraph(bool asNew)
        {
            if (asNew)
            {
                ClearGraphObjects();
                // We assume that the city is a component nested in a game object. 
                // This game object becomes the parent under which all game objects
                // created for the underlying graph are to be nested.
                GameObject root = city.gameObject;
                if (root == null)
                {
                    root = new GameObject();
                    root.name = "SEECityEvolution";
                }
                // TODO/FIXME: We are actually not using the layout that was already 
                // computed for the initial graph. Draw() will calculate the layout
                // from scratch again.
                graphRenderer.Draw(CurrentGraphShown, root);
            }
            else
            {
                IsStillAnimating = true;
                AnimationStartedEvent.Invoke();
                // For all nodes of the current graph not in the next graph; that is, all
                // nodes removed:
                CurrentGraphShown?
                    .Nodes().Except(NextGraphToBeShown.Nodes(), nodeEqualityComparer).ToList()
                    .ForEach(node =>
                    {
                        if (node.IsLeaf())
                        {
                            RenderRemovedOldLeaf(node);
                        }
                        else
                        {
                            RenderRemovedOldInnerNode(node);
                        }
                    });

                // For all edges of the current graph not in the next graph; that is, all
                // edges removed:
                CurrentGraphShown?
                    .Edges().Except(NextGraphToBeShown.Edges(), edgeEqualityComparer).ToList()
                    .ForEach(RenderRemovedOldEdge);

                // Draw all nodes of NextGraphToBeShown.
                NextGraphToBeShown.Traverse(RenderPlane, RenderInnerNode, RenderLeaf);
                // Draw all edges of NextGraphToBeShown.
                NextGraphToBeShown.Edges().ForEach(RenderEdge);
                Invoke("OnAnimationsFinished", Math.Max(AnimationTime, MinimalWaitTimeForNextRevision));
            }
        }

        /// <summary>
        /// Event function triggered when alls animations are finished.
        /// </summary>
        private void OnAnimationsFinished()
        {
            IsStillAnimating = false;
            AnimationFinishedEvent.Invoke();
        }

        /// <summary>
        /// Is called on Constructor the register all given animator,
        /// so they can be updated accordingly.
        /// </summary>
        /// <param name="animators"></param>
        protected virtual void RegisterAllAnimators(List<AbstractAnimator> animators)
        {
            animators.Add(SimpleAnim);
            animators.Add(MoveAnim);
        }

        /// <summary>
        /// Renders a plane enclosing all (transitive) descendant game objects of given
        /// <paramref name="node"/>.
        /// </summary>
        /// <param name="node">the node to be displayed</param>
        protected virtual void RenderPlane(Node node)
        {
            // FIXME: The root node is either a leaf or inner node.
            // GraphRenderer.NewPlane(),

            // FIXME. Code must be adjusted. Planes are not part of the layout.
            /*
            var isPlaneNew = !objectManager.GetPlane(out GameObject plane);
            var nodeTransform = NextLayoutToBeShown[node];
            if (isPlaneNew)
            {
                // if the plane is new instantly apply the position and size
                plane.transform.position = Vector3.zero;
                plane.transform.localScale = nodeTransform.scale;
            }
            else
            {
                // if the tranform of the plane changed animate it
                SimpleAnim.AnimateTo(node, plane, Vector3.zero, nodeTransform.scale);
            }
            */
        }

        /// <summary>
        /// Determines how an inner node that contains other nodes is displayed.
        /// </summary>
        /// <param name="node">node to be displayed</param>
        protected virtual void RenderInnerNode(Node node)
        {
            // FIXME: The form of inner nodes depends upon the user's choice
            // and possibly the kind of layout.

            // Currently, we have the following kinds of InnerNodeKinds:
            // Blocks, Rectangles, Donuts, Circles, Empty, Cylinders.

            var isCircleNew = !objectManager.GetInnerNode(node, out GameObject circle);
            var nodeTransform = NextLayoutToBeShown[circle];

            var circlePosition = nodeTransform.position;
            circlePosition.y = 0.5F;

            var circleRadius = nodeTransform.scale;
            circleRadius.x += 2;
            circleRadius.z += 2;

            if (isCircleNew)
            {
                // if the node is new, animate it by moving it out of the ground
                circlePosition.y = -3;
                circle.transform.position = circlePosition;
                circle.transform.localScale = circleRadius;

                circlePosition.y = 0.5F;
                SimpleAnim.AnimateTo(node, circle, circlePosition, circleRadius);
            }
            else if (node.WasModified())
            {
                SimpleAnim.AnimateTo(node, circle, circlePosition, circleRadius);
            }
            else if (node.WasRelocated(out string oldLinkageName))
            {
                SimpleAnim.AnimateTo(node, circle, circlePosition, circleRadius);
            }
            else
            {
                SimpleAnim.AnimateTo(node, circle, circlePosition, circleRadius);
            }
        }

        /// <summary>
        /// Renders a leaf node.
        /// </summary>
        /// <param name="node">leaf node to be rendered</param>
        protected virtual void RenderLeaf(Node node)
        {
            var isLeafNew = !objectManager.GetLeaf(node, out GameObject leaf);
            var nodeTransform = NextLayoutToBeShown[leaf];

            if (isLeafNew)
            {
                // if the leaf node is new, animate it by moving it out of the ground

                // FIXME: CScape buildings have a different notion of position than cubes.
                var newPosition = nodeTransform.position;
                newPosition.y = -nodeTransform.scale.y;
                leaf.transform.position = newPosition;
            }
            SimpleAnim.AnimateTo(node, leaf, nodeTransform.position, nodeTransform.scale);
        }

        /// <summary>
        /// Determines how an edge is displayed.
        /// </summary>
        /// <param name="edge"></param>
        protected virtual void RenderEdge(Edge edge)
        {
        }

        /// <summary>
        /// Removes the given inner node. The node is not auto destroyed.
        /// </summary>
        /// <param name="node">inner node to be removed</param>
        protected virtual void RenderRemovedOldInnerNode(Node node)
        {
            if (objectManager.RemoveNode(node, out GameObject gameObject))
            {
                // if the node needs to be removed, let it sink into the ground
                var nextPosition = gameObject.transform.position;
                nextPosition.y = -2;
                MoveAnim.AnimateTo(node, gameObject, nextPosition, gameObject.transform.localScale,
                                   OnRemovedNodeFinishedAnimation);
            }
        }
        /// <summary>
        /// Removes the given leaf node. The removal is animating by sinking the
        /// node. The node is not auto destroyed.
        /// </summary>
        /// <param name="node">leaf node to be removed</param>
        protected virtual void RenderRemovedOldLeaf(Node node)
        {
            if (objectManager.RemoveNode(node, out GameObject leaf))
            {
                // if the node needs to be removed, let it sink into the ground
                var newPosition = leaf.transform.position;
                newPosition.y = -leaf.transform.localScale.y;

                SimpleAnim.AnimateTo(node, leaf, newPosition, leaf.transform.localScale, OnRemovedNodeFinishedAnimation);
            }
        }

        /// <summary>
        /// Removes the given edge. The edge is not auto destroyed, however.
        /// </summary>
        /// <param name="edge"></param>
        protected virtual void RenderRemovedOldEdge(Edge edge)
        {
        }

        /// <summary>
        /// Clears all GameObjects created by the used ObjectManager
        /// </summary>
        private void ClearGraphObjects()
        {
            objectManager?.Clear();
            foreach (string tag in SEE.DataModel.Tags.All)
            {
                foreach (GameObject o in GameObject.FindGameObjectsWithTag(tag))
                {
                    DestroyImmediate(o);
                }
            }
        }

        /// <summary>
        /// Event function that destroys a given GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        public void OnRemovedNodeFinishedAnimation(object gameObject)
        {
            if (gameObject != null && gameObject is GameObject)
            {
                Destroy((GameObject)gameObject);
            }
        }
    }
}