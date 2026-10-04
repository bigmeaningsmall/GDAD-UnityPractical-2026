---
Order: 3-C
---
### Overview

#### **What we are building**:
- A **`HealthEventManager`** - a central place that holds events about things being damaged and destroyed.
- Our `Crate`, `ExplodingCrate` and `Enemy` will **broadcast** those events when they're hit.
- An **`EventListener`** that reacts to them, without any of the damaged objects knowing it exists.

#### **Why we are doing this**:
- Part 2 decoupled the bullet from _what it hit_. The bullet still needed to be holding the object to call `TakeDamage` on it.
- Events decouple the other direction: **the sender doesn't need a reference to the receiver at all**. A crate can announce it broke to the whole game without knowing - or caring - whether anything is listening.
- This is the mechanism almost all game architecture is built on. UI updates, audio, scoring, achievements, VFX - all of it works off events.

#### **Learning Objectives**:
- **Delegates**: a variable that holds a method.
- **Events**: a broadcast anyone can subscribe to.
- **Subscribing and unsubscribing**: `+=` and `-=`, and why both matter.
- **One-to-many communication**: one broadcast, any number of listeners.

---
---
# **Part 3: Broadcasting with Events**

Carry on in **Scene-Week3_Code-Communication**. Shooting and damage is working

---

### **Step 1: The Problem (read before coding)**

Say we want the Console to log every time something takes damage. We add a `Debug.Log` to each class.

Now the score UI needs updating. And a sound should play. And the camera should shake. And a damage number should pop up.

Doing that with direct calls means every damageable class needs a reference to the UI, the audio manager, the camera and the VFX system. **Five systems × three classes = fifteen references to wire up**, and every new damageable type has to wire up all five again. Change the UI and you edit every class that talks to it.

The event approach inverts this. The crate says **"I took damage!"**. Anything that cares is listening. The crate has no references to anything, and adding a new listener changes nothing about the crate. Its actually easier to use events than wire things in the editor.

**Terminology** - three words that get used loosely:

- **Delegate** - a _type_ that describes a method shape. "A method that takes an int and returns nothing."
- **Event** - a variable of that delegate type that others can subscribe to, and only the owner can fire.
- **Subscribe** - hand your method to an event so it gets called when the event fires.

---

### **Step 2: Create the `HealthEventManager`**

1. **Create the Script**:
    - Create a C# script called `HealthEventManager.cs`.
    - Delete the Unity boilerplate - **this is not a MonoBehaviour and goes on no GameObject**.

```csharp
// HealthEventManager.cs
// A static class - we never create an instance of it, we just use it directly.
// It holds the events. It does not react to them.

public static class HealthEventManager
{
    // A DELEGATE defines the SHAPE of a method:
    // "returns void, takes one int" - any method matching that shape can subscribe.
    public delegate void HealthEvent(int currentHealth);

    // Called when any object implementing IDamagable takes damage
    public static HealthEvent OnObjectDamaged;

    // Called when any object implementing IDamagable is destroyed
    public static HealthEvent OnObjectDestroyed;
}
```

**Explanation**:

- `static` means there is exactly one `HealthEventManager` for the whole game and we never create one with `new`. We just write `HealthEventManager.OnObjectDamaged`.
- `public delegate void HealthEvent(int currentHealth);` is the **shape**. Any method that returns void and takes a single int fits it.
- `OnObjectDamaged` and `OnObjectDestroyed` are the actual events - the noticeboards objects pin messages to.

> [!note] **Note - Why a separate class for this?**
> 
> The events could live on the `Crate`. But then a listener would need a reference to a specific crate - and crates are being created and destroyed constantly. By putting events in a static class, **a listener can subscribe before any crate exists and stay subscribed after they're all gone.**
> 
> In **part 4** I'll show an alternative way to use events without the static class using a sender and subscriber - depends on the situation or pattern when you would use one or the other. There are a few different ways to use events and event types but they are achieve the same thing.

---

### **Step 3: Broadcast from the Damageable Objects**

Each class now announces what happened. Add the broadcast lines to `TakeDamage`.
- Events are simple once you get to using them but for now type them out, look at the scripts side by side and understand how they broadcast and receive events.

**Only `TakeDamage` changes** - leave the `Health` property, `ShowHitEffect` and everything else from Part 2 exactly as they are.

**`Crate.cs`** - update `TakeDamage`:

```csharp
    public void TakeDamage(int damage)
    {
        health -= damage;

        // Broadcast that something was damaged - we have no idea who (if anyone) is listening.
        // We MUST check for null first - see the explanation below.
        if (HealthEventManager.OnObjectDamaged != null)
        {
            HealthEventManager.OnObjectDamaged(health);
        }

        if (health <= 0)
        {
            // Broadcast that something was destroyed
            if (HealthEventManager.OnObjectDestroyed != null)
            {
                HealthEventManager.OnObjectDestroyed(health);
            }

            Destroy(gameObject);
        }
    }
```

