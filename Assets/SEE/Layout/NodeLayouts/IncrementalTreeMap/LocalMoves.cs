using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Assertions;
using MathNet.Numerics.LinearAlgebra;

namespace SEE.Layout.NodeLayouts.IncrementalTreeMap
{
    static class LocalMoves
    {
        static double pNorm = 2d; // double.PositiveInfinity;

        static int RecursionBoundToBestSelection = 4;
        
        private static IList<LocalMove> findLocalMoves(TSegment segment)
        {
            List<LocalMove> result = new List<LocalMove>();
            if(segment.IsConst)
            {
                return result;
            }
            if(segment.Side1Nodes.Count == 1 && segment.Side2Nodes.Count == 1)
            {
                result.Add(new FlipMove(segment.Side1Nodes.First(),segment.Side2Nodes.First(), true));
                result.Add(new FlipMove(segment.Side1Nodes.First(),segment.Side2Nodes.First(), false));
                return result;
            }
            if(segment.IsVertical)
            {
                TNode upperNode1 = Utils.ArgMaxJ<TNode>(segment.Side1Nodes, x => x.Rectangle.z);
                TNode upperNode2 = Utils.ArgMaxJ<TNode>(segment.Side2Nodes, x => x.Rectangle.z);
                Assert.IsTrue(upperNode1.SegmentsDictionary()[Direction.Upper] == upperNode2.SegmentsDictionary()[Direction.Upper]);

                TNode lowerNode1 = Utils.ArgMinJ<TNode>(segment.Side1Nodes, x => x.Rectangle.z);
                TNode lowerNode2 = Utils.ArgMinJ<TNode>(segment.Side2Nodes, x => x.Rectangle.z);
                Assert.IsTrue(lowerNode1.SegmentsDictionary()[Direction.Lower] == lowerNode2.SegmentsDictionary()[Direction.Lower]);

                result.Add(new StretchMove(upperNode1,upperNode2));
                result.Add(new StretchMove(lowerNode1,lowerNode2));
                return result;
            }
            TNode rightNode1 = Utils.ArgMaxJ<TNode>(segment.Side1Nodes, x => x.Rectangle.x);
            TNode rightNode2 = Utils.ArgMaxJ<TNode>(segment.Side2Nodes, x => x.Rectangle.x);
            Assert.IsTrue(rightNode1.SegmentsDictionary()[Direction.Right] == rightNode2.SegmentsDictionary()[Direction.Right]);

            TNode leftNode1 = Utils.ArgMinJ<TNode>(segment.Side1Nodes, x => x.Rectangle.x);
            TNode leftNode2 = Utils.ArgMinJ<TNode>(segment.Side2Nodes, x => x.Rectangle.x);
            Assert.IsTrue(leftNode1.SegmentsDictionary()[Direction.Left] == leftNode2.SegmentsDictionary()[Direction.Left]);

            result.Add(new StretchMove(rightNode1,rightNode2));
            result.Add(new StretchMove(leftNode1,leftNode2));
            return result;
        }

        public static void AddNode(IList<TNode> nodes,TNode newNode)
        {
            // ArgMax is shit in c#
            // node with rectangle with highest aspect ratio
            TNode bestNode = Utils.ArgMaxJ<TNode>(nodes, x => x.Rectangle.AspectRatio());

            newNode.Rectangle = new TRectangle(x: bestNode.Rectangle.x, z: bestNode.Rectangle.z,
                                               width: bestNode.Rectangle.width, depth: bestNode.Rectangle.depth);
            IDictionary<Direction,TSegment> segments = bestNode.SegmentsDictionary();
            foreach(Direction dir in Enum.GetValues(typeof(Direction)))
            {
                newNode.registerSegment(segments[dir],dir);
            }
            if(bestNode.Rectangle.width >= bestNode.Rectangle.depth)
            {
                // [bestNode]|[newNode]
                TSegment newSegment = new TSegment(isConst: false, isVertical: true);
                newNode.registerSegment(newSegment, Direction.Left);
                bestNode.registerSegment(newSegment, Direction.Right);
                bestNode.Rectangle.width *= 0.5f;
                newNode.Rectangle.width *= 0.5f;
                newNode.Rectangle.x = bestNode.Rectangle.x + bestNode.Rectangle.width;
            }
            else
            {
                // [newNode]
                // ---------
                // [bestNode]
                TSegment newSegment = new TSegment(isConst: false, isVertical: false);
                newNode.registerSegment(newSegment, Direction.Lower);
                bestNode.registerSegment(newSegment, Direction.Upper);
                bestNode.Rectangle.depth *= 0.5f;
                newNode.Rectangle.depth *= 0.5f;
                newNode.Rectangle.z = bestNode.Rectangle.z + bestNode.Rectangle.depth;
            }
        }

