### Implementing a Factory Pattern for Enemy Spawning

In this guide, we’ll be implementing the **Factory Pattern** to streamline the process of creating different enemy types during gameplay. 

The Factory Pattern is a software design principle that provides a way to create objects without specifying the exact class of object that will be created. In our case, the Factory will take care of selecting the right enemy variant based on predefined settings, making it easier to spawn various enemy types from a single point in the code.

To support this Factory Pattern, we’ll start by creating an abstract base class called **`EnemyBase`**. This base class will contain common properties and behaviours shared by all enemies, while allowing each specific enemy type to define its unique characteristics (such as taking damage or dying in different ways). 

### Note : This approach, known as **refactoring**, helps to improve the structure of our code by making it more modular and extendable. 

- Refactoring is a term for making changes to dependencies across the code. Usually we are updating a variable or reference name but here we are making changes to classes.

Step 1 will setup `EnemyBase`. This will form the foundation of our Enemy classes

---
### Step 1 : Implementing `EnemyBase` for the Factory Pattern

In this section, we’ll create the **`EnemyBase`** class, which will be the foundational class for all enemy types in our Factory Pattern setup. This class contains shared properties and methods that all enemy variants will inherit, making it easier to manage common behaviours, such as taking damage and visual effects, across all enemies. By using an abstract class, we also ensure that each specific enemy can implement unique behaviours, like different types of movement or death responses.

### `EnemyBase` Class Code

```csharp

using UnityEngine;  
  
public abstract class EnemyBase : MonoBehaviour, IDamagable  
{  
    protected Color originalColor;         // Stores the original color  
    private Material mat;                  // Reference to the material  
  
    private void Start()  
    {        
	    // Cache the material and the original color  
        mat = GetComponent<Renderer>().material;  
        originalColor = mat.color;  
    }      
    public abstract void TakeDamage(int damage); // Abstract for unique damage handling  
    protected abstract void Die();               // Abstract for unique death handling  
  
    public abstract void Move();                 // Optional: unique movement behaviour  
  
    public void ShowHitEffect()  
    {        
	    // Apply hit effect by changing the material color to red  
        mat.color = Color.red;  
        Invoke("ResetMaterial", 0.1f);           // Reset after a short delay  
    }  
  
    protected void ResetMaterial()  
    {        
	    // Reset material color to the original color  
        mat.color = originalColor;  
    }
}

```

### Explanation

- **Purpose**: `EnemyBase` centralises common behaviour for enemies, allowing subclasses to share code for things like hit effects while defining unique behaviours in their own methods.
- **Abstract Methods**: `TakeDamage`, `Die`, and `Move` are defined as abstract, meaning that every enemy subclass **must** implement these methods, allowing for custom behaviour per enemy type.
- **Hit Effect**: `ShowHitEffect()` changes the enemy’s colour to red briefly, and then resets it to the original colour. This visual feedback is automatically available to all enemy types.
- **Refactoring**: Refactoring is the process of restructuring existing code to improve readability, modularity, and maintainability. By refactoring common properties and methods into `EnemyBase`, we make it easy to add new enemy types without duplicating code. 

This base class helps simplify enemy management and sets up a foundation for our Factory Pattern by providing a consistent structure for all enemy types.

---
### Step 2 : Implementing the `Enemy` Class with `EnemyBase`

Here we’ll create the **`Enemy`** class, which inherits from `EnemyBase`. This approach, known as **refactoring**, involves restructuring our code to make it more modular and maintainable. 

By inheriting from `EnemyBase`, our `Enemy` class gains shared functionalities, such as hit effects, while allowing us to customise behaviours for each enemy variant as needed.

### `Enemy` Class Code

