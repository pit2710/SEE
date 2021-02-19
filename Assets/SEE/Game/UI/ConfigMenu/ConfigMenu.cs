﻿using System;
using System.Collections.Generic;
using System.Linq;
using SEE.DataModel.DG;
using SEE.GO;
using UnityEngine;

namespace SEE.Game.UI.ConfigMenu
{
    enum TabButtonState
    {
        InitialActive,
        Inactive,
    }

    public class ConfigMenu : DynamicUIBehaviour
    {
        private static List<string> _numericAttributes =
            Enum.GetValues(typeof(NumericAttributeNames))
                .Cast<NumericAttributeNames>()
                .Select(x => x.Name())
                .ToList();

        private const string PagePrefabPath = "Assets/Prefabs/UI/Page.prefab";
        private const string TabButtonPrefabPath = "Assets/Prefabs/UI/TabButton.prefab";

        private const string ComboSelectPrefabPath =
            "Assets/Prefabs/UI/Input Group - Dropdown.prefab";
        private const string ColorPickerPrefabPath =
            "Assets/Prefabs/UI/Input Group - Color Picker.prefab";

        private GameObject _pagePrefab;
        private GameObject _tabButtonPrefab;
        private GameObject _comboSelectPrefab;
        private GameObject _colorPickerPrefab;

        private GameObject _tabOutlet;
        private GameObject _tabGroup;

        private SEECity _city;
        private ColorPickerControl _colorPickerControl;

        private void Start()
        {
            GameObject.Find("Implementation")?.MustGetComponent(_city);

            MustGetChild("Canvas/TabNavigation/TabOutlet", out _tabOutlet);
            MustGetChild("Canvas/TabNavigation/TabGroup", out _tabGroup);

            MustGetComponentInChild("Canvas/Picker 2.0", out _colorPickerControl);
            _colorPickerControl.gameObject.SetActive(false);

            // Reset (hide) the color picker on page changes.
            _tabGroup.MustGetComponent(out TabGroup tabGroupController);
            tabGroupController.SubscribeToUpdates(_colorPickerControl.Reset);

            LoadPrefabs();
            SetupPages();
        }

        private void LoadPrefabs()
        {
            _tabButtonPrefab = MustLoadPrefabAtPath(TabButtonPrefabPath);
            _pagePrefab = MustLoadPrefabAtPath(PagePrefabPath);
            _comboSelectPrefab = MustLoadPrefabAtPath(ComboSelectPrefabPath);
            _colorPickerPrefab = MustLoadPrefabAtPath(ColorPickerPrefabPath);
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
            GameObject page = CreateAndInsertPage("Attributes of leaf nodes");
            Transform controls = page.transform.Find("ControlsViewport/ControlsContent");

            // Width metric
            GameObject widthMetricHost =
                Instantiate(_comboSelectPrefab, controls);
            ComboSelectBuilder.Init(widthMetricHost)
                .SetLabel("Width")
                .SetAllowedValues(_numericAttributes)
                .SetDefaultValue(_city.WidthMetric)
                .SetOnChangeHandler(s => _city.WidthMetric = s)
                .Build();

            // Height metric
            GameObject heightMetricHost =
                Instantiate(_comboSelectPrefab, controls);
            ComboSelectBuilder.Init(heightMetricHost)
                .SetLabel("Height")
                .SetAllowedValues(_numericAttributes)
                .SetDefaultValue(_city.HeightMetric)
                .SetOnChangeHandler(s => _city.HeightMetric = s)
                .Build();

            // Height metric
            GameObject depthMetricHost =
                Instantiate(_comboSelectPrefab, controls);
            ComboSelectBuilder.Init(depthMetricHost)
                .SetLabel("Depth")
                .SetAllowedValues(_numericAttributes)
                .SetDefaultValue(_city.DepthMetric)
                .SetOnChangeHandler(s => _city.DepthMetric = s)
                .Build();

            // Leaf style metric
            GameObject leafStyleMetricHost =
                Instantiate(_comboSelectPrefab, controls);
            ComboSelectBuilder.Init(leafStyleMetricHost)
                .SetLabel("Style")
                .SetAllowedValues(_numericAttributes)
                .SetDefaultValue(_city.LeafStyleMetric)
                .SetOnChangeHandler(s => _city.LeafStyleMetric = s)
                .Build();

            // Lower color
            GameObject lowerColorHost =
                Instantiate(_colorPickerPrefab, controls);
            ColorPickerBuilder.Init(lowerColorHost)
                .SetLabel("Lower Color")
                .SetDefaultValue(_city.LeafNodeColorRange.lower)
                .SetOnChangeHandler(c => _city.LeafNodeColorRange.lower = c)
                .SetColorPickerControl(_colorPickerControl)
                .Build();

            // Upper color
            GameObject upperColorHost =
                Instantiate(_colorPickerPrefab, controls);
            ColorPickerBuilder.Init(upperColorHost)
                .SetLabel("Upper Color")
                .SetDefaultValue(_city.LeafNodeColorRange.upper)
                .SetOnChangeHandler(c => _city.LeafNodeColorRange.upper = c)
                .SetColorPickerControl(_colorPickerControl)
                .Build();
        }

        private void SetupInnerNodesPage()
        {
            CreateAndInsertTabButton("Inner nodes");
            CreateAndInsertPage("Attributes of inner nodes");
        }

        private void SetupNodesLayoutPage()
        {
            CreateAndInsertTabButton("Nodes layout");
            CreateAndInsertPage("Nodes and node layout");
        }

        private void SetupEdgesLayoutPage()
        {
            CreateAndInsertTabButton("Edges layout");
            CreateAndInsertPage("Edges and edge layout");
        }

        private void SetupMiscellaneousPage()
        {
            CreateAndInsertTabButton("Miscellaneous");
            CreateAndInsertPage("Miscellaneous");
        }

        private GameObject CreateAndInsertPage(string headline)
        {
            GameObject page = Instantiate(_pagePrefab, _tabOutlet.transform, false);
            page.MustGetComponent(out PageController pageController);
            pageController.headlineText = headline;
            return page;
        }

        private void CreateAndInsertTabButton(string label,
                                              TabButtonState initialState = TabButtonState.Inactive)
        {
            GameObject tabButton = Instantiate(_tabButtonPrefab, _tabGroup.transform, false);
            tabButton.MustGetComponent(out TabButton button);
            button.buttonText = label;
            if (initialState == TabButtonState.InitialActive)
            {
                button.isDefaultActive = true;
            }
        }
    }
}
