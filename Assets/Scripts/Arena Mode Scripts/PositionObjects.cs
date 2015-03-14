using UnityEngine;
using System.Collections;

public class PositionObjects : MonoBehaviour
{
    public bool corners = true;
    public GameObject[] cornerObjects = new GameObject[4];

    public bool centerBorders = false;
    public GameObject topObject, midObject, bottomObject;

    // Use this for initialization
    void Start()
    {
        if (corners)
        {
            //bottom left
            cornerObjects [0].transform.position = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 1));
            //bottom right
            cornerObjects [1].transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 1));
            //top right
            cornerObjects [2].transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 1));
            //top left
            cornerObjects [3].transform.position = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 1));
        }
        if (centerBorders)
        {
            //top mid of the screen
            Vector3 temp = new Vector3(Screen.width / 2f, Screen.height, 10f);
            topObject.transform.position = Camera.main.ScreenToWorldPoint(temp);
            //mid of the screen
            temp = new Vector3(Screen.width / 2f, Screen.height / 2f, 10f);
            midObject.transform.position = Camera.main.ScreenToWorldPoint(temp);
            //bottom mid of the screen
            temp = new Vector3(Screen.width / 2f, 0, 10f);
            bottomObject.transform.position = Camera.main.ScreenToWorldPoint(temp);
        }

    }

}
