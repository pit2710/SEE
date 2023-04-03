using System.Collections.Generic;
using System.Linq;
using SEE.Game.UI.Notification;
using UnityEngine;
using SEE.Game.HolisticMetrics.Components;
using SEE.Utils;

namespace SEE.Game.HolisticMetrics
{
    /// <summary>
    /// This class manages all metric boards.
    /// </summary>
    public static class BoardsManager
    {
        /// <summary>
        /// The board prefab we will be instantiating here.
        /// </summary>
        private static readonly GameObject boardPrefab =
            Resources.Load<GameObject>("Prefabs/HolisticMetrics/SceneComponents/MetricsBoard");

        /// <summary>
        /// This field remembers whether or not the little buttons underneath the boards for moving boards around are
        /// currently enabled.
        /// </summary>
        private static bool movingEnabled;

        /// <summary>
        /// This field remembers whether or not the widgets can be moved currently.
        /// </summary>
        private static bool widgetsMovingEnabled;

        /// <summary>
        /// List of all the <see cref="WidgetsManager"/>s that this manager manages (there should not be any
        /// <see cref="WidgetsManager"/>s in the scene that are not in this list).
        /// </summary>
        private static readonly List<WidgetsManager> widgetsManagers = new();

        /// <summary>
        /// Creates a new metrics board and puts its <see cref="WidgetsManager"/> into the list of
        /// <see cref="WidgetsManager"/>s.
        /// </summary>
        /// <param name="boardConfig">The board configuration for the new board.</param>
        internal static void Create(BoardConfig boardConfig)
        {
            widgetsManagers.RemoveAll(x => x == null);  // remove stale managers
            bool nameExists = widgetsManagers.Any(widgetsManager =>
                widgetsManager.GetTitle().Equals(boardConfig.Title));
            if (nameExists)
            {
                ShowNotification.Error("Cannot create that board", "The name has to be unique.");
                return;
            }

            GameObject newBoard = Object.Instantiate(
                boardPrefab,
                boardConfig.Position,
                boardConfig.Rotation);

            WidgetsManager newWidgetsManager = newBoard.GetComponent<WidgetsManager>();

            // Set the title of the new board
            newWidgetsManager.SetTitle(boardConfig.Title);

            // Add the widgets to the new board
            foreach (WidgetConfig widgetConfiguration in boardConfig.WidgetConfigs)
            {
                newWidgetsManager.Create(widgetConfiguration);
            }

            widgetsManagers.Add(newWidgetsManager);
        }

        /// <summary>
        /// Deletes a metrics board identified by its name.
        /// </summary>
        /// <param name="boardName">The name/title of the board to delete</param>
        internal static void Delete(string boardName)
        {
            WidgetsManager widgetsManager = Find(boardName);
            if (widgetsManager is null)
            {
                Debug.LogError($"Tried to delete a board named {boardName} that does not seem to exist\n");
                return;
            }
            Destroyer.Destroy(widgetsManager.gameObject);
            widgetsManagers.Remove(widgetsManager);
            Destroyer.Destroy(widgetsManager);
        }

        /// <summary>
        /// Changes the position and rotation of a metrics board to the new position and rotation from the parameters.
        /// </summary>
        /// <param name="boardName">The name that identifies the board</param>
        /// <param name="position">The new position of the board</param>
        /// <param name="rotation">The new rotation of the board</param>
        internal static void Move(string boardName, Vector3 position, Quaternion rotation)
        {
            WidgetsManager widgetsManager = Find(boardName);
            if (widgetsManager == null)
            {
                Debug.LogError($"Tried to move a board named {boardName} that does not seem to exist\n");
                return;
            }
            Transform boardTransform = widgetsManager.transform;
            boardTransform.position = position;
            boardTransform.rotation = rotation;
        }

        /// <summary>
        /// Toggles the small buttons underneath the boards that allow the player to drag the boards around.
        /// </summary>
        /// <returns>True if the buttons are enabled now, otherwise false.</returns>
        internal static bool ToggleMoving()
        {
            movingEnabled = !movingEnabled;
            foreach (WidgetsManager controller in widgetsManagers)
            {
                controller.ToggleMoving(movingEnabled);
            }
            return movingEnabled;
        }

        /// <summary>
        /// Finds a board (its <see cref="WidgetsManager"/>, actually) by its name.
        /// </summary>
        /// <param name="boardName">The name to look for.</param>
        /// <returns>Returns the desired <see cref="WidgetsManager"/> if it exists or null if it doesn't.</returns>
        internal static WidgetsManager Find(string boardName)
        {
            return widgetsManagers.Find(manager => manager.GetTitle().Equals(boardName));
        }

        /// <summary>
        /// Returns the names of all <see cref="WidgetsManager"/>s. The names can also be used to identify the
        /// <see cref="WidgetsManager"/>s because they have to be unique.
        /// </summary>
        /// <returns>The names of all <see cref="WidgetsManager"/>s.</returns>
        internal static string[] GetNames()
        {
            string[] names = new string[widgetsManagers.Count];
            for (int i = 0; i < widgetsManagers.Count; i++)
            {
                names[i] = widgetsManagers[i].GetTitle();
            }

            return names;
        }

        /// <summary>
        /// Updates all the widgets on all the metrics boards.
        /// </summary>
        internal static void OnGraphDraw()
        {
            foreach (WidgetsManager widgetsManager in widgetsManagers)
            {
                widgetsManager.OnGraphDraw();
            }
        }

        /// <summary>
        /// This method can be invoked when you wish to let the user click on a board to add a widget.
        /// </summary>
        /// <param name="widgetConfiguration">Information on how the widget to add should be configured</param>
        internal static void AddWidgetAdders(WidgetConfig widgetConfiguration)
        {
            WidgetAdder.Setup(widgetConfiguration);
            foreach (WidgetsManager controller in widgetsManagers)
            {
                controller.gameObject.AddComponent<WidgetAdder>();
            }
        }

        /// <summary>
        /// Toggles the move-ability of all widgets.
        /// </summary>
        /// <returns>Whether or not moving is activated now</returns>
        internal static bool ToggleWidgetsMoving()
        {
            widgetsMovingEnabled = !widgetsMovingEnabled;
            foreach (WidgetsManager manager in widgetsManagers)
            {
                manager.ToggleWidgetsMoving(widgetsMovingEnabled);
            }
            return widgetsMovingEnabled;
        }

        /// <summary>
        /// Adds <see cref="WidgetDeleter"/> components to all widgets on all boards.
        /// </summary>
        internal static void AddWidgetDeleters()
        {
            foreach (WidgetsManager widgetsManager in widgetsManagers)
            {
                widgetsManager.AddWidgetDeleters();
            }
        }
    }
}
