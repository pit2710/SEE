using UnityEngine.UI;
using UnityEngine;
using TMPro;
using SEE.Controls;
using SEE.Utils;
using SEE.GO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SEE.UI
{
    /// <summary>
    /// Handles the user interactions with the settings menu.
    /// </summary>
    public class SettingsMenu : PlatformDependentComponent
    {
        /// <summary>
        /// Prefab for the <see cref="SettingsMenu"/>.
        /// </summary>
        private string SettingsPrefab => UIPrefabFolder + "SettingsMenu";

        /// <summary>
        /// Prefab for the KeyBindingContent.
        /// </summary>
        private string KeyBindingContent => UIPrefabFolder + "KeyBindingContent";

        /// <summary>
        /// Prefab for the ScrollView.
        /// </summary>
        private string ScrollPrefab => UIPrefabFolder + "ScrollPrefab";

        /// <summary>
        /// The game object instantiated for the <see cref="SettingsPrefab"/>.
        /// </summary>
        private GameObject settingsMenuGameObject;

        /// <summary>
        /// The game object instantiated for the <see cref="ScrollPrefab"/>.
        /// </summary>
        private GameObject scrollView;

        /// <summary>
        /// The game object instantiated for the <see cref="KeyBindingContent"/>.
        /// </summary>
        private GameObject keyBindingContent;

        /// <summary>
        /// This dictionary is used, to update the key button labels,
        /// when the key for a binding gets changed.
        /// </summary>
        Dictionary<string, TextMeshProUGUI> buttonToLabel;

        /// <summary>
        /// Sets the content and adds the onClick event ExitGame to the ExitButton.
        /// </summary>
        protected override void StartDesktop()
        {
            string[] buttonNames = KeyBindings.GetButtonNames();
            // instantiates the buttonToLabel dictionary
            buttonToLabel = new Dictionary<string, TextMeshProUGUI>();
            // instantiates the keyBindings dictionary
            Dictionary<KeyCode, string> keyBindings = KeyBindings.GetBindings();
            // group the keyBindings by it scopes
            var groupedBindings = keyBindings.GroupBy(pair => KeyBindings.GetScope(pair.Value));
            // instantiates the SettingsMenu
            settingsMenuGameObject = PrefabInstantiator.InstantiatePrefab(SettingsPrefab, Canvas.transform, false);
            Button exitButton = settingsMenuGameObject.transform.Find("SettingsPanel/ExitButton").gameObject.MustGetComponent<Button>();
            // adds the ExitGame method to the button
            exitButton.onClick.AddListener(ExitGame);
            // set the content
            foreach (var group in groupedBindings)
            {
                // display the scope
                // instantiates the scrollView
                scrollView = PrefabInstantiator.InstantiatePrefab(ScrollPrefab, Canvas.transform, false).transform.gameObject;
                scrollView.transform.SetParent(settingsMenuGameObject.transform.Find("KeybindingsPanel/KeybindingsText/Viewport/Content"));
                // set the titles of the scrollViews to the scopes
                TextMeshProUGUI groupTitle = scrollView.transform.Find("Group").gameObject.MustGetComponent<TextMeshProUGUI>();
                groupTitle.text = $"{group.Key}";
                // display the scrollview for each scope, with its bindings and keys
                foreach (var binding in group)
                {
                    // instantiates the keyBindingContent
                    keyBindingContent = PrefabInstantiator.InstantiatePrefab(KeyBindingContent, Canvas.transform, false).transform.gameObject;
                    keyBindingContent.transform.SetParent(scrollView.transform.Find("Scroll View/Viewport/Content"));

                    // set the text to the bindingName
                    TextMeshProUGUI bindingText = keyBindingContent.transform.Find("Binding").gameObject.MustGetComponent<TextMeshProUGUI>();
                    string bindingName = binding.Value.Substring(0, binding.Value.IndexOf("[")).ToString();
                    bindingText.text = bindingName;

                    // set the label of the key button
                    TextMeshProUGUI key = keyBindingContent.transform.Find("Key/Text (TMP)").gameObject.MustGetComponent<TextMeshProUGUI>();
                    key.text = KeyBindings.GetKeyNameForButton(bindingName);
                    // add the label to the dictionary
                    buttonToLabel[bindingName] = key;
                    // add the actionlistener, to be able to change the key of a binding
                    keyBindingContent.transform.Find("Key").gameObject.MustGetComponent<Button>().onClick.AddListener( () => { StartRebindFor(bindingName); });
                }
            }
        }

        /// <summary>
        /// Toggles the settings panel with the ESC button and handels
        /// the case, that the user wants to change the key of a keyBinding.
        /// </summary>
        protected override void UpdateDesktop()
        {
            // when the buttonToRebind is not null, then the user clicked a button to start the rebind.
            if(buttonToRebind != null)
            {
                SEEInput.KeyboardShortcutsEnabled = false;
                // the next button, that gets pressed, will be the new keyBind.
                if (Input.anyKeyDown)
                {
                    // get the key that was pressed.
                    foreach(KeyCode key in Enum.GetValues(typeof(KeyCode)))
                    {
                        //rebind the key
                        if (Input.GetKeyDown(key))
                        {
                            // check if the key is already bound to another binding, if not, then update the key 
                            if(KeyBindings.SetButtonForKey(buttonToRebind, key))
                            {
                                // update the label of the button of the key
                                buttonToLabel[buttonToRebind].text = key.ToString();
                            }
                            else
                            {
                                string exceptionText = $"Cannot register key {key} for {buttonToRebind}\n Key {key} already bound to {KeyBindings.GetBindings()[key]}\n";
                                settingsMenuGameObject.transform.Find("KeybindingsPanel/Exception/ExceptionText").gameObject.MustGetComponent<TextMeshProUGUI>().text = exceptionText;
                                settingsMenuGameObject.transform.Find("KeybindingsPanel/Exception").gameObject.SetActive(true);
                            }
                            buttonToRebind = null;
                            SEEInput.KeyboardShortcutsEnabled = true;
                            break;
                        }
                    }
                }
            }
            if (SEEInput.ToggleSettings())
            {
                Transform keybindingsPanel = settingsMenuGameObject.transform.Find("KeybindingsPanel");
                GameObject settingsPanel = settingsMenuGameObject.transform.Find("SettingsPanel").gameObject;
                if (keybindingsPanel.gameObject.activeSelf && !settingsPanel.activeSelf)
                {
                    // handles the case where the user is in the KeybindingsPanel but wants to close it
                    keybindingsPanel.gameObject.SetActive(false);
                }
                else
                {
                    // handles the case where the user wants to open/close the SettingsPanel
                    settingsPanel.SetActive(!settingsPanel.activeSelf);
                }
            }
        }

        /// <summary>
        /// The keyBinding which gets updated
        /// </summary>
        string buttonToRebind = null;

        /// <summary>
        /// Sets the <see cref="buttonToRebind"/>.
        /// </summary>
        void StartRebindFor(string buttonName)
        {
            buttonToRebind = buttonName;
            Debug.Log("StartRebindFor: " + buttonName);
        }

        /// <summary>
        /// Terminates the application (exits the game).
        /// </summary>
        private void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
