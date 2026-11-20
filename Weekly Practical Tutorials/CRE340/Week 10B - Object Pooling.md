---
Order: 10-B
---
# **Week 10 : Object Pooling**

### Overview

#### **What we are building**:

- A **`BulletPool`** that makes a set of bullets once and hands them out over and over.
- A **`Bullet`** that switches itself off and goes back to the pool instead of being destroyed.
- A toggle on `Shoot` so you can turn pooling off and compare.

#### **Why we are doing this**:

- Holding the fire button creates and destroys a bullet every fifth of a second, forever. That's the most common performance problem in a game of this kind.
- `Instantiate` and `Destroy` are two of the more expensive things you can do, and the rubbish they leave behind has to be cleared up by the garbage collector, which is what causes those occasional frame hitches.
- It's the last pattern of the module and the one most directly about performance.

#### **Learning Objectives**:

- **The Object Pool pattern** - reuse instead of create.
- **Resetting state** - the part people get wrong.
- **`Queue<T>`** - a list you take from one end of.

---
---

# **Reusing Bullets**

### **Step 1: What Happens When You Shoot**

Press Play, hold the fire button and watch the **Hierarchy**.

Bullets appear, fly off, and disappear. With a 0.2 second cooldown that's five objects created and five destroyed every second, and every one of them involves Unity allocating memory, running `Awake`, adding components to the scene, then tearing all of that back down.

Creating one object is cheap. Creating five a second constantly is not, and the memory left behind by all the destroyed ones has to be collected at some point. When the garbage collector runs you get a frame that takes much longer than the others, which is what a stutter tends to be.

**The idea behind pooling:** make thirty or so bullets at the start, switch them off, and keep them in a pile. When you shoot, take one from the pile, move it into place and switch it on. When it's finished, switch it off and put it back. Nothing is ever created or destroyed while the game is running.

> [!note] **Note - this is why the Factory exists**
> 
> Back in Week 7 we put enemy creation behind `EnemyFactory` and the note said the reason would show up in Week 10. This is a key reason.
> 
> A pooled object isn't created, it's fetched and reset - a completely different way of producing something. If `Shoot` had `Instantiate` written into it in five different places, this week would mean finding all five. With creation behind one method, it's one change.
> 
> We're only pooling bullets, but the same reasoning applies to the enemies if you were to try it

---

### **Step 2: The Bullet Looks After Its Own Ending**

Right now the bullet doesn't decide when it goes away - `ObjectDestruction` does, and it does it by calling `Destroy`. A pooled bullet must never be destroyed, so that has to change. This was a temporary feature tjhat we'll change.

**1. Open the `Bullet` prefab and remove the `Object Destruction` component.** The bullet takes over the job itself.

**2. Update `Bullet.cs`.** Everything you had stays - the additions are marked:

```csharp
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1; // how much damage this bullet deals - used in Part 2

    // --- NEW ---
    public float lifeTime = 5f; // seconds before it gives up and goes away

    private Rigidbody rb;

    // --- NEW ---
    private BulletPool pool;    // null if we were instantiated rather than pooled
    private float timeAlive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // cache the rigidbody
    }

    // --- NEW - the pool calls this when it hands us out, so we know where to go back to ---
    public void SetPool(BulletPool owner)
    {
        pool = owner;
    }

    // --- NEW ---
    // OnEnable runs every time the object is switched on, so this is where we reset.
    // A pooled bullet arrives in whatever state the last shot left it in.
    private void OnEnable()
    {
        timeAlive = 0f;

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // --- NEW - Shoot calls this once the bullet is in position ---
    public void Launch(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    // --- NEW - this replaces what ObjectDestruction was doing for us ---
    private void Update()
    {
        timeAlive = timeAlive + Time.deltaTime;

        if (timeAlive >= lifeTime)
        {
            Finish();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Stop the bullet dead and let it drop - a cheap 'impact' effect
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = true;
        
        // Ask the thing we hit: do you implement IDamagable?
        // TryGetComponent gives us a bool AND the component in one call
        if (collision.gameObject.TryGetComponent<IDamagable>(out IDamagable damageable))
        {
            // We have NO IDEA what we just hit. Crate? Enemy? Something we'll add in the future?
            // Doesn't matter - it promised it has these members, so we can use them.
            damageable.TakeDamage(damage);
            damageable.ShowHitEffect();

            // The Health property lets us ASK the target a question without knowing what it is 
            Debug.Log("Hit something - " + damageable.Health + " health remaining");
            
            //todo sound - we can call the sound here so its the same for all or... make sound dependant on object hit?
            //todo sound - inside the IDamagable check
            AudioEvent.PlaySFX(SoundId.Slap_Heavy, transform.position);
        }

        // --- NEW - go back to the pool instead of waiting to be destroyed ---
        Finish();
    }

    // --- NEW - back to the pool if we came from one, destroyed if we didn't ---
    private void Finish()
    {
        if (pool != null)
        {
            pool.Return(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
```

