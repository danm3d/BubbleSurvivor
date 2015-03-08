using UnityEngine;
using System.Collections;

public class PositionBorder : MonoBehaviour
{

    public GameObject topBorder, bottomBorder, leftBorder, rightBorder;

    // Use this for initialization
    void Start()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height / 2f, 10));
        rightBorder.transform.position = pos;
        pos = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height / 2f, 10));
        leftBorder.transform.position = pos;
        pos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height, 10));
        topBorder.transform.position = pos;
        pos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, 0, 10));
        bottomBorder.transform.position = pos;
    }
	
    public void SetMaterial(Material newMat)
    {
        topBorder.GetComponent<Renderer>().material = newMat;
        bottomBorder.GetComponent<Renderer>().material = newMat;
        leftBorder.GetComponent<Renderer>().material = newMat;
        rightBorder.GetComponent<Renderer>().material = newMat;
    }

}
