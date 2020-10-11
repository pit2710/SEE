﻿// Copyright 2020 Robert Bohnsack
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SEE.DataModel;
using SEE.Game;
using SEE.GO;
using SEE.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace SEE.Charts.Scripts
{
	/// <summary>
	/// Fills Charts with data and manages that data.
	/// </summary>
	public class ChartContent : MonoBehaviour
	{
		/// <summary>
		/// The distance to another marker to recognize it as overlapping.
		/// </summary>
		private const float MarkerOverlapDistance = 22;

		/// <summary>
		/// The number of nodes at which a city is considered large.
		/// </summary>
		private const int BigCityThreshold = 50;
		/// <summary>
		/// The number of seconds to be waited until drawing the charts for large cities.
		/// </summary>
		private const float LongDrawWaitingTime = 5.0f;
		/// <summary>
		/// The number of seconds to be waited until drawing the charts for small cities.
		/// </summary>
		private const float ShortDrawWaitingTime = 0.2f;
        
        /// <summary>
        /// Contains one <see cref="scrollEntryPrefab" /> for each <see cref="Node" /> in the scene.
        /// </summary>
        [SerializeField] private GameObject scrollContent;

		/// <summary>
		/// A checkbox associated to a <see cref="Node" /> in the scene to activate it in the chart.
		/// A checkbox associated to a <see cref="Node" /> in the scene to activate it in the chart.
		/// </summary>
		[SerializeField] private GameObject scrollEntryPrefab;

		/// <summary>
		/// The starting coordinates of the headers in the <see cref="scrollContent" />.
		/// </summary>
		[SerializeField] private Vector2 headerOffset;

		/// <summary>
		/// The starting coordinates of the children in the <see cref="scrollContent" />.
		/// </summary>
		[SerializeField] private Vector2 childOffset;

		/// <summary>
		/// Determines if the entries in the <see cref="scrollContent" /> are displayed as tree.
		/// </summary>
		private bool _displayAsTree;

		/// <summary>
		/// The gap between entries in the <see cref="scrollContent" /> indicating a new hierarchy layer.
		/// </summary>
		private float _xGap;

		/// <summary>
		/// The gap between entries in the <see cref="scrollContent" /> to not make them overlap.
		/// </summary>
		private float _yGap;

		/// <summary>
		/// If a draw is queued, this will not be null.
		/// </summary>
		[HideInInspector] public Coroutine drawing;

		/// <summary>
		/// All game-node objects to be listed in the chart. 
		/// 
		/// Invariant: all game objects in _dataObjects are game objects tagged by Tags.Node
		/// and having a valid graph-node reference.
		/// </summary>
		private ICollection<GameObject> _dataObjects;

		/// <summary>
		/// A list of all <see cref="ChartMarker" />s currently displayed in the chart.
		/// </summary>
		protected List<GameObject> ActiveMarkers = new List<GameObject>();

		/// <summary>
		/// Handles the movement of charts.
		/// </summary>
		[SerializeField] private ChartMoveHandler moveHandler;

		/// <summary>
		/// The <see cref="AxisContentDropdown" /> containing Values for the X-Axis.
		/// </summary>
		public AxisContentDropdown axisDropdownX;

		/// <summary>
		/// The <see cref="AxisContentDropdown" /> containing Values for the Y-Axis.
		/// </summary>
		public AxisContentDropdown axisDropdownY;

		/// <summary>
		/// The prefab used to display content in charts.
		/// </summary>
		[SerializeField] private GameObject markerPrefab;

		/// <summary>
		/// Used to group all content entries of a chart as children of this <see cref="GameObject" />.
		/// </summary>
		[SerializeField] private GameObject entries;

		/// <summary>
		/// Will be shown if no data is to be displayed.
		/// </summary>
		[SerializeField] private GameObject noDataWarning;

		/// <summary>
		/// The minimum value on the x-axis.
		/// </summary>
		[SerializeField] private TextMeshProUGUI minXText;

		/// <summary>
		/// The maximum value on the x-axis.
		/// </summary>
		[SerializeField] private TextMeshProUGUI maxXText;

		/// <summary>
		/// The minimum value on the y-axis.
		/// </summary>
		[SerializeField] private TextMeshProUGUI minYText;

		/// <summary>
		/// The maximum value on the y-axis.
		/// </summary>
		[SerializeField] private TextMeshProUGUI maxYText;

		/// <summary>
		/// A parent of this object. Used in VR to destroy the whole construct of a moveable chart.
		/// </summary>
		public GameObject parent;

		/// <summary>
		/// The panel on which the <see cref="ChartMarker" />s are instantiated.
		/// </summary>
		[Header("For resizing and minimizing")]
		public RectTransform dataPanel;

		/// <summary>
		/// The panel on which the buttons and scales of the chart are displayed.
		/// </summary>
		public RectTransform labelsPanel;

		/// <summary>
		/// Contains all metric names contained in any <see cref="GameObject" /> of <see cref="_dataObjects" />.
		/// </summary>
		public HashSet<string> AllMetricNames { get; } = new HashSet<string>();

		/// <summary>
		/// The number of game objects representing a graph node in the current scene.
		/// A game object representing a graph node is one that is tagged by Tags.Node
		/// having a valid NodeRef to a graph node. Note that this number is across
		/// all current graphs represented in the scene, and not just one particular
		/// graph.
		/// </summary>
        public int TotalNumberOfGraphNodesInTheScene { get => _dataObjects.Count;  }

        /// <summary>
        /// Calls methods to initialize a chart.
        /// </summary>
        private void Awake()
		{
			_xGap = childOffset.x - headerOffset.x;
			_yGap = childOffset.y - headerOffset.y;
			FindDataObjects();
			GetAllNumericAttributes();
		}

		// This entry of the dropdown box represents not a metric but just the enumeration of nodes.
		// This entry can be selected if one wants to have a metric on one axis and then all nodes 
		// sorted by this metric on the other axis.
		private const string NodeEnumeration = "NODES";

		/// <summary>
		/// Fills the chart for the first time and invokes <see cref="CallDrawData" /> to keep the chart up to
		/// date.
		/// </summary>
		protected virtual void Start()
		{
			// The time in seconds to wait until CallDrawData is called.
			// FIXME: Why is this waiting time needed?
			var time = _dataObjects.Count > BigCityThreshold ? LongDrawWaitingTime : ShortDrawWaitingTime;
			axisDropdownX.AddNodeEnumerationEntry(NodeEnumeration);
			// FIXME: Why is this delayed call needed and what consequences does
			// it have? It seems as if this slows down the completion of drawing the objects.
			Invoke(nameof(CallDrawData), time);
		}

		/// <summary>
		/// Fills the scroll view on the right of the chart with one entry for each node in the scene including
		/// two headers to toggle all buildings and all nodes.
		/// </summary>
		private void FillScrollView()
		{
			Performance p = Performance.Begin("FillScrollView()");
			foreach (Transform child in scrollContent.transform) Destroy(child.gameObject);

			var tempObject = Instantiate(scrollEntryPrefab, scrollContent.transform);
			tempObject.TryGetComponent<ScrollViewToggle>(out var parentToggle);
			parentToggle.SetLabel("Leaves");
			tempObject.transform.localPosition = headerOffset;
			parentToggle.Initialize(this);

			var index = 0;
			foreach (var dataObject in _dataObjects)
			{
				if (SceneQueries.IsLeaf(dataObject))
                {
					CreateChildToggle(dataObject, parentToggle, index++, _yGap);
				}
			}

			tempObject = Instantiate(scrollEntryPrefab, scrollContent.transform);
			tempObject.TryGetComponent(out parentToggle);
			parentToggle.SetLabel("Inner Nodes");
			tempObject.transform.localPosition = headerOffset + new Vector2(0, _yGap) * ++index;
			parentToggle.Initialize(this);

			foreach (var dataObject in _dataObjects)
			{
				if (SceneQueries.IsInnerNode(dataObject))
				{
					CreateChildToggle(dataObject, parentToggle, index++, _yGap);
				}
			}

			scrollContent.TryGetComponent<RectTransform>(out var rect);
			rect.sizeDelta = new Vector2(rect.sizeDelta.x, index * Mathf.Abs(_yGap) + 40);
			p.End();
		}

		private void FillScrollView(bool tree)
        {
            Performance p = Performance.Begin("FillScrollView(bool)");
            foreach (Transform child in scrollContent.transform) Destroy(child.gameObject);

            if (!tree)
            {
                FillScrollView();
                p.End();
                return;
            }

            var index = 0;
            var hierarchy = 0;
            var maxHierarchy = 0;

            foreach (var root in SceneQueries.GetRoots(_dataObjects))
            {
                var inScene = _dataObjects.First(entry =>
                {
                    entry.TryGetComponent<NodeRef>(out var nodeRef);
                    return nodeRef.node.ID.Equals(root.ID);
                });
                var tempObject = Instantiate(scrollEntryPrefab, scrollContent.transform);
                tempObject.TryGetComponent<ScrollViewToggle>(out var rootToggle);
                inScene.TryGetComponent<NodeHighlights>(out var highlights);
                rootToggle.LinkedObject = highlights;
                highlights.scrollViewToggle = rootToggle;
                rootToggle.SetLabel(root.SourceName);
                tempObject.transform.localPosition =
                    headerOffset + new Vector2(0f, _yGap) * index;
                rootToggle.Initialize(this);
                if (hierarchy > maxHierarchy) maxHierarchy = hierarchy;
                hierarchy = 0;
                CreateChildToggles(root, rootToggle, ref index, ref hierarchy);
            }

            if (hierarchy > maxHierarchy) maxHierarchy = hierarchy; //TODO: Use this...
            scrollContent.TryGetComponent<RectTransform>(out var rect);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, index * Mathf.Abs(_yGap) + 40);
            p.End();
        }

        /// <summary>
        /// Creates a toggle for an object in the scene that is a node.
        /// </summary>
        /// <param name="dataObject">The object to be toggled.</param>
        /// <param name="parentToggle">The toggle that will toggle this one when clicked.</param>
        /// <param name="index">The position of the toggle in the scrollview.</param>
        /// <param name="gap">The gap between two toggles in the scrollview.</param>
        private void CreateChildToggle(GameObject dataObject, ScrollViewToggle parentToggle,
			int index,
			float gap)
		{
			var tempObject = Instantiate(scrollEntryPrefab, scrollContent.transform);
			tempObject.TryGetComponent<ScrollViewToggle>(out var toggle);
			toggle.Parent = parentToggle;
			dataObject.TryGetComponent<NodeHighlights>(out var highlights);
			toggle.LinkedObject = highlights;
			highlights.scrollViewToggle = toggle;
			toggle.SetLabel(dataObject.name);
			tempObject.transform.localPosition = childOffset + new Vector2(0f, gap) * index;
			toggle.Initialize(this);
			parentToggle.AddChild(toggle);
		}

		private void CreateChildToggles(Node root, ScrollViewToggle parentToggle, ref int index,
			ref int hierarchy)
		{
			if (root.IsLeaf()) return;

			hierarchy++;
			foreach (var child in root.Children())
			{
				var inScene = _dataObjects.First(entry =>
				{
					entry.TryGetComponent<NodeRef>(out var nodeRef);
					return nodeRef.node.ID.Equals(child.ID);
				});
				var tempObject = Instantiate(scrollEntryPrefab, scrollContent.transform);
				tempObject.TryGetComponent<ScrollViewToggle>(out var toggle);
				toggle.Parent = parentToggle;
				inScene.TryGetComponent<NodeHighlights>(out var highlights);
				toggle.LinkedObject = highlights;
				highlights.scrollViewToggle = toggle;
				toggle.SetLabel(child.SourceName);
				tempObject.transform.localPosition =
					childOffset + new Vector2(_xGap, 0f) * hierarchy +
					new Vector2(0f, _yGap) * index++;
				toggle.Initialize(this);
				parentToggle.AddChild(toggle);
				var tempHierarchy = hierarchy;
				CreateChildToggles(child, toggle, ref index, ref tempHierarchy);
			}
		}

		/// <summary>
		/// Gets all metric names for <see cref="float" /> values contained in the <see cref="NodeRef" /> of each
		/// <see cref="GameObject" /> in <see cref="_dataObjects" />. A metric name is the name of a
		/// numeric (either float or int) node attribute that starts with the prefix ChartManager.MetricPrefix.
		/// </summary>
		private void GetAllNumericAttributes()
		{
			Performance p = Performance.Begin("GetAllNumericAttributes");
			AllMetricNames.Clear();
			if (_dataObjects.Count == 0)
            {
				Debug.LogWarning("There are no nodes for showing metrics.\n");
            }
			else
			{
				foreach (GameObject data in _dataObjects)
				{
					if (data.TryGetComponent<NodeRef>(out NodeRef nodeRef))
                    {
						Node node = nodeRef.node;
						if (node != null)
						{
							foreach (var key in node.FloatAttributes.Keys)
							{
								if (key.StartsWith(ChartManager.MetricPrefix))
								{
									AllMetricNames.Add(key);
								}
							}
							foreach (var key in node.IntAttributes.Keys)
							{
								if (key.StartsWith(ChartManager.MetricPrefix))
								{
									AllMetricNames.Add(key);
								}
							}
						}
						else
                        {
							Debug.LogWarningFormat("Game node {0} has a null node reference.\n", data.name);
						}
					}
					else
                    {
						Debug.LogWarningFormat("Game node {0} without node reference.\n", data.name);
                    }
				}
				if (AllMetricNames.Count > 0)
				{
					foreach (string metric in AllMetricNames)
					{
						Debug.LogFormat("Available metric: {0}\n", metric);
					}
				}
				else
                {
					Debug.LogWarning("No metrics available for charts.\n");
                }
			}
			p.End();
		}

		/// <summary>
		/// Fills a List with all <see cref="Node" />s that will be in the chart.
		/// </summary>
		private void FindDataObjects()
        {
			Performance p = Performance.Begin("FindDataObjects: Find nodes");
			_dataObjects = SceneQueries.AllGameNodesInScene(ChartManager.Instance.ShowLeafMetrics, 
				                                            ChartManager.Instance.ShowInnerNodeMetrics);
			p.End();

			int numberOfDataObjectsWithNodeHightLights = 0;
			p = Performance.Begin("FindDataObjects: Node highlights");
            foreach (var entry in _dataObjects)
            {
                if (entry.TryGetComponent<NodeHighlights>(out NodeHighlights highlights))
                {
                    highlights.showInChart[this] = true;
                    numberOfDataObjectsWithNodeHightLights++;
                }
                //if (!highlights.showInChart.Contains(this)) highlights.showInChart.Add(this, true);
            }
            p.End();
            Debug.LogFormat("numberOfDataObjectsWithNodeHightLights: {0}\n", numberOfDataObjectsWithNodeHightLights);

            p = Performance.Begin("FindDataObjects: Fill scroll view");
            FillScrollView(_displayAsTree);
            p.End();
        }

        /// <summary>
        /// Since <see cref="MonoBehaviour.Invoke" /> does not support calls with parameters, it calls this
        /// method to do the work.
        /// </summary>
        private void CallDrawData()
		{
			Performance p = Performance.Begin("CallDrawData");
			DrawData(true);
			p.End();
		}

		/// <summary>
		/// Starts the Draw after a set time to handle calls in quick succession and improve performance.
		/// </summary>
		/// <returns></returns>
		public IEnumerator QueueDraw()
		{
			if (_dataObjects.Count > BigCityThreshold) yield return new WaitForSeconds(LongDrawWaitingTime);
			else yield return new WaitForSeconds(0.5f);

			DrawData(false);
			drawing = null;
		}

		/// <summary>
		/// Fills the chart with data depending on the values of <see cref="axisDropdownX" /> and
		/// <see cref="axisDropdownY" />.
		/// </summary>
		public void DrawData(bool needData)
		{
			Performance p = Performance.Begin("DrawData");
			if (needData)
			{
				FindDataObjects();
			}
			noDataWarning.SetActive(false);
			if (axisDropdownX.CurrentlySelectedMetric.Equals(axisDropdownY.CurrentlySelectedMetric))
			{
				DrawY(true);
			}
			else if (axisDropdownX.CurrentlySelectedMetric.Equals(NodeEnumeration))
            {
				DrawY(false);
			}
			else
			{
				DrawXY();
			}
			if (ActiveMarkers.Count == 0)
			{ 
				noDataWarning.SetActive(true); 
			}
			p.End();
		}

		/// <summary>
		/// Adds a marker for every <see cref="Node" /> for two metrics X and Y to be
		/// put on both axes. A marker's position depends on the values of those metrics.
		/// </summary>
		private void DrawXY()
		{
			Performance p = Performance.Begin("DrawXY");
			// Note that we determine the minimal and maximal metric values of the two
			// axes globally, that is, over all nodes in the scene and not just those
			// shown in this particular chart. This way, the scale of all charts for the
			// same metric is comparable.
			float minX = float.PositiveInfinity; // globally minimal value on X axis
			float maxX = float.NegativeInfinity; // globally maximal value on X axis
			float minY = float.PositiveInfinity; // globally minimal value on Y axis
			float maxY = float.NegativeInfinity; // globally maximal value on Y axis
			List<GameObject> toDraw = new List<GameObject>(); // nodes to be drawn in the chart
			foreach (var data in _dataObjects)
			{
				data.TryGetComponent(out NodeRef nodeRef);
				Node node = nodeRef.node;
				bool inX = false;				
				if (node.TryGetNumeric(axisDropdownX.CurrentlySelectedMetric, out float valueX))
				{
					if (valueX < minX) minX = valueX;
					if (valueX > maxX) maxX = valueX;
					inX = true;
				}
				bool inY = false;
				if (node.TryGetNumeric(axisDropdownY.CurrentlySelectedMetric, out float valueY))
				{
					if (valueY > maxY) maxY = valueY;
					if (valueY < minY) minY = valueY;
					inY = true;
				}
                // Is this node shown in this chart at all?
                if (inX && inY && (bool)nodeRef.highlights.showInChart[this])
				{
					// only nodes to be shown in this chart and having values for both
					// currently selected metrics for the axes will be added to the chart
					toDraw.Add(data);
				}
			}

			if (toDraw.Count > 0)
			{
				var xEqual = minX.Equals(maxX);
				var yEqual = minY.Equals(maxY);
				if (xEqual || yEqual)
				{
					(float min, float max) = minX.Equals(maxX) ? (minY, maxY) : (minX, maxX);
					AddMarkers(toDraw, min, max);
					minXText.text = xEqual ? "0" : minX.ToString("N0");
					maxXText.text = xEqual ? toDraw.Count.ToString() : maxX.ToString("N0");
					minYText.text = yEqual ? "0" : minY.ToString("N0");
					maxYText.text = yEqual ? toDraw.Count.ToString() : maxY.ToString("N0");
				}
				else
				{
					AddMarkers(toDraw, minX, maxX, minY, maxY);
					minXText.text = minX.ToString("N0");
					maxXText.text = maxX.ToString("N0");
					minYText.text = minY.ToString("N0");
					maxYText.text = maxY.ToString("N0");
				}
			}
			else
			{
				foreach (var activeMarker in ActiveMarkers) Destroy(activeMarker);
				noDataWarning.SetActive(true);
			}
			p.End();
		}

		/// <summary>
		/// Adds a marker for every <see cref="Node" /> for a single metric to be put onto the
		/// Y axis. If SortByMetric is true, the markers will be ordered in ascending order 
		/// by that metric and the distance between markers on the x-Axis will be equistant.
		/// If SortByMetric is false, they will be drawn in the order in which they appear
		/// in the list of _dataObjects.
		/// </summary>
		private void DrawY(bool SortByMetric)
		{
			Performance p = Performance.Begin("DrawY");
            List<GameObject> toDraw = new List<GameObject>();
            string metric = axisDropdownY.CurrentlySelectedMetric;

			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

			// Collect all data objects possessing the metric and whose value is to
			// be represented in this chart.
			foreach (var dataObject in _dataObjects)
			{
				if (dataObject.GetComponent<NodeRef>().node.TryGetNumeric(metric, out float value) &&
					(bool)dataObject.GetComponent<NodeHighlights>().showInChart[this])
				{
					if (value > max)
                    {
						max = value;
                    }
					if (value < min)
                    {
						min = value;
                    }
					toDraw.Add(dataObject);
				}
			}
			if (toDraw.Count > 0)
			{
				if (SortByMetric)
                {
                    toDraw.Sort(delegate (GameObject go1, GameObject go2)
                    {
                        go1.GetComponent<NodeRef>().node.TryGetNumeric(metric, out float value1);
                        go2.GetComponent<NodeRef>().node.TryGetNumeric(metric, out float value2);
                        return value1.CompareTo(value2);
                    });
                }
                AddMarkers(toDraw, min, max);

				minXText.text = "0";
				maxXText.text = toDraw.Count.ToString();
				minYText.text = min.ToString("N0");
				maxYText.text = max.ToString("N0");
			}
			else
			{
				foreach (var activeMarker in ActiveMarkers) Destroy(activeMarker);
				noDataWarning.SetActive(true);
			}
			p.End();
		}

		/// <summary>
		/// Adds new markers to the chart and removes the old ones.
		/// </summary>
		/// <param name="toDraw">The markers to add to the chart.</param>
		/// <param name="minX">The minimum value on the x-axis.</param>
		/// <param name="maxX">The maximum value on the x-axis.</param>
		/// <param name="minY">The minimum value on the y-axis.</param>
		/// <param name="maxY">The maximum value on the y-axis.</param>
		private void AddMarkers(IEnumerable<GameObject> toDraw, float minX, float maxX, float minY,
			float maxY)
		{
			Performance p = Performance.Begin("AddMarkers(IEnumerable, float, float, float)");
			var updatedMarkers = new List<GameObject>();
			var dataRect = dataPanel.rect;
			var width = dataRect.width / (maxX - minX);
			var height = dataRect.height / (maxY - minY);
			var positionInLayer = 0;

			foreach (var data in toDraw)
			{
				var marker = Instantiate(markerPrefab, entries.transform);
				marker.GetComponent<SortingGroup>().sortingOrder = positionInLayer++;
				marker.TryGetComponent<ChartMarker>(out ChartMarker chartMarker);
				chartMarker.linkedObject = data;
				chartMarker.ScrollViewToggle = data.GetComponent<NodeHighlights>().scrollViewToggle;
				var node = data.GetComponent<NodeRef>().node;
				node.TryGetNumeric(axisDropdownX.CurrentlySelectedMetric, out var valueX);
				node.TryGetNumeric(axisDropdownY.CurrentlySelectedMetric, out var valueY);
				var type = node.IsLeaf() ? "Building" : "Node";
				chartMarker.SetInfoText("Linked to: " + data.name + " of type " + type + "\nX: " +
				                   valueX.ToString("N") + ", Y: " + valueY.ToString("N"));
				marker.GetComponent<RectTransform>().anchoredPosition =
					new Vector2((valueX - minX) * width, (valueY - minY) * height);
				CheckOverlapping(marker, updatedMarkers.ToArray());
				updatedMarkers.Add(marker);

				var highlightTimeLeft = CheckOldMarkers(data);
				if (highlightTimeLeft > 0f)
					chartMarker.TriggerTimedHighlight(ChartManager.Instance.highlightDuration - highlightTimeLeft,
						true, false);
			}

			foreach (var marker in ActiveMarkers) Destroy(marker);
			ActiveMarkers = updatedMarkers;
			p.End();
		}

		/// <summary>
		/// Adds new markers to the chart if the same metric is displayed on both axes.
		/// </summary>
		/// <param name="toDraw">The markers to add to the chart.</param>
		/// <param name="min">The minimum value of the metric.</param>
		/// <param name="max">The maximum value of the metric.</param>
		private void AddMarkers(List<GameObject> toDraw, float min, float max)
		{
			Performance p = Performance.Begin("AddMarkers(List, float, float)");
			if (min.Equals(max))
			{
				AddMarkers(toDraw);
			}
			else
			{
				var updatedMarkers = new List<GameObject>();
				var dataRect = dataPanel.rect;
				var width = dataRect.width / (toDraw.Count - 1);
				var height = dataRect.height / (max - min);
				var metric = axisDropdownY.CurrentlySelectedMetric;
				var x = 0;
				var positionInLayer = 0;

				foreach (var data in toDraw)
				{
					var marker = Instantiate(markerPrefab, entries.transform);
					marker.GetComponent<SortingGroup>().sortingOrder = positionInLayer++;
					marker.TryGetComponent<ChartMarker>(out var script);
					script.linkedObject = data;
					script.ScrollViewToggle = data.GetComponent<NodeHighlights>().scrollViewToggle;
					var node = data.GetComponent<NodeRef>().node;
					node.TryGetNumeric(metric, out var value);
					var type = node.IsLeaf() ? "Building" : "Node";
					script.SetInfoText("Linked to: " + data.name + " of type " + type + "\n" +
					                   metric +
					                   ": " + value.ToString("N"));
					marker.GetComponent<RectTransform>().anchoredPosition =
						new Vector2(x++ * width, (value - min) * height);
					CheckOverlapping(marker, updatedMarkers.ToArray());
					updatedMarkers.Add(marker);

					if (ActiveMarkers.Count <= 0) continue;
					var highlightTimeLeft = CheckOldMarkers(data);
					if (highlightTimeLeft > 0f)
						script.TriggerTimedHighlight(
							ChartManager.Instance.highlightDuration - highlightTimeLeft, true, false);
				}

				foreach (var marker in ActiveMarkers) Destroy(marker);
				ActiveMarkers = updatedMarkers;
			}
			p.End();
		}

		/// <summary>
		/// Adds markers to the chart where all markers have the same value.
		/// </summary>
		/// <param name="toDraw">The markers to add to the chart.</param>
		private void AddMarkers(List<GameObject> toDraw)
		{
			Performance p = Performance.Begin("AddMarkers(List)");
			var updatedMarkers = new List<GameObject>();
			var dataRect = dataPanel.rect;
			var width = dataRect.width / toDraw.Count;
			var height = dataRect.height / toDraw.Count;
			var x = 0;
			var y = 0;
			var positionInLayer = 0;

			foreach (var data in toDraw)
			{
				var marker = Instantiate(markerPrefab, entries.transform);
				marker.TryGetComponent<SortingGroup>(out var group);
				group.sortingOrder = positionInLayer++;
				marker.TryGetComponent<ChartMarker>(out var script);
				script.linkedObject = data;
				data.TryGetComponent<NodeHighlights>(out var highlights);
				script.ScrollViewToggle = highlights.scrollViewToggle;
				data.TryGetComponent<NodeRef>(out var nodeRef);
				var node = nodeRef.node;
				node.TryGetNumeric(axisDropdownX.CurrentlySelectedMetric, out var valueX);
				node.TryGetNumeric(axisDropdownY.CurrentlySelectedMetric, out var valueY);
				var type = node.IsLeaf() ? "Building" : "Node";
				script.SetInfoText("Linked to: " + data.name + " of type " + type + "\nX: " +
				                   valueX.ToString("0.00") + ", Y: " + valueY.ToString("N"));
				marker.TryGetComponent<RectTransform>(out var anchoredPos);
				anchoredPos.anchoredPosition = new Vector2(x++ * width, y++ * height);
				CheckOverlapping(marker, updatedMarkers.ToArray());
				updatedMarkers.Add(marker);

				if (ActiveMarkers.Count <= 0) break;
				var highlightTimeLeft = CheckOldMarkers(data);
				if (highlightTimeLeft > 0f)
					script.TriggerTimedHighlight(ChartManager.Instance.highlightDuration - highlightTimeLeft,
						true, false);
			}

			foreach (var marker in ActiveMarkers) Destroy(marker);
			ActiveMarkers = updatedMarkers;
			p.End();
		}

		/// <summary>
		/// Checks if a marker is overlapping with any of the already existing new markers and changes its
		/// color for each overlapping marker.
		/// </summary>
		/// <param name="marker">The marker to check.</param>
		/// <param name="updatedMarkers">The already active new markers.</param>
		private static void CheckOverlapping(GameObject marker, GameObject[] updatedMarkers)
		{
			marker.TryGetComponent<Image>(out var image);
			if (updatedMarkers.Length > 10)
				for (var i = updatedMarkers.Length - 10; i < updatedMarkers.Length; i++)
				{
					var updatedMarker = updatedMarkers[i];
					if (Vector3.Distance(marker.transform.position,
							    updatedMarker.transform.position)
						    .CompareTo(MarkerOverlapDistance * marker.transform.lossyScale.x) >=
					    0) return;
					if (image.color.g - 0.1f < 0) return;
					var oldColor = image.color;
					image.color = new Color(oldColor.r, oldColor.g - 0.1f,
						oldColor.b - 0.1f);
				}
			else
				foreach (var updatedMarker in updatedMarkers)
					if (Vector3.Distance(marker.transform.position,
							updatedMarker.transform.position)
						.CompareTo(MarkerOverlapDistance * marker.transform.lossyScale.x) < 0)
						if (image.color.g - 0.1f >= 0)
						{
							var oldColor = image.color;
							image.color = new Color(oldColor.r, oldColor.g - 0.1f,
								oldColor.b - 0.1f);
						}
		}

		/// <summary>
		/// Checks if any of the old markers that will be removed were highlighted. If so, the highlight will
		/// be carried over to the new marker.
		/// </summary>
		/// <param name="marker">The new marker.</param>
		/// <returns></returns>
		private float CheckOldMarkers(GameObject marker)
		{
			loop:
			foreach (var oldMarker in ActiveMarkers)
				if (oldMarker.Equals(null))
				{
					Destroy(oldMarker);
					ActiveMarkers.Remove(oldMarker);
					goto loop;
				}
				else if (oldMarker.TryGetComponent(out ChartMarker oldScript) &&
				         oldScript.linkedObject.GetInstanceID() == marker.GetInstanceID() &&
				         oldScript.TimedHighlight != null)
				{
					ActiveMarkers.Remove(oldMarker);
					Destroy(oldMarker);
					return oldScript.HighlightTime;
				}

			return 0f;
		}

		/// <summary>
		/// Calls <see cref="ChartMarker.TriggerTimedHighlight" /> for all Markers in a rectangle in the chart.
		/// </summary>
		/// <param name="min">The starting edge of the rectangle.</param>
		/// <param name="max">The ending edge of the rectangle.</param>
		/// <param name="direction">True if min lies below max, false if not.</param>
		public virtual void AreaSelection(Vector2 min, Vector2 max, bool direction)
		{
			if (direction)
				foreach (var marker in ActiveMarkers)
				{
					Vector2 markerPos = marker.transform.position;
					if (markerPos.x > min.x && markerPos.x < max.x && markerPos.y > min.y &&
					    markerPos.y < max.y)
						ChartManager.HighlightObject(
							marker.GetComponent<ChartMarker>().linkedObject, false);
				}
			else
				foreach (var marker in ActiveMarkers)
				{
					Vector2 markerPos = marker.transform.position;
					if (markerPos.x > min.x && markerPos.x < max.x && markerPos.y < min.y &&
					    markerPos.y > max.y)
						ChartManager.HighlightObject(
							marker.GetComponent<ChartMarker>().linkedObject, false);
				}
		}

		/// <summary>
		/// Sets the info text of the chart.
		/// </summary>
		public void SetInfoText()
		{
			var metricX = axisDropdownX.CurrentlySelectedMetric;
			var metricY = axisDropdownY.CurrentlySelectedMetric;
			if (metricX.Equals(metricY))
				moveHandler.SetInfoText(metricX);
			else
				moveHandler.SetInfoText("X-Axis: " + axisDropdownX.CurrentlySelectedMetric + "\n" + "Y-Axis: " +
				                        axisDropdownY.CurrentlySelectedMetric);
		}

		/// <summary>
		/// Sets if the scroll view will display the original tree of the file structure or the more convenient
		/// grouping into buildings and nodes.
		/// </summary>
		/// <param name="displayAsTree"></param>
		public void SetDisplayAsTree(bool displayAsTree)
		{
			_displayAsTree = displayAsTree;
			FillScrollView(_displayAsTree);
		}

		/// <summary>
		/// Finds all markers that refer to a given <see cref="GameObject" /> and toggles their highlight
		/// across all charts.
		/// </summary>
		/// <param name="highlight">The object the marker will refer to.</param>
		/// <param name="scrollView">If this is triggered by a <see cref="ScrollViewToggle" /> or not.</param>
		public void HighlightCorrespondingMarker(GameObject highlight, bool scrollView)
		{
			foreach (var activeMarker in ActiveMarkers)
				if (activeMarker)
				{
					activeMarker.TryGetComponent<ChartMarker>(out var script);
					if (!script.linkedObject.Equals(highlight)) continue;
					script.TriggerTimedHighlight(ChartManager.Instance.highlightDuration, false, scrollView);
					break;
				}
		}

		/// <summary>
		/// Finds all markers that refer to a given <see cref="GameObject" /> and if they are highlighted,
		/// their accentuation will be toggled.
		/// </summary>
		/// <param name="highlight">The object the marker will refer to.</param>
		public void AccentuateCorrespondingMarker(GameObject highlight)
		{
			foreach (var activeMarker in ActiveMarkers)
			{
				activeMarker.TryGetComponent<ChartMarker>(out ChartMarker marker);
				if (!marker.linkedObject.Equals(highlight)) continue;
				marker.Accentuate();
				break;
			}
		}

		/// <summary>
		/// Destroys the chart including its container. Called when the user clicks on
		/// the closing button.
		/// </summary>
		public void Destroy()
		{
			Destroy(parent);
		}

		/// <summary>
		/// Removes this chart from all <see cref="NodeHighlights.showInChart" /> dictionaries.
		/// </summary>
		public void OnDestroy()
		{
			ChartManager.Instance.UnregisterChart(gameObject);
			foreach (var dataObject in _dataObjects)
				if (dataObject != null)
					dataObject.GetComponent<NodeHighlights>().showInChart.Remove(this);
		}
	}
}