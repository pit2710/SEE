﻿using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A container for nodes and edges of the graph represented in a scene where the 
/// nodes and edges are GameObjects.
/// </summary>
public class SceneGraph
{
    [Tooltip("The tag of all buildings")]
    public string houseTag = "House";

    [Tooltip("The tag of all connections")]
    public string edgeTag = "Edge";

    [Tooltip("The relative path to the connection preftab")]
    public string linePreftabPath = "Assets/Prefabs/Line.prefab";
    [Tooltip("The relative path to the building preftab")]
    const string housePrefabPath = "Assets/Prefabs/House.prefab";

    // orientation of the edges; 
    // if -1, the edges are drawn below the houses;
    // if 1, the edges are drawn above the houses;
    // use either -1 or 1
    const float orientation = -1f;

    // The list of graph nodes indexed by their unique linkname
    private Dictionary<string, GameObject> nodes = new Dictionary<string, GameObject>();

    // The list of graph edges.
    private List<GameObject> edges = new List<GameObject>();

    /// <summary>
    /// Deletes the edges of the graph.
    /// </summary>
    public void DeleteEdges()
    {
        DeleteGameObjects(edges);
        edges.Clear();
    }

    /// <summary>
    /// Deletes the nodes of the graph.
    /// </summary>
    public void DeleteNodes()
    {
        foreach (KeyValuePair<string, GameObject> entry in nodes)
        {
            UnityEngine.Object.DestroyImmediate(entry.Value);
        }
        nodes.Clear();
    }

    /// <summary>
    /// Deletes all nodes and edges of the graph.
    /// </summary>
    public void Delete()
    {
        DeleteNodes();
        DeleteEdges();
    }

    /// <summary>
    /// Deletes all given objects immediately.
    /// </summary>
    /// <param name="objects"></param>
    private void DeleteGameObjects(List<GameObject> objects)
    {
        foreach (GameObject o in objects)
        {
            UnityEngine.Object.DestroyImmediate(o);
        }
    }

    // The number of nodes.
    public int NodeCount()
    {
        return nodes.Count;
    }

    /// <summary>
    /// Returns the node having the given nodeID. Throws an Exception
    /// if no such node exists.
    /// </summary>
    /// <param name="nodeID">unique ID of the node to be retrieved</param>
    /// <returns></returns>
    public GameObject GetNode(string nodeID)
    {
        if (!nodes.TryGetValue(nodeID, out GameObject result))
        {
            throw new Exception("Unknown node id " + nodeID);
        }
        return result;
    }

    // The underlying graph whose nodes and edges are to be visualized.
    private IGraph graph = new Graph();

    /// <summary>
    /// Loads the graph from the given file and creates the GameObjects representing
    /// its nodes and edges.
    /// </summary>
    /// <param name="filename"></param>
    public void Load(string filename)
    {
        GraphCreator graphCreator = new GraphCreator(filename, graph, new Logger());
        graphCreator.Load();
        Debug.Log("Number of nodes loaded: " + graph.NodeCount + "\n");
        Debug.Log("Number of edges loaded: " + graph.EdgeCount + "\n");
        metricMaxima = DetermineMetricMaxima(widthMetric, heightMetric, breadthMetric);
        CreateNodes();
        CreateEdges();
    }

    private void DumpMetricMaxima()
    {
        foreach (var item in metricMaxima)
        {
            Debug.Log("maximum of " + item.Key + ": " + item.Value + "\n");
        }
    }

    const string widthMetric = "Metric.Lines.LOC";
    const string heightMetric = "Metric.Halstead.Length";
    const string breadthMetric = "Metric.McCabe_Complexity";

    private Dictionary<string, float> metricMaxima;

