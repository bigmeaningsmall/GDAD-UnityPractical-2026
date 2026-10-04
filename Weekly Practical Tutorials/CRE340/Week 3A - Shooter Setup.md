---
Order: 3-A
---
# **Week 3 - Part 1 : Shooter Setup**

### Overview

#### **What we are building**:
- We'll give the Player the ability to **shoot**. Up to now the player moved and rotated but we haven't added the shoot
- We make a `Shoot` script on the Player, a `Bullet` prefab, and use a helper script `ObjectDestruction` to clean bullets up.
- just a working shooter we can build the rest of the weeks on.

#### **Why we are doing this**:
- This week is about **code communication** - how one object tells another object something happened. A bullet hitting a crate is a clean, simple example of that, so we need bullets first.
- Parts 2, 3 and 4 all build on what we set up here. This part is the basic game setup.

#### **Learning Objectives**:
- **Instantiating prefabs at runtime** from a spawn point.
- **Rigidbody velocity** to move an object in a direction.
- **Cooldowns** using `Time.time` (the same pattern as the Week 1 dash).
- **Helper components** - a reusable script that does one job on any object.

---
---

# **Part 1: Getting Shooting Working**

Carry on from Week 2. You should have a **Player** prefab with the `TwinStickController`, `PlayerStats` and an `InputManager` in the scene.

---

### **Step 1: Scene Setup**

- Import the package from Blackboard with the new scene - _Scene-Week3_Code-Communication_
- Open the scene and press Play to check you can move and aim before adding anything. 

> [!note] **Note - Remember the InputManager reference is set per-scene.**
> 
> If the Player doesn't move, drag the scene's **InputManager** object onto the **Input Manager** field on the Twin Stick Controller. - You can attach this to the player prefab.

---

> [!note] **Note TODO - I'll put `TODO`s in the code for things I want to add later or reminders where to come back to**
> 

### **Step 2: Create the `Bullet` Script**

The bullet is simple for now. It gets fired, it flies, it hits something and it stops.

1. **Create the Bullet Script**:
    - In _Assets / _CRE340 / Game Tutorial / Scripts_, create a C# script called `Bullet.cs`.

```csharp
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
```

**Explanation**:

- The bullet doesn't move itself. The `Shoot` script gives it a velocity when it's fired using the physics component `RigidBody`
- `rb.linearVelocity` is the current Unity name for what used to be `rb.velocity`. If you're following an older tutorial online and see `velocity`, this is the same thing renamed.
- The `TODO` is the reminder for Part 2 - we'll add an `Interface` here later.

---

### **Step 3: Add the `ObjectDestruction` Helper**

Every bullet we fire is a new GameObject in the scene. Without cleanup we'd have hundreds of bullets lying around.

This is a general-purpose helper script I use across projects - it destroys an object under a few different conditions, and you tick which ones you want in the Inspector. You can improve it but its just a general purpose script to destroy objects.

1. **Create the ObjectDestruction Script**:
    - Create a C# script called `ObjectDestruction.cs`.

```csharp
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
```

**Explanation**:

- Four independent conditions, each with a bool to turn it on or off. The object dies when whichever one you've enabled fires first.
- This is **composition** again - the bullet doesn't know how to destroy itself, we add a component on that handles it. Drop this on anything: debris, effects, spawned pickups.
- It's written to be reusable, not specific to the bullet. That's the point of a helper. 

> [!note] **Note - This is a utility script, not a part of a learning tutorial.**
> 
> Look over it it and use it. It's here so the scene stays clean. Its not very efficient but has some reusable ideas. 
> 
> There's an important concept though - _why is this a separate component instead of code inside `Bullet`?_ Because then only bullets could use it.

---

### **Step 4: Build the Bullet Prefab**

1. **Create the object**:
    - `GameObject -> 3D Object -> Sphere`. Rename it **Bullet**.
    - Scale it down small (e.g. 0.25, 0.25, 0.25).
2. **Add the components**:
    - `Add Component -> Rigidbody`. **Untick Use Gravity** (the `Bullet` script turns it on at impact).
    - `Add Component -> Bullet`.
    - `Add Component -> Object Destruction`.
3. **Set up Object Destruction** on the bullet :
    - **Destroy After Time** ticked, **Destruction Time** 5.
    - **Destroy On Collision** ticked, **Collision Destroy Threshold** set to **1**.
    - **Destroy Off Screen** - your choice. Handy for a fixed camera, annoying if your camera moves.
4. **Make the prefab**:
    - Drag it into _Assets / _CRE340 / Game Tutorial / Prefabs_, then delete it from the Hierarchy.

