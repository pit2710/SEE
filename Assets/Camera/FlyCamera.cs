﻿using System;
using SEE.DataModel;
using UnityEngine;

namespace SEE
{
    /*
     * Allows to move the camera with WASD, Shift and Space.
     * 
    Written by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.
    Converted to C# 27-02-13 - no credit wanted.
    Reformatted and cleaned by Ryan Breaker 23-6-18
    Original comment:
    Simple flycam I made, since I couldn't find any others made public.
    Made simple to use (drag and drop, done) for regular keyboard layout.
    Controls:
    WASD  : Directional movement
    Shift : Increase speed
    Space : Moves camera directly up per its local Y-axis

    See also: 
     https://answers.unity.com/questions/666905/in-game-camera-movement-like-editor.html
     https://gist.github.com/Mahelita/f82d5b574ab6333f0c834178582280c3
    */

    /// <summary>
    /// Moves the main camera based on keyboard input.
    /// </summary>
    public class FlyCamera : MonoBehaviour
    {
        // These variables are exposed to the editor and can be changed by the user.
        // These parameters determine the principal speed of movement but their
        // actual value is also function of the distance to the ground. The lower we 
        // are to the ground, the slower we move, and vice versa.

        public const float relativeBaseSpeedDefault = 10.0f;

        [Tooltip("Relative base speed without acceleration and independent of the distance to the ground."
                 + " The actual speed is a function of this parameter and the distance to the ground."
                 + " The farther the ground, the higher the speed. This parameter must"
                 + " be greater than 0.")]
        /// <summary>
        /// Normal speed without acceleration. This defines the amount of distance to be covered
        /// per tick.
        /// </summary>
        public float relativeBaseSpeed = relativeBaseSpeedDefault;

        public const float relativeMaximalSpeedDefault = 100.0f;

        [Tooltip("Maximal speed in the presence of acceleration, but independent of the distance to the ground."
         + " The actual maximal speed is a function of this parameter and the distance to the ground,"
         + " but it cannot go beyond the absolute maximum speed."
         + " This parameter must be greater than 0.")]
        public float relativeMaximalSpeed = relativeMaximalSpeedDefault; // Maximum speed when holding shift

        [Tooltip("The absolute minimum speed that can be reached, no matter how"
         + " close we are to the ground. Must not be larger than the absolute maximum speed."
         + " This parameter must be greater than 0.")]
        public float absoluteMinimumSpeed = 0.1f;

        public const float absoluteMaximumSpeedDefault = 300.0f;

        [Tooltip("The absolute maximum speed that can be reached, no matter how much we accelerate and how"
            + " far we are off the ground. Must not be lower than the absolute minimum speed."
            + " This parameter must be greater than 0.")]
        public float absoluteMaximumSpeed = absoluteMaximumSpeedDefault;

        /// <summary>
        /// Sets relativeBaseSpeed, relativeMaximalSpeed, and absoluteMaximumSpeed to their default
        /// multiplied by the given unit factor, unless their value is already greater than that.
        /// </summary>
        /// <param name="unit"></param>
        public void AdjustSettings(float unit)
        {
            relativeBaseSpeed     = Mathf.Max(relativeBaseSpeedDefault * unit,   relativeBaseSpeed);
            relativeMaximalSpeed = Mathf.Max(relativeMaximalSpeedDefault * unit, relativeMaximalSpeed);
            absoluteMaximumSpeed = Mathf.Max(absoluteMaximumSpeedDefault * unit, absoluteMaximumSpeed);
        }

        /// <summary>
        /// Resets relativeBaseSpeed, relativeMaximalSpeed, and absoluteMaximumSpeed to their defaults.
        /// </summary>
        public void SetDefaults()
        {
            relativeBaseSpeed = relativeBaseSpeedDefault;
            relativeMaximalSpeed = relativeMaximalSpeedDefault;
            absoluteMaximumSpeed = absoluteMaximumSpeedDefault;
        }

        // Amount to accelerate when shift is pressed. The actual acceleration is
        // a function of this parameter and the accummulated time since the user
        // started to hold the shift key. That is, we are getting faster, the longer
        // the shift key has been pressed up to the maximal speed. The duration of
        // the acceleration period is captured in variable accelerationPeriod.
        [Tooltip("The amount of acceleration while holding the shift key. The actual acceleration is" +
             " a function of this parameter and the accummulated time since you" +
            " started to hold the shift key. That is, you are getting faster, the longer" +
            " the shift key has been pressed up to the maximal speed."
            + " This parameter must be greater than 0.")]
        public float acceleration = 10.0f;

        // Degree of spinning for each tick without acceleration. This value is
        // independent of the distance to the ground because it is used to turn
        // around at the same location. The actual rotation, however, may depend 
        // on the acceleration.
        [Tooltip("Degree of spinning for each tick. This value is" +
            " independent of the distance to the ground because it is used to turn" +
            " around at the same location. The actual rotation, however, may depend" +
            " on the acceleration."
            + " This parameter must be greater than 0.")]
        public float rotationFactor = 50f;

