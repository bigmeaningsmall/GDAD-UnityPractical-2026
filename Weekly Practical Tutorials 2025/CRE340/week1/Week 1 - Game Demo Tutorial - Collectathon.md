### **3D Collect-a-thon Mini-Game Overview**

#### **Game Concept**:

- The player controls a character (e.g., a sphere or capsule) that moves around a small 3D environment.
- The goal is to **collect items** (e.g., coins, gems) scattered around the scene within a **time limit**.
- A **UI system** tracks the number of items collected and the remaining time.

#### **Learning Objectives**:

- **Unity Components**: Use and manipulate basic Unity components (e.g., Rigidbody, Colliders, Transforms).
- **Input Handling**: Learn to control the player character using keyboard inputs (WASD or arrow keys) and maybe add mouse or camera controls.
- **Basic Physics**: Implement simple physics to move the character and trigger item collection using colliders.
- **UI Elements**: Introduce basic Unity UI for tracking score and a countdown timer.
- **Game Loop**: Understand game flow with start conditions (start button), win conditions (collect all items), and end conditions (time runs out).

### **Mini-Game Structure**:

#### **1. Scene Setup**:

- Create a simple **3D environment** using primitive shapes (e.g., cubes for the floor, walls, ramps).
- Scatter **collectibles** (spheres or cubes) across the map.

#### **2. Player Controller**:

- Add a **Rigidbody** to the player character (sphere or capsule).
- Use basic input to move the player around using `Rigidbody.AddForce()` or `Transform.Translate()`.

#### **3. Collectibles**:

- Use **OnTriggerEnter** to detect when the player collects an item.
- When collected, the item should disappear, and the player's score should increase.

#### **4. Timer & UI**:

- Add a simple **timer** to count down from a set time (e.g., 2 minutes).
- Display the player's **score** and **remaining time** on the screen using Unity's UI system (`TextMeshPro` or `UI Text`).

#### **5. Win/Lose Conditions**:

- If the player collects all items within the time limit, display a **"You Win"** message.
- If time runs out before all items are collected, display a **"Game Over"** message.

### **Key Concepts to Understand During the Practical**:

1.  **Unity Component Basics**: Adding components like Rigidbody, Colliders, and Camera to objects.
2.  **Basic Player Movement**: Using `Input.GetAxis` to control player movement.
3.  **Physics and Collisions**: Detecting collisions with items using colliders and triggers.
4.  **Game State Management**: Implementing basic win/lose conditions using timers and collectible counts.
5.  **User Interface**: Displaying game stats like score and timer using the Canvas system.

### **Optional Extensions**:

- **Power-Ups**: Add power-ups that grant temporary speed boosts or extra time.
- **Camera Control**: Allow the player to control the camera view using the mouse or have a simple follow camera.
- **Obstacle Avoidance**: Add moving obstacles to the environment to make the game more challenging.

# **Complete Guide: Creating a 3D Collect-a-thon Mini-Game in Unity**

This guide walks you through building a simple 3D Collect-a-thon Mini-Game in Unity, where a player collects items within a time limit. We'll cover basic movement, item collection, UI elements, and win/lose conditions.

------------------------------------------------------------------------

# Download the starter unity project from GitHub using GitHub Desktop

#### We are using the tag - CRE340-Week1_v1.0

##### CRE340-Week1

https://github.com/bigmeaningsmall/GDAD-3D-URP-2025/releases/tag/CRE340-Week1_v1.0

or navigate via the repo we are using across the modules
https://github.com/bigmeaningsmall/GDAD-3D-URP-2025

### **Step 1: Scene Setup (Optional - The scene is already setup)**

Before we dive into scripting, let's create the game environment. You can create your own environment or use the scene that is already set up in the unity project

### Premade Scene - Assets / CRE340 / Game1-Collectathon / Scenes / Scene-Collectathon

1.  **Create the Environment**:
    - Use Unity's primitive objects (like **Cubes** and **Planes**) to create the ground and walls.
    - Scale the plane and cubes to form a small area where the player can move and collect items.
2.  **Player Object**:
    - Create a player object (e.g., a **Capsule** or **Sphere**) to represent the player.
    - Add a **Rigidbody** component to this object so that it interacts with physics.
3.  **Collectible Object**:
    - Create a collectible item using a **Sphere** or **Cube** and customize its size and colour.
    - Drag the object from the Hierarchy to the Assets folder to create a **Prefab** (this prefab will be used by the spawner).

