﻿using SEE.Utils;
using UnityEngine;

namespace SEE.Controls
{
    /// <summary>
    /// Provides a logical abstraction of raw Unity inputs by the user.
    /// </summary>
    public static class SEEInput
    {
        /// <summary>
        /// If true, all logical inputs that require keyboard interactions are enabled.
        /// If false, we will not listen to keyboard inputs for any of the logical
        /// input queries. This flag is provided to disable the keyboard shortcuts
        /// when there are dialogs asking the user for keybord inputs. If the shortcuts
        /// were enabled, they would interfere with the user's input for the dialog.
        /// For instance, pressing W would enter the text "W" and move the player
        /// forward.
        /// </summary>
        public static bool KeyboardShortcutsEnabled { set; get; } = true;

        //-----------------------------------------------------
        #region General key bindings
        //-----------------------------------------------------

        /// <summary>
        /// Prints help on the key bindings.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool Help()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.Help);
        }

        /// <summary>
        /// Toggles voice input (i.e., for voice commands) on/off.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ToggleVoiceInput()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ToggleVoiceInput);
        }

        /// <summary>
        /// Turns on/off the player-action menu.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ToggleMenu()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ToggleMenu);
        }

        /// <summary>
        /// Turns on/off the browser.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ToggleBrowser()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ToggleBrowser);
        }

        /// <summary>
        /// Opens/closes the search menu.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        internal static bool ToggleSearch()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.SearchMenu);
        }

        /// <summary>
        /// True if KeyboardShortcutsEnabled and the key for the given <paramref name="digit"/>
        /// was pressed. Used as shortcuts for the menu entries.
        ///
        /// Precondition: 0 &lt;= <paramref name="digit"/> &lt;= 9.
        /// </summary>
        /// <param name="digit">the checked digit</param>
        /// <returns>true if KeyboardShortcutsEnabled and the key for the given <paramref name="digit"/>
        /// was pressed.</returns>
        public static bool DigitKeyPressed(int digit)
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyCode.Alpha1 + digit);
        }

        /// <summary>
        /// Undoes the last action.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool Undo()
        {
#if UNITY_EDITOR == false
            // Ctrl keys are not available when running the game in the editor
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
#endif
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.Undo);
#if UNITY_EDITOR == false
            }
            else
            {
                return false;
            }
#endif
        }

        /// <summary>
        /// Re-does the last action.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool Redo()
        {
#if UNITY_EDITOR == false
            // Ctrl keys are not available when running the game in the editor
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
#endif
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.Redo);
#if UNITY_EDITOR == false
            }
            else
            {
                return false;
            }
#endif
        }

        /// <summary>
        /// Un-does the last change in the CodeWindow
        /// </summary>
        /// <returns>true if the user requests this action and not <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool CodeWindowUndo()
        {
#if UNITY_EDITOR == false
            // Ctrl keys are not available when running the game in the editor
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
               return !KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.Undo);
            }
            else
            {
                return false;
            }
#else
            // Ctrl keys replaced with KeyBindings.CodeWindowUndo in the editor
            return Input.GetKeyDown(KeyBindings.CodeWindowUndo) && !KeyboardShortcutsEnabled;
#endif
        }

        /// <summary>
        /// Re-does the last change in the CodeWindow
        /// </summary>
        /// <returns>true if the user requests this action and not <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool CodeWindowRedo()
        {
#if UNITY_EDITOR == false
            // Ctrl keys are not available when running the game in the editor
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                return !KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.Redo);
            }
            return false;
#else
            //ctrl keys replaced with KeyBindings.CodeWindowUndo in the editor
            return Input.GetKeyDown(KeyBindings.CodeWindowRedo) && !KeyboardShortcutsEnabled;
#endif
        }

        /// <summary>
        /// Saves the changes made in an active code window
        /// </summary>
        /// <returns>true if the user requests this action and not <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool SaveCodeWindow()
        {
#if UNITY_EDITOR == false
            // Ctrl keys are not available when running the game in the editor
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                return !KeyboardShortcutsEnabled && Input.GetKeyDown(KeyCode.S);
            }
            else
            {
                return false;
            }
#else
            // ctrl keys replaced with KeyBindings.CodeWindowSave in the editor
            return Input.GetKeyDown(KeyBindings.CodeWindowSave) && !KeyboardShortcutsEnabled;
#endif
        }

        /// <summary>
        /// Recalculates the Syntaxhighliting
        /// </summary>
        /// <returns>true if the user requests this action and not <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ReCalculateSyntaxHighlighting()
        {
#if UNITY_EDITOR == false
           // Ctrl keys are not available when running the game in the editor
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                return !KeyboardShortcutsEnabled && Input.GetKeyDown(KeyCode.R);
            }
            return false;
#else
            // ctrl keys replaced with KeyBindings.RefreshSyntaxHighlighting in the editor
            return Input.GetKeyDown(KeyBindings.RefreshSyntaxHighlighting) && !KeyboardShortcutsEnabled;