> [!note] **Note - Threshold of 1 means the bullet dies on its first hit.**
> 
> Try setting it to 2 or 3 - now your bullets bounce off one object and carry on if you don't enable gravity. A ricochet weapon, with no extra work from one Inspector value. Use personal preference for destroying or bullet behaviour

---

### **Step 5: Create the `Shoot` Script**

This goes on the Player and fires bullets in whatever direction the Player is facing. With the Week 1 twin stick controller, should be whatever way the right stick is pointer. 
- *Everyone tends to use mouse and keyboard - I'd recommend plug in a game pad - Automatically mapped with the input manager and more ergonomic*

1. **Create the Shoot Script**:
    - Create a C# script called `Shoot.cs`.

```csharp
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
        // Read the fire button from the InputManager
        // Held() keeps firing while the trigger is down - the cooldown controls the fire rate
        if (inputManager.RightTriggerPressed.Held() && Time.time > lastShootTime + shootCooldown)
        {
            Fire();
        }
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
```

1. **Attach and wire it up**:
    - Select the **Player** and `Add Component -> Shoot`.
    - Drag the scene's **InputManager** onto the **Input Manager** field.
    - Drag the **Bullet** prefab onto the **Bullet Prefab** field.
    - Leave **Bullet Spawn Point** empty - the script makes one. (Assign your own child transform later if you want the barrel somewhere specific.)
        - This needs some fiddling to get right - might collide depending on your player setup.
    - **Overrides -> Apply All** to update the Player prefab.

**Explanation**:

- **The cooldown is the same pattern as the Week 1 dash** - store the time we last did a thing, prevent it from happening again until enough seconds have passed. You'll use this constantly.
- `transform.forward` is the Player's facing direction, and the Twin Stick Controller already points that at the right stick. 
- `Instantiate` creates a copy of the prefab in the scene. We give it a position and rotation, then push it with a velocity.

> [!note] **Note - Check the button name against your InputManager.**
> 
> *I've used `RightTriggerPressed.Held()` for hold-to-fire. You can use a different one or look at the input actions asset to see the mappings - `ButtonWest.Held()` or `ButtonEast.Held()` work fine too.*
> 
> *Swap `Held()` for `Pressed()` if you'd rather have single-shot, one bullet per pull.*
> 
> BMS-InputManager lives here - Its my extension to not have to deal with Inputs every time. Its a event wrapper for the input manager. Take it and use it or have a look at the input manager. Its really good but hard to get started. This is easy to use but hides the input manager.
> https://github.com/bigmeaningsmall/BMS-InputManager

---

### **Step 6: Add Something to Shoot At**

We need targets. They are not very interesting and don't do anything yet - Part 2 gives them behaviour.

1. **Make a Crate**:
    - `GameObject -> 3D Object -> Cube`. Rename it **Crate**. Give it a material so it stands out.
    - It already has a Box Collider. **Leave Is Trigger unticked** - we want a solid collision this time, not a trigger.
    - Drag it into the **Prefabs** folder.
2. **Duplicate it** for an **ExplodingCrate** and an **Enemy** (a Capsule works well for the enemy). Different colours.
3. **Scatter a few of each** around the arena.

> [!note] **Note - Triggers vs Collisions.**
> 
> Week 2's items used `OnTriggerEnter` - the player passes _through_ them. Bullets use `OnCollisionEnter` - they physically _hit_ things. The difference is that one tickbox on the collider. Worth knowing which you need and when. 
> - *Collisions are solid*
> - *Triggers are passthrough*

---

### **Step 7: Test It**

1. **Press Play**:
    - Aim with the right stick, hold the fire button. Bullets should fly out in front of the Player.
    - Bullets that hit something stop, drop and are destroyed.
    - Bullets that hit nothing are destroyed after 5 seconds.
2. **Tune it**:
    - `bulletSpeed` and `shootCooldown` on the Player, `destructionTime` on the bullet prefab.
    - A low cooldown and high speed feels like a machine gun. A long cooldown and slow bullets feels completely different. Try both and tune the game feel even at the early stage
        - We may make this into a mini magic system, projectile thing in later weeks.

---

### **What we've set up**:

- A **Bullet** prefab that moves by physics and cleans itself up.
- A **Shoot** component on the Player, firing in the aim direction with a cooldown.
- A reusable **ObjectDestruction** helper.
- Some targets to shoot at.

### **What's missing - the useful part**:

Right now the bullet hits a crate and **nothing happens to the crate**. The bullet knows it hit _something_, but it has no idea what, and no way to tell it.

We could write `if (collision.gameObject.GetComponent<Crate>() != null) ...` then another `if` for the exploding crate, another for the enemy, another for every damageable thing we ever add. That chain grows forever and `Bullet.cs` has to be edited every single time.

**Part 2** addresses that with an **interface**. 