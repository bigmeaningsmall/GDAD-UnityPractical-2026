###  Implementing the State Pattern for Enemy Behaviour

This guide walks you through implementing the **State Pattern** for an enemy in your game. 

This implementation of the pattern enables your enemy to switch between states like **Idle** and **Chase** based on player proximity, creating a modular and extensible system for different behaviours. 

Follow the steps below, ensuring you set up the necessary folders and remove outdated components for a clean implementation.

---
### Step 1: Folder Setup

1. **Create a Folder for State Scripts**: 
   - In your project’s **Scripts/Enemy** folder, create a new folder called **EnemyState**.
   - Place the following scripts into this **EnemyState** folder:
     - `IEnemyState`
     - `EnemyState_Idle`
     - `EnemyState_Chase`

1. **Organise Other Scripts**:
   - Keep `Enemy` in the **Scripts/Enemy** folder.
   - `State_UI` can be in the folder associated with UI elements.

##### **Update the EnemyData class to add `chaseRange`**

```c#
using UnityEngine;  
  
[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemy/EnemyData", order = 1)]  
public class EnemyData : ScriptableObject  
{  
    public string enemyName;           // Name of the enemy  
    public int health;                 // Health value for the enemy  
    public int damage;                 // Damage value for the enemy  
    public float speed;                // Movement speed of the enemy  
    public float chaseRange = 10f;      // Range within which the enemy will chase the player  
    public Color enemyColor;           // Color of the enemy  
    public GameObject enemyPrefab;     // Reference to the specific prefab for this enemy  
}
```


---

### Step 2: Removing the Old Movement Script

With the new **State Pattern** setup, enemy movement during the chase is handled in the `EnemyState_Chase` state. To avoid redundant movement logic:
   - **Remove** the old `EnemyMovement` script from the enemy prefab, as `EnemyState_Chase` will now handle movement when chasing the player.

---

### Step 3: Implement the State Pattern

Follow the code outline provided to understand how each component works in the state-based behaviour.

#### 1. `IEnemyState` Interface

The `IEnemyState` interface defines the structure for each enemy state, ensuring consistency across states. Each state needs methods to **Enter**, **Update**, and **Exit**.

```csharp
public interface IEnemyState
{
    void Enter(Enemy enemy);  // Called when entering the state
    void Update(Enemy enemy); // Called every frame in this state
    void Exit(Enemy enemy);   // Called when exiting the state
}
```

---

#### 2. `Enemy` Class

The `Enemy` class manages the state transitions and defines conditions under which states change. It holds data like health, damage, speed, and the range at which the enemy detects the player.

**Key Components**:
- **State Management**: Use `SetState` to switch between states and call the `Enter` and `Exit` methods accordingly.
- **GetCurrentStateName**: Returns the name of the current state to update the UI.

**Code**:

```csharp

using UnityEngine;
using DG.Tweening;

public class Enemy : EnemyBase
{
	public EnemyData enemyData; // Reference to the EnemyData ScriptableObject  
	public GameObject dieEffectPrefab; // Reference to the die effect prefab  
	public int damage = 10; // Damage dealt by the enemy  
	private int health = 10;  
	public float speed = 2f;  
	public float chaseRange = 5f;  
	private IEnemyState currentState;  
	public Transform target;

	private void OnEnable()
	{  
	    // Store the original scale so we can return to it later  
	    Vector3 initialScale = transform.localScale;  
	    //scale the crate up from 0 to 1 in 1 second using DOTween    
		transform.localScale = Vector3.zero;    
		transform.DOScale(initialScale, 1f).SetEase(Ease.OutBounce);  
	}  
	private void Awake()  
	{  
	    // Apply the data from the ScriptableObject to the enemy  
	    gameObject.name = enemyData.enemyName;  
	    health = enemyData.health;  
	    damage = enemyData.damage;  
	    speed = enemyData.speed;  
	    chaseRange = enemyData.chaseRange; // Set chase range from enemy data  
	    GetComponent<Renderer>().material.color = enemyData.enemyColor;  
	    Debug.Log($"Enemy {enemyData.enemyName} spawned with {enemyData.health} health and {enemyData.speed} speed.");  
	}  
	  
	private void Start()  
	{  
	    SetState(new EnemyState_Idle());  
	    Invoke("LocatePlayer", 1f);  
	}  
	  
	private void Update()  
	{  
	    currentState?.Update(this);  
	}  
	  
	public void SetState(IEnemyState newState)  
	{  
	    currentState?.Exit(this);  
	    currentState = newState;  
	    currentState?.Enter(this);  
	}  
	  
	public string GetCurrentStateName()  
	{  
	    return currentState != null ? currentState.GetType().Name.Replace("Enemy", "") : "No State";  
	}  
	private void LocatePlayer()  
	{  
	    if (target == null)  
	    {        
		    target = GameObject.FindGameObjectWithTag("Player").transform;  
	    }
	}

    // ... (Other methods remain unchanged, including TakeDamage and Die)

}

```

