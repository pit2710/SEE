using System;
using SEE.Game;
using SEE.DataModel;
using UnityEngine;

namespace SEE.GO.NodeFactories
{
    internal class ContributionFactory : CubeFactory
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ContributionFactory()
            : base(Materials.ShaderType.Opaque, ColorRange.Default())
        { }

        /// <summary>
        /// Get a unique color for this branch number.
        /// <param name="branchId">The branch number</param>
        /// </summary>
        protected static Color BranchToColor (int branchId)
        {
            double phi1 = 2.0d / (1.0d + Math.Sqrt(5.0d));
            double hue = branchId * phi1;
            hue -= Math.Floor(hue);
            Color ret = Color.HSVToRGB((float)hue, 1.0f, 1.0f);
            ret.a = 0.25f;
            return ret;
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
        /// the object</param>
        /// <returns>new node representation</returns>
        /// <param name="metrics">the metric values determining the lengths of <paramref name="gameObject"/></param>
        public override GameObject NewBlock(int style = 0, float[] metrics = null)
        {
            GameObject gameObject = new GameObject() { tag = Tags.Node };
            // A MeshFilter is necessary for the gameObject to hold a mesh.
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = GetMesh(metrics);
            SetDimensions(gameObject, metrics);
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            Material baseMat = Resources.Load<Material> ("Materials/TransparentContributionMaterial");
            renderer.material = new Material (baseMat);

            return gameObject;
        }

        /// <summary>
        /// This node does not need a collider.
        /// </summary>
        /// <param name="gameObject">the game object receiving the collider</param>
        protected override void AddCollider(GameObject gameObject)
        {
        }

        /// <summary>
        /// Set the node's material color according to the branch number.
        /// <param name="gameObject">The game object for the node</param>
        /// <param name="branchNr">The branch number</param>
        /// </summary>
        public void SetBranchNumber(GameObject gameObject, int branchNr)
        {
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer> ();
            renderer.sharedMaterial.color = BranchToColor (branchNr);
        }
    }
}
