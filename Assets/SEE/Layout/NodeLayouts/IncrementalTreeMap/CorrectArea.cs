using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;

namespace SEE.Layout.NodeLayouts.IncrementalTreeMap
{
    static class CorrectAreas
    {

        static double precision = 0.00001; 
        public static bool Correct(IList<TNode> nodes)
        {
            if(IsSliceable( nodes, out TSegment slicingSegment))
            {
                Split(nodes, slicingSegment, 
                    out IList<TNode> partition1, 
                    out IList<TNode> partition2);
                
                
                // adjust slicing segment

                slicingSegment.IsConst = true;
                bool works1 = Correct(partition1);
                bool works2 = Correct(partition2);
                slicingSegment.IsConst = false;
                return works1 && works2;
            }
            else
            {
                return GradientDecrease(nodes);
            }

        }
        
        private static bool IsSliceable(IList<TNode> nodes, out TSegment slicingSegment)
        {
            slicingSegment = null;
            var segments = nodes.SelectMany(n => n.SegmentsDictionary().Values).ToHashSet();
            foreach (var segment in segments)
            {
                slicingSegment = segment;
                if (segment.IsConst) return false;
                if (segment.IsVertical)
                {
                    var nodeLowerEnd = Utils.ArgMinJ(segment.Side1Nodes, node => node.Rectangle.z);
                    var nodeUpperEnd = Utils.ArgMaxJ(segment.Side1Nodes,
                        node => node.Rectangle.z + node.Rectangle.depth);
                    return (nodeLowerEnd.SegmentsDictionary()[Direction.Lower].IsConst &&
                            nodeUpperEnd.SegmentsDictionary()[Direction.Upper].IsConst);
                }
                else
                {
                    var nodeLeftEnd = Utils.ArgMinJ(segment.Side1Nodes, node => node.Rectangle.x);
                    var nodeRightEnd = Utils.ArgMaxJ(segment.Side1Nodes,
                        node => node.Rectangle.x + node.Rectangle.width);
                    return (nodeLeftEnd.SegmentsDictionary()[Direction.Left].IsConst &&
                            nodeRightEnd.SegmentsDictionary()[Direction.Right].IsConst);
                }
            }

            return false;
        }

        private static void Split(IList<TNode> nodes, TSegment slicingSegment,
            out IList<TNode> partition1, out IList<TNode> partition2)
        {
        }
        
        
        private static bool GradientDecrease(IList<TNode> nodes)
        {
            HashSet<TSegment> segments = new HashSet<TSegment>();
            foreach(var node in nodes)
            {
                segments.UnionWith(node.SegmentsDictionary().Values);
            }
            segments.RemoveWhere(s => s.IsConst);
            int i = 0;
            Dictionary<TSegment,int> mapSegmentIndex 
                = segments.ToDictionary(s => s, s => i++);

            double distance = 0;
            for(int j = 0; j < 50; j++)
            {
                distance = CalculateOneStep(nodes, mapSegmentIndex);
                if(distance <= precision) break;
            }
            if(distance > precision)
            {
                Debug.LogWarning(" layout correction > " + precision.ToString());
            }
            bool cons = CheckCons(nodes);
            if(!cons)
            {
                Debug.LogWarning("layout correction failed negative rec");
            }
            return (cons && distance < precision);
        }

        private static Matrix<double> JacobianMatrix(
            IList<TNode> nodes, 
            Dictionary<TSegment, int> mapSegmentIndex)
        {
            int n = nodes.Count;
            var matrix = Matrix<double>.Build.Sparse(n,n-1);
            foreach(var node in nodes)
            {
                var segments = node.SegmentsDictionary();
                int index_node = nodes.IndexOf(node);    
                foreach(Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    var segment = segments[dir];
                    if(!segment.IsConst)
                    {
                        double value = 0;
                        switch(dir)
                        {
                            case Direction.Left:
                                value = - node.Rectangle.depth;
                                break;
                            case Direction.Right:
                                value = node.Rectangle.depth;
                                break;
                            case Direction.Lower:
                                value = - node.Rectangle.width;
                                break;
                            case Direction.Upper:
                                value = node.Rectangle.width;
                                break;
                        }
                        matrix[index_node,mapSegmentIndex[segment]] = value;
                    }
                }                   
            }
            return matrix;
        }

        private static double CalculateOneStep(
            IList<TNode> nodes, 
            Dictionary<TSegment, int> mapSegmentIndex)
        {
            Matrix<double> matrix = JacobianMatrix(nodes,mapSegmentIndex);
            
            Vector<double> nodes_sizes_wanted = 
                Vector<double>.Build.DenseOfArray(nodes.Select(node => (double) node.Size).ToArray());
            Vector<double> nodes_sizes_current = 
                Vector<double>.Build.DenseOfArray(nodes.Select(node => (double) node.Rectangle.Area()).ToArray());
            var diff = nodes_sizes_wanted - nodes_sizes_current;
            Matrix<double> pinv;
            try
            {
                pinv = matrix.PseudoInverse();
            }
            catch
            {
                try
                {
                    Matrix<double> bias = Matrix<double>.Build.Random(nodes.Count,mapSegmentIndex.Count, new ContinuousUniform(-.1,0.1));
                    pinv = (matrix + bias).PseudoInverse();
                    Debug.LogWarning("layout correction needs bias");
                }
                catch
                {
                    Debug.LogWarning("layout correction failed");
                    pinv = Matrix<double>.Build.Dense(mapSegmentIndex.Count,nodes.Count,0);
                }
            }

            Vector<double> segmentShift = pinv * diff;

            ApplyShift(segmentShift, nodes, mapSegmentIndex);
            
            Vector<double> nodes_sizes_afterStep = 
                Vector<double>.Build.DenseOfArray(nodes.Select(node => node.Rectangle.Area()).ToArray());
            return (nodes_sizes_afterStep - nodes_sizes_wanted).Norm(2);
        }


        private static void ApplyShift(
            Vector<double> shift,
            IList<TNode> nodes,
            Dictionary<TSegment, int> mapSegmentIndex)
        {
            foreach(var node in nodes)
            {
                var segments = node.SegmentsDictionary();
                foreach(Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    var segment = segments[dir];
                    if(segment.IsConst) continue;
                    var value = shift[mapSegmentIndex[segment]];
                    switch(dir)
                    {
                        case Direction.Left:
                            node.Rectangle.x += value;
                            node.Rectangle.width -= value;
                            break;
                        case Direction.Right:
                            node.Rectangle.width += value;
                            break;
                        case Direction.Lower:
                            node.Rectangle.z += value;
                            node.Rectangle.depth -= value;
                            break;
                        case Direction.Upper:
                            node.Rectangle.depth += value;
                            break;
                    }
                }
            }
        }

        private static bool CheckCons(IList<TNode> nodes)
        {
            foreach(var node in nodes)
            {
                if(node.Rectangle.width <= 0 || node.Rectangle.depth <= 0) return false;
            }
            return true;
        }

    }
}