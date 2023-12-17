using UnityEngine.UI;
using UnityEngine;
using TMPro;
using SEE.Controls;
using SEE.Utils;
using SEE.GO;
using System;
using System.Collections.Generic;
using System.Linq;
using SEE.UI.Notification;

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
        /// A mapping of the short name of the key binding onto the label of the button that allows to
        /// change the binding. This dictionary is used to update the label if the key binding
        /// was changed by the user.
        /// </summary>
        Dictionary<string, TextMeshProUGUI> shortNameOfBindingToLabel;

        /// <summary>
        /// Sets the <see cref="KeyBindingContent"/> and adds the onClick event
        /// <see cref="ExitGame"/> to the ExitButton.
        /// </summary>
        protected override void StartDesktop()
        {
            // instantiates the SettingsMenu
            settingsMenuGameObject = PrefabInstantiator.InstantiatePrefab(SettingsPrefab, Canvas.transform, false);
            // adds the ExitGame method to the exit button
            settingsMenuGameObject.transform.Find("ExitPanel/Buttons/Content/Exit").gameObject.MustGetComponent<Button>()
                .onClick.AddListener(ExitGame);

            shortNameOfBindingToLabel = new Dictionary<string, TextMeshProUGUI>();

            // Displays all bindings grouped by their category.
            foreach (IGrouping<KeyBindings.KeyActionCategory, KeyValuePair<KeyBindings.KeyAction, KeyBindings.KeyActionDescriptor>> group
                in KeyBindings.AllBindings().GroupBy(binding => binding.Value.Category))
            {
                // Creates a list of keybinding descriptions for the given category.
                GameObject scrollView = PrefabInstantiator.InstantiatePrefab(ScrollPrefab, Canvas.transform, false).transform.gameObject;
                scrollView.transform.SetParent(settingsMenuGameObject.transform.Find("KeybindingsPanel/KeybindingsText/Viewport/Content"));
                // set the titles of the scrollViews to the scopes
                TextMeshProUGUI groupTitle = scrollView.transform.Find("Group").gameObject.MustGetComponent<TextMeshProUGUI>();
                groupTitle.text = $"{group.Key}";

                foreach (var binding in group)
                {
                    GameObject keyBindingContent = PrefabInstantiator.InstantiatePrefab(KeyBindingContent, Canvas.transform, false).transform.gameObject;
                    keyBindingContent.transform.SetParent(scrollView.transform.Find("Scroll View/Viewport/Content"));

                    // set the text to the short name of the binding
                    TextMeshProUGUI bindingText = keyBindingContent.transform.Find("Binding").gameObject.MustGetComponent<TextMeshProUGUI>();
                    // The short name of the binding.
                    bindingText.text = binding.Value.Name;
                    // set the label of the key button
                    TextMeshProUGUI key = keyBindingContent.transform.Find("Key/Text (TMP)").gameObject.MustGetComponent<TextMeshProUGUI>();
                    // The name of the key code bound.
                    key.text = binding.Value.KeyCode.ToString();
                    shortNameOfBindingToLabel[binding.Value.Name] = key;
                    // add the actionlistener to be able to change the key code of a binding.
                    keyBindingContent.transform.Find("Key").gameObject.MustGetComponent<Button>().onClick.AddListener(() => StartRebindFor(binding.Value));
                }
            }
        }

        /// <summary>
        /// Toggles the settings panel with the Pause button and handles
        /// the case, that the user wants to change the key of a keyBinding.
        /// </summary>
        protected override void UpdateDesktop()
        {
            // when the buttonToRebind is not null, then the user clicked a button to start the rebind.
            if (bindingToRebind != null)
            {
                SEEInput.KeyboardShortcutsEnabled = false;
                // the next button, that gets pressed, will be the new keyBind.
                if (Input.anyKeyDown)
                {
                    foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKeyDown(key) && KeyBindings.AssignableKeyCode(key))
                        {
                            // check if the key is already bound to another binding, if not, then re-assign the key
                            if (KeyBindings.SetBindingForKey(bindingToRebind, key))
                            {
                                // TODO (#683): We need to open a modal dialog and ask the user
                                // whether he/she really wants to change the binding.
                                shortNameOfBindingToLabel[bindingToRebind.Name].text = key.ToString();
                            }
                            else
                            {
                                string message = string.Empty;
                                if (KeyBindings.TryGetKeyAction(key, out KeyBindings.KeyAction action))
                                {
                                    message = $"\n Key {key} already bound to {action}.";
                                }
                                ShowNotification.Error
                                    ("Key code already bound",
                                    $"Cannot register key {key} for {bindingToRebind}.{message}\n");
                            }
                            bindingToRebind = null;
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
        /// The keyBinding which gets updated.
        /// </summary>
        private KeyBindings.KeyActionDescriptor bindingToRebind = null;

        /// <summary>
        /// Sets the <see cref="bindingToRebind"/>.
        /// </summary>
        private void StartRebindFor(KeyBindings.KeyActionDescriptor binding)
        {
            bindingToRebind = binding;
        }

        /// <summary>
        /// Terminates the application (exits the game).
        /// </summary>
        private static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
