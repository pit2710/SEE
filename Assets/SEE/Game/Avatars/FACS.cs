// Copyright 2023 Amir Safarha
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial
// portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO
// EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
// THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This component has to be attached to a GameObject whose meshes need to have
/// all of the BlendShapes provided by FACSHuman using the naming convention
/// of the Facial Action Coding System (FACS). Using this component, the GameObject
/// will be able to change its facial expressions based on a List of Action Units (AU)
/// provided. Changes will be animated.
/// </summary>
public class FACS : MonoBehaviour
{
    /// <summary>
    /// Inner class for creating instances of FACS Action Units.
    /// </summary>
    public class ActionUnit
    {
        /// <summary>
        /// The provided string refers to the unique name of AU BlendShapes.
        /// </summary>
        public string AU;

        /// <summary>
        /// The provided integer value describes the intensity of a single AU. It must be a number between
        /// 1-5 representing the expression strength A-E from the FACS.
        /// </summary>
        [Range(1, 5)]
        public int Intensity;

        /// <summary>
        /// Constructor of ActionUnit-Class for creating instances.
        /// </summary>
        public ActionUnit (string aU, int intensity)
        {
            AU = aU;
            Intensity = intensity;
        }
    }

    /// <summary>
    /// Component Array which will get filled with all SkinnedMeshRenderer of the child objects.
    /// </summary>
    private Component[] renderer;

    /// <summary>
    /// List of ActionUnits which have to be set. This list is public, so that external scripts can provide the
    /// desired ActionUnits.
    /// </summary>
    public List<FACS.ActionUnit> SelectedActions = new List<FACS.ActionUnit>();

    /// <summary>
    /// List of ActionUnits which already are present.
    /// </summary>
    private List<FACS.ActionUnit> ActualActions = new List<FACS.ActionUnit>();

    /// <summary>
    /// List of ActionUnits which aren't needed no more.
    /// </summary>
    private List<FACS.ActionUnit> NotNeededActions = new List<FACS.ActionUnit>();

    /// <summary>
    /// Float value for in- or decreasing BlendshapeWeights per Update()-execution.
    /// </summary>
    private float animationVelocity = 1.0f;

    /// <summary>
    /// This function is called once in the initializing phase. It retrieves the SkinnedMeshRenderer components from all
    /// meshes of the GameObject.
    /// </summary>
    void Start()
    {
        // Get all SkinnedMeshRenderer components of childs-objects.
        renderer = gameObject.GetComponentsInChildren(typeof(SkinnedMeshRenderer));
    }

    /// <summary>
    /// This function is called once per frame. It checks, whether there are AUs present, which aren't needed no more.
    /// The blendshapeweight of those will be decreased until it reaches 0. The blendshapeweight of selected AUs which
    /// don't have the desired value yet, will be in- or decreased until it reaches the desired value.
    /// </summary>
    void Update()
    {
        // If an AU is present, but does not appear in the new selection of AUs it is not needed anymore.
        // Therefore it gets added to the List<ActionUnit> named NotNeededActions.
        foreach (var action in ActualActions)
        {
            if (!SelectedActions.Any(au => au.AU == action.AU))
            {
                NotNeededActions.Add(action);
            }
        }

        // Check if SkinnedMeshRendererComponents have been found.
        if (renderer != null)
        {
            // The following code gets executed for every mesh of the GameObject, which own BlendShapes.
            foreach (SkinnedMeshRenderer meshRenderer in renderer)
            {
                // The blendshapeweight of AUs which are present but not needed anymore will be decreased inside
                // the following loop until it reaches 0. Afterwards they will be removed from the list.
                foreach (var NotNeededAU in NotNeededActions.ToList())
                {
                    // If an AU was decreasing / occuring in NotNeededAUs, but is now selected again,
                    // this AU doesn't need to be decreased anymore, wherefore it gets removed from the list
                    // and the iteration will stop for this entry.
                    if (SelectedActions.Any(au => au.AU == NotNeededAU.AU))
                    {
                        NotNeededActions.Remove(NotNeededAU);
                        break;
                    }

                    // Get the BlendShapeIndex of a specific AU by the string provided.
                    // GetBlendShapeIndex returns -1, if there was no BlendShape found.
                    var blendShapeIndex = meshRenderer.sharedMesh.GetBlendShapeIndex(NotNeededAU.AU);

                    // Check, whether a BlendShape with the provided name was found.
                    if (blendShapeIndex != -1)
                    {
                        // Get the current BlendShapeWeight of this AU.
                        var actualBlendshapeWeight = meshRenderer.GetBlendShapeWeight(blendShapeIndex);

                        // If the BlendshapeWeight of the AU is still above 0, it gets decreased by the animationvelocity.
                        // If it reaches 0, it will be removed from this list.
                        if (actualBlendshapeWeight > 0.0f)
                        {
                            meshRenderer.SetBlendShapeWeight(blendShapeIndex, actualBlendshapeWeight - animationVelocity);
                        }
                        else
                        {
                            NotNeededActions.Remove(NotNeededAU);
                        }
                    }
                    else
                    {
                        Debug.Log("Blendshape with name: " + NotNeededAU.AU + " not found.\n");
                    }


                }

                // The following loop checks whether AUs which should be displayed have reached their
                // desired intensity. If this is not the case, the BlendShapeWeight of the AU will be
                // in- or decreased until it reaches that value.
                foreach (var SelectedAU in SelectedActions)
                {
                    // Get the BlendShapeIndex of a specific AU by the string provided.
                    // GetBlendShapeIndex returns -1, if there was no BlendShape found.
                    var blendShapeIndex = meshRenderer.sharedMesh.GetBlendShapeIndex(SelectedAU.AU);

                    // Check, whether a BlendShape with the provided name was found.
                    if (blendShapeIndex != -1)
                    {
                        // Get the current BlendShapeWeight of this AU.
                        var actualBlendshapeWeight = meshRenderer.GetBlendShapeWeight(blendShapeIndex);

                        // Convert the Integer which represents the degree of expression of the AU to a
                        // percentual float value.
                        var intensityFloat = 20 * SelectedAU.Intensity;

                        // If the Blendshapeweight of the AU is below the desired intensity, the blendshapeweight will be
                        // increased. Otherwise it will be decreased.
                        if (actualBlendshapeWeight < intensityFloat)
                        {
                            meshRenderer.SetBlendShapeWeight(blendShapeIndex, actualBlendshapeWeight + animationVelocity);
                        }
                        else if (actualBlendshapeWeight > intensityFloat)
                        {
                            meshRenderer.SetBlendShapeWeight(blendShapeIndex, actualBlendshapeWeight - animationVelocity);
                        }
                    }
                    else
                    {
                        Debug.Log("Blendshape with name: " + SelectedAU.AU + " not found.\n");
                    }
                }
            }

            // Set the present AUs.
            ActualActions = SelectedActions;
        } else {
            Debug.LogError("No SkinnedMeshRenderer components found.\n");
        }
    }
}
