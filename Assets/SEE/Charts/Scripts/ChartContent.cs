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
using SEE.GO;
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
		/// Contains one <see cref="scrollEntryPrefab" /> for each <see cref="Node" /> in the scene.
		/// </summary>
		[SerializeField] private GameObject scrollContent;

		/// <summary>
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
		/// If a draw is queued, this wont be null.
		/// </summary>
		[HideInInspector] public Coroutine drawing;

		/// <summary>
		/// All objects to be listed in the chart.
		/// </summary>
		private GameObject[] _dataObjects;

		/// <summary>
		/// The number of nodes in a scene to determine the performance of graphs.
		/// </summary>
		[HideInInspector] public int citySize;

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
		/// Contains all keys contained in any <see cref="GameObject" /> of <see cref="_dataObjects" />.
		/// </summary>
		public List<string> AllKeys { get; } = new List<string>();

		/// <summary>
		/// Calls methods to initialize a chart.
		/// </summary>
		private void Awake()
		{
			_xGap = childOffset.x - headerOffset.x;
			_yGap = childOffset.y - headerOffset.y;
			FindDataObjects();
			GetAllFloats();
			GetAllIntegers();
		}

		/// <summary>
		/// Fills the chart for the first time and invokes <see cref="CallDrawData" /> to keep the chart up to
		/// date.
		/// </summary>
		protected virtual void Start()
		{
			var time = citySize > 50 ? 5f : 0.2f;
			axisDropdownX.SetOther(axisDropdownY);
			axisDropdownY.SetOther(axisDropdownX);
			Invoke(nameof(CallDrawData), time);
		}

		/// <summary>
		/// Fills the scroll view on the right of the chart with one entry for each node in the scene including
		/// two headers to toggle all buildings and all nodes.
		/// </summary>
		private void FillScrollView()
		{
			foreach (Transform child in scrollContent.transform) Destroy(child.gameObject);

			var tempObject = Instantiate(scrollEntryPrefab, scrollContent.transform);
			tempObject.TryGetComponent<ScrollViewToggle>(out var parentToggle);
			parentToggle.SetLabel("Buildings");
			tempObject.transform.localPosition = headerOffset;
			parentToggle.Initialize(this);

			var index = 0;
			foreach (var dataObject in _dataObjects)
				if (dataObject.GetComponent<NodeRef>().node.IsLeaf())
					CreateChildToggle(dataObject, parentToggle, index++, _yGap);

			tempObject = Instantiate(scrollEntryPrefab, scrollContent.transform);
			tempObject.TryGetComponent(out parentToggle);
			parentToggle.SetLabel("Nodes");
			tempObject.transform.localPosition = headerOffset + new Vector2(0, _yGap) * ++index;
			parentToggle.Initialize(this);

			foreach (var dataObject in _dataObjects)
				if (!dataObject.GetComponent<NodeRef>().node.IsLeaf())
					CreateChildToggle(dataObject, parentToggle, index++, _yGap);

			scrollContent.TryGetComponent<RectTransform>(out var rect);
			rect.sizeDelta = new Vector2(rect.sizeDelta.x, index * Mathf.Abs(_yGap) + 40);
		}

		private void FillScrollView(bool tree)
		{
			foreach (Transform child in scrollContent.transform) Destroy(child.gameObject);

			if (!tree)
			{
				FillScrollView();
				return;
			}

			_dataObjects[0].TryGetComponent<NodeRef>(out var node);
			var graph = node.node.ItsGraph;
			var roots = graph.GetRoots();
			var index = 0;
			var hierarchy = 0;
			var maxHierarchy = 0;

			foreach (var root in roots)
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
		/// Gets all keys for <see cref="float" /> values contained in the <see cref="NodeRef" /> of each
		/// <see cref="GameObject" /> in <see cref="_dataObjects" />.
		/// </summary>
		private void GetAllFloats()
		{
			foreach (var data in _dataObjects)
			foreach (var key in data.GetComponent<NodeRef>().node.FloatAttributes.Keys)
				if (!AllKeys.Contains(key))
					AllKeys.Add(key);
		}

		/// <summary>
		/// Gets all keys for <see cref="int" /> values contained in the <see cref="NodeRef" /> of each
		/// <see cref="GameObject" /> in <see cref="_dataObjects" />.
		/// </summary>
		private void GetAllIntegers()
		{
			foreach (var data in _dataObjects)
			foreach (var key in data.GetComponent<NodeRef>().node.IntAttributes.Keys)
				if (!AllKeys.Contains(key))
					AllKeys.Add(key);
		}

		/// <summary>
		/// Fills a List with all <see cref="Node" />s that will be in the chart.
		/// </summary>
		private void FindDataObjects()
		{
			_dataObjects = GameObject.FindGameObjectsWithTag("Node");
			foreach (var entry in _dataObjects)
			{
				entry.TryGetComponent<NodeHighlights>(out var highlights);
				if (!highlights.showInChart.Contains(this)) highlights.showInChart.Add(this, true);
			}

			citySize = _dataObjects.Length;
			FillScrollView(_displayAsTree);
		}

		/// <summary>
		/// Since <see cref="MonoBehaviour.Invoke" /> does not support calls with parameter, it calls this
		/// method to do the work.
		/// </summary>
		private void CallDrawData()
		{
			DrawData(true);
		}

		/// <summary>
		/// Starts the Draw after a set time to handle calls in quick succession and improve performance.
		/// </summary>
		/// <returns></returns>
		public IEnumerator QueueDraw()
		{
			if (citySize > 50) yield return new WaitForSeconds(5f);
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
			if (needData) FindDataObjects();
			noDataWarning.SetActive(false);
			if (axisDropdownX.Value.Equals(axisDropdownY.Value))
				DrawOneAxis();
			else
				DrawTwoAxes();
			if (ActiveMarkers.Count == 0) noDataWarning.SetActive(true);
		}

		/// <summary>
		/// Adds a marker for every <see cref="Node" /> containing the metrics from both axes. It's position
		/// depends on the values of those metrics.
		/// </summary>
		private void DrawTwoAxes()
		{
			var i = 0;
			_dataObjects[i].TryGetComponent<NodeRef>(out var nodeRef);
			var node = nodeRef.node;
			var showInChart = (bool) nodeRef.highlights.showInChart[this];
			var contained = node.TryGetNumeric(axisDropdownX.Value, out var minX) && showInChart;
			while (!contained)
			{
				i++;
				_dataObjects[i].TryGetComponent(out nodeRef);
				node = nodeRef.node;
				showInChart = (bool) nodeRef.highlights.showInChart[this];
				contained = showInChart && node.TryGetNumeric(axisDropdownX.Value, out minX);
			}

			var maxX = minX;
			i = 0;
			_dataObjects[i].TryGetComponent(out nodeRef);
			node = nodeRef.node;
			showInChart = (bool) nodeRef.highlights.showInChart[this];
			contained = node.TryGetNumeric(axisDropdownY.Value, out var minY) && showInChart;
			while (!contained)
			{
				i++;
				_dataObjects[i].TryGetComponent(out nodeRef);
				node = nodeRef.node;
				showInChart = (bool) nodeRef.highlights.showInChart[this];
				contained = showInChart && node.TryGetNumeric(axisDropdownY.Value, out minY);
			}

			var maxY = minY;
			var toDraw = new List<GameObject>();
			foreach (var data in _dataObjects)
			{
				data.TryGetComponent(out nodeRef);
				node = nodeRef.node;
				showInChart = (bool) nodeRef.highlights.showInChart[this];
				var inX = false;
				var inY = false;
				if (node.TryGetNumeric(axisDropdownX.Value, out var tempX))
				{
					if (tempX < minX) minX = tempX;
					if (tempX > maxX) maxX = tempX;
					inX = true;
				}

				if (node.TryGetNumeric(axisDropdownY.Value, out var tempY))
				{
					if (tempY > maxY) maxY = tempY;
					if (tempY < minY) minY = tempY;
					inY = true;
				}

				if (inX && inY && showInChart)
					toDraw.Add(data);
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
		}

		/// <summary>
		/// Adds a marker for every <see cref="Node" /> containing the metric that is the same for both axes.
		/// Therefore markers will be ordered by that one value and the distance between markers on the x-Axis
		/// will be consistent.
		/// </summary>
		private void DrawOneAxis()
		{
			var toDraw = new List<GameObject>();
			var metric = axisDropdownY.Value;

			foreach (var dataObject in _dataObjects)
				if (dataObject.GetComponent<NodeRef>().node.TryGetNumeric(metric, out _) &&
				    (bool) dataObject.GetComponent<NodeHighlights>().showInChart[this])
					toDraw.Add(dataObject);
			if (toDraw.Count > 0)
			{
				toDraw.Sort(delegate(GameObject go1, GameObject go2)
				{
					go1.GetComponent<NodeRef>().node.TryGetNumeric(metric, out var value1);
					go2.GetComponent<NodeRef>().node.TryGetNumeric(metric, out var value2);
					return value1.CompareTo(value2);
				});

				toDraw.First().GetComponent<NodeRef>().node.TryGetNumeric(metric, out var min);
				toDraw.Last().GetComponent<NodeRef>().node.TryGetNumeric(metric, out var max);

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
			var updatedMarkers = new List<GameObject>();
			var dataRect = dataPanel.rect;
			var width = dataRect.width / (maxX - minX);
			var height = dataRect.height / (maxY - minY);
			var positionInLayer = 0;

			foreach (var data in toDraw)
			{
				var marker = Instantiate(markerPrefab, entries.transform);
				marker.GetComponent<SortingGroup>().sortingOrder = positionInLayer++;
				marker.TryGetComponent<ChartMarker>(out var script);
				script.linkedObject = data;
				script.ScrollViewToggle = data.GetComponent<NodeHighlights>().scrollViewToggle;
				var node = data.GetComponent<NodeRef>().node;
				node.TryGetNumeric(axisDropdownX.Value, out var valueX);
				node.TryGetNumeric(axisDropdownY.Value, out var valueY);
				var type = node.IsLeaf() ? "Building" : "Node";
				script.SetInfoText("Linked to: " + data.name + " of type " + type + "\nX: " +
				                   valueX.ToString("N") + ", Y: " + valueY.ToString("N"));
				marker.GetComponent<RectTransform>().anchoredPosition =
					new Vector2((valueX - minX) * width, (valueY - minY) * height);
				CheckOverlapping(marker, updatedMarkers.ToArray());
				updatedMarkers.Add(marker);

				var highlightTimeLeft = CheckOldMarkers(data);
				if (highlightTimeLeft > 0f)
					script.TriggerTimedHighlight(ChartManager.Instance.highlightDuration - highlightTimeLeft,
						true, false);
			}

			foreach (var marker in ActiveMarkers) Destroy(marker);
			ActiveMarkers = updatedMarkers;
		}

		/// <summary>
		/// Adds new markers to the chart if the same metric is displayed on both axes.
		/// </summary>
		/// <param name="toDraw">The markers to add to the chart.</param>
		/// <param name="min">The minimum value of the metric.</param>
		/// <param name="max">The maximum value of the metric.</param>
		private void AddMarkers(List<GameObject> toDraw, float min, float max)
		{
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
				var metric = axisDropdownY.Value;
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
		}

		/// <summary>
		/// Adds markers to the chart where all markers have the same value.
		/// </summary>
		/// <param name="toDraw">The markers to add to the chart.</param>
		private void AddMarkers(List<GameObject> toDraw)
		{
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
				node.TryGetNumeric(axisDropdownX.Value, out var valueX);
				node.TryGetNumeric(axisDropdownY.Value, out var valueY);
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
			var metricX = axisDropdownX.Value;
			var metricY = axisDropdownY.Value;
			if (metricX.Equals(metricY))
				moveHandler.SetInfoText(metricX);
			else
				moveHandler.SetInfoText("X-Axis: " + axisDropdownX.Value + "\n" + "Y-Axis: " +
				                        axisDropdownY.Value);
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
				activeMarker.TryGetComponent<ChartMarker>(out var script);
				if (!script.linkedObject.Equals(highlight)) continue;
				script.Accentuate();
				break;
			}
		}

		/// <summary>
		/// Destroys the chart including its container if VR is activated.
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
			foreach (var dataObject in _dataObjects)
				if (dataObject != null)
					dataObject.GetComponent<NodeHighlights>().showInChart.Remove(this);
		}
	}
}