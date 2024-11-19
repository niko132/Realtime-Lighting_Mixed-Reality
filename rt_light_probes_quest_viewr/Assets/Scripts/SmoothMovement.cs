using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothMovement : MonoBehaviour
{
    public float moveSpeed = 1.0f;
    public float rotationSpeed = 5.0f;
    public Transform cameraTransform;  // AR camera or main camera
    public Transform characterTransform; // The transform you want to move and rotate

    void Start()
    {

    }

    void Update()
    {
        // Get input from the left and right thumbsticks
        Vector2 moveInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector2 rotationInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        // Calculate movement direction based on camera's forward and right directions
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Flatten the forward and right vectors to ignore vertical movement
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Compute the desired movement direction relative to the camera
        Vector3 desiredMoveDirection = forward * moveInput.y + right * moveInput.x;

        // Move the character
        characterTransform.position += 2.0f * desiredMoveDirection * moveSpeed * Time.deltaTime;

        // Calculate the desired rotation based on the right thumbstick input
        if (rotationInput.sqrMagnitude > 0.01f) // Check if there is a significant input
        {
            // Quaternion desiredRotation = Quaternion.LookRotation(forward * rotationInput.y + right * rotationInput.x);
            // characterTransform.rotation = Quaternion.Slerp(characterTransform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);

            characterTransform.Rotate(0, 50.0f * rotationInput.x * rotationSpeed * Time.deltaTime, 0);
        }
    }
}
