﻿using SEE.GO;
using SEE.GO.Menu;
using SEE.Utils;
using UnityEngine;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Creates the in-game menu with menu entries for moving a node within a 
    /// code city, mapping a node between two code cities, and undoing these
    /// two actions.
    /// </summary>
    public class PlayerMenu : MonoBehaviour
    {
        /// <summary>
        /// Radius of the menu.
        /// </summary>
        [Tooltip("The radius of the circular menu.")]
        [Range(0, 2)]
        public float Radius = 0.3f;

        /// <summary>
        /// Radius of the menu.
        /// </summary>
        [Tooltip("The depth of the circular menu (z axis).")]
        [Range(0, 0.1f)]
        public float Depth = 0.01f;

        /// <summary>
        /// The player actions attached to the gameObject. The selection of 
        /// menu entries will be forwarded to this component.
        /// </summary>
        private PlayerActions playerActions;

        /// <summary>
        /// Creates the <see cref="menu"/> if it does not exist yet.
        /// Sets <see cref="mainCamera"/>.
        /// </summary>
        protected virtual void Start()
        {
            MenuFactory.CreateMenu(EntriesParameter, Radius, Depth);
            if (!gameObject.TryGetComponent<PlayerActions>(out playerActions))
            {
                Debug.LogErrorFormat("Player {0} does not have PlayerActions.\n", name);
                enabled = false;
            }
        }

        /// <summary>
        /// Called from the menu as a callback when the user selects the browse menu entry.
        /// Passes the browse request on to <see cref="playerActions"/>.
        /// </summary>
        private void BrowseOn()
        {
            playerActions.Browse();
        }

        /// <summary>
        /// Called from the menu as a callback when the user selects the move menu entry.
        /// Passes the move request on to <see cref="playerActions"/>.
        /// </summary>
        private void MoveOn()
        {
            playerActions.Move();
        }

        /// <summary>
        /// Called from the menu as a callback when the user selects the map menu entry.
        /// Passes the map request on to <see cref="playerActions"/>.
        /// </summary>
        private void MapOn()
        {
            playerActions.Map();
        }

        /// <summary>
        /// Path of the prefix for the sprite to be instantiated for the menu entries.
        /// </summary>
        private const string menuEntrySprite = "Icons/Circle";

        /// <summary>
        /// The entries of the menu.
        /// </summary>
        private MenuDescriptor[] EntriesParameter;

        private void Awake()
        {
            EntriesParameter = new MenuDescriptor[]
            {
                // Normal browsing mode 
                new MenuDescriptor(label: "Browse",
                                   spriteFile: menuEntrySprite,
                                   activeColor: Color.blue,
                                   inactiveColor: Color.blue.Lighter(),
                                   entryOn: BrowseOn,
                                   entryOff: null,
                                   isTransient: true),
                // Moving a node within a graph
                new MenuDescriptor(label: "Move",
                                   spriteFile: menuEntrySprite,
                                   activeColor: Color.red,
                                   inactiveColor: Color.red.Lighter(),
                                   entryOn: MoveOn,
                                   entryOff: null,
                                   isTransient: true),
                // Mapping a node from one graph to another graph
                new MenuDescriptor(label: "Map",
                                   spriteFile: menuEntrySprite,
                                   activeColor: Color.green,
                                   inactiveColor: Color.green.Lighter(),
                                   entryOn: MapOn,
                                   entryOff: null,
                                   isTransient: true),
            };
        }
    }
}