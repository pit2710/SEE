﻿using UnityEngine;

namespace SEE.Net
{

    /// <summary>
    /// The player synchronizes the player transform with the camera transform.
    /// </summary>
    public class Player : MonoBehaviour
    {
        /// <summary>
        /// The transform of the main camera.
        /// </summary>
        Transform cameraTransform;

        /// <summary>
        /// Initializes the player prefab or destroys this script, if this client is not
        /// the owner, so that the transform is not synchronized with the main camera of
        /// the other client.
        /// </summary>
        void Start()
        {
            if (GetComponent<ViewContainer>().IsOwner())
            {
                cameraTransform = Camera.main.transform ?? throw new System.ArgumentNullException("Main camera must not be null!");
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            else
            {
                Destroy(this);
            }
        }

        /// <summary>
        /// Synchronizes transform with camera transform.
        /// </summary>
        void Update()
        {
            transform.position = cameraTransform.position;
            transform.rotation = cameraTransform.rotation;
        }
    }

}
