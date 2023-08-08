using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f; // Controls the rotation speed/lag
    [SerializeField] private float smoothingFactor = 5f;

    private Transform mainCameraTransform;
    private Quaternion desiredRotation; // Used for damping

    // Update is called once per frame
    void Update()
    {
        // Get the main camera in the scene
        if (mainCameraTransform == null)
            mainCameraTransform = Camera.main.transform;

        if (mainCameraTransform != null)
        {
            // Calculate the direction from the object to the main camera
            Vector3 lookDirection = mainCameraTransform.position - transform.position;
            lookDirection = -lookDirection;

            // Filter noise by smoothing the lookDirection vector (optional)
            lookDirection = Vector3.Lerp(transform.forward, lookDirection, Time.deltaTime * smoothingFactor);

            // Calculate the desired rotation to face the camera
            desiredRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

            // Apply rotation with lag using spherical linear interpolation (slerp)
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            Debug.LogWarning("No main camera found in the scene.");
        }
    }
}
