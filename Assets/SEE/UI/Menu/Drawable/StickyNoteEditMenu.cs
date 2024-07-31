﻿using Michsky.UI.ModernUIPack;
using SEE.Game.Drawable;
using SEE.Game.Drawable.Configurations;
using SEE.Net.Actions.Drawable;
using SEE.UI.Drawable;
using SEE.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SEE.UI.Menu.Drawable
{
    /// <summary>
    /// This class provides the edit menu for sticky notes.
    /// </summary>
    public static class StickyNoteEditMenu
    {
        /// <summary>
        /// The prefab of the sticky note edit menu.
        /// </summary>
        private const string editMenuPrefab = "Prefabs/UI/Drawable/StickyNoteEdit";

        /// <summary>
        /// The instance for the sticky note edit menu.
        /// </summary>
        private static GameObject instance;

        /// <summary>
        /// Destroys the menu.
        /// </summary>
        public static void Disable()
        {
            if (instance != null)
            {
                Destroyer.Destroy(instance);
            }
        }

        /// <summary>
        /// Create and enables the edit menu.
        /// It add's the necessary Handler to the GUI areas.
        /// </summary>
        public static void Enable(GameObject stickyNote, DrawableConfig newConfig)
        {
            /// Instantiate the menu.
            instance = PrefabInstantiator.InstantiatePrefab(editMenuPrefab,
                                                            UICanvas.Canvas.transform, false);

            /// Initialize the oder in layer slider.
            LayerSlider(stickyNote, newConfig);

            /// Initialize the color picker for the sticky note color.
            ColorPicker(stickyNote, newConfig);

            UnityAction callback = () =>
            {
                instance.SetActive(true);
                StickyNoteRotationMenu.Disable();
                ScaleMenu.Instance.Destroy();
            };

            /// Initialize the edit rotation button.
            Rotation(stickyNote, callback);

            /// Initialize the edit scale button.
            Scale(stickyNote, callback);

            /// Initialize the lighting switch manager.
            Lighting(stickyNote, newConfig);
        }

        /// <summary>
        /// Initializes the order in layer slider and adds the required handler.
        /// The handler executes the <see cref="GameStickyNoteManager.ChangeLayer"/>
        /// and saves the new order in the layer in the configuration.
        /// </summary>
        /// <param name="stickyNote">The sticky note which order in layer should be changed.</param>
        /// <param name="newConfig">The configuration which holds the new values.</param>
        private static void LayerSlider(GameObject stickyNote, DrawableConfig newConfig)
        {
            LayerSliderController orderInLayerSlider = instance.GetComponentInChildren<LayerSliderController>();
            orderInLayerSlider.AssignValue(newConfig.Order);
            orderInLayerSlider.OnValueChanged.AddListener(order =>
            {
                newConfig.Order = order;
                GameStickyNoteManager.ChangeLayer(stickyNote, order);
                new EditLayerNetAction(GameFinder.GetDrawableSurface(stickyNote).name, stickyNote.name, "",
                    order).Execute();
            });
        }

        /// <summary>
        /// Initializes the lighting switch manager and adds the required handler.
        /// The handler executes the <see cref="GameStickyNoteManager.ChangeLighting"/>
        /// and saves the new lighting state in the configuration.
        /// </summary>
        /// <param name="stickyNote">The sticky note which lighting should be changed.</param>
        /// <param name="newConfig">The configuration which holds the new values.</param>
        private static void Lighting(GameObject stickyNote, DrawableConfig newConfig)
        {
            SwitchManager lightingManager = instance.GetComponentInChildren<SwitchManager>();
            lightingManager.OffEvents.AddListener(() =>
            {
                newConfig.Lighting = false;
                GameDrawableManager.ChangeLighting(stickyNote, false);
                new DrawableChangeLightingNetAction(newConfig).Execute();
            });
            lightingManager.OnEvents.AddListener(() =>
            {
                newConfig.Lighting = true;
                GameDrawableManager.ChangeLighting(stickyNote, true);
                new DrawableChangeLightingNetAction(newConfig).Execute();
            });

            /// Assigns the current status to the switch and updates the UI.
            lightingManager.isOn = newConfig.Lighting;
            lightingManager.UpdateUI();
        }

        /// <summary>
        /// Initializes the color picker and adds the required handler.
        /// The handler executes the <see cref="GameStickyNoteManager.ChangeColor"/>
        /// and saves the new color in the configuration.
        /// </summary>
        /// <param name="stickyNote">The sticky note which color should be changed.</param>
        /// <param name="newConfig">The configuration which holds the new values.</param>
        private static void ColorPicker(GameObject stickyNote, DrawableConfig newConfig)
        {
            HSVPicker.ColorPicker picker = instance.GetComponentInChildren<HSVPicker.ColorPicker>();
            picker.AssignColor(newConfig.Color);
            picker.onValueChanged.AddListener(color =>
            {
                newConfig.Color = color;
                GameDrawableManager.ChangeColor(stickyNote, color);
                new DrawableChangeColorNetAction(newConfig).Execute();
            });
        }

        /// <summary>
        /// Initializes the rotation button and adds the required handler.
        /// The handler opens the <see cref="StickyNoteRotationMenu"/>.
        /// </summary>
        /// <param name="stickyNote">The sticky note which rotation should be changed.</param>
        /// <param name="callback">The call back to return to the parent menu.</param>
        private static void Rotation(GameObject stickyNote, UnityAction callback)
        {
            ButtonManagerBasic rotation = GameFinder.FindChild(instance, "Rotation").GetComponent<ButtonManagerBasic>();
            rotation.clickEvent.AddListener(() =>
            {
                instance.SetActive(false);
                StickyNoteRotationMenu.Enable(GameFinder.GetHighestParent(stickyNote), null, callback);
            });
        }

        /// <summary>
        /// Initializes the scale button and adds the required handler.
        /// The handler opens the <see cref="ScaleMenu"/>.
        /// </summary>
        /// <param name="stickyNote">The sticky note which scale should be changed.</param>
        /// <param name="callback">The call back to return to the parent menu.</param>
        private static void Scale(GameObject stickyNote, UnityAction callback)
        {
            ButtonManagerBasic scale = GameFinder.FindChild(instance, "Scale").GetComponent<ButtonManagerBasic>();
            scale.clickEvent.AddListener(() =>
            {
                instance.SetActive(false);
                ScaleMenu.Enable(stickyNote, true, callback);
            });
        }
    }
}