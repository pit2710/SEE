﻿using SEE.Game.UI.RuntimeConfigMenu;

namespace SEE.Net.Actions.RuntimeConfig
{
    public class UpdateBoolCityFieldNetAction : AbstractNetAction
    {
        public int CityIndex;
        public bool Value;
        public string WidgetPath;

        protected override void ExecuteOnServer()
        {
        }

        protected override void ExecuteOnClient()
        {
            if (!IsRequester())
                RuntimeConfigMenu.GetMenuForCity(CityIndex).OnSyncField?.Invoke(WidgetPath, Value);
        }
    }
}