---
Order: 4-C
---
# **Week 4 - Part 3 : Arrays, Lists and the Spawner**

### Overview

#### **What we are building**:
- We'll look at **Arrays** and **Lists** properly - what each one is for and how to choose between them.
- Then we'll use both to build a single **`Spawner`** class that replaces the `ItemSpawner` we wrote in Week 2.
- The same script will handle spawning items _and_ spawning enemies, just configured differently in the Inspector.

#### **Why we are doing this**:
- We've ended up with two spawners doing nearly the same job - the one from Week 2 and the one we'd need for enemies. Merging them into one is good practice and it's a realistic job you'd do.
- Arrays and Lists are the two collections you'll use constantly, and a spawner needs both for different reasons. It's a good example because the reason for each one is obvious once you see it.
- The game also needs enemies turning up over time rather than sitting where we placed them. We've been shooting stationary targets.

#### **Learning Objectives**:
- **Arrays** - fixed size, set at author time.
- **Lists** - grow and shrink while the game runs.
- **Choosing between them** based on what the data actually does.
- **Refactoring** - merging two similar classes into one that covers both.
- **Removing from a list safely** - and why you loop backwards.

---
---

# **Part 3: One Spawner for Everything**

Carry on in the same scene. You should have the `ItemSpawner` from Week 2 in there spawning potions.

---

### **Step 1: Arrays and Lists (read before coding)**

Both hold a collection of things. The difference is whether the size can change.

**An Array** is a fixed-size block. You decide how big it is and that's that.

```csharp
    public GameObject[] prefabsToSpawn;   // the [] makes it an array
```

In the Inspector an array shows up with a **Size** field. You type in how many slots you want and fill them in. Once the game is running, that size doesn't change. 

**A List** can grow and shrink any time.

```csharp
    private List<GameObject> spawnedObjects = new List<GameObject>();
```

You `Add()` to it, `RemoveAt()` from it, and ask it for its `Count`. We used one in Part 2 to hold the log lines.

**So which do you use?** Ask what the data is actually doing:

|Comparing|Array|List|
|---|---|---|
|Size|Fixed when created|Changes while running|
|Set by|Usually you, in the editor|Usually the code, at runtime|
|Good for|A set of options you chose|Things that currently exist|
|In our spawner|Which prefabs can spawn|Which objects have spawned|

That last row above is the whole point. The **prefabs we're allowed to spawn** is a decision we make in the editor and it never changes while playing - that's an array. The **objects currently alive in the scene** changes constantly - that's a list.

> [!note] **Note - you can mostly use a List for everything and get away with it.**
> 
> Lists are more flexible and in a small project you'd rarely notice a difference. Arrays are slightly faster and use slightly less memory, but that's not really the point at this scale.
> 
> The reason to use an array is that it **says something about the data**. When another programmer sees `GameObject[]` they know that set is fixed. When they see a `List` they know it's going to change. The type is telling them something before they read any of your code.

---

### **Step 2: The Spawners We Already Have**

Open `ItemSpawner.cs` from Week 2 and look at it again.

```csharp
    public GameObject healthPotionPrefab;
    public GameObject manaPotionPrefab;
```

Two named fields, one per prefab type. It works, but think about adding a third potion - you'd add a third field, a third `SpawnRow` call, and edit the class every time.

We're also about to need something that spawns enemies. If we wrote that as a separate class it would be nearly identical.

So rather than write a second one, we'll write a single `Spawner` that covers both. An array of prefabs means it doesn't care whether it's spawning potions, enemies or anything else we invent later.

> [!info]
> Again, this is my 'mistake' but its really more of a workflow practice. Programming involves decisions and some backwards steps to simplify and unify systems. 
> 
> If I start to need a duplicate class to do a similar thing to some other class then I need to think about merging or making a generic system


---
### **Step 3: Create the `Spawner` Script**

1. **Create a C# script called `Spawner.cs`** in the Scripts folder.

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A general purpose spawner.
// The same script handles items, enemies, or anything else - we just configure it differently.
public class Spawner : MonoBehaviour
{
    [Header("What to spawn")]
    // ARRAY - the set of prefabs this spawner is allowed to use.
    // We decide this in the editor and it doesn't change while the game runs.
    public GameObject[] prefabsToSpawn;

    [Header("Where to spawn")]
    public Vector3 spawnArea = new Vector3(20f, 0f, 20f);  // width, height, depth
    public float spawnHeight = 0.5f;                       // how high off the floor

