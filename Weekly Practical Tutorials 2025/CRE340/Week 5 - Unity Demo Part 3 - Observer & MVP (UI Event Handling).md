### Student Guide: Adding the Observer Pattern to Update the UI (Part 3)

In this guide, we will implement an **Observer Pattern** in Unity to automatically update the UI whenever the player's name, health, or score changes. This pattern will include a **UIEventHandler** to broadcast the changes and a **UI_EventListener** to update the UI based on those events. This pattern also has elements of the **Model-View-Presenter (MVP)** structure, where the **GameManager** acts as the model and controller, the **UIEventHandler** broadcasts changes, and the **UI_EventListener** updates the view.

------------------------------------------------------------------------

### Step 1: Documenting the **UIEventHandler**

The **UIEventHandler** acts as the **broadcaster** that fires events when the player's name, health, or score changes. Other scripts can listen to these events and update accordingly.

#### **UIEventHandler.cs**

``` csharp
using System;
using UnityEngine;

public class UI_EventHandler
{
    // Events to notify listeners when player state changes
    public static event Action<string> OnPlayerNameChanged;
    public static event Action<int> OnPlayerHealthChanged;
    public static event Action<int> OnScoreChanged;

    // Method to invoke the player name change event
    public static void PlayerNameChanged(string playerName)
    {
        OnPlayerNameChanged?.Invoke(playerName);
    }

    // Method to invoke the player health change event
    public static void PlayerHealthChanged(int playerHealth)
    {
        OnPlayerHealthChanged?.Invoke(playerHealth);
    }

    // Method to invoke the score change event
    public static void ScoreChanged(int score)
    {
        OnScoreChanged?.Invoke(score);
    }
}
```

### Explanation:

1.  **Events**:
    - `OnPlayerNameChanged`, `OnPlayerHealthChanged`, and `OnScoreChanged` are events that are fired whenever the respective value changes.
2.  **Methods**:
    - `PlayerNameChanged()`, `PlayerHealthChanged()`, and `ScoreChanged()` are methods that invoke the respective events and notify listeners about the changes.

------------------------------------------------------------------------

### Step 2: Documenting the **UI_EventListener**

The **UI_EventListener** listens for the events broadcasted by the **UIEventHandler** and updates the UI when changes occur.

#### **UI_EventListener.cs**

``` csharp
using UnityEngine;
using TMPro; // Use TextMeshPro for UI elements

public class UI_EventListener : MonoBehaviour
{
    private UI_Display uiDisplay;

    private void Awake()
    {
        // Get the UI_Display component
        uiDisplay = GetComponent<UI_Display>();
    }

    private void OnEnable()
    {
        // Subscribe to UI events
        UIEventHandler.OnPlayerNameChanged += UpdatePlayerName;
        UIEventHandler.OnPlayerHealthChanged += UpdatePlayerHealth;
        UIEventHandler.OnScoreChanged += UpdateScore;
    }

    private void OnDisable()
    {
        // Unsubscribe from UI events
        UIEventHandler.OnPlayerNameChanged -= UpdatePlayerName;
        UIEventHandler.OnPlayerHealthChanged -= UpdatePlayerHealth;
        UIEventHandler.OnScoreChanged -= UpdateScore;
    }

    // Update the player name in the UI
    private void UpdatePlayerName(string playerName)
    {
        if(uiDisplay != null)
        {
            uiDisplay.UpdatePlayerName(playerName);
        }
    }

    // Update the player health in the UI
    private void UpdatePlayerHealth(int playerHealth)
    {
        if(uiDisplay != null)
        {
            uiDisplay.UpdatePlayerHealth(playerHealth);
        }
    }

    // Update the score in the UI
    private void UpdateScore(int score)
    {
        if(uiDisplay != null)
        {
            uiDisplay.UpdateScore(score);
        }
    }
}
```

### Explanation:

1.  **Awake**: The **UI_EventListener** retrieves the reference to the **UI_Display** component, which is responsible for displaying player data on the screen.

2.  **OnEnable**: When the listener becomes active, it subscribes to the events from **UIEventHandler**.

3.  **OnDisable**: When the listener is disabled, it unsubscribes from the events to avoid memory leaks.

4.  **Update Methods**: The listener methods `UpdatePlayerName()`, `UpdatePlayerHealth()`, and `UpdateScore()` are triggered when the respective event is fired. These methods update the UI by calling the appropriate method in **UI_Display**.

------------------------------------------------------------------------

### Step 3: Documenting the **UI_Display**

The **UI_Display** is responsible for updating the UI text components to display the player's name, health, and score. This is the **View** part of the **MVP** structure.

#### **UI_Display.cs**

