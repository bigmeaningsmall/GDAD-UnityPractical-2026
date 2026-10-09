---
Order: 4-D
---
# **Week 4 - Part 4 : Scriptable Objects**

### Overview

#### **What we are building**:

- We'll create an **`EnemyData`** ScriptableObject holding the stats for an enemy type - name, health, speed, damage and colour.
- Two enemy variants get made from it, **Bad Guy - Green** and **Bad Guy - Blue**, each with different stats.
- The enemies will then **chase the player and damage them**, which means making `PlayerStats` damageable using the interface we wrote in Week 3.

#### **Why we are doing this**:

- ScriptableObjects are how Unity handles **authored data** - values a designer sets in the editor, stored in their own asset file, shared by everything that uses them.
- It's one of the most useful things in Unity and it's central to this week's data theme. Once you're using them you tend to use them a lot.
- It also closes the loop on the game. Enemies that arrive, chase you and hurt you means health works.

#### **Learning Objectives**:

- **ScriptableObjects** - what they are and when to use them.
- **Authored data vs runtime state** - the distinction that decides what goes in an SO.
- **`[CreateAssetMenu]`** - making your SOs creatable from the editor.
- **Reusing an interface** - `IDamagable` applied to something it wasn't written for.
- **Finding and identifying objects** - using components instead of tag strings.

---

---

# **Part 4: Data-Driven Enemies**

Carry on in the same scene. You should have enemies spawning from Part 3, and they should all be completely identical and harmless.

---

### **Step 1: What is a ScriptableObject? (read before coding)**

A **ScriptableObject** is a class that lives as an **asset file in your project**, not as a component on a GameObject. You create it in the Project window the same way you'd create a material or a prefab, and you fill in its values in the Inspector.

The important part is what that's _for_. Compare it to the plain `PlayerData` class we wrote in Week 2:

|Comparing|`PlayerData` (plain class)|`EnemyData` (ScriptableObject)|
|---|---|---|
|Lives|Inside a component, per object|As an asset file in the project|
|Each object gets|Its own copy|A shared reference to the same one|
|Values change|Constantly, while playing|Set by a designer, then left alone|
|We call this|**Runtime state**|**Authored data**|
|Gets saved to disk?|Yes (Week 9)|No - it's already a file|

That's the decision. Ask yourself: **does every object need its own copy of this, or are they all reading the same thing?**

- The player's **current health** changes every time they get hit, and nothing shares it. Runtime state, so its a plain class.
- An enemy type's **starting health** is a number the designer picked, and every Bad Guy - Green uses the same one. Authored data, so a ScriptableObject.

> [!note] **Note - this is the question that gets asked**
> 
> _"Why not just use ScriptableObjects for everything?"_
> 
> Because an SO is **shared**. If ten enemies reference the same `EnemyData`, there is one copy of those values between the lot of them. If you change it then you change it for all ten at once.
> 
> That's what you DO want for a starting health value, and what you DON'T want for current health. We'll demonstrate this properly in Step 6, because it's clearer when you see it happen.

---

### **Step 2: Create the `EnemyData` ScriptableObject**

1. **Create a C# script called `EnemyData.cs`** in the Enemy scripts folder.
2. Delete the default Unity code - this inherits from `ScriptableObject`, not `MonoBehaviour`.

```csharp
using UnityEngine;

// [CreateAssetMenu] adds this to Unity's Create menu so we can make assets from it.
// fileName is the default name for a new one, menuName is where it appears in the menu.
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "CRE340 - Game Tutorial/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Bad Guy";
    public Color enemyColor = Color.red;

    [Header("Stats")]
    public int startingHealth = 10;   // health this enemy type STARTS with
    public float moveSpeed = 2f;      // how fast it chases the player
    public int contactDamage = 5;     // damage it deals by touching the player

    [Header("Rewards")]
    public int scoreValue = 10;       // TODO (Week 5) - XP awarded when this enemy dies
}
```

**Explanation**:

