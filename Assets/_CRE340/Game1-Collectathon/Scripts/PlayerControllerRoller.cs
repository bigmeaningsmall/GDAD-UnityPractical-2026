using UnityEngine;

public class PlayerControllerRoller : MonoBehaviour
{
	// variables and references
    public float speed = 5f;
    private Rigidbody rb;

    void Start()
    {
	    //get the rigidbody component from the gameobject we attach the player controller to
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
	    // get some inputs for horizontal and vertical using the old input system
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

		// create a vector3 (x,y,z) for movement and 'cast' the inputs to vector arguements or parameters 
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        
        //apply the movement to our rigidbody and multiply by speed - this allows us to tune the movement speed to the setting we like
        rb.AddForce(movement * speed);
    }
}