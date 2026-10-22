---
Order: 6-B
---
# **Week 6 - Part 2 : The Event Bus**

### Overview

#### **What we are building**:
- A generic **`EventBus<T>`** - one small class that can carry any kind of message.
- An **`EnemyKilled`** message, published once when an enemy dies.
- **Three unrelated systems** subscribing to it, none of which know about each other.

#### **Why we are doing this**:
- Killing an enemy is interesting to several things at once - progression wants the XP, something wants to spawn an effect, the log wants to report it. That list will keep growing.
- Declaring a named event by hand for every single thing that happens in a game gets tedious quickly.
- The death effect subscriber is the hook the VFX work will connect into - a message carrying a position is everything an effect needs.

#### **Learning Objectives**:
- **Generic classes** - one class that works with any type.
- **Message types** - describing an event as data rather than as a method signature.
- **Publish and subscribe** with no shared reference at all.
- **Choosing between a direct call, a named event and a bus.**

---
---

# **Part 2: One Bus, Any Message**

Carry on from Part 1. The HUD should be working and the GameManager should be awarding XP when enemies die.

---

### **Step 1: The Problem**

Open `Enemy.cs` and look at `Die()`:

```csharp
    private void Die()
    {
        GameManager.Instance.AddXP(enemyData.xpValue);

        Destroy(gameObject);
    }
```

That works, and in Week 5 it was a good way to do it because only one thing cared. Now think about what else wants to know when an enemy dies:

- Progression wants the XP
- Something should spawn an explosion where it died 
- A sound should play
- The event log should report it
- Later on, a wave system might want to count what's left

We could add a line to `Die()` for each of those, and the enemy would end up holding a reference to five different systems. Or we could add five named events to a manager class, which is better but it's five delegates, five events and five firing methods to write by hand. We could also make a general event and pass a load of parameters around but somewhere there is going to end up a lot of conditional logic. This tends to happen often but here we're going to do a clean implementation.

We'll describe **what happened** as a piece of data, publish it once, and let anything that cares subscribe.

---

### **Step 2: Create the `EventBus`**

1. **Create a C# script called `EventBus.cs`**. Delete the Unity boilerplate.

```csharp
using System;

// A generic event bus.
// The <T> means this class works with ANY type - we decide what T is when we use it.
//
// EventBus<EnemyKilled> and EventBus<CoinCollected> are two completely separate
// classes as far as C# is concerned, each with its own event. One bus per message type.

public static class EventBus<T>
{
    // Action<T> is a built-in delegate - a method that takes one T and returns void.
    // Saves us declaring our own delegate every time.
    private static Action<T> OnEvent;

    public static void Subscribe(Action<T> handler)
    {
        OnEvent += handler;
    }

    public static void Unsubscribe(Action<T> handler)
    {
        OnEvent -= handler;
    }

    public static void Publish(T message)
    {
        if (OnEvent != null)
        {
            OnEvent(message);
        }
    }
}
```

**Explanation**:

- **`<T>`** is a **type parameter**. Writing `EventBus<EnemyKilled>` gives us a version of this class that works with `EnemyKilled` messages, and the compiler checks it - you can't publish the wrong type by mistake.
- Each closed version gets **its own `OnEvent`**. Subscribing to `EventBus<EnemyKilled>` won't get you `EventBus<CoinCollected>` messages.
- `Action<T>` is a delegate type that comes with C#. We could have written `public delegate void MessageHandler(T message);` ourselves, and `Action<T>` is the same thing already written for us.
- Ten lines that covers every message type we will ever add.

> [!note] **Note - generics can be hard to understand**
> 
> The odd part is that `EventBus<EnemyKilled>` and `EventBus<CoinCollected>` are **separate classes**, built from the same source. Each one has its own copy of the static field.
> 
> You've used generics already without thinking about it - `List<GameObject>` and `List<Item>` are separate types made from the same `List<T>`. This is the same idea, we're just writing the `<T>` version this time.

---

### **Step 3: Create the Message**

A message is a small piece of data describing what happened. It has no behaviour. We're going to use a `struct`. Its like a small efficient data structure. Here its just 3 variables that the message carries.

1. **Create a C# script called `GameMessages.cs`** in the Scripts folder. We'll keep all our message types in one file.

