---
Order: 4-A
---
# **Week 4 - Part 1 : Extending the Event System**

### Overview

#### **What we are building**:
- We'll go back into the event system from last week and **extend it** so the events carry more information.
- At the moment an event tells us a health number and nothing else. We'll add the **name** of the object that was damaged.
- Small change, but it's the thing that makes the events more potentially useful to the rest of the game.

#### **Why we are doing this**:
- At the end of Week 3 we looked at where our event system falls down - the listener gets an `int` and has no idea what it belongs to. Crate? Enemy? The player? Its not defined.
- This week is about **data** - how we structure it, pass it around and store it. Extending an event to carry better data is a good place to start.
- It also shows something worth knowing about events - they decouple the _reference_ but the _signature_ is still a shared contract.

#### **Learning Objectives**:
- **Changing a delegate signature** and understanding what that breaks. 
- **Passing multiple parameters** through an event.
- **Shared contracts** - why every subscriber has to agree on the shape.

---
---

# **Part 1: Adding Data to the Events**

Carry on from Week 3. You should have the `HealthEventManager`, the three damageable objects (`Crate`, `ExplodingCrate`, `Enemy`) and an `EventListener` logging to the Console.

---

### **Step 1: Where We Left Off**

At the end of Week 3 Part 3 we fired an event like this:

```csharp
HealthEventManager.OnObjectDamaged(health);
```

One `int`. The remaining health.

The listener gets that number and can do very little with it. It can't tell what was hit, so it can't award points for an enemy but not a crate, it can't play the right sound, and it can't pick the right health bar.

We'll fix that by adding the object's name to the event. It's a small change but you'll see it affects every class that uses the event, which is worth noticing.

---

### **Step 2: Change the Delegate**

Open `HealthEventManager.cs`. We only need to change one line - the delegate.

**The original:**

```csharp
    public delegate void HealthEvent(int currentHealth);
```

**The modified version:**

```csharp
    // The delegate now describes a method that takes a STRING and an INT
    // Every method that subscribes has to match this shape exactly
    public delegate void HealthEvent(string name, int currentHealth);
```

The full class should now look like this:

```csharp
// HealthEventManager.cs
// A static class - we never create an instance of it, we just use it directly.
// It holds the events. It does not react to them.

public static class HealthEventManager
{
    // A DELEGATE defines the SHAPE of a method:
    // "returns void, takes a string and an int" - any method matching that shape can subscribe
    public delegate void HealthEvent(string name, int currentHealth);

    // Called when any object implementing IDamagable takes damage
    public static HealthEvent OnObjectDamaged;

    // Called when any object implementing IDamagable is destroyed
    public static HealthEvent OnObjectDestroyed;
}
```

**Explanation**:

- The two events don't change at all - they are still of type `HealthEvent`. We've only changed what `HealthEvent` means.
- Both events get the new shape automatically because they share the delegate. That's one of the reasons we declared the delegate separately rather than writing the shape out twice.

> [!warning] **Note - save this and look at the Console. Everything is broken.**
> 
> Changing a delegate is a **breaking change**. Every class that fires the event and every method that subscribes to it now has the wrong shape, and the project won't compile until we fix them all.
> 
> This is worth thinking about. Events decoupled our classes from each other - the crate doesn't know the listener exists. But the **signature** is still a contract that everyone shares, so changing it affects everything.
> 
> _It's a trade-off._ Decoupling the reference is not the same as decoupling completely. We'll come back to this later in the module when we look at other ways to structure events.

---

### **Step 3: Update the Objects That Fire the Event**

Each class now passes its name along with the health. `gameObject.name` gives us the name of the GameObject the script is sitting on.

**`Crate.cs`** - update `TakeDamage`:

```csharp
    public void TakeDamage(int damage)
    {
        health -= damage;

        // Now passing TWO things - the name of this object, and the health left
        if (HealthEventManager.OnObjectDamaged != null)
        {
            HealthEventManager.OnObjectDamaged(gameObject.name, health);
        }

        if (health <= 0)
        {
            if (HealthEventManager.OnObjectDestroyed != null)
            {
                HealthEventManager.OnObjectDestroyed(gameObject.name, health);
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
            HealthEventManager.OnObjectDamaged(gameObject.name, health);
        }

        if (health <= 0)
        {
            Explode();

            if (HealthEventManager.OnObjectDestroyed != null)
            {
                HealthEventManager.OnObjectDestroyed(gameObject.name, health);
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
            HealthEventManager.OnObjectDamaged(gameObject.name, health);
        }

        if (health <= 0)
        {
            if (HealthEventManager.OnObjectDestroyed != null)
            {
                HealthEventManager.OnObjectDestroyed(gameObject.name, health);
            }

            Die();
        }
    }
```

**Explanation**:

- The order of the arguments has to match the delegate - `string` first, then `int`. Swap them and it won't compile.
- Everything else in these classes stays exactly as it was. We're only touching the lines that fire events.
	- A good IDE will help you out here with debugging and potentially refactoring
	- VS Code is a code editor. Visual Studio is an Integrated Development Environment (IDE)

> [!note] **Note - you can use the shorthand now if you'd rather.**
> 
> In Week 3 we wrote the null check out in full so you could see what it was doing. If you're comfortable with it, the `?.` version does the same job in one line:
> 
> `HealthEventManager.OnObjectDamaged?.Invoke(gameObject.name, health);`
> 
> Use whichever you find easier to read. I use the long version in these tutorials so it stays consistent, and its what I'm used to.

---

### **Step 4: Update the Listener**

The `EventListener` methods have to match the new shape too.

```csharp
    // The parameters now match the delegate - string first, then int
    private void HandleObjectDamaged(string name, int remainingHealth)
    {
        string message = "An object called " + name + " was damaged! Remaining Health: " + remainingHealth;
        Debug.Log(message);
    }

    private void HandleObjectDestroyed(string name, int remainingHealth)
    {
        string message = "An object called " + name + " was destroyed!";
        Debug.Log(message);
    }
```

The `OnEnable` and `OnDisable` subscriptions don't change at all - we're still handing over the same two methods, they've just changed shape internally.

**Explanation**:

- The parameter _names_ are up to you - `name` and `remainingHealth` here, but you could call them anything. It's the **types and the order** that have to match.
- Notice how little had to change in the listener. It was already set up to receive and log, we've just given it more to work with.

---

### **Step 5: Test It**

1. **Press Play** and shoot a few things.
2. **Watch the Console** - the messages now name the object:
    - `An object called Crate was damaged! Remaining Health: 8`
    - `An object called Enemy was destroyed!`

> [!note] **Note - spawned objects get `(Clone)` on the end of their name.**
> 
> If you spawn something with `Instantiate`, Unity names the copy `Enemy(Clone)`. You'll see that in the log and it looks a bit untidy.
> 
> You can set the name yourself after spawning, which we'll do in Part 4 when the ScriptableObject gives each enemy a proper name. For now it's just something to be aware of.

---

### **What we've done**:

- Changed the `HealthEvent` delegate to carry a **name** as well as a health value.
- Updated the three damageable classes to pass `gameObject.name`.
- Updated the listener to receive it.

### **Worth thinking about**:

We fixed the immediate problem, but think about where this goes. If we later want the **damage amount**, we add a third parameter and break everything again. Then the **position** for a hit effect, then **who fired the shot**...

At some point passing loose parameters stops scaling and you'd want to pass a single object holding all of it - something like a `DamageInfo` class with all the details inside. That's a data structure decision, which is what this week is about.  

Data is just parameters that get passed around. Parameters can be contained in a class (`DamageInfo`). We can pass the class around as an object.

In **Part 2** we'll uses the new name parameter and build a proper event log in the UI.