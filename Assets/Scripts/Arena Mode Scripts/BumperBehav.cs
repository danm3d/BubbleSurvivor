using UnityEngine;
using System.Collections;

public class BumperBehav : MonoBehaviour
{

    private Animator anim;
    private bool bumping = false;
	
    void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("Bumping", bumping);
    }
	
    void OnTriggerEnter2D(Collider2D collider)
    {
        bumping = true;
        anim.SetBool("Bumping", bumping);
    }

    public void ToggleBump()
    {
        bumping = false;
        anim.SetBool("Bumping", bumping);
    }

}
