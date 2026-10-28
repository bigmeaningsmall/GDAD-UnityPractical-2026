---
Order: 6-D
---


# **CRE343 Week 6 - Part 2 : Camera Reactions**

### Overview

#### **What we are building**:

- A **camera kick** when the player fires, and a bigger **shake** when an enemy dies.
- Camera **noise that builds as health drops**, so being nearly dead feels different.
- All of it driven by the event system from CRE340, with no changes to gameplay code.

#### **Why we are doing this**:

- The camera is the single biggest feel lever you have. Shake, kick and framing do more than particles.
- It's the clearest example of the thing this week is about - an effect reacting to something that happened elsewhere, with neither side knowing about the other.
- Shooting currently has no feedback at all. It's the action players perform most, so it's where the gain is biggest.

#### **Learning Objectives**:

- **Subscribing to gameplay events** to drive an effect.
- **Impulse vs noise** - which Cinemachine tool fits which job.
- **Using data from a message** - position for falloff, health for intensity.
- **Restraint** - why the first version is always too much.

---

---

# **Part 2: Making the Camera React**

Carry on in the same project. Part 1's spawn pops should be working.

---

### **Step 1: What's Already Wired Up**

In CRE340 you built an event system. We're going to use it rather than build more of it, so here's what's available.

If you don't have this or your weeks are not synced up - go to Blackboard and get my package for the project - You might have to delete the folder 'CRE340-GameTutorial' and bring in the whole week. Ask me if you're stuck.

**From the `EventBus`:**

```csharp
public struct EnemyKilled
{
    public string enemyName;
    public int xpValue;
    public Vector3 position;
}
```

You built this one. `Enemy.Die()` publishes it and `EnemyDeathEffects` already subscribes.

**New - I've added this one for you:** - if not then make a script called `GameMessages.cs` and add both structs

```csharp
public struct WeaponFired
{
    public Vector3 position;
    public Vector3 direction;
}
```

It's published from `Shoot.Fire()`:

- Go to the `Shoot.cs` class and add this NEW part at the end of the `Fire()` function

```csharp
void Fire()  
{  
    GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);  
  
    Vector3 bulletDirection = transform.forward;  
  
    Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();  
  
    if (bulletRb != null)  
    {        
	    bulletRb.linearVelocity = bulletDirection * bulletSpeed;  
    }  

    lastShootTime = Time.time;  
  
    // --- NEW - announce that a shot was fired ---  
    WeaponFired message = new WeaponFired();  
    message.position = bulletSpawnPoint.position;  
    message.direction = bulletDirection;  
  
    EventBus<WeaponFired>.Publish(message);  
}
```

That's the entire change to the gameplay code for this whole session. Four lines and a struct. This part below is the events we already have in our game.

**From `UIEventManager`:**

```csharp
OnHealthChanged(int current, int max)
OnManaChanged(int current, int max)
OnCoinsChanged(int value)
OnXPChanged(int value)
OnLivesChanged(int value)
OnGameOver()
```

**From `HealthEventManager`:**

```csharp
OnObjectDamaged(string name, int currentHealth)
OnObjectDestroyed(string name, int currentHealth)
```

*This part if probably confusing but don't worry about it. I'm basically saying we already have all the events we need to connect up and drive the visual effects. Everything else just subscribes and reacts with effects* 

> [!note] **Note - look at what each message carries**
> 
> This decides what you can do with it, and it's worth checking before you plan an effect.
> 
> `EnemyKilled` carries a **position**, so you can put something in the world where it happened. `WeaponFired` carries a position **and a direction**, so a muzzle flash can point the right way.
> 
> `OnObjectDamaged` carries a name and a number but **no position**. So you can flash the screen when a crate is hit, but you can't put a spark on the crate - there's nothing in the message telling you where it is.
> 
> When one of your brainstorm ideas can't be built, this is usually why. The fix is to add a field to a message, or add a new message - which is a five minute job, and exactly what `WeaponFired` change is.



---

### **Step 2: Check Your Cinemachine Version**

The provided scene has a Cinemachine camera following the player.
- I added a Cinemachine Follow camera to the scene and a script attached to it to get the player from the `GameManager` and use them as the tracked target for the camera.
	- You just need to enable the `CameraFollowPlayer` script that is attached to the Cinemachine camera
- Because we are instantiating the player then we need to get them as the camera target each time they are instantiated.

