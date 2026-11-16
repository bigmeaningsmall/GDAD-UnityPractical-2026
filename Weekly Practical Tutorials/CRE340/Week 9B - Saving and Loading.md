---
Order: 9-B
---

# **Week 9B : Saving and Loading**

### Overview

#### **What we are building**:

- A **`SaveData`** class - the values worth keeping between sessions.
- An **`ISaveable`** interface - anything with state to save fills in its own part.
- A static **`SaveSystem`** for the file work, and a **`SaveManager`** that ties it together.

#### **Why we are doing this**:

- Everything resets when you stop playing. Coins, XP and lives all go back to zero.
- Serialisation is turning objects in memory into text you can store or send. JSON is the usual format and Unity has it built in.
- Back in Week 5 we said we'd come back and split some jobs off the GameManager once it was carrying save and load. 

#### **Learning Objectives**:

- **`JsonUtility`** - `ToJson` and `FromJson`, and what it can and can't handle.
- **`Application.persistentDataPath`** - where save files are and why
- **An interface as a contract for a responsibility** - objects report their own state rather than a manager looking in for it.

---
---

# **Save and Load with JSON**

Carry on from Week 8. Four new scripts, all small, and two methods on the GameManager.

---

### **Step 1: What We're Saving**

Open `GameManager.cs` and look at what it holds: coins, XP, lives, game over, the player reference, the player prefab, the spawn point, delays.

Most of that shouldn't be saved.

|Value|Save it?|Why|
|---|---|---|
|`coins`|Yes|Progress the player earned|
|`xp`|Yes|Same|
|`lives`|Yes|Part of where they are in the run|
|`isGameOver`|No|Loading into a game over state is no use to anyone|
|`Player`|No|Spawned fresh every time|
|`playerPrefab`, `playerSpawnPoint`|No|Set in the Inspector, same every run|
|Enemy positions|No|The spawner makes new ones|

The rule is **save what you can't work out again**. Anything the game rebuilds on startup, or that comes from the Inspector, or that's derived from something else you're already saving, stays out. Saving extra isn't free - every field is one more thing to keep in step when the game changes.

**One thing worth noticing before we start.** Health is on `PlayerStats`, not here, and we're not saving it. The player gets destroyed and respawned, so health is temporary by design - you come back at full. Coins, XP and lives are on the GameManager precisely because they have to survive the player dying.

That split from Week 5 is what makes this week straightforward. The things worth saving are already in one place.
- Partly this is as much an example of how to fit saving and loading in to the game - The actual need for data save in this example is minimal

---

### **Step 2: The `SaveData` Class**

**Create a C# script called `SaveData.cs`** and delete the Unity boilerplate.

```csharp
// WHAT we save. Plain data - not a MonoBehaviour, no methods, goes on no GameObject.
// Everything in the game that wants to be saved adds its fields here.
[System.Serializable]
public class SaveData
{
    public int coins;
    public int xp;
    public int lives;
}
```

**Explanation**:

- **`[System.Serializable]`** is the same attribute we used on `PlayerData` in Week 2. It tells Unity this class can be converted - to JSON here, to Inspector fields there.
- **Public fields, not properties.** `JsonUtility` ignores properties completely. The GameManager keeps `coins` private behind a read-only property, which is right for the GameManager and wrong for this. Different jobs.
- **One class for the whole save**, not one per system. Add an inventory later and its fields go in here alongside these.

---

### **Step 3: The `ISaveable` Interface**

**Create a C# script called `ISaveable.cs`** and delete the Unity boilerplate.

```csharp
// Anything in the game that has state worth saving implements this.
// It fills in its own part of the save, and takes its own part back out.
public interface ISaveable
{
    void Capture(SaveData data);   // put my values into the save
    void Restore(SaveData data);   // take my values back out of it
}
```

**Explanation**:

- Third interface to add - `IDamagable` in Week 3B, `IEnemyState` for doing state, this one now. Same idea: a contract saying what a class promises to provide.
- **The object decides what it saves.** Nothing connect into the GameManager and pulls values out of it. It gets handed the save and fills in the parts it needs
- **The same `SaveData` goes to everything.** Each implementer touches only the fields it cares about and ignores the rest.
- Add an inventory later (beyond the scope) and it implements `ISaveable`. Nothing else in the save system changes.

---

### **Step 4: The `SaveSystem`**

The part that uses files and JSON.

