using SEE.DataModel.DG;
using SEE.Layout.NodeLayouts.Cose;
using SEE.Layout.NodeLayouts.IncrementalTreeMap;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Layout.NodeLayouts
{
    /// <summary>
    /// </summary>
    public class IncrementalTreeMapLayout : HierarchicalNodeLayout, IIncrementalNodeLayout
    {
        /// <summary>
        /// Constructor. The width and depth are assumed to be in Unity units.
        /// </summary>
        /// <param name="groundLevel">the y co-ordinate setting the ground level; all nodes will be
        /// placed on this level</param>
        /// <param name="width">width of the rectangle in which to place all nodes in Unity units</param>
        /// <param name="depth">width of the rectangle in which to place all nodes in Unity units</param>
        public IncrementalTreeMapLayout(float groundLevel,
                             float width,
                             float depth)
        : base(groundLevel)
        {
            name = "IncrementalTreeMap";
            this.width = width;
            this.depth = depth;
        }

        /// <summary>
        /// The width of the rectangle in which to place all nodes in Unity units.
        /// </summary>
        private readonly float width;

        /// <summary>
        /// The depth of the rectangle in which to place all nodes in Unity units.
        /// </summary>
        private readonly float depth;

        /// <summary>
        /// The node layout we compute as a result.
        /// </summary>
        private Dictionary<ILayoutNode, NodeTransform> layout_result  = new Dictionary<ILayoutNode, NodeTransform>();
        private Dictionary<string,TNode>               TNodeMap       = new Dictionary<string, TNode>();
        private Dictionary<string,ILayoutNode>         ILayoutNodeMap = new Dictionary<string, ILayoutNode>();

        /// <summary>
        /// Some padding will be added between nodes. That padding depends upon the minimum
        /// of the width and depth of a node, multiplied by this factor.
        /// </summary>
        private const float PaddingFactor = 0.025f;

        /// <summary>
        /// The minimal padding between nodes in absolute (world space) terms.
        /// </summary>
        private const float MinimalAbsolutePadding = 0.002f;

        /// <summary>
        /// The maximal padding between nodes in absolute (world space) terms.
        /// </summary>
        private const float MaximalAbsolutePadding = 0.1f;

        private const int NumberOfLocalMoves = 1;

        private IncrementalTreeMapLayout oldLayout;

        public IIncrementalNodeLayout OldLayout
        {   set {
                    if(value is IncrementalTreeMapLayout)
                    {
                        this.oldLayout = (IncrementalTreeMapLayout) value;
                    }
                    else
                    {
                        this.oldLayout = null;
                        Debug.LogWarning("Incremental layout was not of same type");
                    }
                }
        }

        public override Dictionary<ILayoutNode, NodeTransform> Layout(IEnumerable<ILayoutNode> layoutNodes)
        {
            List<ILayoutNode> layoutNodeList = layoutNodes.ToList();
            switch (layoutNodeList.Count)
            {
                case 0:
                    throw new ArgumentException("No nodes to be laid out.");
                case 1:
                {
                    using IEnumerator<ILayoutNode> enumerator = layoutNodeList.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        // MoveNext() must be called before we can call Current.
                        ILayoutNode gameNode = enumerator.Current;
                        UnityEngine.Assertions.Assert.AreEqual(gameNode.AbsoluteScale, gameNode.LocalScale);
                        layout_result[gameNode] = new NodeTransform(Vector3.zero,
                            new Vector3(width, gameNode.LocalScale.y, depth));
                    }
                    else
                    {
                        Assert.IsTrue(false, "We should never arrive here.\n");
                    }

                    break;
                }
                default:
                {
                    this.roots = LayoutNodes.GetRoots(layoutNodeList);
                    InitTNodes();
                    CalculateLayout();
                    break;
                }
            }
            return layout_result;
        }    

        private void InitTNodes()
        {
            float totalLocalScale = 0;
            foreach(ILayoutNode node in roots)
            {
                totalLocalScale += InitTNode(node);
            }
            // adjust size 
            float adjustFactor = (width*depth) / totalLocalScale;
            foreach(var node in TNodeMap.Values)
            {
                node.Size *= adjustFactor;
            }
        }
        private float InitTNode(ILayoutNode node)
        {
            if (node.IsLeaf)
            {
                // a leaf
                Vector3 size = node.LocalScale;
                // x and z lengths may differ; we need to consider the larger value
                float result = Mathf.Max(size.x, size.z);
                TNode newTNode = new TNode(node.ID);
                newTNode.Size = result;
                TNodeMap.Add(node.ID, newTNode);
                ILayoutNodeMap.Add(node.ID,node);
                return result;
            }
            else
            {
                TNode newTNode = new TNode(node.ID);
                TNodeMap.Add(node.ID, newTNode);
                ILayoutNodeMap.Add(node.ID,node);
                float total_size = 0.0f;
                foreach (ILayoutNode child in node.Children())
                {
                    total_size += InitTNode(child);
                }
                newTNode.Size = total_size;
                return total_size;
            }
        }

        /// <summary>
        /// Adds positioning and scales to <see cref="layout_result"/> for all root nodes (nodes with no parent)
        /// within a rectangle whose center position is Vector3.zero and whose width and depth is
        /// as specified by the constructor call. This function is then called recursively for the
        /// children of each root (until leaves are reached).
        /// </summary>
        private void CalculateLayout()
        {
            /// Our "logical" rectangle in which to put the whole treemap is assumed to have its
            /// center at Vector3.zero here. <see cref="CalculateLayout(ICollection{ILayoutNode}, float, float, float, float)"/>
            /// assumes the rectangle's location be specified by its left front corner.
            /// Hence, we need to transform the center of the "logical" rectangle to the left front
            /// corner of the rectangle by -width/2 and -depth/2, respectively.
            TRectangle rectangle = new TRectangle(x: -width / 2.0f, z: -depth / 2.0f, width, depth);
            if (roots.Count == 1)
            {
                ILayoutNode root = roots[0];
                Assert.AreEqual(root.AbsoluteScale, root.LocalScale);
                layout_result[root] = new NodeTransform(Vector3.zero,
                    new Vector3(width, root.LocalScale.y, depth));
                CalculateLayout(root.Children(), rectangle);
            }
            else
            {
                CalculateLayout(roots,rectangle);
            }
        }

        private void CalculateLayout(ICollection<ILayoutNode> siblings,TRectangle rectangle)
        {
            // GetNodes can be done before if then else
            if(    this.oldLayout == null
                || NumberOfOccurrencesInOldGraph(siblings) <= 4
                || ParentsInOldGraph(siblings).Count != 1)
            {
                IList<TNode> nodes = GetTNodes(siblings);
                Dissect.dissect(rectangle, nodes);
            }
            else
            {
                // here can be done some improvement

                //  [         oldTNodes           ]--------------   <- nodes of old Layout, do not edit them
                //  [         workWith            ]--------------   <- nodes of new Layout, designed to be changed over time to newTNodes
                //  --------------[           newTNodes         ]   <- nodes of new Layout 
                //  [ toBeDeleted ]---------------[  toBeAdded  ]   <- nodes of new Layout

                // not only the siblings that are in the old graph and in the new one, but all siblings in old graph
                // note that there is exact one single parent (because of if-clause), but can be null if children == roots
                ILayoutNode oldILayoutParent = ParentsInOldGraph(siblings).First();
                ICollection<ILayoutNode> oldILayoutSiblings = oldILayoutParent == null ? oldLayout.roots : oldILayoutParent.Children();
                IList<TNode> oldTNodes = oldLayout.GetTNodes(oldILayoutSiblings);
                IList<TNode> newTNodes = GetTNodes(siblings);

                IList<TNode> workWith          = new List<TNode>();
                IList<TNode> nodesToBeDeleted  = new List<TNode>();
                IList<TNode> nodesToBeAdded    = new List<TNode>();

                // get nodes form old layout .. over take their rectangles
                foreach(var oldTNode in oldTNodes)
                {
                    TNode newTNode = ((List<TNode>) newTNodes).Find(x => x.ID.Equals(oldTNode.ID));
                    if(newTNode == null)
                    {   
                        newTNode = new TNode(oldTNode.ID);
                        nodesToBeDeleted.Add(newTNode);
                        workWith.Add(newTNode);
                    }
                    else
                    {
                        workWith.Add(newTNode);
                    }
                    newTNode.Rectangle = (TRectangle) oldTNode.Rectangle.Clone();
                }

                foreach(var newTNode in newTNodes)
                {
                    if( !workWith.Contains(newTNode))
                    {
                        nodesToBeAdded.Add(newTNode);
                    }
                }

                // get segments from old layout
                var debugSegments = oldLayout.ExtractSegments(oldTNodes);
                foreach(var segment in debugSegments)
                {
                    TSegment newSegment = new TSegment(segment.IsConst, segment.IsVertical);
                    foreach(var oldTNode in segment.Side1Nodes)
                    {
                        TNode newNode = workWith.Where(x => x.ID == oldTNode.ID).First();
                        newNode.registerSegment(newSegment, newSegment.IsVertical ? Direction.Right : Direction.Upper);
                    }
                    foreach(var oldTNode in segment.Side2Nodes)
                    {
                        TNode newNode = workWith.Where(x => x.ID == oldTNode.ID).First();
                        newNode.registerSegment(newSegment, newSegment.IsVertical ? Direction.Left : Direction.Lower);
                    }
                }


                TRectangle oldRectangle = oldLayout.GetParentTRectangle(oldTNodes[0]);
                
                TransformRectangles(workWith, oldRectangle: oldRectangle, newRectangle: rectangle);

                foreach(var obsoleteNode in nodesToBeDeleted)
                {
                    LocalMoves.DeleteNode(obsoleteNode);
                    workWith.Remove(obsoleteNode);
                    CheckConsistent(workWith);
                }
                CorrectAreas.Correct(workWith);
                CheckConsistent(workWith);
                foreach(var nodeToBeAdded in nodesToBeAdded)
                {
                    LocalMoves.AddNode(workWith,nodeToBeAdded);
                    workWith.Add(nodeToBeAdded);
                    CheckConsistent(workWith);
                }
                CheckEqualNodeSets(workWith, newTNodes);

                CorrectAreas.Correct(workWith);
                LocalMoves.MakeLocalMoves(workWith.ToList(), NumberOfLocalMoves);
                CheckConsistent(workWith);
            }

            AddToLayout(GetTNodes(siblings));

            foreach (ILayoutNode node in siblings)
            {
                ICollection<ILayoutNode> children = node.Children();
                if (children.Count > 0)
                {
                    // Note: nodeTransform.position is the center position, while
                    // CalculateLayout assumes co-ordinates x and z as the left front corner

                    Assert.AreEqual(node.AbsoluteScale, node.LocalScale);
                    TRectangle childRectangle = TNodeMap[node.ID].Rectangle;
                    CalculateLayout(children, childRectangle);
                }
            }
        }

        private ICollection<ILayoutNode> ParentsInOldGraph(ICollection<ILayoutNode> nodes)
        {
            HashSet<ILayoutNode> parents = new HashSet<ILayoutNode>();
            foreach(ILayoutNode node in nodes)
            {
                if(oldLayout.ILayoutNodeMap.TryGetValue(node.ID, out var oldNode))
                {
                    parents.Add(oldNode.Parent);
                }
            }
            return parents;
        }

        private int NumberOfOccurrencesInOldGraph(ICollection<ILayoutNode> nodes)
        {
            int counter = 0;
            foreach (ILayoutNode node in nodes)
            {
                counter += oldLayout.ILayoutNodeMap.ContainsKey(node.ID) ? 1 : 0;
            }
            return counter;
        }

        public override Dictionary<ILayoutNode, NodeTransform> Layout
            (ICollection<ILayoutNode> layoutNodes, ICollection<Edge> edges,
             ICollection<SublayoutLayoutNode> sublayouts)
        {
            throw new NotImplementedException();
        }

        public override bool UsesEdgesAndSublayoutNodes()
        {
            return false;
        }

        internal IList<TNode> GetTNodes(ICollection<ILayoutNode> layoutNodes)
        {
            List<TNode> result = new List<TNode>();
            foreach( ILayoutNode layoutNode in layoutNodes)
            {
                result.Add(this.TNodeMap[layoutNode.ID]);
            }
            return result;
        }

        internal HashSet<TSegment> ExtractSegments(ICollection<TNode> nodes)
        {
            HashSet<TSegment> result = new HashSet<TSegment>();
            foreach(TNode node in nodes)
            {
                ICollection<TSegment> boundingSegments = node.SegmentsDictionary().Values;
                foreach(TSegment segment in boundingSegments)
                {
                    Assert.IsNotNull(segment);
                    result.Add(segment);
                }
            }
            return result;
        }

        private void AddToLayout (IList<TNode> nodes)
        {
            float aspect_ratio = Math.Max(depth/width, width/depth);
            float padding = (float) (Math.Min(depth,width) / ( (1f / PaddingFactor) * Math.Pow(TNodeMap.Count / aspect_ratio,0.5f)));
            padding = MinimalAbsolutePadding; //Mathf.Clamp(padding, MinimalAbsolutePadding, MaximalAbsolutePadding);

            foreach (TNode node in nodes)
            {
                ILayoutNode o = ILayoutNodeMap[node.ID];
                TRectangle rect = node.Rectangle;
                if (rect.width <= 2 * padding || rect.depth <= 2 * padding)
                {
                    Debug.LogWarning("Rectangle to small for padding");
                }

                Vector3 position = new Vector3((float)(rect.x + rect.width / 2.0d),
                    groundLevel,
                    (float)(rect.z + rect.depth / 2.0d));
                Vector3 scale = new Vector3(
                    rect.width - 2 * padding > 0 ?
                        (float) (rect.width - 2 * padding) : (float) rect.width,
                    o.LocalScale.y,
                    rect.depth - 2 * padding > 0 ? (float) (rect.depth - 2 * padding) : (float) rect.depth);
                Assert.AreEqual(o.AbsoluteScale, o.LocalScale, $"{o.ID}: {o.AbsoluteScale} != {o.LocalScale}");
                layout_result[o] = new NodeTransform(position, scale);
            }
        }

        private void TransformRectangles(IList<TNode> nodes, TRectangle newRectangle ,TRectangle oldRectangle)
        {

            // linear transform line   x1<---->x2
            //               to line       y1<------->y2
            // f  : [x1,x2] -> [y1,y2]
            // f  : x   maps to (x - x1) * ((y2-y1)/(x2-x1)) + y1

            double scale_x = newRectangle.width / oldRectangle.width;
            double scale_z = newRectangle.depth / oldRectangle.depth;

            foreach( var node in nodes)
            {
                node.Rectangle.x = (node.Rectangle.x - oldRectangle.x) * scale_x + newRectangle.x;
                node.Rectangle.z = (node.Rectangle.z - oldRectangle.z) * scale_z + newRectangle.z;
                node.Rectangle.width *= scale_x;
                node.Rectangle.depth *= scale_z;
            }
        }

        private TRectangle GetParentTRectangle(TNode node)
        {
            var layoutNode = ILayoutNodeMap[node.ID];
            TRectangle result;
            try
            {
                var parentTNode = TNodeMap[layoutNode.Parent.ID];
                result = (TRectangle) parentTNode.Rectangle.Clone();
            }
            catch
            {   
                result = new TRectangle(-.5f * width,-.5f * depth, width, depth);
            }
            return result;
        }

        private void CheckConsistent(IList<TNode> nodes)
        {
            foreach(var node in nodes)
            {
                var segs = node.SegmentsDictionary(); 
                foreach(Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    var seg = segs[dir];
                    Assert.IsNotNull(seg);
                    if(seg.IsConst)
                    {
                        Assert.IsTrue(seg.Side1Nodes.Count == 0 || seg.Side2Nodes.Count == 0);
                    }
                    else
                    {
                        Assert.IsTrue(seg.Side1Nodes.Count != 0 && seg.Side2Nodes.Count != 0);
                    }
                    if(dir == Direction.Left || dir == Direction.Right)
                    {
                        Assert.IsTrue(seg.IsVertical);
                    }
                    else
                    {
                        Assert.IsTrue(!seg.IsVertical);
                    }

                    if(dir == Direction.Left)
                    {
                        foreach(TNode neighborNode in seg.Side1Nodes)
                        {
                            Assert.IsTrue(neighborNode.SegmentsDictionary()[Direction.Right] == seg);
                        }
                    }
                    if(dir == Direction.Right)
                    {
                        foreach(TNode neighborNode in seg.Side2Nodes)
                        {
                            Assert.IsTrue(neighborNode.SegmentsDictionary()[Direction.Left] == seg);
                        }
                    }
                    if(dir == Direction.Lower)
                    {
                        foreach(TNode neighborNode in seg.Side1Nodes)
                        {
                            Assert.IsTrue(neighborNode.SegmentsDictionary()[Direction.Upper] == seg);
                        }
                    }
                    if(dir == Direction.Upper)
                    {
                        foreach(TNode neighborNode in seg.Side2Nodes)
                        {
                            Assert.IsTrue(neighborNode.SegmentsDictionary()[Direction.Lower] == seg);
                        }
                    }

                }
                Assert.IsTrue(segs[Direction.Left].Side2Nodes.Contains(node));
                Assert.IsTrue(segs[Direction.Right].Side1Nodes.Contains(node));
                Assert.IsTrue(segs[Direction.Lower].Side2Nodes.Contains(node));
                Assert.IsTrue(segs[Direction.Upper].Side1Nodes.Contains(node));

                Assert.IsTrue(node.Rectangle.width > 0);
                Assert.IsTrue(node.Rectangle.depth > 0);
            }
        }
    
        private void CheckEqualNodeSets(IList<TNode> nodes1, IList<TNode> nodes2)
        {
            foreach(var node in nodes1)
            {
                Assert.IsTrue(nodes2.Contains(node));
            }
            foreach(var node in nodes2)
            {
                Assert.IsTrue(nodes1.Contains(node));
            }
        }
    
        private void PrintGraphForDebugDebug ()
        {
            string msg = "data = [";
            foreach(var node in TNodeMap.Values)
            {
                if(node == null || node.Rectangle == null) continue;
                msg += "\n";
                msg += "(" + node.ID + ", " ;
                msg += node.Rectangle.x.ToString() + ", ";
                msg += node.Rectangle.width.ToString() + ", ";
                msg += node.Rectangle.z.ToString() + ", ";
                msg += node.Rectangle.depth.ToString() + "),";
            }
            msg += "]";
            Debug.Log(msg);
        }

    }
}