**Explanation**:

- **`OnEnable` is the reset, and it's the whole point of the week.** `Instantiate` gives you a brand new object with everything at its default. A pool gives you a **used** object, still carrying whatever the last shot did to it.
- **Look at your own collision code for the proof.** `rb.useGravity = true;` is the last thing that happens to a bullet before it goes away. Without the reset, the next bullet out of the pool comes with gravity switched on and drops out of the air immediately. The reset in `OnEnable` is undoing exactly that line.
- **`Finish` handles both cases.** A bullet that came from the pool goes back to it, and one that was instantiated gets destroyed. The bullet doesn't care which it is, it just checks whether it has a pool.
- **`Launch` means `Shoot` doesn't touch the Rigidbody.** It also means both paths through `Fire` end with the same line.
- **The lifetime is a float in `Update`** because `ObjectDestruction` is gone. If you liked the other options on that script - off screen, idle - you could bring them across, but any `Destroy` in them would need replacing with a return.
- The damage check, the Debug.Log and the hit sound are untouched. Pooling changes when the bullet goes away, not what it does.

> [!warning] **If you forget the reset**
> 
> Comment out the three `rb` lines in `OnEnable`, press Play and shoot a crate, then keep shooting.
> 
> The next bullet arrives with gravity on and falls straight down. That's the bug people hit the first time they write a pool, and it's always the same cause - something wasn't put back the way it was found.

---

### **Step 3: The Pool**

**Create a C# script called `BulletPool.cs`** and put it on an empty GameObject called **Bullet Pool**.

```csharp
using System.Collections.Generic;
using UnityEngine;

// Makes a set of bullets once and hands them out over and over.
public class BulletPool : MonoBehaviour
{
    public Bullet bulletPrefab;      // the Bullet prefab - note the type, not GameObject
    public int startingSize = 30;

    // A Queue is a list you add to one end of and take from the other.
    // Any switched-off bullet will do, so the order doesn't matter to us.
    private Queue<Bullet> available = new Queue<Bullet>();

    private void Awake()
    {
        // Make them all now, so nothing has to be created once the game is running
        for (int i = 0; i < startingSize; i++)
        {
            Bullet bullet = CreateBullet();

            available.Enqueue(bullet);
        }
    }

    // Makes one bullet, switched off and parented to this object
    private Bullet CreateBullet()
    {
        Bullet bullet = Instantiate(bulletPrefab, transform);

        bullet.gameObject.SetActive(false);

        return bullet;
    }

    public Bullet Get(Vector3 position, Quaternion rotation)
    {
        Bullet bullet;

        if (available.Count > 0)
        {
            bullet = available.Dequeue();
        }
        else
        {
            // The pool ran dry. Make one more - it joins the pool when it comes back.
            bullet = CreateBullet();

            Debug.Log("Bullet pool ran out, made an extra one.");
        }

        // Move it into place BEFORE switching it on, so it doesn't appear
        // for a frame wherever the last bullet finished.
        bullet.transform.position = position;
        bullet.transform.rotation = rotation;

        bullet.SetPool(this);
        bullet.gameObject.SetActive(true);

        return bullet;
    }

    public void Return(Bullet bullet)
    {
        // Already back in the pool, so ignore it. Without this check the same
        // bullet could end up in the queue twice and get handed out to two shots.
        if (!bullet.gameObject.activeSelf)
        {
            return;
        }

        bullet.gameObject.SetActive(false);

        available.Enqueue(bullet);
    }

    // Handy for debugging - how many are sitting waiting
    public int AvailableCount()
    {
        return available.Count;
    }
}
```

