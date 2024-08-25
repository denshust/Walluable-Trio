using UnityEngine;

public class CollectorMovement : MonoBehaviour
{
    public float moveSpeed = 5f;          // Speed of the character's movement
    public float acceleration = 5f;       // Speed at which the character accelerates to full speed
    public float deceleration = 5f;       // Speed at which the character decelerates to a stop

    private float initialY;
    private float initialZ;
    private float currentSpeed = 0f;      // Current speed of the character
    private float targetSpeed = 0f;       // Target speed based on input
    private bool canMove = true;

    void Start()
    {
        // Store the initial Y and Z positions of the character
        initialY = transform.position.y;
        initialZ = transform.position.z;
    }

    void Update()
    {
        if (canMove)
        {
            // Check for left or right movement input (Z for left, C for right)
            if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.LeftArrow))
            {
                targetSpeed = -moveSpeed; // Move left
            }
            else if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.RightArrow))
            {
                targetSpeed = moveSpeed;  // Move right
            }
            else
            {
                targetSpeed = 0f;         // No input, decelerate to stop
            }

            // Smoothly interpolate the current speed towards the target speed
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, (targetSpeed == 0 ? deceleration : acceleration) * Time.deltaTime);

            // Calculate new position
            Vector3 newPosition = transform.position + new Vector3(currentSpeed * Time.deltaTime, 0f, 0f);

            // Lock the Y and Z position
            newPosition.y = initialY;
            newPosition.z = initialZ;

            // Update the character's position
            transform.position = newPosition;
        }
    }

    // Method to enable or disable movement
    public void SetMovement(bool isEnabled)
    {
        canMove = isEnabled;
        if (!canMove)
        {
            currentSpeed = 0f; // Stop movement immediately when disabling movement
        }
    }
}
