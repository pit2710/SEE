﻿using Michsky.UI.ModernUIPack;
using SEE.Game.Drawable;
using SEE.Game.Drawable.Configurations;
using SEE.Net.Actions.Drawable;
using SEE.UI.Drawable;
using SEE.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SEE.UI.Menu.Drawable
{
    /// <summary>
    /// This class provides a menu for editing an image.
    /// </summary>
    public static class ImageMenu
    {
        /// <summary>
        /// The location where the image menu prefeb is placed.
        /// </summary>
        private const string imageMenuPrefab = "Prefabs/UI/Drawable/ImageMenu";

        /// <summary>
        /// The instance of the image menu.
        /// </summary>
        public static GameObject instance;

        /// <summary>
        /// The action for the HSV Color Picker that should also be carried out.
        /// </summary>
        private static UnityAction<Color> pickerAction;

        /// <summary>
        /// The slider controller for the order in layer.
        /// </summary>
        private static LayerSliderController orderInLayerSlider;

        /// <summary>
        /// The HSV color picker.
        /// </summary>
        private static HSVPicker.ColorPicker picker;

        /// <summary>
        /// The mirror switch. It will needed to mirror the image on the y axis at 180°.
        /// </summary>
        private static SwitchManager mirrorSwitch;

        /// <summary>
        /// The thubnail image of the chosen image.
        /// </summary>
        private static Image thumbnail;

        /// <summary>
        /// To destroy the image menu.
        /// </summary>
        public static void Disable()
        {
            if (instance != null)
            {
                Destroyer.Destroy(instance);
            }
        }

        /// <summary>
        /// Gets the state if the menu is already opened.
        /// </summary>
        /// <returns>true, if the menu is alreay opened. Otherwise false.</returns>
        public static bool IsOpen()
        {
            return instance != null;
        }

        /// <summary>
        /// Enables the image menu and register the necessary Handler to the components.
        /// </summary>
        /// <param name="imageObj">The image object which should be changed.</param>
        /// <param name="newValueHolder">The configuration file which should be changed.</param>
        public static void Enable(GameObject imageObj, DrawableType newValueHolder)
        {
            if (newValueHolder is ImageConf imageConf)
            {
                Instantiate();
                GameObject drawable = GameFinder.GetDrawable(imageObj);
                string drawableParentName = GameFinder.GetDrawableParentName(drawable);

                /// Assigns an action to the slider that should be executed 
                /// along with the current order in layer value
                AssignOrderInLayer(order =>
                {
                    GameEdit.ChangeLayer(imageObj, order);
                    imageConf.orderInLayer = order;
                    ImageConf conf = ImageConf.GetImageConf(imageObj);
                    conf.fileData = null;
                    new EditImageNetAction(drawable.name, drawableParentName, conf).Execute();
                }, imageConf.orderInLayer);

                /// Assigns an action to the color picker that should be executed 
                /// along with the current color.
                AssignColorArea(color =>
                {
                    GameEdit.ChangeImageColor(imageObj, color);
                    imageConf.imageColor = color;
                    ImageConf conf = ImageConf.GetImageConf(imageObj);
                    conf.fileData = null;
                    new EditImageNetAction(drawable.name, drawableParentName, conf).Execute();
                }, imageConf.imageColor);

                /// Initialize the switch for mirror the image.
                InitMirrorSwitch(imageObj, imageConf, drawable, drawableParentName);

                /// Sets the thumbnail of the original image.
                thumbnail.sprite = imageObj.GetComponent<Image>().sprite;
            }
        }

        /// <summary>
        /// Create the instance for the image menu and initialize
        /// the GUI elements.
        /// </summary>
        private static void Instantiate()
        {
            instance = PrefabInstantiator.InstantiatePrefab(imageMenuPrefab,
                GameObject.Find("UI Canvas").transform, false);
            orderInLayerSlider = instance.GetComponentInChildren<LayerSliderController>();
            picker = instance.GetComponentInChildren<HSVPicker.ColorPicker>();
            mirrorSwitch = instance.GetComponentInChildren<SwitchManager>();
            thumbnail = GameFinder.FindChild(instance, "Image").GetComponent<Image>();
        }

        /// <summary>
        /// Initialize the mirror switch.
        /// It mirrors an image.
        /// Off is normal and on is mirrored. 
        /// </summary>
        /// <param name="imageObj">The image object</param>
        /// <param name="imageConf">The configuration which holds the new values.</param>
        /// <param name="drawable">The drawable on which the image is displayed</param>
        /// <param name="drawableParentName">The id of the drawable parent</param>
        private static void InitMirrorSwitch(GameObject imageObj, ImageConf imageConf,
            GameObject drawable, string drawableParentName)
        {
            /// Removes the old handler.
            mirrorSwitch.OnEvents.RemoveAllListeners();
            mirrorSwitch.OffEvents.RemoveAllListeners();

            /// Mirrored display
            mirrorSwitch.OnEvents.AddListener(() =>
            {
                GameMoveRotator.SetRotateY(imageObj, 180f);
                imageConf.eulerAngles = new Vector3(0, 180, imageConf.eulerAngles.z);
                new RotatorYNetAction(drawable.name, drawableParentName, imageObj.name, 180f).Execute();
            });

            /// Normal display
            mirrorSwitch.OffEvents.AddListener(() =>
            {
                GameMoveRotator.SetRotateY(imageObj, 0);
                imageConf.eulerAngles = new Vector3(0, 0, imageConf.eulerAngles.z);
                new RotatorYNetAction(drawable.name, drawableParentName, imageObj.name, 0).Execute();
            });

            /// Sets the state of the switch.
            mirrorSwitch.isOn = imageConf.eulerAngles.y == 180;
        }

        /// <summary>
        /// Assigns an action and a order to the order in layer slider.
        /// </summary>
        /// <param name="orderInLayerAction">The int action that should be assigned</param>
        /// <param name="order">The order that should be assigned.</param>
        private static void AssignOrderInLayer(UnityAction<int> orderInLayerAction, int order)
        {
            orderInLayerSlider.onValueChanged.RemoveAllListeners();
            orderInLayerSlider.AssignValue(order);
            orderInLayerSlider.onValueChanged.AddListener(orderInLayerAction);
        }

        /// <summary>
        /// Assigns an action and a color to the HSV Color Picker.
        /// It removes the previous additional action, if there was one.
        /// </summary>
        /// <param name="colorAction">The color action that should be assigned</param>
        /// <param name="color">The color that should be assigned.</param>
        private static void AssignColorArea(UnityAction<Color> colorAction, Color color)
        {
            if (pickerAction != null)
            {
                picker.onValueChanged.RemoveListener(pickerAction);
            }
            pickerAction = colorAction;
            picker.AssignColor(color);
            picker.onValueChanged.AddListener(colorAction);
        }
    }
}