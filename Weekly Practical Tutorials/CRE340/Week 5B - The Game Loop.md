---
Order: 5-B
---
### Overview

#### **What we are building**:
- The **GameManager** takes over spawning the player, rather than us dropping one in the scene.
- We add **lives**, so dying respawns you instead of just ending things.
- When the lives run out we get a **game over** and the level restarts.
#### **Why we are doing this**:
- Up to now the game has no start and no end. You play until you get bored of it.
- Lives and a respawn give us an actual loop, and it's a small amount of code on top of what we built in Part 1.
- Spawning and lifecycle are a reasonable job for a GameManager, and it gives us a good example of how these classes grow.

#### **Learning Objectives**:
- **Instantiating from a manager** rather than placing objects by hand.
- **Coroutines for timed sequences** - respawn and restart delays.
- **`SceneManager.LoadScene`** - reloading the current level.
- **`yield break`** - ending a coroutine early.

---
---

# **Part 2: Lives, Respawning and Game Over**

Carry on from Part 1. You should have the GameManager tracking coins and XP, with the player registering itself.

---

### **Step 1: What We're Adding**

The loop we're after:

> Spawn the player - play - die - lose a life - respawn - run out of lives - game over - restart

At the moment the player is placed in the scene by hand. That works fine until the player dies, because there's nothing to bring them back. If the GameManager creates the player then it can also create another one.

---

### **Step 2: Check the Player Prefab**

The player is going to be created at runtime now, so everything it needs has to be **inside the prefab**. Nothing can point at an object sitting in the scene.