- **`: ScriptableObject`** instead of `: MonoBehaviour`. It can't be attached to a GameObject, it exists as an asset.
- **`[CreateAssetMenu]`** is what puts it in the right-click `Create` menu. Without it you'd have no way to make one.
- The text in `menuName` is the **menu path**, with `/` separating the levels. Change it to whatever suits your project, just keep it consistent so all your SO types end up in one place rather than scattered through the Create menu.
- Notice the field is called **`startingHealth`**, not `health`. Its a meaningful name - it's the value an enemy _begins_ with, not the health it currently has. We'll see why that matters in the next step.
- `scoreValue` isn't used yet. I've put it in now because we'll need it later when we start tracking progression.

---

### **Step 3: Create the Two Enemy Variants**

Now we make the actual asset files.

1. **Right-click in the Enemy scripts folder** (or make a new folder called **Data**, which is tidier).
2. Go to **`Create > _CRE340 - Game Tutorial > Enemy Data`**.
3. Name it **Bad Guy - Green** and set it up in the Inspector:
    - **Enemy Name**: Bad Guy - Green
    - **Enemy Color**: green
    - **Starting Health**: 4
    - **Move Speed**: 3
    - **Contact Damage**: 5
    - **Score Value**: 10
4. **Create a second one** called **Bad Guy - Blue**:
    - **Enemy Name**: Bad Guy - Blue
    - **Enemy Color**: blue
    - **Starting Health**: 12
    - **Move Speed**: 1.5
    - **Contact Damage**: 15
    - **Score Value**: 25

You now have two asset files sitting in your project. Click on either one and you can edit it like any other asset.

**Explanation**:
- Green is a fast, weak enemy. Blue is slow and tanky. Two different things to fight from one scriptable object before we've started the code.
- That's the point of authored data. A designer or non-programmer can create and balance a dozen enemy types from here without opening a script. _Data driven design!_. Programmers are making the tools that allow designers or other disciplines to build the game without having to request hard changes to the codebase. Great stuff!

---

### **Step 4: Wire the `Enemy` Class to the Data**

This is the parts that's important. Replace `Enemy.cs` with the following:

```csharp
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable
{
    [Header("Enemy Data")]
    // The AUTHORED data - shared by every enemy using this asset.
    // Drag an EnemyData asset onto this in the Inspector.
    public EnemyData enemyData;

    // RUNTIME state - this enemy's own health, copied from the data when it spawns.
    // It is NOT public, because nothing should be setting it except TakeDamage.
    private int health;

    // Required by IDamagable
    public int Health
    {
        get { return health; }
    }

    private Material mat;
    private Color originalColor;
    private Transform playerTransform;

    private void Awake()
    {
        // Safety check - without data we can't set anything up
        if (enemyData == null)
        {
            Debug.LogError("Enemy on " + gameObject.name + " has no EnemyData assigned!");
            return;
        }

        // COPY the authored values into our own runtime state.
        // From here on we change our 'health', never 'enemyData.startingHealth'.
        health = enemyData.startingHealth;

        // Use the name from the data - this also tidies up the '(Clone)' we saw in Part 1
        gameObject.name = enemyData.enemyName;

        // Apply the colour from the data
        mat = GetComponent<Renderer>().material;
        mat.color = enemyData.enemyColor;
        originalColor = mat.color;
    }

    private void Start()
    {
        // Find the player once at the start rather than every frame.
        // We search for the COMPONENT - if something has PlayerStats, it's the player.
        PlayerStats player = FindFirstObjectByType<PlayerStats>();

        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        // Very simple chase - move straight towards the player at our data's speed
        if (playerTransform != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerTransform.position,
                enemyData.moveSpeed * Time.deltaTime
            );
        }
    }

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

    public void ShowHitEffect()
    {
        StartCoroutine(FlashColour(Color.white));
    }

    private IEnumerator FlashColour(Color flashColour)
    {
        mat.color = flashColour;

        yield return new WaitForSeconds(0.1f);

        mat.color = originalColor;
    }

    private void Die()
    {
        // TODO (Week 5) - award enemyData.scoreValue to the GameManager that we haven't built yet
        Destroy(gameObject);
    }
}
```

