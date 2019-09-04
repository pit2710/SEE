﻿using UnityEngine;

/// <summary>
/// Rotates a 3D text so that it always looks to the main camera such that
/// it can be seen from all angles.
/// </summary>
public class TextFacingCamera : MonoBehaviour
{
    // The time in seconds until the text is updated again.
    private const float updatePeriod = 1.0f;

    /// <summary>
    /// The minimal distance between the text and the main camera to become visible.
    /// If the actual distance is below this value, the object will not be visible.
    /// If minimalDistance > maximalDistance, the object will not be visible.
    /// </summary>
    public float minimalDistance = 5.0f;

    /// <summary>
    /// The maximal distance between the text and the main camera to become visible.
    /// If the actual distance is above this value, the object will not be visible.
    /// If minimalDistance > maximalDistance, the object will not be visible.
    /// </summary>
    public float maximalDistance = 20.0f;

    // Time since the start of the last update period.
    private float timer = updatePeriod;

    // The last known position of the main camera.
    private Vector3 lastCameraPosition = Vector3.zero;

    // Vector to describe the rotation of the text. Needed to show the text correctly.
    private static Vector3 rotation = Vector3.up - new Vector3(0, 180, 0);

    // The renderer of the gameObject.
    private Renderer gameObjectRenderer;

    private void Start()
    {
        gameObjectRenderer = gameObject.GetComponentInChildren<Renderer>();
    }

    /// <summary>
    /// Updates the associated game object (the text) every update period if the
    /// camera has moved at all since the last update. The associated object will
    /// be rendered if its distance to the main camera does not exceed the
    /// minimalDistance. If it is rendered, it will be rotated to the face the
    /// camera so that it can always be seen.
    /// </summary>
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0.0f)
        {
            timer = updatePeriod;
            Camera mainCamera = Camera.main;
            if (mainCamera.transform.position != lastCameraPosition)
            {
                Vector3 heading = transform.position - mainCamera.transform.position;
                float distance = Mathf.Abs(Vector3.Dot(heading, mainCamera.transform.forward));
                gameObjectRenderer.enabled = (minimalDistance <= distance && distance <= maximalDistance);

                Debug.LogFormat("{0} is within range: {1}  {2}\n", gameObject.name, (gameObjectRenderer.enabled ? "yes" : "no"), distance);

                if (gameObjectRenderer.enabled)
                {
                    lastCameraPosition = mainCamera.transform.position;
                    gameObject.transform.LookAt(mainCamera.transform);
                    gameObject.transform.Rotate(rotation);
                } 
            }
        }
    }
}
