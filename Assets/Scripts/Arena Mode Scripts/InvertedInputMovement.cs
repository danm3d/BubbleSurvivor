using UnityEngine;
using System.Collections;

public class InvertedInputMovement : MonoBehaviour
{

    private Vector2 direction;
    private Rigidbody2D myRigidbody;
    private float accelInitialX = 0, accelInitialY = 0;

    // Use this for initialization
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        //Record initial rotation to adjust for starting rotation.
        accelInitialX = Mathf.Clamp(Input.acceleration.x, -0.5f, 0.5f);
        accelInitialY = Mathf.Clamp(Input.acceleration.y, -0.5f, 0.5f);
    }
	
    // Update is called once per frame
    void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        direction.x = -(Input.acceleration.x - accelInitialX);
        direction.y = -(Input.acceleration.y - accelInitialY);
        if (direction == Vector2.zero)
        {
            direction.x = -Input.GetAxis("Horizontal");
            direction.y = -Input.GetAxis("Vertical");
        }
        
        if (direction.sqrMagnitude > 1)
        {
            direction.Normalize();
        }
        direction = new Vector3(20f * direction.x, 20f * direction.y);

        myRigidbody.velocity = (direction);
    }

}
