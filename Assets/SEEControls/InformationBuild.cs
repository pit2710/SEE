﻿using UnityEngine;
using UnityEngine.UI;

[System.Obsolete("Will be removed when the transition to new design of input-actions mapping is implemented.")]
public class InformationBuild : MonoBehaviour
{
    /// <summary>
    /// The text field on the panel holding the current targets name.
    /// Must be assigned in editor
    /// </summary>
    public GameObject NamePanel;

    void Start()
    {
        Camera.main.GetComponent<TouchControlsSEE>().OnTargetChanged.AddListener(LoadTargetInfo);
    }

    /// <summary>
    /// Loads the information of the current target from the main control script to the panel.
    /// </summary>
    private void LoadTargetInfo()
    {
        NamePanel.GetComponent<Text>().text = Camera.main.GetComponent<TouchControlsSEE>().GetTarget().gameObject.name;
    }
}