**Create a C# script called `SaveSystem.cs`** and delete the Unity boilerplate.

```csharp
using System.IO;
using UnityEngine;

// HOW we save. Converts a SaveData to JSON, writes the file, reads it back.
// Static because there is nothing to remember between calls - same reasoning as EnemyFactory.
public static class SaveSystem
{
    private const string FileName = "savegame.json";

    // Unity picks a folder we're allowed to write to, and it's different on every platform.
    private static string FilePath
    {
        get { return Path.Combine(Application.persistentDataPath, FileName); }
    }

    public static void Save(SaveData data)
    {
        // The true turns on pretty print - line breaks and indenting so we can read it
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(FilePath, json);

        Debug.Log("Saved to " + FilePath);
        Debug.Log(json);
    }

    // Returns null when there's nothing saved, so the caller decides what to do about it
    public static SaveData Load()
    {
        if (!File.Exists(FilePath))
        {
            Debug.LogWarning("No save file at " + FilePath);
            return null;
        }

        string json = File.ReadAllText(FilePath);

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Debug.Log("Loaded from " + FilePath);

        return data;
    }

    public static bool HasSave()
    {
        return File.Exists(FilePath);
    }

    public static void Delete()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);

            Debug.Log("Save file deleted");
        }
        else
        {
            Debug.LogWarning("No save file to delete");
        }
    }
}
```

**Explanation**:

- **`JsonUtility.ToJson`** takes any serialisable object and gives you a string. **`FromJson<T>`** goes the other way, and you tell it which type to build.
- **`Path.Combine`** joins the folder and the filename with the right slash for the platform. Writing `path + "/" + name` works on Windows and Mac but not on every other OS (e.g. Android or Linus based OS).
- **`Load` returns `null` rather than an empty `SaveData`.** There's a difference between "no save file" and "a save with nothing in it", and the caller needs to know which.
- **This class knows nothing about the game.** No GameManager, no coins, no player. Hand it a `SaveData` and it writes a file. That's the whole job.  It means you could drop this script into any project unchanged.
- `using System.IO` is the .NET file library rather than a Unity one. `File.WriteAllText` and `File.ReadAllText` do the actual work.
	- Standard way to use input and output from the system library

> [!note] **Note - what `JsonUtility` can and can't handle**
> 
> **It handles:** public fields of ints, floats, bools, strings and enums; Unity types like `Vector3`, `Quaternion` and `Color`; **arrays and `List<T>` as fields**; and other `[Serializable]` classes nested as fields. Private fields work too if you mark them `[SerializeField]`. That covers most of what a save file needs.
> 
> **It doesn't handle:**
> 
> - **Properties.** Fields only, and it skips them silently - you save, you load, the value is zero and there's no error anywhere.
> - **`Dictionary`.** Use two parallel lists, or a list of a small serialisable class.
> - **Polymorphism.** See below (this is complicated to explain but the programming patterns project has a simple example)
> - **A list or array at the top level.** `JsonUtility.ToJson(myList)` gives you `{}`. Wrap the list in a class, which is what we're doing anyway.
> - **`null`.** A null string comes back as `""`, and a missing field comes back as 0 rather than complaining.
> 
> If you need more than that, **Newtonsoft Json.NET** handles all of it and Unity ships it as a package.

> [!warning] **Note - polymorphism and your inventory**
> 
> For some additional info. If we built an inventory this is what something like an `Item` hierarchy would run into.
> 
> ```csharp
> [System.Serializable]
> public class Item { public string itemName; }
> 
> [System.Serializable]
> public class HealthItem : Item { public int healAmount; }
> ```
> 
> Put a `HealthItem` into a `List<Item>`, save it, load it back, and you get an `Item` with the name and nothing else. `healAmount` is gone, and so is the fact it was ever a `HealthItem`.
> 
> `JsonUtility` writes the **declared type of the field**, not the actual type of the object. A `List<Item>` gets saved as a list of `Item`.
> 
> The usual ways round it are a flat record with a type field - `itemType` as a string or an enum, plus every field any item might need - or Json.NET, which records the real type and rebuilds it properly. Best approach for working with this is trying it in a scratch scene and looking at the JSON. 

---

### **Step 5: The `SaveManager`**

The part that knows who has data. It doesn't touch a file - it asks `SaveSystem` to do that.

**Create a C# script called `SaveManager.cs`** and put it on an empty GameObject called **Save Manager**.