        public static void DeleteNode(TNode obsoleteNode)
        {
            // check wether node is grounded
            var segments = obsoleteNode.SegmentsDictionary();
            bool isGrounded = false;
            if(segments[Direction.Left].Side2Nodes.Count == 1 && !segments[Direction.Left].IsConst)
            {
                isGrounded = true;
                //[E][O]
                var expandingNodes = segments[Direction.Left].Side1Nodes.ToArray();
                foreach(var node in expandingNodes)
                {
                    node.Rectangle.width += obsoleteNode.Rectangle.width;
                    node.registerSegment(segments[Direction.Right], Direction.Right);
                }
            }
            else if(segments[Direction.Right].Side1Nodes.Count == 1 && !segments[Direction.Right].IsConst)
            {
                isGrounded = true;
                //[O][E]
                var expandingNodes = segments[Direction.Right].Side2Nodes.ToArray();
                foreach(var node in expandingNodes)
                {
                    node.Rectangle.x = obsoleteNode.Rectangle.x;
                    node.Rectangle.width += obsoleteNode.Rectangle.width;
                    node.registerSegment(segments[Direction.Left], Direction.Left);
                }
            }
            else if(segments[Direction.Lower].Side2Nodes.Count == 1 && !segments[Direction.Lower].IsConst)
            {
                isGrounded = true;
                //[O]
                //[E]
                var expandingNodes = segments[Direction.Lower].Side1Nodes.ToArray();
                foreach(var node in expandingNodes)
                {
                    node.Rectangle.depth += obsoleteNode.Rectangle.depth;
                    node.registerSegment(segments[Direction.Upper], Direction.Upper);
                }
            }
            else if(segments[Direction.Upper].Side1Nodes.Count == 1 && !segments[Direction.Upper].IsConst)
            {  
                isGrounded = true;
                //[E]
                //[O]
                var expandingNodes = segments[Direction.Upper].Side2Nodes.ToArray();
                foreach(var node in expandingNodes)
                {
                    node.Rectangle.z = obsoleteNode.Rectangle.z;
                    node.Rectangle.depth += obsoleteNode.Rectangle.depth;
                    node.registerSegment(segments[Direction.Lower], Direction.Lower);
                }
            }
            if(isGrounded)
            {
                foreach(Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    obsoleteNode.deregisterSegment(dir);
                }
            }
            else
            {
                TSegment bestSegment = Utils.ArgMinJ<TSegment>(segments.Values, x => x.Side1Nodes.Count + x.Side2Nodes.Count);
                
                var moves = findLocalMoves(bestSegment);
                Assert.IsTrue(moves.All(x => x is (StretchMove)));
                foreach(var move in moves)
                {
                    if(move.Node1 != obsoleteNode && move.Node2 != obsoleteNode)
                    {
                        move.Apply();
                        DeleteNode(obsoleteNode);
                        return;
                    }
                }
                // We should never arrive here
                Assert.IsFalse(true);
            }
        }
        
