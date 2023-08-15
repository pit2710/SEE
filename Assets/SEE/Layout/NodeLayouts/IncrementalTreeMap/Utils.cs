using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace SEE.Layout.NodeLayouts.IncrementalTreeMap
{
    /// <summary>
    /// Provides helper functions for lay-outing nodes.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// arg max function, returns a item of a collection, that maximizes a function.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="eval">evaluation function</param>
        /// <returns></returns>
        public static T ArgMax<T>(ICollection<T> collection, Func<T, IComparable> eval)
        {
            var bestVal = collection.Max(eval);
            return collection.First(x => eval(x).CompareTo(bestVal) == 0);
        }

        /// <summary>
        /// arg min function, returns a item of a collection, that minimizes a function.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="eval">evaluation function</param>
        /// <returns></returns>
        public static T ArgMin<T>(ICollection<T> collection, Func<T, IComparable> eval)
        {
            var bestVal = collection.Min(eval);
            return collection.First(x => eval(x).CompareTo(bestVal) == 0);
        }

        /// <summary>
        /// Creates a new rectangle that includes all rectangles of <paramref name="nodes"/>.
        /// </summary>
        /// <param name="nodes">nodes with not null rectangles</param>
        /// <returns>new parent rectangle</returns>
        public static Rectangle CreateParentRectangle(IList<Node> nodes)
        {
            double x = nodes.Min(node => node.Rectangle.x);
            double z = nodes.Min(node => node.Rectangle.z);
            double width = nodes.Max(node => node.Rectangle.x + node.Rectangle.width) - x;
            double depth = nodes.Max(node => node.Rectangle.z + node.Rectangle.depth) - z;
            return new Rectangle(x, z, width, depth);
        }

        /// <summary>
        /// Linear Transformation, a list of <paramref name="nodes"/> with rectangles
        /// that are laid out in a <paramref name="oldRectangle"/> will be transformed, so that they fit
        /// in <paramref name="newRectangle"/>
        /// </summary>
        /// <param name="nodes">nodes with rectangles that should be transformed</param>
        /// <param name="newRectangle">new parent rectangle</param>
        /// <param name="oldRectangle">old parent rectangle</param>
        public static void TransformRectangles(IList<Node> nodes, Rectangle newRectangle, Rectangle oldRectangle)
        {
            // linear transform line   x1<---->x2
            //               to line       y1<------->y2
            // f  : [x1,x2] -> [y1,y2]
            // f  : x   maps to (x - x1) * ((y2-y1)/(x2-x1)) + y1

            double scaleX = newRectangle.width / oldRectangle.width;
            double scaleZ = newRectangle.depth / oldRectangle.depth;

            foreach (var node in nodes)
            {
                node.Rectangle.x = (node.Rectangle.x - oldRectangle.x) * scaleX + newRectangle.x;
                node.Rectangle.z = (node.Rectangle.z - oldRectangle.z) * scaleZ + newRectangle.z;
                node.Rectangle.width *= scaleX;
                node.Rectangle.depth *= scaleZ;
            }
        }

        /// <summary>
        /// Clones a list of nodes and preserves the layout including segments.
        /// Therefor the segments of the nodes are cloned to.
        /// </summary>
        /// <param name="nodes">siblings to be cloned</param>
        /// <returns>Dictionary that maps ID to new clone node</returns>
        public static IDictionary<string, Node> CloneGraph(IList<Node> nodes)
        {
            // clones the nodes only partially: does NOT clone the segment
            IDictionary<string, Node> clonesMap = nodes.ToDictionary(node => node.ID,
                node => new Node(node.ID)
                {
                    Rectangle = (Rectangle)node.Rectangle.Clone(), Size = node.Size
                }
            );

            // segments are here cloned separately
            var segments = nodes.SelectMany(n => n.SegmentsDictionary().Values).ToHashSet();
            Assert.IsTrue(segments.Count + 1 - 4 == nodes.Count);
            foreach (var segment in segments)
            {
                var segmentClone = new Segment(segment.IsConst, segment.IsVertical);
                foreach (var node in segment.Side1Nodes)
                {
                    clonesMap[node.ID].RegisterSegment(segmentClone,
                        segmentClone.IsVertical ? Direction.Right : Direction.Upper);
                }
                foreach (var node in segment.Side2Nodes)
                {
                    clonesMap[node.ID].RegisterSegment(segmentClone,
                        segmentClone.IsVertical ? Direction.Left : Direction.Lower);
                }
            }
            return clonesMap;
        }

        public static void CheckConsistent(IList<Node> nodes)
        {
            return;
            foreach (var node in nodes)
            {
                var segs = node.SegmentsDictionary();
                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    var seg = segs[dir];
                    Assert.IsNotNull(seg);
                    if (seg.IsConst)
                    {
                        Assert.IsTrue(seg.Side1Nodes.Count == 0 || seg.Side2Nodes.Count == 0);
                    }
                    else
                    {
                        Assert.IsTrue(seg.Side1Nodes.Count != 0 && seg.Side2Nodes.Count != 0);
                    }

                    if (dir == Direction.Left || dir == Direction.Right)
                    {
                        Assert.IsTrue(seg.IsVertical);
                    }
                    else
                    {
                        Assert.IsTrue(!seg.IsVertical);
                    }

                    if (dir == Direction.Left)
                    {
                        foreach (Node neighborNode in seg.Side1Nodes)
                        {
                            Assert.IsTrue(neighborNode.SegmentsDictionary()[Direction.Right] == seg);
                        }
                    }

                    if (dir == Direction.Right)
                    {
                        foreach (Node neighborNode in seg.Side2Nodes)
                        {
                            Assert.IsTrue(neighborNode.SegmentsDictionary()[Direction.Left] == seg);
                        }
                    }

                    if (dir == Direction.Lower)
                    {
                        foreach (Node neighborNode in seg.Side1Nodes)
                        {
                            Assert.IsTrue(neighborNode.SegmentsDictionary()[Direction.Upper] == seg);
                        }
                    }

                    if (dir == Direction.Upper)
                    {
                        foreach (Node neighborNode in seg.Side2Nodes)
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

        public static void CheckEqualNodeSets(IList<Node> nodes1, IList<Node> nodes2)
        {
            foreach (var node in nodes1)
            {
                Assert.IsTrue(nodes2.Contains(node));
            }

            foreach (var node in nodes2)
            {
                Assert.IsTrue(nodes1.Contains(node));
            }
        }
    }
}