```csharp
using System.Collections.Generic;
using UnityEngine;

// Keeps track of everything that has state worth saving, gathers it up,
// and hands it to the SaveSystem. Knows nothing about files or JSON.
public class SaveManager : MonoBehaviour
{
    // Static so things can register before this object exists
    private static List<ISaveable> saveables = new List<ISaveable>();

    public static void Register(ISaveable saveable)
    {
        // The Contains check stops anything registering twice
        if (!saveables.Contains(saveable))
        {
            saveables.Add(saveable);
        }
    }

    public static void Unregister(ISaveable saveable)
    {
        saveables.Remove(saveable);
    }

    public void Save()
    {
        SaveData data = new SaveData();

        // Ask everything to fill in its own part
        foreach (ISaveable saveable in saveables)
        {
            saveable.Capture(data);
        }

        SaveSystem.Save(data);
    }

    public void Load()
    {
        SaveData data = SaveSystem.Load();

        // Nothing saved yet, so leave the game as it is
        if (data == null)
        {
            return;
        }

        // Hand the same data to everything and let each take what it needs
        foreach (ISaveable saveable in saveables)
        {
            saveable.Restore(data);
        }
    }

    public void DeleteSave()
    {
        SaveSystem.Delete();
    }

    #region Debug Keys

    [Header("Debug Keys")]
    public bool useDebugKeys = true;

    private void Update()
    {
        if (!useDebugKeys)
        {
            return;
        }

        // F5 - save
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Save();
        }

        // F9 - load
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Load();
        }

        // F10 - delete the save file
        if (Input.GetKeyDown(KeyCode.F10))
        {
            DeleteSave();
        }
    }

    #endregion
}
```

**Explanation**:

- **`Register` and `Unregister` are static** so anything can call them without needing a reference to this object, and without caring whether it exists yet.
- **The two loops are the whole system.** Classes fill in the save, write it. Read it back, and pass it. Adding a system to the save doesn't change either loop.
- **`Save` and `Load` are public instance methods** so you can drag this object onto a UI Button's **OnClick** and pick them from the dropdown.
- The debug keys are in here rather than in their own script because a finished game wouldn't have them at all - it would save at a checkpoint or on quit. The toggle lets you turn them off without deleting anything.
- F5 and F9 are the usual quicksave and quickload keys.

> [!note] **Note - register, like subscribing**
> 
> This is the same shape as everything else we've built. Something announces itself in `OnEnable` and takes itself back out in `OnDisable`, and the thing collecting them never needs a reference in the Inspector.
> 
> `HealthEventManager` in Week 3, the `EventBus` in Week 6, the state from week 8. 
> 
> The alternative is for the SaveManager to go looking - search the scene for everything implementing `ISaveable` when it needs to save. That works, but it only finds MonoBehaviours in the scene, and it can't find a plain C# class like maybe an inventory that isn't on a GameObject. 

---

### **Step 6: The GameManager Implements `ISaveable`**

Now the GameManager's part, which is a small change.

**1. Add the interface to the class line:**

```csharp
public class GameManager : MonoBehaviour, ISaveable
```

**2. Add two lines to the `OnEnable` and `OnDisable` you already have**, keeping the event bus lines:

```csharp
    private void OnEnable()
    {
        EventBus<EnemyKilled>.Subscribe(HandleEnemyKilled);

        SaveManager.Register(this);
    }

    private void OnDisable()
    {
        EventBus<EnemyKilled>.Unsubscribe(HandleEnemyKilled);

        SaveManager.Unregister(this);
    }
```

**3. Add a new region** for the two interface methods:

```csharp
    #region Saving

    // Fill in the parts of the save that belong to us
    public void Capture(SaveData data)
    {
        data.coins = coins;
        data.xp = xp;
        data.lives = lives;
    }

    // Take our parts back out
    public void Restore(SaveData data)
    {
        coins = data.coins;
        xp = data.xp;
        lives = data.lives;

        RefreshUI();
    }

    // The HUD only hears about CHANGES, so after a load it needs telling the new values
    private void RefreshUI()
    {
        UIEventManager.OnCoinsChanged(coins);
        UIEventManager.OnXPChanged(xp);
        UIEventManager.OnLivesChanged(lives);
    }

    #endregion
```

**Explanation**:

