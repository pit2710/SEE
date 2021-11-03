﻿// Copyright 2021 Ruben Smidt
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Michsky.UI.ModernUIPack;
using SEE.Controls;
using SEE.DataModel.DG;
using SEE.GO;
using SEE.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SEE.Game.UI.ConfigMenu
{
    enum TabButtonState
    {
        InitialActive,
        Inactive,
    }

    /// <summary>
    /// The primary wrapper script for the config menu prefab. The config menu allows for runtime
    /// configuration of a SEECity instance. It's agnostic about what instance can be manipulated and
    /// offers an easy way to extend the list of instances that can be accessed.
    ///
    /// This script instantiates almost all of its used game objects.
    /// </summary>
    public class ConfigMenu : DynamicUIBehaviour
    {
        /// <summary>
        /// A list of numeric attributes digestible by the ComboSelect component.
        ///
        /// FIXME: NumericAttributes is just a list of predefined metric names. They
        /// may or may not exist in the graph. We need to retrieve the set of metrics
        /// from the graph.
        /// </summary>
        private static readonly List<string> NumericAttributes =
            Enum.GetValues(typeof(NumericAttributeNames))
                .Cast<NumericAttributeNames>()
                .Select(x => x.Name())
                .ToList();

        /// <summary>
        /// The list of SEECity instances this menu can manipulate.
        /// </summary>
        private static readonly List<EditableInstance> EditableInstances =
            new List<EditableInstance>
            {
                EditableInstance.Architecture,
                EditableInstance.Implementation
            };

        private const string PagePrefabPath = "Prefabs/UI/Page";
        private const string TabButtonPrefabPath = "Prefabs/UI/TabButton";
        private const string ActionButtonPrefabPath = "Prefabs/UI/ActionButton";
        private const string PointerPrefabPath = "Prefabs/UI/Pointer";

        private GameObject pagePrefab;
        private GameObject actionButtonPrefab;
        private GameObject tabButtonPrefab;

        private GameObject tabOutlet;
        private GameObject tabButtons;
        private GameObject actions;

        private SEECity city;
        private ColorPickerControl colorPickerControl;
        private ButtonManager cityLoadButton;
        private Canvas canvas;
        private HorizontalSelector editingInstanceSelector;

        /// <summary>
        /// The event handler that gets called when a user interaction changes the currently edited
        /// SEECity instance.
        /// </summary>
        public UnityEvent<EditableInstance> OnInstanceChangeRequest = new UnityEvent<EditableInstance>();

        /// <summary>
        /// The currently edited SEECity instance.
        /// </summary>
        public EditableInstance CurrentlyEditing = EditableInstance.Implementation;

        private void Start()
        {
            SetupCity(CurrentlyEditing);
            MustGetChild("Canvas/TabNavigation/TabOutlet", out tabOutlet);
            MustGetChild("Canvas/TabNavigation/Sidebar/TabButtons", out tabButtons);
            MustGetChild("Canvas/Actions", out actions);
            MustGetComponentInChild("Canvas", out canvas);
            // initially the canvas should be inactive; it can be activated by the user on demand
            Off();
            MustGetComponentInChild("Canvas/Picker 2.0", out colorPickerControl);
            colorPickerControl.gameObject.SetActive(false);

            // Reset (hide) the color picker on page changes.
            tabButtons.MustGetComponent(out TabGroup tabGroupController);
            tabGroupController.SubscribeToUpdates(colorPickerControl.Reset);

            MustGetComponentInChild("Canvas/TabNavigation/Sidebar/CityLoadButton", out cityLoadButton);
            cityLoadButton.clickEvent.AddListener(() =>
            {
                city.LoadData();
                actions.SetActive(true);
                cityLoadButton.gameObject.SetActive(false);
            });

            SetupInstanceSwitch();
            SetupEnvironment();
            LoadPrefabs();
            SetupActions();
            SetupPages();
        }

        private void SetupCity(EditableInstance instanceToEdit)
        {
            // FIXME: Find should be avoided.
            GameObject instanceGameObject = GameObject.Find(instanceToEdit.GameObjectName);
            if (instanceGameObject != null)
            {
                instanceGameObject.MustGetComponent(out city);
            }
            else
            {
                Debug.LogError("Did not find a city instance.\n");
            }
        }

        private void SetupInstanceSwitch()
        {
            MustGetComponentInChild("Canvas/TabNavigation/Sidebar/CitySwitch",
                                    out editingInstanceSelector);
            editingInstanceSelector.itemList.Clear();
            EditableInstances.ForEach(instance =>
                                          editingInstanceSelector.CreateNewItem(
                                              instance.DisplayValue));
            editingInstanceSelector.defaultIndex = EditableInstances.IndexOf(CurrentlyEditing);
            editingInstanceSelector.SetupSelector();
            editingInstanceSelector.selectorEvent.AddListener(index =>
            {
                string displayValue = editingInstanceSelector.itemList[index].itemTitle;
                EditableInstance newInstance =
                    EditableInstances.Find(instance => instance.DisplayValue == displayValue);
                OnInstanceChangeRequest.Invoke(newInstance);
            });
        }

        private void SetupEnvironment()
        {
            if (PlayerSettings.GetInputType() == PlayerInputType.VRPlayer)
            {
                // Attach the pointer to the appropriate hand.
                Transform attachmentPoint = GameObject
                    .Find("VRPlayer/SteamVRObjects/RightHand/ObjectAttachmentPoint").transform;
                GameObject pointer =
                    PrefabInstantiator.InstantiatePrefab(PointerPrefabPath, parent: attachmentPoint);
                Camera pointerCamera = pointer.GetComponent<Camera>();

                // Replace the default input system with our VR input system.
                GameObject vrEventSystem = GameObject.FindWithTag("VREventSystem");
                vrEventSystem.GetComponent<StandaloneInputModule>().enabled = false;
                VRInputModule vrInputModule = vrEventSystem.AddComponent<VRInputModule>();
                vrInputModule.PointerCamera = pointerCamera;
                pointer.GetComponent<Pointer>().InputModule = vrInputModule;

                // Set the canvas to world space and adjust its positition.
                MustGetComponentInChild("Canvas", out RectTransform rectTransform);
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = pointerCamera;
                rectTransform.anchoredPosition3D = Vector3.zero;
                rectTransform.localScale = Vector3.one;

                // Make the color picker slightly rotated towards the user.
                colorPickerControl.gameObject.transform.Rotate(0f, 45f, 0f);

                // Place the menu as a whole in front of the 'table'.
                // FIXME: Do not use absolute positioning. Instead, it may better to use
                // positioning relative to the table or to the player, since the absolute
                // position of the table may change in the future (or on other platforms,
                // like it does on the HoloLens).
                gameObject.transform.position = new Vector3(-0.36f, 1.692f, -0.634f);
            }
        }

        private void LoadPrefabs()
        {
            tabButtonPrefab = PrefabInstantiator.LoadPrefab(TabButtonPrefabPath);
            pagePrefab = PrefabInstantiator.LoadPrefab(PagePrefabPath);
            actionButtonPrefab = PrefabInstantiator.LoadPrefab(ActionButtonPrefabPath);
        }
        private void SetupActions()
        {
            actions.SetActive(false);
            CreateActionButton("Delete Graph", () =>
            {
                city.Reset();
                cityLoadButton.gameObject.SetActive(true);
            });
            CreateActionButton("Save Graph", city.Save);
            CreateActionButton("Draw", () =>
            {
                city.DrawGraph();
                Toggle();
            });
            CreateActionButton("Re-Draw", city.ReDrawGraph);
            CreateActionButton("Save layout", city.SaveLayout);
            CreateActionButton("Add References", city.SetNodeEdgeRefs);
        }

        private void CreateActionButton(string buttonText, UnityAction onClick)
        {
            GameObject deleteGraphButtonGo = Instantiate(actionButtonPrefab, actions.transform, false);
            deleteGraphButtonGo.MustGetComponent(out ButtonManagerBasic deleteGraphButton);
            deleteGraphButton.buttonText = buttonText;
            deleteGraphButton.clickEvent.AddListener(onClick);
        }

        private void SetupPages()
        {
            SetupLeafNodesPage();
            SetupInnerNodesPage();
            SetupNodesLayoutPage();
            SetupEdgesLayoutPage();
            SetupMiscellaneousPage();
        }

        private void SetupLeafNodesPage()
        {
            CreateAndInsertTabButton("Leaf nodes", TabButtonState.InitialActive);
            GameObject page = CreateAndInsertPage("Leaf nodes");
            Transform controls = page.transform.Find("ControlsViewport/ControlsContent");

            // FIXME: We can edit only the Unspecified domain. The concept of domains will
            // be dropped soon. So we will not invest any effort in this for the time being.
            LeafNodeAttributes leafNodeAttributes = city.LeafNodeSettings;
            {
                // Shape type for leaf nodes
                ComboSelectBuilder.Init(controls.transform)
                    .SetLabel("Shape")
                    .SetAllowedValues(EnumToStr<LeafNodeKinds>())
                    .SetDefaultValue(leafNodeAttributes.Kind.ToString())
                    .SetOnChangeHandler(s => Enum.TryParse(s, out leafNodeAttributes.Kind))
                    .SetComboSelectMode(ComboSelectMode.Restricted)
                    .Build();

                // Width metric
                ComboSelectBuilder.Init(controls.transform)
                    .SetLabel("Width")
                    .SetAllowedValues(NumericAttributes)
                    .SetDefaultValue(leafNodeAttributes.WidthMetric)
                    .SetOnChangeHandler(s => leafNodeAttributes.WidthMetric = s)
                    .Build();

                // Height metric
                ComboSelectBuilder.Init(controls.transform)
                    .SetLabel("Height")
                    .SetAllowedValues(NumericAttributes)
                    .SetDefaultValue(leafNodeAttributes.HeightMetric)
                    .SetOnChangeHandler(s => leafNodeAttributes.HeightMetric = s)
                    .Build();

                // Height metric
                ComboSelectBuilder.Init(controls.transform)
                    .SetLabel("Depth")
                    .SetAllowedValues(NumericAttributes)
                    .SetDefaultValue(leafNodeAttributes.DepthMetric)
                    .SetOnChangeHandler(s => leafNodeAttributes.DepthMetric = s)
                    .Build();

                // Leaf style metric
                ComboSelectBuilder.Init(controls.transform)
                    .SetLabel("Style")
                    .SetAllowedValues(NumericAttributes)
                    .SetDefaultValue(leafNodeAttributes.ColorMetric)
                    .SetOnChangeHandler(s => leafNodeAttributes.ColorMetric = s)
                    .Build();

                // Lower color
                ColorPickerBuilder.Init(controls.transform)
                    .SetLabel("Lower color")
                    .SetDefaultValue(leafNodeAttributes.ColorRange.lower)
                    .SetOnChangeHandler(c => leafNodeAttributes.ColorRange.lower = c)
                    .SetColorPickerControl(colorPickerControl)
                    .Build();

                // Upper color
                ColorPickerBuilder.Init(controls.transform)
                    .SetLabel("Upper color")
                    .SetDefaultValue(leafNodeAttributes.ColorRange.upper)
                    .SetOnChangeHandler(c => leafNodeAttributes.ColorRange.upper = c)
                    .SetColorPickerControl(colorPickerControl)
                    .Build();

                // Number of colors
                SliderBuilder.Init(controls.transform)
                    .SetLabel("# Colors")
                    .SetMode(SliderMode.Integer)
                    .SetDefaultValue(leafNodeAttributes.ColorRange.NumberOfColors)
                    .SetOnChangeHandler(f => leafNodeAttributes.ColorRange.NumberOfColors =
                                            (uint)Math.Round(f))
                    .SetRange((0, 15))
                    .Build();

                CreateLabelSettingsInputs(controls, leafNodeAttributes.LabelSettings);
            }
        }

        private void SetupInnerNodesPage()
        {
            CreateAndInsertTabButton("Inner nodes");
            GameObject page = CreateAndInsertPage("Inner nodes");
            Transform controls = page.transform.Find("ControlsViewport/ControlsContent");

            // FIXME: We can edit only the Unspecified domain. The concept of domains will
            // be dropped soon. So we will not invest any effort in this for the time being.
            InnerNodeAttributes innerNodeAttributes = city.InnerNodeSettings;
            {
                // Shape type for inner nodes
                ComboSelectBuilder.Init(controls.transform)
                    .SetLabel("Shape")
                    .SetAllowedValues(EnumToStr<InnerNodeKinds>())
                    .SetDefaultValue(innerNodeAttributes.Kind.ToString())
                    .SetOnChangeHandler(s => Enum.TryParse(s, out innerNodeAttributes.Kind))
                    .SetComboSelectMode(ComboSelectMode.Restricted)
                    .Build();

                // Height metric
                ComboSelectBuilder.Init(controls.transform)
                    .SetLabel("Height")
                    .SetAllowedValues(NumericAttributes)
                    .SetDefaultValue(innerNodeAttributes.HeightMetric)
                    .SetOnChangeHandler(s => innerNodeAttributes.HeightMetric = s)
                    .Build();

                // Leaf style metric
                ComboSelectBuilder.Init(controls.transform)
                    .SetLabel("Style")
                    .SetAllowedValues(NumericAttributes)
                    .SetDefaultValue(innerNodeAttributes.ColorMetric)
                    .SetOnChangeHandler(s => innerNodeAttributes.ColorMetric = s)
                    .Build();

                // Lower color
                ColorPickerBuilder.Init(controls.transform)
                    .SetLabel("Lower color")
                    .SetDefaultValue(innerNodeAttributes.ColorRange.lower)
                    .SetOnChangeHandler(c => innerNodeAttributes.ColorRange.lower = c)
                    .SetColorPickerControl(colorPickerControl)
                    .Build();

                // Upper color
                ColorPickerBuilder.Init(controls.transform)
                    .SetLabel("Upper color")
                    .SetDefaultValue(innerNodeAttributes.ColorRange.upper)
                    .SetOnChangeHandler(c => innerNodeAttributes.ColorRange.upper = c)
                    .SetColorPickerControl(colorPickerControl)
                    .Build();

                // Number of colors
                SliderBuilder.Init(controls.transform)
                    .SetLabel("# Colors")
                    .SetMode(SliderMode.Integer)
                    .SetDefaultValue(innerNodeAttributes.ColorRange.NumberOfColors)
                    .SetOnChangeHandler(f => innerNodeAttributes.ColorRange.NumberOfColors =
                                            (uint)Math.Round(f))
                    .SetRange((0, 15))
                    .Build();

                CreateLabelSettingsInputs(controls, innerNodeAttributes.LabelSettings);
            }
        }

        private void CreateLabelSettingsInputs(Transform parent, LabelAttributes labelSettings)
        {
            // Show labels
            SwitchBuilder.Init(parent)
                .SetLabel("Show labels")
                .SetDefaultValue(labelSettings.Show)
                .SetOnChangeHandler(b => labelSettings.Show = b)
                .Build();

            // Label distance
            SliderBuilder.Init(parent)
                .SetLabel("Label distance")
                .SetMode(SliderMode.Float)
                .SetDefaultValue(labelSettings.Distance)
                .SetOnChangeHandler(f => labelSettings.Distance = f)
                .SetRange((0, 2))
                .Build();

            // Label font size
            SliderBuilder.Init(parent)
                .SetLabel("Label font size")
                .SetMode(SliderMode.Float)
                .SetDefaultValue(labelSettings.FontSize)
                .SetOnChangeHandler(f => labelSettings.FontSize = f)
                .SetRange((0, 2))
                .Build();

            // Label animation duration
            SliderBuilder.Init(parent)
                .SetLabel("Label anim. duration")
                .SetMode(SliderMode.Float)
                .SetDefaultValue(labelSettings.AnimationDuration)
                .SetOnChangeHandler(f => labelSettings.AnimationDuration = f)
                .SetRange((0, 2))
                .Build();
        }

        private void SetupNodesLayoutPage()
        {
            CreateAndInsertTabButton("Nodes layout");
            GameObject page = CreateAndInsertPage("Nodes and node layout");
            Transform controls = page.transform.Find("ControlsViewport/ControlsContent");

            // Node layout
            ComboSelectBuilder.Init(controls.transform)
                .SetLabel("Node layout")
                .SetAllowedValues(EnumToStr<NodeLayoutKind>())
                .SetDefaultValue(city.NodeLayoutSettings.Kind.ToString())
                .SetOnChangeHandler(s => Enum.TryParse(s, out city.NodeLayoutSettings.Kind))
                .SetComboSelectMode(ComboSelectMode.Restricted)
                .Build();

            // Layout file
            FilePickerBuilder.Init(controls.transform)
                .SetLabel("Layout file")
                .SetPathInstance(city.NodeLayoutSettings.LayoutPath)
                .Build();

            // Z-score scaling
            SwitchBuilder.Init(controls.transform)
                .SetLabel("Z-score scaling")
                .SetDefaultValue(city.ZScoreScale)
                .SetOnChangeHandler(b => city.ZScoreScale = b)
                .Build();

            // Leaf/inner node metric scaling
            SwitchBuilder.Init(controls.transform)
                .SetLabel("Scale only leaf metrics")
                .SetDefaultValue(city.ScaleOnlyLeafMetrics)
                .SetOnChangeHandler(b => city.ScaleOnlyLeafMetrics = b)
                .Build();

            // Show leaf erosions
            SwitchBuilder.Init(controls.transform)
                .SetLabel("Show leaf erosions")
                .SetDefaultValue(city.ErosionSettings.ShowLeafErosions)
                .SetOnChangeHandler(b => city.ErosionSettings.ShowLeafErosions = b)
                .Build();

            // Show inner erosions
            SwitchBuilder.Init(controls.transform)
                .SetLabel("Show inner erosions")
                .SetDefaultValue(city.ErosionSettings.ShowInnerErosions)
                .SetOnChangeHandler(b => city.ErosionSettings.ShowInnerErosions = b)
                .Build();

            // loadDashboardMetrics
            SwitchBuilder.Init(controls.transform)
                .SetLabel("Load dashboard metrics")
                .SetDefaultValue(city.ErosionSettings.LoadDashboardMetrics)
                .SetOnChangeHandler(b => city.ErosionSettings.LoadDashboardMetrics = b)
                .Build();

            // FIXME: Provide an configuration input for city.nodeLayoutSettings.issuesAddedFromVersion.
            // Apparently, there is no string input field.

            // overrideMetrics
            SwitchBuilder.Init(controls.transform)
                .SetLabel("Dashboard metrics override")
                .SetDefaultValue(city.ErosionSettings.OverrideMetrics)
                .SetOnChangeHandler(b => city.ErosionSettings.OverrideMetrics = b)
                .Build();

            // Erosion scaling factor
            SliderBuilder.Init(controls.transform)
                .SetLabel("Erosion scaling factor")
                .SetMode(SliderMode.Float)
                .SetDefaultValue(city.ErosionSettings.ErosionScalingFactor)
                .SetOnChangeHandler(f => city.ErosionSettings.ErosionScalingFactor = f)
                .SetRange((1, 10))
                .Build();
        }

        private void SetupEdgesLayoutPage()
        {
            CreateAndInsertTabButton("Edges layout");
            GameObject page = CreateAndInsertPage("Edges and edge layout");
            Transform controls = page.transform.Find("ControlsViewport/ControlsContent");

            // Edge layout
            ComboSelectBuilder.Init(controls.transform)
                .SetLabel("Edge layout")
                .SetAllowedValues(EnumToStr<EdgeLayoutKind>())
                .SetDefaultValue(city.EdgeLayoutSettings.Kind.ToString())
                .SetOnChangeHandler(s => Enum.TryParse(s, out city.EdgeLayoutSettings.Kind))
                .SetComboSelectMode(ComboSelectMode.Restricted)
                .Build();

            // Edge width
            SliderBuilder.Init(controls.transform)
                .SetLabel("Edge width")
                .SetMode(SliderMode.Float)
                .SetDefaultValue(city.EdgeLayoutSettings.EdgeWidth)
                .SetOnChangeHandler(f => city.EdgeLayoutSettings.EdgeWidth = f)
                .SetRange((0, 0.5f))
                .Build();

            // Edges above block
            SwitchBuilder.Init(controls.transform)
                .SetLabel("Edges above block")
                .SetDefaultValue(city.EdgeLayoutSettings.EdgesAboveBlocks)
                .SetOnChangeHandler(b => city.EdgeLayoutSettings.EdgesAboveBlocks = b)
                .Build();

            // Bundling tension
            SliderBuilder.Init(controls.transform)
                .SetLabel("Bundling tension")
                .SetMode(SliderMode.Float)
                .SetDefaultValue(city.EdgeLayoutSettings.Tension)
                .SetOnChangeHandler(f => city.EdgeLayoutSettings.Tension = f)
                .SetRange((0, 1))
                .Build();

            // TODO: rdp
        }

        private void SetupMiscellaneousPage()
        {
            CreateAndInsertTabButton("Miscellaneous");
            GameObject page = CreateAndInsertPage("Miscellaneous");
            Transform controls = page.transform.Find("ControlsViewport/ControlsContent");

            // Settings file
            FilePickerBuilder.Init(controls.transform)
                .SetLabel("Settings file")
                .SetPathInstance(city.CityPath)
                .Build();

            // LOD culling
            SliderBuilder.Init(controls.transform)
                .SetLabel("LOD culling")
                .SetMode(SliderMode.Float)
                .SetRange((0f, 1f))
                .SetDefaultValue(city.LODCulling)
                .SetOnChangeHandler(f => city.LODCulling = f);

            // GXL file
            FilePickerBuilder.Init(controls.transform)
                .SetLabel("GXL file")
                .SetPathInstance(city.GXLPath)
                .Build();

            // Metric file
            FilePickerBuilder.Init(controls.transform)
                .SetLabel("Metric file")
                .SetPathInstance(city.CSVPath)
                .Build();
        }

        private GameObject CreateAndInsertPage(string headline)
        {
            GameObject page = Instantiate(pagePrefab, tabOutlet.transform, false);
            page.MustGetComponent(out PageController pageController);
            pageController.HeadlineText = headline;
            return page;
        }

        private void CreateAndInsertTabButton(string label,
                                              TabButtonState initialState = TabButtonState.Inactive)
        {
            GameObject tabButton = Instantiate(tabButtonPrefab, tabButtons.transform, false);
            tabButton.name = $"{label}Button";
            tabButton.MustGetComponent(out TabButton button);
            button.ButtonText = label;
            if (initialState == TabButtonState.InitialActive)
            {
                button.IsDefaultActive = true;
            }
        }

        /// <summary>
        /// Converts an enum to a list of strings.
        /// </summary>
        /// <typeparam name="EnumType">The enum to map.</typeparam>
        /// <returns>a list of string representations of the enum.</returns>
        public static List<string> EnumToStr<EnumType>() where EnumType : Enum
        {
            return Enum.GetValues(typeof(EnumType))
                .Cast<EnumType>()
                .Select(v => v.ToString())
                .ToList();
        }

        /// <summary>
        /// Toggles the visibility of the menu.
        /// </summary>
        public void Toggle()
        {
            canvas.gameObject.SetActive(!canvas.gameObject.activeSelf);
        }

        /// <summary>
        /// Turns configuration menu off.
        /// </summary>
        public void Off()
        {
            canvas.gameObject.SetActive(false);
        }

        /// <summary>
        /// Turns configuration menu on.
        /// </summary>
        public void On()
        {
            canvas.gameObject.SetActive(true);
        }
    }
}
