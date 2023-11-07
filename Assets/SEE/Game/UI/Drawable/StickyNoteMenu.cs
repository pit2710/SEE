﻿using Michsky.UI.ModernUIPack;
using SEE.Controls.Actions;
using SEE.Controls.Actions.Drawable;
using SEE.Game.Drawable;
using SEE.Utils;
using System.Collections;
using UnityEngine;
using static SEE.Controls.Actions.Drawable.StickyNoteAction;

namespace Assets.SEE.Game.UI.Drawable
{
    /// <summary>
    /// This class provides a menu, with which the player can select 
    /// an operation for the sticky notes.
    /// </summary>
    public static class StickyNoteMenu
    {
        /// <summary>
        /// The location where the menu prefeb is placed.
        /// </summary>
        private const string stickyNoteMenuPrefab = "Prefabs/UI/Drawable/StickyNoteMenu";

        /// <summary>
        /// The instance for the sticky note menu.
        /// </summary>
        private static GameObject instance;

        /// <summary>
        /// Whether this class has an operation in store that wasn't yet fetched.
        /// </summary>
        private static bool gotOperation;

        /// <summary>
        /// If <see cref="gotOperation"/> is true, this contains the operation which the player selected.
        /// </summary>
        private static Operation chosenOperation;

        /// <summary>
        /// Enables the image source menu and register the needed Handler to the button's.
        /// </summary>
        public static void Enable()
        {
            instance = PrefabInstantiator.InstantiatePrefab(stickyNoteMenuPrefab,
                GameObject.Find("UI Canvas").transform, false);

            ButtonManagerBasic spawn = GameFinder.FindChild(instance, "Spawn").GetComponent<ButtonManagerBasic>();
            spawn.clickEvent.AddListener(() =>
            {
                gotOperation = true;
                chosenOperation = Operation.Spawn;
            });
            ButtonManagerBasic move = GameFinder.FindChild(instance, "Move").GetComponent<ButtonManagerBasic>();
            move.clickEvent.AddListener(() =>
            {
                gotOperation = true;
                chosenOperation = Operation.Move;
            });
            ButtonManagerBasic edit = GameFinder.FindChild(instance, "Edit").GetComponent<ButtonManagerBasic>();
            edit.clickEvent.AddListener(() =>
            {
                gotOperation = true;
                chosenOperation = Operation.Edit;
            });
            ButtonManagerBasic delete = GameFinder.FindChild(instance, "Delete").GetComponent<ButtonManagerBasic>();
            delete.clickEvent.AddListener(() =>
            {
                gotOperation = true;
                chosenOperation = Operation.Delete;
            });

        }

        /// <summary>
        /// Destroy's the menu.
        /// </summary>
        public static void Disable()
        {
            if (instance != null)
            {
                Destroyer.Destroy(instance);
            }
        }

        public static bool IsOpen()
        {
            return instance != null;
        }

        /// <summary>
        /// If <see cref="gotOperation"/> is true, the <paramref name="operation"/> will be the chosen operation by the
        /// player. Otherwise it will be some dummy value.
        /// </summary>
        /// <param name="operation">The operation the player confirmed, if that doesn't exist, some dummy value</param>
        /// <returns><see cref="gotOperation"/></returns>
        public static bool TryGetOperation(out Operation operation)
        {
            if (gotOperation)
            {
                operation = chosenOperation;
                gotOperation = false;
                return true;
            }

            operation = Operation.None;
            return false;
        }
    }
}