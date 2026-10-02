using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1; // how much damage this bullet deals - used in Part 2

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // cache the rigidbody
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Stop the bullet dead and let it drop - a cheap 'impact' effect
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = true;

        // TODO (Part 2) - check if what we hit can be damaged, and damage it
    }
}