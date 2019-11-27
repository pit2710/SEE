﻿using SEE.DataModel;
using SEE.Layout;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An ObjectManager creates and manages GameObjects with a given BlockFactory.
/// Non-existing GameObjects are created and stored for reuse during query,
/// depending on the implementation. Each GameObject is assigned to the
/// LinkName of a node and can be retrieved via any node with the same LinkName.
/// </summary>
public abstract class AbstractCCAObjectManager
{
    /// <summary>
    /// The BlockFactory used internally for creating missing
    /// GameObjects.
    /// </summary>
    private readonly NodeFactory _nodeFactory;

    /// <summary>
    /// Returns the internally used BlockFactory.
    /// </summary>
    public NodeFactory NodeFactory => _nodeFactory;

    /// <summary>
    /// Creates a new ObjectManager with the given BlackFactory.
    /// </summary>
    /// <param name="nodeFactory">The given BlockFactory.</param>
    public AbstractCCAObjectManager(NodeFactory nodeFactory)
    {
        nodeFactory.AssertNotNull("nodeFactory");

        _nodeFactory = nodeFactory;
    }

    /// <summary>
    /// Returns all created GameObjects till now
    /// </summary>
    public abstract List<GameObject> GameObjects
    {
        get;
    }

    /// <summary>
    /// Returns a saved GameObject for the root or generates a new one if it does not already exist.
    /// </summary>
    /// <param name="root">The root GameObject or null if no GameObject could be found or generated.</param>
    /// <returns>True if the GameObject already existed and false if it was generated.</returns>
    public abstract bool GetRoot(out GameObject root);

    /// <summary>
    /// Returns a saved GameObject for an inner node or generates a new one if it does not already exist.
    /// </summary>
    /// <param name="node">The inner node under which a GameObject may be stored.</param>
    /// <param name="innerNode">The GameObject associated to node or null if no GameObject could be found or generated.</param>
    /// <returns>True if the GameObject already existed and false if it was generated.</returns>
    public abstract bool GetInnerNode(Node node, out GameObject innerNode);

    /// <summary>
    /// Returns a saved GameObject for a leaf node or generates a new one if it does not already exist.
    /// </summary>
    /// <param name="node">The leaf node under which a GameObject may be stored.</param>
    /// <param name="leaf">The GameObject associated to node or null if no GameObject could be found or generated.</param>
    /// <returns>True if the GameObject already existed and false if it was generated.</returns>
    public abstract bool GetLeaf(Node node, out GameObject leaf);

    /// <summary>
    /// Removes the GameObject from a given node in the internal memory structure
    /// and returns it for further use.
    /// </summary>
    /// <param name="node">The node under which a GameObject may be stored.</param>
    /// <param name="gameObject">The Gameobject, which belongs to the given node.</param>
    /// <returns>True if a GameObject was found for the given node.</returns>
    public abstract bool RemoveNode(Node node, out GameObject gameObject);

    /// <summary>
    /// Removes all generated GameObjects from internally used memory structures.
    /// This does not delete or destroy GameObjects.
    /// </summary>
    public abstract void Clear();
}
