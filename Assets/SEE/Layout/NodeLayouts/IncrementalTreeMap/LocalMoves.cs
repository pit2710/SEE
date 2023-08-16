using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using SEE.Game.City;
using UnityEngine.Assertions;
using static SEE.Layout.NodeLayouts.IncrementalTreeMap.Direction;

namespace SEE.Layout.NodeLayouts.IncrementalTreeMap
{
    /// <summary>
    /// Provides algorithms for adding and deleting nodes to a layout
    /// and a algorithm to improve visual quality of a layout
    /// </summary>
    internal static class LocalMoves
    {
        /// <summary>
        /// Finds possible <see cref="LocalMove"/>s like flip the segment or stretch a node over the segment.
        /// </summary>
        /// <param name="segment">the segment</param>
        /// <returns>List of <see cref="LocalMove"/>s</returns>
        private static IList<LocalMove> FindLocalMoves(Segment segment)
        {
            List<LocalMove> result = new List<LocalMove>();
            if (segment.IsConst)
            {
                return result;
            }

            if (segment.Side1Nodes.Count == 1 && segment.Side2Nodes.Count == 1)
            {
                result.Add(new FlipMove(segment.Side1Nodes.First(), segment.Side2Nodes.First(), true));
                result.Add(new FlipMove(segment.Side1Nodes.First(), segment.Side2Nodes.First(), false));
                return result;
            }

            if (segment.IsVertical)
            {
                Node upperNode1 = Utils.ArgMax(segment.Side1Nodes, x => x.Rectangle.z);
                Node upperNode2 = Utils.ArgMax(segment.Side2Nodes, x => x.Rectangle.z);
                Assert.IsTrue(upperNode1.SegmentsDictionary()[Upper] == upperNode2.SegmentsDictionary()[Upper]);

                Node lowerNode1 = Utils.ArgMin(segment.Side1Nodes, x => x.Rectangle.z);
                Node lowerNode2 = Utils.ArgMin(segment.Side2Nodes, x => x.Rectangle.z);
                Assert.IsTrue(lowerNode1.SegmentsDictionary()[Lower] == lowerNode2.SegmentsDictionary()[Lower]);

                result.Add(new StretchMove(upperNode1, upperNode2));
                result.Add(new StretchMove(lowerNode1, lowerNode2));
                return result;
            }

            Node rightNode1 = Utils.ArgMax(segment.Side1Nodes, x => x.Rectangle.x);
            Node rightNode2 = Utils.ArgMax(segment.Side2Nodes, x => x.Rectangle.x);
            Assert.IsTrue(rightNode1.SegmentsDictionary()[Right] == rightNode2.SegmentsDictionary()[Right]);

            Node leftNode1 = Utils.ArgMin(segment.Side1Nodes, x => x.Rectangle.x);
            Node leftNode2 = Utils.ArgMin(segment.Side2Nodes, x => x.Rectangle.x);
            Assert.IsTrue(leftNode1.SegmentsDictionary()[Left] == leftNode2.SegmentsDictionary()[Left]);

            result.Add(new StretchMove(rightNode1, rightNode2));
            result.Add(new StretchMove(leftNode1, leftNode2));
            return result;
        }

        /// <summary>
        /// Adds a node to the layout, by slicing the node with the worst aspect ratio.
        /// Will NOT add <paramref name="newNode"/> to the list <paramref name="nodes"/>.
        /// </summary>
        /// <param name="nodes">nodes that represent a layout</param>
        /// <param name="newNode">the that should be added</param>
        public static void AddNode(IList<Node> nodes, Node newNode)
        {
            // node with rectangle with highest aspect ratio
            Node bestNode = Utils.ArgMax(nodes, x => x.Rectangle.AspectRatio());

            newNode.Rectangle = new Rectangle(x: bestNode.Rectangle.x, z: bestNode.Rectangle.z,
                width: bestNode.Rectangle.width, depth: bestNode.Rectangle.depth);
            IDictionary<Direction, Segment> segments = bestNode.SegmentsDictionary();
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                newNode.RegisterSegment(segments[dir], dir);
            }

