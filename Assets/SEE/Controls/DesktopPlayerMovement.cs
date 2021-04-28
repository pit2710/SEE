﻿using SEE.Utils;
using UnityEngine;

namespace SEE.Controls
{

    public class DesktopPlayerMovement : MonoBehaviour
    {
        [Tooltip("Speed of movements")]
        public float Speed = 2.0f;
        [Tooltip("Boost factor of speed, applied when shift is pressed.")]
        public float BoostFactor = 2.0f;

        private struct CameraState
        {
            internal float distance;
            internal float yaw;
            internal float pitch;
            internal bool freeMode;
        }

        private CameraState cameraState;        

        [Tooltip("The code city which the player is focusing on.")]
        public GO.Plane focusedObject;

        private void Start()
        {
            lastAxis = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Camera mainCamera = MainCamera.Camera;
            if (focusedObject != null)
            {                
                mainCamera.transform.position = focusedObject.CenterTop;
            }
            else
            {
                cameraState.freeMode = true;
                Debug.Log($"Player {name} has no focus object assigned.\n");
            }
            cameraState.distance = 2.0f;
            cameraState.yaw = 0.0f;
            cameraState.pitch = 45.0f;
            mainCamera.transform.rotation = Quaternion.Euler(cameraState.pitch, cameraState.yaw, 0.0f);
            mainCamera.transform.position -= mainCamera.transform.forward * cameraState.distance;
            lastAxis = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        private void Update()
        {
            Camera mainCamera = MainCamera.Camera;
            if (SEEInput.ToggleCameraLock())
            {
                if (cameraState.freeMode)
                {
                    Vector3 positionToFocusedObject = focusedObject.CenterTop - transform.position;
                    cameraState.distance = positionToFocusedObject.magnitude;
                    transform.forward = positionToFocusedObject;
                    Vector3 pitchYawRoll = transform.rotation.eulerAngles;
                    cameraState.pitch = pitchYawRoll.x;
                    cameraState.yaw = pitchYawRoll.y;
                }
                cameraState.freeMode = !cameraState.freeMode;
            }

            float speed = Speed * Time.deltaTime;
            if (SEEInput.BoostCameraSpeed())
            {
                speed *= BoostFactor;
            }

            if (!cameraState.freeMode)
            {
                float d = 0.0f;
                if (SEEInput.MoveForward())
                {
                    d += speed;
                }
                if (SEEInput.MoveBackward())
                {
                    d -= speed;
                }
                cameraState.distance -= d;

                HandleRotation();
                mainCamera.transform.position = focusedObject.CenterTop;
                mainCamera.transform.rotation = Quaternion.Euler(cameraState.pitch, cameraState.yaw, 0.0f);
                mainCamera.transform.position -= mainCamera.transform.forward * cameraState.distance;
            }
            else // cameraState.freeMode == true
            {
                Vector3 v = Vector3.zero;
                if (SEEInput.MoveForward())
                {
                    v += mainCamera.transform.forward;
                }
                if (SEEInput.MoveBackward())
                {
                    v -= mainCamera.transform.forward;
                }
                if (SEEInput.MoveRight())
                {
                    v += mainCamera.transform.right;
                }
                if (SEEInput.MoveLeft())
                {
                    v -= mainCamera.transform.right;
                }
                if (SEEInput.MoveUp())
                {
                    v += Vector3.up;
                }
                if (SEEInput.MoveDown())
                {
                    v += Vector3.down;
                }
                v.Normalize();
                v *= speed;
                mainCamera.transform.position += v;

                HandleRotation();
                mainCamera.transform.rotation = Quaternion.Euler(cameraState.pitch, cameraState.yaw, 0.0f);
            }
        }

        /// <summary>
        /// The mouse position of the last frame.
        /// </summary>
        private Vector2 lastAxis;

        /// <summary>
        /// If the user wants us, we rotate the camera according to mouse input.
        /// Modifies <see cref="cameraState.yaw"/> and <see cref="cameraState.pitch"/>.
        /// 
        /// Note: This is a workaround of issues with the correct mouse position
        /// in a remote-desktop session.
        /// </summary>
        private void HandleRotation()
        {
            if (SEEInput.RotateCamera())
            {
                float x = -(lastAxis.x - Input.mousePosition.x) * 0.1f;
                float y = -(lastAxis.y - Input.mousePosition.y) * 0.1f;

                // These were the original statements which, however, do not work in
                // a remote-desktop session (RDP).
                // float x = Input.GetAxis("Mouse X");
                // float y = Input.GetAxis("Mouse Y");

                cameraState.yaw += x;
                cameraState.pitch -= y;
            }
            lastAxis.x = Input.mousePosition.x;
            lastAxis.y = Input.mousePosition.y;
        }
    }
}