
## Simple JSON Save / Load Tutorial

In this exercise you will set up a very simple **save system** in Unity using **JSON serialisation**.

You will be able to:

- Store some example player data in a C# class
- Save it to a JSON file on disk
- Load it back into your game
- Delete the save file
- See the JSON file in your persistent data folder

> The scene is provided. Your job is to add the scripts and wire them up.

---
## 1. What is serialisation and JSON?

- **Serialisation** means turning an object in memory (like a C# class) into a format that can be stored or sent (like text in a file).
- **JSON** (JavaScript Object Notation) is a simple text format that looks like:
    
    ```json
    {
      "playerName": "Player One",
      "level": 1
    }
    ```
    
- Unity has a built-in helper called **`JsonUtility`** that converts C# objects to and from JSON strings.

---

## 2. Where is the file saved? (`Application.persistentDataPath`)

Unity gives you a safe folder for save files called **`Application.persistentDataPath`**.

- It is **different on each platform** (Windows, Mac, etc.) but every platform has a `persistentDataPath`.
- Unity chooses the location for you.
- We only need to know:
    - It is **writable** (we are allowed to save files there).
    - It **does not reset** every time you press Play.
    - We can print the path to the Console and then open it in Explorer / Finder.

In our code we combine this path with a file name:

```csharp
Path.Combine(Application.persistentDataPath, fileName);
```

This gives us the **full path** to our JSON save file.

---

## 3. Step 1: Create the data class (`DataObject`)

This class holds the data we want to save.  
It is just a **plain C# class**, not a MonoBehaviour.

Create a new script called **`DataObject.cs`** and add:

```csharp
using System;
using System.Collections.Generic;

// Mark this class as serialisable so Unity can turn it into JSON
[Serializable]
public class DataObject
{
    public string playerName = "Player One";
    public int level = 1;
    public float health = 100f;

    // Example array – e.g. IDs of collected items
    public int[] collectedItemIDs = new int[] { 1, 5, 9 };

    // Example list – e.g. unlocked abilities or skills
    public List<string> unlockedAbilities = new List<string>()
    {
        "Double Jump",
        "Dash",
        "Fireball"
    };
}
```

### Key points

- `[Serializable]` tells Unity that this class can be turned into JSON.
- `DataObject` is our **save data**: player name, level, health.

- We also store:
    - `int[] collectedItemIDs` → an **array** of item IDs.
    - `List<string> unlockedAbilities` → a **list** of ability names.

These make the JSON look “game related” but JSON or similar can be any form of data or used in every application.

---

## 4. Step 2: Create the serialisation manager

This MonoBehaviour will actually **save / load / delete** the JSON file.

Create **`DataSerialisationManager.cs`** and add:

```csharp
using System.IO; // For file operations
using UnityEngine;

public class DataSerialisationManager : MonoBehaviour
{
    [Header("File Settings")]
    public string fileName = "saveData.json";

    [Header("Data To Save")]
    public DataObject currentData = new DataObject();

    // Full path to the save file in the persistent data folder
    private string FilePath => Path.Combine(Application.persistentDataPath, fileName);

    // Save: convert currentData to JSON and write it to a file
    public void Save()
    {
        // Convert to JSON (prettyPrint = true so it is easy to read)
        string json = JsonUtility.ToJson(currentData, true);

        // Write JSON text to file
        File.WriteAllText(FilePath, json);

        Debug.Log($"[Save] Data saved to: {FilePath}");
        Debug.Log(json);
    }

    // Load: read JSON file and overwrite currentData
    public void Load()
    {
        if (!File.Exists(FilePath))
        {
            Debug.LogWarning("[Load] No save file found.");
            return;
        }

        // Read JSON text from file
        string json = File.ReadAllText(FilePath);

        // Turn JSON back into a DataObject
        currentData = JsonUtility.FromJson<DataObject>(json);

        Debug.Log($"[Load] Data loaded from: {FilePath}");
        Debug.Log(json);
    }

    // Delete: remove the save file from disk
    public void DeleteSave()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
            Debug.Log($"[Delete] Save file deleted: {FilePath}");
        }
        else
        {
            Debug.LogWarning("[Delete] No save file to delete.");
        }
    }
}
```

### Key points

- `currentData` holds the **data we are saving**.
- `Save()`:
    - Uses `JsonUtility.ToJson` to convert `currentData` into a JSON string.
    - Uses `File.WriteAllText` to store it in `saveData.json` inside the persistent data path.
- `Load()`:
    - Checks if the file exists.
    - Reads the JSON text.
    - Uses `JsonUtility.FromJson<DataObject>` to turn it back into a `DataObject`.
- `DeleteSave()`:
    - Deletes the file if it exists.

---

## 5. Step 3: Create the input controller (`SaveController`)

This script listens for key presses and calls the manager functions.

Create **`SaveController.cs`** and add:

```csharp
using UnityEngine;

public class SaveController : MonoBehaviour
{
    public DataSerialisationManager dataManager;

    private void Awake()
    {
        // Try to auto find the manager if it was not assigned in the Inspector
        if (dataManager == null)
        {
            dataManager = FindObjectOfType<DataSerialisationManager>();

            if (dataManager == null)
            {
                Debug.LogError("[SaveController] No DataSerialisationManager found in the scene.");
            }
        }
    }

    private void Update()
    {
        // If we do not have a reference, do nothing
        if (dataManager == null)
        {
            return;
        }

        // Press 1 to Save
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            dataManager.Save();
        }

        // Press 2 to Load
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            dataManager.Load();
        }

        // Press 3 to Delete save
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            dataManager.DeleteSave();
        }
    }
}
```

### Key points

- `SaveController` is a simple **input script**.
- It tries to **find** `DataSerialisationManager` automatically if you forget to assign it.
- Keys:
    - `1` → Save
    - `2` → Load
    - `3` → Delete

---

## 6. Step 4: Set up the scene

In the provided scene:

- Create an empty GameObject called **`DataManager`**.
- Add the **`DataSerialisationManager`** script to it.
- Create another empty GameObject called **`SaveController`** (or add the script to an existing object).
- Add the **`SaveController`** script.
- In the `SaveController` component:
    - Either leave `dataManager` empty (it will auto find).
    - Or drag the `DataManager` GameObject into the `dataManager` field.

---

## 7. Step 5: Edit data in the Inspector

Select the `DataManager` GameObject and look at the **DataSerialisationManager** component.

- Expand `currentData`.
- Change:
    - `playerName`
    - `level`
    - `health`
    - The `collectedItemIDs` array
    - The `unlockedAbilities` list

These values are what will be written into the JSON file when you save.

---

## 8. Step 6: Test saving, loading and deleting

- Press Play.
- Press:
    - **1** → Save
    - **2** → Load
    - **3** → Delete
        
- Watch the **Console**:
    - You will see log messages showing:
        - The full path of the file.
        - The JSON content.


---

## 9. Step 7: Open the JSON file (*unfortunately the folder is hidden in the Studio Computers*)

From the Console:

- Look at the log:
    - `[Save] Data saved to: C:/Users/.../AppData/.../saveData.json`
- Copy that path.
- Paste it into Explorer (Windows) or Finder (Mac) and open the folder.
- Open `saveData.json` in a text editor.

You should see something like:

```json
{
  "playerName": "Player One",
  "level": 1,
  "health": 100.0,
  "collectedItemIDs": [
    1,
    5,
    9
  ],
  "unlockedAbilities": [
    "Double Jump",
    "Dash",
    "Fireball"
  ]
}
```

Try changing values in the file (for example, change `"level": 10`) and then press **2** (Load) again in Play mode to see the updated data come back into Unity.

---

## 10. Summary

- `DataObject` = **what** we save (player data, arrays, lists, etc.).
- `DataSerialisationManager` = **how** we save, load and delete (JSON + file I/O).
- `SaveController` = **when** we call save, load and delete (key presses).

This is the basic pattern you can reuse:

- Put your game data in a serialisable class.
- Convert it to JSON.
- Save to `Application.persistentDataPath`.
- Load it later and restore the values.