            if (bestNode.Rectangle.width >= bestNode.Rectangle.depth)
            {
                // [bestNode]|[newNode]
                Segment newSegment = new Segment(isConst: false, isVertical: true);
                newNode.RegisterSegment(newSegment, Left);
                bestNode.RegisterSegment(newSegment, Right);
                bestNode.Rectangle.width *= 0.5f;
                newNode.Rectangle.width *= 0.5f;
                newNode.Rectangle.x = bestNode.Rectangle.x + bestNode.Rectangle.width;
            }
            else
            {
                // [newNode]
                // ---------
                // [bestNode]
                Segment newSegment = new Segment(isConst: false, isVertical: false);
                newNode.RegisterSegment(newSegment, Lower);
                bestNode.RegisterSegment(newSegment, Upper);
                bestNode.Rectangle.depth *= 0.5f;
                newNode.Rectangle.depth *= 0.5f;
                newNode.Rectangle.z = bestNode.Rectangle.z + bestNode.Rectangle.depth;
            }
        }

        /// <summary>
        /// Deleting a node of the layout, by expanding neighbor nodes over it.
        /// </summary>
        /// <param name="obsoleteNode">node to be delete, part of a layout</param>
        public static void DeleteNode(Node obsoleteNode)
        {
            // check whether node is grounded
            var segments = obsoleteNode.SegmentsDictionary();
            bool isGrounded = false;
            if (segments[Left].Side2Nodes.Count == 1 && !segments[Left].IsConst)
            {
                isGrounded = true;
                //[E][O]
                var expandingNodes = segments[Left].Side1Nodes.ToArray();
                foreach (var node in expandingNodes)
                {
                    node.Rectangle.width += obsoleteNode.Rectangle.width;
                    node.RegisterSegment(segments[Right], Right);
                }
            }
            else if (segments[Right].Side1Nodes.Count == 1 && !segments[Right].IsConst)
            {
                isGrounded = true;
                //[O][E]
                var expandingNodes = segments[Right].Side2Nodes.ToArray();
                foreach (var node in expandingNodes)
                {
                    node.Rectangle.x = obsoleteNode.Rectangle.x;
                    node.Rectangle.width += obsoleteNode.Rectangle.width;
                    node.RegisterSegment(segments[Left], Left);
                }
            }
            else if (segments[Lower].Side2Nodes.Count == 1 && !segments[Lower].IsConst)
            {
                isGrounded = true;
                //[O]
                //[E]
                var expandingNodes = segments[Lower].Side1Nodes.ToArray();
                foreach (var node in expandingNodes)
                {
                    node.Rectangle.depth += obsoleteNode.Rectangle.depth;
                    node.RegisterSegment(segments[Upper], Upper);
                }
            }
            else if (segments[Upper].Side1Nodes.Count == 1 && !segments[Upper].IsConst)
            {
                isGrounded = true;
                //[E]
                //[O]
                var expandingNodes = segments[Upper].Side2Nodes.ToArray();
                foreach (var node in expandingNodes)
                {
                    node.Rectangle.z = obsoleteNode.Rectangle.z;
                    node.Rectangle.depth += obsoleteNode.Rectangle.depth;
                    node.RegisterSegment(segments[Lower], Lower);
                }
            }

            if (isGrounded)
            {
                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    obsoleteNode.DeregisterSegment(dir);
                }
            }
            else
            {
                Segment bestSegment = Utils.ArgMin(segments.Values, x => x.Side1Nodes.Count + x.Side2Nodes.Count);

                var moves = FindLocalMoves(bestSegment);
                Assert.IsTrue(moves.All(x => x is (StretchMove)));
                foreach (var move in moves)
                {
                    if (move.Node1 != obsoleteNode && move.Node2 != obsoleteNode)
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

        /// <summary>
        /// Searches the space of layouts, that are similar to the layout of <paramref name="nodes"/>
        /// (in terms of distance in local moves).
        /// Apply the layout with the best visual quality to <paramref name="nodes"/>
        /// </summary>
        /// <param name="nodes">nodes that represent a layout</param>
        /// <param name="settings">settings for search</param>
        public static void LocalMovesSearch(List<Node> nodes, IncrementalTreeMapSetting settings)
        {
            var allResults = RecursiveMakeMoves(
                nodes,
                new List<LocalMove>(),
                settings);
            allResults.Add(new Tuple<List<Node>, double, IList<LocalMove>>
                (nodes, AspectRatiosPNorm(nodes, settings.PNorm), new List<LocalMove>()));
            var bestResult = Utils.ArgMin(allResults,
                x => x.Item2 * 10 + x.Item3.Count).Item1;

            var nodesDictionary = nodes.ToDictionary(n => n.ID, n => n);
            foreach (var resultNode in bestResult)
            {
                nodesDictionary[resultNode.ID].Rectangle = resultNode.Rectangle;
            }

            Utils.CloneSegments(from: bestResult, to: nodesDictionary);
        }

        /// <summary>
        /// Makes recursively local moves on clones of the layout to find similar layout with good visual quality.
        /// </summary>
        /// <param name="nodes">nodes that represent a layout</param>
        /// <param name="movesTillNow">moves that are done before in recursion</param>
        /// <param name="settings">the settings</param>
        /// <returns>selection of reached layouts, as tuples of nodes, visual quality measure of the layout and
        /// the local moves that are applied to get this layout.</returns>
        private static List<Tuple<List<Node>, double, IList<LocalMove>>> RecursiveMakeMoves(
            IList<Node> nodes,
            IList<LocalMove> movesTillNow,
            IncrementalTreeMapSetting settings)
        {
            var resultThisRecursion = new List<Tuple<List<Node>, double, IList<LocalMove>>>();
            if (movesTillNow.Count >= settings.localMovesDepth) return resultThisRecursion;
            ICollection<Segment> relevantSegments;
            if (movesTillNow.Count == 0)
            {
                relevantSegments = nodes.SelectMany(n => n.SegmentsDictionary().Values).ToHashSet();
            }
            else
            {
                var relevantNodes = movesTillNow.SelectMany(m => new[] { m.Node1.ID, m.Node2.ID }).ToHashSet()
                    .Select(id => nodes.First(n => n.ID == id));
                relevantSegments = relevantNodes.SelectMany(n => n.SegmentsDictionary().Values).ToHashSet();
            }

            var possibleMoves = relevantSegments.SelectMany(FindLocalMoves);
            foreach (var move in possibleMoves)
            {
                var nodeClonesDictionary = Utils.CloneGraph(nodes);
                var nodeClonesList = nodeClonesDictionary.Values.ToList();
                var moveClone = move.Clone(nodeClonesDictionary);
                Utils.CheckConsistent(nodeClonesList);
                moveClone.Apply();
                var works = CorrectAreas.Correct(nodeClonesList, settings);
                if (!works) continue;

                Utils.CheckConsistent(nodeClonesList);

                var newMovesList = new List<LocalMove>(movesTillNow) { moveClone };
                resultThisRecursion.Add(
                    new Tuple<List<Node>, double, IList<LocalMove>>
                        (nodeClonesList, AspectRatiosPNorm(nodeClonesList, settings.PNorm), newMovesList));
            }

            resultThisRecursion.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            while (resultThisRecursion.Count > settings.localMovesBranchingLimit)
            {
                resultThisRecursion.RemoveAt(settings.localMovesBranchingLimit);
            }

            var resultsNextRecursions = new List<Tuple<List<Node>, double, IList<LocalMove>>>();
            foreach (var result in resultThisRecursion)
            {
                resultsNextRecursions.AddRange(RecursiveMakeMoves(result.Item1, result.Item3, settings));
            }

            return resultThisRecursion.Concat(resultsNextRecursions).ToList();
        }

        /// <summary>
        /// A p norm is a class of vector norms, that is used in the local moves search algorithm
        /// to assess the visual quality of a list of <paramref name="nodes"/>,
        /// by considering the vector of aspect ratios of the <paramref name="nodes"/>.
        /// The kind of p norm changes what layout would be considered the best.
        /// For example with <paramref name="p"/>=1 (Manhattan Norm) the algorithm would
        /// minimize the sum of aspect ratios, while with <paramref name="p"/>=infinity (Chebyshev Norm)
        /// the algorithm would minimize the maximal aspect ratio over the <paramref name="nodes"/>.
        /// The other p norms range between these extremes.
        /// </summary>
        /// <param name="nodes">The nodes the should be assessed.</param>
        /// <param name="p">Determines the specific norm.</param>
        /// <returns>A measure for the visual quality of the nodes.</returns>
        private static double AspectRatiosPNorm(IList<Node> nodes, double p)
        {
            Vector<double> aspectRatios =
                Vector<double>.Build.DenseOfEnumerable(nodes.Select(n => n.Rectangle.AspectRatio()));
            return aspectRatios.Norm(p);
        }
    }
}