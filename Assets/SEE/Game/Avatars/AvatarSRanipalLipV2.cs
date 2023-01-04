﻿using System;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;

namespace SEE.Game.Avatars
{
    /// <summary>
    /// Changed version of <see cref="SRanipal_AvatarLipSample_v2"/>. Used to initialize LipShape Tables.
    /// </summary>
    public class AvatarSRanipalLipV2 : MonoBehaviour
    {

        /// <summary>
        /// taken from <see cref="SRanipal_AvatarLipSample_v2"/>
        /// </summary>
        [SerializeField] private List<LipShapeTable_v2> LipShapeTables;

        /// <summary>
        /// taken from <see cref="SRanipal_AvatarLipSample_v2"/>
        /// </summary>
        public bool NeededToGetData = true;

        /// <summary>
        /// Lip Weightings
        /// </summary>
        private Dictionary<LipShape_v2, float> LipWeightings;

        /// <summary>
        /// In order to use our fake blendshapes from <see cref="AvatarBlendshapeExpressions"/>, we need to create a
        /// lipshape for each blendshape. For this purpose we need a <see cref="LipShapeTable_v2"/>
        /// which also contains a <see cref="LipShapeTable_v2"/> to store all Lip Shapes.
        /// </summary>
        private void Start()
        {
            if (!SRanipal_Lip_Framework.Instance.EnableLip)
            {
                enabled = false;
                return;
            }

            // Create new LipShapeTable
            LipShapeTables = new List<LipShapeTable_v2>(new LipShapeTable_v2[1]);

            // add empty LipShapeTable to list of LipShapeTables.
            LipShapeTables[0] = new LipShapeTable_v2();

            SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

            if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
            {
                LipShapeTables[0].skinnedMeshRenderer = skinnedMeshRenderer;
                int lipShapeTableSize = skinnedMeshRenderer.sharedMesh.blendShapeCount;

                LipShapeTables[0].lipShapes = new LipShape_v2[lipShapeTableSize];

                for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; ++i)
                {
                    string elementName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);

                    LipShapeTables[0].lipShapes[i] = LipShape_v2.None;
                    foreach (LipShape_v2 lipShape in (LipShape_v2[])Enum.GetValues(typeof(LipShape_v2)))
                    {
                        // Match blendshape name with lip shape name.
                        if (elementName == lipShape.ToString())
                        {
                            // Add correct lip shape to list of lip shapes.
                            LipShapeTables[0].lipShapes[i] = lipShape;
                        }
                    }
                }
            }
            SetLipShapeTables(LipShapeTables);
        }

        /// <summary>
        /// taken from <see cref="SRanipal_AvatarLipSample_v2"/>
        /// </summary>
        private void Update()
        {
            if (SRanipal_Lip_Framework.Status != SRanipal_Lip_Framework.FrameworkStatus.WORKING)
            {
                return;
            }

            if (NeededToGetData)
            {
                SRanipal_Lip_v2.GetLipWeightings(out LipWeightings);
                UpdateLipShapes(LipWeightings);
            }
        }

        /// <summary>
        /// taken from <see cref="SRanipal_AvatarLipSample_v2"/>
        /// </summary>
        public void SetLipShapeTables(List<LipShapeTable_v2> lipShapeTables)
        {
            bool valid = true;
            if (lipShapeTables == null)
            {
                valid = false;
            }
            else
            {
                for (int table = 0; table < lipShapeTables.Count; ++table)
                {
                    if (lipShapeTables[table].skinnedMeshRenderer == null)
                    {
                        valid = false;
                        break;
                    }
                    for (int shape = 0; shape < lipShapeTables[table].lipShapes.Length; ++shape)
                    {
                        LipShape_v2 lipShape = lipShapeTables[table].lipShapes[shape];
                        if (lipShape > LipShape_v2.Max || lipShape < 0)
                        {
                            valid = false;
                            break;
                        }
                    }
                }
            }
            if (valid)
            {
                LipShapeTables = lipShapeTables;
            }
        }

        /// <summary>
        /// taken from <see cref="SRanipal_AvatarLipSample_v2"/>
        /// </summary>
        public void UpdateLipShapes(Dictionary<LipShape_v2, float> lipWeightings)
        {
            foreach (LipShapeTable_v2 table in LipShapeTables)
            {
                RenderModelLipShape(table, lipWeightings);
            }
        }

        /// <summary>
        /// taken from <see cref="SRanipal_AvatarLipSample_v2"/>
        /// </summary>
        private void RenderModelLipShape(LipShapeTable_v2 lipShapeTable, Dictionary<LipShape_v2, float> weighting)
        {
            for (int i = 0; i < lipShapeTable.lipShapes.Length; i++)
            {
                int targetIndex = (int)lipShapeTable.lipShapes[i];
                if (targetIndex > (int)LipShape_v2.Max || targetIndex < 0)
                {
                    continue;
                }
                lipShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[(LipShape_v2)targetIndex] * 100);
            }
        }
    }
}