using UnityEngine;

/// <summary>
/// A class for destroying objects using various conditions
/// - destroy after a fixed time period
/// - destroy after a set number of collisions with another object
/// - destroy if the object is off screen - camera render viewport
/// - destroy if the object is idle for a fixed time - not moving
/// </summary>

public class ObjectDestruction : MonoBehaviour
{
    public bool destroyAfterTime = true;
    public float destructionTime = 5f;

    public bool destroyOnCollision = true;
    public int collisionDestroyThreshold = 2; // Number of collisions before destruction
    private int collisionCount;

    public bool destroyOffScreen = true;

    public bool destroyIfIdle = false;
    public float idleTimeThreshold = 3f;
    private float lastMoveTime;
    private Vector3 lastPosition;

    private Camera mainCamera;
    private bool isOffScreen = false;

    void Start()
    {
        mainCamera = Camera.main;
        lastPosition = transform.position;
        lastMoveTime = Time.time;

        if (destroyAfterTime)
        {
            Destroy(gameObject, destructionTime); // Destroy takes an optional delay in seconds
        }
    }

    void Update()
    {
        if (destroyOffScreen && !isOffScreen)
        {
            CheckOffScreen();
        }

        if (destroyIfIdle && (transform.position == lastPosition))
        {
            if (Time.time - lastMoveTime > idleTimeThreshold)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            lastPosition = transform.position;
            lastMoveTime = Time.time;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        collisionCount++;
        if (destroyOnCollision && collisionCount >= collisionDestroyThreshold)
        {
            Destroy(gameObject);
        }
    }

    void CheckOffScreen()
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        if (!onScreen)
        {
            isOffScreen = true;
            Destroy(gameObject);
        }
    }
}