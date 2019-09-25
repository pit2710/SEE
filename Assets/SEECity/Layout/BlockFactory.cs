﻿using System;
using UnityEngine;

namespace SEE.Layout
{
    /// <summary>
    /// A factory for visual representations of graph nodes in the scene.
    /// </summary>
    public abstract class BlockFactory
    {
        /// <summary>
        /// Creates and returns a new block representation of a graph node.
        /// </summary>
        /// <returns>new block representation</returns>
        public abstract GameObject NewBlock();

        /// <summary>
        /// Creates a new visual representation of the graph node and attaches it 
        /// to the parent as one of its immediate children. This function is equivalent
        /// to  AttachBlock(parent, NewBlock()).
        /// </summary>
        /// <param name="parent">parent of the visual representation of a graph node</param>
        public virtual void AddBlock(GameObject parent)
        {
            AttachBlock(parent, NewBlock());
        }

        /// <summary>
        /// Attaches an existing block to the parent as one of its immediate children.
        /// Adds a BlockModifier accordingly. 
        /// Note: This method must be extended by subclasses to attach the appropriate
        /// BlockModifier.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="block"></param>
        public virtual void AttachBlock(GameObject parent, GameObject block)
        {
            block.transform.parent = parent.transform;
            block.name = "house " + parent.name;
        }

        /// <summary>
        /// The length unit of a block representation in Unity measures.
        /// </summary>
        /// <returns>length unit of a block representation in Unity measure</returns>
        public virtual float Unit()
        {
            return 1.0f;
        }

        /// <summary>
        /// Returns the size of the block generated by this factory.
        /// Precondition: The given block must have been generated by this factory.
        /// </summary>
        /// <param name="block">block whose size is to be returned</param>
        /// <returns>size of the block</returns>
        public virtual Vector3 GetSize(GameObject block)
        {
            // Nodes represented by cubes have a renderer from which we can derive the
            // extent.
            Renderer renderer = block.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.size;
            }
            else
            {
                Debug.LogErrorFormat("Node {0} (tag: {1}) without renderer.\n", block.name, block.tag);
                return Vector3.one;
            }
        }

        /// <summary>
        /// Scales the given block by the given scale. Note: The unit of scaling depends
        /// upon a block factory type. Subclasses may use different units. For instance,
        /// a cube factory measures in terms of Unity units, while a BuildingFactory
        /// uses floors.
        /// Precondition: The given block must have been generated by this factory.
        /// </summary>
        /// <param name="block">block to be scaled</param>
        /// <param name="scale">scaling factor</param>
        public abstract void ScaleBlock(GameObject block, Vector3 scale);

        /// <summary>
        /// Sets the position of the current block. The given position is interpreted
        /// as the center of the block.
        /// </summary>
        /// <param name="block">block to be positioned</param>
        /// <param name="position">where to position the block</param>
        public virtual void SetPosition(GameObject block, Vector3 position)
        {
            // the default position of a game object in Unity is its center
            block.transform.position = position;
        }

        /// <summary>
        /// Returns the center of the roof of the given block.
        /// </summary>
        /// <param name="block">block for which to determine the roof position</param>
        /// <returns>roof position</returns>
        public virtual Vector3 Roof(GameObject block)
        {
            Vector3 result = block.transform.position;
            result.y += GetSize(block).y / 2.0f;
            return result;
        }

        /// <summary>
        /// Returns the center of the ground of a block.
        /// </summary>
        /// <param name="block">block for which to determine the ground position</param>
        /// <returns>ground position</returns>
        public virtual Vector3 Ground(GameObject block)
        {
            Vector3 result = block.transform.position;
            result.y -= GetSize(block).y / 2.0f;
            return result;
        }
    }
}
