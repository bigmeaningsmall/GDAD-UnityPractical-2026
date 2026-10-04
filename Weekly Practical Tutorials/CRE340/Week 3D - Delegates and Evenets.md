---
Order: 3-D
---
# **Week 3 - Part 4 : Delegates and Events - Direct Sender and Receiver**

### Overview

#### **What we are building**:

- The same event mechanism as Part 3, but stripped to the bare minimum - **one sender, one event, one receiver**, no game logic in the way.
- An **`EventSender`** that fires an event with **parameters**.
- An **`EventReceiver`** that subscribes and animates itself in response.

#### **Why this is separate**:
- Part 3 put events into the game, which is where they're useful but also where there's a lot going on.
- This is an **alternative arrangement**: the event lives on the sender class itself rather than in a central manager. Both are valid and you'll see both in projects.
- It also shows **passing parameters** with an event, which Part 3 only did with a single int.

> [!note] **Note - If Part 3 felt like hidden things going on, do this one first.**
> 
> There's nothing here but the mechanism. Two small scripts and a cube that squashes when you press a button. Once this makes sense, go back and re-read Part 3 - it'll look like the same thing with more going on around it.

#### **Learning Objectives**:

- **Events as class members**: an event owned by the sender rather than a manager.
- **Parameters**: passing data with an event.
- **Coroutines + Lerp**: using received values to drive an animation.
- **Comparing two arrangements**: central manager vs. event on the sender.

---
---

# **Part 4: The Minimal Event Example**

This can go in **Scene-Week3_Code-Communication** or a scratch scene - it's self-contained.

The idea: the sender raises an `OnFire` event carrying a **scale** and a **speed**. The receiver catches it and scales itself up and back down using those values.

> [!note] **Note - Scale and speed are just examples.**
> 
> An event can carry any combination of parameters as long as the receiver's method accepts the same ones. It could be a `Vector3` position, a `string` name, a GameObject reference, or nothing at all.

---

### **Step 1: Create the `EventSender`**

1. **Create a C# script called `EventSender.cs`.**

```csharp
using UnityEngine;

public class EventSender : MonoBehaviour
{
    // Define the delegate type for the OnFire event
    // This one takes TWO floats - the shape the receiver must match
    public delegate void FireEventHandler(float scale, float speed);

    // Define the event based on the delegate
    // 'static' so receivers can subscribe via the class name without a reference to this object
    public static event FireEventHandler OnFire; // i don;t usually write the comments - just have the delegate and event on two lines together

    [Header("Parameters to pass with the event")]
    public float scale = 2.0f;
    public float speed = 10.0f;

    private void Update()
    {
        // Using the old input system here for simplicity - this is a mechanism demo, not game code - might change later
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Only fire if something is actually subscribed
            if (OnFire != null)
            {
                OnFire(scale, speed); // fire the event, passing both values
            }
        }
    }
}
```

**Explanation**:

- The **delegate shape has changed** - two floats instead of Part 3's one int. The shape is entirely up to you; the receiver just has to match it.
- `if (OnFire != null)` does the same job as Part 3's `?.Invoke(...)`. Two ways of writing the same null check - you'll see both in other people's code.
- The event is `static`, so a receiver subscribes with `EventSender.OnFire += ...` and never needs a reference to the sender GameObject.

---

### **Step 2: Create the `EventReceiver`**

1. **Create a C# script called `EventReceiver.cs`.**

```csharp
using System.Collections;
using UnityEngine;

public class EventReceiver : MonoBehaviour
{
    private Vector3 originalScale;

    private void OnEnable()
    {
        // Subscribe to the OnFire event
        EventSender.OnFire += HandleFireEvent;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks - same discipline as Part 3
        EventSender.OnFire -= HandleFireEvent;
    }

    private void Start()
    {
        originalScale = transform.localScale;
    }

    // Matches the delegate shape exactly: void return, two floats - THIS IS WHAT THE EVENT CONNECTS TOO!
    private void HandleFireEvent(float scale, float speed)
    {
        // Use the values we received to drive the animation
        StartCoroutine(ScaleObject(scale, speed));
    }

//------------------ This below is just animation stuff - THE ABOVE IS THE EVENT - BELOW IS WHAT HAPPENS AFTER
    // Coroutine to scale the object up and back down using Lerp
    private IEnumerator ScaleObject(float targetScale, float speed)
    {
        Vector3 targetSize = originalScale * targetScale;
        float elapsedTime = 0f;
        float duration = 1f / speed; // higher speed = shorter duration

        // Scale up
        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetSize, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // wait one frame
        }

        transform.localScale = targetSize; // make sure we land exactly on target

        elapsedTime = 0f;

        // Scale back down
        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(targetSize, originalScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale; // reset exactly
    }
}
```