The code here is written for **Cinemachine 3.x**. If you're on 2.x, change the `using` line and the property names and everything else works the same.

> [!warning] **Note - this is the number one time sink with Cinemachine**
> 
> Almost every tutorial and forum answer online is written for 2.x, and the renames mean the code won't compile against 3.x 
> 
> If you're following something from YouTube and it won't build, check this first.

---

### **Step 3: Impulse or Noise?**

Cinemachine gives you two ways to move the camera about, and they're for different things.

**Impulse** - a one-off hit. You call `GenerateImpulse()` and the camera jolts and recovers. Several impulses blend together, and it can fall off with distance so a far away explosion shakes less. Use it for **events** - firing, deaths, impacts.

**Noise** - continuous movement, controlled by an amplitude value. Set it and the camera keeps moving until you change it. Use it for **states** - low health, sprinting, being near something dangerous.

We'll use both, for the jobs they suit.

---

### **Step 4: Camera Kick on Firing**

1. **Check the setup** - the Cinemachine camera should have a **Cinemachine Impulse Listener** component 
	1. you can do this by going to AddExtension on the CineMachine Virtual Camera
2. There should be an empty GameObject called **Impulse Source** with a **Cinemachine Impulse Source** on it. 
	1. I have it deactivated as a child of the cinemachine camera in the scene - I'd encourage you to make your own so you do the steps to add it.
	2. You need to create this too and add the component `Cinemachine Impulse Source`
*Cinemachine is a lot of steps to do things but its great once you get it setup - just a pain to deal with because it can do every conceivable camera and cinematic effect*
3. **Create a script called `CameraShake.cs`** and attach it to the Impulse Source object:

```csharp
using UnityEngine;
using Unity.Cinemachine;   // Cinemachine 3.x - use 'using Cinemachine;' on 2.x

// Listens for gameplay events and shakes the camera.
// Nothing in the game knows this exists.
public class CameraShake : MonoBehaviour
{
    [Header("Impulse")]
    public CinemachineImpulseSource impulseSource;

    [Header("Strength")]
    public float fireShake = 0.1f;
    public float killShake = 0.5f;

    private void Awake()
    {
        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }
    }

    private void OnEnable()
    {
        EventBus<WeaponFired>.Subscribe(HandleWeaponFired);
        EventBus<EnemyKilled>.Subscribe(HandleEnemyKilled);
    }

    private void OnDisable()
    {
        EventBus<WeaponFired>.Unsubscribe(HandleWeaponFired);
        EventBus<EnemyKilled>.Unsubscribe(HandleEnemyKilled);
    }

    private void HandleWeaponFired(WeaponFired message)
    {
        Shake(message.position, fireShake);
    }

    private void HandleEnemyKilled(EnemyKilled message)
    {
        Shake(message.position, killShake);
    }

    private void Shake(Vector3 position, float strength)
    {
        if (impulseSource == null)
        {
            return;
        }

        // Move the source to where it happened, so things further away shake less
        impulseSource.transform.position = position;

        impulseSource.GenerateImpulse(strength);
    }
}
```

1. **Press Play** and hold fire.

**Explanation**:

- Subscribe in `OnEnable`, unsubscribe in `OnDisable` - same as every listener you wrote in CRE340.
- One class handles both events because they want the same kind of response at different strengths.
- Moving the impulse source to `message.position` before firing is what gives you falloff. Kill an enemy across the arena and it shakes less than one in your face. That's the position field earning its place.

> [!note] **Note - it's almost certainly too much**
> 
> Hold fire for five seconds. The gun fires five times a second, so that's five impulses a second stacking on top of each other and the screen is probably unplayable.
> 
> **Turn `fireShake` down until you can barely tell it's there**, then play for a minute, then turn it off entirely and play again. The difference between "barely there" and "off" is bigger than you'd expect, and the difference between "barely there" and what you started with is the whole lesson.
> 
> Shake for a repeated action has to be tiny. Shake for a rare action can be big. Getting those the wrong way round is the most common mistake in this kind of work.

---

### **Step 5: Shake in the Right Direction**

A kick that goes the same way every time reads as a rattle. Firing should push the camera **back along the shot**.

`WeaponFired` carries a direction, so we can use that:

```csharp
    private void HandleWeaponFired(WeaponFired message)
    {
        if (impulseSource == null)
        {
            return;
        }

        impulseSource.transform.position = message.position;

        // Push the camera back against the direction we fired
        Vector3 kick = -message.direction * fireShake;

        impulseSource.GenerateImpulse(kick);
    }
```