**Explanation**:
- **`health = enemyData.startingHealth;`** is the key line. We **copy** the authored value into our own private field. Every enemy gets its own `health` that it can change freely, while `enemyData` stays untouched.
- The flash colour is now white rather than red, since the enemy's own colour comes from the data and a red flash on a red enemy is hard to see.
- **`FindFirstObjectByType<PlayerStats>()`** in `Start` finds the player once and caches the result. Searching the whole scene is slow, so we don't want it in `Update`.
    - Notice we're asking for the **component**, not a tag. I'm not keen on tag strings and there's a better way - see the note below.
- The chase is about as simple as movement gets - it walks in a straight line towards you and ignores everything else. Its simple but it works.

> [!note] **Note - components instead of tags**
> 
> The usual way you'd see this written is `GameObject.FindWithTag("Player")`, and later on `other.CompareTag("Player")` to check if something is the player. We used a tag back in Week 2 for the item pickups. To me this is a mistake I would fix.
> 
> I've moved away from it here, and the reason isn't speed. `CompareTag` is actually fine performance-wise - it compares an internal ID rather than doing a string comparison. _(It's `gameObject.tag == "Player"` that's slow and allocates memory. People mix the two up or is not something you need to explore.)_
> 
> The problem with tags is that **the compiler can't check them**. Type `"player"` instead of `"Player"` and nothing goes red - it silently never finds anything, and you're left wondering why your enemies won't chase. Rename a tag in the Tag Manager and none of your code updates with it. They are quick and easy and useful a lot of times but not robust.
> 
> Asking for a component instead means the compiler is doing the work. **What makes something the player isn't a dropdown setting, it's the fact that it has a `PlayerStats` component on it.** Rename the class and every reference updates, or you get a compile error telling you where to look.

> [!note] **Note - when tags and layers ARE the right tool**
> 
> I'm not saying never use tags. They're built in, they need no setup, and for quick prototyping they're perfectly reasonable. You'll also see them everywhere in other people's code so you need to recognise them.
> 
> **Layers** are a different thing again and they're often what people use for tags to do. A layer decides _whether two things collide at all_, set up in the **Physics collision matrix** in Project Settings. If you want enemies to pass through each other instead of shoving each other around the arena, that's a layer job and it needs no code. Have a look at it - it's one of the more useful bits of Unity that people don't find for ages.
> 
> Rough split: **layers** for "should these two things interact", **components** for "what is this thing", **tags** for quick jobs where neither of those fits.

> [!note] **Note - `FindFirstObjectByType` is still not great**
> 
> It's better than a tag, but it's still searching the whole scene. With thirty enemies spawning that's thirty scene searches, and it only works because we do it once in `Start` rather than every frame.
> 
> The proper answer is for the player to **register itself** somewhere that anything can reach, so nothing has to go looking. That's the job of the **Singleton** in Week 5, so we'll come back and replace this line then.
> 
> _Also worth knowing - the older version of this method is `FindObjectOfType`, which is deprecated in current Unity. If you see it in a tutorial online, `FindFirstObjectByType` is the replacement and I just learned that too._

---

### **Step 5: Make the Enemy Prefab Variants**

The spawner uses an array of prefabs, so we need one prefab per enemy type.

1. **Open your `Enemy` prefab** and make sure the `Enemy` script is on it with **Enemy Data** empty for now.
2. **Drag the Enemy prefab into the scene** twice.
3. On the **first one**, drag **Bad Guy - Green** into the **Enemy Data** field. Rename the object **Enemy_Green**, then drag it back into the **Prefabs** folder to make a new prefab.
4. On the **second one**, drag **Bad Guy - Blue** into the field. Rename it **Enemy_Blue** and make a prefab the same way.
5. Delete both from the Hierarchy - the spawner will create them.
6. **Select the Enemy Spawner** and set **Prefabs To Spawn** to **Size 2**, with **Enemy_Green** in slot 0 and **Enemy_Blue** in slot 1.

Press Play. Green and blue enemies now spawn in a mix, move at different speeds, and take a different number of shots to kill.

