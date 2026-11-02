---
Order: 7-A
---
# **Week 7 : The Factory Pattern**

### Overview

#### **What we are building**:

- An **`EnemyFactory`** - one place that knows how to build an enemy from an `EnemyData` asset.
- Collapsing **Enemy_Green** and **Enemy_Blue** back into a **single Enemy prefab**.
- A copy of the Week 4 spawner that works from data rather than prefabs.

#### **Why we are doing this**:

- We've got two prefabs that are identical apart from one field. I flagged that in Week 4 as something the Factory would fix.
- Adding an enemy type currently means a new data asset, a new prefab, and remembering to drag it into the spawner. It should only mean a new data asset.
- Creating an object is more than calling `Instantiate` - there's setup to do afterwards. A Factory does the setup so it gets written once.

#### **Learning Objectives**:

- **The Factory Pattern** - centralising creation so callers don't have to know how.
- **Initialisation order** - why `Awake` is too early to receive data from a factory.

---

---

# **Building Enemies from Data**

Carry on from Week 6. This is a light work week - four scripts being modified and most of it is moving code you already have.

---

### **Step 1: What can be Improved in the Current Setup**

Adding a third enemy type right now means:

1. Create a new `EnemyData` asset.
2. Duplicate the Enemy prefab, drag the new data onto it, rename it.
3. Raise the spawner's array size and drag the new prefab in.

Open `Enemy_Green` and `Enemy_Blue` and compare them. They have the same components, settings, but one field different. Steps 2 and 3 are extra work caused by the data being part of the prefab. Not a lot of work but the point is to write architecture for extendable projects.

**What we want:** one Enemy prefab, many data assets. Something has to **make** the enemy and **tell it** which type it is, and that something is a Factory.

---

### **Step 2: Give `EnemyData` a Prefab**

**In `EnemyData.cs`**, add one field:

```csharp
    [Header("Prefab")]
    public GameObject enemyPrefab;    // the prefab this enemy type is built from
```

On **both** data assets - Bad Guy - Green and Bad Guy - Blue - drag the **same** `Enemy` prefab into that field.

Both types now point at one prefab, and everything that makes them different is in the data. It's a field rather than something fixed inside the factory so you can give a boss its own mesh later without changing any code.

---

### **Step 3: Move Setup Out of `Awake`**

This is the part is important when making a factory.

`Enemy.Awake` reads `enemyData` and sets the enemy up from it. That works when the data exists on the prefab, and stops working when a factory is involved:

```csharp
GameObject instance = Instantiate(prefab, position, rotation);   // Awake runs HERE
Enemy enemy = instance.GetComponent<Enemy>();
enemy.enemyData = data;                                          // too late, Awake already ran
```

**`Awake` runs during `Instantiate`.** By the time the factory can assign anything, setup has already happened with nothing to work from.

**In `Enemy.cs`**, replace `Awake` with this:

```csharp
    private void Awake()
    {
        // An enemy placed in the scene by hand already has its data, so set up now. means we can have enemies in the scene already or instantiate them
        // Enemies made by the factory arrive and get Initialise called instead.
        if (enemyData != null)
        {
            Initialise(enemyData);
        }
    }

    // Everything that used to be in Awake. The factory calls this after creating us.
    public void Initialise(EnemyData data)
    {
        enemyData = data;

        health = enemyData.startingHealth;
        gameObject.name = enemyData.enemyName;

        mat = GetComponent<Renderer>().material;
        mat.color = enemyData.enemyColor;
        originalColor = mat.color;
    }
```

**Explanation**:

- `Initialise` is the same code that was in `Awake`, just somewhere it can be called on demand.
- The null check means both routes work - hand-placed enemies set themselves up, factory-made ones get told.
- The general point: **`Instantiate` gives you an object, not a configured one.** Anything passed in has to happen after, so the object needs a way to receive it.

---

### **Step 4: The Factory**

1. **Create a C# script called `EnemyFactory.cs`** in the Enemy scripts folder. Delete the Unity boilerplate - this isn't a MonoBehaviour and goes on no GameObject.

