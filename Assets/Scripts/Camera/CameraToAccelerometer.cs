using UnityEngine;
using System.Collections;

public class CameraToAccelerometer : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
	
    }
	
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(Input.acceleration);
    }
}
