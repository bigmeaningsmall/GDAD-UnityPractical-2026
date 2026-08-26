### Part 2: Setting Up the Inventory Manager and Display

In Part 2, we'll set up an `InventoryManager` to manage collected items and an `InventoryDisplay` to update the UI with each collected item using an event-driven structure.

The `InventoryManager` will be responsible for adding and removing items, while the `InventoryDisplay` will respond to changes and visually display the inventory in a grid format.
- Note: we are only going to implement `AddItem` fully but only test `RemoveItem`.

------------------------------------------------------------------------

#### Step 1: Setting Up the `InventoryManager`

- **Create the `InventoryManager` Script**:
  - In your **Project** window, right-click, select **Create \> C# Script**, and name it `InventoryManager`.
  - Open `InventoryManager.cs` and paste in the following code:

  ``` csharp
  using System;
  using System.Collections.Generic;
  using UnityEngine;

  public class InventoryManager : MonoBehaviour {

      public static event Action<List<InventoryItem>> OnInventoryChanged;

      public List<InventoryItem> items = new List<InventoryItem>();
      private const int MaxItems = 8; // Limit inventory to 8 items

      public bool CanAddItem() {
          return items.Count < MaxItems; // Returns true if there's room for more items
      }

      public void AddItem(InventoryItem newItem) {
          if (items.Count < MaxItems) {
              items.Add(newItem);
              OnInventoryChanged?.Invoke(items); // Notify UI about inventory change
          } else {
              Debug.Log("Inventory is full");
          }
      }

      public void RemoveItem(InventoryItem item) {
          if (items.Contains(item)) {
              items.Remove(item);
              OnInventoryChanged?.Invoke(items); // Notify UI about inventory change
          }
      }

      // For testing purposes, remove an item when R is pressed
      private void Update() {
          if (Input.GetKeyDown(KeyCode.R)) {
              if (items.Count > 0) {
                  RemoveItem(items[0]);
              }
          }
      }
  }
  ```
- **Explanation**:
  - `AddItem` and `RemoveItem` manage the inventory list. When an item is added or removed, `OnInventoryChanged` broadcasts the updated list to any subscribed UI elements.
  - `CanAddItem` ensures the inventory never exceeds 8 items.
  - `Update` temporarily includes a feature to test item removal by pressing `R`.
- **Add `InventoryManager` to the Player**:
  - Open your **PlayerPrefab**.
  - Drag the `InventoryManager` script onto the Player to add it as a component.

------------------------------------------------------------------------

#### Step 2: Setting Up the `UI_InventoryDisplay`

The `UI_InventoryDisplayInventoryDisplay` script will listen for changes to the inventory and display each item in a grid layout (2 columns by 4 rows) with icons and names.
\###### Create this script in the UI scripts folder for consistency

- **Create the `InventoryDisplay` Script**:
  - In the **Project** window, create a new script called `InventoryDisplay`.
  - Open `InventoryDisplay.cs` and paste in the following code:

  ``` csharp
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.UI;
  using TMPro;

  public class UI_InventoryDisplay : MonoBehaviour {
      public GameObject slotPrefab;     // Slot prefab to represent each inventory item
      public Transform slotParent;      // Parent transform for positioning slots
      public float xSpacing = 128f;     // Horizontal spacing between slots
      public float ySpacing = 128f;     // Vertical spacing between slots

      private List<GameObject> slots = new List<GameObject>();

      private void OnEnable() {
          InventoryManager.OnInventoryChanged += UpdateInventoryUI;
      }

      private void OnDisable() {
          InventoryManager.OnInventoryChanged -= UpdateInventoryUI;
      }

      private void UpdateInventoryUI(List<InventoryItem> items) {
          // Clear previous slots
          foreach (GameObject slot in slots) {
              Destroy(slot);
          }
          slots.Clear();

          // Create a new slot for each item in the inventory, arranged in a 2x4 grid
          for (int i = 0; i < items.Count; i++) {
              InventoryItem item = items[i];
              GameObject newSlot = Instantiate(slotPrefab, slotParent);

              // Calculate grid position
              int row = i / 2;     // 2 items per row
              int column = i % 2;  // 0 for first item in row, 1 for second item

              // Position slot based on calculated grid position
              newSlot.transform.localPosition = new Vector3(column * xSpacing, -row * ySpacing, 0);
              slots.Add(newSlot);

              // Update the slot with item data
              Image icon = newSlot.transform.Find("Icon").GetComponent<Image>();
              TextMeshProUGUI nameText = newSlot.transform.Find("NameText").GetComponent<TextMeshProUGUI>();

              icon.sprite = item.icon;
              nameText.text = item.itemName;
          }
      }
  }
  ```
- **Explanation**:
  - **Grid Positioning**: The inventory items are positioned in a 2x4 grid layout. Each slot is positioned based on `row` and `column` calculations.
  - **Dynamic Update**: The `UpdateInventoryUI` method clears the previous UI slots and re-generates them based on the current items, displaying `icon` and `itemName` for each item.

------------------------------------------------------------------------

#### Step 3: Setting Up the Inventory UI

# \*\*\*\* NOTE \*\*\*\* These UI and InventorySlotPrefab are created and imported with this weeks package

1.  **Create the Inventory UI Canvas**:
    - In the **Hierarchy** window, right-click and select **UI \> Canvas**.
    - Name the Canvas `InventoryCanvas` and ensure its **Render Mode** is set to **Screen Space - Overlay**.
2.  **Set Up Inventory Display Object**:
    - In the `InventoryCanvas`, create an empty GameObject and name it `Inventory`.
    - Attach the `InventoryDisplay` script to this `Inventory` GameObject.
    - In the **Inspector**, assign `slotParent` to be this `Inventory` GameObject itself (or create a child object for more customization).
    - Set `xSpacing` and `ySpacing` to values that position items correctly within the grid (e.g., `128`).
3.  **Create the Slot Prefab**:
    - In the **Project** window, right-click and select **Create \> UI \> Image** to create a new `Image` UI object.
    - Name it `InventorySlotPrefab` and set it up as follows:
      - Add an **Icon** child (an `Image` component) to represent the item icon.
      - Add a **NameText** child (a `TextMeshPro - Text` component) to display the item name.
    - Drag this prefab into the **Prefabs** folder.
    - Assign `slotPrefab` in the `InventoryDisplay` script to this prefab.

------------------------------------------------------------------------

### Final Testing

1.  **Add Items to the Object Spawner for Collection Testing**:
    - Locate your `ObjectSpawner` script or component in the scene.
    - Add the `InventoryItem` ScriptableObjects (such as `HealthItem`, `WeaponItem`, or `BonusItem`) to the spawner's **spawn array** or list.
    - When you play the scene, the items will spawn in the game world, allowing you to test the player's ability to collect them.
2.  **Remove Items for Testing**:
    - Press the `R` key to remove the first item in the inventory, simulating item removal.
    - The `InventoryDisplay` should update to reflect the removed item.

------------------------------------------------------------------------

### Summary

In this part, you've:
- Set up an `InventoryManager` to manage collected items and handle adding/removing items.
- Created an event-driven `InventoryDisplay` that automatically updates the inventory UI grid when items are collected or removed.
- Configured UI elements to display item icons and names in a 2x4 grid layout.

This completes the setup for a basic inventory system with an event-driven UI display in Unity!
