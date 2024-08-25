using System.Collections;
using UnityEngine;

public class ClawMechanism : MonoBehaviour
{
    public float dropSpeed = 5f; // Speed at which the claw drops
    public float retractSpeed = 5f; // Speed at which the claw retracts
    private float retractCurrentSpeed;
    public float maxExtensionLength = 10f; // Maximum length the claw can extend
    public Transform clawEnd; // Reference to the end of the claw
    public LayerMask barLayer; // Layer mask to identify bars
    public CollectorMovement characterMovement; // Reference to the CharacterMovement script
    public Animator animator;

    private bool isDropping = false;
    private bool isRetracting = false;
    private Transform grabbedObject = null;
    private Vector3 originalPosition;
    private float startY;
    private LineRenderer ropeLineRenderer;

    public BarShooter barShooter;

    void Start()
    {
        ropeLineRenderer = GetComponent<LineRenderer>();
        originalPosition = clawEnd.transform.localPosition;
        startY = clawEnd.transform.position.y;
    }

    void Update()
    {
        
        if ((Input.GetKeyDown(KeyCode.X)  || Input.GetKeyDown(KeyCode.DownArrow)) && !isDropping && !isRetracting)
        {
            isDropping = true;
            characterMovement.SetMovement(false); // Disable character movement
        }

        if (isDropping)
        {
            // Move the claw downwards
            clawEnd.transform.Translate(Vector3.down * dropSpeed * Time.deltaTime);

            // Check if claw has reached max extension length
            if (Mathf.Abs(clawEnd.transform.position.y - startY) >= maxExtensionLength)
            {
                isDropping = false;
                isRetracting = true;
                retractCurrentSpeed = retractSpeed;
            }
        }

        if (isRetracting)
        {
            // Move the claw upwards
            clawEnd.transform.localPosition = Vector3.MoveTowards(clawEnd.transform.localPosition, originalPosition, retractCurrentSpeed * Time.deltaTime);

            // If the claw has reached its original position
            if (clawEnd.transform.localPosition == originalPosition)
            {
                animator.SetBool("close", false);
                if (grabbedObject != null)
                {
                    Destroy(grabbedObject.gameObject); // Destroy the bar object
                    grabbedObject = null;
                }
                isRetracting = false;
                characterMovement.SetMovement(true); // Re-enable character movement

                if(retractCurrentSpeed!=retractSpeed)
                {
                    barShooter.AddAmmo();
                }
            }
        }
        // Update the LineRenderer to draw the rope between the character and the claw
        ropeLineRenderer.SetPosition(0, transform.position); // Start point of the rope (at the character's position)
        ropeLineRenderer.SetPosition(1, clawEnd.position); // End point of the rope (at the claw's position)
    }

    // Detect collision with a bar
    private void OnTriggerEnter(Collider other)
    {
        if (isDropping && ((1 << other.gameObject.layer) & barLayer) != 0)
        {
            animator.SetBool("close", true);
            // If the claw collides with a bar, grab it
            grabbedObject = other.transform;
            StartCoroutine(GrabCourotine());
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (isDropping && ((1 << other.gameObject.layer) & barLayer) != 0)
        {
            animator.SetBool("close", true);
            // If the claw collides with a bar, grab it
            grabbedObject = other.transform;
            StartCoroutine(GrabCourotine());
        }
    }

    IEnumerator GrabCourotine()
    {
        float waitTime = 0;
        while(waitTime<0.05f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
        PerformGrab();
    }
    void PerformGrab()
    {
        grabbedObject.SetParent(clawEnd);
        grabbedObject.position = 
        new Vector3((grabbedObject.position.x + clawEnd.position.x) / 2, 
        grabbedObject.position.y, grabbedObject.position.z);
        isDropping = false;
        isRetracting = true;
        retractCurrentSpeed = retractSpeed*2;
    }
}
