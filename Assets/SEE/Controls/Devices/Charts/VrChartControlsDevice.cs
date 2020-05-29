﻿using UnityEngine;
using Valve.VR;

namespace SEE.Controls.Devices
{
	public class VrChartControlsDevice : ChartControls
	{
		private readonly SteamVR_Action_Vector2 _moveAction =
			SteamVR_Input.GetVector2Action(DefaultActionSet, MoveActionName);

		private readonly SteamVR_Action_Boolean _resetAction =
			SteamVR_Input.GetBooleanAction(DefaultActionSet, ResetChartsName);

		private readonly SteamVR_Action_Boolean _clickAction =
			SteamVR_Input.GetBooleanAction(DefaultActionSet, ClickActionName);

		private readonly SteamVR_Action_Boolean _createAction =
			SteamVR_Input.GetBooleanAction(DefaultActionSet, CreateChartActionName);

		public override bool Toggle => false;

		public override bool Select => false;

		public override Vector2 Move => _moveAction != null ? _moveAction.axis : Vector2.zero;

		public override bool ResetCharts => _resetAction.stateDown;

		public override bool Click => _clickAction.state;

		public override bool Create => _createAction.stateDown;
	}
}