---
Order: 5-A
---
### Overview

#### **What we are building**:
- A **`GameManager`** using the **Singleton** pattern, so anything in the game can reach it without needing a reference.
- It will track **coins** and **XP**, and hold a reference to the player.
- We'll add a **Coin** item for the coins to come from, and have enemies award XP when they die.
#### **Why we are doing this**:
- We left three `TODO`s in the code last week and they're all waiting on the same thing - something that exists once and can be reached from anywhere.
- Week 1 used a quick version of this pattern. It was flagged it at the time as a hack. This week we build it properly.
- Singleton is a pattern you'll see everywhere in Unity projects. It's also one of the most argued about, so it's worth understanding the problems as well as what it gives you.
#### **Learning Objectives**:
- **The Singleton pattern** - one instance, reachable from anywhere.
- **`static`** - members that belong to the class rather than to an object.
- **Properties with a private setter** - letting other scripts read a value but not change it.
- **Deciding when a Singleton is the right tool** - and when it isn't.

---
---

# **Part 1: Building the GameManager**

Carry on from Week 4. You should have enemies spawning and chasing, the player taking damage, and the event logger running.

---

### **Step 1: What We're Missing**

Have a look at the `TODO`s we left last week:

```csharp
    // In Enemy.cs
    private void Die()
    {
        // TODO (Week 5) - award enemyData.scoreValue to the GameManager
        Destroy(gameObject);
    }
```

```csharp
    // In PlayerStats.cs
    private void Die()
    {
        Debug.Log("Player died!");
        // TODO (Week 5) - the GameManager will handle game over and restarting
    }
```

```csharp
    // In Enemy.cs
    private void Start()
    {
        // Searching the whole scene every time an enemy spawns
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        ...
    }
```

All three want the same thing. **Something that exists exactly once, that any object can reach without being handed a reference first.**

An enemy dying in the corner of the arena needs to award XP. It has no reference to anything that tracks XP, and we don't want to drag one onto every enemy prefab - they're spawned at runtime so we couldn't anyway.

The Singleton pattern solves that.

---

### **Step 2: The Week 1 Version (and what's wrong with it)**

We already used a version of this back in Week 1. Open the collectathon GameManager and have a look:

```csharp
    // From Week 1 - the 'lazy singleton'
    public static GameManager_Collectathon instance;

    void Awake()
    {
        instance = this;
    }
```

It works, and for a tiny game it's fine. I referred to it as a hack at the time. Here's what's wrong with it:

1. **`instance` is public**, so any script in the game can overwrite it. `GameManager_Collectathon.instance = null;` compiles fine and breaks everything.
2. **No duplicate check.** Put two GameManagers in the scene and whichever one runs `Awake` last wins. No error, and you'd spend a while working out why half your score updates vanish.
3. **No help when it's missing.** Forget to put the GameManager in the scene and the first script that uses it throws a `NullReferenceException` pointing at a line that looks fine.
4. **Nothing stops it growing.** It's easy to reach, so everything gets put in it. We'll come back to that at the end of Part 2.

We'll fix the first three now.

---

### **Step 3: Create the `GameManager`**

1. **Create a C# script called `GameManager.cs`** in the Scripts folder.

```csharp
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton

    // The one and only GameManager.
    // 'private' so nothing outside this class can overwrite it.
    // 'static' means it belongs to the CLASS, not to any one object - there is only ever one of these.
    private static GameManager instance;

    // The public way in. Any script can use GameManager.Instance
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Nothing has set it yet, so look for one in the scene
                instance = FindFirstObjectByType<GameManager>();

                if (instance == null)
                {
                    Debug.LogError("There is no GameManager in the scene. Add one.");
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        // If there is already a GameManager then this one is a duplicate, so remove it
        if (instance != null && instance != this)
        {
            Debug.LogWarning("A second GameManager was found. Destroying it.");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    #endregion

    #region Progression

    [Header("Progression")]
    [SerializeField] private int coins = 0;
    [SerializeField] private int xp = 0;

    // Other scripts can READ these but not change them.
    // The only way to change them is through the two methods below.
    public int Coins
    {
        get { return coins; }
    }

    public int XP
    {
        get { return xp; }
    }

    public void AddCoins(int amount)
    {
        coins = coins + amount;
        Debug.Log("Coins: " + coins);
    }

    public void AddXP(int amount)
    {
        xp = xp + amount;
        Debug.Log("XP: " + xp);
    }

    #endregion

    #region Player Reference

    // The player registers itself here so anything that needs it can ask us
    private PlayerStats player;

    public PlayerStats Player
    {
        get { return player; }
    }

    public void RegisterPlayer(PlayerStats newPlayer)
    {
        player = newPlayer;
    }

    #endregion
}
```

2. **Create an empty GameObject** in the scene called **GameManager** and attach the script.