> [!note] **Note - prefab plus data, rather than one prefab per enemy**
> 
> We've got two prefabs here, but they are identical except for one field. All the actual difference is in the data assets.
> 
> Adding a third enemy type is now: make a new `EnemyData`, duplicate a prefab, drag the data in, add it to the spawner array. No code at all.
> 
> _In Week 7 the **Factory Pattern** takes this further and removes the need for separate prefabs, by having one place that knows how to build each type._

---

### **Step 6: The Shared Data Demo (optional: have a go to see how the thing works)**

This is a few minute detour experiment that will demonstrate the shared data.

1. **Temporarily** change the `health -= damage;` line in `TakeDamage` to write to the data instead:

```csharp
    enemyData.startingHealth -= damage;   // DON'T leave this in - we're testing a point
```

2. Also change the `if (health <= 0)` check to `if (enemyData.startingHealth <= 0)`.
3. **Press Play** and shoot **one** green enemy a couple of times.
4. Watch the other green enemies. **They all lose health together**, and they all die at once.
5. **Stop playing.** Click on the **Bad Guy - Green** asset and look at its **Starting Health**.

It's been permanently changed. The asset file on disk is different now, and your next play session starts from the damaged value.

**Now put it back** the way it was in Step 4.

**Explanation**:
- Every enemy using that asset holds a reference to **the same object in memory**. There is one set of values between all of them.
- Changes to it in Play Mode stick, because it's a project asset and not a scene object. Unity doesn't reset it when you stop.
- That shared-ness is a **feature** when you use it correctly - edit the asset while the game is running to tune balance live, and every enemy updates instantly. Try bumping Move Speed mid-game and watch them all speed up.

> [!warning] **Note - the rule is simple enough**
> 
> **Read from the ScriptableObject, write to your own fields.**
> 
> We do the reading in one line in `Awake` and then never touch the data again. If you find yourself writing to an SO at runtime, stop and have a think about whether that value should have been runtime state all along.

---

### **Step 7: Make the Player Damageable**

The enemies are chasing us but can't do anything yet. Time to fix the hole in our game - health that only ever goes up.

We already have an interface for this. `IDamagable` was written in Week 3 for crates, and the player is about as unrelated to a crate - which is the point of an interface.

Open `PlayerStats.cs` and update it:

```csharp
using System.Collections;
using UnityEngine;

// Now implements IDamagable - the same interface the crates and enemies use
public class PlayerStats : MonoBehaviour, IDamagable
{
    // An INSTANCE of our plain PlayerData class - runtime state, this player's own numbers
    [SerializeField] private PlayerData data = new PlayerData();

    // A public way for other scripts to read the data (but not replace it)
    public PlayerData Data
    {
        get { return data; }
    }

    // Required by IDamagable - our health lives inside PlayerData
    public int Health
    {
        get { return data.currentHealth; }
    }

    private Material mat;
    private Color originalColor;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;
    }

    // Add health, but never go above maxHealth
    public void RestoreHealth(int amount)
    {
        data.currentHealth = data.currentHealth + amount;
        data.currentHealth = Mathf.Clamp(data.currentHealth, 0, data.maxHealth);

        Debug.Log("Player health is now " + data.currentHealth + " / " + data.maxHealth);
    }

    // Add mana, but never go above maxMana
    public void RestoreMana(int amount)
    {
        data.currentMana = data.currentMana + amount;
        data.currentMana = Mathf.Clamp(data.currentMana, 0, data.maxMana);

        Debug.Log("Player mana is now " + data.currentMana + " / " + data.maxMana);
    }

    // Required by IDamagable - take damage the same way anything else in the game does
    public void TakeDamage(int damage)
    {
        data.currentHealth = data.currentHealth - damage;
        data.currentHealth = Mathf.Clamp(data.currentHealth, 0, data.maxHealth);

        // Broadcast it, exactly like the crates and enemies do.
        // The event logger picks this up without us doing anything else.
        if (HealthEventManager.OnObjectDamaged != null)
        {
            HealthEventManager.OnObjectDamaged(gameObject.name, data.currentHealth);
        }

        if (data.currentHealth <= 0)
        {
            Die();
        }
    }

    // Required by IDamagable
    public void ShowHitEffect()
    {
        StartCoroutine(FlashColour(Color.red));
    }

    private IEnumerator FlashColour(Color flashColour)
    {
        mat.color = flashColour;

        yield return new WaitForSeconds(0.1f);

        mat.color = originalColor;
    }

    private void Die()
    {
        if (HealthEventManager.OnObjectDestroyed != null)
        {
            HealthEventManager.OnObjectDestroyed(gameObject.name, data.currentHealth);
        }

        Debug.Log("Player died!");

        // TODO (Week 5) - the GameManager will handle game over and restarting
    }
}
```

