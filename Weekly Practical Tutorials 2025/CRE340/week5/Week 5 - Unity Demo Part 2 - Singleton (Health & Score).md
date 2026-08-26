### Student Guide: Modifying the GameManager for Health and Score (Part 2)

In this guide, we will modify the **GameManager** to manage the **player's health** and the **game's score**. You will then connect the **Player** and **Enemy** classes to update the **GameManager** as changes occur during the game.

In the next guide, we will connect this system to the UI using an **Observer Pattern**.

------------------------------------------------------------------------

### Step 1: Modifying the GameManager for Health and Score Tracking

Here is how your **GameManager** should look after the modifications to track the **player's health** and **score**. We've removed the event handlers for now, which will be added in the next guide.

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

    // Player reference (use your Player class)
    public GameObject playerPrefab;
    private Player playerInstance;

    // Private backing fields for inspector visibility
    [SerializeField] private string playerName = "Player1"; // Default player name
    [SerializeField] private int playerHealth = 100;        // Default health
    [SerializeField] private int score = 0;                 // Default score

    // Public properties to access these fields but prevent external modification
    public string PlayerName
    {
        get { return playerName; }
        private set
        {
            playerName = value;
        }
    }

    public int PlayerHealth
    {
        get { return playerHealth; }
        private set
        {
            playerHealth = value;
        }
    }

    public int Score
    {
        get { return score; }
        private set
        {
            score = value;
        }
    }

    #endregion

    #region Unity Methods
    private void Start()
    {
        // Initialize with default values (optional in later development we would use the GM for managing gamestates)
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
            SetPlayerName(playerInstance.name);
            SetPlayerHealth(playerInstance.health);
        }
    }

    // Method to set the player name
    public void SetPlayerName(string name)
    {
        PlayerName = name;
    }

    // Method to update player health
    public void SetPlayerHealth(int health)
    {
        PlayerHealth = Mathf.Clamp(health, 0, 100); // Ensure health stays between 0 and 100
        if (PlayerHealth <= 0)
        {
            // Handle player death, such as restarting level or showing game over
            Invoke("RestartLevel", 5F);
        }
    }

    // Method to increase the score
    public void AddScore(int points)
    {
        Score += points;
    }

    // Method to restart the current level
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion
}
```

------------------------------------------------------------------------

### Step 2: Connecting the Player to the GameManager

To track the player's health in the **GameManager**, add the following line inside the **Player** class's **TakeDamage()** method:

``` csharp
GameManager.Instance.SetPlayerHealth(health);
```

This will ensure that whenever the **Player** takes damage, the **GameManager** is notified and updates the player's health accordingly.

------------------------------------------------------------------------

### Step 3: Connecting the Enemy to the GameManager

To increase the player's score when an enemy dies, add the following line inside the **Enemy** class's **Die()** method:

``` csharp
GameManager.Instance.AddScore(10); // Add 10 points when an enemy dies
```

This will ensure that whenever an **Enemy** dies, the **GameManager** is notified and updates the score.

------------------------------------------------------------------------

### Summary

At this stage:
- **GameManager** is responsible for managing the **player's health** and the **game's score**.
- **Player** updates the **GameManager** whenever it takes damage.
- **Enemy** updates the **GameManager** by adding to the score when it dies.

In the **next guide**, we will connect these changes to the UI using an **Observer Pattern** so that the UI automatically updates when health or score changes.

Test your game by ensuring that the player's health updates when taking damage and the score increases when enemies are defeated!
