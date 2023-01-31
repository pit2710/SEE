using SEE.Utils;
using UnityEngine;

namespace SEE.Game.HolisticMetrics.Components
{
    /// <summary>
    /// This component can be attached to a metrics board. It will then start listening for left mouse clicks on the
    /// board and once that occurs, it will create a widget where the click happened. Also whenever a left click
    /// happens, all instances of this class will delete themselves.
    /// </summary>
    public class WidgetAdder : MonoBehaviour
    {
        /// <summary>
        /// The configuration of the widget to add. We get this from the AddWidgetDialog.
        /// </summary>
        private Vector3 position;

        /// <summary>
        /// Whether or not this class has just added a widget. If this is true, all instances of this class should
        /// delete themselves in the next Update step.
        /// </summary>
        private static bool positioningDone;
        
        /// <summary>
        /// This method does the setup for all WidgetAdder instances, meaning it sets the positioningDone field to false
        /// and sets the widgetConfiguration field to the parameter value.
        /// </summary>
        internal static void Setup()
        {
            positioningDone = false;
        }

        internal static void Stop()
        {
            positioningDone = true;
        }
        
        /// <summary>
        /// When the mouse is lifted up after clicking on the metrics board, we get the position of the mouse and then
        /// add the widget there.
        /// </summary>
        private void OnMouseUp()
        {
            if (MainCamera.Camera != null)
            {
                if (!Raycasting.IsMouseOverGUI() && Raycasting.RaycastAnything(out RaycastHit hit))
                {
                    position = transform.InverseTransformPoint(hit.point);
                    positioningDone = true;
                }
            }
        }

        internal bool GetPosition(out Vector3 clickPosition)
        {
            clickPosition = position;
            return positioningDone;
        }

        /// <summary>
        /// This method checks whether or not the positioning is done. If so, it will delete this instance. Doing it
        /// this way allows us to add WidgetAdders to all metrics boards and they will take care of deleting themselves
        /// afterwards automatically.
        /// </summary>
        private void Update()
        {
            if (positioningDone)
            {
                Destroy(this);    
            }
        }
    }
}