```csharp
using UnityEngine;

// Messages that get sent around the game.
// These are plain data - no MonoBehaviour or methods, just the data about what happened.

public struct EnemyKilled
{
    public string enemyName;
    public int xpValue;
    public Vector3 position;
}
```

**Explanation**:

- A **`struct`** is like a small class, but it's copied by value rather than passed by reference. For a few fields of data like this it's the usual choice and it avoids creating garbage every time an enemy dies.
- The fields are chosen by asking **what would a subscriber need?** The XP for progression, the position for an effect, the name for the log. Add more when something needs more.
- Keeping messages in one file makes it easy to see everything the game can announce.

---

### **Step 4: Publish from the Enemy**

1. **In `Enemy.cs`**, replace `Die()`:

```csharp
    private void Die()
    {
        // Describe what happened
        EnemyKilled message = new EnemyKilled();
        message.enemyName = enemyData.enemyName;
        message.xpValue = enemyData.xpValue;
        message.position = transform.position;

        // Announce it. We have no idea whats listening. 
        EventBus<EnemyKilled>.Publish(message);

        Destroy(gameObject);
    }
```

2. **Press Play** and kill an enemy. Nothing happens - no XP, no log entry. Nothing is subscribed yet.

**Explanation**:

- Have a look through `Enemy.cs` now. **It doesn't mention `GameManager` anywhere.** Before this change it connected into it to award XP.
- The enemy's job is to be an enemy. Reporting that it died is fine. Knowing what XP is for shouldn't be a part of the enemy's job.

---

### **Step 5: Subscribe - Progression**

1. **In `GameManager.cs`**, add subscription methods:

```csharp
    private void OnEnable()
    {
        EventBus<EnemyKilled>.Subscribe(HandleEnemyKilled);
    }

    private void OnDisable()
    {
        EventBus<EnemyKilled>.Unsubscribe(HandleEnemyKilled);
    }

    private void HandleEnemyKilled(EnemyKilled message)
    {
        AddXP(message.xpValue);
    }
```

**Press Play** and kill an enemy. XP goes up and the HUD updates, same as before.

**Explanation**:

- The chain now goes like this: 
	- enemy publishes 
		- GameManager hears it and adds XP 
			- `AddXP` fires the UI event 
				-  the HUD updates. 
- A few steps, and no class in that chain holds a reference to the next one.
- Same subscribe and unsubscribe process as everything else.

---

### **Step 6: Subscribe - Death Effects**

Now a second subscriber that has nothing to do with the first.

1. **Create a C# script called `EnemyDeathEffects.cs`** and attach it to an empty GameObject called **Death Effects**:

```csharp
using UnityEngine;

// Spawns an effect wherever an enemy died.
// No reference to Enemy, and Enemy has no reference to this.
public class EnemyDeathEffects : MonoBehaviour
{
    public GameObject deathEffectPrefab;   // a particle system, or anything you like

    private void OnEnable()
    {
        EventBus<EnemyKilled>.Subscribe(HandleEnemyKilled);
    }

    private void OnDisable()
    {
        EventBus<EnemyKilled>.Unsubscribe(HandleEnemyKilled);
    }

    private void HandleEnemyKilled(EnemyKilled message)
    {
        if (deathEffectPrefab == null)
        {
            return;
        }

        // The message told us where it happened
        Instantiate(deathEffectPrefab, message.position, Quaternion.identity);
    }
}
```

2. **Make something to spawn.** A particle system is ideal. If you want something quick, a scaled-down sphere with an `ObjectDestruction` component set to destroy after half a second will do to prove it works.
3. **Press Play** and kill an enemy. XP goes up **and** an effect appears where it died.

**Explanation**:

- We added a whole new system and changed nothing. No edit to `Enemy`, no edit to `GameManager`.
- `message.position` is the reason the effect works in the right place. The enemy is already destroyed by the time this runs, so the position had to travel with the message.

> [!note] **Note - this is an example of a hook for your VFX work or your game feel**
> 
> A message carrying a position and a name is everything an effect needs to decide what to spawn and where.
> 
> This is also where the module can be connected - screen shake, hit pauses, impact effects, anything reacting to gameplay. The gameplay code publishes what happened, and the feel work subscribes. Neither has to know about the other. 
> 
> *A good event system makes it way easier to do game feel!*

---

### **Step 7: Subscribe - The Log**