        // Factor for speed based on the distance to the ground. The y co-ordinate 
        // determines the distance to the ground.
        [Tooltip("Factor for speed based on the distance to the ground." +
            " The actual speed is a function of the relative base speed and the distance to the" +
            " ground multiplied by this parameters."
            + " This parameter must be greater than 0.")]
        public float groundDistanceFactor = 0.1f;

        // The main camera that is controlled by this controller.
        private Camera mainCamera;

        /// <summary>
        /// Yields the amount of movement as a product of movementDelta, the distance
        /// to the ground, and groundDistanceFactor. This value is always positive.
        /// </summary>
        /// <param name="movementDelta">the speed; must be greater than 0</param>
        /// <returns>amount of movement (always greater than 0)</returns>
        private float SpeedFunction(float movementDelta)
        {
            // Abs because we might be flying below the ground level.
            return movementDelta * Mathf.Abs(mainCamera.transform.position.y) * groundDistanceFactor;
        }

        /// <summary>
        /// Yields the amount of movement as a product of relativeBaseSpeed, the distance
        /// to the ground, and groundDistanceFactor. This value is always in the range
        /// of [absoluteMinimumSpeed, absoluteMaximumSpeed].
        /// </summary>
        /// <returns>amount of movement</returns>
        private float GetNormalSpeed()
        {
            return Mathf.Clamp(SpeedFunction(relativeBaseSpeed), absoluteMinimumSpeed, absoluteMaximumSpeed);
        }

        /// <summary>
        /// Yields the amount of maximal movement as a product of relativeMaximalSpeed, the distance
        /// to the ground, and groundDistanceFactor. This value is always in the range
        /// of [absoluteMinimumSpeed, absoluteMaximumSpeed].
        /// </summary>
        /// <returns>amount of movement</returns>
        private float GetMaximalSpeed()
        {
            return Mathf.Clamp(SpeedFunction(relativeMaximalSpeed), absoluteMinimumSpeed, absoluteMaximumSpeed);
        }
        
        [Tooltip("The sensitivity to the mouse movement.")]
        public float camSens = 0.15f;   // Mouse sensitivity
        
        // the position of the mouse cursor of the last tick
        private Vector3 lastMouse = new Vector3(255, 255, 255);  // kind of in the middle of the screen, rather than at the top (play)

        // the accumulated time of the acceleration across ticks
        private float accelerationPeriod = 1.0f;

        private Vector3 previousPosition = new Vector3(0f, 0f, 0f);
        private Quaternion previousRotation = new Quaternion(0f, 0f, 0f, 0f);

        // The scene graph this camera observes.
        // private SceneGraph sceneGraph = null; // TODO: Re-enable

        // The GUI text field showing the object name of a the currently selected node.
        private GameObject guiObjectNameTextField = null;

        // Spin the object in given direction around the origin of the object at rotationFactor per tick.
        private void Rotate(Vector3 direction, bool accelerationMode)
        {
            float degree = accelerationMode ? accelerationPeriod * rotationFactor : rotationFactor;
            transform.RotateAround(transform.position, direction, degree * Time.deltaTime);
        }

        /// <summary>
        /// Intializes sceneGraph and guiObjectNameTextField.
        /// 
        /// Called by unity on the frame when a script is enabled just before any of the Update 
        /// methods are called the first time. It is called only once.
        /// 
        /// This is a Unity message.  It is not defined in the base MonoBehaviour class, 
        /// so you we don't have to specify override in your derived class. Instead Unity 
        /// uses reflection to see if we've put a method named "Start" in our class and 
        /// if we have then it will call that method as needed.
        /// 
        /// See also https://docs.unity3d.com/Manual/ExecutionOrder.html on the execution order
        /// of messages.
        /// </summary>
        void Start()
        {
            if (guiObjectNameTextField == null)
            {
                guiObjectNameTextField = GameObject.Find("Objectname");
            }

            mainCamera = gameObject.GetComponentInParent<Camera>();

            if (mainCamera == null)
            {
                Debug.LogError("Parent has no camera.\n");
                mainCamera = Camera.current;
            }
        }

        /// <summary>
        /// Unity message called for every frame.
        /// 
        /// Reacts to user interactions by moving the camera or showing information
        /// on selected entities.
        /// Note: Update is called once per frame by Unity.
        /// </summary>
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // If a node is hit by a left mouse click, the name of the selected
                // node is shown in the guiObjectNameTextField.
                if (guiObjectNameTextField != null)
                {
                    if (mainCamera != null)
                    {
                        ShowSelectedObject(mainCamera);
                    }
                    else
                    {
                        Debug.LogError("No current camera found.\n");
                    }
                }
            }

            // whether the user wants us to accelerate by holding the shift key
            bool accelerationMode = Input.GetKey(KeyCode.LeftShift);

