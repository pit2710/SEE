﻿using System;
using System.Collections.Generic;
using SEE.Controls;
using SEE.Game;
using SEE.Utils;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.GO
{
    /// <summary>
    /// Provides default material that can be shared among game objects to
    /// reduce the number of drawing calls. The material does not have
    /// any reflexions to save computation run-time.
    /// </summary>
    public class Materials
    {
        /// <summary>
        /// Different types of shaders used to draw the materials.
        /// </summary>
        public enum ShaderType
        {
            Opaque,          // fully drawn with no transparency
            Transparent,     // transparency is available
            TransparentLine, // for lines with transparency
            Invisible        // the object will be invisible
        }

        // Normal materials
        public const string OpaqueMaterialName = "Materials/OpaquePortalMaterial";
        public const string TransparentMaterialName = "Materials/TransparentPortalMaterial";
        public const string TransparentLineMaterialName = "Materials/TransparentLinePortalMaterial";
        public const string InvisibleMaterialName = "Materials/InvisibleMaterial";

        // MRTK variants
        public const string MRTKOpaqueMaterialName = "Materials/OpaqueMRTKMaterial";
        public const string MRTKTransparentMaterialName = "Materials/TransparentMRTKMaterial";
        public const string MRTKTransparentLineMaterialName = "Materials/TransparentLineMRTKMaterial";
        public const string MRTKInvisibleMaterialName = "Materials/InvisibleMRTKMaterial";

        // Special materials
        public const string TransparentMeshParticleSystemMaterialName = "Materials/TransparentMeshParticleMaterial";
        public const string MeshParticleSystemMaterialName = "Materials/MeshParticleMaterial";

        /// <summary>
        /// The type of the shaders of this material instance.
        /// </summary>
        public readonly ShaderType Type;

        /// <summary>
        /// The number of different colors and, thus, the number of
        /// different materials we create: one material for each color.
        /// </summary>
        public readonly uint NumberOfMaterials;

        /// <summary>
        /// The color at the lower end of the color spectrum.
        /// </summary>
        public readonly Color Lower;

        /// <summary>
        /// The color at the higher end of the color spectrum.
        /// </summary>
        public readonly Color Higher;

        /// <summary>
        /// The different materials. They depend upon two aspects:
        /// the offset in the rendering queue and the number of colors requested.
        /// The first index in the list of <see cref="materials"/> is the offset
        /// in the rendering queue, the second index in the materials array,
        /// which is an element of that list, is the color index.
        /// The entries of the inner material array are all alike except for the color.
        /// We will use a color gradient and one material for each color.
        /// Similarly, <see cref="materials"/>[i] and <see cref="materials"/>[j] will
        /// be alike except for the respective <see cref="Material.renderQueue"/> attribute.
        /// </summary>
        private readonly List<Material[]> materials;

        /// <summary>
        /// Creates materials for the given <paramref name="colorRange"/>, one material
        /// for each color at render queue offset 0, with the associated <paramref name="shaderType"/>.
        /// All created materials are alike except for their color.
        ///
        /// Precondition: <paramref name="colorRange.NumberOfColors"/> must be greater than 0.
        /// </summary>
        /// <param name="shaderType">shader type to be used to draw the new materials</param>
        /// <param name="colorRange">the color range for the new materials</param>
        public Materials(ShaderType shaderType, ColorRange colorRange)
        {
            Type = shaderType;
            Assert.IsTrue(colorRange.NumberOfColors > 0, "At least one color is needed");
            NumberOfMaterials = colorRange.NumberOfColors;
            Lower = colorRange.lower;
            Higher = colorRange.upper;
            // materials[0] is set up with the given colorRange for the render-queue offset 0.
            materials = new List<Material[]>() { Init(shaderType, colorRange.NumberOfColors, colorRange.lower, colorRange.upper, 0) };
        }

        /// <summary>
        /// Creates and returns the materials, one for each different color.
        ///
        /// Precondition: <paramref name="numberOfColors"/> > 0.
        /// </summary>
        /// <param name="shaderType">the type of the shader to be used to create the material</param>
        /// <param name="numberOfColors">the number of materials with different colors to be created</param>
        /// <param name="lower">the color at the lower end of the color spectrum</param>
        /// <param name="higher">the color at the higher end of the color spectrum</param>
        /// <param name="renderQueueOffset">the offset of the render queue</param>
        /// <returns>materials</returns>
        private static Material[] Init(ShaderType shaderType, uint numberOfColors, Color lower, Color higher, int renderQueueOffset)
        {
            Material[] result = new Material[numberOfColors];
            if (numberOfColors == 1)
            {
                result[0] = New(shaderType, Color.Lerp(lower, higher, 0.5f), renderQueueOffset);
            }
            else
            {
                // Assumption: numberOfColors > 1; if numberOfColors == 0, we would divide by zero.
                for (int i = 0; i < result.Length; i++)
                {
                    Color color = Color.Lerp(lower, higher, i / (float)(numberOfColors - 1));
                    result[i] = New(shaderType, color, renderQueueOffset);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the default material for the given <paramref name="index"/> (always the identical
        /// material, no matter how often this method is called). That means, if
        /// the caller modifies this material, other objects using it will be affected, too.
        /// <paramref name="renderQueueOffset"/> specifies the offset of the render queue for rendering.
        /// The larger the offset, the later the object will be rendered. An object drawn later
        /// will cover objects drawn earlier.
        /// Precondition: 0 <= degree <= numberOfColors-1 and renderQueueOffset >= 0; otherwise an exception is thrown
        /// </summary>
        /// <param name="renderQueueOffset">offset for the render queue</param>
        /// <param name="index">index of the material (color) in the range [0, numberOfColors-1]</param>
        /// <returns>default material</returns>
        public Material Get(int renderQueueOffset, int index)
        {
            if (index < 0 || index >= NumberOfMaterials)
            {
                throw new Exception($"Color degree {index} out of range [0, {NumberOfMaterials - 1}]");
            }
            if (renderQueueOffset < 0)
            {
                throw new Exception("Render queue offset must not be negative");
            }
            if (renderQueueOffset >= materials.Count)
            {
                // there are not materials for this renderQueueOffset; we need to create these first
                for (int i = materials.Count; i <= renderQueueOffset; i++)
                {
                    materials.Add(Init(Type, NumberOfMaterials, Lower, Higher, i));
                }
            }
            return materials[renderQueueOffset][index];
        }

        /// <summary>
        /// Sets the shared material of <paramref name="renderer"/> to the material with given <paramref name="index"/>
        /// and <paramref name="renderQueueOffset"/>. The <paramref name="index"/> will be clamped into
        /// [0, <see cref="NumberOfMaterials"/> - 1].
        /// </summary>
        /// <param name="renderer">renderer whose shared material is to be set</param>
        /// <param name="renderQueueOffset">the offset in the render queue</param>
        /// <param name="index">the index of the material</param>
        public void SetSharedMaterial(Renderer renderer, int renderQueueOffset, int index)
        {
            renderer.sharedMaterial = Get(renderQueueOffset, Mathf.Clamp(index, 0, (int)NumberOfMaterials - 1));
        }

        /// <summary>
        /// Creates and returns a new material. The material is loaded from a resource file with given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">the name of the file for the material; must be located in a resources folder</param>
        /// <param name="color">the color of the new material</param>
        /// <param name="renderQueueOffset">the offset of the new material in the render queue</param>
        /// <returns>new material</returns>
        public static Material New(string name, Color color, int renderQueueOffset = 0)
        {
            Material prefab = Resources.Load<Material>(name);
            Assert.IsNotNull(prefab, $"Material resource '{name}' could not be found!");
            Material material = new Material(prefab)
            {
                // FIXME this is not a good solution. we may want to add an enum or something for
                // possible materials, such that we can ensure the correct renderQueue. That would
                // adding new materials make easier as well.
                renderQueue = (int) (name.Contains("Transparent") ? UnityEngine.Rendering.RenderQueue.Transparent
                                                                  : UnityEngine.Rendering.RenderQueue.Geometry) + renderQueueOffset,
                color = color
            };
            return material;
        }

        /// <summary>
        /// Creates and returns a new material of given <paramref name="shaderType"/> and
        /// <paramref name="color"/>. This material will be unique and not reused by this
        /// class!
        /// </summary>
        /// <param name="shaderType">the type of the shader to be used to create the
        /// material</param>
        /// <param name="color">requested color of the new material</param>
        /// <param name="renderQueueOffset">the offset of the render queue</param>
        /// <returns>new material</returns>
        public static Material New(ShaderType shaderType, Color color, int renderQueueOffset = 0)
        {
            string name = null;

            // When the user is on a HoloLens, the special MRTK shader variants should be used
            if (PlayerSettings.GetInputType() != PlayerInputType.HoloLensPlayer)
            {
                switch (shaderType)
                {
                    case ShaderType.Opaque:             name = OpaqueMaterialName; break;
                    case ShaderType.Transparent:        name = TransparentMaterialName; break;
                    case ShaderType.TransparentLine:    name = TransparentLineMaterialName; break;
                    case ShaderType.Invisible:          name = InvisibleMaterialName; break;
                    default: Assertions.InvalidCodePath(); break;
                }
            }
            else
            {
                switch (shaderType)
                {
                    case ShaderType.Opaque:             name = MRTKOpaqueMaterialName; break;
                    case ShaderType.Transparent:        name = MRTKTransparentMaterialName; break;
                    case ShaderType.TransparentLine:    name = MRTKTransparentLineMaterialName; break;
                    case ShaderType.Invisible:          name = MRTKInvisibleMaterialName; break;
                    default: Assertions.InvalidCodePath(); break;
                }
            }
            return New(name, color, renderQueueOffset);
        }

        /// <summary>
        /// Creates and returns a new material of given <paramref name="shaderType"/> and
        /// <paramref name="color"/>. This material will be unique and not reused by this
        /// class!
        /// </summary>
        /// <param name="shaderType">the type of the shader to be used to create the
        /// material</param>
        /// <param name="renderQueueOffset">the offset of the render queue</param>
        /// <returns>new material</returns>
        public static Material New(ShaderType shaderType, int renderQueueOffset = 0)
        {
            return New(shaderType, Color.white, renderQueueOffset);
        }
    }
}
