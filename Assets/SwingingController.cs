using UnityEngine;

public class SwingingController : MonoBehaviour
{
  
    public float swingForce = 10f;           // Force applied to swing left/right
    public Transform holdPoint1, holdPoint2;              // The point where the player holds the bar (e.g., hands)
    public LayerMask barLayer;               // Layer mask to detect bars
    public float releaseMomentumFactor = 1.0f; // Factor to maintain momentum when releasing

    public float spring = 10f;               // Spring constant for SpringJoint
    public float damper = 1f;                // Damping for SpringJoint
    public float breakForce=100;

    private Rigidbody rb;
    private Transform currentBar;
    private Joint swingJoint;                // Can be SpringJoint
    private bool isSwinging;
    private Vector3 lastVelocity;

    private Vector3 holdPointInitialPosition1, holdPointInitialPosition2;
    private Transform lastBar;

    private void Start()
    {
        holdPointInitialPosition1 = holdPoint1.localPosition;
        holdPointInitialPosition2 = holdPoint2.localPosition;
        rb = GetComponent<Rigidbody>();
        rb.sleepThreshold = 0.0f;
        isSwinging = false;
    }

    private void Update()
    {
        if (isSwinging)
        {
            if(swingJoint==null || currentBar == null)
            {
                ReleaseBar();
                return;
            }
            swingJoint.connectedAnchor = currentBar.position;
            holdPoint1.position = currentBar.position + new Vector3(0, 0, 0.5f);
            holdPoint2.position = currentBar.position + new Vector3(0, 0, -0.5f);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ReleaseBar(); // Release the bar
               
            }
            else if (Input.GetKey(KeyCode.A))// || Input.GetKey(KeyCode.LeftArrow))
            {
                Swing(-1); // Swing left
            }
            else if (Input.GetKey(KeyCode.D))// || Input.GetKey(KeyCode.RightArrow))
            {
                Swing(1); // Swing right
            }
            lastVelocity = rb.velocity;
        }
        
    }

    private void Swing(int direction)
    {
        if (currentBar == null) return;

        // Calculate swing direction
        Vector3 barDirection = currentBar.up;
        Vector3 swingDirection = Vector3.Cross(Vector3.up, barDirection).normalized * direction;

        // Apply force to swing
        rb.AddForce(swingDirection * swingForce * Time.deltaTime *100, ForceMode.Acceleration);
        //lastVelocity = rb.velocity;
    }

    public void ReleaseBar()
    {
      

        // Remove the joint and maintain momentum
        if (swingJoint != null)
        {
            Destroy(swingJoint);
        }

        rb.velocity = lastVelocity * releaseMomentumFactor;
        isSwinging = false;

        holdPoint1.localPosition = holdPointInitialPosition1;
        holdPoint2.localPosition = holdPointInitialPosition2;

        if (currentBar == null) return;
        currentBar = null;
        Invoke("ForgetBar", 0.7f);
        //lastBar = null;


    }
    private void ForgetBar()
    {
        lastBar = null;
    }
    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("OnTriggerEnter");
    //    if (isSwinging) return;
    //    Debug.LogWarning("OnTriggerEnter and will attach");
    //    // Check if the player has collided with a bar
    //    if (barLayer == (barLayer | (1 << other.gameObject.layer)))
    //    {
    //        Debug.LogError("OnTriggerEnter ATTACHED");
    //        AttachToBar(other.transform);
    //    }
    //}
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("OnCollisionEnter");
        if (isSwinging) return;
        //Debug.LogWarning("OnCollisionEnter and will attach");
        // Check if the player has collided with a bar
        if (barLayer == (barLayer | (1 << collision.gameObject.layer)))
        {
            Transform barTransform = collision.transform.GetChild(0);
            Debug.LogError("OnCollisionEnter ATTACHED");
            AttachToBar(barTransform.transform);
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log("OnCollisionEnter");
        if (isSwinging) return;
        //Debug.LogWarning("OnCollisionEnter and will attach");
        // Check if the player has collided with a bar
        if (barLayer == (barLayer | (1 << collision.gameObject.layer)))
        {
            Transform barTransform = collision.transform.GetChild(0);
            Debug.LogError("OnCollisionStay ATTACHED");
            AttachToBar(barTransform.transform);
        }
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    //Debug.LogError("OnTriggerStay");
    //    if (isSwinging) return;


    //    // Check if the player has collided with a bar
    //    if (barLayer == (barLayer | (1 << other.gameObject.layer)))
    //    {
    //        Debug.LogError("OnTriggerStay ATTACHED");
    //        AttachToBar(other.transform);
    //    }
    //}
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (isSwinging) return;

    //    // Check if the player has collided with a bar
    //    if (barLayer == (barLayer | (1 << collision.gameObject.layer)))
    //    {
    //        // Find the bar's transform from the collision object
    //        Transform bar = collision.transform;

    //        // Perform the attachment
    //        AttachToBar(bar);
    //    }
    //}



    private void AttachToBar(Transform bar)
    {
        if (lastBar == bar)
        {
            return;
        }
        lastBar = bar;
        currentBar = bar;
        isSwinging = true;

        // Remove any existing joint
        if (swingJoint != null)
        {
            Destroy(swingJoint);
        }

        holdPoint1.localPosition = new Vector3(0, 22.7f, 0.5f);
        holdPoint2.localPosition = new Vector3(0, 22.7f, -0.5f);

        swingJoint = gameObject.AddComponent<SpringJoint>();
        ConfigureSpringJoint((SpringJoint)swingJoint, bar);

        //swingJoint = gameObject.AddComponent<ConfigurableJoint>();
        //ConfigureCustomJoint((ConfigurableJoint)swingJoint, bar);
    }

    private void ConfigureSpringJoint(SpringJoint joint, Transform bar)
    {
        joint.connectedBody = bar.GetComponent<Rigidbody>();

        // Calculate the local anchor position of the holdPoint relative to the player
        Vector3 holdPointLocalPosition = new Vector3(0, 2, 0);// holdPoint.localPosition;

        // Calculate the world position of the bar
        Vector3 barWorldPosition = bar.position;
        joint.autoConfigureConnectedAnchor = false;
        joint.breakForce = breakForce;
        // Set SpringJoint properties
        joint.anchor = holdPointLocalPosition;            // Anchor where the player’s hands would be
        joint.connectedAnchor = barWorldPosition;         // Anchor where the bar is positioned
        joint.spring = spring;                           // Spring constant
        joint.damper = damper;                           // Damping factor
        joint.minDistance = 0.0f;                        // Minimal distance
        joint.maxDistance = 0.01f;                        // No additional distance
    }

    private void ConfigureCustomJoint(ConfigurableJoint joint, Transform bar)
    {
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = bar.GetComponent<Rigidbody>();

        Vector3 holdPointLocalPosition1 = holdPoint1.localPosition;
        Vector3 holdPointLocalPosition2 = holdPoint2.localPosition;
        Vector3 barWorldPosition = bar.position;

        joint.anchor = (holdPointLocalPosition1+ holdPointLocalPosition2) /2;
        joint.connectedAnchor = barWorldPosition;

        // Set motion types
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Free;

        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        // Linear Limit
        joint.linearLimit = new SoftJointLimit { limit = 1000f }; // Decreased limit to allow for spring action

        // Angular Limits
        joint.lowAngularXLimit = new SoftJointLimit { limit = 300f };
        joint.angularYLimit = new SoftJointLimit { limit = 300f };
        joint.angularZLimit = new SoftJointLimit { limit = 300f };

        // Joint Drive Settings
        joint.xDrive = new JointDrive
        {
            positionSpring = spring,  // Ensure spring value is set appropriately
            positionDamper = damper,  // Ensure damper value is set appropriately
            maximumForce = Mathf.Infinity
        };
        joint.yDrive = new JointDrive
        {
            positionSpring = spring,
            positionDamper = damper,
            maximumForce = Mathf.Infinity
        };
        joint.zDrive = new JointDrive
        {
            positionSpring = spring,
            positionDamper = damper,
            maximumForce = Mathf.Infinity
        };

        // Adjust Rigidbody settings if necessary
        Rigidbody connectedBody = joint.connectedBody;
        if (connectedBody != null)
        {
            connectedBody.drag = 0.5f;  // Add some drag for stability
            connectedBody.angularDrag = 0.5f; // Add angular drag for stability
        }
    }


}
