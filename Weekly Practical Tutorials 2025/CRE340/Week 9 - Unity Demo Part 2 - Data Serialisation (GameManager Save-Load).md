### **Unity Practical: Modifying the GameManager to Integrate SaveLoadManager**

---
### **Objective**
In this part of the practical, we will modify the `GameManager` to interact with the `SaveLoadManager` for managing player data persistence. This integration will enable features like auto-saving, loading, and clearing player data during gameplay.


---

### **Part 2: Modify the `GameManager` for Saving and Loading**

#### **Step 1: Setup the SaveLoadManager in GameManager**

1. **Add a Reference to SaveLoadManager:**
   - Open the `GameManager` script.
   - Add a private field for `SaveLoadManager` to store a reference.

```csharp
     private SaveLoadManager saveLoadManager;
```

2. **Find the SaveLoadManager Instance:**
   - In the `Start()` method, use `FindObjectOfType` to get the `SaveLoadManager` in the scene.

```csharp
     private void Start()
     {
         saveLoadManager = FindObjectOfType<SaveLoadManager>();
         if (saveLoadManager == null)
         {
             Debug.LogError("SaveLoadManager not found in the scene.");
         }
         else
         {
             if (saveLoadManager.autoLoad)
             {
                 LoadData();
             }
         }
     }
```

#### **Step 2: Update Player Properties in the GameManager**

1. **Modify Properties to Sync with SaveLoadManager:**
   - Update the `PlayerName`, `Experience`, and `Coins` properties to synchronise changes with `SaveLoadManager`.

```csharp
     public string PlayerName
     {
         get { return playerName; }
         private set
         {
             playerName = value;
             UI_EventHandler.PlayerNameChanged(playerName);
             saveLoadManager.SetPlayerName(playerName);
         }
     }

     public int Experience
     {
         get { return experience; }
         private set
         {
             experience = value;
             UI_EventHandler.ExperienceChanged(experience);
             saveLoadManager.GainExperience(experience);
         }
     }

     public int Coins
     {
         get { return coins; }
         private set
         {
             coins = value;
             UI_EventHandler.CoinsChanged(coins);
             saveLoadManager.AddCoins(coins);
         }
     }
```

- **Add public methods to the `GameManager` to interact with the XP and Coin collect**

```csharp
	public void AddExperience(int points)  
	{  
	    Experience += points;  
	    UI_EventHandler.ExperienceChanged(experience);  
	    saveLoadManager.playerProperties.experience = experience;  
	    if (saveLoadManager.autoSave){  
	        saveLoadManager.SaveData();  
	    }
	}  
	  
	public void AddCoins(int amount)  
	{  
	    coins += amount;  
	    UI_EventHandler.CoinsChanged(coins);  
	    saveLoadManager.playerProperties.coins = coins;  
	    if(saveLoadManager.autoSave){  
	        saveLoadManager.SaveData();  
	    }
	}
```


#### **Step 3: Add Save, Load, and Clear Methods**

1. **Create Methods to Save, Load, and Clear Data:**
   - Add public methods in `GameManager` to handle save, load, and clear operations.

```csharp
     public void SaveData()
     {
         saveLoadManager.SaveData();
         GetSaveData();
         UpdateUI();
     }

     public void LoadData()
     {
         saveLoadManager.LoadData();
         GetSaveData();
         UpdateUI();
     }

     public void ClearData()
     {
         ResetData();
         saveLoadManager.ClearData();
         UpdateUI();
     }
```

#### **Step 4: Auto-Save and Auto-Load**

1. **Add Save Data on Quit and Disable:**
   - Ensure data is saved when the application quits or the object is disabled.

```csharp
     private void OnApplicationQuit()
     {
         if (saveLoadManager.autoSave)
         {
             saveLoadManager.SaveData();
         }
     }

     private void OnDisable()
     {
         if (saveLoadManager.autoSave)
         {
             saveLoadManager.SaveData();
         }
     }
```


2. **Helper Methods for Data and UI Updates:**
   - Implement helper methods to reset, retrieve, and update UI with the latest data.

```csharp
     private void ResetData()
     {
         playerName = "Player1";
         playerHealth = 100;
         score = 0;
         experience = 0;
         coins = 0;
     }

     private void GetSaveData()
     {
         playerName = saveLoadManager.playerProperties.name;
         playerHealth = 100; // Not saved, kept default
         score = 0;          // Not saved, kept default
         experience = saveLoadManager.playerProperties.experience;
         coins = saveLoadManager.playerProperties.coins;
     }

     private void UpdateUI()
     {
         UI_EventHandler.PlayerNameChanged(playerName);
         UI_EventHandler.PlayerHealthChanged(playerHealth);
         UI_EventHandler.ScoreChanged(score);
         UI_EventHandler.ExperienceChanged(experience);
         UI_EventHandler.CoinsChanged(coins);
     }
```

---


### **Step 5: Add XP and Coins from the Enemy and CoinCollect**

`Enemy.cs` when you kill an enemy add XP via the `GameManager`

```c#

// Increase the player's experience based on enemy health  
GameManager.Instance.AddExperience(1 * enemyData.health);

```

`CollectableCoin.cs`  Add the coin value to the `GameManager`

```c#

// Add coins to the GameManager  
GameManager.Instance.AddCoins(coinValue);

```


**You should now (hopefully) be able to add data to the `GameManager` that automatically saves/loads**

### **Step 6: Connect UI Buttons**

1. **Create UI Buttons:**
   - Add three UI buttons to your canvas in the Unity Editor:
     - Save Button
     - Load Button
     - Clear Button

2. **Assign Button Actions:**
   - Select each button in the Hierarchy.
   - In the **Inspector**, locate the `OnClick()` section.
   - Click the `+` icon to add a new event.
   - Drag the GameObject with the `GameManager` script attached into the object field.
   - From the dropdown, select the corresponding method:
     - Save Button → `GameManager.SaveData()`
     - Load Button → `GameManager.LoadData()`
     - Clear Button → `GameManager.ClearData()`

---

### **Testing the Integration**

1. **Set Up SaveLoadManager:**
   - Ensure the `SaveLoadManager` script is attached to a GameObject in the scene.

2. **Set Save/Load Options:**
   - Enable or disable `autoSave` and `autoLoad` in the `SaveLoadManager` Inspector.

3. **Test UI Buttons:**
   - Run the game and click the buttons to test saving, loading, and clearing data.

4. **Modify Values:**
   - Change values (e.g., `PlayerName`, `Experience`, or `Coins`) in the Unity Inspector during runtime.
   - Use the Save button to persist changes and Load button to retrieve them.

5. **Test Auto-Save and Load:**
   - Enable `autoSave` and `autoLoad` in the `SaveLoadManager`.
   - Modify values during gameplay, exit Play mode, and restart the game to confirm changes persist.

---