---

#### 3. `EnemyState_Idle` Class

The `EnemyState_Idle` class represents the **Idle** state. It checks if the player is within chase range, and if so, transitions to the **Chase** state.

```csharp
using UnityEngine;

public class EnemyState_Idle : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Idle State");
    }

    public void Update(Enemy enemy)
    {
        if (enemy.target == null) return;

        if (Vector3.Distance(enemy.transform.position, enemy.target.position) < enemy.chaseRange)
        {
            enemy.SetState(new EnemyState_Chase());
        }
    }

    public void Exit(Enemy enemy)
    {
        Debug.Log("Exiting Idle State");
    }
}
```

---

#### 4. `EnemyState_Chase` Class

The `EnemyState_Chase` class represents the **Chase** state, moving the enemy toward the player and checking if the player moves out of range, at which point the enemy returns to **Idle**.

```csharp
using UnityEngine;

public class EnemyState_Chase : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Chase State");
    }

    public void Update(Enemy enemy)
    {
        enemy.transform.position = Vector3.MoveTowards(
            enemy.transform.position,
            enemy.target.position,
            enemy.speed * Time.deltaTime
        );

        if (Vector3.Distance(enemy.transform.position, enemy.target.position) > enemy.chaseRange)
        {
            enemy.SetState(new EnemyState_Idle());
        }
    }

    public void Exit(Enemy enemy)
    {
        Debug.Log("Exiting Chase State");
    }
}
```

---

### Step 4: Displaying the State with `State_UI`

The `State_UI` script displays the current enemy state in a **TextMeshPro** component above the enemy. 

Enable the `TextMeshPro` child object to the enemy prefab, ensuring it’s visible above the enemy character.

**Code**:

```csharp
using UnityEngine;
using TMPro;

public class EnemyState_UI : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();

        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }

        if (textMesh == null)
        {
            Debug.LogError("TextMeshPro component not found on State_UI!");
        }
    }

    private void Update()
    {
        UpdateStateText();
    }

    public void UpdateStateText()
    {
        if (enemy != null && textMesh != null)
        {
            textMesh.text = enemy.GetCurrentStateName();
        }
    }
}
```

---

### Summary

- **Folder Setup**: Place each script in the correct folder for better organisation.
- **Removing Old Components**: Remove any movement-specific scripts from the enemy prefab since `EnemyState_Chase` now handles movement.
- **Enemy States**: Use the `IEnemyState` interface to set up each state (Idle and Chase) and transition between them using `SetState` in `Enemy`.
- **Debug Display**: `State_UI` updates the **TextMeshPro** component to show the current state for debugging.

With this setup, your enemy will dynamically switch between Idle and Chase states based on proximity to the player, displaying each state above the enemy for easy tracking. 

This implementation provides a clean, modular approach to enemy behaviour and can extend to more states by adding new state classes.