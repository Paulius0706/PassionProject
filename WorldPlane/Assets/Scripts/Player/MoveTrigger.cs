using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTrigger : MonoBehaviour
{
    public bool contactToObjects;
    private double maxNonContactTime = 0.01;
    private double lastContact;
    // Start is called before the first frame update
    void Start()
    {
        contactToObjects = false;
        lastContact = Time.realtimeSinceStartupAsDouble;
    }
    private void Update()
    {
        if (Time.realtimeSinceStartupAsDouble - lastContact > maxNonContactTime)
        {
            contactToObjects = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.isTrigger == false)
        {
            contactToObjects = true;
            lastContact = Time.realtimeSinceStartupAsDouble;
        }
    }
}
