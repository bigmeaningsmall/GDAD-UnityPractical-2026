## **Unity Practical: Data Serialisation with JSON Utility**

### **Objective**
In this practical, you will learn to implement data serialisation in Unity using JSON. You'll create a `PlayerProperties` class to define player data, a `SaveLoadManager` to handle saving and loading operations, and later integrate these into a `GameManager`.

---
### **Part 1: Creating the `PlayerProperties` Class**

#### **Purpose**
The `PlayerProperties` class is used to define the structure of data we want to save. This includes properties like the player's name, experience, coins, and inventory.

#### **Steps**

1. **Create the Class:**
   - Right-click in your **Scripts** folder in Unity and create a new C# script called `PlayerProperties.cs`.
   - Replace the default code with the following:

```csharp
     using System.Collections.Generic;

     [System.Serializable]
     public class PlayerProperties
     {
         public string name;
         public int experience;
         public int coins;
         public List<string> inventory;

         // Constructor for custom player data
         public PlayerProperties(string name, int experience, int coins, List<string> inventory)
         {
             this.name = name;
             this.experience = experience;
             this.coins = coins;
             this.inventory = inventory;
         }

         // Default constructor for empty player data
         public PlayerProperties()
         {
             name = "Player";
             experience = 0;
             coins = 0;
             inventory = new List<string>();
         }
     }
```

2. **Explanation of Code:**
   - The `[System.Serializable]` attribute allows Unity's JSON Utility to serialise this class.
   - **Constructors:**
     - One constructor allows custom initialisation (e.g., starting with specific data).
     - The default constructor sets basic initial values.

3. **Save Your Script:**
   Save the file and return to Unity. Ensure there are no compile errors.

---
---

#### **Part 2: Implementation of the `SaveLoadManager`**

### **Overview**

The `SaveLoadManager` script is the backbone of the save/load system. It manages serialisation and deserialisation of the `PlayerProperties` data to and from a JSON file. 

---

### **Steps to Create the SaveLoadManager**

1. **Create a New Script:**
   - In Unity, right-click in your **Scripts** folder and select **Create > C# Script**.
   - Name the script `SaveLoadManager`.

2. **Write in the Following Code or Copy in Parts :**

   **Ensure to read the code carefully to understand the typical serialisation class methods**
   - Initialisation
   - Save
   - Load
   - Clear
   - Modify Methods

```csharp
   // Purpose: Save and load player data to and from a JSON file.
   using System.IO;
   using UnityEngine;

   public class SaveLoadManager : MonoBehaviour
   {
       [Header("Save and Load Options")]
       [Space(10)]
       public bool autoLoad; // Option to auto-load data
       public bool autoSave; // Option to auto-save data

       [Header("Player Properties to Save and Load")]
       [Space(10)]
       public PlayerProperties playerProperties;

       private string filePath; // File path to save and load data

       #region Setup and Initialization
       private void Awake()
       {
           // Set the file path to the persistent data path
           filePath = Application.persistentDataPath + "/playerData.json";

           // Initialize with default data if no existing data is loaded
           if (playerProperties == null)
           {
               playerProperties = new PlayerProperties();
           }

           // Auto-load data if the option is enabled
           if (autoLoad)
           {
               LoadData();
           }
       }
       #endregion
       
       #region Save Load Clear Data
       public void LoadData()
       {
           // Check if the file exists
           if (File.Exists(filePath))
           {
               string json = File.ReadAllText(filePath);
               playerProperties = JsonUtility.FromJson<PlayerProperties>(json);
               Debug.Log("Data loaded from " + filePath);
           }
           else
           {
               Debug.LogWarning("Save file not found at " + filePath);
           }
       }

       public void SaveData()
       {
           // Convert the player data to JSON format
           string json = JsonUtility.ToJson(playerProperties, true);
           File.WriteAllText(filePath, json);
           Debug.Log("Data saved to " + filePath);
       }
       
       public void ClearData()
       {
           // Check if the file exists
           if (File.Exists(filePath))
           {
               File.Delete(filePath);
               Debug.Log("Save data cleared from " + filePath);
           }
           else
           {
               Debug.LogWarning("No save file to delete at " + filePath);
           }

           // Reset player properties to default state
           playerProperties = new PlayerProperties();
       }
       #endregion
       
       #region Modify Player Data Methods
       public void AddToInventory(string item)
       {
           playerProperties.inventory.Add(item);
           Debug.Log(item + " added to inventory.");
       }

       public void GainExperience(int amount)
       {
           playerProperties.experience += amount;
           Debug.Log("Gained " + amount + " experience. Total: " + playerProperties.experience);
       }

       public void AddCoins(int amount)
       {
           playerProperties.coins += amount;
           Debug.Log("Gained " + amount + " coins. Total: " + playerProperties.coins);
       }

       public void SetPlayerName(string name)
       {
           playerProperties.name = name;
           Debug.Log("Player name set to " + name);
       }
       #endregion
   }
```

---

### **Explanation**

1. **Script Setup:**
   - The file path is initialised in the `Awake()` method using `Application.persistentDataPath`, ensuring the save file is stored in a platform-independent location.

2. **Key Methods:**
   - **`SaveData`:** Converts `PlayerProperties` to a JSON string and saves it to a file.
   - **`LoadData`:** Reads the JSON file and deserialises it into `PlayerProperties`.
   - **`ClearData`:** Deletes the save file and resets player data to default values.

3. **Additional Features:**
   - Public methods like `AddToInventory`, `GainExperience`, and `SetPlayerName` allow you to modify the player data directly.

---

### **Testing the Script**

1. **Attach to GameObject:**
   - Add the script to the  `SaveLoadManager` to the GameManager GameObject in the scene.

1. **Test Save and Load:**
   - In Play mode, interact with the `SaveLoadManager` via the Unity Inspector to manually call `SaveData()`, `LoadData()`, and `ClearData()` methods.
      - You can do this by adding key presses to the scrip to call functions
      - *Part 2 will add ways to interact using the `GameManager` and UI buttons*
   - View the logs in the Console to confirm the save/load operations.

1. **Check the Save File:**
   - Navigate to the save file location using `Application.persistentDataPath` in your Unity Console.
      - This folder is hidden on University computers and require admin to view unfortunately 

---

### **Next Steps**

In the next part, you will integrate this system with the `GameManager` to handle saving, loading, and modifying data dynamically during gameplay. 