**Explanation**:
- **`static`** means the member belongs to the class rather than to an object. We never write `new GameManager()` - we write `GameManager.Instance` and get the one that already exists.
- The **`Instance` property** is way to access it. If `instance` hasn't been set yet it looks for one in the scene, and tells you clearly if there isn't one.
- **`Awake` handles duplicates.** If one already exists, the second destroys itself rather than taking over.
- **`coins` and `xp` are private with public read-only properties.** Other scripts can check the values but can't set them directly - they have to go through `AddCoins` and `AddXP`. Same idea as the `Health` property we put on `IDamagable` in Week 3.

> [!note] **Note - why no "create one if it doesn't exist"**
> 
> A lot of singleton examples you'll find online have an extra bit in the getter that creates a GameObject and adds the component if it can't find one, so `Instance` can never be null.
> 
> I've left that out on purpose. A GameManager created that way has nothing set in the Inspector, so you end up with an object that exists but isn't configured, and no error to tell you. A clear message in the Console is more use than an empty object that half works.
> 
> _Have a look at the version we used in Week 1 - it had the create branch in it. Fine for that game because it had nothing to configure._

> [!note] **Note - `#region` blocks**
> 
> `#region` and `#endregion` let you fold sections of a script in your editor. They do nothing to the code, they're purely for keeping a longer file readable.
> 
> Handy on a class like this that's going to have a few different jobs in it. This is where the central manager class is controversial - if a class needs ten regions to be readable, the class is probably doing too much.

---

### **Step 4: Rename `scoreValue` to `xpValue`**

Small refactor first, same as we did with the `EventLogger` in Week 4.

We called the field `scoreValue` in the `EnemyData` ScriptableObject, but we're tracking **XP** rather than a generic score, so the name should say that.

1. **Open `EnemyData.cs`** and update the Rewards section:

```csharp
using UnityEngine;
using UnityEngine.Serialization;   // needed for FormerlySerializedAs

...

    [Header("Rewards")]
    [FormerlySerializedAs("scoreValue")]
    public int xpValue = 10;       // XP awarded when this enemy dies
```

2. **Check your two EnemyData assets** - Bad Guy - Green should still say 10, Bad Guy - Blue should still say 25.

> [!warning] **Note - renaming a serialised field loses the value**
> 
> This one can catches people out
> 
> Unity saves serialised fields **by name**. Rename `scoreValue` to `xpValue` and as far as Unity is concerned the old field has gone and a brand new one has appeared, so your authored values on both assets reset to the default.
> 
> **`[FormerlySerializedAs("scoreValue")]`** tells Unity "this used to be called that, bring the old value across". Once everything has loaded once you can delete the attribute.
> 

---

### **Step 5: Award XP When an Enemy Dies**

Now the first `TODO` gets filled in.

**In `Enemy.cs`**, update `Die()`:

```csharp
    private void Die()
    {
        // Tell the GameManager about the kill - it handles progression, we just report it
        GameManager.Instance.AddXP(enemyData.xpValue);

        Destroy(gameObject);
    }
```

**Press Play** and shoot a few enemies. The Console reports the XP going up, and green enemies are worth less than blue ones because that's what we set on the data assets.

**Explanation**:

- `GameManager.Instance` gets us the GameManager without the enemy holding a reference to it.
- The enemy doesn't track XP or know what happens to it. It reports the kill and the GameManager decides what that's worth.
- This is the part a Singleton makes easy. The enemy was spawned at runtime by the Spawner, so there was never a point where we could have dragged a reference onto it in the Inspector.

---

### **Step 6: Create the Coin Item**

XP comes from kills. Coins need something to come from, so we'll add a new item type.

This uses the `Item` base class from Week 2 - we're not writing anything new, just another class that inherits from it.

1. **Create a C# script called `Coin.cs`** in the Scripts folder.

```csharp
using UnityEngine;

// Derived class Coin - it IS an Item, and picking it up adds to the coin total
public class Coin : Item
{
    [Header("Coin")]
    public int coinValue = 1;

    protected override void Awake()
    {
        base.Awake();   // always call the base version first

        data = new ItemData("Coin", "Worth something to somebody.");
    }

    // The player parameter is here because the base class asks for it.
    // This item doesn't need it - coins go to the GameManager, not the player.
    public override void Use(PlayerStats player)
    {
        Debug.Log("Picked up a coin worth " + coinValue);

        GameManager.Instance.AddCoins(coinValue);
    }
}
```

2. **Build the prefab**:
    - `GameObject -> 3D Object -> Cylinder or Similar`. (There is a coin mesh in the project) - Rename it **Coin** and scale it flat, something like 0.3, 0.05, 0.3.
    - Tick **Is Trigger** on the collider.
    - `Add Component -> Coin`. Set **Item Color** to yellow.
    - Drag it into the **Prefabs** folder and delete it from the Hierarchy.
3. **Add it to the item spawner** - set **Prefabs To Spawn** to Size 3 and drop the Coin prefab into the new slot.

Press Play and move over a coin. The Console reports the pickup and the coin total going up.

**Explanation**:
- We wrote **one** new class and got spinning, colour, the pickup trigger and the polymorphic `Use()` call for free. All of that is in the base `Item` class from Week 2.
- The spawner needed no changes at all. It holds a `GameObject[]` of prefabs and doesn't care what they are.
- Notice `Use()` ignores the `player` parameter it's given. The base class requires the parameter because most items need it, and this one doesn't. Not a problem, just something you'll run into when a shared method has to suit everything that uses it.

