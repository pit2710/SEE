using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using SEE.Game;
using SEE.Game.City;
using SEE.UI.Notification;
using SEE.GO;
using SEE.IDE;
using SEE.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace SEE.UI.Window.CodeWindow
{
    /// <summary>
    /// This part of the <see cref="CodeWindow"/> class contains the desktop specific UI code.
    /// </summary>
    public partial class CodeWindow
    {
        /// <summary>
        /// The scroll area.
        /// </summary>
        private GameObject scrollable;

        /// <summary>
        /// Scrollbar which controls the currently visible area of the code window.
        /// </summary>
        private ScrollRect scrollRect;

        protected override void StartDesktop()
        {
            if (text == null)
            {
                Debug.LogError("Text must be defined when setting up CodeWindow!\n");
                return;
            }

            base.StartDesktop();

            scrollable = PrefabInstantiator.InstantiatePrefab(codeWindowPrefab, Window.transform.Find("Content"), false);
            scrollable.name = "Scrollable";

            // Set text and preferred font size
            GameObject code = scrollable.transform.Find("Code").gameObject;
            if (code.TryGetComponentOrLog(out textMesh))
            {
                textMesh.fontSize = FontSize;
                textMesh.text = text;
            }

            Transform breakpoints = code.transform.Find("Breakpoints");
            if (FilePath == null)
            {
                Destroyer.Destroy(breakpoints.gameObject);
            } else
            {
                foreach (Transform child in breakpoints)
                {
                    Destroyer.Destroy(child.gameObject);
                }
                Dictionary<int, SourceBreakpoint> fileBreakpoints = DebugBreakpointManager.Breakpoints.GetValueOrDefault(FilePath);
                for (int i = 0; i < lines; i++)
                {
                    GameObject breakpoint = PrefabInstantiator.InstantiatePrefab(breakpointPrefab, breakpoints, false);

                    TextMeshProUGUI dummyMesh = breakpoint.MustGetComponent<TextMeshProUGUI>();
                    dummyMesh.fontSize = FontSize;

                    int line = i;
                    Button button = breakpoint.MustGetComponent<Button>();
                    button.onClick.AddListener(() => DebugBreakpointManager.ToggleBreakpoint(FilePath, line + 1));

                    TextMeshProUGUI buttonMesh = breakpoint.transform.Find("Content").gameObject.MustGetComponent<TextMeshProUGUI>();
                    buttonMesh.color = fileBreakpoints != null && fileBreakpoints.ContainsKey(i) ? breakpointColorActive : breakpointColorInactive;

                }
                DebugBreakpointManager.OnBreakpointAdded += OnBreakpointAdded;
                DebugBreakpointManager.OnBreakpointRemoved += OnBreakpointRemoved;

            }

            var temp = SceneQueries.GetCodeCity(transform);
            if (temp && temp.gameObject.TryGetComponentOrLog(out AbstractSEECity city)) {
                // Get button for IDE interaction and register events.
                Window.transform.Find("Dragger/IDEButton").gameObject.GetComponent<Button>()
                      .onClick.AddListener(() =>
                      {
                          IDEIntegration.Instance?.OpenFileAsync(FilePath, city.SolutionPath.Path, markedLine).Forget();
                      });
            }

            // Register events to find out when window was scrolled in.
            // For this, we have to register two events in two components, namely Scrollbar and ScrollRect, with
            // OnEndDrag and OnScroll.
            if (scrollable.TryGetComponentOrLog(out scrollRect))
            {
                if (scrollRect.gameObject.TryGetComponentOrLog(out EventTrigger trigger))
                {
                    trigger.triggers.ForEach(x => x.callback.AddListener(_ => ScrollEvent.Invoke()));
                    if (!trigger.triggers.Any())
                    {
                        Debug.LogError("Event Trigger in 'ScrollRect' isn't set up correctly. "
                                       + "Triggers for the 'EndDrag' and 'Scroll' event need to be added.\n");
                    }
                }

                if (scrollRect.transform.Find("Scrollbar").gameObject.TryGetComponentOrLog(out trigger))
                {
                    trigger.triggers.ForEach(x => x.callback.AddListener(_ => ScrollEvent.Invoke()));
                    if (!trigger.triggers.Any())
                    {
                        Debug.LogError("Event Trigger in 'Scrollbar' isn't set up correctly. "
                                       + "Triggers for the 'EndDrag' and 'Scroll' event need to be added.\n");
                    }
                }
            }

            RecalculateExcessLines();

            // Animate scrollbar to scroll to desired line
            ScrolledVisibleLine = Mathf.Clamp(Mathf.FloorToInt(preStartLine), 1, lines);
        }

        /// <summary>
        /// Tooltip containing all issue descriptions.
        /// </summary>
        private Tooltip.Tooltip issueTooltip;

        protected override void UpdateDesktop()
        {
            // Show issue info on click (on hover would be too expensive)
            if (issueDictionary.Count != 0 && Input.GetMouseButtonDown(0))
            {
                // Passing camera as null causes the screen space rather than world space camera to be used
                int link = TMP_TextUtilities.FindIntersectingLink(textMesh, Input.mousePosition, null);
                if (link != -1)
                {
                    char linkId = textMesh.textInfo.linkInfo[link].GetLinkID()[0];
                    issueTooltip ??= gameObject.AddComponent<Tooltip.Tooltip>();
                    // Display tooltip containing all issue descriptions
                    UniTask.WhenAll(issueDictionary[linkId].Select(x => x.ToDisplayStringAsync()))
                           .ContinueWith(x => issueTooltip.Show(string.Join("\n", x), 0f))
                           .Forget();
                }
                else if (issueTooltip != null)
                {
                    // Hide tooltip by clicking somewhere else
                    issueTooltip.Hide();
                }
            }
            else if (issueDictionary.Count != 0 && Input.GetMouseButtonDown(1) && issueTooltip != null)
            {
                // Hide tooltip by right-clicking
                issueTooltip.Hide();
            }
        }

        /// <summary>
        /// Recalculates the <see cref="excessLines"/> using the current window height and line height of the text.
        /// This method should be called every time the window height or the line height changes.
        /// For more information, see the documentation of <see cref="excessLines"/>.
        /// </summary>
        private void RecalculateExcessLines()
        {
            try
            {
                textMesh.ForceMeshUpdate();
            }
            catch (IndexOutOfRangeException)
            {
                // FIXME (#250): Use multiple TMPs: Either one as an overlay, or split the main TMP up into multiple ones.
                ShowNotification.Error("File too big", "This file is too large to be displayed correctly.");
            }

            if (lines > 0 && Window.transform.Find("Content/Scrollable").gameObject.TryGetComponentOrLog(out RectTransform rect))
            {
                excessLines = Mathf.CeilToInt(rect.rect.height / textMesh.textInfo.lineInfo[0].lineHeight) - 2;
            }
        }

        /// <summary>
        /// Removes listeners.
        /// </summary>
        private void OnDestroy()
        {
            DebugBreakpointManager.OnBreakpointAdded -= OnBreakpointAdded;
            DebugBreakpointManager.OnBreakpointRemoved -= OnBreakpointRemoved;
        }

        /// <summary>
        /// Sets the color for added breakpoints.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="line">The source code line.</param>
        private void OnBreakpointAdded(string path, int line)
        {
            if (path == FilePath)
            {
                Transform breakpoint = scrollable.transform.Find("Code/Breakpoints").GetChild(line - 1);

                TextMeshProUGUI buttonMesh = breakpoint.transform.Find("Content").gameObject.MustGetComponent<TextMeshProUGUI>();
                buttonMesh.color = breakpointColorActive;
            }
        }

        /// <summary>
        /// Sets the color for removed breakpoints.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="line">The source code line.</param>
        private void OnBreakpointRemoved(string path, int line)
        {
            if (path == FilePath)
            {
                Transform breakpoint = scrollable.transform.Find("Code/Breakpoints").GetChild(line - 1);

                TextMeshProUGUI buttonMesh = breakpoint.transform.Find("Content").gameObject.MustGetComponent<TextMeshProUGUI>();
                buttonMesh.color = breakpointColorInactive;
            }
        }
    }
}
