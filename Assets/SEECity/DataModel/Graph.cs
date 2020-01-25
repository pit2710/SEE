﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SEE.DataModel
{
    /// <summary>
    /// A graph with nodes and edges representing the data to be visualized 
    /// by way of blocks and connections.
    /// </summary>
    [System.Serializable]
    public class Graph : Attributable
    {
        // The list of graph nodes indexed by their unique linkname
        [SerializeField]
        private StringNodeDictionary nodes = new StringNodeDictionary();

        // The list of graph edges.
        [SerializeField]
        private List<Edge> edges = new List<Edge>();

        // The (view) name of the graph.
        [SerializeField]
        private string viewName = "";

        // The path of the file from which this graph was loaded. Could be the
        /// empty string if the graph was not created by loading it from disk.
        [SerializeField]
        private string path = "";

        /// Adds a node to the graph. 
        /// Preconditions:
        ///   (1) node must not be null
        ///   (2) node.Linkname must be defined.
        ///   (3) a node with node.Linkname must not have been added before
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(Node node)
        {
            if (node == null)
            {
                throw new System.Exception("node must not be null");
            }
            if (String.IsNullOrEmpty(node.LinkName))
            {
                throw new System.Exception("linkname of a node must neither be null nor empty");
            }
            if (nodes.ContainsKey(node.LinkName))
            {
                throw new System.Exception("linknames must be unique");
            }
            nodes[node.LinkName] = node;
        }

        /// <summary>
        /// Returns true of this graph contains a node with the same unique linkname
        /// as the given node.
        /// Throws an exception if node is null or node has no valid linkname.
        /// </summary>
        /// <param name="node">node to be checked for containment</param>
        /// <returns>true iff there is a node contained in the graph with node.LinkName</returns>
        public bool Contains(Node node)
        {
            if (node == null)
            {
                throw new System.Exception("node must not be null");
            }
            if (String.IsNullOrEmpty(node.LinkName))
            {
                throw new System.Exception("linkname of a node must neither be null nor empty");
            }
            return nodes.ContainsKey(node.LinkName);
        }

        /// <summary>
        /// Adds a non-hierarchical edge to the graph.
        /// Precondition: edge must not be null.
        /// </summary>
        public void AddEdge(Edge edge)
        {
            edges.Add(edge);
        }

        /// <summary>
        /// Removes the given edge from the graph.
        /// </summary>
        /// <param name="edge">edge to be removed</param>
        public void RemoveEdge(Edge edge)
        {
            edges.Remove(edge);
        }

        /// <summary>
        /// The number of nodes of the graph.
        /// </summary>
        public int NodeCount => nodes.Count;

        /// <summary>
        /// The number of edges of the graph.
        /// </summary>
        public int EdgeCount => edges.Count;

        /// <summary>
        /// Name of the graph (the view name of the underlying RFG).
        /// </summary>
        public string Name
        {
            get => viewName;
            set => viewName = value;
        }

        /// <summary>
        /// The path of the file from which this graph was loaded. Could be the
        /// empty string if the graph was not created by loading it from disk.
        /// </summary>
        public string Path
        {
            get => path;
            set => path = value;
        }

        /// <summary>
        /// Returns all nodes of the graph.
        /// </summary>
        /// <returns>all nodes</returns>
        public List<Node> Nodes()
        {
            return nodes.Values.ToList();
        }

        /// <summary>
        /// Returns all non-hierarchical edges of the graph.
        /// </summary>
        /// <returns>all non-hierarchical edges</returns>
        public List<Edge> Edges()
        {
            return edges;
        }

        /// <summary>
        /// Returns the node with the given unique linkname. If there is no
        /// such node, node will be null and false will be returned; otherwise
        /// true will be returned.
        /// </summary>
        /// <param name="linkname">unique linkname of the searched node</param>
        /// <param name="node">the found node, otherwise null</param>
        /// <returns>true if a node could be found</returns>
        public bool TryGetNode(string linkname, out Node node)
        {
            return nodes.TryGetValue(linkname, out node);
        }

        /// <summary>
        /// Returns the list of nodes without parent.
        /// </summary>
        /// <returns>root nodes of the hierarchy</returns>
        public List<Node> GetRoots()
        {
            List<Node> result = new List<Node>();
            foreach (Node node in nodes.Values)
            {
                if (node.IsRoot())
                {
                    result.Add(node);
                }
            }
            return result;
        }

        /// <summary>
        /// Dumps the hierarchy for each root. Used for debugging.
        /// </summary>
        internal void DumpTree()
        {
            foreach (Node root in GetRoots())
            {
                DumpTree(root);
            }
        }

        /// <summary>
        /// Dumps the hierarchy for given root. Used for debugging.
        /// </summary>
        internal void DumpTree(Node root)
        {
            DumpTree(root, 0);
        }

        /// <summary>
        /// Dumps the hierarchy for given root by adding level many blanks 
        /// as indentation. Used for debugging.
        /// </summary>
        private void DumpTree(Node root, int level)
        {
            string indentation = "";
            for (int i = 0; i < level; i++)
            {
                indentation += "-";
            }            
            Debug.Log(indentation + root.LinkName + "\n");
            foreach (Node child in root.Children())
            {
                DumpTree(child, level + 1);
            }
        }

        /// <summary>
        /// Destroys the GameObjects of the graph's nodes and edges including the
        /// associated Node and Edge components as well as the GameObject of the graph 
        /// itself (and its Graph component). The graph is unusable afterward.
        /// </summary>
        public void Destroy()
        {
            edges.Clear();
            nodes.Clear();
        }

        /// <summary>
        /// Sorts the list of children of all nodes using the given comparison.
        /// </summary>
        /// <param name="comparison">the comparison used to sort the nodes in the hierarchy</param>
        public void SortHierarchy(Comparison<Node> comparison)
        {
            foreach (Node node in nodes.Values)
            {
                node.SortChildren(comparison);
            }
        }

        /// <summary>
        /// Sorts the list of children of all nodes using Node.CompareTo(), which compares the
        /// nodes by their names (either Source.Name or Linkname).
        /// </summary>
        public void SortHierarchyByName()
        {
            SortHierarchy(Node.CompareTo);
        }

        /// <summary>
        /// Returns the maximal depth of the graph. Precondition: Graph must be tree.
        /// </summary>
        /// <returns>The maximal depth of the graph.</returns>
        public int GetMaxDepth()
        {
            return GetMaxDepth(GetRoots(), -1);
        }

        /// <summary>
        /// Returns all edges of graph whose source and target is contained in 
        /// selectedNodes.
        /// </summary>
        /// <param name="graph">graph whose edges are to be filtered</param>
        /// <param name="selectedNodes">source and target nodes of required edges</param>
        /// <returns>all edges of graph whose source and target is contained in selectedNodes</returns>
        public IList<Edge> ConnectingEdges(ICollection<Node> selectedNodes)
        {
            IList<Edge> result = new List<Edge>();
            HashSet<Node> nodes = new HashSet<Node>(selectedNodes);

            foreach (Edge edge in this.Edges())
            {
                if (nodes.Contains(edge.Source) && nodes.Contains(edge.Target))
                {
                    result.Add(edge);
                }
            }
            return result;
        }

        private int GetMaxDepth(List<Node> nodes, int currentDepth)
        {
            int max = currentDepth + 1;
            for (int i = 0; i < nodes.Count; i++)
            {
                max = Math.Max(max, GetMaxDepth(nodes[i].Children(), currentDepth + 1));
            }
            return max;
        }

        /// <summary>
        /// Returns the graph in a JSON-like format including its attributes and all its nodes 
        /// and edges including their attributes.
        /// </summary>
        /// <returns>graph in textual form</returns>
        public override string ToString()
        {
            string result = "{\n";
            result += " \"kind\": graph,\n";
            result += " \"name\": \"" + viewName + "\",\n";
            // its own attributes
            result += base.ToString();
            // its nodes
            foreach (Node node in nodes.Values)
            {
                result += node.ToString() + ",\n";
            }
            foreach (Edge edge in edges)
            {
                result += edge.ToString() + ",\n";
            }
            result += "}\n";
            return result;
        }

        /// <summary>
        /// Sets the level of each node in the graph. The level of a root node is 0.
        /// For all other nodes, the level is the level of its parent + 1.
        /// </summary>
        public void CalculateLevels()
        {
            foreach (Node root in GetRoots())
            {
                root.SetLevel(0);
            }
        }

        /// <summary>
        /// Creates deep copies of attributes where necessary. Is called by
        /// Clone() once the copy is created. Must be extended by every 
        /// subclass that adds fields that should be cloned, too.
        /// 
        /// IMPORTANT NOTE: Cloning a graph means to create deep copies of
        /// its node and children, too. The hierarchy will be isomporph.
        /// </summary>
        /// <param name="clone">the clone receiving the copied attributes</param>
        protected override void HandleCloned(object clone)
        {
            base.HandleCloned(clone);
            Graph target = (Graph)clone;
            target.viewName = this.viewName;
            target.path = this.path;
            target.nodes = CopyNodes(this.nodes);
            target.edges = CopyEdges(this.edges);
            CopyHierarchy(this, target);
        }

        private StringNodeDictionary CopyNodes(StringNodeDictionary nodes)
        {
            StringNodeDictionary result = new StringNodeDictionary();
            foreach (var entry in nodes)
            {
                result.Add(entry.Key, (Node)entry.Value.Clone());
            }
            return result;
        }

        private List<Edge> CopyEdges(List<Edge> edges)
        {
            List<Edge> result = new List<Edge>();
            foreach (Edge edge in edges)
            {
                Edge clone = (Edge)edge.Clone();
                // set corresponding source
                if (TryGetNode(edge.Source.LinkName, out Node source))
                {
                    clone.Source = source;
                }
                else
                {
                    throw new Exception("target graph does not have a node with linkname " + edge.Source.LinkName);
                }
                // set corresponding target
                if (TryGetNode(edge.Target.LinkName, out Node target))
                {
                    clone.Target = target;
                }
                else
                {
                    throw new Exception("target graph does not have a node with linkname " + edge.Target.LinkName);
                }
                result.Add(clone);
            }
            return result;
        }

        private static void CopyHierarchy(Graph fromGraph, Graph toGraph)
        {
            foreach (Node fromRoot in fromGraph.GetRoots())
            {
                if (toGraph.TryGetNode(fromRoot.LinkName, out Node toRoot))
                {
                    CopyHierarchy(fromRoot, toRoot, toGraph);
                }
                else
                {
                    throw new Exception("target graph does not have a node with linkname " + fromRoot.LinkName);
                }
            }
            toGraph.CalculateLevels();
        }

        /// <summary>
        /// Adds the children to toParent corresponding to the children of fromParent in toGraph.
        /// </summary>
        /// <param name="fromParent">a parent node in the original graph</param>
        /// <param name="toParent">a parent node in copied graph (toGraph) whose children are to be added</param>
        /// <param name="toGraph">the graph copy containing toParent and its children</param>
        private static void CopyHierarchy(Node fromParent, Node toParent, Graph toGraph)
        {
            foreach (Node fromChild in fromParent.Children())
            {
                // Get the node in toGraph corresponding to fromChild.
                if (toGraph.TryGetNode(fromChild.LinkName, out Node toChild))
                {
                    // fromChild is a parent of fromParent and
                    // toChild must become a child of toParent
                    toParent.AddChild(toChild);
                    CopyHierarchy(fromChild, toChild, toGraph);
                }
                else
                {
                    throw new Exception("target graph does not have a node with linkname " + fromChild.LinkName);
                }
            }
        }
    }
}