```csharp
using UnityEngine;

// One place that knows how to build an enemy.
// Callers hand it a data asset and a position, and get a working enemy back.
public static class EnemyFactory
{
    public static Enemy CreateEnemy(EnemyData data, Vector3 position)
    {
        if (data == null || data.enemyPrefab == null)
        {
            Debug.LogError("EnemyFactory is missing data or a prefab.");
            return null;
        }

        // Make the object
        GameObject instance = Object.Instantiate(data.enemyPrefab, position, Quaternion.identity);

        Enemy enemy = instance.GetComponent<Enemy>();

        if (enemy == null)
        {
            Debug.LogError("The prefab on " + data.name + " has no Enemy component.");
            Object.Destroy(instance);
            return null;
        }

        // Tell it what it is
        enemy.Initialise(data);

        return enemy;
    }
}
```

**Explanation**:

- **`static`** because there's no state to keep. We call `EnemyFactory.CreateEnemy(...)` rather than ever writing `new EnemyFactory()`.
- **`Object.Instantiate`** with the prefix, because a static class isn't a MonoBehaviour and doesn't get Unity's shorthand version.
- **Two steps that are always in the same order** - make it, then initialise it. The point of the factory is that this sequence is written once.

> [!note] **Note - what a Factory actually does**
> 
> Right now it's `Instantiate` plus one method call, and you'd be right thinking whether it needs a class of its own
> 
> The reason is for what the game might potentially do. Creating an enemy will grow - a difficulty multiplier, registering with a wave manager, picking a spawn effect. Each of those goes **inside the factory** and nothing that calls it changes. If we had an ambitious game with lots of diverse spawning we'd do that at the factory rather than the spawner.
> 
> Week 10 we'll do some pooling an optimisation that demonstrates the purpose. A pooled object isn't created, it's fetched from a pile and reset - a completely different way of producing an enemy. With a factory that's a change to one method. Without one it's a change everywhere an enemy is made.

---

### **Step 5: Point the Spawner at Data**

The existing `Spawner` works from a `GameObject[]` of prefabs, which is still right for the items. Enemies now come from data, so they get their own version.

**Create a C# script called `EnemySpawner.cs`** and paste in the following.

It's the Week 4 `Spawner` with four things changed:

- The **array** holds `EnemyData` instead of `GameObject`
- The **list** holds `Enemy` instead of `GameObject`
- **`SpawnOne`** asks the factory instead of calling `Instantiate`
- Every `spawnedObjects` is now `spawnedEnemies`

Everything else - spawn area, intervals, `maxAlive`, the game over check, the backwards loop, the gizmo - is untouched.

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawns enemies at intervals using the factory.
// Same shape as Spawner, but it works from EnemyData assets rather than prefabs.
public class EnemySpawner : MonoBehaviour
{
    [Header("What to spawn")]
    public EnemyData[] enemyTypes;      // data assets, not prefabs

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

    // LIST - every enemy this spawner has created and that is still alive
    [SerializeField]
    private List<Enemy> spawnedEnemies = new List<Enemy>();

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

            // Stop spawning once the game is over
            if (GameManager.Instance.IsGameOver)
            {
                yield break;   // ends this coroutine completely
            }

            // Tidy the list before we check how many are alive
            CleanUpList();

