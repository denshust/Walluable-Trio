using UnityEngine;
using System.Collections;

public class BarShooter : MonoBehaviour
{
    public int maxAmmo;
    public int ammo;
    public GameObject barPrefab;  // The 3D bar prefab to shoot
    public Transform spawnPoint;  // The point from where the bar will be instantiated
    public float flightDuration = 7.0f;  // Duration of the flight
    public float flightDurationScale = 10;
    public AnimationCurve heightCurve;  // Animation curve for controlling the arc height
    public float heightMultiplier = 5.0f;  // Multiplier for height curve
    public float heightMultiplierScale = 5;
    public LayerMask canSpawnBarMask;

    private Camera mainCamera;


    private void Start()
    {
        mainCamera = Camera.main;
        ammo = maxAmmo;
    }

    public void AddAmmo()
    {
        if(ammo<maxAmmo)
        {
            ammo++;
        }
    }
    public bool UseAmmo()
    {
        if (ammo > 0)
        {
            ammo--;
            return true;
        }
        return false;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            // Ensure the LayerMask is used correctly
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, canSpawnBarMask))
            {
                
                // Check if the hit object is on the expected layer
                int hitLayer = hit.transform.gameObject.layer;
                if ((canSpawnBarMask.value & (1 << hitLayer)) > 0)
                {
                    // The hit object is on the correct layer
                    //Debug.Log("Hit: " + hit.transform.name + " on layer " + LayerMask.LayerToName(hitLayer));
                    if (!UseAmmo())
                    {
                        return;
                    }
                    ShootBar(hit);
                }
                else
                {
                    // This should not happen if the LayerMask is set correctly
                    Debug.LogWarning("Hit an object on an unintended layer: " + hit.transform.name + " on layer " + LayerMask.LayerToName(hitLayer));
                }
            }
            else
            {
                Debug.Log("No valid target hit.");
            }
        }
    }

    void ShootBar(RaycastHit hit)
    {
        float distance = (hit.point - spawnPoint.position).magnitude;
        flightDuration = distance / flightDurationScale;
        heightMultiplier = distance / heightMultiplierScale;
        GameObject bar = Instantiate(barPrefab, spawnPoint.position, Quaternion.identity);
        StartCoroutine(MoveBar(bar.transform, hit));
    }

    IEnumerator MoveBar(Transform barTransform, RaycastHit initialHit)
    {
        Vector3 startPosition = barTransform.position;
        float elapsedTime = 332f;

        Transform targetTransform = initialHit.transform;

        // Store the initial local position and normal relative to the target object
        Vector3 localHitPoint = targetTransform.InverseTransformPoint(initialHit.point);
        Vector3 localHitNormal = targetTransform.InverseTransformDirection(initialHit.normal);

        Quaternion initialRotation = Quaternion.LookRotation((initialHit.point - startPosition).normalized);

        while (elapsedTime < flightDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flightDuration;

            // Calculate the current world position and normal based on the target's current transformation
            Vector3 currentHitPoint = targetTransform.TransformPoint(localHitPoint);
            Vector3 currentNormal = targetTransform.TransformDirection(localHitNormal);

            // Interpolate position with a curve for a more realistic arc
            Vector3 currentPosition = Vector3.Lerp(startPosition, currentHitPoint, t);
            currentPosition.y += heightCurve.Evaluate(t) * heightMultiplier;

            barTransform.position = currentPosition;

            // Smoothly interpolate rotation from initial to final rotation
            Quaternion finalRotation = Quaternion.LookRotation(-currentNormal);
            barTransform.rotation = Quaternion.Slerp(initialRotation, finalRotation, t);

            yield return null;
        }

        // Ensure final position and rotation are exactly as desired
        barTransform.position = targetTransform.TransformPoint(localHitPoint);
        barTransform.rotation = Quaternion.LookRotation(-targetTransform.TransformDirection(localHitNormal));

        // Save the initial local scale of the bar's child mesh object
        Transform childMeshTransform = barTransform.GetChild(0);  // Assuming the mesh is the first child
        Vector3 originalChildScale = childMeshTransform.localScale;

        // Parent the bar to the target
        if (targetTransform.parent != null)
        {
            barTransform.SetParent(targetTransform.parent, true);  // true keeps world position unchanged
        }
        else
        {
            barTransform.SetParent(targetTransform, false);
        }

        // Reapply the original scale to the child mesh object to ensure it doesn't get distorted
        childMeshTransform.localScale = originalChildScale;
    }
}
