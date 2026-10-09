using UnityEngine;

public class Shoot : MonoBehaviour
{
    [Header("Input Manager Reference")]
    public InputManager inputManager; // drag the InputManager object onto this in the Inspector

    [Space(10)]

    public GameObject bulletPrefab;     // Reference to the bullet prefab
    public Transform bulletSpawnPoint;  // Where the bullet appears from

    public float bulletSpeed = 20f;     // Speed of the bullet
    public float shootCooldown = 0.2f;  // Cooldown in seconds between shots

    private float lastShootTime = -100f; // Initialise to a low value so we can fire immediately

    void Start()
    {
        // If no bullet spawn point is assigned, create one in front of the player
        if (bulletSpawnPoint == null)
        {
            bulletSpawnPoint = new GameObject().transform;
            bulletSpawnPoint.name = "Bullet Spawn Point";
            bulletSpawnPoint.parent = transform;                        // child of the player, so it rotates with us
            bulletSpawnPoint.localPosition = new Vector3(0f, 0.2f, 1f); // slightly up and 1 unit in front
            bulletSpawnPoint.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        // TODO - CHANGE THIS  TO USE A BUTTON OR - Dash is now set to buttonwest
        // Read the fire button from the InputManager
        // Held() keeps firing while the trigger is down - the cooldown controls the fire rate
        if (inputManager.RightTriggerPressed.Held() && Time.time > lastShootTime + shootCooldown)
        {
            Fire();
        }
        //todo buttons need changed 
        // if (inputManager.ButtonSouth.Held() && Time.time > lastShootTime + shootCooldown)
        // {
        //     Fire();
        // }
    }

    void Fire()
    {
        // Instantiate the bullet prefab at the spawn point position and rotation
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Fire it in the direction the player is facing
        Vector3 bulletDirection = transform.forward;

        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

        if (bulletRb != null)
        {
            bulletRb.linearVelocity = bulletDirection * bulletSpeed;
        }

        // Update the last shoot time to enforce the cooldown
        lastShootTime = Time.time;
    }
}