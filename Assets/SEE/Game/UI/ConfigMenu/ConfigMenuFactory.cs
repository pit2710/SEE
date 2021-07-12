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

using SEE.GO;
using UnityEngine;
using Valve.VR;
using PlayerSettings = SEE.Controls.PlayerSettings;

namespace SEE.Game.UI.ConfigMenu
{
    /// <summary>
    /// The script responsible for constructing a config menu and modifying its runtime behavior,
    /// e.g. hotkey handling to show/hide the menu.
    ///
    /// This gets usually attached to a player (currently VR/Desktop).
    /// </summary>
    public class ConfigMenuFactory : DynamicUIBehaviour
    {
        private static readonly EditableInstance DefaultInstanceToEdit =
            EditableInstance.Implementation;
        private static readonly string ConfigMenuPrefabPath = "Assets/Prefabs/UI/ConfigMenu.prefab";

        private readonly SteamVR_Action_Boolean _openAction =
            SteamVR_Actions._default.OpenSettingsMenu;
        private readonly SteamVR_Input_Sources _inputSource = SteamVR_Input_Sources.Any;

        private GameObject _configMenuPrefab;
        private ConfigMenu _configMenu;
        private bool _isModPressed;

        private void Awake()
        {
            _configMenuPrefab = MustLoadPrefabAtPath(ConfigMenuPrefabPath);
            BuildConfigMenu(DefaultInstanceToEdit);
        }

        private void BuildConfigMenu(EditableInstance instanceToEdit)
        {
            GameObject configMenuGo = Instantiate(_configMenuPrefab);
            configMenuGo.transform.SetSiblingIndex(0);
            configMenuGo.MustGetComponent(out _configMenu);
            _configMenu.CurrentlyEditing = instanceToEdit;
            _configMenu.OnInstanceChangeRequest.AddListener(ReplaceMenu);
        }

        private void ReplaceMenu(EditableInstance newInstance)
        {
            Destroy(_configMenu.gameObject);
            BuildConfigMenu(newInstance);
        }

        private void Update()
        {
            switch (PlayerSettings.GetInputType())
            {
                case PlayerInputType.DesktopPlayer:
                    HandleDesktopUpdate();
                    break;
                case PlayerInputType.VRPlayer:
                    HandleVRUpdate();
                    break;
                default:
                    throw new System.NotImplementedException($"ConfigMenuFactory.Update not implemented for {PlayerSettings.GetInputType()}.");
            }
        }
        private void HandleDesktopUpdate()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                _isModPressed = true;
            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
                _isModPressed = false;

            if (_isModPressed && Input.GetKeyUp(KeyCode.Escape))
                _configMenu.Toggle();
        }

        private void HandleVRUpdate()
        {
            if (_openAction.GetStateDown(_inputSource))
                _configMenu.Toggle();
        }
    }
}
