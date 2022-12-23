using System;
using SEE.GO;
using SEE.Utils;
using UnityEngine;

namespace SEE.Game.UI.CodeWindow
{
    /// <summary>
    /// Represents a movable window.
    /// </summary>
    public abstract class BaseWindow<V> : PlatformDependentComponent where V: WindowValues
    {
        /// <summary>
        /// The title (e.g. filename) for the window.
        /// </summary>
        public string Title;

        /// <summary>
        /// Resolution of the window.
        /// </summary>
        protected Vector2 Resolution = new(900, 500);

        /// <summary>
        /// GameObject containing the actual UI for the window.
        /// </summary>
        public GameObject window { get; protected set; }

        /// <summary>
        /// Path to the window canvas prefab.
        /// </summary>
        private const string WINDOW_PREFAB = "Prefabs/UI/BaseWindow";

        protected override void StartDesktop()
        {
            if (Title == null)
            {
                Debug.LogError("Title must be defined when setting up Window!\n");
                return;
            }

            window = PrefabInstantiator.InstantiatePrefab(WINDOW_PREFAB, Canvas.transform, false);

            // Position code window in center of screen
            window.transform.localPosition = Vector3.zero;

            // Set resolution to preferred values
            if (window.TryGetComponentOrLog(out RectTransform rect))
            {
                rect.sizeDelta = Resolution;
            }
        }

        /// <summary>
        /// Shows or hides the window, depending on the <see cref="show"/> parameter.
        /// </summary>
        /// <param name="show">Whether the window should be shown.</param>
        /// <remarks>If this window is used in a <see cref="CodeSpace"/>, this method
        /// needn't (and shouldn't) be used.</remarks>
        public void Show(bool show)
        {
            switch (Platform)
            {
                case PlayerInputType.DesktopPlayer:
                    ShowDesktop(show);
                    break;
                case PlayerInputType.TouchGamepadPlayer:
                    ShowDesktop(show);
                    break;
                case PlayerInputType.VRPlayer:
                    PlatformUnsupported();
                    break;
                case PlayerInputType.None: // nothing needs to be done
                    break;
                default:
                    Debug.LogError($"Platform {Platform} not handled in switch case.\n");
                    break;
            }
        }

        /// <summary>
        /// When disabling the window, its controlled UI objects will also be disabled.
        /// </summary>
        public void OnDisable()
        {
            if (window)
            {
                window.SetActive(false);
            }
        }

        /// <summary>
        /// When enabling the window, its controlled UI objects will also be enabled.
        /// </summary>
        public void OnEnable()
        {
            if (window)
            {
                window.SetActive(true);
            }
        }

        /// <summary>
        /// Shows or hides the window on Desktop platforms.
        /// </summary>
        /// <param name="show">Whether the window should be shown.</param>
        private void ShowDesktop(bool show)
        {
            if (window)
            {
                window.SetActive(show);
            }
        }

        /// <summary>
        /// Sets up this newly created window from the values given in the <paramref name="valueObject"/>.
        /// 
        /// Note that the <see cref="Title"/> and <c>AttachedTo</c> attributes needn't be handled, only newly added
        /// fields compared to <see cref="WindowValues"/> are relevant here.
        /// 
        /// </summary>
        /// <param name="valueObject">The window value object whose values shall be used.</param>
        /// <remarks>
        /// <see cref="Start"/> has not been called at this point.
        /// </remarks>
        protected abstract void InitializeFromValueObject(V valueObject);
        
        /// <summary>
        /// Recreates a window from the given <paramref name="valueObject"/> and attaches it to
        /// the GameObject <paramref name="attachTo"/>.
        /// </summary>
        /// <param name="valueObject">The value object from which the window should be constructed</param>
        /// <param name="attachTo">The game object the window should be attached to. If <c>null</c>,
        /// the game object will be attached to the game object with the name specified in the value object.</param>
        /// <returns>The newly re-constructed window</returns>
        /// <exception cref="InvalidOperationException">If both <paramref name="attachTo"/> is <c>null</c>
        /// and the game object specified in <paramref name="valueObject"/> can't be found.</exception>
        public static T FromValueObject<T>(V valueObject, GameObject attachTo = null) where T: BaseWindow<V>
        {
            if (attachTo == null)
            {
                attachTo = GraphElementIDMap.Find(valueObject.AttachedTo);
                if (attachTo == null)
                {
                    throw new InvalidOperationException($"GameObject with name {attachTo} could not be found.\n");
                }
            }

            T window = attachTo.AddComponent<T>();
            window.Title = valueObject.Title;
            window.InitializeFromValueObject(valueObject);
            return window;
        }

        /// <summary>
        /// Generates and returns a value object for this window.
        /// </summary>
        /// <returns>The newly created window value object, matching this class</returns>
        public abstract V ToValueObject();
    }
        
    /// <summary>
    /// Represents the values of a window needed to re-create its content.
    /// Used for serialization when sending a window over the network.
    /// </summary>
    [Serializable]
    public abstract class WindowValues
    {
        /// <summary>
        /// Title of the window.
        /// </summary>
        [field: SerializeField]
        public string Title { get; private set; }
        
        [field: SerializeField]
        /// <summary>
        /// Name of the game object this window was attached to.
        /// </summary>
        public string AttachedTo { get; private set; }

        /// <summary>
        /// Creates a new WindowValues object from the given parameters.
        /// </summary>
        /// <param name="title">The title of the window.</param>
        /// <param name="attachedTo">Name of the game object the code window is attached to.</param>
        internal WindowValues(string title, string attachedTo = null)
        {
            AttachedTo = attachedTo;
            Title = title;
        }
    }
}