**Explanation**:

- `HandleFireEvent` **must match the delegate** - void, two floats. Rename the parameters all you like, but the types and order are fixed.
- `Vector3.Lerp(a, b, t)` blends between two values. `t` goes 0 → 1 and we feed it `elapsedTime / duration`.
- `yield return null` waits a single frame. A `while` loop with that inside is the standard way to animate something over time in code.

---

### **Step 3: Set Up in Unity**

1. **Create an empty GameObject** called **EventSender** and attach the `EventSender` script.
2. **Create a Cube** called **EventReceiver** and attach the `EventReceiver` script.
3. **Press Play** and hit the space bar. The cube squashes up and back.
4. **Duplicate the receiver cube five or six times** and spread them around.
5. **Press space again** - they all react to the one event.

**Now the useful experiment**:

- Select the **EventSender** and change `scale` and `speed` _while playing_. Press space again. The receivers use whatever values arrived with the event.
- **Delete the EventSender object** while playing, then press space. Nothing happens, no errors.
- **Disable one receiver** (untick it in the Inspector). Press space. It doesn't react - because `OnDisable` unsubscribed it. Re-enable it and it's back.

*REMEMBER*: **Subscription is what makes an object "listening"** - not its existence in the scene.

---

### **Step 4: The Two Arrangements Compared**

You've now seen both. Neither is more correct - they suit different jobs.

| **Comparing**       | **Part 3 - central manager**             | **Part 4 - event on the sender**     |
| ------------------- | ------------------------------------ | -------------------------------- |
| **Event lives in**  | A separate static class              | The sender class itself          |
| **Subscribe with**  | `HealthEventManager.OnObjectDamaged` | `EventSender.OnFire`             |
| **Who can fire it** | Any script, from anywhere            | Really only the sender should    |
| **Best for**        | Many unrelated senders, one message  | One clear source of one event    |
| **Downside**        | Harder to trace who fired it         | Subscribers must know the sender |

**Our damage events suit the manager** - crates, enemies and exploding crates are unrelated classes that all raise the same message, and listeners shouldn't need to know about any of them.

**A "player fired a weapon" event suits living on the sender** - there's one obvious source, and `PlayerWeapon.OnFire` says exactly where it came from.

> [!note] **Note - a third option exists and we'll get to it.**
> 
> Both of these need the subscriber to know a class name - either the manager or the sender. A generic **Event Bus** removes even that: subscribe to a message _type_ rather than a class. It's more machinery, so we'll meet it look at it when the game is big enough to need it or it makes sense in the module.

---

### **Key Learning Points**

1. **Event communication** - two classes talk with no reference between them.
2. **Parameters** - events carry whatever data the receiver needs, as long as the shapes match.
3. **One-to-many** - one broadcast, any number of receivers, including zero.
4. **Subscribe / unsubscribe discipline** - `OnEnable` and `OnDisable`, always paired.

### **Week 3 Summary - Code Communication**

Four parts, one central idea - **how much does the caller need to know?**

1. **Direct call** - the caller needs the exact type and a reference to the object. (`GameManager.instance.IncreaseScore()` from Week 1.)
2. **Interface** - the caller still needs the reference, but not the type. The bullet damages anything damageable.
3. **Event** - the caller needs neither. The crate announces only.

Each step removes something the caller has to know. That's what **decoupling** means in practice, and it's the main learning running through the rest of the module - every pattern in some way is works around this concept.