        public static void MakeLocalMoves(List<TNode> nodes, int amount)
        {
            var allResults = RecursiveMakeMoves(nodes,amount);
            allResults.Add(new Tuple<List<TNode>, double>(nodes,AspectRatiosPNorm(nodes)));
            var bestResult = Utils.ArgMinJ(allResults, x => x.Item2).Item1;
            foreach(var node in nodes)
            {
                var resultNode = bestResult.Find(n => n.ID == node.ID);
                node.Rectangle = resultNode.Rectangle;
            }
            HashSet<TSegment> resultSegments = new HashSet<TSegment>();
            foreach(var resultNode in bestResult)
            {
                resultSegments.UnionWith(resultNode.SegmentsDictionary().Values);
            }
            foreach(var resultSegment in resultSegments)
            {
                var newSegment = new TSegment(resultSegment.IsConst,resultSegment.IsVertical);
                foreach(var resultNode in resultSegment.Side1Nodes.ToArray())
                {
                    var node = nodes.Find(n => n.ID == resultNode.ID);
                    node.registerSegment(newSegment, 
                        newSegment.IsVertical ? Direction.Right : Direction.Upper);
                }
                foreach(var resultNode in resultSegment.Side2Nodes.ToArray())
                {
                    var node = nodes.Find(n => n.ID == resultNode.ID);
                    node.registerSegment(newSegment, 
                        newSegment.IsVertical ? Direction.Left : Direction.Lower);
                }
            }
        }

        private static List<Tuple<List<TNode>,double>> RecursiveMakeMoves(
            List<TNode> nodes,
            int amount)
        {
            var result_ThisRecursion = new List<Tuple<List<TNode>,double>>();
            
            if(amount <= 0) return result_ThisRecursion;

            HashSet<TSegment> segments = new HashSet<TSegment>();
            foreach(var node in nodes)
            {
                segments.UnionWith(node.SegmentsDictionary().Values);
            }
            List<LocalMove> possibleMoves = new List<LocalMove>();
            foreach(var segment in segments)
            {
                possibleMoves.AddRange(findLocalMoves(segment));
            }

            foreach(var move in possibleMoves)
            {
                var nodeClones = CloneGraph(nodes,segments);
                var moveClone = move.Clone(nodeClones);
                moveClone.Apply();
                var works = CorrectAreas.Correct(nodeClones.Values.ToList());
                if(!works) continue;
                result_ThisRecursion.Add(
                    new Tuple<List<TNode>, double>(nodeClones.Values.ToList(),AspectRatiosPNorm(nodeClones.Values.ToList())));
            }
            result_ThisRecursion.Sort((x,y) => x.Item2.CompareTo(y.Item2));
            while(result_ThisRecursion.Count > RecursionBoundToBestSelection)
            {
                result_ThisRecursion.RemoveAt(RecursionBoundToBestSelection);
            }

            var results_NextRecursions = new List<Tuple<List<TNode>,double>>(); 
            foreach(var result in result_ThisRecursion)
            {
                results_NextRecursions.AddRange(RecursiveMakeMoves(result.Item1, amount-1));
            }
            return result_ThisRecursion.Concat(results_NextRecursions).ToList();
        }

        private static double AspectRatiosPNorm(IList<TNode> nodes)
        {
            Vector<double> aspectRatios = Vector<double>.Build.DenseOfEnumerable(nodes.Select(n => n.Rectangle.AspectRatio()));
            return aspectRatios.Norm(pNorm);
        }

        private static Dictionary<string,TNode> CloneGraph(IList<TNode> nodes, HashSet<TSegment> segments)
        {
            Dictionary<string,TNode> mapOriginalClone = 
            nodes.ToDictionary(
                node => node.ID,
                node => {
                            var nodeClone = new TNode(node.ID);
                            nodeClone.Rectangle = (TRectangle) node.Rectangle.Clone();
                            nodeClone.Size = node.Size;
                            return nodeClone;
                        });

            foreach(var segment in segments)
            {
                var segmentClone = new TSegment(segment.IsConst, segment.IsVertical);
                foreach(var node in segment.Side1Nodes.ToArray())
                {
                    mapOriginalClone[node.ID].registerSegment(segmentClone, 
                        segmentClone.IsVertical ? Direction.Right : Direction.Upper);
                }
                foreach(var node in segment.Side2Nodes.ToArray())
                {
                    mapOriginalClone[node.ID].registerSegment(segmentClone, 
                        segmentClone.IsVertical ? Direction.Left : Direction.Lower);
                }
            }
            return mapOriginalClone;
        }
    }
}