using UnityEngine;

public class ForcePushTester : MonoBehaviour
{
    public float thrust;
    public Vector3 direction;
    public Rigidbody rb;
    
    public bool isPushing;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update(){
        if (Input.GetKey(KeyCode.Space)){
            isPushing = true;
        }
        else{
            isPushing = false;
        }
    }
    void FixedUpdate()
    {
        if (isPushing){
            rb.AddForce(direction * thrust);
        }
        
    }
}
