using SEE.Game.UI.RuntimeConfigMenu;
using UnityEngine;

namespace SEE.Net.Actions.RuntimeConfig
{
    /// <summary>
    /// Network action when a file picker was changed. 
    /// </summary>
    public class UpdatePathCityFieldNetAction : UpdateCityNetAction
    {
        /// <summary>
        /// Whether the path is absolute or relative.
        /// </summary>
        public bool IsAbsolute;
        
        /// <summary>
        /// The changed value
        /// </summary>
        public string Value;

        /// <summary>
        /// Triggers 'SyncPath' on <see cref="RuntimeTabMenu"/>.
        /// </summary>
        protected override void ExecuteOnClient()
        {
            if (!IsRequester())
            {
                RuntimeConfigMenu.GetMenuForCity(CityIndex).SyncPath?.Invoke(WidgetPath, Value, IsAbsolute);
            }
        }
    }
}