using UnityEngine;

public class AirshipCircularMovement : MonoBehaviour
{
    public Transform centerPoint; // The point around which the airship will move
    public float radiusX = 10f; // Horizontal radius of the circle/oval
    public float radiusZ = 5f; // Vertical radius of the circle/oval
    public float speed = 1f; // Speed of the circular movement

    private float angle = 0f; // Tracks the current angle of the airship

    void Update()
    {
        // Update the angle based on speed and time
        angle += speed * Time.deltaTime;

        // Calculate the new position in a circular/oval path
        float x = Mathf.Cos(angle) * radiusX;
        float z = Mathf.Sin(angle) * radiusZ;
        Vector3 newPosition = new Vector3(x, 0f, z) + centerPoint.position;

        Vector3 direction = (newPosition - transform.position).normalized;
        // Move the airship to the new position
        transform.position = newPosition;

        // Rotate the airship to face the direction of motion
        if (direction != Vector3.zero) // Avoid zero vector error
        {
            var rot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }
}
