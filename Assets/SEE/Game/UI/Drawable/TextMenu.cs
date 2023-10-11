﻿using Crosstales;
using SEE.Controls;
using SEE.Controls.Actions;
using SEE.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.SEE.Game.UI.Drawable
{
    /// <summary>
    /// This class holds the instance for the text menu.
    /// </summary>
    public static class TextMenu
    {
        /// <summary>
        /// The location where the text menu prefeb is placed.
        /// </summary>
        private const string textMenuPrefab = "Prefabs/UI/Drawable/TextMenu";

        /// <summary>
        /// The instance of the text menu.
        /// </summary>
        public static GameObject instance;

        /// <summary>
        /// The action for the Font Style Buttons that should also be carried out.
        /// </summary>
        private static UnityAction<FontStyles> fontStyleAction;
        /// <summary>
        /// The action for the HSV Color Picker that should also be carried out.
        /// </summary>
        private static UnityAction<Color> pickerAction;

        /// <summary>
        /// The Label for the bold font style state
        /// </summary>
        private const string Bold = "Bold";
        /// <summary>
        /// The Label for the italic font style state
        /// </summary>
        private const string Italic = "Italic";
        /// <summary>
        /// The Label for the underline font style state
        /// </summary>
        private const string Underline = "Underline";
        /// <summary>
        /// The Label for the strikethrough font style state
        /// </summary>
        private const string Strikethrough = "Strikethrough";
        /// <summary>
        /// The Label for the lower case font style state
        /// </summary>
        private const string LowerCase = "LowerCase";
        /// <summary>
        /// The Label for the upper case font style state
        /// </summary>
        private const string UpperCase = "UpperCase";
        /// <summary>
        /// The Label for the small caps font style state
        /// </summary>
        private const string SmallCaps = "SmallCaps";

        /// <summary>
        /// The dictionary that holds the font style states with their values whether they should be active or not.
        /// </summary>
        private static readonly Dictionary<string, bool> styles = new() { { Bold, false },
            {Italic, false },
            {Underline, false },
            {Strikethrough, false},
            {LowerCase, false},
            {UpperCase, false},
            {SmallCaps, false} };

        /// <summary>
        /// The button for the bold font style
        /// </summary>
        private static Button boldBtn;
        /// <summary>
        /// The button for the italic font style
        /// </summary>
        private static Button italicBtn;
        /// <summary>
        /// The button for the underline font style
        /// </summary>
        private static Button underlineBtn;
        /// <summary>
        /// The button for the strikethrough font style
        /// </summary>
        private static Button strikethroughBtn;
        /// <summary>
        /// The button for the lower case font style
        /// </summary>
        private static Button lowerCaseBtn;
        /// <summary>
        /// The button for the upper case font style
        /// </summary>
        private static Button upperCaseBtn;
        /// <summary>
        /// The button for the small caps font style
        /// </summary>
        private static Button smallCapsBtn;
        /// <summary>
        /// The color block for the colors if a font style button is not selected.
        /// </summary>
        private static ColorBlock notSelectedBlock;
        /// <summary>
        /// The color block for the colors if a font style button is selected.
        /// </summary>
        private static ColorBlock selectedBlock = new ColorBlock();

        /// <summary>
        /// The game object of the layer with the edit text button.
        /// </summary>
        private static GameObject editText;
        /// <summary>
        /// The edit text button.
        /// </summary>
        private static Button editTextBtn;
        /// <summary>
        /// The font color button.
        /// </summary>
        private static Button fontColorBtn;
        /// <summary>
        /// The outline color button.
        /// </summary>
        private static Button outlineColorBtn;
        /// <summary>
        /// The HSV color picker.
        /// </summary>
        private static HSVPicker.ColorPicker picker;
        /// <summary>
        /// The thickness slider controller for the outline thickness.
        /// </summary>
        private static FloatValueSliderController thicknessSlider;
        /// <summary>
        /// The game object of the outline thickness layer.
        /// </summary>
        private static GameObject thicknessLayer;
        /// <summary>
        /// The game object of the order in layer layer.
        /// </summary>
        private static GameObject orderInLayer;
        /// <summary>
        /// The slider controller for the order in layer.
        /// </summary>
        private static LayerSliderController orderInLayerSlider;
        /// <summary>
        /// The input field with their up and down button for the font size.
        /// </summary>
        private static InputFieldWithButtons fontSizeInput;

        /// <summary>
        /// A list of actions where the text menu is already displayed at awake.
        /// </summary>
        public static List<ActionStateType> usedIn = new() { ActionStateTypes.WriteText};

        /// <summary>
        /// The init constructor that create the instance for the text menu.
        /// It hides the text menu by default.
        /// </summary>
        static TextMenu()
        {
            instance = PrefabInstantiator.InstantiatePrefab(textMenuPrefab,
                GameObject.Find("UI Canvas").transform, false);
            initBtn();
            instance.SetActive(false);
        }

        /// <summary>
        /// This method assigns the corresponding objects of the TextMenu instance to the buttons, sliders and other GameObjects.
        /// It also adds their initial AddListeners to the objects.
        /// </summary>
        private static void initBtn()
        {
            boldBtn = instance.transform.Find("FontStyles").Find("Bold").GetComponent<Button>();
            italicBtn = instance.transform.Find("FontStyles").Find("Italic").GetComponent<Button>();
            underlineBtn = instance.transform.Find("FontStyles").Find("Underline").GetComponent<Button>();
            strikethroughBtn = instance.transform.Find("FontStyles").Find("Strikethrough").GetComponent<Button>();
            lowerCaseBtn = instance.transform.Find("FontStyles").Find("LowerCase").GetComponent<Button>();
            upperCaseBtn = instance.transform.Find("FontStyles").Find("UpperCase").GetComponent<Button>();
            smallCapsBtn = instance.transform.Find("FontStyles").Find("SmallCaps").GetComponent<Button>();

            initFontStyleButtons();

            notSelectedBlock = boldBtn.colors;
            selectedBlock = notSelectedBlock;
            selectedBlock.normalColor = selectedBlock.selectedColor = selectedBlock.disabledColor = selectedBlock.highlightedColor = selectedBlock.pressedColor = Color.gray;
            
            fontColorBtn = instance.transform.Find("ColorAreaSelector").Find("FontColorBtn").GetComponent<Button>();
            fontColorBtn.interactable = false;
            fontColorBtn.onClick.AddListener(MutuallyExclusiveColorButtons);

            outlineColorBtn = instance.transform.Find("ColorAreaSelector").Find("OutlineColorBtn").GetComponent<Button>();
            outlineColorBtn.onClick.AddListener(MutuallyExclusiveColorButtons);
            thicknessLayer = instance.transform.Find("Thickness").gameObject;
            thicknessSlider = thicknessLayer.GetComponentInChildren<FloatValueSliderController>();
            thicknessLayer.SetActive(false);
            
            picker = instance.GetComponentInChildren<HSVPicker.ColorPicker>();
            fontSizeInput = instance.transform.Find("FontSize").GetComponentInChildren<InputFieldWithButtons>();
            editText = instance.transform.Find("EditText").gameObject;
            editTextBtn = editText.GetComponentInChildren<Button>();
            orderInLayer = instance.transform.Find("Layer").gameObject;
            orderInLayerSlider = orderInLayer.GetComponentInChildren<LayerSliderController>();
        }

        /// <summary>
        /// This method adds the inital AddListeners to the font style buttons.
        /// </summary>
        private static void initFontStyleButtons()
        {
            boldBtn.onClick.AddListener(() => Press(Bold));
            italicBtn.onClick.AddListener(() => Press(Italic));
            underlineBtn.onClick.AddListener(() => Press(Underline));
            strikethroughBtn.onClick.AddListener(() => Press(Strikethrough));
            lowerCaseBtn.onClick.AddListener(() => Press(LowerCase));
            upperCaseBtn.onClick.AddListener(() => Press(UpperCase));
            smallCapsBtn.onClick.AddListener(() => Press(SmallCaps));
        }

        /// <summary>
        /// To hide the text menu.
        /// It's enable the keyboard shortcuts.
        /// </summary>
        public static void disableTextMenu()
        {
            instance.SetActive(false);
            SEEInput.KeyboardShortcutsEnabled = true;
        }

        /// <summary>
        /// Resets the text menu to its initial state.
        /// </summary>
        private static void Reset()
        {
            ResetStyles();
            fontColorBtn.onClick.RemoveAllListeners();
            fontColorBtn.onClick.AddListener(MutuallyExclusiveColorButtons);
            outlineColorBtn.onClick.RemoveAllListeners();
            outlineColorBtn.onClick.AddListener(MutuallyExclusiveColorButtons);
            thicknessSlider.onValueChanged.RemoveAllListeners();
            fontSizeInput.onValueChanged.RemoveAllListeners();
            orderInLayerSlider.onValueChanged.RemoveAllListeners();
        }

        /// <summary>
        /// Reveal the text menu
        /// </summary>
        /// <param name="reset">Specifies whether the menu should be reset to its initial state</param>
        /// <param name="showEditMode">Specifies whether the menu should be opened for edit mode. Otherwise it will be opened for the WriteTextAction</param>
        public static void enableTextMenu(bool reset = true, bool showEditMode = false)
        {
            if (reset)
            {
                Reset();
            }
            if (showEditMode)
            {
                orderInLayer.SetActive(true);
                editText.SetActive(true);
            }
            else
            {
                orderInLayer.SetActive(false);
                editText.SetActive(false);
            }
            instance.SetActive(true);
        }

        /// <summary>
        /// Reveal the text menu
        /// </summary>
        /// <param name="colorAction">The inital action for the hsv color picker.</param>
        /// <param name="color">The inital color for the hsv color picker.</param>
        /// <param name="reset">Specifies whether the menu should be reset to its initial state.</param>
        /// <param name="showEditMode">Specifies whether the menu should be opened for edit mode. Otherwise it will be opened for the WriteTextAction</param>
        public static void enableTextMenu(UnityAction<Color> colorAction, Color color, bool reset = true, bool showEditMode = false)
        {
            if (reset)
            {
                Reset();
            }
            if (showEditMode)
            {
                orderInLayer.SetActive(true);
                editText.SetActive(true);
            }
            else
            {
                orderInLayer.SetActive(false);
                editText.SetActive(false);
            }
            instance.SetActive(true);
            if (fontColorBtn.interactable)
            {
                MutuallyExclusiveColorButtons();
            }
            AssignColorArea(colorAction, color);
        }

        /// <summary>
        /// This method will be used as an action for the AddListener of the color buttons (font/outline).
        /// This allows only one color to be active at a time.
        /// </summary>
        private static void MutuallyExclusiveColorButtons()
        {
            fontColorBtn.interactable = !fontColorBtn.IsInteractable();
            outlineColorBtn.interactable = !outlineColorBtn.IsInteractable();
            if (!outlineColorBtn.interactable)
            {
                thicknessLayer.SetActive(true);
            } else
            {
                thicknessLayer.SetActive(false);
            }
        }

        /// <summary>
        /// Assigns an action and a color to the HSV Color Picker.
        /// </summary>
        /// <param name="colorAction">The color action that should be assigned</param>
        /// <param name="color">The color that should be assigned.</param>
        public static void AssignColorArea(UnityAction<Color> colorAction, Color color)
        {
            if (pickerAction != null)
            {
                picker.onValueChanged.RemoveListener(pickerAction);
            }
            pickerAction = colorAction;
            picker.AssignColor(color);
            picker.onValueChanged.AddListener(colorAction);
        }

        /// <summary>
        /// Assigns an action and a thickness to the outline thickness slider.
        /// </summary>
        /// <param name="thicknessAction">The float action that should be assigned</param>
        /// <param name="thickness">The thickness that should be assigned.</param>
        public static void AssignOutlineThickness(UnityAction<float> thicknessAction, float thickness)
        {
            thicknessSlider.onValueChanged.RemoveAllListeners();
            thicknessSlider.AssignValue(thickness);
            thicknessSlider.onValueChanged.AddListener(thicknessAction);
        }

        /// <summary>
        /// Assigns an action and a font size to the font size input field.
        /// </summary>
        /// <param name="fontSizeAction">The float action that should be assigned</param>
        /// <param name="fontSize">The font size that should be assigned.</param>
        public static void AssignFontSize(UnityAction<float> fontSizeAction, float fontSize)
        {
            fontSizeInput.onValueChanged.RemoveAllListeners();
            fontSizeInput.AssignValue(fontSize);
            fontSizeInput.onValueChanged.AddListener(fontSizeAction);
        }

        /// <summary>
        /// Assigns an action and a order to the order in layer slider.
        /// </summary>
        /// <param name="orderInLayerAction">The int action that should be assigned</param>
        /// <param name="order">The order that should be assigned.</param>
        public static void AssignOrderInLayer(UnityAction<int> orderInLayerAction, int order)
        {
            orderInLayerSlider.onValueChanged.RemoveAllListeners();
            orderInLayerSlider.AssignValue(order);
            orderInLayerSlider.onValueChanged.AddListener(orderInLayerAction);
        }

        /// <summary>
        /// Assigns an action to the edit text button.
        /// </summary>
        /// <param name="action">The action that should be assigned</param>
        public static void AssignEditTextButton(UnityAction action)
        {
            editTextBtn.onClick.RemoveAllListeners();
            editTextBtn.onClick.AddListener(action);
        }

        /// <summary>
        /// Assigns an action and font styles to the font style buttons.
        /// </summary>
        /// <param name="action">The font styles action that should be assigned</param>
        /// <param name="styles">The styles that should be assigned.</param>
        public static void AssignFontStyles(UnityAction<FontStyles> action, FontStyles styles)
        {
            fontStyleAction = action;
            AssignStyles(styles);
        }

        /// <summary>
        /// Returns the font color button
        /// </summary>
        /// <returns>The font color button</returns>
        public static Button GetFontColorButton()
        {
            return fontColorBtn;
        }

        /// <summary>
        /// Returns the outline color button
        /// </summary>
        /// <returns>The outline color button</returns>
        public static Button GetOutlineColorButton()
        {
            return outlineColorBtn;
        }

        /// <summary>
        /// This method will be used as inital AddListener action for the font style buttons.
        /// It enters the status of the selected font style into the dictionary and ensures that mutually exclusive font styles remain exclusive.
        /// </summary>
        /// <param name="pressedStyle"></param>
        public static void Press(string pressedStyle)
        {
            if (styles.TryGetValue(pressedStyle, out bool value)) {
                styles[pressedStyle] = !value;
                if (styles[pressedStyle])
                {
                    GetPressedButton(pressedStyle).colors = selectedBlock;
                    MutuallyExclusiveStyles(pressedStyle);
                    if (fontStyleAction != null)
                    {
                        fontStyleAction.Invoke(GetFontStyle());
                    }
                } else
                {
                    GetPressedButton(pressedStyle).colors = notSelectedBlock;
                    fontStyleAction.Invoke(GetFontStyle());
                }
            }
        }

        /// <summary>
        /// This method ensures that the three mutually exclusive font styles do not overlap.
        /// </summary>
        /// <param name="selectedStyle">The chosen font style.</param>
        private static void MutuallyExclusiveStyles(string selectedStyle)
        {
            switch(selectedStyle)
            {
                case LowerCase:
                    styles[UpperCase] = false;
                    upperCaseBtn.colors = notSelectedBlock;
                    styles[SmallCaps] = false;
                    smallCapsBtn.colors = notSelectedBlock;
                    break;
                case UpperCase:
                    styles[LowerCase] = false;
                    lowerCaseBtn.colors = notSelectedBlock;
                    styles[SmallCaps] = false;
                    smallCapsBtn.colors = notSelectedBlock;
                    break;
                case SmallCaps:
                    styles[LowerCase] = false;
                    lowerCaseBtn.colors = notSelectedBlock;
                    styles[UpperCase] = false;
                    upperCaseBtn.colors = notSelectedBlock;
                    break;
            }
        }

        /// <summary>
        /// Returns the corresponding button for a given string with a style name.
        /// </summary>
        /// <param name="pressedStyle">The given style name</param>
        /// <returns>The corresponding button</returns>
        private static Button GetPressedButton(string pressedStyle)
        {
            Button btn = null;
            switch (pressedStyle)
            {
                case Bold:
                    btn = boldBtn;
                    break;
                case Italic:
                    btn = italicBtn;
                    break;
                case Underline:
                    btn = underlineBtn;
                    break;
                case Strikethrough:
                    btn = strikethroughBtn;
                    break;
                case LowerCase:
                    btn = lowerCaseBtn;
                    break;
                case UpperCase:
                    btn = upperCaseBtn;
                    break;
                case SmallCaps:
                    btn = smallCapsBtn;
                    break;
            }
            return btn;
        }

        /// <summary>
        /// Set's the font style stats in dicitonary to false and changes the color block to not selected.
        /// </summary>
        private static void ResetStyles()
        {
            foreach (string key in styles.Keys.ToList())
            {
                styles[key] = false;
            }
            boldBtn.colors = notSelectedBlock;
            italicBtn.colors = notSelectedBlock;
            underlineBtn.colors = notSelectedBlock;
            strikethroughBtn.colors = notSelectedBlock;
            lowerCaseBtn.colors = notSelectedBlock;
            upperCaseBtn.colors = notSelectedBlock;
            smallCapsBtn.colors = notSelectedBlock;
        }

        /// <summary>
        /// Assigns the respective font styles their value and 
        /// changes their button color when they are selected. 
        /// </summary>
        /// <param name="style"></param>
        private static void AssignStyles(FontStyles style)
        {
            styles[Bold] = ((style & FontStyles.Bold) != 0);
            styles[Italic] = ((style & FontStyles.Italic) != 0);
            styles[Underline] = ((style & FontStyles.Underline) != 0);
            styles[Strikethrough] = ((style & FontStyles.Strikethrough) != 0);
            styles[LowerCase] = ((style & FontStyles.LowerCase) != 0);
            styles[UpperCase] = ((style & FontStyles.UpperCase) != 0);
            styles[SmallCaps] = ((style & FontStyles.SmallCaps) != 0);

            foreach (string key in styles.Keys.ToList())
            {
                if (styles[key])
                {
                    GetPressedButton(key).colors = selectedBlock;
                    MutuallyExclusiveStyles(key);
                }
            }
        }

        /// <summary>
        /// Returns the corresponding font style of a given keyword.
        /// </summary>
        /// <param name="key">The font style keyword</param>
        /// <returns>The corresponding font style</returns>
        private static FontStyles GetFontStyleOfKey(string key)
        {
            FontStyles style = FontStyles.Normal;
            switch (key)
            {
                case Bold:
                    style = FontStyles.Bold;
                    break;
                case Italic:
                    style = FontStyles.Italic;
                    break;
                case Underline:
                    style = FontStyles.Underline;
                    break;
                case Strikethrough:
                    style = FontStyles.Strikethrough;
                    break;
                case LowerCase:
                    style = FontStyles.LowerCase;
                    break;
                case UpperCase:
                    style = FontStyles.UpperCase;
                    break;
                case SmallCaps:
                    style = FontStyles.SmallCaps;
                    break;
            }
            return style;
        }

        /// <summary>
        /// Creates a font style with contains all the selected font styles.
        /// </summary>
        /// <returns>A font style with the chosen font styles</returns>
        public static FontStyles GetFontStyle()
        {
            FontStyles style = FontStyles.Normal;
            foreach (string key in styles.Keys)
            {
                if (styles[key])
                {
                    style |= GetFontStyleOfKey(key);
                }
            }
            return style;
        }
    }
}