    Dictionary<string, float> DetermineMetricMaxima(params string[] metrics)
    {
        Dictionary<string, float> result = new Dictionary<string, float>();
        foreach (string metric in metrics)
        {
            result.Add(metric, 0.0f);
        }
   
        foreach (INode node in graph.Nodes())
        {
            foreach (string metric in metrics)
            {
                if (node.TryGetInt(metric, out int value))
                {
                    if (value > result[metric])
                    {
                        result[metric] = value;
                    }
                }
                else
                {
                    Debug.Log(node.LinkName + " has no " + metric + "\n");
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Creates the GameObjects representing the nodes of the graph.
    /// The graph must have been loaded before via Load().
    /// </summary>
    public void CreateNodes()
    {
        GameObject housePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(housePrefabPath);
        if (housePrefab == null)
        {
            Debug.LogError(housePrefabPath + " does not exist.\n");
        }
        else
        {
            int length = (int)Mathf.Sqrt(graph.NodeCount);
            int column = 0;
            int row = 1;

            foreach (INode node in graph.Nodes())
            {
                column++;
                if (column > length)
                {
                    // exceeded length of the square => start a new row
                    column = 1;
                    row++;
                }
                GameObject house = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(housePrefab);
                house.name = node.GetString("Source.Name");

                float width = NormalizedIntMetric(node, widthMetric);
                float breadth = NormalizedIntMetric(node, breadthMetric);
                float height = NormalizedIntMetric(node, heightMetric);
                house.transform.localScale = new Vector3(width, height, breadth);

                // The position is the center of a GameObject. We want all GameObjects
                // be placed at the same ground level 0. That is why we need to "lift"
                // every building by half of its height.
                house.transform.position = new Vector3(row + row * 0.3f, height / 2.0f, column + column * 0.3f);
                /*
                {
                    Renderer renderer;
                    //Fetch the GameObject's Renderer component
                    renderer = house.GetComponent<Renderer>();
                    //Change the GameObject's Material Color to red
                    //m_ObjectRenderer.material.color = Color.red;
                    Debug.Log("house size: " + renderer.bounds.size + "\n");
                }
                */
                nodes.Add(node.LinkName, house);
            }
        }
    }

    // The minimal length of any axis (width, breadth, height) of a block.
    // Must not exceed 1.0f.
    const float minimalLength = 0.1f;

    private float NormalizedIntMetric(INode node, string metric)
    {
        float max = metricMaxima[metric];

        if (max <= 0.0f)
        {
            return minimalLength;
        }
        if (node.TryGetInt(metric, out int width))
        {
            if (width <= minimalLength)
            {
                return minimalLength;
            }
            else
            {
                float result = (float)width / max;
                if (result > 1.0f)
                {
                    Debug.LogError("unexpected metric value for " + metric + ": " + result);
                    throw new Exception("unexpected metric value for " + metric + ": " + result);
                }
                return result;
            }
        }
        else
        {
            return minimalLength;
        }
    }

    /// <summary>
    /// Creates the GameObjects representing the edges of the graph.
    /// The graph must have been loaded before via Load().
    /// </summary>
    public void CreateEdges()
    {
        /*
        GameObject linePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(linePreftabPath);

        if (linePrefab == null)
        {
            Debug.LogError(linePreftabPath + " does not exist.\n");
        }
        else
        {
            // the distance of the edges relative to the houses; the maximal height of
            // a house is 1.0
            const float above = orientation * (1f / 2.0f);


            for (int i = 1; i <= totalEdges; i++)
            {
                // pick two nodes randomly (node ids are in the range 1..graph.NodeCount()
                int start = UnityEngine.Random.Range(1, graph.NodeCount() + 1);
                int end = UnityEngine.Random.Range(1, graph.NodeCount() + 1);
                GameObject edge = drawLine(graph.GetNode(start.ToString()), graph.GetNode(end.ToString()), linePrefab, above);
                graph.AddEdge(edge);
                if (totalEdges % 100 == 0)
                {
                    Debug.Log("Created " + i + "/" + totalEdges + " rows of buildings.\n");
                }
            }
            Debug.Log("Created city with " + totalEdges + " connections.\n");
        }
        */
    }

    private GameObject drawLine(GameObject from, GameObject to, GameObject linePrefab, float above)
    {
        GameObject edge = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(linePrefab);
        LineRenderer renderer = edge.GetComponent<LineRenderer>();

        renderer.sortingLayerName = "OnTop";
        renderer.sortingOrder = 5;
        renderer.positionCount = 4; // number of vertices

        var points = new Vector3[renderer.positionCount];
        // starting position
        points[0] = from.transform.position;
        // position above starting position
        points[1] = from.transform.position;
        points[1].y += above;
        // position above ending position
        points[2] = to.transform.position;
        points[2].y += above;
        // ending position
        points[3] = to.transform.position;
        renderer.SetPositions(points);

        //renderer.SetWidth(0.5f, 0.5f);
        renderer.useWorldSpace = true;
        return edge;
    }
}