------------------------------------------------------------------------

### **Step 2: Player Movement Script**

Let's create a script to move the player.

1.  **Create the PlayerController Script**:
    - In the `Assets` folder, right-click and choose `Create -> C# Script`. Name it `PlayerControllerRoller`.
    - Attach this script to your player object (e.g., Capsule or Sphere).
    - Replace the script with the following code:

``` csharp
using UnityEngine;

public class PlayerControllerRoller : MonoBehaviour
{
    // variables and references
    public float speed = 5f;
    private Rigidbody rb;

    void Start()
    {
        //get the rigidbody component from the gameobject we attach the player controller to
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // get some inputs for horizontal and vertical using the old input system
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // create a vector3 (x,y,z) for movement and 'cast' the inputs to vector arguements or parameters 
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        
        //apply the movement to our rigidbody and multiply by speed - this allows us to tune the movement speed to the setting we like
        rb.AddForce(movement * speed);
    }
}
```

**Explanation**:
- The `PlayerControllerRoller` script moves the player using `Input.GetAxis()` to detect keyboard inputs.
- The player's movement is applied using `Rigidbody.AddForce()` to make it feel natural with physics.

------------------------------------------------------------------------

### **Step 3: Creating Collectibles and the Spawner**

Now, we need to spawn collectibles in random positions on the map.

1.  **Create the CollectibleSpawner Script**:
    - Create an empty GameObject called "CollectibleSpawner" and attach this script to it.
    - In the `Assets` folder, create a new C# script called `CollectibleSpawner`.
    - Replace the code with the following:

``` csharp

using UnityEngine;  
  
public class CollectibleSpawner : MonoBehaviour  
{  
    //reference to our prefab that we will spawn
    public GameObject collectiblePrefab;  
    
    public int numberOfCollectibles = 10;  
    public Vector3 spawnArea; // x, y, z (width, height, depth) of the spawn area (20,0,20)  - set in the editor
  
    void Awake()  
    {        
        SpawnCollectibles();  // call our spawn function 'before' the game starts using awake
    }  
    
    void SpawnCollectibles()  
    {        
        // loop through the number of collectables we want to create - set a random position in the area - instantiate the collectable
        for (int i = 0; i < numberOfCollectibles; i++)  
        {            
            // random position
            Vector3 randomPosition = new Vector3(  
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),  
                Random.Range(0, spawnArea.y),  
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)  
            );  
            
            //create the collectable
            Instantiate(collectiblePrefab, randomPosition, Quaternion.identity);  // instantiate takes 3 parameters (GameObject, position, rotation)
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

**Explanation**:
- The `CollectibleSpawner` script randomly spawns collectibles within the specified area (`spawnArea`).
- You can adjust the spawn area and number of collectibles in the Unity Inspector.

------------------------------------------------------------------------

### **Step 4: Collecting Items**

We'll now create a script that allows the player to collect items.

1.  **Create the Collectible Script**:
    - Create a new C# script called `Collectible` and attach it to the collectible prefab in the Prefabs folder (*CRE340 / Game1-Collectathon / Prefabs*).
    - Replace the code with the following:

**Note - you will get an error as the 'GameManager_Collectathon' script is referenced but not yet created - comment the line for now**

``` csharp
using UnityEngine;  
  
public class Collectable : MonoBehaviour  
{  
    // check when the object/collectable is hit with a trigger - note the 
    void OnTriggerEnter(Collider other)  
    {        
        // check if the object that hit the trigger ios the Player - Player is a default tag 
        if (other.CompareTag("Player"))  
        {            
            // we are calling the gamemanager to increase the score - using a 'static instance' / 'lazy singleton' (not best practice but does the job!)
            GameManager_Collectathon.instance.IncreaseScore();  
            
            //destroy the collatable - we are saying 'Destroy and passing this gameobject as the parameter' (Destroy 'self')
            Destroy(this.gameObject);  
        }    
    }   
}
```

**Explanation**:
- The `Collectible` script detects when the player collides with a collectible (using `OnTriggerEnter`).
- When collected, the item is destroyed, and the player's score is updated via the `GameManager` script.

------------------------------------------------------------------------

### **Step 5: Game Manager Script (Tracking Score and Timer)**

The `GameManager` handles the player's score, the game timer, and win/lose conditions.

1.  **Create the GameManager Script**:
    - Create an empty GameObject called "GameManager" and attach this script to it.
    - Create a new C# script called `GameManager_Collectathon` and replace the code with the following:

- **Assign the references to the GameManager**:
  - UI elements need to be assigned in the editor or the game will **throw errors**

\*\* **Note - Normally we would call the script 'GameManager'. We are using 'GameManager_Collectathon' as we will develop other game examples in the same project to avoid name conflicts**

\*\* **Note - We are using a 'lazy singleton' pattern for this by creating a static instance but not following good practice - This is ok for small projects but we will implement it in a robust way in larger projects using patterns and following programming principles**

``` c#