``` csharp
using UnityEngine;
using TMPro;

public class UI_Display : MonoBehaviour
{
    // References to the UI elements in the scene
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI scoreText;

    // Method to update the player's name in the UI
    public void UpdatePlayerName(string playerName)
    {
        if(playerNameText != null)
        {
            playerNameText.text = "Player: " + playerName;
        }
    }

    // Method to update the player's health in the UI
    public void UpdatePlayerHealth(int playerHealth)
    {
        if(playerHealthText != null)
        {
            playerHealthText.text = "Health: " + playerHealth.ToString();
        }
    }

    // Method to update the score in the UI
    public void UpdateScore(int score)
    {
        if(scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }
}
```

### Explanation:

1.  **UI References**: `playerNameText`, `playerHealthText`, and `scoreText` are references to the **TextMeshProUGUI** components in your scene that will display the player's name, health, and score.

2.  **Update Methods**: These methods update the respective UI elements when the **UI_EventListener** calls them.

------------------------------------------------------------------------

### Step 4: Attaching the UI Components in Unity

1.  **Create a Canvas**:
    - In your Unity scene, create a **Canvas** (if you don't already have one).
    - Inside the **Canvas**, add **TextMeshProUGUI** elements to display the **Player Name**, **Player Health**, and **Score**.
2.  **Attach the **UI_Display\*\* Component\*\*:
    - Attach the **UI_Display** script to a **GameObject** (you can use an empty GameObject or the Canvas itself).
    - Assign the **TextMeshProUGUI** components to the appropriate fields in the **UI_Display** script in the Unity Inspector.
3.  **Attach the **UI_EventListener\*\* Component\*\*:
    - Attach the **UI_EventListener** script to the same **GameObject** that holds the **UI_Display** component.

    Now, the **UI_EventListener** will listen for changes from the **UIEventHandler** and update the **UI_Display** accordingly.

------------------------------------------------------------------------

### Recap so far...

At this stage, you have successfully implemented an **Observer Pattern** with elements of **MVP** to automatically update the player's name, health, and score in the UI when they change.

In the **next step**, we will connect the **GameManager** to the **UIEventHandler** so that the **GameManager** can notify the UI about changes to player state.

------------------------------------------------------------------------

### Step 5: Connecting the Event Handler to the GameManager

To finalize the **Observer Pattern** and enable the **GameManager** to broadcast changes to the UI, we need to modify the **GameManager** to call the **UIEventHandler** methods when the player's name, health, or score changes.

Here's how to modify the **GameManager** to connect it to the **UIEventHandler** and finalize the pattern.

------------------------------------------------------------------------

### Modifying the **GameManager** to Broadcast Events

1.  **Player Name**: Whenever the **PlayerName** is set, we will notify the UI about the change.

    Modify the setter of the `PlayerName` property:

``` csharp
public string PlayerName
{
    get { return playerName; }
    private set
    {
        playerName = value;
        // Notify the UI when the player name changes
        UI_EventHandler.PlayerNameChanged(playerName);
    }
}
```

2.  **Player Health**: Whenever the **PlayerHealth** is updated, we will notify the UI about the change.

    Modify the setter of the `PlayerHealth` property:

``` csharp
public int PlayerHealth
{
    get { return playerHealth; }
    private set
    {
        playerHealth = value;
        // Notify the UI when the player health changes
        UI_EventHandler.PlayerHealthChanged(playerHealth);
    }
}
```

3.  **Score**: Whenever the **Score** is updated, we will notify the UI about the change.

    Modify the setter of the `Score` property:

``` csharp
public int Score
{
    get { return score; }
    private set
    {
        score = value;
        // Notify the UI when the score changes
        UIEventHandler.ScoreChanged(score);
    }
}
```

------------------------------------------------------------------------

### Finalising the Observer Pattern

With these modifications, the **GameManager** will now notify the **UIEventHandler** whenever:
- The **PlayerName** changes.
- The **PlayerHealth** changes.
- The **Score** is updated.

These notifications will trigger the respective events (`OnPlayerNameChanged`, `OnPlayerHealthChanged`, and `OnScoreChanged`), which the **UI_EventListener** is subscribed to. As a result, the **UI_Display** will be automatically updated with the latest player information.

------------------------------------------------------------------------

### Example Workflow:

1.  When the player's health changes in the **GameManager**:
    - The `SetPlayerHealth(int health)` method updates the `PlayerHealth` property.
    - The `PlayerHealth` property setter calls `UIEventHandler.PlayerHealthChanged(playerHealth)`.
    - The **UIEventHandler** broadcasts the change to all listeners (e.g., **UI_EventListener**).
    - The **UI_EventListener** receives the event and updates the **UI_Display** to reflect the new health value.

------------------------------------------------------------------------

### Conclusion

With these changes, you've successfully connected the **GameManager** to the **UIEventHandler**, completing the **Observer Pattern**. The UI will now automatically reflect changes in the player's name, health, and score without direct interaction from the **GameManager** or other game components.

You're now ready to see the UI update in real-time as you play the game!
