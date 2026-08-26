### Unity Demo Guide: Adding ScriptableObjects for Enemy Variants

In this guide, we will add ScriptableObjects to manage different enemy variants and apply their properties to the existing enemy prefab. This approach allows you to easily create and modify enemy types without changing the core prefab. We’ll set up two enemy variants: **"Bad Guy - Green"** and **"Bad Guy - Blue."**

#### Step 1: Create the `EnemyData` ScriptableObject Class

First, we need to create the `EnemyData` ScriptableObject class that will hold enemy properties like name, health, speed, and color.

- Inside the `Enemy` scripts folder
- Right click in the folder to access the `Create menu`
	- Create - Scripting - ScriptableObject Script
	- Name the ScriptableObject - `EnemyData`

**Code for `EnemyData` ScriptableObject:**

```c#
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;  // Name of the enemy
    public int health;        // Health value for the enemy
    public float speed;       // Movement speed of the enemy
    public Color enemyColor;  // Color of the enemy
}
```

This script allows you to define data for different enemy variants by creating instances of this `EnemyData` ScriptableObject.

#### Step 2: Set Up the `Enemy` Prefab to Use ScriptableObjects

Before creating the ScriptableObject instances, we need to ensure that the `Enemy` prefab is configured to use this data.

1. **Open the `Enemy` Script**: Open the `Enemy` class and add a public variable to reference the `EnemyData` ScriptableObject.
   
   **Modify the `Enemy` class:**
   ```c#
   public class Enemy : MonoBehaviour
   {
		public EnemyData enemyData; // Reference to the EnemyData scriptable object  
  
		public int health = 10;  
  
		private Material mat;  
		private Color originalColor;  
  
		private void Awake()  
		{  
		    // Apply the data from the ScriptableObject to the enemy  
		    gameObject.name = enemyData.enemyName;  
		    GetComponent<Renderer>().material.color = enemyData.enemyColor;  
  
		    Debug.Log($"Enemy {enemyData.enemyName} spawned with {enemyData.health} health and {enemyData.speed} speed.");  
		}  
  
		private void Start()
		{  
		    mat = GetComponent<Renderer>().material;  
		    originalColor = mat.color;  
		}

       // Other existing methods (e.g., TakeDamage) remain unchanged
   }
   
   ```

2. **Assign the `EnemyData` in the Inspector**: You will later assign different ScriptableObjects to this `enemyData` field for each enemy variant.

#### Step 3: Create the ScriptableObjects in Unity

Now that the `Enemy` script is ready to use `EnemyData`, follow these steps to create and configure the two enemy variants.

1. **Create the First ScriptableObject ("Bad Guy - Green"):**
   - Right-click in the **Project** window **inside your Enemy Scripts folder** and go to `Create > Scriptable Objects > EnemyData`.
   - Name the ScriptableObject **"Bad Guy - Green."**
   - Select the **"Bad Guy - Green"** ScriptableObject in the Project window, and set the following properties in the **Inspector**:
     - **Enemy Name**: Bad Guy - Green
     - **Health**: 4
     - **Speed**: 5
     - **Enemy Color**: Set this to **Green**.

2. **Create the Second ScriptableObject ("Bad Guy - Blue"):**
   - Right-click again in the **Project** window and go to `Create > Scriptable Objects > EnemyData`.
   - Name the ScriptableObject **"Bad Guy - Blue."**
   - Select the **"Bad Guy - Blue"** ScriptableObject, and set the following properties in the **Inspector**:
     - **Enemy Name**: Bad Guy - Blue
     - **Health**: 5
     - **Speed**: 7
     - **Enemy Color**: Set this to **Blue**.

#### Step 4: Apply ScriptableObjects to the Enemy Prefab

1. **Select the Enemy Prefab**: In your **Hierarchy** or **Project** window, find and select the `Enemy` prefab.
   
2. **Assign the ScriptableObjects**: In the **Inspector**, under the `Enemy` script component:
   - For one instance of the prefab, drag the **"Bad Guy - Green"** ScriptableObject into the `Enemy Data` field.
   - For another instance of the prefab, drag the **"Bad Guy - Blue"** ScriptableObject into the `Enemy Data` field.

#### Step 5: Test the Enemy Variants in the Scene

1. **Place the Enemy Prefabs in the Scene**:
   - Drag and drop the `Enemy` prefab into the scene twice (once for each variant).
   - Assign the `EnemyData` for each instance (as done in the previous step).

2. **Run the Game**:
   - Hit the Play button in Unity.
   - You should now see two different enemies in the scene: **"Bad Guy - Green"** and **"Bad Guy - Blue"**, each with their own health, speed, and color based on their assigned ScriptableObject data.

#### Step 6: Extend and Modify as Needed

Now that your enemies are set up using ScriptableObjects, you can easily create more enemy types or modify existing ones without changing the core `Enemy` prefab. Simply create new ScriptableObjects and assign them to different enemy instances.

---

With these steps, you’ve successfully used ScriptableObjects to manage enemy data in Unity, allowing for flexible and modular enemy variants. This approach makes it easy to tweak properties and add variations without touching the underlying prefab.

#### Testing
- Run the scene to see the scriptable data applied to the enemy variants
- Everything should be logging across the event system and showing the UI
	- Change the scriptable data to see changes to the enemies at run time.