using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] private Vector3 respawnPos;
    [SerializeField] private Quaternion respawnRot;
    public LayerMask lavaLayer;

    private SwingingController swingingController;
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        swingingController = GetComponent<SwingingController>();
        respawnPos = transform.position;
        respawnRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RespawnPlayer()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = respawnRot;
        transform.position = respawnPos;
    }

    private void OnTriggerStay(Collider other)
    {
        //Debug.LogError("OnTriggerStay");
        // Check if the player has collided with a bar
        if (lavaLayer == (lavaLayer | (1 << other.gameObject.layer)))
        {
            Debug.LogError("LAVA !!!!!");
            swingingController.ReleaseBar();
            RespawnPlayer();
            //AttachToBar(other.transform);
        }
    }
}
