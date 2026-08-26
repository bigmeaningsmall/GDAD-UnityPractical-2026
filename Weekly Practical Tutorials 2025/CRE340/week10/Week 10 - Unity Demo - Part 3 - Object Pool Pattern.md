
# **Introduction to Object Pooling**

**Object Pooling** is an important and widely used **optimisation design pattern** in game development. It focuses on reusing objects that are expensive to create and destroy, such as bullets, enemies, or particle effects. Instead of instantiating a new object every time it is needed and destroying it after use, a pool of pre-instantiated objects is maintained and reused. 

---

### **Why Use Object Pooling?**

1. **Performance Optimisation**:
   - Creating and destroying objects frequently consumes CPU and memory resources.
   - Object pooling minimises the overhead of `Instantiate` and `Destroy`, leading to smoother gameplay.

2. **Reduced Garbage Collection**:
   - Frequent destruction of objects generates garbage (unused memory) that Unity's garbage collector must clean up.
   - Object pooling reduces garbage generation, preventing spikes in frame time caused by garbage collection.

3. **Essential for High-Frequency Objects**:
   - Games with rapid object creation (e.g., shooting projectiles, spawning enemies) benefit significantly.
   - Object pooling ensures a steady frame rate, even during intense action scenes.

---

### **Why Is It Important as a Design Pattern?**

- **Reusability**:
  - Objects in the pool are reset and reused, following the principles of resource efficiency.
  
- **Scalability**:
  - Object pooling handles performance bottlenecks when the number of active objects increases.
  
- **Flexibility**:
  - It can be applied to various game elements like projectiles, particle systems, enemy AI, or even UI elements like damage indicators.

---

By adopting the object pooling design pattern, developers can ensure their games run efficiently, especially in scenarios where large numbers of objects are created and destroyed repeatedly. It is a fundamental practice in optimised game design and a great learning opportunity for understanding resource management.



# **Practical Guide: Adding and Using a Bullet Object Pool**

In this guide, you will learn to integrate and use an **object pool** to optimise bullet spawning and reusability in your Unity project. This will reduce the overhead of repeatedly instantiating and destroying bullets. Follow these steps to add the object pool and connect it with the updated `Shoot` and `Bullet` scripts.

---

### **What is an Object Pool?**
An **object pool** is a collection of reusable objects that are pre-created and managed to improve performance. Instead of instantiating a new bullet every time you shoot (and destroying it after), the object pool reuses inactive bullets, reducing CPU and memory overhead.

---

### **Steps to Add and Use the Bullet Object Pool**

#### **1. Add the `BulletPool` Manager**
1. Create an empty GameObject in the scene and name it `BulletPool`.
2. Add the `BulletPool` script to this GameObject.
3. Assign the bullet prefab to the `bulletPrefab` field in the Inspector.
	- Create a `PF_Bullet_Pooled` prefab in the Prefabs folder.
		- This bullet does not destroy itself but is recycled
		- `PF_Bullet_alt` is still attached to the player `Shoot` script. It will be used as normal (*Instantiate and Destroy*) is the object pool is set false.
4. Set the `poolSize` to the desired number of bullets (e.g., `30`).

Here’s the `BulletPool` script:

```csharp
using UnityEngine;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{
    public GameObject bulletPrefab; // The prefab to pool
    public int poolSize = 30; // Number of bullets in the pool

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        // Prepopulate the pool with bullet instances
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.transform.parent = transform; // Set the pool as the parent
            bullet.SetActive(false); // Disable bullets by default
            pool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet()
    {
        // If the pool is empty, create a new bullet (fallback for overflow)
        if (pool.Count == 0)
        {
            GameObject newBullet = Instantiate(bulletPrefab);
            newBullet.SetActive(false);
            return newBullet;
        }

        // Retrieve a bullet from the pool
        GameObject bullet = pool.Dequeue();
        bullet.SetActive(true);
        return bullet;
    }

    public void ReturnBullet(GameObject bullet)
    {
        // Reset bullet properties and return it to the pool
        bullet.SetActive(false);
        pool.Enqueue(bullet);
    }
}
```

---

#### **2. Connect the Object Pool to the Shooting Mechanic**
Update the `Shoot` script to use the object pool.