```csharp

using UnityEngine;  
using DG.Tweening;  
  
public class Enemy : EnemyBase  
{  
    public EnemyData enemyData; // Reference to the EnemyData ScriptableObject  
    public GameObject dieEffectPrefab; // Reference to the die effect prefab  
    public int damage = 10; // Damage dealt by the enemy  
  
    private int health = 10;  
      
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
        GetComponent<Renderer>().material.color = enemyData.enemyColor;  
  
        Debug.Log($"Enemy {enemyData.enemyName} spawned with {enemyData.health} health and {enemyData.speed} speed.");  
    }      
    
    // Method to handle taking damage (from player or other sources)  
    public override void TakeDamage(int damage)  
    {        
	    health -= damage;  
  
        // Trigger the OnObjectDamaged event  
        HealthEventManager.OnObjectDamaged?.Invoke(gameObject.name, health);  
  
        ShowHitEffect();  
  
        if (health <= 0)  
        {            
	        Die();  
  
            // Trigger the OnObjectDestroyed event  
            HealthEventManager.OnObjectDestroyed?.Invoke(gameObject.name, health);  
        }    
	}  
    
    protected override void Die()  
    {        
	    // Instantiate die effect and apply area damage  
        if (dieEffectPrefab != null)  
        {            
	        Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);  
        }  
        
        // Optional: add death logic, like spawning loot or playing an animation  
        Destroy(gameObject);  
  
        // Debug log to show that the enemy has died  
        Debug.Log("Enemy has died"); 
         
        //increase the players score in the game manager  
        GameManager.Instance.AddScore(10);      
	}  
  
    public override void Move()  
    {        
	    // Define movement specific to this enemy, if needed  
    }  
  
    // Method for the enemy to deal damage to another IDamagable object  
    private void OnCollisionEnter(Collision collision)  
    {        
	    // Check if the collided object has the IDamagable interface  
        IDamagable damagableObject = collision.gameObject.GetComponent<IDamagable>();  
        
        // Prevent enemy from damaging other enemies (check the tag or another distinguishing property)  
        if (damagableObject != null && collision.gameObject.tag != "Enemy")  
        {            
	        // Call TakeDamage on the object, dealing the enemy's damage amount  
            damagableObject.TakeDamage(damage);  
            Debug.Log($"{gameObject.name} dealt {damage} damage to {collision.gameObject.name}.");  
        }    
	}
}

```

### Explanation

- **Inheritance from EnemyBase**: The `Enemy` class inherits methods from `EnemyBase`, such as `ShowHitEffect()`, making it easy to apply shared behaviours across all enemies.
- **Custom Implementation**: By overriding abstract methods like `TakeDamage` and `Die`, the `Enemy` class defines its unique behaviours for taking damage and handling death effects.
- **Spawning and Collision**: The `OnEnable` method triggers a spawn animation, and `OnCollisionEnter` handles interactions with other objects implementing `IDamagable`, such as the player.
  
This refactored approach simplifies enemy creation and management, ensuring consistency across all enemy types while enabling custom behaviours where needed. This class will work seamlessly within our Factory Pattern, allowing for centralised control of enemy spawning and setup.

---


### Step 3 : Setting Up `EnemyData` for the Factory Pattern

To allow for easy configuration of different enemy types, we’ll update the `EnemyData` ScriptableObject to hold all necessary properties and the specific prefab for each enemy. 
This setup allows the **EnemyFactory** to create enemies with unique characteristics by simply using the appropriate `EnemyData`(Scriptable Object) asset.

#### `EnemyData` Class Code

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemy/EnemyData", order = 1)]
public class EnemyData : ScriptableObject
{
    public string enemyName;           // Name of the enemy
    public int health;                 // Health value for the enemy
    public int damage;                 // Damage value for the enemy
    public float speed;                // Movement speed of the enemy
    public Color enemyColor;           // Color of the enemy
    public GameObject enemyPrefab;     // Reference to the specific prefab for this enemy
}
```

### Explanation

- **Prefab Reference**: Each `EnemyData` asset includes a reference to a unique enemy prefab (`enemyPrefab`). This prefab will be used by the Factory to instantiate the correct enemy variant.
- **Configurable Properties**: Properties like `enemyName`, `health`, `damage`, `speed`, and `enemyColor` can be configured for each type of enemy. This makes it easy to create multiple `EnemyData` assets with varying stats and visual characteristics.
- **How It Works**: When we use the **EnemyFactory**, the factory will read from `EnemyData` to create each enemy with these specific properties, making our spawning process more flexible and manageable.

---
# Factory Pattern
### Step 4 : Creating the Enemy Factory

The **EnemyFactory** is responsible for instantiating enemies using the data from `EnemyData` assets. By centralising the creation process in a factory, we ensure consistent setup of enemies and make it easy to add new enemy types without modifying the spawner code.

#### `EnemyFactory` Class Code

```csharp
using UnityEngine;

public static class EnemyFactory
{
    public static EnemyBase CreateEnemy(EnemyData enemyData, Vector3 position)
    {
        if (enemyData.enemyPrefab == null)
        {
            Debug.LogError($"Enemy prefab not assigned in {enemyData.name}!");
            return null;
        }

        // Instantiate the specific enemy prefab at the given position
        GameObject enemyInstance = GameObject.Instantiate(enemyData.enemyPrefab, position, Quaternion.identity);

        // Get the Enemy component and assign the EnemyData properties
        EnemyBase enemy = enemyInstance.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.GetComponent<Enemy>().enemyData = enemyData;
        }
        else
        {
            Debug.LogError("The prefab does not contain an EnemyBase component!");
        }

