using System;
using System.Collections.Generic;
using System.Linq;
using SEE.Controls.Actions.HolisticMetrics;
using SEE.Game.UI.Notification;
using UnityEngine;
using SEE.Game.HolisticMetrics.Components;
using Object = UnityEngine.Object;

namespace SEE.Game.HolisticMetrics
{
    /// <summary>
    /// This class manages all metrics boards.
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
        /// List of all the <see cref="WidgetsManager"/>s that this manager manages (there should not be any
        /// <see cref="WidgetsManager"/>s in the scene that are not in this list).
        /// </summary>
        private static readonly List<WidgetsManager> widgetsManagers = new List<WidgetsManager>();

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
            Object.Destroy(widgetsManager.gameObject);
            widgetsManagers.Remove(widgetsManager);
            Object.Destroy(widgetsManager);
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

        internal static bool GetMovement(out string boardName, out Vector3 oldPosition, out Vector3 newPosition,
            out Quaternion oldRotation, out Quaternion newRotation)
        {
            foreach (WidgetsManager widgetsManager in widgetsManagers)
            {
                if (widgetsManager.GetMovement(out oldPosition, out newPosition, out oldRotation,
                        out newRotation))
                {
                    boardName = widgetsManager.GetTitle();
                    return true;
                }
            }

            boardName = null;
            oldPosition = Vector3.zero;
            newPosition = Vector3.zero;
            oldRotation = Quaternion.identity;
            newRotation = Quaternion.identity;
            return false;
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
        internal static void AddWidgetAdders()
        {
            WidgetAdder.Setup();
            foreach (WidgetsManager controller in widgetsManagers)
            {
                controller.gameObject.AddComponent<WidgetAdder>();
            }
        }

        internal static bool GetWidgetAdditionPosition(out string boardName, out Vector3 position)
        {
            foreach (WidgetsManager widgetsManager in widgetsManagers)
            {
                if (widgetsManager.gameObject.GetComponent<WidgetAdder>().GetPosition(out position))
                {
                    boardName = widgetsManager.GetTitle();
                    return true;
                }
            }

            boardName = null;
            position = Vector3.zero;
            return false;
        }

        /// <summary>
        /// Toggles the move-ability of all widgets.
        /// </summary>
        /// <param name="enable">Whether the widgets should be movable</param>
        internal static void ToggleWidgetsMoving(bool enable)
        {
            foreach (WidgetsManager manager in widgetsManagers)
            {
                manager.ToggleWidgetsMoving(enable);
            }
        }
        
        /// <summary>
        /// Check whether one of the widgets on one of the boards managed by this class has a movement that hasn't yet
        /// been fetched by the <see cref="MoveWidgetAction"/>.
        /// </summary>
        /// <param name="originalPosition">The position of the widget before the movement</param>
        /// <param name="newPosition">The position of the widget after the movement</param>
        /// <param name="containingBoardName">The title of the board that contains the widget</param>
        /// <param name="widgetID">The ID of the widget</param>
        /// <returns>whether one of the widgets on one of the boards managed by this class has a movement that hasn't yet
        /// been fetched by the <see cref="MoveWidgetAction"/></returns>
        internal static bool GetWidgetMovement(
            out Vector3 originalPosition,
            out Vector3 newPosition,
            out string containingBoardName,
            out Guid widgetID)
        {
            foreach (WidgetsManager widgetsManager in widgetsManagers)
            {
                if (widgetsManager.GetWidgetMovement(
                        out originalPosition, 
                        out newPosition, 
                        out widgetID))
                {
                    containingBoardName = widgetsManager.GetTitle();
                    return true;
                }
            }

            originalPosition = Vector3.zero;
            newPosition = Vector3.zero;
            containingBoardName = null;
            widgetID = Guid.NewGuid();
            return false;
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

        /// <summary>
        /// Tries to get a pending deletion of a widget from any of the boards managed by this manager.
        /// </summary>
        /// <param name="boardName">The name of the board that contains the widget that's to be deleted</param>
        /// <param name="widgetConfig">The configuration of the widget that's to be deleted</param>
        /// <returns>Whether a pending deletion was found</returns>
        internal static bool GetWidgetDeletion(out string boardName, out WidgetConfig widgetConfig)
        {
            foreach (WidgetsManager widgetsManager in widgetsManagers)
            {
                if (widgetsManager.GetWidgetDeletion(out widgetConfig))
                {
                    boardName = widgetsManager.GetTitle();
                    return true;
                }
            }

            boardName = null;
            widgetConfig = null;
            return false;
        }
    }
}
