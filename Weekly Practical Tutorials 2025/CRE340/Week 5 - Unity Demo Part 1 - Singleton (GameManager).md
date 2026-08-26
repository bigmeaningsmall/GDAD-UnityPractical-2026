### Setting Up a Singleton GameManager (Part 1)

In this guide, you will set up a **Singleton GameManager** in Unity. The **GameManager** will be responsible for key game components, and for now, we will focus on setting up the **Singleton** and **spawning the player**. The **Player Prefab** is already set up for you in the **CRE340 Prefabs** folder.

- A Player Class has been added
- Enemies have been setup as prefab variants with movement

**Check the Player and Enemy prefabs are tagged: 'Player' 'Enemy'**

In future guides, we will:
- Connect the **GameManager** to the **Player** and **Enemy** to manage health and score.
- Implement an **Observer Pattern** to update the UI with player information.

------------------------------------------------------------------------

### Step 1: Understanding the Singleton Pattern

A **Singleton** ensures that only one instance of a class exists and provides a global point of access to that instance. For game development, a **GameManager** is typically a Singleton because you only want one instance managing your game's key systems (e.g., spawning players, tracking states).

------------------------------------------------------------------------

### Step 2: Setting Up the GameManager Singleton

#### 1. **Create the GameManager Script**

- Open Unity and in your project, create a new **C# script** called `GameManager`.
- Add the following code to implement the **Singleton** and player spawning:

``` csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton Implementation
    
    // Singleton instance
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<GameManager>();
                    singletonObject.name = typeof(GameManager).ToString() + " (Singleton)";
                }
            }
            return instance;
        }
    }

    #endregion

    #region Properties and Fields
    
    // Player reference
    public GameObject playerPrefab;
    private Player playerInstance;

    // Inspector-visible default player state
    [SerializeField] private string playerName = "Player1"; // Default player name
    [SerializeField] private int playerHealth = 100;        // Default health
    [SerializeField] private int score = 0;                 // Default score

    #endregion

    #region Unity Methods
    private void Start()
    {
        // Initialize with default values
        Debug.Log("GameManager initialized with default player state.");
    }
    #endregion

    #region Custom Public Methods
    
    // Method to instantiate the player and keep track of its instance
    public void SpawnPlayer(Vector3 spawnPosition)
    {
        if (playerInstance == null) // Ensure we don't spawn multiple players
        {
            GameObject playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            playerInstance = playerObject.GetComponent<Player>();
        }
    }
    
    #endregion
}
```

### Step 3: Breaking Down the Code

#### 1. **Singleton Pattern** (`GameManager.Instance`)

- The **Singleton** pattern ensures that only one instance of the `GameManager` exists in the game.
- The `Instance` property checks if the `GameManager` is already in the scene. If not, it creates a new one dynamically.

#### 2. **Player Prefab**

- We include a `playerPrefab` field that allows you to set up the player in the **Inspector**.
- The `SpawnPlayer()` method instantiates the player at a given position and tracks its instance.

#### 3. **Inspector Variables**

- The default values for `playerName`, `playerHealth`, and `score` are exposed in the Unity **Inspector** for easy tracking. These fields are private but visible in the Inspector for debugging.

------------------------------------------------------------------------

### Step 4: Setting Up the Player Prefab

In this project, the **Player Prefab** is already set up in the **CRE340 Prefabs** folder. Follow these steps to connect it to the **GameManager**:

1.  **Assign the Player Prefab to the GameManager**
    - In the **Project** panel, locate the **PlayerPrefab** inside the **CRE340 Prefabs** folder.
    - In the **Inspector**, drag the `PlayerPrefab` from the folder into the `playerPrefab` field of the **GameManager**.

------------------------------------------------------------------------

### Step 5: Spawning the Player

Now that we've connected the **PlayerPrefab** to the **GameManager**, let's spawn the player when the game starts:

1.  **Create a Script Called 'PlayerSpawner'**:
    - Attach the script to the 'Spawn' GameObject in the scene
    - The PlayerSpawner will call the **GameManager** `SpawnPlayer()` method to spawn the player when the game starts:

``` csharp
public class PlayerSpawner : MonoBehaviour  
{  
    public Vector3 spawnPosition; // Set this to the desired spawn position in the Inspector  
    [Range(0,2)]  
    public float spawnDelay = 0.5f; // Set this to the desired spawn delay in the Inspector  
  
    void Start()  
    {        
        Invoke("SpawnPlayer", spawnDelay); // Delay the player spawn by 2 seconds  
    }  
    private void SpawnPlayer()  
    {        
        // Call the SpawnPlayer method from the GameManager  
        GameManager.Instance.SpawnPlayer(spawnPosition);  
    }
}
```

------------------------------------------------------------------------

### Step 6: Testing the GameManager

1.  **Add the GameManager to the Scene**
    - Create an empty GameObject in your scene.
    - Name it **GameManager**.
    - Attach the **GameManager** script to this GameObject.
2.  **Assign the Player Prefab**
    - In the Inspector for the **GameManager**, assign the `PlayerPrefab` field with the **Player** prefab from the **CRE340 Prefabs** folder.
3.  **Run the Game**
    - Press **Play**. The player will be spawned at the specified position when the game starts.

------------------------------------------------------------------------

### Conclusion

Great job! 🎉 You've successfully set up a **Singleton GameManager** that spawns the player when the game starts.

### What's Next?

In the **next guide**, we will:
1. **Connect the GameManager to the Player and Enemy** to manage and update **health** and **score**.
2. Add **logic for restarting the level** when the player dies.

In the **third guide**, we will:
- Add an **Observer Pattern** to keep the UI updated with player information (such as health and score) automatically.

Feel free to test and tweak your **GameManager** before moving on to the next step!
