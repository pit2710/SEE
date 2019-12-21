﻿using System.Collections;
using System.Collections.Generic;
using SEE.Layout;
using UnityEngine;

namespace Assets.SEECity.Charts.Scripts
{
	/// <summary>
	/// Fills Charts with data and manages that data.
	/// </summary>
	public class ChartContent : MonoBehaviour
	{
		private GameObject[] _dataObjects;

		/// <summary>
		/// The <see cref="AxisContentDropdown" /> containing Values for the X-Axis.
		/// </summary>
		[SerializeField] private AxisContentDropdown _xAxisDropdown;

		/// <summary>
		/// The <see cref="AxisContentDropdown" /> containing Values for the Y-Axis.
		/// </summary>
		[SerializeField] private AxisContentDropdown _yAxisDropdown;

		/// <summary>
		/// A <see cref="ChartMarker" /> to display content in charts.
		/// </summary>
		[SerializeField] private GameObject _markerPrefab;

		/// <summary>
		/// A list of all <see cref="ChartMarker" />s currently displayed in the chart.
		/// </summary>
		private readonly List<GameObject> _activeMarkers = new List<GameObject>();

		/// <summary>
		/// Game Object to group all content entries of a chart.
		/// </summary>
		[SerializeField] private GameObject _entries;

		/// <summary>
		/// A parent of this object. Used in VR to destroy the whole construct of a moveable chart.
		/// </summary>
		[SerializeField] private GameObject _parent;

		/// <summary>
		/// The panel on which the <see cref="ChartMarker" />s are instantiated.
		/// </summary>
		[Header("For Resizing"), SerializeField]
		private RectTransform _dataPanel;

		[SerializeField] private RectTransform _labelsPanel;

		[SerializeField] private RectTransform _destroyButton;

		[SerializeField] private RectTransform _dragButton;

		[SerializeField] private RectTransform _chart;

		/// <summary>
		/// Calls methods to initialize a chart.
		/// </summary>
		private void Start()
		{
			StartCoroutine(FirstInitialization());
		}

		/// <summary>
		/// Calls <see cref="DrawData" /> during the first initialization after everything else has been
		/// initialized.
		/// </summary>
		/// <returns>A <see cref="WaitForEndOfFrame" />.</returns>
		private IEnumerator FirstInitialization()
		{
			yield return new WaitForEndOfFrame();
			DrawData();
		}

		/// <summary>
		/// Fills a List with all objects that will be in the chart.
		/// TODO: Show different types of objects in chart (edges, nodes)?
		/// </summary>
		private void FindDataObjects()
		{
			_dataObjects = GameObject.FindGameObjectsWithTag("Building");
		}

		/// <summary>
		/// Fills the chart with data depending on the values of <see cref="_xAxisDropdown" /> and
		/// <see cref="_yAxisDropdown" />.
		/// </summary>
		public void DrawData()
		{
			foreach (GameObject marker in _activeMarkers) Destroy(marker);
			_activeMarkers.Clear();
			FindDataObjects();
			_dataObjects[0].GetComponent<NodeRef>().node
				.TryGetNumeric(_xAxisDropdown.Value, out float minX);
			float maxX = minX;
			_dataObjects[0].GetComponent<NodeRef>().node
				.TryGetNumeric(_yAxisDropdown.Value, out float minY);
			float maxY = minY;
			foreach (GameObject data in _dataObjects)
			{
				data.GetComponent<NodeRef>().node
					.TryGetNumeric(_xAxisDropdown.Value, out float tempX);
				if (tempX < minX) minX = tempX;
				if (tempX > maxX) maxX = tempX;
				data.GetComponent<NodeRef>().node
					.TryGetNumeric(_yAxisDropdown.Value, out float tempY);
				if (tempY > maxY) maxY = tempY;
				if (tempY < minY) minY = tempY;
			}

			float width = _dataPanel.rect.width / (maxX - minX);
			float height = _dataPanel.rect.height / (maxY - minY);
			foreach (GameObject data in _dataObjects)
			{
				GameObject marker = Instantiate(_markerPrefab, _entries.transform);
				marker.GetComponent<ChartMarker>().LinkedObject = data;
				data.GetComponent<NodeRef>().node
					.TryGetNumeric(_xAxisDropdown.Value, out float valueX);
				data.GetComponent<NodeRef>().node
					.TryGetNumeric(_yAxisDropdown.Value, out float valueY);
				marker.GetComponent<RectTransform>().anchoredPosition = new Vector2(
					(valueX - minX) * width, (valueY - minY) * height);
				_activeMarkers.Add(marker);
			}
		}

		/// <summary>
		/// Changes the width and height of the chart.
		/// </summary>
		/// <param name="width">The new width of the chart.</param>
		/// <param name="height">The new height of the chart.</param>
		public void ChangeSize(float width, float height)
		{
			//DataPanel
			_dataPanel.sizeDelta = new Vector2(width - 100, height - 100);
			_dataPanel.anchoredPosition = new Vector2(width / 2, height / 2);
			//LabelsPanel
			_labelsPanel.sizeDelta = new Vector2(width, height);
			_labelsPanel.anchoredPosition = new Vector2(width / 2, height / 2);
			//xDropdown
			RectTransform xDropdown = _xAxisDropdown.GetComponent<RectTransform>();
			xDropdown.anchoredPosition = new Vector2(width / 2, xDropdown.anchoredPosition.y);
			xDropdown.sizeDelta = new Vector2(width / 3, xDropdown.sizeDelta.y);
			//yDropdown
			RectTransform yDropdown = _yAxisDropdown.GetComponent<RectTransform>();
			yDropdown.anchoredPosition = new Vector2(yDropdown.anchoredPosition.x, height / 2);
			yDropdown.sizeDelta = new Vector2(height / 3, yDropdown.sizeDelta.y);
			//Chart
			_chart.sizeDelta = new Vector2(width, height);
			//DestroyButton
			_destroyButton.anchoredPosition = new Vector2(-width / 2 + 25, height / 2 - 25);
			//DragButton
			_dragButton.anchoredPosition = new Vector2(width / 2 - 25, -height / 2 + 25);

			DrawData();
		}

		/// <summary>
		/// Destroys the chart including its container if VR is activated.
		/// </summary>
		public void Destroy()
		{
			Destroy(_parent);
		}
	}
}