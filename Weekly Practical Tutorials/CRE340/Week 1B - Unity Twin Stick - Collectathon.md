---
Order: 1-B
---
### **Collectathon Mini-Game - Part 2 Overview**

#### **What we are building**:

- This is **Part 2** of Week 1. We take the **Player** from Part 1 and build a small game around it.
- We scatter **collectibles** around the arena, let the player **collect** them, and track **score** and a **countdown timer** on the UI.
- We add **win / lose** conditions - collect everything to win, run out of time to lose.

#### **Learning Objectives**:

- **Prefabs & Spawning**: Instantiate a prefab many times from a script.
- **Triggers & Collisions**: Detect the player picking something up with `OnTriggerEnter`.
- **Game State**: Track score, a timer, and win / lose conditions in one place.
- **UI**: Update TextMeshPro text from code.

** **Note - *READ THIS FIRST. This mini-game is a deliberately HACKY refresher on some simple Unity and C# ideas - prefabs, triggers, a game loop, UI text. To keep it short we use a 'lazy singleton' and let one GameManager do everything. That is NOT generally how we'll build things going forward which is what we want to understand across the module. What are the better approaches and structures we can use?***


---
---

# **Part 2: Building the Collectathon Mini-Game**

This carries on directly from **Part 1**, in the **same project** and the **same scene** (_Scene-Week1_TwinStick-Collectathon_). You should already have a **Player** prefab that moves, aims (aim is not important right now), and dashes.

We'll build the collectible and its spawner, then the GameManager that ties the game together.

---

### **Step 1: Create the Collectible and make it a Prefab**

First we make the thing the player collects. We build it once and save it as a **Prefab** so the spawner can make as many as it likes - and so we can reuse it in later weeks.

1. **Create the object**:
    - In the Project folder search for the prefab '*PickupPrototype-HalfSize*' - Its in '`_SharedAssets/Prototyping/Environment/Prefabs`' folder
    - Drag it into the hierarchy
    - Rename it to `Collectable-Coin`
    - Notice its scale (e.g. 0.5, 0.5, 0.5) 
    - Set its position is (0,0,0) - good practice to start at Vector3 Zero
    - Its has a material which you can swap out for a different colour - `_YellowSmooth`
    - Notice it has a *Mesh Collider* which you can keep or change to a *Sphere Collider* or *Box Collider* - I'll go with a box for efficiency
2. **Make its collider a Trigger**:
    - The Sphere already has a **Collider**. In the Inspector, tick **Is Trigger**.
    - A trigger lets the player pass _through_ it (instead of bumping it like a wall) and fires the `OnTriggerEnter` message we'll use to detect the pickup.

** **Note - Triggers need a Rigidbody in the mix to fire. Our Player has one (we added it in Part 1) - the Player moving into the Collectible's trigger is what raises the event.**

1. **Save it as a new Prefab**:
    - Open _Assets / _CRE340 - Game Tutorial / Prefabs_.
    - Drag the **Collectible-Coin** from the Hierarchy into the **Prefabs** folder to create the prefab.
        - Make it an `Original Prefab` 
    - Delete the Collectible from the Hierarchy - the **spawner** will create them for us at runtime.

** **Note - This prefab is the reusable bit. Around Week 6 we rework collectibles properly using inheritance (a base Item class with Health / Weapon / Bonus types) and an mini-inventory.**


---

### **Step 2: Create the Collectible Script**

Now the behaviour - what happens when the player touches a collectible.

1. **Create the Collectable Script**:
    - In _Assets / _CRE340 - Game Tutorial / Scripts/Collectable_, create a new C# script called `Collectable`.
    - Attach it to the **Collectible-Coin prefab** (select the prefab in the Prefabs folder, `Add Component`, or open the prefab and add it).
    - Replace the code with the following:

**Note - you'll get an error because the 'GameManager_Collectathon' script is referenced but not created yet - comment that line out for now and uncomment it after Step 4.**

