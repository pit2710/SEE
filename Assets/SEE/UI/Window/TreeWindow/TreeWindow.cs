using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using SEE.DataModel;
using SEE.DataModel.DG;
using SEE.DataModel.GraphSearch;
using SEE.UI.Notification;
using SEE.Utils;
using UnityEngine;

namespace SEE.UI.Window.TreeWindow
{
    /// <summary>
    /// A window that displays a 2D tree view of a graph.
    ///
    /// The window contains a scrollable list of expandable items.
    /// Each item represents a node in the graph.
    /// In addition to its children, the expanded form of an item also shows its connected edges.
    /// </summary>
    public partial class TreeWindow : BaseWindow, IObserver<ChangeEvent>
    {
        /// <summary>
        /// Path to the tree window content prefab.
        /// </summary>
        private const string treeWindowPrefab = "Prefabs/UI/TreeView";

        /// <summary>
        /// Path to the tree window item prefab.
        /// </summary>
        private const string treeItemPrefab = "Prefabs/UI/TreeViewItem";

        /// <summary>
        /// The graph to be displayed.
        /// Must be set before starting the window.
        /// </summary>
        public Graph Graph;

        /// <summary>
        /// The search helper used to search for elements in the graph.
        /// We also use this to keep track of the current filter, sort, and group settings.
        /// </summary>
        private GraphSearch Searcher;

        /// <summary>
        /// Transform of the object containing the items of the tree window.
        /// </summary>
        private RectTransform items;

        /// <summary>
        /// The context menu that is displayed when the user right-clicks on an item
        /// or uses the filter or sort buttons.
        /// </summary>
        private TreeWindowContextMenu ContextMenu;

        /// <summary>
        /// The subscription to the graph observable.
        /// </summary>
        private IDisposable subscription;

        protected override void Start()
        {
            Searcher = new GraphSearch(Graph);
            subscription = Graph.Subscribe(this);
            base.Start();
        }

        private void OnDestroy()
        {
            subscription.Dispose();
        }

        /// <summary>
        /// Adds the roots of the graph to the tree view.
        /// It may take up to a frame to add and reorder all items, hence this method is asynchronous.
        /// </summary>
        private async UniTask AddRoots()
        {
            // We will traverse the graph and add each node to the tree view.
            IList<Node> roots = WithHiddenChildren(Graph.GetRoots()).ToList();

            if (roots.Count == 0)
            {
                ShowNotification.Warn("Empty graph", "Graph has no roots. TreeView will be empty.");
                return;
            }

            foreach (Node root in roots)
            {
                AddNode(root);
            }
            await UniTask.Yield();
            foreach (Node root in roots)
            {
                OrderTree(root);
            }
        }

        /// <summary>
        /// Clears the tree view of all items.
        /// </summary>
        private void ClearTree()
        {
            foreach (Transform child in items)
            {
                if (child != null)
                {
                    Destroyer.Destroy(child.gameObject);
                }
            }
        }

        public override void RebuildLayout()
        {
            // Nothing needs to be done.
        }

        protected override void InitializeFromValueObject(WindowValues valueObject)
        {
            // TODO: Should tree windows be sent over the network?
            throw new NotImplementedException();
        }

        public override void UpdateFromNetworkValueObject(WindowValues valueObject)
        {
            throw new NotImplementedException();
        }

        public override WindowValues ToValueObject()
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            // Graph has been destroyed.
            Destroyer.Destroy(this);
        }

        public void OnError(Exception error)
        {
            throw error;
        }

        public void OnNext(ChangeEvent value)
        {
            // Rebuild tree when graph changes.
            switch (value)
            {
                case EdgeChange:
                case EdgeEvent:
                case GraphElementTypeEvent:
                case HierarchyEvent:
                case NodeEvent:
                    ClearTree();
                    AddRoots().Forget();
                    break;
            }
        }
    }
}