Third subscriber, and this one goes on a class we already have.

1. **In `EventLogger.cs`**, add to the existing `OnEnable` and `OnDisable`:

```csharp
    private void OnEnable()
    {
        HealthEventManager.OnObjectDamaged += HandleObjectDamaged;
        HealthEventManager.OnObjectDestroyed += HandleObjectDestroyed;

        EventBus<EnemyKilled>.Subscribe(HandleEnemyKilled);
    }

    private void OnDisable()
    {
        HealthEventManager.OnObjectDamaged -= HandleObjectDamaged;
        HealthEventManager.OnObjectDestroyed -= HandleObjectDestroyed;

        EventBus<EnemyKilled>.Unsubscribe(HandleEnemyKilled);
    }
```

2. And add the handler:

```csharp
    private void HandleEnemyKilled(EnemyKilled message)
    {
        AddToLog("<color=yellow>" + message.enemyName + " killed - " + message.xpValue + " XP</color>");
    }
```

**Explanation**:

- The `EventLogger` now listens to **two different systems** - the Week 3 `HealthEventManager` and the new bus. They coexist with no trouble.
- That's by design. The bus is **additive** - we haven't replaced anything that was working, we've added another way to communicate for the cases that need it.

---

### **Step 8: Test**

1. **Press Play** and kill a few enemies.
2. One publish, three things happen - XP, an effect, a log line.
3. **Disable the Death Effects object** while playing and keep killing. XP and the log carry on with no errors. Re-enable it and effects come back.
4. **Open `Enemy.cs`** and confirm it has no reference to the GameManager, the effects, or the log.

---

### **Which one should you use?**

We've now got four ways for one object to tell another something. They all have a purpose.

| Way                                | Use it when                                                      |
| ---------------------------------- | ---------------------------------------------------------------- |
| Direct call on a reference         | You already have the reference and there's one user              |
| `GameManager.Instance.Something()` | One known user, but no way to get a reference                    |
| Named event (`UIEventManager`)     | A known set of listeners, and the event means something specific |
| `EventBus<T>`                      | Several unrelated listeners, or you don't know who yet           |

Our game uses all four, on purpose:

- `Bullet` calls `TakeDamage` straight on what it hit. It has the reference, there's one target.
- `Coin` calls `GameManager.Instance.AddCoins()`. One consumer, but a spawned coin has no reference to anything.
- `PlayerStats` fires `UIEventManager.HealthChanged`. The UI is the known audience and the meaning is specific.
- `Enemy` publishes `EnemyKilled` to the bus. Three listeners already and more to add.

The test that decides it is **how many things care, and do you know what they are?** One known thing is a call. A known audience is a named event. An unknown and growing list is the bus.

Also, I like using event structures in programming. It very little extra work. Its nice and tidy and very hard to break. Its means others can work on different parts without messing up each others work.

> [!note] **Note - the bus has a trade-off**
> 
> The thing that makes it useful can be a problem. Publish a message and there is **nothing in the code** showing you what runs next - you have to go looking for subscribers. A good IDE will sort this out.
> 
> With a direct call you can click through in the editor and see exactly where you end up. With the bus you can't.
> 
> That trade-off is worth making when several systems need the same message, and not worth making when one does. Using the the bus for everything makes a codebase very hard to follow, which is the opposite of what it's for.

---

### **What we've set up**:

- A generic **`EventBus<T>`** that carries any message type.
- An **`EnemyKilled`** message with the data its subscribers need.
- **Three subscribers** - progression, death effects and the log - none of which know about each other.
- An `Enemy` class that reports what happened and nothing more.

### **Week 6 Summary**

- **Part 1** - **Observer** and **MVP**. The game announces changes, the UI subscribes and updates itself, and the model knows nothing about the display.
- **Part 2** - the **Event Bus**. One generic class for messages with several interested parties.

Both parts are the same underlying idea - something announces that a thing happened, and anything that cares listens. What changes between them is how specific the announcement is and how many things are expected to hear it. 

### **Next**:

**Week 7** is the **Factory** pattern, where we take the enemy spawning from Week 4 and give it a cleaner way to create different enemy types.

Might start on the **State** pattern too. Factory is a small change but state is into the character behaviour work. We want to cover Factory, State, Serialisation (save system) and make a mini inventory over the next few weeks. 