1. **Open the Player prefab** (double click it in the Prefabs folder, don't just select the one in the Hierarchy).
2. **Check the InputManager** is a child of the Player inside the prefab.
3. **Check the references**:
    - On **Twin Stick Controller**, the **Input Manager** field should point at the Player's own child.
    - Same on the **Shoot** component.
4. If either field is empty or showing a missing reference, drag the child InputManager onto it **while the prefab is open**.
5. **Delete the Player from the scene** - the GameManager will make one.
6. **Create an empty GameObject** called **Player Spawn Point** and put it somewhere sensible in the arena. Leave it at ground level.

> [!warning] **Note - prefab references and scene references**
> 
> This is the bit to get right before anything else.
> 
> A prefab can hold a reference to **its own children**, because those come with it when it's instantiated. It **cannot** hold a reference to something sitting in the scene - Unity clears those, and you get `None (Input Manager)` in the Inspector.
> 
> That's why the InputManager being a child of the Player matters. If it were a separate object in the scene, every spawned player would come out with no input and wouldn't move.
> 
> _Easy way to check - look at the field while the prefab asset is open in the Prefab view. If it shows a value there, it's internal and it'll survive instantiation._

> [!note] **Note - if you're using a follow camera**
> 
> The camera in this scene is fixed so it doesn't care, but if you've set up a camera that follows the player then it's holding a scene reference to a player that no longer exists at startup.
> 
> You'd want the camera to ask `GameManager.Instance.Player` for its target instead, the same way the enemies do, and handle it being null between respawns. We'll do this in a later tutorial when we start to work with the camera or cine machine.

---

### **Step 3: Add Spawning to the GameManager**

Add this to `GameManager.cs`. You'll need two more `using` lines at the top:

```csharp
using System.Collections;              // for coroutines
using UnityEngine;
using UnityEngine.SceneManagement;     // for reloading the scene
```

Then add a new region to the class:

```csharp
    #region Player Spawning

    [Header("Player")]
    public GameObject playerPrefab;        // drag the Player prefab here
    public Transform playerSpawnPoint;     // drag the Player Spawn Point object here
    public float respawnDelay = 2f;        // seconds before the player comes back

    private void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null || playerSpawnPoint == null)
        {
            Debug.LogError("GameManager is missing the player prefab or the spawn point.");
            return;
        }

        Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);

        // We don't need to do anything with the new player here -
        // PlayerStats registers itself with us in its own Awake.
    }

    #endregion
```

**Set it up in the Inspector**:

- Drag the **Player** prefab onto **Player Prefab**.
- Drag the **Player Spawn Point** object onto **Player Spawn Point**.

Press Play. The player appears at the spawn point and everything works as before - moving, shooting, collecting, taking damage.

**Explanation**:

- `Start` runs after every `Awake`, so the GameManager has already set itself up as the instance by the time we spawn anything.
- We don't keep the object `Instantiate` returns. The player tells us about itself in its own `Awake`, which happens during that `Instantiate` call, so by the time the next line runs `GameManager.Instance.Player` is already set.
- The null checks save you a confusing error later. Forgetting to drag one of these in is easy and the message tells you exactly which one.

---

### **Step 4: Lives and Respawning**

Now we handle the player dying.

Add this inside the same region, under `SpawnPlayer`:

```csharp
    [Header("Lives")]
    [SerializeField] private int lives = 3;

    public int Lives
    {
        get { return lives; }
    }

    // Called by PlayerStats when the player's health hits zero
    public void PlayerDied()
    {
        lives = lives - 1;

        Debug.Log("Player died. Lives left: " + lives);

        if (lives > 0)
        {
            StartCoroutine(RespawnAfterDelay());
        }
        else
        {
            GameOver();
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        SpawnPlayer();
    }
```

**Explanation**:

- `PlayerDied` is the only thing the player has to call. The GameManager works out whether that means a respawn or a game over.
- The coroutine gives a short gap before the player comes back, which stops you respawning straight into the enemy that just killed you. Same coroutine pattern we've used since Week 2.
- `lives` is `[SerializeField] private` with a read-only property, same as coins and XP. The UI will want to read it in Week 6.

> [!note] **Note - who decides what happens on death?**
> 
> The player doesn't know about lives and doesn't know whether dying means a respawn or the end of the game. It reports that it died and the GameManager decides.
> 
> That split keeps `PlayerStats` focused on being a player, and it means you can change how lives work without opening the player script at all. Try setting `lives` to 1 in the Inspector and you've got a one-life game, with no code changes.

---

### **Step 5: Game Over and Restart**

Add one more region to the GameManager:

```csharp
    #region Game State

    [Header("Game State")]
    [SerializeField] private bool isGameOver = false;
    public float restartDelay = 3f;

    public bool IsGameOver
    {
        get { return isGameOver; }
    }

    public void GameOver()
    {
        isGameOver = true;

        Debug.Log("GAME OVER - Coins: " + coins + "   XP: " + xp);

        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);

        // Reload the scene we're currently in. Everything resets - enemies, items, etc...
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion
```

**Explanation**:

- `isGameOver` is a flag other scripts can check. We'll use it in Step 7 and the UI will use it in Week 6.
- `SceneManager.GetActiveScene().buildIndex` gets the scene we're in, and `LoadScene` reloads it. Everything is destroyed and recreated from scratch, including the GameManager, so coins and XP go back to zero.
- The scene has to be in **File -> Build Settings** for this to work. Add it if you get an error about the scene not being loaded.

> [!note] **Note - freezing the game on game over**
> 
> Week 1's collectathon used `Time.timeScale = 0f` to freeze everything on win or lose. You could do the same here, but it'll break.
> 
> **`WaitForSeconds` uses scaled time**, so setting `timeScale` to 0 stops the restart coroutine dead and the level never reloads.
> 
> If you want the freeze, use **`WaitForSecondsRealtime`** instead, which ignores `timeScale`. It catches people out regularly - a coroutine that works fine until someone adds a pause menu.

---

### **Step 6: Report the Death**

Open `PlayerStats.cs` and finish off the `Die()` method:

```csharp
    private void Die()
    {
        if (HealthEventManager.OnObjectDestroyed != null)
        {
            HealthEventManager.OnObjectDestroyed(gameObject.name, data.currentHealth);
        }

        // Tell the GameManager - it deals with lives and respawning
        GameManager.Instance.PlayerDied();

        Destroy(gameObject);
    }
```

Press Play and let an enemy kill you. The Console counts down the lives and a new player appears a couple of seconds later. Lose three and the level reloads.

**Explanation**:

- The player still broadcasts its death to `HealthEventManager`, so it still appears in the event log alongside everything else.
- Then it tells the GameManager and destroys itself.
- While it's gone, `GameManager.Instance.Player` returns null and the enemies stop chasing - that's the null check we put in during Part 1 doing its job.

---

### **Step 7: Stop the Spawners on Game Over**

One loose end. The spawners keep running after game over, so enemies carry on arriving over a dead arena while you wait for the restart.

Open `Spawner.cs` and update `SpawnLoop`:

```csharp
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

            if (spawnedObjects.Count < maxAlive)
            {
                SpawnOne();
            }
        }
    }
```

**Explanation**:

- **`yield break`** ends a coroutine there and then. The `while (true)` loop would otherwise run forever.
- Both spawners get this, because both use the same script. Four lines covers the item spawner and the enemy spawner.

> [!note] **Note - the cost of using the Singleton**
> 
> That was relatively quick to write, and it's worth noticing what it did to the `Spawner` class.
> 
> `Spawner` now needs a `GameManager` to exist. Drop it into a test scene on its own and it throws an error. Before this step it had no dependencies at all and would work in any scene you put it in.
> 
> It's a trade off Every time something reaches for `GameManager.Instance` it ties itself to the GameManager a bit more, and it's easy to do without noticing because there's no reference in the Inspector showing you it happened.

---

### **Step 8: Test the Loop**

1. **Press Play**. The player spawns at the spawn point.
2. **Let an enemy kill you.** Lives drop, a new player appears after the delay.
3. **Watch the enemies while you're dead** - they stop chasing and stand still.
4. **Lose all three lives.** Game over in the Console with your final coins and XP, spawners stop, level reloads a few seconds later.
5. **Tune it** - `Lives`, `Respawn Delay` and `Restart Delay` on the GameManager, and the enemy spawner's intervals and `Max Alive` for difficulty.

A few things worth trying:

6. **Set Lives to 1** for a harsher game. No code change.
7. **Turn the respawn delay down to 0.1** and see why we put a delay there in the first place.

---

### **A look at the GameManager now**

Open the script and have a read through it. It currently handles:

- The singleton setup
- Coins
- XP
- The player reference
- Spawning the player
- Lives and respawning
- Game over
- Restarting the level

Every one of those went in for a sensible reason, and each one was a small addition at the time. The class is still readable and nothing in it is wrong.

This is how a **god object** starts. Nobody chooses to write one - a class that's easy to reach collects jobs, one reasonable addition at a time, and by the end of a project it's a thousand lines that everything depends on and nobody wants to touch.

Two things keep it in check:

- **Watch what the class is called.** `GameManager` manages the game - lives, progression, state. The moment something player-specific goes in it (health, ammo, inventory) that's a sign it belongs elsewhere. We kept health in `PlayerStats` for exactly that reason.
- **Notice when you add a job.** Spawning went in this week and it was reasonable. Next time it'll be easier to add the one after that.

We'll come back to this in a later **Week 8-9** and look at how you'd split it up, once it's carrying save and load as well.

---

### **What we've set up**:

- The **GameManager spawning the player** from a prefab and a spawn point.
- **Lives**, with a delayed respawn.
- **Game over** and a level restart.
- **Spawners stopping** once the game is over.

### **Week 5 Summary**

- **Part 1** - the **Singleton** pattern, built properly, with coins, XP and a player reference. Plus a Coin item and XP from kills.
- **Part 2** - the GameManager handling the **game loop** - spawning, lives, game over, restart.

The thing to take from this week is less about the code and more about **where state lives**. The GameManager owns things that belong to the game. `PlayerStats` owns things that belong to the player. Each number has one owner, and anything that needs it asks that owner.

### **Next week**:

Everything is reporting to the Console at the moment, which is no use to a player. In **Week 6** we build the **Observer** pattern and **MVP** to get health, coins, XP and lives onto the screen - with the UI updating itself when things change, rather than the GameManager having to know a UI exists. This is also how we'll tie our event based patterns in to our reactive audio-visual.
