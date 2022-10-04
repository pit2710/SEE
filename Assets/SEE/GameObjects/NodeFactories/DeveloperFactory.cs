using SEE.Game;
using SEE.DataModel;
using UnityEngine;

namespace SEE.GO.NodeFactories
{
    internal class DeveloperFactory : NodeFactory
    {
        private GameObject flyingSaucer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DeveloperFactory()
            : base(Materials.ShaderType.Opaque, ColorRange.Default())
        {
            this.flyingSaucer = Resources.Load<GameObject> ("Prefabs/flyingsaucer");
        }

        /// <summary>
        /// Creates and returns a new block representation of a graph node.
        /// The interpretation of the given <paramref name="style"/> depends upon
        /// the subclasses. It can be used to specify a visual property of the
        /// objects such as the color. The allowed range of a style index depends
        /// upon the  subclasses, too, but must be in [0, NumberOfStyles()-1].
        /// The <paramref name="renderQueueOffset"/> specifies the offset of the render
        /// queue of the new block. The higher the value, the later the object
        /// will be drawn. Objects drawn later will cover objects drawn earlier.
        /// This parameter can be used for the rendering of transparent objects,
        /// where the inner nodes must be rendered before the leaves to ensure
        /// correct sorting.
        ///
        /// Parameter <paramref name="metrics"/> specifies the lengths of the returned
        /// object. If <c>null</c>, the default lengths are used. What a "length"
        /// constitutes, depends upon the kind of shape (mesh) used for the object
        /// and may be decided by subclasses of this <see cref="NodeFactory"/>.
        /// For instance, for a cube, the dimensions are its widths, height, and
        /// depth.
        /// </summary>
        /// <param name="style">specifies an additional visual style parameter of
        /// the object. This parameter is ignored.</param>
        /// <returns>new node representation</returns>
        /// <param name="metrics">the metric values determining the lengths of <paramref name="gameObject"/>.
        /// This parameter is ignored.
        /// </param>
        public override GameObject NewBlock(int style = 0, float[] metrics = null)
        {
            return GameObject.Instantiate(this.flyingSaucer) as GameObject;
        }

        public void SetName(GameObject dev, string devName)
        {
            FlyingDeveloper fd = dev.GetComponent<FlyingDeveloper> ();
            fd.AuthorName = devName;
        }

        /// <summary>
        /// This node does not need a collider.
        /// </summary>
        /// <param name="gameObject">the game object receiving the collider</param>
        protected override void AddCollider(GameObject gameObject)
        {
        }

        /// <summary>
        /// Returns a mesh for a node.
        /// </summary>
        /// <param name="metrics">the metric values determining the lengths of <paramref name="gameObject"/>.
        /// This value is ignored.
        /// </param>
        /// <returns>mesh for a node</returns>
        protected override Mesh GetMesh(float[] metrics)
        {
            // FIXME
            return null;
        }

        /// <summary>
        /// Sets the dimensions of <paramref name="gameObject"/>.
        ///
        /// The dimensions of a flying saucer are fixed. Changes are ignored.
        /// </summary>
        /// <param name="gameObject">the game object whose dimensions are to be set</param>
        /// <param name="metrics">the metric values determining the lengths of <paramref name="gameObject"/>.
        /// This value is ignored.
        /// </param>
        protected override void SetDimensions(GameObject gameObject, float[] metrics)
        {
        }

        /// <summary>
        /// Sets the size (its scale) of the given block by the given size. Note: The unit of
        /// size is Unity worldspace units.
        ///
        /// The size of a flying saucer is fixed. Changes are ignored.
        /// </summary>
        /// <param name="block">block to be scaled</param>
        /// <param name="size">new size in worldspace</param>
        public override void SetSize(GameObject block, Vector3 size)
        {
        }

        /// <summary>
        /// Sets the position of the current block. The given position is
        /// interpreted as the center (x,z) of the block on the ground (y).
        /// </summary>
        /// <param name="block">block to be positioned</param>
        /// <param name="position">where to position the block (its center) on the ground y</param>
        public override void SetGroundPosition(GameObject block, Vector3 position)
        {
            block.transform.position = new Vector3(position.x, position.y + 0.5f, position.z);
        }

        /// <summary>
        /// Sets the local position of the current block within its parent object.
        /// The given position is interpreted as the center (x,z) of the block on the ground (y).
        /// </summary>
        /// <param name="block">block to be positioned</param>
        /// <param name="position">where to position the block (its center)</param>
        public override void SetLocalGroundPosition(GameObject block, Vector3 position)
        {
            block.transform.localPosition = new Vector3(position.x, position.y + 0.5f, position.z);
        }
    }
}