- **`RefreshUI` exists because of how Week 6 works.** The HUD updates when it's told something changed. A load sets three values without going through `AddCoins` or `AddXP`, so nothing fires and the display shows the old numbers.
- **The GameManager has no idea saving involves a file.** It doesn't mention JSON, `persistentDataPath`, `SaveSystem` or any of it. Game and save are separate and the manager acts as a bridge.
- **`Capture` and `Restore` are public because the interface says so.** 

---

### **Step 7: Set Up and Test**

1. **Check the Save Manager object is in the scene** with the script on it.
2. **Press Play.** Collect some coins and kill a few enemies so you've got numbers worth saving.
3. **Press F5.** The Console prints the file path and the JSON itself.
4. **Stop, press Play again.** Everything's back to zero.
5. **Press F9.** Coins, XP and lives come back and the HUD updates.
6. **Press F10** to delete it, then F9 again - you get the warning and the game carries on unchanged.

**Open the file.** Copy the path out of the Console into Explorer or Finder. _This folder is hidden on the studio machines and needs admin to get at, so you may only manage this at home._

```json
{
    "coins": 14,
    "xp": 60,
    "lives": 2
}
```

**Edit it in a text editor** - set coins to 9999 - save it, go back to Unity and press F9. Your change is in the game.

Important to note. A plain text save file is readable and editable by anyone who finds it. For a single player game that's mostly harmless and occasionally a feature. For anything with leaderboards or purchases it isn't, which is when people start encrypting save files or keeping the real copy on a server.

> [!note] **Note - the save file exists outside your code**
> 
> `persistentDataPath` doesn't get cleared when you press Play, rebuild, or change your scripts. That's the point of it, and it causes a specific confusing bug.
> 
> Add a new field to `SaveData`, load an old save file, and the new field comes back as 0 with no error. `JsonUtility` doesn't mind fields being absent.
> 
> The usual answer is a `public int saveVersion;` field in `SaveData` - check it on load, and if it's older than the current one either upgrade the data or throw it away and start fresh. Not worth it for three ints, very much worth it once a save file has been in players' hands.

---

### **What This Bought Us**

At the end of Week 5 we listed what the GameManager does and said we'd come back once it was carrying save and load too. Here's what it does now:

- The singleton setup
- Coins and XP
- The player reference
- Spawning the player
- Lives and respawning
- Game over and restarting the level
- Listening for enemy deaths
- **Reporting its own values when something asks**

It isn't a save system but it interacts with the save system. The file handling is in `SaveSystem`, the decision about when to save is in `SaveManager`, and the GameManager contributes three ints (simple data just for the example).

The obvious alternative was to put `SaveGame` and `LoadGame` straight on the GameManager. It would have worked and it would have been fewer files. If the save system was part of the gamemanager:

- **The GameManager would need to know about every saveable thing.** Add an inventory and you'd be editing the GameManager to connect to it.
- **Nothing else could save anything** without going through a class that also spawns players and tracks lives.
- **You couldn't test or reuse any of it** without dragging the whole GameManager along.

With this arrangement, adding the inventory to the save is: add the fields to `SaveData`, put `ISaveable` on the inventory class, write `Capture` and `Restore`. 

> [!note] **Note - how this maps to the standalone example**
> 
> The standalone JSON tutorial splits it three ways and this is the same split, with one extra piece:
> 
> |Standalone|Here|Job|
> |---|---|---|
> |`DataObject`|`SaveData`|**What** gets saved|
> |`DataSerialisationManager`|`SaveSystem`|**How** - JSON and the file|
> |-|`ISaveable`|**Who** owns each part of it|
> |`SaveController`|`SaveManager`|**When** it happens|
> 
> The standalone version keeps all the data in one place and saves it directly, which is the right size for learning the JSON part on its own. Once the data is spread across several systems, `ISaveable` is what stops one class having to know about all of them.

---

### **Where this is going**

**Your own projects.** The next thing worth saving is usually an inventory or unlocks, which means lists rather than ints. Lists are fine - it's lists of _different subclasses_ that aren't, so read the polymorphism note before you design any save file. 

---

### **What we've set up**:

- A **`SaveData`** class holding only what's worth keeping.
- An **`ISaveable`** interface so each system owns its own part of the save.
- A static **`SaveSystem`** that reads and writes JSON to `persistentDataPath`.
- A **`SaveManager`** that gathers everything up, with keys to test it.
- A GameManager that reports its values and knows nothing about files.