**Explanation**:

- **One line is most of the work** - `: IDamagable` on the class declaration. The compiler then told us the three members we needed to enforce the contract.
- The player's `Health` property reads from `PlayerData` rather than a separate field. **Still one owner for the number**, which is the thing we've been careful about since Week 2.
- Because the player now fires `HealthEventManager` events, **it turns up in the event log automatically**. We didn't change the logger at all.

> [!note] **Note - look at what the interface just added**
> 
> `IDamagable` was written in Week 3 for crates and barrels. We've now applied it to the player, a completely different kind of object with a completely different internal structure - its health is nested two layers deep inside a data class.
> 
> The interface didn't care. It only  asked for a way to read health and a method to call. _That_ is the difference between "what is this thing" and "what can this thing do".

---

### **Step 8: Give the Enemy Contact Damage**

Last piece. Add this to `Enemy.cs`:

```csharp
    [Header("Contact Damage")]
    public float damageCooldown = 1f;      // seconds between hits while touching
    private float lastDamageTime = -100f;  // start in the past so the first hit lands

    // OnCollisionStay fires every frame while two objects are touching
    private void OnCollisionStay(Collision collision)
    {
        // Cooldown first - it's the cheapest check and it fails most of the time
        if (Time.time < lastDamageTime + damageCooldown)
        {
            return;
        }

        // Is this the player? We ask for the COMPONENT rather than checking a tag.
        // If it has PlayerStats then it's the player - nothing else in the game has one.
        if (collision.gameObject.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            playerStats.TakeDamage(enemyData.contactDamage);
            playerStats.ShowHitEffect();

            lastDamageTime = Time.time;
        }
    }
```

**Explanation**:

- **`OnCollisionStay`** keeps firing while the two objects are in contact, unlike `OnCollisionEnter` which only fires on the first touch. Without it an enemy could sit on top of you doing nothing after the first hit.
- **The cooldown** is the third time we've used this pattern now - dash in Week 1, shooting in Week 3, and here. Store when you last did something, refuse to do it again until enough time has passed.
- **Checking the cheap condition first** is a decent habit. Most frames the cooldown hasn't elapsed, so we exit before doing anything more expensive.
- We're asking for **`PlayerStats`** rather than `IDamagable` here, and that's on purpose. The enemy's rule isn't "hurt anything damageable" - if it was, they'd smash crates just by walking into them. The rule is "hurt the player", so we ask for the thing that only the player has.
	- You could do this with conditions or by other means but we know the unique component on the player.

> [!note] **Note - the Bullet and the Enemy want different things**
> 
> The `Bullet` asks for `IDamagable` because a bullet should damage anything it hits - crate, enemy, barrel, whatever we add later..
> 
> The `Enemy` asks for `PlayerStats` because it only ever attacks one thing.
> 
> Same collision check, different question, because they have different rules. Use the interface when you mean "anything that can do this", and the concrete type when you mean "that specific thing". _Take the tag check out of the old version and watch enemies demolish the crates - also, you might want everything damaging everything or use it to your advantage if its more of a chaos thing happening.

#### Small job - tidy up my mess from Week 2 while you're here

Open `Item.cs` from Week 2 and look at `OnTriggerEnter`:

```csharp
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();

            if (playerStats != null)
            {
                Use(playerStats);
                Destroy(this.gameObject);
            }
        }
    }
```

The tag check isn't doing anything. The `GetComponent<PlayerStats>()` on the next line is the gate - if it comes back null we don't collect the item anyway. So the tag is just an extra condition that happens to agree with the one underneath it.

So we can delete the tag check and leave everything else alone:

```csharp
    private void OnTriggerEnter(Collider other)
    {
        // Try to get the stats component - if it comes back null then it wasn't the player
        PlayerStats playerStats = other.GetComponent<PlayerStats>();

        if (playerStats != null)
        {
            Use(playerStats);
            Destroy(this.gameObject);
        }
    }
```

Same behaviour with one less condition, one less named string, and a level of nesting removed. **Deleting code redundant code is a good thing and it's most of what refactoring is.**

---

### **Step 9: Test the Whole Loop**

1. **Press Play** and let it run.
2. **Green enemies** should be quick and fragile, **blue** slow and tough.
3. **Let one touch you** - you flash red, lose health, and the event log reports it alongside everything else.
4. **Grab a Health Potion** to top yourself back up.
5. **Let your health hit zero** - the Console reports the player died. Nothing else happens yet. We'll get to that in a later week.

**You now have a simple game loop but a nice structure to it**:

> Enemies spawn - they chase you - they damage you - you shoot them - you collect potions to heal - repeat

Everything from here is about making that cleaner and deeper rather than making it work.

6. **Tune it** while playing - `Contact Damage` and `Move Speed` on the data assets, `Max Alive` and intervals on the spawner. It'll be badly balanced to start with but we can develop the code and the feel as we go.

> [!note] **Note - the enemies will shove each other about**
> 
> They all walk straight at you with no awareness of each other, so they may bunch up and push each another around. I'm calling it a feature'
> 
> This is the **layers** thing from the earlier note - put the enemies on their own layer and untick Enemy/Enemy in the Physics collision matrix, and they'll pass through each other. No code. Worth trying just to see how much that tickbox changes things. They'll merge into overlap.

---

### **What we've done**:

- Built an **`EnemyData` ScriptableObject** holding authored enemy stats.
- Made **two enemy variants** that play differently, without writing type-specific code.
- Learned the **authored data vs runtime state** distinction, and seen what happens when you get it wrong.
- Swapped **tag strings for component checks**, and looked at where layers fit in.
- Applied the **`IDamagable`** interface from Week 3 to the player.
- Closed the game loop with chasing, damaging enemies.

### **Week 4 Summary - Data**

Four parts, all about how data is structured and moved around:
1. **Extending an event** - what data gets passed between objects, and what it costs to change it.
2. **The event logger** - a `List<string>` holding something that changes constantly, and a rename refactor.
3. **Arrays and Lists** - fixed sets versus runtime collections, and one spawner instead of two.
4. **ScriptableObjects** - authored data in its own asset, shared and separate from runtime state.

The theme through all of this is **who owns a piece of data and how long it lives?**. An event parameter exists for a single call. A List lives as long as its component. A ScriptableObject outlives the game session and exists in the project.

That question comes back in **Week 9**, when we save the game. The answer there is a direct consequence of this week - you save `PlayerData` because it's runtime state that changed, and you don't save `EnemyData` because it's authored content that's already a file.

### **Next week**:

**Week 5** has the class test in the morning, so the practical is light. We'll build a proper **Singleton** `GameManager` to track score and game state, which also gives us somewhere for the `scoreValue` TODO to be worked on - and a better way for the enemies to find the player than searching the whole scene.

We'll also do some extra minor extension of events probably ongoing - Its easy and cheap to expand the event system
- Log player health and item events, pickups etc..
	- In our game we have a logger and all the entities report to it. Its a feature you can use in lots of different games.

We may also refactor - this is an ongoing task in development as you gain hind sight
- `PlayerStats` ---> refactor to `Player`
	- So our Player would become a `PlayerData` C# class and a `Player` script that uses the `PlayerData` class, inherits from MonoBehaviour and implements `IDamagable` interface