#endif
        }

        /// <summary>
        /// Returns true if the user wants to toggle the run-time configuration
        /// menu allowing him/her to define the settings for code cities.
        /// </summary>
        /// <returns>true if the user wants to toggle the run-time configuration menu</returns>
        internal static bool ToggleConfigMenu()
        {
            return KeyboardShortcutsEnabled & Input.GetKeyDown(KeyBindings.ConfigMenu);
        }

        /// <summary>
        /// The user wants to toggle the visibility of all edges in a hovered code city.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        internal static bool ToggleEdges()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ToggleEdges);
        }

        /// <summary>
        /// The user wants to toggle the visibility of the tree view window.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        internal static bool ToggleTreeView()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.TreeView);
        }

        #endregion

        //-----------------------------------------------------
        #region Camera path recording and playing
        //-----------------------------------------------------

        /// <summary>
        /// Saves the current position when recording paths.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool SavePathPosition()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.SavePathPosition);
        }

        /// <summary>
        /// Toggles automatic path playing.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool TogglePathPlaying()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.TogglePathPlaying);
        }

        #endregion

        //-----------------------------------------------------
        #region Metric charts
        //-----------------------------------------------------

        /// <summary>
        /// Turns the metric charts on/off.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ToggleMetricCharts()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ToggleCharts);
        }

        /// <summary>
        /// Toggles hovering/selection for markers in metric charts.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ToggleMetricHoveringSelection()
        {
            return KeyboardShortcutsEnabled && Input.GetKey(KeyBindings.ToggleMetricHoveringSelection);
        }

        #endregion

        //-----------------------------------------------------
        #region Manipulating nodes
        //-----------------------------------------------------

        /// <summary>
        /// The user wants to scale a selected node.
        /// </summary>
        /// <returns>true if the user requests this action</returns>
        internal static bool Scale()
        {
            return Input.GetMouseButton(0);
        }

        #endregion

        //-----------------------------------------------------
        #region Navigation in a code city
        //-----------------------------------------------------

        /// <summary>
        /// Forgets all currently selected objects.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool Unselect()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.Unselect);
        }

        /// <summary>
        /// Toggles between the locked and free camera mode.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ToggleCameraLock()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ToggleCameraLock);
        }

        /// <summary>
        /// Cancels an action.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool Cancel()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.Cancel);
        }

        /// <summary>
        /// To reset a NavigationAction: resets position/rotation to the original position/rotation.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool Reset()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.Reset);
        }

        /// <summary>
        /// Zooms into a city.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ZoomInto()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ZoomInto);
        }

        /// <summary>
        /// While moving the city, snaps to one of eight predefined directions.
        /// While rotating the city, rotates in 45 degree steps.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool Snap()
        {
            return KeyboardShortcutsEnabled && Input.GetKey(KeyBindings.Snap);
        }

        /// <summary>
        /// The user wants to drag the hovered element of the city on its plane.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        internal static bool DragHovered()
        {
            return KeyboardShortcutsEnabled && Input.GetKey(KeyBindings.DragHovered);
        }

        /// <summary>
        /// The user wants to drag the city in its entirety or parts of it.
        /// </summary>
        /// <returns>true if the user requests this action</returns>
        internal static bool Drag()
        {
            return Input.GetMouseButton(middleMouseButton);
        }

        #endregion

        //-----------------------------------------------------
        #region Player (camera) movements.
        //-----------------------------------------------------

        /// <summary>
        /// Boosts the speed of the player movement. While pressed, movement is faster.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool BoostCameraSpeed()
        {
            return KeyboardShortcutsEnabled && Input.GetKey(KeyBindings.BoostCameraSpeed);
        }
        /// <summary>
        /// Move camera (player) forward.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool MoveForward()
        {
            return KeyboardShortcutsEnabled && Input.GetKey(KeyBindings.MoveForward);
        }
        /// <summary>
        /// Move camera (player) backward.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool MoveBackward()
        {
            return KeyboardShortcutsEnabled && Input.GetKey(KeyBindings.MoveBackward);
        }
        /// <summary>
        /// Move camera (player) to the right.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool MoveRight()
        {
            return KeyboardShortcutsEnabled && Input.GetKey(KeyBindings.MoveRight);
        }
        /// <summary>
        /// Move camera (player) to the left.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool MoveLeft()
        {
            return KeyboardShortcutsEnabled && Input.GetKey(KeyBindings.MoveLeft);
        }
        /// <summary>
        /// Move camera (player) up.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool MoveUp()
        {
            return KeyboardShortcutsEnabled && Input.GetKey(KeyBindings.MoveUp);
        }
        /// <summary>
        /// Move camera (player) down.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool MoveDown()
        {
            return KeyboardShortcutsEnabled && Input.GetKey(KeyBindings.MoveDown);
        }

        /// <summary>
        /// Index of the left mouse button.
        /// </summary>
        private const int leftMouseButton = 0;

        /// <summary>
        /// Index of the right mouse button.
        /// </summary>
        private const int rightMouseButton = 1;

        /// <summary>
        /// Index of the middle mouse button.
        /// </summary>
        private const int middleMouseButton = 2;

        /// <summary>
        /// Rotates the camera.
        /// </summary>
        /// <returns>true if the user requests this action</returns>
        public static bool RotateCamera()
        {
            return Input.GetMouseButton(rightMouseButton)
                || (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(leftMouseButton));
        }

        /// <summary>
        /// True if the user wishes to point.
        /// </summary>
        /// <returns>true if the user wishes to point</returns>
        public static bool TogglePointing()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.Pointing);
        }

        #endregion

        //--------------------------
        #region Evolution
        //--------------------------

        /// <summary>
        /// Sets a new marker.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool SetMarker()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.SetMarker);
        }
        /// <summary>
        /// Deletes a marker.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool DeleteMarker()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.DeleteMarker);
        }
        /// <summary>
        /// Toggles between between the two canvases for the animation and selection of a revision.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ToggleEvolutionCanvases()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ToggleEvolutionCanvases);
        }

        #endregion

        //----------------------------------------------------
        #region Animation (shared by Debugging and Evolution)
        //----------------------------------------------------

        /// <summary>
        /// The previous revision is to be shown.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool Previous()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.Previous);
        }
        /// <summary>
        /// The next revision is to be shown.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool Next()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.Next);
        }
        /// <summary>
        /// Toggles auto play of the animation.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ToggleAutoPlay()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ToggleAutoPlay);
        }

        /// <summary>
        /// Toggles execution order (forward/backward).
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ToggleExecutionOrder()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ToggleExecutionOrder);
        }

        /// <summary>
        /// Execution is back to very first statement.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool FirstStatement()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.FirstStatement);
        }

        /// <summary>
        /// Double animation speed.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool IncreaseAnimationSpeed()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.IncreaseAnimationSpeed);
        }

        /// <summary>
        /// Halve animation speed.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool DecreaseAnimationSpeed()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.DecreaseAnimationSpeed);
        }

        #endregion

        //--------------------------
        #region Debugging
        //--------------------------

        /// <summary>
        /// Continues execution until next breakpoint is reached.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ExecuteToBreakpoint()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ExecuteToBreakpoint);
        }

        #endregion

        //--------------------
        #region Source-code viewer
        //--------------------

        /// <summary>
        /// Toggles the menu of the open windows.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool ShowWindowMenu()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ShowWindowMenu);
        }

        #endregion

        //-------------------
        #region Selection
        //-------------------

        /// <summary>
        /// If true, selection is enabled. Selection can be disabled by action directly
        /// determining whether anything is selected; for instance, the <see cref="DeleteAction"/>
        /// listens to a selection interaction to determine the graph element to be deleted.
        /// This selection interaction should not interfere with the general <see cref="SelectAction"/>.
        /// </summary>
        public static bool SelectionEnabled = true;

        /// <summary>
        /// True if the user selects a game object (in a desktop environment, the user
        /// presses the left mouse but while the mouse cursor is not over a GUI element).
        /// Selection is enabled only if <see cref="SelectionEnabled"/>.
        /// </summary>
        /// <returns>true if the user selects a game object and <see cref="SelectionEnabled"/></returns>
        public static bool Select()
        {
            return SelectionEnabled && Input.GetMouseButtonDown(0) && !Raycasting.IsMouseOverGUI();
        }

        #endregion

        //----------------------------------------------------
        #region Chat
        //----------------------------------------------------

        /// <summary>
        /// Opens the text chat.
        /// </summary>
        /// <returns>true if the user requests this action and <see cref="KeyboardShortcutsEnabled"/></returns>
        public static bool OpenTextChat()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.OpenTextChat);
        }

        #endregion

        //----------------------------------------------------
        #region Notifications
        //----------------------------------------------------

        /// <summary>
        /// True if the user wants to close all notifications.
        /// </summary>
        /// <returns>True if the user wants to close all notifications.</returns>
        public static bool CloseAllNotifications()
        {
            return KeyboardShortcutsEnabled && Input.GetKey(KeyBindings.CloseNotifications);
        }

        #endregion

        #region FaceCam
        /// <summary>
        /// True if the user wants to turn the FaceCam on or off (toggling).
        /// </summary>
        /// <returns>True if the user wants to turn the FaceCam on or off.</returns>
        internal static bool ToggleFaceCam()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ToggleFaceCam);
        }

        /// <summary>
        /// True if the user wants to switch the position of the FaceCam on the player's face (toggling).
        /// </summary>
        /// <returns>True if the user wants to switch the position of the FaceCam.</returns>
        internal static bool ToggleFaceCamPosition()
        {
            return KeyboardShortcutsEnabled && Input.GetKeyDown(KeyBindings.ToggleFaceCamPosition);
        }

        #endregion
    }
}
