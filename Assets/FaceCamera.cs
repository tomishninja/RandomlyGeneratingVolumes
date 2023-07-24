using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // Get the main camera in the scene
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            // Calculate the direction from the object to the main camera
            Vector3 lookDirection = mainCamera.transform.position - transform.position;
            lookDirection = -lookDirection;

            // Rotate the object's transform to face the camera
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
        else
        {
            Debug.LogWarning("No main camera found in the scene.");
        }
    }
}