    [Header("Spawn at the start")]
    public bool spawnOnStart = true;
    public int startingAmount = 5;

    [Header("Keep spawning")]
    public bool spawnContinuously = false;
    public float minSpawnInterval = 2f;
    public float maxSpawnInterval = 5f;
    public int maxAlive = 20;    // stop spawning once we hit this many

    // LIST - everything this spawner has created and that is still alive.
    // This changes constantly while the game runs, so it has to be a List.
    [SerializeField] // so we can see in the editor
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        // Spawn a batch straight away
        if (spawnOnStart)
        {
            for (int i = 0; i < startingAmount; i++)
            {
                SpawnOne();
            }
        }

        // Start the loop that keeps spawning over time
        if (spawnContinuously)
        {
            StartCoroutine(SpawnLoop());
        }
    }

    // Coroutine that spawns forever at random intervals
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Wait a random amount of time before the next spawn
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // Tidy the list before we check how many are alive
            CleanUpList();

            if (spawnedObjects.Count < maxAlive)
            {
                SpawnOne();
            }
        }
    }

    // Spawn a single random prefab from the array
    private void SpawnOne()
    {
        // Nothing assigned in the Inspector, so there's nothing we can do
        if (prefabsToSpawn.Length == 0)
        {
            Debug.LogWarning("Spawner on " + gameObject.name + " has no prefabs assigned!");
            return;
        }

        // Pick a random slot from the array.
        // Random.Range with ints is EXCLUSIVE of the upper number, so Length is correct here.
        int randomIndex = Random.Range(0, prefabsToSpawn.Length);
        GameObject prefabToSpawn = prefabsToSpawn[randomIndex];

        // Pick a random position inside the spawn area
        Vector3 randomPosition = new Vector3(
            Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
            spawnHeight,
            Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
        );

        // Offset by this spawner's own position so the area follows the object
        randomPosition = randomPosition + transform.position;

        // Create it and remember it
        GameObject newObject = Instantiate(prefabToSpawn, randomPosition, Quaternion.identity);
        spawnedObjects.Add(newObject);
    }

    // Remove anything from the list that has been destroyed
    private void CleanUpList()
    {
        // Loop BACKWARDS when removing from a list - see the note below for why
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
            }
        }
    }

    // Handy for debugging - how many of our objects are still alive
    public int GetAliveCount()
    {
        CleanUpList();
        return spawnedObjects.Count;
    }

    // Draw the spawn area in the editor so we can see it
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, spawnArea);
    }
}
```

**Explanation**:

- **The array** (`prefabsToSpawn`) is the set of options. `prefabsToSpawn.Length` tells us how many slots there are, and `prefabsToSpawn[randomIndex]` gets one out.
- **The list** (`spawnedObjects`) tracks what we've made. `Add()` puts things in, `RemoveAt()` takes them out, `Count` tells us how many.
- **`[Header("...")]`** groups fields in the Inspector. Purely cosmetic but it makes a component with this many settings much easier to use.
- The spawn position is offset by `transform.position` so you can move the spawner object around and the area moves with it.

> [!warning] **Note - why `CleanUpList` loops backwards.**
> 
> This one catches people out.
> 
> When you remove an item from a list, everything after it shifts down one position. If you're looping forwards and you remove item 3, what _was_ item 4 becomes item 3 - and your loop has already moved on to position 4. You skipped one.
> 
> Looping backwards avoids it completely, because the items you've already checked are the ones above the one you removed, and they're the only ones that move.
> 
> Try changing it to a forward loop and see null entries survive. Its a thing with loops they can be hard to think through mentally, hard to debug and easy to make a mistake that gets null entries.

> [!note] **Note - a coroutine instead of `InvokeRepeating`.**
> 
> You'll see a lot of Unity tutorials use `InvokeRepeating("SpawnRandomObject", 2f, 5f)` for this. It works, but it's the same string-based method naming we've avoided previously - rename the method and it breaks but doesn't log to the console.
> 
> The coroutine does the same job, it's checked by the compiler, and the `while (true)` loop with a `yield return new WaitForSeconds()` is a pattern you'll use constantly for anything on a timer. Timing, VFX, Audio and sequence of events can all be done by `Coroutine` 

---

### **Step 4: Replace the Item Spawner**

Now we swap the old spawner out for the new one.

1. **Select the `ItemSpawner` object** in the Hierarchy and rename it to **Item Spawner** (or leave it, up to you).
2. **Remove the `Item Spawner` component** from it.
3. **Add the new `Spawner` component.**
4. **Set it up**:
    - **Prefabs To Spawn** - set **Size** to 2, then drag **HealthPotion** into slot 0 and **ManaPotion** into slot 1.
    - **Spawn Area** - something like 20, 0, 20 to cover the arena.
    - **Spawn On Start** ticked, **Starting Amount** 8.
    - **Spawn Continuously** ticked, intervals 4 and 8, **Max Alive** 10.
5. **Delete `ItemSpawner.cs`** from the project. It's been replaced.

Press Play - potions scatter around the arena at the start, and more trickle in as you collect them.

> [!note] **Note - deleting code you wrote two weeks ago is normal.**
> 
> It can feel like wasted effort but its not. `ItemSpawner` taught us instantiating and `List<Item>`, and that knowledge is helps us write the better version. Code gets replaced constantly in real projects as you understand the problem better. 
> 
> _If you'd rather keep it for reference, move it to a '**Scrap**' folder rather than deleting - just don't leave two spawners in the project doing the same job or one that is there but never going to be used._

---

### **Step 5: Add an Enemy Spawner**

Here's the payoff - the same script, configured completely differently.

1. **Create an empty GameObject** called **Enemy Spawner**. Place it at the origin.
2. **Add the `Spawner` component.**
3. **Set it up**:
    - **Prefabs To Spawn** - **Size** 1, drag the **Enemy** prefab into slot 0.
    - **Spawn Area** - a bit bigger than the item one - we'll move to a bigger environment at some stage and be able to use spanners of different sizes in different areas
    - **Spawn On Start** ticked, **Starting Amount** 3.
    - **Spawn Continuously** ticked, intervals 3 and 6, **Max Alive** 15.

Press Play. Enemies now arrive steadily while you're shooting, and the count tops out at 15 so it doesn't get out of hand.

**Explanation**:
- **One script, two components with completely different behaviour.** The difference is in the Inspector values rather than in code.
- This is the first example of **data driving behaviour**. The `Spawner` class describes _how_ to spawn; the values on each instance describe _what_ to spawn and _when_.
- Part 4 takes that idea further with `ScriptableObjects`, where the data exists in its own asset file rather than on the component.

> [!note] **Note - you can keep adding spawners now, for free.**
> 
> Want a spawner that only produces exploding crates in one corner? Add another GameObject, add the component, fill in the array. No new code.
> 
> That's the thing to notice about this sort of refactor. We didn't just merge two classes to tidy up, we ended up with something more capable than either of the originals and we can use in a different project when we need it. Its a generic spawner.

---

### **Step 6: Test and Tune**

1. **Press Play** and let it run for a minute.
2. Shoot enemies and collect potions - both spawners should keep topping things up.
3. **Tune the numbers** until it feels about right:
    - **Max Alive** on the enemy spawner is the difficulty dial.
    - Short intervals make it frantic, long intervals make it sparse.
    - Too many potions and there's no pressure, too few and it's potentially difficult. 
    - *Note: We are missing player damage and meaningful enemy behaviour but we can get to that later.*

> [!note] **Note - this is game design happening in the Inspector.**
> 
> Nothing in the code decides whether the game is too hard. Those numbers do, and you can change them while the game is running to find what feels right.
> 
> That's worth bearing in mind generally - the more of your tuning that lives in the Inspector rather than hard-coded in a script, the faster you can iterate on how your game actually feels.

---

### **What we've done**:
- Looked at **Arrays** (fixed set of options) and **Lists** (things that change at runtime), and where each fits.
- **Merged two spawners into one** that handles items, enemies, or anything else.
- Used the same script twice with different Inspector settings to get two different behaviours.
- Learned why you loop backwards when removing from a list. We have an example of loops can work in different ways.

### **Where we are**:
The game now has a proper flow to it - enemies arriving, items to collect, things to shoot. Two things are still missing though:
- Every enemy is identical. Same health and everything else
- Nothing can actually hurt you. Your health only ever goes up.

**Part 4** we will deal with both, using **ScriptableObjects** to give enemies variety.

### Scriptable Objects are dead handy data assets Unity uses. They can be tricky to start with but extremely useful. They work similar to a data classes but are actual data assets you can use in your project.