            if (spawnedEnemies.Count < maxAlive)
            {
                SpawnOne();
            }
        }
    }

    // Spawn a single random enemy type from the array
    private void SpawnOne()
    {
        // Nothing assigned in the Inspector, so there's nothing we can do
        if (enemyTypes.Length == 0)
        {
            Debug.LogWarning("EnemySpawner on " + gameObject.name + " has no enemy types assigned!");
            return;
        }

        // Pick a random slot from the array
        int randomIndex = Random.Range(0, enemyTypes.Length);
        EnemyData chosenType = enemyTypes[randomIndex];

        // Pick a random position inside the spawn area
        Vector3 randomPosition = new Vector3(
            Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
            spawnHeight,
            Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
        );

        // Offset by this spawner's own position so the area follows the object
        randomPosition = randomPosition + transform.position;

        // Ask the factory for an enemy. We don't know or care how it makes one.
        Enemy newEnemy = EnemyFactory.CreateEnemy(chosenType, randomPosition);

        if (newEnemy != null)
        {
            spawnedEnemies.Add(newEnemy);
        }
    }

    // Remove anything from the list that has been destroyed
    private void CleanUpList()
    {
        // Loop BACKWARDS when removing from a list
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    // Handy for debugging - how many of our enemies are still alive
    public int GetAliveCount()
    {
        CleanUpList();
        return spawnedEnemies.Count;
    }

    // Draw the spawn area in the editor so we can see it
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, spawnArea);
    }
}
```

**Explanation**:

- **You now have two spawner scripts.** `Spawner` for the items, working from prefabs exactly as it did. `EnemySpawner` for the enemies, working from data.
- Everything except the type of the array, the type of the list, and `SpawnOne` is identical to the original spawner. That was all written in Week 4 and does a simple spawning job
- **The only real difference is the input.** Prefabs became data, and one `Instantiate` became one call to the factory.
- `EnemyFactory.CreateEnemy` is now the only line that knows an enemy gets made at all.
- The gizmo is red rather than green so you can tell the two spawn areas apart in the Scene view. 
	- Handy stuff the gizmos!

> [!note] **Note - didn't we merge the spawners already**
> 
> Yes. The two we merged were doing the same job - take a prefab, put a copy somewhere.
> 
> This one takes a data asset and asks a factory to produce something. The idea and structure are the same but the input is different.
> 
> Worth noticing the two classes are now mostly identical though, and if you added a third you'd want to do something about it - a shared base class with the spawning logic in it, and each one overriding how it makes the thing. We're not doing it for two, but it's the potential next move if it were to grow.


---

### **Step 6: Set It Up**

1. **Delete `Enemy_Green` and `Enemy_Blue`** from the Prefabs folder. One `Enemy` prefab is enough now.
2. **Open the `Enemy` prefab** and **clear the Enemy Data field**. The factory fills it in. Leave everything else alone - collider, rigidbody, `SpawnPop`.
3. **Select the Enemy Spawner** object.
    - Remove the `Spawner` component, `Add Component -> Enemy Spawner`.
    - Set **Enemy Types** to Size 2 and drag in **Bad Guy - Green** and **Bad Guy - Blue**. Data assets, not prefabs.
    - Put the spawn area and intervals back to what you had.
4. **Leave the Item Spawner alone.** It still uses `Spawner` and still works from prefabs.

**Press Play.** Everything behaves exactly as it did. Only how enemies get made has changed.

---

### **Step 7: The Test**

The point of the week is what a third enemy type requires now.

1. **Create -> CRE340 - Game Tutorial -> Enemy Data.** Name it **Bad Guy - Purple** - purple, 8 health, speed 4, damage 10, XP 15.
2. Drag the **Enemy** prefab into its **Enemy Prefab** field.
3. On the Enemy Spawner, raise **Enemy Types** to Size 3 and drag it in.

**Press Play.** Three enemy types with no new prefab, new script, or change to the factory or the spawner.

---

### **Where this is going**

**Week 8 - the State Pattern.** Our enemies have one behaviour, a straight line at the player in `Update`. Next week they get states and the ability to switch between them. This week was variety in **stats**, next week is variety in **behaviour**.

**Week 10 - pooling.** A pool reuses dead objects instead of destroying and recreating them. Because the factory exists, it's the only file that would need to know.

> [!note] **Note - what we didn't do**
> 
> A factory often returns an **abstract base type** so it can produce genuinely different classes behind one method. We've got one `Enemy` class with all variety coming from ScriptableObjects, so a base class with a single subclass under it would be an empty layer.
> 
> The question to keep asking with patterns: **what does this let me do that I can't do now?** If the answer is nothing yet, then its not required.
> 
> For your own understanding you can read up on abstract factory. https://refactoring.guru/design-patterns/abstract-factory

---

### **What we've set up**:

- An **`EnemyFactory`** - one place that knows how to build an enemy.
- **One Enemy prefab** instead of one per type.
- An **`EnemySpawner`** holding data assets and no prefabs.
- An `Enemy` that can be configured after creation rather than only from the Inspector.