            // Handle acceleration
            if (accelerationMode)
            {
                // still speeding up
                accelerationPeriod += Time.deltaTime;
            }
            else
            {
                // cool down acceleration by half
                accelerationPeriod = Mathf.Clamp(accelerationPeriod * 0.5f, 1f, 1000f);
            }

            // Rotation; keys have higher priority than mouse pointer
            if (Input.GetKey(KeyCode.E))
            {
                Rotate(Vector3.up, accelerationMode);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                Rotate(Vector3.down, accelerationMode);
            }
            else if (Input.GetKey(KeyCode.Y))
            {
                Rotate(Vector3.left, accelerationMode);
            }
            else if (Input.GetKey(KeyCode.X))
            {
                Rotate(Vector3.right, accelerationMode);
            }
            else
            {
                lastMouse = Input.mousePosition - lastMouse;
                lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
                lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
                transform.eulerAngles = lastMouse;
                lastMouse = Input.mousePosition;
            }
            // Rotation of the object is done.

            // Moving the object.
            // Keyboard commands give the basic direction.
            Vector3 newPosition = GetBaseInput();

            if (accelerationMode)
            {
                // handle acceleration
                newPosition *= accelerationPeriod * acceleration * GetNormalSpeed();
                float maxSpeed = GetMaximalSpeed();
                // Note: we need to clamp by -maxSpeed and not 0 because we
                // might be moving backward in the respective dimension.
                newPosition.x = Mathf.Clamp(newPosition.x, -maxSpeed, maxSpeed);
                newPosition.y = Mathf.Clamp(newPosition.y, -maxSpeed, maxSpeed);
                newPosition.z = Mathf.Clamp(newPosition.z, -maxSpeed, maxSpeed);
            }
            else
            {
                newPosition *= GetNormalSpeed();
            }

            // Make the move to the new position
            transform.Translate(newPosition * Time.deltaTime);

            if (previousPosition != transform.position)
            {
                // Debug.Log("position: " + transform.position + "\n");
                previousPosition = transform.position;
            }
            if (previousRotation != transform.rotation)
            {
                // Debug.Log("rotation: " + transform.rotation + "\n");
                previousRotation = transform.rotation;
            }
        }

        private void ShowSelectedObject(Camera camera)
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            // Note: The object to be hit needs a collider.
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // if the hit object is a node, we show the Source.Name
                // of the graph node the object represents if it has a
                // name; in all other cases, the name attribute of object
                // is used instead.

                GameObject objectHit = hit.transform.gameObject;
                UnityEngine.UI.Text text = guiObjectNameTextField.GetComponent<UnityEngine.UI.Text>();
                if (objectHit.TryGetComponent<Node>(out Node node))
                {
                    if (node.TryGetString("Source.Name", out string nodeName))
                    {
                        text.text = nodeName;
                    }
                    //else if (!string.IsNullOrEmpty(node.LinkName))
                    //{
                    //    text.text = node.LinkName;
                    //}
                    
                    else
                    {
                        text.text = node.Type;
                        // text.text = objectHit.name;
                        Debug.Log("Node has neither Source.Name nor unique linkname.\n");
                        Dump(objectHit);
                    }
                }
                else if (objectHit.TryGetComponent<Edge>(out Edge edge))
                {
                    text.text = "Edge " + objectHit.name;
                    Debug.Log("Edge hit.\n");
                }
                else
                {
                    text.text = objectHit.name;
                    Debug.Log("No node hit.\n");
                }
            }
            else
            {
                Debug.Log("No object hit.\n");
            }
        }

        private static void Dump(GameObject obj)
        {
            Debug.Log("Selected: " + obj.name + "\n");
            if (obj.TryGetComponent<Node>(out Node node))
            {
                Debug.Log(node.ToString() + "\n");
            }
        }

        /// <summary>
        /// Returns a vector indicating the direction of the movement.
        /// An element in this vector can have one of the following
        /// values: -1, 0, or 1. A value of 0 means no move in this
        /// dimension; 1 to move forward in this dimension, and -1
        /// to move backward in this dimension.
        /// </summary>
        /// <returns>vector indicating the direction</returns>
        private Vector3 GetBaseInput()
        {
            Vector3 p_Velocity = new Vector3();

            // Forwards
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                p_Velocity += Vector3.forward;

            // Backwards
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                p_Velocity += Vector3.back;

            // Left
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                p_Velocity += Vector3.left;

            // Right
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                p_Velocity += Vector3.right;

            // Up
            if (Input.GetKey(KeyCode.Space))
                p_Velocity += Vector3.up;

            // Down
            if (Input.GetKey(KeyCode.LeftControl))
                p_Velocity += Vector3.down;

            return p_Velocity;
        }

        /*
        void OnScene(SceneView sceneview)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                Ray ray = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y));
                Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
            }
        } */
    }
}