**`ExplodingCrate.cs`** - update `TakeDamage`:

```csharp
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (HealthEventManager.OnObjectDamaged != null)
        {
            HealthEventManager.OnObjectDamaged(health);
        }

        if (health <= 0)
        {
            Explode();

            if (HealthEventManager.OnObjectDestroyed != null)
            {
                HealthEventManager.OnObjectDestroyed(health);
            }

            Destroy(gameObject);
        }
    }
```

**`Enemy.cs`** - update `TakeDamage`:


```csharp
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (HealthEventManager.OnObjectDamaged != null)
        {
            HealthEventManager.OnObjectDamaged(health);
        }

        if (health <= 0)
        {
            if (HealthEventManager.OnObjectDestroyed != null)
            {
                HealthEventManager.OnObjectDestroyed(health);
            }

            Die();
        }
    }
```

**Explanation**:

- **Firing an event looks exactly like calling a method.** `OnObjectDamaged(health)` - the event holds a list of methods, and this calls every one of them with `health` as the argument.
- **Why the null check?** An event with no subscribers isn't an empty list - it's **`null`**. Nothing has handed it a method yet, so there's nothing there at all. Calling a method on null throws a `NullReferenceException` and your game stops working. So we check first, every time.
- This is the important bit: **the crate is now broadcasting but nothing is listening yet.**  The `if` is false, we skip the line, and the game runs on.

> [!warning] **Note - the null check is not optional.**
> 
> This catches people out constantly. You write the event, you fire it, you forget the check, and it works fine _while you're testing with the listener in the scene_. Then you load a different scene with no listener and the game throws errors on every hit.
> 
> **An event with zero subscribers is null.** Always check before firing.
> 

---

### **Step 3b: The Shorthand You'll See Everywhere**

Once you understand the null check, C# gives you a shorter way to write the exact same thing:
- *For me I prefer verbose code. Its easy to read and consistent.* 
- *I see lots of the shorthand code these days, usually because AI tends to do this and people use it without having learning the verbose text.* 
- *Brackets and conditions are very readable by humans. `?` is good when you are experienced or tidying up working code*

```csharp
    // These two blocks do IDENTICAL things:

    // The long way - what we wrote above
    if (HealthEventManager.OnObjectDamaged != null)
    {
        HealthEventManager.OnObjectDamaged(health);
    }

    // The short way - the '?.' does the null check for you - AI code looks like this and people who submit it usually don't understand it
    HealthEventManager.OnObjectDamaged?.Invoke(health);
```

The `?.` is called the **null-conditional operator**. It means _"only carry on if the thing to my left isn't null"_. Because `?.` needs a member to call rather than a bare `()`, we spell the call out as `.Invoke(...)` - which is just the delegate's own name for "call all the methods in your list".

Ultimately, use what you prefer but you can see the shorter is harder to understand. **I'd suggest writing the long version until the mechanism is second nature**, then switching - that way `?.` 

> [!warning] **Note - two completely different things called `Invoke`. Do not mix them up.**
> 
> - **`Invoke("MethodName", 0.1f)`** - a _Unity_ method that calls one of your own methods after a delay, by name, as a string. This is the one we deliberately avoided in Part 2 because the string can silently break.
> - **`OnObjectDamaged.Invoke(health)`** - a _C#_ delegate calling every method subscribed to it, right now, no delay.
> 
> Same word, unrelated features, and nothing in the syntax warns you. One is Unity's timer, the other is how events fire. This is why the long-form null check is worth writing out at least a few times - it has no `Invoke` in it at all, so there's nothing to confuse.

> [!note] **Note - `?.` is useful well beyond events.**
> 
> It works on anything that might be null. `player?.TakeDamage(5)` only calls the method if `player` isn't null. It replaces a lot of `if (x != null)` blocks once you're comfortable with it.

---

### **Step 4: Create a Listener**

Now something to receive the broadcast.

1. **Create `EventListener.cs`** and attach it to **any** GameObject in the scene (make an empty one called **EventListener** if you like).

```csharp
using UnityEngine;

public class EventListener : MonoBehaviour
{
    private void OnEnable()
    {
        // SUBSCRIBE - hand our methods to the events
        // '+=' means ADD this method to the list of things to call
        HealthEventManager.OnObjectDamaged += HandleObjectDamaged;
        HealthEventManager.OnObjectDestroyed += HandleObjectDestroyed;
    }

    private void OnDisable()
    {
        // UNSUBSCRIBE - always undo what you did in OnEnable
        // '-=' removes our method from the list
        HealthEventManager.OnObjectDamaged -= HandleObjectDamaged;
        HealthEventManager.OnObjectDestroyed -= HandleObjectDestroyed;
    }

    // This method MATCHES the delegate shape: returns void, takes one int
	private void HandleObjectDamaged(int remainingHealth)  
	{  
	    Debug.Log("EVENT LISTENER SAYS: An object was damaged! Remaining Health: " + remainingHealth);  
	}  
	  
	private void HandleObjectDestroyed(int remainingHealth)  
	{  
	    Debug.Log("EVENT LISTENER SAYS: An object was destroyed!");  
	}
}
```

