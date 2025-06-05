using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassScript : MonoBehaviour
{
    void FixedUpdate()
    {

        GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");
        if (targets.Length == 0) return;

        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        if (closestTarget != null)
        {
            Vector3 direction = closestTarget.transform.position - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
    void Update()
    {
    }
}
