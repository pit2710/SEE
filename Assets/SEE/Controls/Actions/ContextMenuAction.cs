using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using SEE.DataModel.DG;
using SEE.Game;
using SEE.Game.SceneManipulation;
using SEE.GO;
using SEE.Tools.ReflexionAnalysis;
using SEE.UI.Notification;
using SEE.UI.PopupMenu;
using SEE.UI.Window;
using SEE.UI.Window.TreeWindow;
using SEE.Utils;
using UnityEngine;
using SEE.Game.City;
using SEE.Utils.History;
using SEE.GO.Menu;
using SEE.UI.Menu.Drawable;
using SEE.UI.Window.PropertyWindow;
using Crosstales.RTVoice;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Shows a context menu with available actions when the user requests it.
    /// </summary>
    public class ContextMenuAction : MonoBehaviour
    {
        /// <summary>
        /// The popup menu that is shown when the user requests the context menu.
        /// </summary>
        private PopupMenu popupMenu;

        /// <summary>
        /// The position where the menu should be opened.
        /// </summary>
        private Vector3 position;

        /// <summary>
        /// The interactable object during the start must be the same as when
        /// the right mouse button is released in order for the context menu to open.
        /// </summary>
        private InteractableObject startObject;

        /// <summary>
        /// Tries to open the context menu with multiselection.
        /// </summary>
        private bool multiselection = false;

        private void Start()
        {
            popupMenu = gameObject.AddComponent<PopupMenu>();
        }

        private void Update()
        {
            if (SEEInput.StartOpenContextMenu())
            {
                if (InteractableObject.SelectedObjects.Count <= 1)
                {
                    Raycasting.RaycastInteractableObject(out _, out InteractableObject o);
                    startObject = o;
                    multiselection = false;
                }
                else
                {
                    startObject = null;
                    multiselection = true;
                }
            }
            if (SEEInput.OpenContextMenu())
            {
                if (!multiselection)
                {
                    HitGraphElement hit = Raycasting.RaycastInteractableObject(out RaycastHit raycastHit, out InteractableObject o);
                    if (hit == HitGraphElement.None)
                    {
                        return;
                    }
                    if (o == startObject)
                    {
                        position = Input.mousePosition;
                        IEnumerable<PopupMenuEntry> entries = GetApplicableOptions(popupMenu, position, raycastHit.point, o.GraphElemRef.Elem, o.gameObject);
                        popupMenu.ShowWith(entries, position);
                    }
                }
                else
                {
                    // TODO (#664): Detect if multiple elements are selected and adjust options accordingly.
                    HitGraphElement hit = Raycasting.RaycastInteractableObject(out RaycastHit raycastHit, out InteractableObject o);
                    if (hit == HitGraphElement.None)
                    {
                        return;
                    }
                    if (InteractableObject.SelectedObjects.Contains(o))
                    {
                        position = Input.mousePosition;
                        IEnumerable<PopupMenuEntry> entries = GetApplicableOptionsForMultiselection(popupMenu, InteractableObject.SelectedObjects);
                        popupMenu.ShowWith(entries, position);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the context menu.
        /// </summary>
        /// <param name="popupMenu">The popup menu in which the options should be displayed.</param>
        /// <param name="position">The position to be displayed the popup menu.</param>
        /// <param name="entries">The new entries for the context menu.</param>
        private static void UpdateEntries(PopupMenu popupMenu, Vector3 position, IEnumerable<PopupMenuEntry> entries)
        {
            popupMenu.ShowWith(entries, position);
        }

        #region Multiple-Selection
        /// <summary>
        /// Returns the options available for multiple selection.
        /// </summary>
        /// <param name="popupMenu">The popup menu in which the options should be displayed.</param>
        /// <param name="selectedObjects">The selected objects.</param>
        /// <returns>Options available for the selected objects.</returns>
        private IEnumerable<PopupMenuEntry> GetApplicableOptionsForMultiselection(PopupMenu popupMenu, HashSet<InteractableObject> selectedObjects)
        {
            List<PopupMenuEntry> entries = new()
            {
                new PopupMenuHeading("More graph elements selected!", int.MaxValue),

                new PopupMenuActionDoubleIcon("Inspect", () =>
                {
                    List<PopupMenuEntry> submenuEntries = new()
                    {
                        new PopupMenuAction("Inspect", () =>
                        {
                            UpdateEntries(popupMenu, position, GetApplicableOptionsForMultiselection(popupMenu, selectedObjects));
                        }, Icons.ArrowLeft, CloseAfterClick: false),
                        new PopupMenuAction("Properties", ShowProperties, Icons.Info),
                        new PopupMenuAction("Show Metrics", ShowMetrics, Icons.Info),
                        new PopupMenuAction("Show in City", Highlight, Icons.LightBulb)
                    };

                    if (selectedObjects.Any(o => o.GraphElemRef.Elem.Filename != null))
                    {
                        submenuEntries.Add(new PopupMenuAction("Show Code", ShowCode, Icons.Code));
                        if (selectedObjects.Any(o => o.gameObject.ContainingCity<DiffCity>() != null))
                        {
                            submenuEntries.Add(new PopupMenuAction("Show Code Diff", ShowDiffCode, Icons.Code));
                        }
                    }
                    UpdateEntries(popupMenu, position, submenuEntries);
                },
                Icons.Info, Icons.ArrowRight, CloseAfterClick: false, Priority: 5),
                new PopupMenuAction("Delete", Delete, Icons.Trash)
            };

            foreach (InteractableObject iO in selectedObjects)
            {
                // ACCEPT DIVERGENCE
            }
            return entries;

            void Delete()
            {
                ActionStateType previousAction = GlobalActionHistory.Current();
                GlobalActionHistory.Execute(ActionStateTypes.Delete);
                DeleteAction action = (DeleteAction)GlobalActionHistory.CurrentAction();
                action.ContextMenuExecution(selectedObjects.Select(iO => iO.gameObject));
                ExcecutePreviousAction(action, previousAction);
            }

            void ShowProperties()
            {
                foreach (InteractableObject iO in selectedObjects)
                {
                    if (iO.gameObject != null)
                    {
                        ActivateWindow(CreatePropertyWindow(iO.gameObject.MustGetComponent<GraphElementRef>()));
                    }
                }
            }

            void ShowMetrics()
            {
                foreach (InteractableObject iO in selectedObjects)
                {
                    if (iO.gameObject != null)
                    {
                        ActivateWindow(CreateMetricWindow(iO.gameObject.MustGetComponent<GraphElementRef>()));
                    }
                }
            }

            void ShowCode()
            {
                foreach(InteractableObject iO in selectedObjects)
                {
                    if (iO.gameObject != null)
                    {
                        ActivateWindow(ShowCodeAction.ShowCode(iO.gameObject.MustGetComponent<GraphElementRef>()));
                    }
                }
            }

            void ShowDiffCode()
            {
                foreach(InteractableObject iO in selectedObjects)
                {
                    if (iO.gameObject != null && iO.gameObject.ContainingCity<DiffCity>())
                    {
                        ActivateWindow(ShowCodeAction.ShowVCSDiff(iO.gameObject.MustGetComponent<GraphElementRef>(),
                                                          iO.gameObject.ContainingCity<DiffCity>()));
                    }
                }
            }

            void Highlight()
            {
                foreach (InteractableObject iO in selectedObjects)
                {
                    if (iO.gameObject != null)
                    {
                        iO.gameObject.Operator().Highlight(duration: 10);
                    }
                }
            }
        }


        #endregion

        #region Single-Selection
        /// <summary>
        /// Returns the options available for the given graph element.
        /// </summary>
        /// <param name="popupMenu">The popup menu in which the options should be displayed.</param>
        /// <param name="position">The context menu position.</param>
        /// <param name="graphElement">The graph element to get the options for</param>
        /// <param name="gameObject">The game object that the graph element is attached to</param>
        /// <returns>Options available for the given graph element</returns>
        /// <param name="appendActions">Actions to be append at the end of the entries.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the graph element is neither a node nor an edge</exception>
        public static IEnumerable<PopupMenuAction> GetOptionsForTreeView(PopupMenu popupMenu, Vector3 position, GraphElement graphElement,
            GameObject gameObject = null, IEnumerable<PopupMenuAction> appendActions = null)
        {
            List<PopupMenuAction> actions = new();
            actions.AddRange(GetApplicableOptions(popupMenu, position, position, graphElement, gameObject, appendActions).OfType<PopupMenuAction>());

            return actions;
        }

        /// <summary>
        /// Returns the options available for the given graph element.
        /// </summary>
        /// <param name="popupMenu">The popup menu in which the options should be displayed.</param>
        /// <param name="position">The context menu position.</param>
        /// <param name="raycastHitPosition">The position of the raycast hit.</param>
        /// <param name="graphElement">The graph element to get the options for</param>
        /// <param name="gameObject">The game object that the graph element is attached to</param>
        /// <returns>Options available for the given graph element</returns>
        /// <param name="appendActions">Actions to be append at the end of the entries.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the graph element is neither a node nor an edge</exception>
        private static IEnumerable<PopupMenuEntry> GetApplicableOptions(PopupMenu popupMenu, Vector3 position, Vector3 raycastHitPosition,
            GraphElement graphElement, GameObject gameObject = null, IEnumerable<PopupMenuAction> appendActions = null)
        {
            IEnumerable<PopupMenuEntry> options = GetCommonOptions(popupMenu, position, raycastHitPosition, graphElement, gameObject, appendActions);
            return options.Concat(graphElement switch
            {
                Node node => GetNodeOptions(popupMenu, position, raycastHitPosition, node, gameObject, appendActions),
                Edge edge => GetEdgeOptions(popupMenu, position, raycastHitPosition, edge, gameObject, appendActions),
                _ => throw new ArgumentOutOfRangeException()
            });
        }

        /// <summary>
        /// Returns the common options available for all graph elements.
        /// </summary>
        /// <param name="popupMenu">The popup menu in which the options should be displayed.</param>
        /// <param name="position">The position to be displayed the popup menu.</param>
        /// <param name="raycastHitPosition">The position of the raycast hit.</param>
        /// <param name="graphElement">The graph element to get the options for</param>
        /// <param name="gameObject">The game object that the graph element is attached to</param>
        /// <param name="appendActions">Actions to be append at the end of the entries.</param>
        /// <returns>Common options available for all graph elements</returns>
        private static IEnumerable<PopupMenuEntry> GetCommonOptions(PopupMenu popupMenu, Vector3 position, Vector3 raycastHitPosition,
            GraphElement graphElement, GameObject gameObject = null, IEnumerable<PopupMenuAction> appendActions = null)
        {
            string name = graphElement.ID;
            string target, source = target = null;
            if (graphElement is Node node
                && !string.IsNullOrEmpty(node.SourceName))
            {
                name = node.SourceName;
            }
            if (graphElement is Edge edge)
            {
                name = edge.Type;
                source = edge.Source.SourceName ?? edge.Source.ID;
                target = edge.Target.SourceName ?? edge.Target.ID;
            }
            IList<PopupMenuEntry> entries = new List<PopupMenuEntry>
            {
                new PopupMenuHeading(name, Priority: int.MaxValue)
            };
            if (source != null && target != null)
            {
                entries.Add(new PopupMenuHeading("Source: " + source, Priority: int.MaxValue));
                entries.Add(new PopupMenuHeading("Target: " + target, Priority: int.MaxValue));
            }
            entries.Add(new PopupMenuAction("Delete", DeleteElement, Icons.Trash, Priority: 0));

            entries.Add(new PopupMenuActionDoubleIcon("Inspect", () =>
            {
                List<PopupMenuEntry> subMenuEntries = new()
                    {
                        new PopupMenuAction("Inspect", () =>
                        {
                            if (appendActions != null)
                            {
                                List<PopupMenuAction> actions = new (GetApplicableOptions(popupMenu, position, raycastHitPosition,
                                                                        graphElement, gameObject, appendActions)
                                                                    .OfType<PopupMenuAction>()
                                                                    .Where(x=>!x.Name.Contains("TreeWindow")));
                                actions.AddRange(appendActions);
                                UpdateEntries(popupMenu, position, actions);
                            }
                            else
                            {
                                UpdateEntries(popupMenu, position, GetApplicableOptions(popupMenu, position, raycastHitPosition, graphElement, gameObject));
                            }
                        },
                            Icons.ArrowLeft, CloseAfterClick: false),
                        new PopupMenuAction("Properties", ShowProperties, Icons.Info),
                        new PopupMenuAction("Show Metrics", ShowMetrics, Icons.Info),
                    };
                if (gameObject != null)
                {
                    subMenuEntries.Add(new PopupMenuAction("Show in City", Highlight, Icons.LightBulb));
                }

                if (graphElement.Filename != null)
                {
                    subMenuEntries.Add(new PopupMenuAction("Show Code", ShowCode, Icons.Code));
                    if (gameObject.ContainingCity<DiffCity>() != null)
                    {
                        subMenuEntries.Add(new PopupMenuAction("Show Code Diff", ShowDiffCode, Icons.Code));
                    }
                }
                subMenuEntries.AddRange(graphElement switch
                {
                    Node node => GetNodeShowOptions(node, gameObject, appendActions != null),
                    Edge edge => GetEdgeShowOptions(edge, gameObject),
                    _ => throw new ArgumentOutOfRangeException()
                });
                UpdateEntries(popupMenu, position, subMenuEntries);

            }, Icons.Info, Icons.ArrowRight, CloseAfterClick: false, Priority: 1));

            return entries;

            void DeleteElement()
            {
                if (gameObject != null)
                {
                    ActionStateType previousAction = GlobalActionHistory.Current();
                    GlobalActionHistory.Execute(ActionStateTypes.Delete);
                    DeleteAction action = (DeleteAction)GlobalActionHistory.CurrentAction();
                    action.ContextMenuExecution(gameObject);
                    ExcecutePreviousAction(action, previousAction);
                }
                else
                {
                    // TODO: Test the case. I'm not sure when this case would be triggered.
                    // It should be better documented how this case could occur.
                    ConfirmDialogMenu confirm = new($"Do you really want to delete the element {graphElement.ID}?\r\nThis action cannot be undone.");
                    confirm.ExecuteAfterConfirmAsync(() => graphElement.ItsGraph.RemoveElement(graphElement)).Forget();
                }
            }

            void ShowProperties()
            {
                ActivateWindow(CreatePropertyWindow(gameObject.MustGetComponent<GraphElementRef>()));
            }

            void ShowMetrics()
            {
                ActivateWindow(CreateMetricWindow(gameObject.MustGetComponent<GraphElementRef>()));
            }

            void ShowCode()
            {
                ActivateWindow(ShowCodeAction.ShowCode(gameObject.MustGetComponent<GraphElementRef>()));
            }

            void ShowDiffCode()
            {
                ActivateWindow(ShowCodeAction.ShowVCSDiff(gameObject.MustGetComponent<GraphElementRef>(),
                                                          gameObject.ContainingCity<DiffCity>()));
            }

            void Highlight()
            {
                if (gameObject != null)
                {
                    gameObject.Operator().Highlight(duration: 10);
                }
                else
                {
                    ShowNotification.Warn("No game object", "There is nothing to highlight for this element.");
                }
            }
        }

        /// <summary>
        /// Returns the show options available for the given node.
        /// </summary>
        /// <param name="node">The node to get the show options for</param>
        /// <param name="gameObject">The game object that the node is attached to</param>
        /// <param name="openViaTreeView">Whether the popup menu was opened via context menu.</param>
        /// <returns>Show options available for the given node</returns>
        private static IEnumerable<PopupMenuEntry> GetNodeShowOptions(Node node, GameObject gameObject, bool openViaTreeView)
        {
            List<PopupMenuEntry> actions = new();
            if (!openViaTreeView)
            {
                actions.Add(new PopupMenuAction("Reveal in TreeView", RevealInTreeView, Icons.TreeView));
            }
            if (node.OutgoingsOfType(LSP.Reference).Any())
            {
                actions.Add(new PopupMenuAction("Show References", () => ShowTargets(LSP.Reference, false).Forget(), Icons.IncomingEdge));
            }
            if (node.OutgoingsOfType(LSP.Declaration).Any())
            {
                actions.Add(new PopupMenuAction("Show Declaration", () => ShowTargets(LSP.Declaration).Forget(), Icons.OutgoingEdge));
            }
            if (node.OutgoingsOfType(LSP.Definition).Any())
            {
                actions.Add(new PopupMenuAction("Show Definition", () => ShowTargets(LSP.Definition).Forget(), Icons.OutgoingEdge));
            }
            if (node.OutgoingsOfType(LSP.OfType).Any())
            {
                actions.Add(new PopupMenuAction("Show Type", () => ShowTargets(LSP.OfType).Forget(), 'T'));
            }
            return actions;


            void RevealInTreeView()
            {
                ActivateTreeWindow(node, gameObject.transform).RevealElementAsync(node).Forget();
            }

            // Highlights all nodes that are targets of the given kind of edge.
            async UniTaskVoid ShowTargets(string kind, bool outgoings = true)
            {
                IList<Node> nodes;
                if (outgoings)
                {
                    nodes = node.OutgoingsOfType(kind).Select(e => e.Target).ToList();
                }
                else
                {
                    nodes = node.IncomingsOfType(kind).Select(e => e.Source).ToList();
                }
                if (nodes.Count == 1)
                {
                    // We will just highlight the target node directly.
                    nodes.First().Operator().Highlight(duration: 10);
                }
                else
                {
                    TreeWindow window = ActivateTreeWindow(node, gameObject.transform, title: $"{kind}s of " + node.SourceName);
                    await UniTask.Yield();
                    window.ConstrainToAsync(nodes).Forget();
                }
            }
        }

        /// <summary>
        /// Returns the options available for the given node.
        /// </summary>
        /// <param name="popupMenu">The popup menu in which the options should be displayed.</param>
        /// <param name="position">The position to be displayed the popup menu.</param>
        /// <param name="raycastHitPosition">The position of the raycast hit.</param>
        /// <param name="node">The node to get the options for</param>
        /// <param name="gameObject">The game object that the node is attached to</param>
        /// <param name="appendActions">Actions to be append at the end of the entries.</param>
        /// <returns>Options available for the given node</returns>
        private static IEnumerable<PopupMenuEntry> GetNodeOptions(PopupMenu popupMenu, Vector3 position, Vector3 raycastHitPosition,
            Node node, GameObject gameObject, IEnumerable<PopupMenuAction> appendActions)
        {
            IList<PopupMenuEntry> actions = new List<PopupMenuEntry>
            {
                new PopupMenuAction("Edit Node", EditNode, Icons.PenToSquare, Priority: 1),
            };
            if (appendActions == null)
            {
                if (!node.IsRoot())
                {
                    actions.Add(new PopupMenuAction("Move", MoveNode, Icons.Move, Priority: 5));
                }
                actions.Add(new PopupMenuAction("New Node", NewNode, '+', Priority: 3));
                actions.Add(new PopupMenuAction("New Edge", NewEdge, Icons.Edge, Priority: 2));

                if (gameObject != null)
                {
                    VisualNodeAttributes gameNodeAttributes = gameObject.ContainingCity().NodeTypes[node.Type];
                    if (gameNodeAttributes.AllowManualResize)
                    {
                        actions.Add(new PopupMenuAction("Rotate", RotateNode, Icons.Rotate, Priority: 4));
                        actions.Add(new PopupMenuAction("Resize Node", ResizeNode, Icons.Resize));
                        actions.Add(new PopupMenuAction("Scale Node", ScaleNode, Icons.Scale));
                    }
                }
            }

            return new List<PopupMenuEntry>() { CreateSubMenu(popupMenu, position, raycastHitPosition,
                "Node Options", Icons.Node, actions, node, gameObject, 2, appendActions) };

            void MoveNode()
            {
                ActionStateType previousAction = GlobalActionHistory.Current();
                GlobalActionHistory.Execute(ActionStateTypes.Move);
                UpdatePlayerMenu();
                MoveAction action = (MoveAction)GlobalActionHistory.CurrentAction();
                action.ContextMenuExecution(gameObject, raycastHitPosition);
                ExcecutePreviousAction(action, previousAction);
            }

            void RotateNode()
            {
                ActionStateType previousAction = GlobalActionHistory.Current();
                GlobalActionHistory.Execute(ActionStateTypes.Rotate);
                UpdatePlayerMenu();
                RotateAction action = (RotateAction)GlobalActionHistory.CurrentAction();
                action.ContextMenuExecution(gameObject);
                ExcecutePreviousAction(action, previousAction);
            }

            void NewNode()
            {
                ActionStateType previousAction = GlobalActionHistory.Current();
                GlobalActionHistory.Execute(ActionStateTypes.NewNode);
                AddNodeAction action = (AddNodeAction)GlobalActionHistory.CurrentAction();
                action.ContextMenuExecution(gameObject, raycastHitPosition);
                ExcecutePreviousAction(action, previousAction);
            }

            void NewEdge()
            {
                ActionStateType previousAction = GlobalActionHistory.Current();
                GlobalActionHistory.Execute(ActionStateTypes.NewEdge);
                UpdatePlayerMenu();
                AddEdgeAction action = (AddEdgeAction)GlobalActionHistory.CurrentAction();
                action.ContextMenuExecution(gameObject);
                ExcecutePreviousAction(action, previousAction);
            }

            void EditNode()
            {
                ActionStateType previousAction = GlobalActionHistory.Current();
                GlobalActionHistory.Execute(ActionStateTypes.EditNode);
                UpdatePlayerMenu();
                EditNodeAction action = (EditNodeAction)GlobalActionHistory.CurrentAction();
                action.ContextMenuExecution(node);
                ExcecutePreviousAction(action, previousAction);
            }

            void ResizeNode()
            {
                ActionStateType previousAction = GlobalActionHistory.Current();
                GlobalActionHistory.Execute(ActionStateTypes.ResizeNode);
                UpdatePlayerMenu();
                ResizeNodeAction action = (ResizeNodeAction)GlobalActionHistory.CurrentAction();
                action.ContextMenuExecution(gameObject);
                ExcecutePreviousAction(action, previousAction);
            }

            void ScaleNode()
            {
                ActionStateType previousAction = GlobalActionHistory.Current();
                GlobalActionHistory.Execute(ActionStateTypes.ScaleNode);
                UpdatePlayerMenu();
                ScaleNodeAction action = (ScaleNodeAction)GlobalActionHistory.CurrentAction();
                action.ContextMenuExecution(gameObject);
                ExcecutePreviousAction(action, previousAction);
            }
        }

        /// <summary>
        /// Returns the show options available for the given edge.
        /// </summary>
        /// <param name="edge">The edge to get the show options for</param>
        /// <param name="gameObject">The game object that the edge is attached to</param>
        /// <returns>Show options available for the given edge</returns>
        private static IEnumerable<PopupMenuEntry> GetEdgeShowOptions(Edge edge, GameObject gameObject)
        {
            List<PopupMenuEntry> entries = new() {
                new PopupMenuAction("Show at Source (TreeView)", RevealAtSource, Icons.TreeView),
                new PopupMenuAction("Show at Target (TreeView)", RevealAtTarget, Icons.TreeView)
            };

            if (edge.Type == "Clone")
            {
                entries.Add(new PopupMenuAction("Show Unified Diff", ShowUnifiedDiff, Icons.Compare));
            }

            return entries;


            void RevealAtSource()
            {
                ActivateTreeWindow(edge, gameObject.transform).RevealElementAsync(edge, viaSource: true).Forget();
            }

            void RevealAtTarget()
            {
                ActivateTreeWindow(edge, gameObject.transform).RevealElementAsync(edge, viaSource: false).Forget();
            }

            void ShowUnifiedDiff()
            {
                ActivateWindow(ShowCodeAction.ShowUnifiedDiff(gameObject.MustGetComponent<EdgeRef>()));
            }

        }

        /// <summary>
        /// Returns the options available for the given edge.
        /// </summary>
        /// <param name="popupMenu">The popup menu in which the options should be displayed.</param>
        /// <param name="position">The position to be displayed the popup menu.</param>
        /// <param name="raycastHitPosition">The position of the raycast hit.</param>
        /// <param name="edge">The edge to get the options for</param>
        /// <param name="gameObject">The game object that the edge is attached to</param>
        /// <param name="appendActions">Options to be append at the end of the entries.</param>
        /// <returns>Options available for the given edge</returns>
        private static IEnumerable<PopupMenuEntry> GetEdgeOptions(PopupMenu popupMenu, Vector3 position, Vector3 raycastHitPosition,
            Edge edge, GameObject gameObject, IEnumerable<PopupMenuAction> appendActions = null)
        {
            IList<PopupMenuEntry> actions = new List<PopupMenuEntry>
            {
                //new PopupMenuAction("Edit Edge", EditEdge, Icons.PenToSquare)
            };

            if (edge.IsInImplementation() && ReflexionGraph.IsDivergent(edge))
            {
                actions.Add(new PopupMenuAction("Accept Divergence", AcceptDivergence, Icons.Checkmark));
            }

            List<PopupMenuEntry> entries = new();
            if (actions.Count > 0)
            {
                entries.Add(CreateSubMenu(popupMenu, position, raycastHitPosition,
                    "Edge Options", Icons.Node, actions, edge, gameObject, 2, appendActions));
            }
            return entries;

            void AcceptDivergence()
            {
                ActionStateType previousAction = GlobalActionHistory.Current();
                GlobalActionHistory.Execute(ActionStateTypes.AcceptDivergence);
                AcceptDivergenceAction action = (AcceptDivergenceAction)GlobalActionHistory.CurrentAction();
                action.ContextMenuExecution(edge);
                ExcecutePreviousAction(action, previousAction);
            }

            void EditEdge()
            {
                // TODO: Useful?
                ShowNotification.Info("Not implemented", "The action is not implemented yet.");
            }
        }
        #endregion

        /// <summary>
        /// Activates the tree window for the given graph element and returns it.
        /// </summary>
        /// <param name="graphElement">The graph element to activate the tree window for</param>
        /// <param name="transform">The transform of the game object that the graph element is attached to</param>
        /// <param name="title">The title of the tree window to be used. Should only be set if this is not supposed
        /// to be the main tree window.</param>
        /// <returns>The activated tree window</returns>
        private static TreeWindow ActivateTreeWindow(GraphElement graphElement, Transform transform, string title = null)
        {
            WindowSpace manager = WindowSpaceManager.ManagerInstance[WindowSpaceManager.LocalPlayer];
            TreeWindow openWindow = manager.Windows.OfType<TreeWindow>().FirstOrDefault(x => x.Graph == graphElement.ItsGraph && (title == null || x.Title == title));
            if (openWindow == null)
            {
                // Window is not open yet, so we create it.
                GameObject city = SceneQueries.GetCodeCity(transform).gameObject;
                openWindow = city.AddComponent<TreeWindow>();
                openWindow.Graph = graphElement.ItsGraph;
                if (title != null)
                {
                    openWindow.Title = title;
                }
                manager.AddWindow(openWindow);
            }
            manager.ActiveWindow = openWindow;
            return openWindow;
        }

        /// <summary>
        /// Returns a <see cref="MetricWindow"/> showing the attributes of <paramref name="graphElementRef"/>.
        /// </summary>
        /// <param name="graphElementRef">The graph element to activate the metric window for</param>
        /// <returns>The <see cref="MetricWindow"/> object showing the attributes of the specified graph element.</returns>
        private static MetricWindow CreateMetricWindow(GraphElementRef graphElementRef)
        {
            // Create new window for active selection, or use existing one
            if (!graphElementRef.TryGetComponent(out MetricWindow metricMenu))
            {
                metricMenu = graphElementRef.gameObject.AddComponent<MetricWindow>();
                metricMenu.Title = "Metrics for " + graphElementRef.Elem.ToShortString();
                metricMenu.GraphElement = graphElementRef.Elem;
            }
            return metricMenu;
        }

        /// <summary>
        /// Returns a <see cref="PropertyWindow"/> showing the attributes of <paramref name="graphElementRef"/>.
        /// </summary>
        /// <param name="graphElementRef">The graph element to activate the property window for</param>
        /// <returns>The <see cref="PropertyWindow"/> object showing the attributes of the specified graph element.</returns>
        private static PropertyWindow CreatePropertyWindow(GraphElementRef graphElementRef)
        {
            // Create new window for active selection, or use existing one
            if (!graphElementRef.TryGetComponent(out PropertyWindow propertyMenu))
            {
                propertyMenu = graphElementRef.gameObject.AddComponent<PropertyWindow>();
                propertyMenu.Title = "Properties for " + graphElementRef.Elem.ToShortString();
                propertyMenu.GraphElement = graphElementRef.Elem;
            }
            return propertyMenu;
        }

        /// <summary>
        /// Activates the given window, that is, adds it to the window space and makes it the active window.
        /// </summary>
        /// <param name="window">The window to activate</param>
        private static void ActivateWindow(BaseWindow window)
        {
            WindowSpace manager = WindowSpaceManager.ManagerInstance[WindowSpaceManager.LocalPlayer];
            if (!manager.Windows.Contains(window))
            {
                manager.AddWindow(window);
            }
            manager.ActiveWindow = window;
        }

        /// <summary>
        /// Creates a sub menu for the context menu.
        /// </summary>
        /// <param name="popupMenu">The popup menu in which the options should be displayed.</param>
        /// <param name="position">The position to be displayed the popup menu.</param>
        /// <param name="raycastHitPosition">The position of the raycast hit.</param>
        /// <param name="name">The name for the sub menu.</param>
        /// <param name="icon">The icon for the sub menu.</param>
        /// <param name="actions">A list of the actions which should be displayed in the sub menu.</param>
        /// <param name="graphElement">The graph element to get the options for</param>
        /// <param name="gameObject">The game object that the graph element is attached to</param>
        /// <param name="priority">The priority for this sub menu.</param>
        /// <param name="appendActions">Actions to be append at the end of the entries.</param>
        /// <returns>The created sub menu.</returns>
        private static PopupMenuActionDoubleIcon CreateSubMenu(PopupMenu popupMenu, Vector3 position, Vector3 raycastHitPosition,
            string name, char icon, IEnumerable<PopupMenuEntry> actions,
            GraphElement graphElement, GameObject gameObject = null, int priority = 0, IEnumerable<PopupMenuAction> appendActions = null)
        {
            return new(name, () =>
            {
                List<PopupMenuEntry> entries = new()
                    {
                        new PopupMenuAction(name, () =>
                        {
                            if (appendActions != null)
                            {
                                List<PopupMenuAction> treeViewActions = new (GetApplicableOptions(popupMenu, position, raycastHitPosition,
                                                                        graphElement, gameObject, appendActions)
                                                                    .OfType<PopupMenuAction>()
                                                                    .Where(x=>!x.Name.Contains("TreeWindow")));
                                treeViewActions.AddRange(appendActions);
                                UpdateEntries(popupMenu, position, treeViewActions);
                            }
                            else
                            {
                                UpdateEntries(popupMenu, position, GetApplicableOptions(popupMenu, position, raycastHitPosition, graphElement, gameObject));
                            }
                        },
                        Icons.ArrowLeft, CloseAfterClick: false, Priority: int.MaxValue)
                    };
                entries.AddRange(actions);
                UpdateEntries(popupMenu, position, entries);

            }, icon, Icons.ArrowRight, CloseAfterClick: false, priority);
        }

        /// <summary>
        /// Ensures that the previous action is executed again after the current action has been fully completed (<see cref="IReversibleAction.Progress.Completed"/>).
        /// Additionally, the <see cref="PlayerMenu"> is updated.
        /// </summary>
        /// <param name="action">The current action which was executed via context menu.</param>
        /// <param name="previousAction">The previously executed action to be re-executed.</param>
        private static async void ExcecutePreviousAction(IReversibleAction action, ActionStateType previousAction)
        {
            while (action.CurrentProgress() != IReversibleAction.Progress.Completed)
            {
                await UniTask.Yield();
            }
            GlobalActionHistory.Execute(previousAction);
            UpdatePlayerMenu();
        }

        /// <summary>
        /// Updates the current active entry in the <see cref="PlayerMenu"/>.
        /// </summary>
        private static void UpdatePlayerMenu()
        {
            LocalPlayer.TryGetPlayerMenu(out PlayerMenu menu);
            menu.UpdateActiveEntry();
        }
    }
}