        Debug.Log($"Created {enemyData.enemyName} at {position}");
        return enemy;
    }
}
```

### Explanation

- **Prefab Instantiation**: The factory uses `enemyData.enemyPrefab` to instantiate the correct prefab for each enemy type. This allows the spawner to create enemies based on `EnemyData` without directly referencing prefabs, making the process modular and flexible.
- **Setting Enemy Data**: Once the prefab is instantiated, the factory assigns `enemyData` to the enemy instance. This passes along the specific properties, such as `enemyName`, `health`, and `enemyColor`, allowing each enemy to inherit characteristics directly from the `EnemyData` ScriptableObject.
- **Centralised Creation**: With `EnemyFactory`, we centralise the process of creating and configuring enemies. Adding new enemy types now only requires creating a new `EnemyData` asset with the desired stats and prefab, which the factory can immediately use to spawn enemies of that type.

This Factory setup streamlines enemy creation, making it easy to manage and extend. By combining `EnemyData` assets and `EnemyFactory`, we’ve simplified the spawning system and provided a scalable foundation for adding more enemies in the future.

---

### Step 5 : Setting Up the Enemy Spawner

The **EnemySpawner** class is responsible for continuously spawning random enemy types at random positions within a designated spawn area. This setup makes use of the **Factory Pattern** by leveraging the `EnemyFactory` to create enemies based on `EnemyData` configurations, rather than manually assigning prefabs.

### `EnemySpawner` Class Code

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public EnemyData[] enemyTypes;       // Array of enemy data types (configured with EnemyData assets)
    public Vector3 spawnArea;            // Dimensions (x, y, z) of the spawn area
    public float startDelay = 1f;        // Initial delay before spawning begins
    public float minSpawnInterval = 2f;  // Minimum interval between spawns
    public float maxSpawnInterval = 5f;  // Maximum interval between spawns
    public int maxSpawnedObjects = 100;  // Maximum number of active spawned enemies

    private List<EnemyBase> spawnedEnemies = new List<EnemyBase>(); // Tracks all spawned enemies

    private void Start()
    {
        StartCoroutine(Spawner());
    }

    private IEnumerator Spawner()
    {
        yield return new WaitForSeconds(startDelay);

        // Repeatedly spawn enemies at random intervals until reaching maxSpawnedObjects
        while (spawnedEnemies.Count < maxSpawnedObjects)
        {
            SpawnRandomEnemy();
            float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnRandomEnemy()
    {
        if (enemyTypes.Length == 0) return;  // Ensure there are enemy types to spawn

        // Pick a random enemy type from the array
        int randomIndex = Random.Range(0, enemyTypes.Length);
        EnemyData selectedEnemyData = enemyTypes[randomIndex];

        // Generate a random spawn position within the spawn area
        Vector3 randomPosition = new Vector3(
            Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
            Random.Range(0, spawnArea.y),
            Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
        );

        // Use the factory to create the enemy
        EnemyBase enemy = EnemyFactory.CreateEnemy(selectedEnemyData, randomPosition);

        if (enemy != null)
        {
            spawnedEnemies.Add(enemy); // Add the spawned enemy to the tracking list
        }
    }
}
```

### Explanation

- **Enemy Data Array**: `enemyTypes` is an array of `EnemyData` ScriptableObjects. Each `EnemyData` asset represents a different enemy type with its own prefab and properties.
- **Spawn Logic**: `Spawner()` starts with a delay (`startDelay`), then spawns enemies at random intervals (between `minSpawnInterval` and `maxSpawnInterval`) until the limit (`maxSpawnedObjects`) is reached.
- **Randomised Enemy and Position**: `SpawnRandomEnemy()` picks a random enemy type and position within `spawnArea`. It then calls `EnemyFactory.CreateEnemy()` to create the enemy instance based on the `EnemyData` asset, adding the enemy to `spawnedEnemies` for tracking.

### Setting Up in Unity

1. **Attach the `EnemySpawner`**: Drag the `EnemySpawner` script onto a GameObject in the scene (e.g., an empty GameObject named "Spawner").
2. **Assign `EnemyData` Assets**: In the `EnemySpawner` component, add each `EnemyData` ScriptableObject to the `enemyTypes` array instead of assigning prefabs directly.
3. **Configure Spawn Area**: Set `spawnArea` dimensions to define the area where enemies will appear. Adjust `startDelay`, `minSpawnInterval`, and `maxSpawnInterval` for timing control.
4. **Run the Scene**: With the setup complete, run the scene to see enemies spawn at random intervals and positions, leveraging the Factory Pattern to configure each instance based on its `EnemyData` asset.

This approach simplifies adding and managing different enemy types, as all configuration is handled through `EnemyData` assets and the centralised `EnemyFactory`. 

You can now easily expand your game by adding new `EnemyData` assets without modifying the `EnemySpawner` code.
