﻿using SEE;
using SEE.DataModel;
using SEE.Layout;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The CCAnimation manages user inputs and interfaces.
/// </summary>
public class CCAnimation : MonoBehaviour
{
    /// <summary>
    /// The camera from the user.
    /// </summary>
    public FlyCamera FlyCamera;

    /// <summary>
    /// The in game seen while viewing the animations.
    /// </summary>
    public GameObject InGameCanvas;

    /// <summary>
    /// The menu for selecting the shown revision.
    /// </summary>
    public GameObject MainMenuCanvas;

    /// <summary>
    /// The viewmodel for InGameCanvas.
    /// </summary>
    private InGameMenuModel inGameMenu;

    /// <summary>
    /// The viewmodel for MainMenuCanvas
    /// </summary>
    private MainMenuModel mainMenu;

    /// <summary>
    /// The StateManager containing all necessary components for controlling the animations.
    /// </summary>
    public CCAStateManager stateManager;

    /// <summary>
    /// Returns true if MainMenuCanvas is shown.
    /// </summary>
    public bool IsMainMenuOpen => !FlyCamera.IsEnabled;

    void Start()
    {
        mainMenu = MainMenuCanvas.GetComponent<MainMenuModel>();
        inGameMenu = InGameCanvas.GetComponent<InGameMenuModel>();

        mainMenu.AssertNotNull("mainMenu");
        inGameMenu.AssertNotNull("inGameMenu");

        mainMenu.CloseViewButton.onClick.AddListener(ToogleMainMenu);
        mainMenu.OpenVersionDropdown.onValueChanged.AddListener(OnDropDownChanged);

        ToogleMainMenu(true);
        OnViewDataChanged();
        stateManager.ViewDataChangedEvent.AddListener(OnViewDataChanged);
    }

    void Update()
    {
        if (!IsMainMenuOpen)
        {
            if (Input.GetKeyDown("k"))
            {
                stateManager.ShowPreviousGraph();
            }
            else if (Input.GetKeyDown("l"))
            {
                stateManager.ShowNextGraph();
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                stateManager.ToggleAutoplay();
            }

            string[] animationTimeKeys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            float[] animationTimeValues = { 0.1f, 0.5f, 1, 2, 3, 4, 5, 8, 16, 0 };
            for (int i = 0; i < animationTimeKeys.Length; i++)
            {
                if (Input.GetKeyDown(animationTimeKeys[i]))
                {
                    stateManager.AnimationTime = animationTimeValues[i];
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToogleMainMenu(FlyCamera.IsEnabled);
        }
    }

    /// <summary>
    /// Toggles the visibility from the MainMenuCanvas.
    /// </summary>
    void ToogleMainMenu()
    {
        ToogleMainMenu(FlyCamera.IsEnabled);
    }

    /// <summary>
    /// Changes the visibility of the MainMenuCanvas to the given enabled.
    /// </summary>
    /// <param name="enabled"></param>
    void ToogleMainMenu(bool enabled)
    {
        FlyCamera.IsEnabled = !enabled;
        InGameCanvas.SetActive(!enabled);
        MainMenuCanvas.SetActive(enabled);
        stateManager.ToggleAutoplay(false);
        if (enabled)
        {
            mainMenu.OpenVersionDropdown.ClearOptions();
            var options = Enumerable
                .Range(1, stateManager.GraphCount)
                .Select(i => new Dropdown.OptionData(i.ToString()))
                .ToList();
            mainMenu.OpenVersionDropdown.AddOptions(options);
            mainMenu.OpenVersionDropdown.value = stateManager.OpenGraphIndex;
        }
    }

    /// <summary>
    /// Event function that updates all show data for the user.
    /// E.g. the revision number shown in the InGameCanvas.
    /// </summary>
    void OnViewDataChanged()
    {
        inGameMenu.RevisionNumberText.text = (stateManager.OpenGraphIndex + 1) + " / " + stateManager.GraphCount;
        inGameMenu.AutoplayToggle.isOn = stateManager.IsAutoPlay;
        inGameMenu.AnimationTimeText.text = "Revision animation time: " + stateManager.AnimationTime + "s";
    }

    /// <summary>
    /// Event function that changes the show revision to the
    /// given value index.
    /// </summary>
    /// <param name="value"></param>
    void OnDropDownChanged(int value)
    {
        if (value != stateManager.OpenGraphIndex)
        {
            stateManager.TryShowSpecificGraph(value);
        }
    }
}
