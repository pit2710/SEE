﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.SEECity.Charts.Scripts
{
	/// <summary>
	/// Handles the dragging and minimization of charts.
	/// </summary>
	public class ChartMoveHandler : MonoBehaviour, IDragHandler, IPointerDownHandler,
		IPointerUpHandler
	{
		/// <summary>
		/// Contains some settings used in this script.
		/// </summary>
		protected ChartManager ChartManager;

		/// <summary>
		/// Contains the position of the chart on the <see cref="Canvas" />.
		/// </summary>
		private RectTransform _chart;

		/// <summary>
		/// The current size of the screen the charts can be displayed on.
		/// </summary>
		private RectTransform _screenSize;

		/// <summary>
		/// The time between <see cref="OnPointerDown" /> and <see cref="OnPointerUp" /> to be recognized as
		/// click instead of a drag.
		/// </summary>
		private float _dragDelay;

		/// <summary>
		/// Tracks the time between <see cref="OnPointerDown" /> and <see cref="OnPointerUp" />.
		/// </summary>
		private float _timer;

		/// <summary>
		/// If the pointer is currently down or not.
		/// </summary>
		protected bool PointerDown;

		/// <summary>
		/// If the chart is currently minimized or not.
		/// </summary>
		protected bool Minimized;

		private Sprite _maximizedSprite;

		private Sprite _minimizedSprite;

		/// <summary>
		/// The TODO: Comment
		/// </summary>
		[SerializeField] protected GameObject SizeButton = null;

		/// <summary>
		/// Links the <see cref="Scripts.ChartManager" /> and initializes some values and settings with the
		/// values from
		/// the <see cref="ChartManager" />.
		/// </summary>
		protected virtual void Awake()
		{
			GetSettingData();
			_chart = transform.parent.GetComponent<RectTransform>();
			_screenSize = _chart.transform.parent.parent.GetComponent<RectTransform>();
		}

		/// <summary>
		/// Links the <see cref="Scripts.ChartManager" /> and gets its setting data.
		/// </summary>
		protected virtual void GetSettingData()
		{
			ChartManager = GameObject.FindGameObjectWithTag("ChartManager")
				.GetComponent<ChartManager>();
			_dragDelay = ChartManager.DragDelay;
			_maximizedSprite = ChartManager.MaximizedSprite;
			_minimizedSprite = ChartManager.MinimizedSprite;
		}

		/// <summary>
		/// Adds the time passed since the last frame to the <see cref="_timer" />
		/// </summary>
		protected virtual void Update()
		{
			if (PointerDown) _timer += Time.deltaTime;
		}

		/// <summary>
		/// Moves the chart to the position the player dragged it to.
		/// </summary>
		/// <param name="eventData">Contains the position data.</param>
		public virtual void OnDrag(PointerEventData eventData)
		{
			RectTransform pos = GetComponent<RectTransform>();
			if (eventData.position.x > 0 &&
			    eventData.position.x < _screenSize.sizeDelta.x * _screenSize.lossyScale.x &&
			    eventData.position.y > 0 &&
			    eventData.position.y < _screenSize.sizeDelta.y * _screenSize.lossyScale.y)
				_chart.position =
					new Vector2(eventData.position.x - pos.anchoredPosition.x * pos.lossyScale.x,
						eventData.position.y - pos.anchoredPosition.y * pos.lossyScale.y);
		}

		/// <summary>
		/// Starts the pointer down timer.
		/// </summary>
		/// <param name="eventData">Event payload associated with pointer (mouse / touch) events.</param>
		public void OnPointerDown(PointerEventData eventData)
		{
			_timer = 0f;
			PointerDown = true;
		}

		/// <summary>
		/// Stops the pointer down timer and triggers a click depending on the time the pointer was down for.
		/// </summary>
		/// <param name="eventData">Event payload associated with pointer (mouse / touch) events.</param>
		public void OnPointerUp(PointerEventData eventData)
		{
			PointerDown = false;
			if (_timer < _dragDelay) ToggleMinimize();
		}

		/// <summary>
		/// Toggles the minimization of the chart.
		/// </summary>
		protected virtual void ToggleMinimize()
		{
			ChartContent chart = _chart.GetComponent<ChartContent>();
			chart.LabelsPanel.gameObject.SetActive(Minimized);
			chart.DataPanel.gameObject.SetActive(Minimized);
			SizeButton.SetActive(Minimized);
			if (Minimized)
				GetComponent<Image>().sprite = _maximizedSprite;
			else
				GetComponent<Image>().sprite = _minimizedSprite;
			Minimized = !Minimized;
		}
	}
}