**Explanation**:

- **`Queue<Bullet>`** is first in, first out. `Enqueue` puts one on the back, `Dequeue` takes one off the front. A `List` would work too - we're using a Queue because taking from the front is exactly what we want and there's no searching involved.
- **The prefab field is typed `Bullet`, not `GameObject`.** `Instantiate` then hands us back a `Bullet` directly and we skip the `GetComponent`. Worth doing whenever you know what you're making.
- **Position before `SetActive`.** `SetActive(true)` runs `OnEnable` and the object becomes visible that frame, so moving it afterwards gives you a one-frame flash of the bullet at the last one's position.
- **Running dry is handled rather than ignored.** Thirty bullets at a 0.2 second cooldown and a 5 second lifetime is plenty, but a shotgun or a faster weapon would empty it. Making one more is better than dropping the shot, and the pool quietly grows to whatever the game actually needs.
- **The double-return check matters.** If a bullet's lifetime ran out on the same frame it hit something, `Finish` would be called twice and the same bullet would sit in the queue in two places. Then two shots get the same bullet and one of them vanishes. Four lines to prevent a bug that's very hard to find.
- Parenting the bullets to the pool keeps the Hierarchy tidy and gives you something to look at while it runs.

---

### **Step 4: Shoot Asks the Pool**

**1. Add these fields to `Shoot.cs`:**

```csharp
    [Header("Object Pooling")]
    public bool useObjectPooling = true;

    private BulletPool bulletPool;
```

**2. Add the lookup to `Start`**, keeping the spawn point code already there:

```csharp
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

        // --- NEW ---
        // Find the pool once at the start. We can't drag it in because the Player
        // is a prefab the GameManager instantiates, and a prefab can't hold a
        // reference to something sitting in the scene.
        bulletPool = FindFirstObjectByType<BulletPool>();
    }
```

**3. Update `Fire`.** Only the top of it changes - everything below the cooldown line stays as it is:

```csharp
    void Fire()
    {
        // Fire it in the direction the player is facing
        Vector3 bulletDirection = transform.forward;

        // --- NEW - where the bullet comes from is the only thing that changed ---
        Bullet bullet;

        if (useObjectPooling && bulletPool != null)
        {
            // Ask the pool for one that already exists
            bullet = bulletPool.Get(bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        }
        else
        {
            // The old way, kept so we can compare the two
            GameObject newObject = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

            bullet = newObject.GetComponent<Bullet>();
        }

        // Same line either way - the bullet doesn't know where it came from
        bullet.Launch(bulletDirection * bulletSpeed);

        // Update the last shoot time to enforce the cooldown
        lastShootTime = Time.time;

        // --- announce that a shot was fired ---
        WeaponFired message = new WeaponFired();
        message.position = bulletSpawnPoint.position;
        message.direction = bulletDirection;

        EventBus<WeaponFired>.Publish(message);
        
        Debug.Log("WeaponFired published at " + message.position);
        
        //TODO - We'll make the sound event here - could use the existing event system but we'll use an audiomanager event setup
        //AudioEvent.PlaySFX(SoundId.Pistol_Shot);
        AudioEvent.PlaySFX(SoundId.Sci_Fi_Shotgun, 0.8f);
        //or
        //AudioEvent.PlaySFX(SoundId.SoundDef_GunShots);
    }
```

**Explanation**:

- **`bulletDirection` moves to the top** of the method, because it's now needed in two places - the launch and the `WeaponFired` message. Same line, just earlier.
- **The `bulletRb` block is gone.** `bullet.Launch(...)` does that job, and it means the pooled path and the instantiated path don't each need their own copy of it.
- **The two branches only differ in where the bullet comes from.** Everything after is identical, which is a sign the split is in the right place.
- **`WeaponFired`, the Debug.Log and the shot sound are all untouched.** Anything subscribed to `WeaponFired` carries on with no idea the bullets are being recycled, which is the point of having published it rather than called into things directly.
- **`FindFirstObjectByType` is the same problem we hit in Week 5.** The InputManager was solved by making it a child of the Player, but the pool can't be - it would be destroyed every time the player died and you'd lose all thirty bullets. So the player looks it up instead.
- It's a one-off lookup in `Start`, not something happening per frame. `FindFirstObjectByType` is slow and that's fine once per spawn.
- **Keep the `bulletPrefab` field.** The non-pooled path still needs it, and so does the pool.

---

### **Step 5: Set It Up**

1. **Open the `Bullet` prefab** and remove the **Object Destruction** component if you haven't already.
2. **Select the Bullet Pool** object and drag the **Bullet** prefab onto **Bullet Prefab**. Set **Starting Size** to 30 or higher.
3. **Open the Player prefab**, select **Shoot**, and check **Use Object Pooling** is ticked.
4. **Press Play.**

The Bullet Pool object should have thirty switched-off bullets under it in the Hierarchy before you fire a shot.

---

### **Step 6: Test and Compare**

1. **Hold the fire button and watch the Hierarchy.** Bullets under the Bullet Pool switch on and off. Nothing is being created or destroyed, and the count stays at thirty.
2. **Untick Use Object Pooling while playing** and hold fire again. Bullets appear and disappear in the Hierarchy as separate objects.
3. **Watch the performance monitor** while you toggle it. At this fire rate the difference will be small - a twin stick with one gun is not a hard problem.
4. **Make it a hard problem.** Drop `shootCooldown` to 0.02 and hold fire with pooling off, then on. That's fifty bullets a second, and the difference is much easier to see.
5. **Check the pool growing.** With the cooldown at 0.02 the pool will run dry and you'll see the Console message. Watch the child count under Bullet Pool climb and then settle.
6. **Check the sounds still fire properly.** Fifty shots a second will find the limits of the audio before it finds the limits of the pool.

> [!note] **Note - when is this worth doing?**
> 
> At five bullets a second, its not needed. The game runs fine.
> 
> It's worth it when something happens often - projectiles, particles, impact effects, damage numbers - and the cost is that there's now a rule about resetting state that whoever writes the next bullet feature has to know about. A pool where something forgets to reset is harder to debug than no pool at all.
> 
> The useful habit is the measuring, not the pooling. Find out which thing is actually slow before you optimise it - that's what the performance monitor is for. Guessing where the time goes is wrong more often than it's right.

---

### **What Else Would You Pool**

Worth thinking about for your own projects when you have a lot of instancing. The test is **does this happen a lot, and does it look the same every time**.

- **Good candidates** - projectiles, muzzle flashes, impact effects, shell casings, floating damage text, enemies in a wave shooter.
- **Not worth it** - the player, a boss, UI panels, anything there's one of.
- **Needs care** - anything with a particle system or a trail renderer, because those keep their own internal state and need clearing on reuse. Particle systems have a `Clear()` for this.

Unity also has a built-in `ObjectPool<T>` in `UnityEngine.Pool` which does what we've written here, with callbacks for the get and release steps. Its better to use the Unity Object Pool class but this example is the manual way its done.

---

### **What we've set up**:

- A **`BulletPool`** that makes thirty bullets once and reuses them.
- A **`Bullet`** that resets itself in `OnEnable` and returns itself when it's finished.
- A **toggle** so the two approaches can be compared.
- Nothing being created or destroyed while the game runs.

### We may be finished with tutorials on this project! Its a small game but shows all the different ways to approach code and structure.