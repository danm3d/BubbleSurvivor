using UnityEngine;
using System.Collections;

public class BumperBehav : MonoBehaviour
{

    private Animator anim;
    private bool bumping = false;
    //private BoxCollider2D boxColl;
	
    void Start()
    {
        anim = GetComponent<Animator>();
        //boxColl = GetComponent<BoxCollider2D>();
    }
	
    void OnTriggerEnter2D(Collider2D collider)
    {
        ToggleBump();
    }

    public void ToggleBump()
    {
        //boxColl.enabled = bumping;
        bumping = !bumping;
        anim.SetBool("Bumping", bumping);
    }

}
