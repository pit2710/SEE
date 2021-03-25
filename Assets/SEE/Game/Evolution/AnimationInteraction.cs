﻿//Copyright 2020 Florian Garbade

//Permission is hereby granted, free of charge, to any person obtaining a
//copy of this software and associated documentation files (the "Software"),
//to deal in the Software without restriction, including without limitation
//the rights to use, copy, modify, merge, publish, distribute, sublicense,
//and/or sell copies of the Software, and to permit persons to whom the Software
//is furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
//PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
//USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using SEE.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SEE.Game.Evolution
{
    /// <summary>
    /// The AnimationInteraction manages user inputs and interfaces
    /// </summary>
    public class AnimationInteraction : MonoBehaviour
    {
        /// <summary>
        /// The camera from the user.
        /// </summary>
        //public FlyCamera FlyCamera; // serialized by Unity (it is a MonoBehaviour)

        /// <summary>
        /// As to whether the UI for selecting revisions is currently shown.
        /// </summary>
        private bool isRevisionSelectionOpen = false;

        /// <summary>
        /// Returns true if RevisionSelectionCanvas is currently shown.
        /// </summary>
        public bool IsRevisionSelectionOpen => isRevisionSelectionOpen; // serialized by Unity

        /// <summary>
        /// The in-game animation canvas shown while viewing the animations. It contains
        /// the panel for the instructions shown to the user (explaining the
        /// keys) and a panel for the currently shown revision, the total number
        /// of revisions and the auto-play toggle. If the ESC key is hit, the
        /// RevisionSelectionCanvas is shown again.
        /// </summary>
        public GameObject AnimationCanvas; // serialized by Unity

        /// <summary>
        /// The user-data model for AnimationCanvas.
        /// </summary>
        private AnimationDataModel animationDataModel; // not serialized; will be set in Init()

        /// <summary>
        /// The in-game canvas containing the menu for selecting the shown graph revision. 
        /// It is shown when the user enters the ESC key. Beside the revision selection
        /// menu, it also contains a close button. If this button is pressed, the
        /// AnimationCanvas is shown again.
        /// </summary>
        public GameObject RevisionSelectionCanvas; // serialized by Unity

        /// <summary>
        /// The time in between two revisions in auto-play mode.
        /// </summary>
        private float animationTimeValue = 2;

        /// <summary>
        /// The user-data model for RevisionSelectionCanvas.
        /// </summary>
        private RevisionSelectionDataModel revisionSelectionDataModel; // not serialized; will be set in Init()

        /// <summary>
        /// The evolution renderer doing the rendering and animations of the graphs.
        /// </summary>
        private EvolutionRenderer evolutionRenderer; // not serialized; will be set in property EvolutionRenderer

        /// <summary>
        /// The container for the markers, needed for serialization 
        /// </summary>
        private SliderMarkerContainer sliderMarkerContainer; // not serialized; will be set in Init()

        /// <summary>
        /// The currently selected marker
        /// </summary>
        private Button selectedMarker; 

        /// <summary>
        /// A dictionary linking markers and comments, needed for saving the comments on application quit and deleting the comments
        /// </summary>
        private Dictionary<Button, InputField> markerDictionary = new Dictionary<Button, InputField>();

        /// <summary>
        /// Specifies whether the animation is currently being fast-forwarded
        /// </summary>
        private bool isFastForward;

        /// <summary>
        /// Specifies whether the animation is currently being fast-backwarded
        /// </summary>
        private bool isFastBackward;

        /// <summary>
        /// The evolution renderer doing the rendering and animations of the graphs.
        /// </summary>
        public EvolutionRenderer EvolutionRenderer
        {
            set
            {
                evolutionRenderer = value;
                Init();
            }
        }

        private void Init()
        {

            Canvas canvas = AnimationCanvas.GetComponent<Canvas>();
            canvas.worldCamera = MainCamera.Camera;

            revisionSelectionDataModel = RevisionSelectionCanvas.GetComponent<RevisionSelectionDataModel>();
            animationDataModel = AnimationCanvas.GetComponent<AnimationDataModel>();

            revisionSelectionDataModel.AssertNotNull("revisionSelectionDataModel");
            animationDataModel.AssertNotNull("animationDataModel");

            revisionSelectionDataModel.CloseViewButton.onClick.AddListener(ToogleMode);
            revisionSelectionDataModel.RevisionDropdown.onValueChanged.AddListener(OnDropDownChanged);

            animationDataModel.Slider.minValue = 1;
            animationDataModel.Slider.maxValue = evolutionRenderer.GraphCount-1;
            animationDataModel.Slider.value = evolutionRenderer.CurrentGraphIndex;

            animationDataModel.PlayButton.onClick.AddListener(TaskOnClickPlayButton);
            animationDataModel.FastForwardButton.onClick.AddListener(TaskOnClickFastForwardButton);
            animationDataModel.ReverseButton.onClick.AddListener(TaskOnClickReverseButton);
            animationDataModel.FastBackwardButton.onClick.AddListener(TaskOnClickFastBackwardButton);

            SliderDrag sliderDrag;
            if (animationDataModel.Slider.TryGetComponent<SliderDrag>(out sliderDrag))
            {
                sliderDrag.EvolutionRenderer = evolutionRenderer;
            }
            else
            {
                Debug.LogError("SliderDrag script could not be loaded.\n");
            }

            try
            {
                sliderMarkerContainer = SliderMarkerContainer.Load(Path.Combine(Application.persistentDataPath, "sliderMarkers.xml"));
                
            } catch(FileNotFoundException)
            {
                sliderMarkerContainer = new SliderMarkerContainer();
            }

            foreach (SliderMarker sliderMarker in sliderMarkerContainer.SliderMarkers)
            {
                Vector3 markerPos = new Vector3(sliderMarker.MarkerX, sliderMarker.MarkerY, sliderMarker.MarkerZ);
                string comment = sliderMarker.Comment;
                AddMarker(markerPos, comment);
            }

            SetMode(true);
            OnShownGraphHasChanged();
            evolutionRenderer.Register(OnShownGraphHasChanged);
        }

        /// <summary>
        /// Saves the marker data on application quit
        /// </summary>
        void OnApplicationQuit()
        {
            foreach (KeyValuePair<Button, InputField> p in markerDictionary)
            {
                SliderMarker sliderMarker = sliderMarkerContainer.getSliderMarkerForLocation(p.Key.transform.position);
                sliderMarker.SetComment(p.Value.text);
            }
          
            sliderMarkerContainer.Save(Path.Combine(Application.persistentDataPath, "sliderMarkers.xml"));
        }


        /// <summary>
        /// Handles actions for when the Play/Pause button has been clicked.
        /// </summary>
        private void TaskOnClickPlayButton()
        {
            if (!evolutionRenderer.IsAutoPlayReverse)
            {
                if (isFastBackward)
                {
                    animationTimeValue = 2;
                    evolutionRenderer.AnimationLag = animationTimeValue;
                    isFastBackward = false;
                    animationDataModel.FastBackwardButtonText.text = "◄◄";
                }
                if (!evolutionRenderer.IsAutoPlay)
                {
                    animationDataModel.PlayButtonText.text = "ll";
                    evolutionRenderer.ToggleAutoPlay();
                }
                else
                {
                    animationDataModel.PlayButtonText.text = "►";
                    evolutionRenderer.ToggleAutoPlay();
                }
            }
            
        }

        /// <summary>
        /// Handles actions for when the Reverse/Pause button has been clicked.
        /// </summary>
        private void TaskOnClickReverseButton()
        {
            if (!evolutionRenderer.IsAutoPlay)
            {
                if (isFastForward)
                {
                    animationTimeValue = 2;
                    evolutionRenderer.AnimationLag = animationTimeValue;
                    isFastForward = false;
                    animationDataModel.FastFowardButtonText.text = "►►";
                }
                if (!evolutionRenderer.IsAutoPlayReverse)
                {
                    animationDataModel.ReverseButtonText.text = "ll";
                    evolutionRenderer.ToggleAutoPlayReverse();
                }
                else
                {
                    animationDataModel.ReverseButtonText.text = "◄";
                    evolutionRenderer.ToggleAutoPlayReverse();
                }
            }

        }

        /// <summary>
        /// Handles actions for when the fast forward button has been clicked.
        /// Also resets the fast backward button.
        /// If the animation is playing backwards it does nothing.
        /// </summary>
        private void TaskOnClickFastForwardButton()
        {
            if (evolutionRenderer.IsAutoPlayReverse) 
            {
                return;
            }
            if (isFastBackward) 
            {
                animationTimeValue = 2;
                evolutionRenderer.AnimationLag = animationTimeValue;
                isFastBackward = false;
                animationDataModel.FastBackwardButtonText.text = "◄◄";
            }
            if (animationTimeValue == 2)
            {
                isFastForward = true;
                animationTimeValue = 1;
                evolutionRenderer.AnimationLag = animationTimeValue;
                animationDataModel.FastFowardButtonText.text = "►►2x";
            } else if (animationTimeValue == 1)
            {
                isFastForward = true;
                animationTimeValue = 0.5f;
                evolutionRenderer.AnimationLag = animationTimeValue;
                animationDataModel.FastFowardButtonText.text = "►►4x";
            } else if (animationTimeValue == 0.5f)
            {
                isFastForward = false;
                animationTimeValue = 2;
                evolutionRenderer.AnimationLag = animationTimeValue;
                animationDataModel.FastFowardButtonText.text = "►►";
            }
        }

        /// <summary>
        /// Handles actions for when the fast forward button has been clicked.
        /// If the animation is playing forwards it does nothing.
        /// </summary>
        private void TaskOnClickFastBackwardButton()
        {
            if (evolutionRenderer.IsAutoPlay) 
            {
                return;
            }
            if (isFastForward)
            {
                animationTimeValue = 2;
                evolutionRenderer.AnimationLag = animationTimeValue;
                isFastForward = false;
                animationDataModel.FastFowardButtonText.text = "►►";
            }
            if (animationTimeValue == 2)
            {
                isFastBackward = true;
                animationTimeValue = 1;
                evolutionRenderer.AnimationLag = animationTimeValue;
                animationDataModel.FastBackwardButtonText.text = "◄◄2x";
            }
            else if (animationTimeValue == 1)
            {
                isFastBackward = true;
                animationTimeValue = 0.5f;
                evolutionRenderer.AnimationLag = animationTimeValue;
                animationDataModel.FastBackwardButtonText.text = "◄◄4x";
            }
            else if (animationTimeValue == 0.5f)
            {
                isFastBackward = false;
                animationTimeValue = 2;
                evolutionRenderer.AnimationLag = animationTimeValue;
                animationDataModel.FastBackwardButtonText.text = "◄◄";
            }
        }

        /// <summary>
        /// Handles actions for when a marker is clicked.
        /// </summary>
        /// <param name="clickedMarker"> Marker that has been clicked. </param>
        private void TaskOnClickMarker(Button clickedMarker)
        {
            selectedMarker = clickedMarker;
            string commentName = clickedMarker.GetHashCode().ToString() + "-comment";
            if (clickedMarker.transform.Find(commentName) != null)
            {
                GameObject comment = clickedMarker.transform.Find(commentName).gameObject;
                comment.SetActive(!comment.activeSelf);
            }
        }

        /// <summary>
        /// Adds an InputField to enter comments to the specified marker.
        /// </summary>
        /// <param name="marker"> Marker </param>
        /// <param name="comment"> comment to be added to the InputField, optional </param>
        /// <returns> Created InputField </returns>
        private InputField AddCommentToMarker(Button marker, string comment = null)
        {
            string commentName = marker.GetHashCode().ToString() + "-comment";
            InputField commentField = Instantiate(animationDataModel.CommentPrefab);
            Vector3 commentPos = new Vector3(1500f, 0, 0);
            commentField.transform.SetParent(marker.transform, false);
            commentField.transform.localScale = new Vector3(16f, 1f, 1f);
            commentField.transform.localPosition = commentPos;
            commentField.name = commentName;
            if (comment != null) 
            {
                commentField.text = comment;
            }
            markerDictionary.Add(marker, commentField);
            return commentField;
        }


        /// <summary>
        /// Adds a new marker at the specified position
        /// </summary>
        /// <param name="markerPos"> Position to add the marker at </param>
        /// <param name="comment"> Comment to be added to the marker, optional </param>
        private void AddMarker(Vector3 markerPos, string comment = null)
        {
            Button newMarker = Instantiate(animationDataModel.MarkerPrefab);
            newMarker.transform.SetParent(animationDataModel.Slider.transform, false);
            newMarker.transform.position = markerPos;
            newMarker.onClick.AddListener(() => TaskOnClickMarker(newMarker));
            if (sliderMarkerContainer.getSliderMarkerForLocation(markerPos) == null)
            {
                SliderMarker newSliderMarker = new SliderMarker();
                newSliderMarker.MarkerX = markerPos.x;
                newSliderMarker.MarkerY = markerPos.y;
                newSliderMarker.MarkerZ = markerPos.z;
                sliderMarkerContainer.SliderMarkers.Add(newSliderMarker);
            }
            InputField commentField = AddCommentToMarker(newMarker, comment);
            commentField.gameObject.SetActive(false);      
        }

        /// <summary>
        /// Removes the specified marker
        /// </summary>
        /// <param name="marker"> Marker to remove </param>
        private void RemoveMarker(Button marker)
        {
            SliderMarker sliderMarker = sliderMarkerContainer.getSliderMarkerForLocation(marker.transform.position);
            sliderMarkerContainer.SliderMarkers.Remove(sliderMarker);
            InputField comment = markerDictionary[marker];
            markerDictionary.Remove(marker);
            GameObject.Destroy(comment.gameObject);
            GameObject.Destroy(marker.gameObject);
        }

        /// <summary>
        /// Handles the user input as follows:
        ///   k   => previous graph revision is shown
        ///   l   => next graph revision is shown
        ///   m   => create new marker
        ///   tab => auto-play mode is toggled
        ///   del => delete selected marker
        ///   0-9 => the time in between two revisions in auto-play mode is adjusted
        ///   ESC => toggle between the two canvases AnimationCanvas and RevisionSelectionCanvas
        /// </summary>
        private void Update()
        {
            if (!IsRevisionSelectionOpen)
            {
                if (Input.GetKeyDown("k"))
                {
                    evolutionRenderer.ShowPreviousGraph();
                }
                else if (Input.GetKeyDown("l"))
                {
                    evolutionRenderer.ShowNextGraph();
                }
                else if (Input.GetKeyDown(KeyCode.Tab))
                {
                    evolutionRenderer.ToggleAutoPlay();
                } else if (Input.GetKeyDown("m"))
                {
                    Vector3 handlePos = animationDataModel.Slider.handleRect.transform.position;
                    Vector3 markerPos = new Vector3(handlePos.x, handlePos.y + .08f, handlePos.z);
                    if (sliderMarkerContainer.getSliderMarkerForLocation(markerPos) != null) 
                    { 
                        return;
                    }
                    AddMarker(markerPos, null);
                } else if (Input.GetKeyDown(KeyCode.Delete))
                {
                    RemoveMarker(selectedMarker);
                }

                string[] animationTimeKeys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                float[] animationTimeValues = { 0.1f, 0.5f, 1, 2, 3, 4, 5, 8, 16, 0 };
                for (int i = 0; i < animationTimeKeys.Length; i++)
                {
                    if (Input.GetKeyDown(animationTimeKeys[i]))
                    {
                        evolutionRenderer.AnimationLag = animationTimeValues[i];
                    }
                }

            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToogleMode();
            }
        }

        /// <summary>
        /// Toggles between the animation-interaction mode and the revision-selection
        /// mode. 
        /// In the animation-interaction mode, the user can see and control
        /// the animations of the graph revisions through the AnimationCanvas
        /// and freely move in the city. 
        /// In the revision-selection mode, the user can select the revision to be shown
        /// through the RevisionSelectionCanvas. No movement is possible in that mode.
        /// </summary>
        private void ToogleMode()
        {
            SetMode(!isRevisionSelectionOpen);
        }

        /// <summary>
        /// Toggles between the animation-interaction mode and the revision-selection
        /// mode. If <paramref name="enabled"/> is true, the revision-selection mode
        /// is activated; otherwise the animation-interaction mode is turned on.
        /// 
        /// In the revision-selection mode, the user can select the revision to be shown
        /// through the RevisionSelectionCanvas. No movement is possible in that mode.
        /// 
        /// In the animation-interaction mode, the user can see and control
        /// the animations of the graph revisions through the AnimationCanvas
        /// and freely move in the city. 
        /// 
        /// Both modes are mutually exclusive.
        /// 
        /// Auto-play animation is always turned off independent of <paramref name="enabled"/>.
        /// </summary>
        /// <param name="enabled">if true, revision-selection mode is turned on; otherwise
        /// animation-interaction mode is turned on</param>
        private void SetMode(bool enabled)
        {
            isRevisionSelectionOpen = enabled;

            AnimationCanvas.SetActive(!isRevisionSelectionOpen);
            RevisionSelectionCanvas.SetActive(isRevisionSelectionOpen);
            evolutionRenderer.SetAutoPlay(false);
            if (isRevisionSelectionOpen)
            {
                // if revision-selection mode is enabled, we re-fill the drop-down
                // selection menu with all available graph indices.
                revisionSelectionDataModel.RevisionDropdown.ClearOptions();
                System.Collections.Generic.List<Dropdown.OptionData> options = Enumerable
                    .Range(1, evolutionRenderer.GraphCount)
                    .Select(i => new Dropdown.OptionData(i.ToString()))
                    .ToList();
                revisionSelectionDataModel.RevisionDropdown.AddOptions(options);
                revisionSelectionDataModel.RevisionDropdown.value = evolutionRenderer.CurrentGraphIndex;
            }
        }

        /// <summary>
        /// Event function that updates all shown data for the user;
        /// e.g. the revision number shown in the animation canvas.
        /// This method is called as a callback of the evolution renderer
        /// when any of the graph data have changed.
        /// </summary>
        private void OnShownGraphHasChanged()
        {
            animationDataModel.RevisionNumberText.text = (evolutionRenderer.CurrentGraphIndex + 1) + " / " + evolutionRenderer.GraphCount;
            animationDataModel.Slider.value = evolutionRenderer.CurrentGraphIndex;
        }

        /// <summary>
        /// Event function that changes the shown revision to the given value index.
        /// This method is called as a callback when the user selects an entry in
        /// the RevisionDropdown box.
        /// </summary>
        /// <param name="value">the revision index selected from the drop-down box</param>
        private void OnDropDownChanged(int value)
        {
            if (value != evolutionRenderer.CurrentGraphIndex)
            {
                evolutionRenderer.TryShowSpecificGraph(value);
            }
        }
    }
}
