﻿using UnityEngine;
using NUnit.Framework;
using SEE.DataModel;
using System.Collections.Generic;
using SEE.DataModel.IO;
using System.Linq;
using SEE.Game;
using SEE.GO;

namespace SEE.Layout
{
    /// <summary>
    /// Test cases for SEE.Layout.IO.Reader and SEE.Layout.IO.Writer.
    /// </summary>
    public class TestLayoutIO
    {
        private static Graph LoadGraph(string filename)
        {
            GraphReader graphReader = new GraphReader(filename, new HashSet<string>() { hierarchicalEdgeType });
            graphReader.Load();
            return graphReader.GetGraph();
        }

        /// <summary>
        /// The name of the hierarchical edge type we use for emitting the parent-child
        /// relation among nodes.
        /// </summary>
        private const string hierarchicalEdgeType = "Enclosing";

        [Test]
        public void TestWriteRead()
        {
            ICollection<ILayoutNode> gameObjects = NodeCreator.CreateNodes();
            //SEE.Layout.IO.Reader reader 
            //    = new SEE.Layout.IO.Reader(Application.dataPath + "/../Data/GXL/SEE/Architecture.gvl", 
            //                               gameObjects.Cast<IGameNode>().ToList());

        }

        /// <summary>
        /// Tests whether a file can be read that was generated by Gravis.
        /// </summary>
        [Test]
        public void TestRead()
        {
            GameObject go = new GameObject();
            SEECity city = go.AddComponent<SEECity>();
            city.NodeLayout = AbstractSEECity.NodeLayouts.Manhattan;
            city.EdgeLayout = AbstractSEECity.EdgeLayouts.None;
            city.LeafObjects = AbstractSEECity.LeafNodeKinds.Blocks;
            city.InnerNodeObjects = AbstractSEECity.InnerNodeKinds.Blocks;

            Graph graph = LoadGraph(Application.dataPath + "/../Data/GXL/SEE/Architecture.gxl");
            GraphRenderer graphRenderer = new GraphRenderer(city);
            graphRenderer.Draw(graph, go);
            // The game-object hierarchy for the nodes in graph are children of go.
            ICollection<ILayoutNode> gameObjects = GetGameObjects(go);
            SEE.Layout.IO.Reader reader = new SEE.Layout.IO.Reader(Application.dataPath + "/../Data/GXL/SEE/Architecture.gvl", 
                                                                   gameObjects.Cast<IGameNode>().ToList());
            Dump(gameObjects);
        }

        private void Dump(ICollection<ILayoutNode> gameObjects)
        {
            foreach (ILayoutNode layoutNode in gameObjects)
            {
                Debug.LogFormat("{0} position={1} worldScale={2}\n", layoutNode.ID, layoutNode.CenterPosition, layoutNode.AbsoluteScale);
            }
        }

        private ICollection<ILayoutNode> GetGameObjects(GameObject go)
        {
            List <ILayoutNode>  result = new List<ILayoutNode>();
            if (go.tag == Tags.Node)
            {
                result.Add(ToTestGameNode(go));
            }
            foreach (Transform child in go.transform)
            {
                ICollection<ILayoutNode> ascendants = GetGameObjects(child.gameObject);
                result.AddRange(ascendants);
            }
            return result;
        }

        private ILayoutNode ToTestGameNode(GameObject go)
        {
            NodeRef nodeRef = go.GetComponent<NodeRef>();
            return new LayoutVertex(nodeRef.node.ID);
        }
    }
}
