using UnityEngine;
using UnityEngine.InputSystem; // Required to use the new Input System

public class EyeFollowCursor : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How fast the eye rotates to look at the cursor.")]
    public float rotationSpeed = 5f;

    private Camera mainCamera;

    void Start()
    {
        // Find and assign the main camera in the scene automatically
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Make sure your camera has the 'MainCamera' tag.");
        }
    }

    void Update()
    {
        // Safety check: Make sure a mouse is connected and recognized by the new Input System
        if (Mouse.current == null || mainCamera == null) return;

        // Get the current mouse position on the 2D screen
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        // Calculate the distance from the camera to the eye object
        float distanceFromCamera = Vector3.Distance(transform.position, mainCamera.transform.position);

        // Convert the 2D mouse position to a 3D point in the world
        // We use the distance from the camera so the point is at the exact same depth as the eye
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, distanceFromCamera));

        // Calculate the direction from the eye to the 3D mouse position
        Vector3 lookDirection = mouseWorldPosition - transform.position;

        // Ensure the direction is not zero to avoid console errors
        if (lookDirection != Vector3.zero)
        {
            // Determine the mathematical rotation needed to look at the target (up, down, left, right)
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            // Smoothly transition from the current rotation to the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}