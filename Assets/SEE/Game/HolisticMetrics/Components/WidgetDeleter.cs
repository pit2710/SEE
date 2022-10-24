using SEE.Controls.Actions.HolisticMetrics;
using SEE.Game.HolisticMetrics.Metrics;
using SEE.Game.HolisticMetrics.WidgetControllers;
using UnityEngine;

namespace SEE.Game.HolisticMetrics.Components
{
    /// <summary>
    /// This component can be attached to a widget. It will listen for left clicks on the widget and when it notices a
    /// left click, it will delete that widget.
    /// </summary>
    public class WidgetDeleter : MonoBehaviour
    {
        /// <summary>
        /// Whether or not the deletion is done. This is needed because we will add this component to all widgets until
        /// the player clicks on one, and we need to know when the player is done with the deleting of one widget so we
        /// can then delete all other instances of this component.
        /// </summary>
        private static bool deletionDone;

        /// <summary>
        /// Sets the deletionDone field to false so the WidgetDeleter instances won't delete themselves at the next
        /// Update() step.
        /// </summary>
        internal static void Setup()
        {
            deletionDone = false;
        }
        
        /// <summary>
        /// When the mouse is clicked on the widget and then it is released again, we will delete the widget on which
        /// the cursor was clicked and then delete all WidgetDeleter instances.
        /// </summary>
        private void OnMouseUp()
        {
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out _))
                {
                    var parentTransform = transform.parent;
                    deletionDone = true;
                    
                    // A config instance of the widget to delete, so it can be restored if needed
                    WidgetConfig config = ConfigManager.GetWidgetConfig(
                        GetComponent<WidgetController>(), 
                        GetComponent<Metric>());
                    
                    new DeleteWidgetAction(
                            parentTransform.GetComponent<WidgetsManager>().GetTitle(), 
                            config)
                        .Execute();
                }
            }
        }

        /// <summary>
        /// When the deletion is done (deletionDone field is true), all WidgetDeleter instances will delete themselves.
        /// </summary>
        private void Update()
        {
            if (deletionDone)
            {
                Destroy(this);
            }
        }
    }
}