```csharp
using UnityEngine;

public class Collectable : MonoBehaviour
{
    // check when the object / collectable is hit with a trigger
    void OnTriggerEnter(Collider other)
    {
        // check if the object that hit the trigger is the Player - Player is a default tag (we set it in Part 1)
        if (other.CompareTag("Player"))
        {
            // we are calling the gamemanager to increase the score - using a 'static instance' / 'lazy singleton' (not best practice but does the job!)
            GameManager_Collectathon.instance.IncreaseScore();

            // destroy the collectable - we are saying 'Destroy and pass this gameobject as the parameter' (Destroy 'self')
            Destroy(this.gameObject);
        }
    }
}
```

**Explanation**:

- `OnTriggerEnter` runs when something enters our trigger collider.
- We check it was the **Player** (by tag), tell the GameManager to add a point, then destroy ourselves.

---

### **Step 3: Create the Spawner**

Rather than placing collectibles by hand, we spawn them at random positions when the game starts.

1. **Create the CollectableSpawner Script**:
    - Create an empty GameObject in the scene called **CollectableSpawner** and place it at the world origin (0, 0, 0) so the debug gizmo lines up with where things spawn.
    - In _Assets / _CRE340 - Game Tutorial / Scripts/Spawner_, create a C# script called `CollectableSpawner` and attach it to that object.
    - Replace the code with the following:

```csharp
using UnityEngine;

public class CollectableSpawner : MonoBehaviour
{
    // reference to the prefab we will spawn
    public GameObject collectablePrefab;

    public int numberOfCollectables = 10;
    public Vector3 spawnArea; // x, y, z (width, height, depth) of the spawn area (e.g. 20, 0, 20) - set in the editor

    void Awake()
    {
        SpawnCollectables(); // call our spawn function 'before' the game starts using Awake
    }

    void SpawnCollectables()
    {
        // loop through the number of collectables we want - pick a random position in the area - instantiate the collectable
        for (int i = 0; i < numberOfCollectables; i++)
        {
            // random position
            Vector3 randomPosition = new Vector3(
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                Random.Range(0, spawnArea.y),
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
            );

            // create the collectable
            Instantiate(collectablePrefab, randomPosition, Quaternion.identity); // Instantiate takes 3 parameters (GameObject, position, rotation)
        }
    }

    // handy debugging for drawing things in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, spawnArea);
    }
}
```

2. **Set it up in the Inspector**:
    - Drag your **Collectable** prefab into the **Collectable Prefab** field.
    - Set **Number Of Collectables** (e.g. 10) and **Spawn Area** to fit the arena (e.g. 20, 0, 20).
    - Select the spawner in the scene to see the green wire cube showing the spawn area.

**Explanation**:

- The spawner runs in `Awake` (before the game starts) and instantiates the prefab at random positions inside the spawn area.
- `OnDrawGizmosSelected` just draws that area in the editor so you can size it visually. 
	- Re-use this concept in your scripts for making the scene view all fancy!

---

### **Step 4: Create the Game Manager (Score, Timer, Win / Lose)**

The `GameManager_Collectathon` tracks the score, runs the timer, and handles win / lose.

1. **Create the GameManager Script**:
    - Make a folder for the manager called `Manager` in _Assets / _CRE340 - Game Tutorial / Scripts/_
    - Create an empty GameObject in the scene called **GameManager** and attach this script to it.
    - Create a C# script called `GameManager_Collectathon` in _Assets / _CRE340 - Game Tutorial / Scripts/Manager_ and replace the code with the following:

```csharp
using UnityEngine;
using TMPro; // to use TextMeshPro we include the library / namespace

// in this example mini game, GameManager handles score, timer and the UI

public class GameManager_Collectathon : MonoBehaviour
{
    // this makes a static instance of this class publicly available to any other class (public, static, ClassName/Type (self), referenceName)
    public static GameManager_Collectathon instance;

    // text / UI references
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject winText;
    public GameObject gameOverText;

    // logic variables
    private int score = 0;
    private int totalCollectables;
    public float timer = 20f; // 60 = 1 min

    void Awake()
    {
        instance = this; // before the game starts, set this class as the instance (make it globally available to interact with)
    }

    void Start()
    {
        // find all the spawned collectables - the spawner uses 'Awake' to create them, and 'Awake' runs before 'Start'
        totalCollectables = FindObjectsByType<Collectable>(FindObjectsSortMode.None).Length;

        UpdateScoreText(); // set the initial score display
        // set up the UI - hide the end game text
        winText.SetActive(false);
        gameOverText.SetActive(false);
    }

    void Update()
    {
        timer -= Time.deltaTime; // count the timer down (minus equals Time.deltaTime)
        UpdateTimerText();

        // timer logic - do something when the countdown finishes
        if (timer <= 0)
        {
            GameOver();
        }
        if (score >= totalCollectables)
        {
            Win();
        }
    }

    // public function for increasing score and updating the UI - called on collect
    public void IncreaseScore()
    {
        score++;
        UpdateScoreText();
    }

    // UI display functions
    void UpdateScoreText()
    {
        scoreText.text = "Score : " + score.ToString();
    }
    void UpdateTimerText()
    {
        timerText.text = "Time : " + Mathf.Ceil(timer).ToString();
    }

    // game condition functions
    void Win()
    {
        winText.SetActive(true);
        Time.timeScale = 0f;
    }
    void GameOver()
    {
        gameOverText.SetActive(true);
        Time.timeScale = 0f;
    }
}
```

- **Now un-comment** the `GameManager_Collectathon.instance.IncreaseScore();` line in the **Collectable** script from Step 2.
	- Notice what this line was actually doing
		- Its able to get a reference to the GameManager as an instance and call a public function
		- The GameManager is a `static` so it can be globally accessed from any script

** **Note - *Normally we'd call this class 'GameManager'. We use 'GameManager_Collectathon' because we build other game examples in the same project and want to avoid name clashes*.**

** **Note - THE HACK PROBLEM  
	*This GameManager is doing far too much - it's a 'god object' (score, timer, UI, win/lose all in one) and the 'lazy singleton' (a public static instance) is an 'anti-pattern' (more on this in later weeks!).* 
	*It's fine for a small throwaway game and refreshing the basics, but it doesn't scale well. Around Week 5 we rebuild this as a proper Singleton with event-driven UI (Observer / MVP) so the UI isn't hard-wired into the manager, and in Week 6 we rework the collectibles with inheritance and an inventory. Same game, much cleaner, decoupled structure.***

---

### **Step 5: Hook up the UI (The UI is already in the scene)**

The scene already has the Canvas with the Score and Time text (you saw it in Part 1). We just point the GameManager at it.

1. **Assign the references**:
    - Select the **GameManager** object.
    - Drag the **Score** text into **Score Text**, and the **Time** text into **Timer Text**.
    - Drag your **Win** and **Game Over** message objects into **Win Text** and **Game Over Text**, and make sure they start **inactive** in the Hierarchy.

** **Note - *The UI references MUST be assigned or the game throws Null Reference errors on Play. If you're missing a Win / Game Over object, add a TextMeshPro object to the Canvas, set its message, and disable it. Throughout the module we'll develop patterns and structures to avoid these types of reference errors, object coupling etc..***

---

### **Step 6: Test the Game**

1. **Press Play**:
    - Drive the Player around with the twin stick controls from Part 1 and collect the pickups.
    - The **Score** goes up as you collect, the **Time** counts down.
2. **Check the conditions**:
    - Collect everything to see the **Win** message.
    - Let the timer hit zero to see the **Game Over** message.
    - Both freeze the game with `Time.timeScale = 0f`. 
3. **Tune it**:
    - Adjust the **timer**, **number of collectibles**, and **spawn area** until it feels fair.

---

### **What we've set up**:

- A **Collectible** prefab with a trigger, and a **spawner** that scatters it around the arena.
- A **GameManager** tracking score, timer, and win / lose, updating the UI.
- A complete little game loop built on the **Player** from Part 1.

### **Things that carry forward**:

- **Reusable now**: the **Player** prefab (Part 1) and the **Collectible** prefab (Part 2). We'll reuse and further develop these is later weeks.
- **Throwaway now, rebuilt later**: the singleton GameManager and the direct UI updates. This was the refresher. From here we move to more decoupled structures and try to build things in a more scalable, extendable way

*This is just a little baseline mini-game example as a refresher. The project will be small scale but our goal in the up-coming weeks will be to build things cleaner, with de-coupled structures in a more scalable, extendable way.*