using UnityEngine;  
using TMPro;  // to use text mesh pro we need to use or include the library / programming interface
  
// in this example mini game. GameManager handles scores, timers and the UI
  
public class GameManager_Collectathon : MonoBehaviour  
{  
    // this makes a static instance of this class publically available to any other class (public, static, ClassName/Type (self), referenceName)
    public static GameManager_Collectathon instance;  
    
    // text/UI references
    public TextMeshProUGUI scoreText;  
    public TextMeshProUGUI timerText;  
    public GameObject winText;  
    public GameObject gameOverText;  
  
    // logic variables
    private int score = 0;  
    private int totalCollectibles;  
    public float timer = 20f; // 60=1min  
  
    void Awake()  
    {        
        instance = this;  // before the game starts initialise this class as an instance (make it globally available to interact with)
    }  
    void Start()  
    {        
        // find all the instantiated collectables - we used 'Awake' to instantiate the collectables - 'Awake' executes before 'Start'
        totalCollectibles = FindObjectsOfType<Collectable>().Length;  
        
        UpdateScoreText();  // call the score function to set its initial state
        // set up the UI - hide the end game text
        winText.SetActive(false);  
        gameOverText.SetActive(false);  
    }  
    void Update()  
    {        
        timer -= Time.deltaTime;  // countdown times (minus equals time.deltaTime)
        UpdateTimerText();  
          
        //timer logic - do something when countdown finishes
        if (timer <= 0)  
        {            
            GameOver();  
        }  
        if (score >= totalCollectibles)  
        {            
            Win();  
        }    
    }  
    
    // public function for increasing score and updating UI - called on collect
    public void IncreaseScore()  
    {        
        score++;  
        UpdateScoreText();  
    }  
    
    // UI Display functions 
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

**Explanation**:
- The `GameManager_Collectathon` keeps track of the player's score and the remaining time.
- If all collectibles are gathered, a "You Win" message is displayed.
- If the timer runs out, a "Game Over" message is displayed.

------------------------------------------------------------------------

### **Step 6: Setting up the UI (Optional - The UI is already setup in the supplied scene)**

Now, let's set up the user interface (UI) to display the score and timer.

\*\* **As the supplied scene has the UI setup already, drag the UI objects to the GameManager in the inspector**

1.  **Create a Canvas**:
    - Right-click in the Hierarchy and select `UI -> Canvas`.
    - Create **TextMeshProUGUI** objects for the **Score** and **Timer**. Adjust their size and position in the Scene view.
    - Create **TextMeshProUGUI** objects for the **Win** and **Game Over** messages. Set them to inactive by default.
2.  **Link UI Elements**:
    - In the Inspector, drag the `Score Text` and `Timer Text` objects into the appropriate fields in the `GameManager` script.
    - Similarly, drag the `Win` and `Game Over` texts into their respective slots.

------------------------------------------------------------------------

### **Step 7: Testing the Game**

1.  **Run the Game**:
    - Press `Play` in Unity and test the game.
    - The player should be able to move around, collect items, and see the score and timer update.
2.  **Win/Lose Conditions**:
    - Collect all items to win, or let the timer run out to trigger the game over condition.

------------------------------------------------------------------------

### **Optional Extensions**:

- **Add Power-Ups**: Create new objects that grant temporary speed boosts or add more time to the timer.
- **Camera Control**: Add a script to follow the player using a smooth camera or let the player control the camera with the mouse.
- **Obstacles**: Add moving obstacles to the environment for increased difficulty.

------------------------------------------------------------------------

### **Conclusion**:

This guide walked you through creating a simple 3D Collect-a-thon Mini-Game in Unity. You refreshed how to implement player movement, spawn collectibles, track score and time, and manage win/lose conditions.

Use this as a foundation and extend the game by adding more features like power-ups, camera control, or obstacles!