**Press Play** and fire in different directions. The kick now follows your aim.

**Explanation**:

- `GenerateImpulse` has an overload taking a `Vector3` velocity rather than a float, which is what lets you aim the shake.
- The minus sign is the recoil. The shot goes one way, the camera goes the other.
- This is the whole reason `direction` is in the message. Without it the best you could do is shake randomly.

---

### **Step 6: Low Health Camera Noise**

Now the other tool, for a state rather than an event.

1. **Check the Cinemachine camera** 
2. You'll see a **Noise** setting. Add a **Cinemachine Basic Multi Channel Perlin** component 
3. You'll have to make a noise profile for it and give it some amplitude and frequency values
	1. Right click in the project window then under Cinemachine create a '**Noise Settings**'
	2. Again it feels like a lot of setup but its basically a Scriptable Object for adding noise to the camera
		1. Once you repeat the workflows you see they very repeatable and work the same as you would your own profiles, data and behaviours.
4. Set its **Amplitude Gain** to 0 so it starts still.
5. **Create a script called `LowHealthCamera.cs`** and attach it to the Cinemachine camera:

```csharp
using UnityEngine;
using Unity.Cinemachine;
using PrimeTween;

// Builds camera noise as the player's health drops.
// This is a STATE, not an event - it stays on until health changes again.
public class LowHealthCamera : MonoBehaviour
{
    public CinemachineBasicMultiChannelPerlin noise;

    [Header("Tuning")]
    public float threshold = 0.3f;      // below this fraction we start shaking
    public float maxAmplitude = 0.5f;   // how strong it gets at zero health
    public float blendTime = 0.4f;      // how long it takes to reach the new level

    private void Awake()
    {
        if (noise == null)
        {
            noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    private void OnEnable()
    {
        UIEventManager.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        UIEventManager.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (noise == null || max <= 0)
        {
            return;
        }

        float fraction = (float)current / (float)max;
        float target = 0f;

        if (fraction < threshold)
        {
            // The further below the threshold we are, the stronger it gets
            float amount = 1f - (fraction / threshold);
            target = maxAmplitude * amount;
        }

        // Tween to the new level rather than snapping to it
        Tween.Custom(noise.AmplitudeGain, target, duration: blendTime,
            onValueChange: value => noise.AmplitudeGain = value);
    }
}
```

3. **Press Play** and let an enemy hit you a few times. The camera starts to breathe as you get low, and settles when you heal.

**Explanation**:

- **`Tween.Custom` animates any value you like.** Give it a start, an end, a duration and a line saying what to do with each step. It's the escape hatch for anything PrimeTween doesn't have a built-in tween for.
- The event carries `current` **and** `max`, which is why we can work out a fraction. If it only carried the current health we'd have no idea whether 20 was nearly dead or barely scratched.
- Healing runs the same code and tweens the amplitude back down. 

> [!note] **Note - the event says what changed, not what it means**
> 
> `OnHealthChanged` fires for damage and for healing. It hands you two numbers and no opinion about them.
> 
> Working out whether that's good or bad news is the listener's job, not the event's. That keeps `PlayerStats` simple and it means a dozen different listeners can each draw their own conclusion from the same message.

---

### **Step 7: Check What You Didn't Touch**

- `Shoot.cs` - four lines publishing a message, nothing else
- `Enemy.cs` - unchanged since CRE340
- `PlayerStats.cs` - unchanged since CRE340
- `GameManager.cs` - unchanged since CRE340

**None of them mention Cinemachine, PrimeTween, impulses or shake.** The entire camera feel system could be deleted and the game would run exactly as it did.

That's the event observer structure working make things more reactive. Programmers built something that announces what's happening, and the feel work can be done without changing gameplay logic. Both sides can be worked on at once without either breaking the other.

---

### **What we've set up**:

- A **camera kick** on firing, aimed using the direction from the message.
- A **shake** on enemy death, with distance falloff from the position.
- **Camera noise** that builds as health drops, tweened rather than snapped.
- No changes to the gameplay code beyond one publish call.

### **Next**:

In **Part 3** we go after the screen itself - post-processing on damage, a desaturation on game over, and some punch on the UI. 

This seems a little code heavy but the events you already have. We're really just coming up with effects to add on the end of our events. 
- We're trying to make things cause and effect reactive