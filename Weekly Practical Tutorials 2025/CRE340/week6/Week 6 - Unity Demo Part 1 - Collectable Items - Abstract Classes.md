### Part 1: Setting Up the Inventory Item System and Collectable Items

This guide will walk you through creating an abstract `InventoryItem` class with multiple item types, setting up collectible items, and adding items to the inventory when the player interacts with them.

------------------------------------------------------------------------

### \*\*\*\* Note : We will use an Enum for our data type - Add this in 'Scripts/Inventory/Enums.cs'

- **Set Up ItemType Enum**:
  - Right-click in the **Project** window, select **Create \> C# Script**, and name it `ItemType`.
  - Open `ItemType.cs` and define the different item categories:

  ``` csharp
  public enum ItemType {
      Health,
      Weapon,
      Bonus
  }
  ```

#### Step 1: Create the Abstract `InventoryItem` Class Using Scriptable Object

- **Create a New Script**:
  - In your Unity project, right-click in the **Project** window, navigate to **Create \> C# Script**, and name it `InventoryItem`.
  - Open `InventoryItem.cs` and replace any default code with the following:

  ``` csharp
  using UnityEngine;

  public abstract class InventoryItem : ScriptableObject {
      public string itemName;      // Name of the item
      public Sprite icon;          // Icon to display in the inventory UI
      public ItemType itemType;    // Category of the item (e.g., Health, Weapon, Bonus)
  }
  ```
- **Explanation**:
  - This abstract class will serve as the base for all items in the inventory system, defining shared properties, such as `itemName`, `icon`, and `itemType`.
  - `ItemType` should be an enumeration (enum) that lists different item types (we will create that next).

------------------------------------------------------------------------

#### Step 2: Create Specific Item Types as Scriptable Objects

Now, we'll create item types that inherit from `InventoryItem`, with additional properties specific to each type.

- **Health Item**:
  - Create a new script called `HealthItem.cs` and replace any default code with the following:

  ``` csharp
  using UnityEngine;

  [CreateAssetMenu(fileName = "NewHealthItem", menuName = "Inventory/Health Item")]
  public class HealthItem : InventoryItem {
      public int healingAmount;

      private void OnEnable() {
          itemType = ItemType.Health;
      }
  }
  ```

  - **Explanation**: `CreateAssetMenu` allows you to create `HealthItem` ScriptableObjects in the Unity Editor. `healingAmount` specifies the amount of health restored by this item.
- **Weapon Item**:
  - Create a new script called `WeaponItem.cs` and replace any default code with the following:

  ``` csharp
  using UnityEngine;

  [CreateAssetMenu(fileName = "NewWeaponItem", menuName = "Inventory/Weapon Item")]
  public class WeaponItem : InventoryItem {
      public int damage;
      public int ammoCount;

      private void OnEnable() {
          itemType = ItemType.Weapon;
      }
  }
  ```

  - **Explanation**: `damage` defines the weapon's damage, and `ammoCount` tracks the available ammo.
- **Bonus Item**:
  - Create a new script called `BonusItem.cs` and replace any default code with the following:

  ``` csharp
  using UnityEngine;

  [CreateAssetMenu(fileName = "NewBonusItem", menuName = "Inventory/Bonus Item")]
  public class BonusItem : InventoryItem {
      public int bonusPoints;

      private void OnEnable() {
          itemType = ItemType.Bonus;
      }
  }
  ```

  - **Explanation**: `bonusPoints` is specific to items that provide some kind of bonus in the game.
- **Create Scriptable Objects in the Editor**:
  - In the **Project** window, right-click and select **Create \> Inventory \> Health Item**, **Weapon Item**, or **Bonus Item** to create instances of each type.
  - Customize each item by setting its `itemName`, `icon`, and other properties (such as `healingAmount` for health items).
    - Icons can be found in the Shared Assets Icons folder you imported

------------------------------------------------------------------------

#### Step 3: Setting Up the Collectable Item Script

The `CollectableItem` script will be attached to objects in the game world that the player can collect, such as health packs, weapons, or bonus items.

1.  **Create the CollectableItem Script**:
    - Create a new script called `CollectableItem.cs` and replace any default code with the following:

##### **Note: This will be an error until we have created the `InventoryManager` (comment the lines for now)**

``` csharp
using UnityEngine;

public class CollectableItem : MonoBehaviour 
{
    public InventoryItem itemData; // Reference to the ScriptableObject

    private InventoryManager inventoryManager; // Reference to the InventoryManager - we will use this to add the item to the inventory
 
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) { // Ensure the player is the one collecting
            GameObject player = other.gameObject;
            if (player.GetComponent<InventoryManager>() != null) {
                inventoryManager = player.GetComponent<InventoryManager>(); // Get the InventoryManager reference
                
                // Check if there's room in the inventory - NOTE: THIS WILL CAUSE AND ERROR UNTIL THE INVENTORY MANAGER IS CREATED (PART 2)
                if (inventoryManager.CanAddItem()) {
                    Collect(); // Collect the item
                } else {
                    Debug.Log("Cannot collect item, inventory is full");
                }
            }
        }
    }
 
    public void Collect() {
        inventoryManager.AddItem(itemData); // Add the item to the inventory
        Collected();
    }

    private void Collected() {
        Destroy(gameObject); // Optionally destroy the item in the scene after collection
    }
}
```

- **Explanation**:
  - `CollectableItem` references an `InventoryItem` ScriptableObject (`itemData`), representing the item to be added to the inventory.
  - `OnTriggerEnter` checks if the colliding object has the `Player` tag and tries to locate an `InventoryManager` component on the player.
  - If space is available in the inventory, `Collect()` is called, adding the item to the inventory and then destroying the collectible in the game world.
- **Setting Up Collectible Items in the Editor**:
  - *The prefabs folder has the Health, Weapon and Bonus prefab set up. You will need to add the script and assign the scriptable object to them.*
  - Add this script to any GameObject you want to make collectible (e.g., a health pack model).
  - Assign the `InventoryItem` field in the **Inspector** to link it to a specific item ScriptableObject (such as a `HealthItem` or `WeaponItem`).
  - Make sure the GameObject has a `Collider` component (e.g., `BoxCollider` or `SphereCollider`) and check the **Is Trigger** option to allow trigger-based collection.

------------------------------------------------------------------------

This concludes Part 1. In this section, you have:
- Set up an abstract `InventoryItem` class with common item properties.
- Created specific item types (`HealthItem`, `WeaponItem`, and `BonusItem`) using ScriptableObjects.
- Created a `CollectableItem` script to enable item collection in the game world.

In **Part 2**, we will:
- Implement `InventoryManager` to manage collected items.
- Set up an inventory UI that updates in response to collected items using an observer event-driven structure.