> [!note] **Note - the Week 2 Inheritance Work**
> 
> To get a sense the inheritance setup in Week 2 was worth the effort. A whole new item type, working with the spawner, the trigger system and the visuals, in about fifteen lines.
> 
> _We'll add more item types in Week 6 when we build the inventory, and they'll all be about this size and similar code._

---

### **Step 7: Let the Player Register Itself**

The last `TODO` from the list - enemies searching the whole scene to find the player.

Now that we have a GameManager, the player can tell it where it is when it wakes up, and anything that needs the player can ask.

1. **In `PlayerStats.cs`**, add an `Awake` method:

```csharp
    private void Awake()
    {
        // Tell the GameManager we exist, so anything that needs the player can find us through it
        GameManager.Instance.RegisterPlayer(this);
    }
```

2. **In `Enemy.cs`**, delete the whole `Start()` method and the `playerTransform` field, then update `Update()`:

```csharp
    private void Update()
    {
        // Ask the GameManager where the player is.
        // There might not be one - not spawned yet, or dead - so check before using it.
        if (GameManager.Instance.Player == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            GameManager.Instance.Player.transform.position,
            enemyData.moveSpeed * Time.deltaTime
        );
    }
```

**Explanation**:
- No more scene searching. The player registers itself once, and the GameManager passes it on to anything that needs it.
- We're asking every frame rather than caching it in `Start`. That might look wasteful but `Player` is a property that returns a field, so it costs almost nothing, and it means the enemy doesn't care **when** the player appeared.
- The null check matters more than it looks. In Part 2 the player gets spawned by the GameManager and respawns after dying, so there are stretches where no player exists. The enemies will just stand still until one turns up.

> [!note] **Note - this is what the Singleton is actually for**
> 
> Before this, an enemy needed either a reference dragged in the Inspector (impossible, it's spawned at runtime) or a scene search (slow, and it only worked because we did it once in `Start`).
> 
> Now it asks a class that it knows will exist. No reference or searching or worrying about what order things were created in.
> 
> The cost is that `Enemy` now depends on `GameManager`. It won't compile without it, and it won't run in a scene that hasn't got one. Worth keeping in mind as we add more of these.

---

### **Step 8: Test It**

1. **Press Play**.
2. **Shoot enemies** - XP goes up in the Console, and the GameManager Inspector shows the running total.
3. **Collect coins** - coin total goes up.
4. **Collect potions** - health and mana still work exactly as before, handled by `PlayerStats` rather than the GameManager.
5. **Select the GameManager** while playing and watch `coins` and `xp` change in the Inspector.

Try breaking it on purpose:

6. **Duplicate the GameManager object** in the Hierarchy and press Play. One destroys itself and tells you in the Console. Delete the copy afterwards.
7. **Delete the GameManager** and press Play. The first enemy kill gives you a clear error telling you what's missing, rather than a null reference on some unrelated line.

---

### **When is a Singleton the right tool?**

The pattern gets overused and it's one of the more argued about patterns in game programming.

The test I use is two questions. **both** should be yes:

1. **Is there only ever one of these?** One GameManager, one audio manager, one save system. If you could reasonably have two, it isn't a singleton.
2. **Do lots of unrelated things need to reach it, from places you can't hand them a reference?** Spawned enemies, collected items, UI buttons.

Coins and XP pass both. An enemy dying and an item being collected both need to report progression, and neither one is anywhere near the thing that tracks it.

**Player health fails the second test.** The player object owns its own health, and anything that damages the player is already touching the player - our enemies get `PlayerStats` straight off the collision. Routing that through a global would be adding a middleperson for no reason.

> [!note] **Note - what a Singleton costs**
> 
> It's worth being even handed about this because you'll see people describe Singleton as an anti-pattern.
> 
> **Anything can call it from anywhere.** That's the whole point, and it's also the problem - you can't look at a class and tell what depends on it. `Enemy`, `Coin` and `PlayerStats` all reach into `GameManager` now and there's no reference anywhere to show you that.
> 
> **It's awkward to test.** You can't easily swap in a fake GameManager to test a class on its own, because the class goes and gets the real one itself.
> 
> **It makes more work.** Because it's easy to reach, everything ends up in it.
> 
> None of that makes it wrong, it just means use it where it needs to be rather than by default. We'll look at alternatives later in the module - the **Observer** pattern in Week 6 removes a lot of the need for global access.

---

### **What we've set up**:

- A **`GameManager`** singleton with duplicate protection and a clear error when it's missing.
- **Coins and XP**, read-only from outside, changed only through methods.
- A **Coin** item built on the Week 2 `Item` base class.
- Enemies awarding **XP** on death.
- The player **registering itself**, so nothing has to search the scene for it.

### **Next**:

In **Part 2** we hand the GameManager the job of spawning the player, give the game **lives**, and wire up **game over** and restarting. That gives us a loop with no bugs that we can work on adding new features and patterns.