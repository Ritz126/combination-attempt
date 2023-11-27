using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ZombieAi : MonoBehaviour
{
    private const float SPEED_DIVIDER = 1000;

    public GameObject target;
    public float speed = 1.5f;
    public float maxDistance = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            target = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            target = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) {

        }
        else
        {
            if (Vector3.Distance(transform.position, target.transform.position) > maxDistance)
            {
                transform.LookAt(target.gameObject.transform);
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed / SPEED_DIVIDER);
            }
        }
        
    }
}
