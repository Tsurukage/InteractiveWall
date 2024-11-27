using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudLooper : MonoBehaviour
{
    public Camera mainCamera; // Reference to the perspective camera
    public float speed = 5f; // Speed of movement
    public float depth = 10f; // Depth at which the object moves

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main; // Automatically assign the main camera if not set
    }

    void Update()
    {
        // Move the object to the left
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Convert position to viewport space
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        // Check if the object is out of bounds
        if (viewportPosition.x < 0)
        {
            // Reposition it to the right side
            Vector3 newViewportPosition = new Vector3(1, viewportPosition.y, viewportPosition.z);
            transform.position = mainCamera.ViewportToWorldPoint(newViewportPosition);
        }
    }
}
