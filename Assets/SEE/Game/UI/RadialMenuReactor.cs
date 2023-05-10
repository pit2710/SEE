using SEE.GO;
using SEE.Utils;
using UnityEngine.Assertions;
using UnityEngine;

namespace SEE.Game.UI
{

    public class RadialMenuReactor : MonoBehaviour
    {
        /// <summary>
        /// The canvas containing the radial menu.
        /// </summary>
        [Tooltip("The canvas containing the radial menu.")]
        public Canvas canvas;

        /// <summary>
        /// Sets the canvas's worldcamera to the transform of <see cref="MainCamera.Camera"/>
        /// if there is such a camera. Otherwise registers this component to be informed
        /// when the camera becomes available (via <see cref="OnCameraAvailable(Camera)"/>
        /// and disables itself until then.
        /// </summary>
        private void Start()
        {
            Camera camera = MainCamera.GetCameraNowOrLater(OnCameraAvailable);
            if (camera)
            {
                SetCanvasCamera(camera);
            }
            else
            {
                // Disable until we have a camera.
                enabled = false;
            }
        }

        private bool menuIsOn = false;

        private GameObject hitGameObject;

        private void Update()
        {
            if (UserTogglesMenu())
            {
                Debug.Log($"User toggles menu. menuIsOn={menuIsOn}\n");
                if (Raycasting.RaycastGraphElement(out RaycastHit hit, out GraphElementRef graphElementRef) == HitGraphElement.None)
                {
                    Debug.Log("Neither node or edge hit.\n");
                    // User does not hit a node or edge. Hence, the menu should be closed if it is currently open.
                    if (menuIsOn)
                    {
                        HideMenu();
                    }
                }
                else
                {
                    Debug.Log($"Node or edge was hit.  menuIsOn={menuIsOn}\n");
                    // User has hit a node or edge. Hence, the menu should be opened in case it wasn't.
                    if (!menuIsOn)
                    {
                        ShowMenu(hit, graphElementRef);
                    }
                }
            }
        }

        private void HideMenu()
        {
            canvas?.gameObject.SetActive(false);
            menuIsOn = false;
        }

        private void ShowMenu(RaycastHit hit, GraphElementRef graphElementRef)
        {
            canvas?.gameObject.SetActive(true);
            menuIsOn = true;
            Assert.IsNotNull(hit.collider);
            Assert.IsNotNull(graphElementRef);
            GameObject newlyHitGameObject = hit.collider.gameObject;
            Debug.Log($"ShowMenu({graphElementRef.elem.ToShortString()})\n");
            canvas.gameObject.transform.position = hit.point;
            hitGameObject = newlyHitGameObject;

        }

        private static bool UserTogglesMenu()
        {
            const int MidddleMouseButton = 2;
            // TODO: other interaction for VR needs to be added.
            return Input.GetMouseButtonDown(MidddleMouseButton);
        }

        /// <summary>
        /// A delegate to be called when a camera is available.
        /// It will set the worldcamera of the canvas via <see cref="SetCanvasCamera(Camera)"/>.
        /// </summary>
        /// <param name="camera">the availabe camera</param>
        private void OnCameraAvailable(Camera camera)
        {
            Assert.IsNotNull(camera);
            SetCanvasCamera(camera);
            enabled = true;
        }

        /// <summary>
        /// Sets the world camera of the <see cref="canvas"/> to given <paramref name="camera"/>.
        /// </summary>
        /// <param name="camera"></param>
        private void SetCanvasCamera(Camera camera)
        {
            if (canvas == null)
            {
                canvas = GetCanvas();
            }
            if (canvas == null)
            {
                Debug.LogError($"{gameObject.FullName()} has no associated {nameof(Canvas)}.\n");
            }
            else
            {
                canvas.worldCamera = camera;
            }
        }

        /// <summary>
        /// Returns the canvas the radial menu is contained in.
        ///
        /// Assumption: there is an immediate child game object containing the canvas.
        /// </summary>
        /// <returns>the canvas the radial menu is contained in or null if none is found</returns>
        private Canvas GetCanvas()
        {
            foreach (Transform child in gameObject.transform)
            {
                if (child.TryGetComponent(out Canvas canvas))
                {
                    return canvas;
                }
            }
            return null;
        }

        public void Option1()
        {
            Debug.Log("Option 1\n");
            HideMenu();
        }

        public void Option2()
        {
            Debug.Log("Option 2\n");
            HideMenu();
        }

        public void Option3()
        {
            Debug.Log("Option 3\n");
            HideMenu();
        }
    }
}
