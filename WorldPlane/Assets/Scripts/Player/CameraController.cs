using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float sensitivity;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = target.transform.position + offset;
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && Input.GetKey(KeyCode.LeftShift))
        {
            offset = offset - offset.normalized * Time.deltaTime * sensitivity;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && Input.GetKey(KeyCode.LeftShift))
        {
            offset = offset + offset.normalized * Time.deltaTime * sensitivity;
        }
    }
}