1. Assign the `BulletPool` in the scene to the `bulletPool` variable.
2. Set the `useObjectPooling` toggle to enable or disable pooling for comparison.

Here’s the updated `Shoot` script:

```csharp
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform bulletSpawnPoint; // Reference to the bullet spawn point

    public float bulletSpeed = 20f; // Speed of the bullet
    public float shootCooldown = 0.1f; // Cooldown in seconds between shots

    private float lastShootTime = -100f; // Initialize to a low value
    private BulletPool bulletPool; // Reference to the bullet pool manager

    public bool useObjectPooling = true; // Toggle for object pooling

    void Start()
    {
        // If no bullet spawn point is assigned, create a new one
        if (bulletSpawnPoint == null)
        {
            bulletSpawnPoint = new GameObject().transform;
            bulletSpawnPoint.name = "Bullet Spawn Point";
            bulletSpawnPoint.parent = transform; // Set it as a child of the player
            bulletSpawnPoint.position = transform.position + transform.forward + new Vector3(0, 0.2f, 0); // Slightly in front of the player
        }

        // Find the BulletPool component
        bulletPool = FindObjectOfType<BulletPool>();
    }

    void Update()
    {
        // Check for spacebar input and shoot if cooldown has elapsed
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > lastShootTime + shootCooldown)
        {
            Fire();
            FireEffects();
        }
    }

    void Fire()
    {
        GameObject bullet;

        if (useObjectPooling && bulletPool != null)
        {
            // Use object pooling
            bullet = bulletPool.GetBullet();
            bullet.transform.position = bulletSpawnPoint.position;
            bullet.transform.rotation = bulletSpawnPoint.rotation;
        }
        else
        {
            // Non-object pooling fallback
            bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        }

        // Set bullet velocity
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.velocity = transform.forward * bulletSpeed;
        }

        // Update the last shoot time to enforce cooldown
        lastShootTime = Time.time;
    }

    private void FireEffects()
    {
        // TODO - Add a muzzle flash effect when shooting
        FeedbackEventManager.ShakeCamera(5f, 1f, 0.25f);
    }
}
```

---

#### **3. Update the Bullet Logic**
Update the `Bullet` script to return bullets to the pool when they collide or after a short delay.

Here’s the updated `Bullet` script:

```csharp
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;

    private Rigidbody rb;
    private BulletPool bulletPool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        // Find the BulletPool manager
        bulletPool = FindObjectOfType<BulletPool>();
        
        // Enable the bullet's collider and reset gravity
        GetComponent<Collider>().enabled = true;
        rb.useGravity = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        rb.velocity = Vector3.zero;
        rb.useGravity = true;

        // Disable the bullet's collider
        GetComponent<Collider>().enabled = false;

        // Check if the bullet hit something that has the 'IDamagable' interface
        if (collision.gameObject.GetComponent<IDamagable>() != null)
        {
            IDamagable damageable = collision.gameObject.GetComponent<IDamagable>();
            damageable.TakeDamage(damage);
            damageable.ShowHitEffect();

            // TODO - Add audio feedback when hitting an object
            AudioEvent.PlaySFX("Slap Heavy", 1.0f, true); // with random pitch
        }


        // Return the bullet to the pool or destroy it if pooling is not enabled
        StartCoroutine(WaitAndDestroy(0.5f));
    }

    private IEnumerator WaitAndDestroy(float time)
    {
        yield return new WaitForSeconds(time);
        if (bulletPool != null)
        {
            bulletPool.ReturnBullet(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
```

---

### **Testing Instructions**
1. **Set Up the Scene**:
   - Assign the `PF_Bullet_Pooled` from the Prefabs folder to the pool and set a `poolSize` to 30.

2. **Test Object Pooling**:
   - Set `useObjectPooling` to `true` and test the shooting mechanic.
   - Observe that bullets are reused instead of being destroyed.

3. **Compare Performance**:
   - Toggle `useObjectPooling` to `false` and compare the performance impact of instantiating/destroying bullets.

---

### **Key Benefits of Object Pooling**
- **Reduced CPU and Memory Usage**: Reusing bullets avoids the cost of frequent `Instantiate` and `Destroy` operations.
- **Improved Performance**: Particularly noticeable when firing bullets at a high rate.
- **Maintainability**: Easily adaptable for other reusable objects like enemies, projectiles, or particles.