2. **Press Play** and shoot something. The Console reports every hit.

**Explanation**:

- **`OnEnable` / `OnDisable` are the standard place to subscribe and unsubscribe.** They run when the object is turned on and off, so they always pair up correctly.
- `+=` adds your method to the event's list. `-=` removes it.
- `HandleObjectDamaged` matches the delegate shape exactly - void return, one int. Change it to take a string and it won't compile.

> [!warning] **Note - ALWAYS unsubscribe. This is the classic event bug.**
> 
> If you subscribe in `OnEnable` and never unsubscribe, the event keeps a reference to your method - and therefore to your object - _after the object is destroyed_. The event then calls a method on a dead object and you get `MissingReferenceException`, or worse, the object is kept alive in memory forever. That's a **memory leak**. You can actually load a new scene and and still get the error from not unsubscribing. I spent half a week debugging a project that was missing a `-` so the object was stuck in memory.
> 
> Subscribe in `OnEnable`, unsubscribe in `OnDisable`. Every time. Make it a habit.

---

### **Step 5: Prove It's One-to-Many**

This is the best part now.

1. **Duplicate the EventListener GameObject** two or three times.
2. **Press Play** and shoot one crate.
3. Every listener logs. **One broadcast, many receivers.**
4. Now **delete all of them** and shoot again. No errors - the null check catches it. **One broadcast, zero receivers.**

The crate's code is identical in all three cases. It has no idea how many things are listening, and it never needs to.

> [!note] **Note - this is the Observer pattern.**
> 
> You've just built the mechanism the **Observer pattern** is named after - subjects broadcast, observers subscribe. We'll name it properly, and build it more formally with a UI in **Week 5**.
> 
> It's worth knowing the pattern is just a name for what you already did here. Patterns are usually like this - a name for a solution people kept reinventing. You'll have invented this in a project as a  solution. We'll cover it in detail in later weeks. 
> 
> *You want to think less of "This object needs to find and get that object to tell it to something or change something" and more of "This one broadcasts an event and these ones react to it"*

---

### **Step 6: Where This Falls Down (important)**

Our event works, but look at what it actually sends:

```csharp
HealthEventManager.OnObjectDamaged(health);
```

One int. The remaining health. **The listener has no idea what was damaged.** Crate? Enemy? The player? All it gets is a number.

Compare that to the bullet in Part 2. It could write `damageable.Health` and ask the target a question, because it was **holding a reference** to the thing it hit. Our listener holds nothing and thats why its decoupled.  **Decoupling has a cost but there are ways around this which we'll address in later weeks**

So a score system can't tell whether to award points. The audio system can't choose between a wood-splinter sound and an enemy grunt. The UI can't show the right health bar.

That's the ceiling of this approach, but we built the simplest thing that works, and now we can see where it breaks. There are a few ways around:

- Pass more with the event (the GameObject, or a small data class describing the hit).
- Separate events per type - `OnEnemyDamaged`, `OnCrateDamaged`.
- A generic **Event Bus** that carries typed messages, which we build later in the module - This one is tricky but its the more advanced, fancy way.

> [!note] **Note - Don't fix this yet.**
> 
> We leave it as-is. When we build the Event Bus in a later week to solve the problem. 
> 
> This is a good habit generally - build the simple version, find its limit through use, then improve it. Jumping to the "most advanced" architecture before you've worked out the connections is how projects end up over-engineered. Depends what you are doing and scale you intend to go to.

---

### **Step 7: One Extra to Know About**

`HealthEventManager` is `static`, which means its events live for the lifetime of the whole application, not the scene.

If you have **Domain Reload disabled** in Project Settings (people turn it off to make entering Play Mode faster), static values **persist between Play sessions in the editor**. Stale subscriptions can survive, and you'll see events firing into objects from your last Play session.

If you get bizarre behaviour that only happens on your second Play, this is usually why. Good unsubscribing in `OnDisable` prevents it.

---

### **What we've set up**:

- A static **`HealthEventManager`** holding two events.
- Three damageable classes **broadcasting** without any references to listeners.
- An **`EventListener`** reacting, with correct subscribe/unsubscribe discipline.

### **The idea to take away**:

**Interfaces decouple the caller from the type. Events decouple the caller from the reference entirely.**

With the interface, the bullet still had to be holding the crate. With the event, the crate talks to systems it has never heard of and will never hold a reference to.

**Part 4** strips this back to the smallest possible example - two scripts, one event - so you can see the mechanism on its own with no game logic in the way.