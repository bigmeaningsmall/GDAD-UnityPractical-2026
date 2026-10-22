---
Order: 6-A
---
---

## Order: 6-A

# **Week 6 - Part 1 : Observer and MVP**

### Overview

#### **What we are building**:
- A **HUD** showing health, mana, coins, XP and lives, plus a game over panel.
- Health and mana get shown as **text and a sliding bar**.
- The UI updates itself when things change, using the **Observer** pattern.
- We'll split the UI into a **View** and a **Presenter**, which is the **MVP** structure.
#### **Why we are doing this**:
- Everything we've built is reporting to the Console, which is no use to players
- The GameManager and PlayerStats shouldn't have to know a UI exists. If they hold references to text objects then changing the UI means editing gameplay code.
- Observer and MVP are both on the module spec, and the UI is the cleanest place to show them.
#### **Learning Objectives**:
- **Observer** - a subject announces changes, observers subscribe.
- **MVP** - splitting Model, View and Presenter so each one has a single job.
- **Polling vs events** - why checking every frame is the wrong default.
- **Initial state** - getting the display right before anything has changed.
- **Composition** - a small reusable component doing one job on the view side.

---

---

# **Part 1: Getting the Game on Screen**

Carry on from Week 5. You should have the GameManager tracking coins, XP and lives, with the player spawning and respawning.

---

### **Step 1: The Obvious Way First** - Skip this step if you want

Before we do this _properly_, we'll show it the way most people update UIs.

1. **Make a TextMeshPro object** in the Canvas called **Text - Coins**.
2. **Create a script called `CoinDisplayPolling.cs`** and attach it to that object:

```csharp
using UnityEngine;
using TMPro;

public class CoinDisplayPolling : MonoBehaviour
{
    private TextMeshProUGUI coinText;

    private void Start()
    {
        coinText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        // Ask the GameManager for the coin count. Every single frame
        coinText.text = "Coins: " + GameManager.Instance.Coins;
    }
}
```

3. **Press Play** and collect a coin. It works fine.

What's wrong with it?

- It runs **every frame**, whether anything changed or not. At 60fps that's 60 checks a second to find out that the number is still 0.
- One display is fine. Now add health, mana, XP, lives, an inventory, a minimap. Every one of them polling every frame. Extrapolate that to a city builder or grand strategy and its a huge ammount of updates.
- Each UI display has to **know where to look**. This one knows about `GameManager`. A health display would need to know about `PlayerStats`, and to find the player first.

The alternative is to let the game **tell** the UI when something changes, and for the UI to sit quietly the rest of the time. That's the **Observer** pattern. It's the same mechanism we're already using.

**Delete `CoinDisplayPolling.cs`** and the component. We're going to build a proper UI.

> [!note] **Note - polling isn't always wrong**
> 
> There are cases where checking every frame is the right way to do it - anything continuous, like a cooldown bar filling or a speedometer. Firing an event every frame for those could be worse.
> 
> The question is whether the value changes **often and smoothly** or **occasionally and in steps**. Coins change when you pick one up, which could be ten seconds apart. Those are the ones for an event.

---

### **Step 2: Create the `UIEventManager`**

Same shape as `HealthEventManager` from Week 3 - a static class holding events, no data and no logic of its own.

1. **Create a C# script called `UIEventManager.cs`**. Delete the Unity boilerplate so its not a MonoBehaviour. Just another `static` event class.

```csharp
// UIEventManager.cs
// A static class holding the UI events. It stores nothing. its just the events
// Things that change announce it here, and anything interested subscribes.

public static class UIEventManager
{
    // ---------- Player ----------

    public delegate void StatEvent(int current, int max);

    public static StatEvent OnHealthChanged;
    public static StatEvent OnManaChanged;

    public static void HealthChanged(int current, int max)
    {
        if (OnHealthChanged != null)
        {
            OnHealthChanged(current, max);
        }
    }

    public static void ManaChanged(int current, int max)
    {
        if (OnManaChanged != null)
        {
            OnManaChanged(current, max);
        }
    }

    // ---------- Progression ----------

    public delegate void ValueEvent(int value);

    public static ValueEvent OnCoinsChanged;
    public static ValueEvent OnXPChanged;
    public static ValueEvent OnLivesChanged;

    public static void CoinsChanged(int value)
    {
        if (OnCoinsChanged != null)
        {
            OnCoinsChanged(value);
        }
    }

    public static void XPChanged(int value)
    {
        if (OnXPChanged != null)
        {
            OnXPChanged(value);
        }
    }

    public static void LivesChanged(int value)
    {
        if (OnLivesChanged != null)
        {
            OnLivesChanged(value);
        }
    }

    // ---------- Game State ----------

    public delegate void SimpleEvent();

    public static SimpleEvent OnGameOver;

    public static void GameOver()
    {
        if (OnGameOver != null)
        {
            OnGameOver();
        }
    }
}
```

**Explanation**:

- Two delegates cover everything. `StatEvent` carries a current and a max, which is what a health or mana bar needs. `ValueEvent` carries a single number.
- Each event has a **method that fires it**. In Week 3 we wrote the null check out by hand everywhere the event was raised - this way it's written once and the calling code is a single line.
- Naming follows what we settled on in Week 4 - broadcasters are called `SomethingEventManager`, so this matches `HealthEventManager`.

> [!note] **Note - two event systems in one project is fine**
> 
> `HealthEventManager` deals with damage, and it still works exactly as it did. `UIEventManager` deals with display values. They don't overlap and neither needs to know about the other.
> 
> You'll often see projects try to funnel everything through one event system. Separate ones per concern are usually easier to follow, as long as the split is obvious.

---

### **Step 3: Make the Game Broadcast**

Now the parts that own the data announce when it changes.

**In `GameManager.cs`** - update the two progression methods:

```csharp
    public void AddCoins(int amount)
    {
        coins = coins + amount;

        // Announce it. No idea what's listening and we don't have to know
        UIEventManager.CoinsChanged(coins);
    }

    public void AddXP(int amount)
    {
        xp = xp + amount;

		//same thing we just announce the value has changed
        UIEventManager.XPChanged(xp);
    }
```

The `Debug.Log` lines can be removed - the UI shows this now.

**Still in `GameManager.cs`** - update `PlayerDied` and `GameOver`:

```csharp
    public void PlayerDied()
    {
        lives = lives - 1;

        UIEventManager.LivesChanged(lives);

        if (lives > 0)
        {
            StartCoroutine(RespawnAfterDelay());
        }
        else
        {
            GameOver();
        }
    }
```

```csharp
    public void GameOver()
    {
        isGameOver = true;

        UIEventManager.GameOver();

        StartCoroutine(RestartAfterDelay());
    }
```

**In `PlayerStats.cs`** - announce health and mana whenever they change:

```csharp
    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;

        // A new player has appeared - tell the UI what our stats are
        UIEventManager.HealthChanged(data.currentHealth, data.maxHealth);
        UIEventManager.ManaChanged(data.currentMana, data.maxMana);
    }
```

Then add a line to each of the three methods that change the values:

```csharp
    public void RestoreHealth(int amount)
    {
        data.currentHealth = data.currentHealth + amount;
        data.currentHealth = Mathf.Clamp(data.currentHealth, 0, data.maxHealth);

        UIEventManager.HealthChanged(data.currentHealth, data.maxHealth);
    }

    public void RestoreMana(int amount)
    {
        data.currentMana = data.currentMana + amount;
        data.currentMana = Mathf.Clamp(data.currentMana, 0, data.maxMana);

        UIEventManager.ManaChanged(data.currentMana, data.maxMana);
    }
```

And in `TakeDamage`, after the clamp:

```csharp
        UIEventManager.HealthChanged(data.currentHealth, data.maxHealth);
```

**Explanation**:

- Every one of these is a single line, added where the value actually changes.
- Nothing has subscribed yet, so this does nothing at all. The game runs exactly as before.
- Announcing in `Start` matters for respawning - a new `PlayerStats` is created with a full health bar, and the HUD needs updated.

---

### **Step 4: The Bar**

Before the View itself we'll make a small component for the health and mana bars.

A bar does one thing - it take a value and slide a slider towards it. Its a different kind of job from setting a piece of text, so it gets its own component rather than being part of the View.

1. **Create a C# script called `UIBar.cs`**:

```csharp
using UnityEngine;
using UnityEngine.UI;   // needed for Slider

// A single bar that slides towards a value.
// Put it on any Slider - player health, mana, an enemy's health, a boss bar,
// a cooldown. It has no idea what the number means.
public class UIBar : MonoBehaviour
{
    public Slider slider;
    public float speed = 8f;      // how fast it catches up - set it high for instant

    private float target = 1f;    // where we're heading. 0 is empty, 1 is full

    private void Awake()
    {
        // If nothing was dragged in, look for a Slider on this object
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
    }

    // Give it a current and a max and it works out the rest
    public void SetValue(int current, int max)
    {
        if (max <= 0)
        {
            target = 0f;   // avoid dividing by zero
            return;
        }

        // The (float) casts matter - int divided by int is integer division,
        // so 50 / 100 would give us 0 rather than 0.5 //maths and programming!
        target = (float)current / (float)max;
    }

    private void Update()
    {
        if (slider == null)
        {
            return;
        }

        // Move a little closer to the target every frame
        slider.value = Mathf.Lerp(slider.value, target, Time.deltaTime * speed);
    }
}
```

**Explanation**:
- `SetValue` takes the same `current` and `max` the health event carries, and turns it into a 0 to 1 fraction.
- `Update` walks the slider towards that fraction. The bar holds its own target, which is state about display rather than about the game.
- `speed` is the feel dial, and each bar has its own. Set it to 2 for a slow drain, 50 and you won't see the slide at all. Mana could drain slower than health if you wanted.
- It knows nothing about health, mana, the player or the GameManager. Drop it on an enemy health bar or a cooldown and it works the same.

> [!note] **Note - why a separate component instead of code inside the View?**
> 
> Same reason as `ObjectDestruction` back in Week 3 - if the animation existed inside `HUDView` then only the HUD could ever use it.
> 
> There's a second reason too. Setting a piece of text is immediate and needs no memory of anything. Sliding a bar happens over time and needs a target and an `Update` loop. Those are two different kinds of job, and keeping them apart means the View stays a simple pass-through.
> 
> **This is not a fourth part of MVP.** `UIBar` is a small component the View is built out of - composition on the view side. The three MVP roles are still Model, View and Presenter.
> 
> With the UIBar we are technically extending the functionality of the slider to be able to lerp

> [!note] **Note - about this use of `Mathf.Lerp`**
> 
> Lerping towards a target like this is the quick way to smooth a value and you'll see it in a lot of Unity code. Two things worth knowing about it:
> 
> - It gets closer and closer to the target but never exactly arrives. Fine for a bar, nobody can see the last fraction of a pixel.
> - The speed is tied to frame rate, so it moves slightly differently on a machine running at 30fps and one at 144fps.
> 
> Neither matters here. They would matter for anything gameplay-critical, where you'd lerp over a **fixed duration** instead - the `ScaleObject` coroutine back in Week 3 Part 4 is that version, using `elapsedTime / duration`.
> 
> *Linear Interpolation (Lerp) is a big topic and we'll cover it in detail as an isolated tutorial.* 

---

### **Step 5: The View**

Now the UI side. We're splitting it into two classes. This is purely based on doing the standard pattern 'Model View Presenter'.

The View knows about TextMeshPro and the bars we just made, and nothing else.

1. **Create a C# script called `HUDView.cs`**:

```csharp
using UnityEngine;
using TMPro;

// The VIEW - it knows how to display things and nothing else.
// No events, no GameManager, no game logic. Give it a number and it shows the number.
public class HUDView : MonoBehaviour
{
    [Header("Player - Text")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI manaText;

    [Header("Player - Bars")]
    public UIBar healthBar;
    public UIBar manaBar;

    [Header("Progression")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI livesText;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    private void Awake()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void ShowHealth(int current, int max)
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + current + " / " + max;
        }

        if (healthBar != null)
        {
            healthBar.SetValue(current, max);
        }
    }

    public void ShowMana(int current, int max)
    {
        if (manaText != null)
        {
            manaText.text = "Mana: " + current + " / " + max;
        }

        if (manaBar != null)
        {
            manaBar.SetValue(current, max);
        }
    }

    public void ShowCoins(int value)
    {
        if (coinsText != null)
        {
            coinsText.text = "Coins: " + value;
        }
    }

    public void ShowXP(int value)
    {
        if (xpText != null)
        {
            xpText.text = "XP: " + value;
        }
    }

    public void ShowLives(int value)
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + value;
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}
```

**Explanation**:

- Every method does one thing - take a value and pass it on to whatever displays it. `ShowHealth` updates two things now, the text and the bar, but it isn't doing the work itself.
- No `Update`, no stored values, and no `using UnityEngine.UI` - all of that moved into `UIBar`. The View is back to being a pass-through.
- The null checks mean you can leave a field empty while you're building the UI and nothing breaks. You can run with just text, just bars, or both.
- This class could be dropped into a completely different game. It has no idea what a coin is.

> [!note] **Note - this is where visual polish goes**
> 
> These methods - and small components like `UIBar` - are the place to add anything that makes the UI feel good. The sliding bar is the first bit of that - next would be a number that counts up rather than snapping, or a scale punch when coins increase.
> 
> Because all of that lives in the View, you can add it without touching a single line of gameplay code. Worth keeping in mind for the VFX and game feel work were going to do.
> 
> Following structure conventions is really intended to segment up development and add flexibility. The UI and effects people can work away on the front end and make the interface juicy and the gameplay programmer can work away on the backend without each interfering or breaking each others changes.

---

### **Step 6: The Presenter**

The **Presenter** listens to the game and tells the View what to show. It knows about the game, and it knows nothing about TextMeshPro.

1. **Create a C# script called `HUDPresenter.cs`**:

```csharp
using UnityEngine;

// The PRESENTER - it listens to the game and tells the view what to display.
// It knows about GameManager and PlayerStats, but nothing about text or canvases.
public class HUDPresenter : MonoBehaviour
{
    public HUDView view;   // drag the HUDView component here - OR itg could be on the same object and get it on awake

    private void OnEnable()
    {
        UIEventManager.OnHealthChanged += HandleHealthChanged;
        UIEventManager.OnManaChanged += HandleManaChanged;
        UIEventManager.OnCoinsChanged += HandleCoinsChanged;
        UIEventManager.OnXPChanged += HandleXPChanged;
        UIEventManager.OnLivesChanged += HandleLivesChanged;
        UIEventManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        UIEventManager.OnHealthChanged -= HandleHealthChanged;
        UIEventManager.OnManaChanged -= HandleManaChanged;
        UIEventManager.OnCoinsChanged -= HandleCoinsChanged;
        UIEventManager.OnXPChanged -= HandleXPChanged;
        UIEventManager.OnLivesChanged -= HandleLivesChanged;
        UIEventManager.OnGameOver -= HandleGameOver;
    }

    private void Start()
    {
        // Events only tell us about CHANGES. At startup nothing has changed yet,
        // so we ask for the current values once to fill the display in. We cant really avoid having to initialise the UI
        RefreshAll();
    }

    private void RefreshAll()
    {
        view.ShowCoins(GameManager.Instance.Coins);
        view.ShowXP(GameManager.Instance.XP);
        view.ShowLives(GameManager.Instance.Lives);
    }

    private void HandleHealthChanged(int current, int max)
    {
        view.ShowHealth(current, max);
    }

    private void HandleManaChanged(int current, int max)
    {
        view.ShowMana(current, max);
    }

    private void HandleCoinsChanged(int value)
    {
        view.ShowCoins(value);
    }

    private void HandleXPChanged(int value)
    {
        view.ShowXP(value);
    }

    private void HandleLivesChanged(int value)
    {
        view.ShowLives(value);
    }

    private void HandleGameOver()
    {
        view.ShowGameOver();
    }
}
```

**Explanation**:

- Subscribe in `OnEnable`, unsubscribe in `OnDisable`. Same setup as every event we've used since Week 3.
- **`RefreshAll` in `Start` solves the startup problem.** An event only fires when something changes, so at the moment the game begins the UI has been told nothing. Reading the current values once fills it in.
- Notice `RefreshAll` uses `GameManager.Instance` - the singleton. That's the split I've mentioned : **the singleton is how you find something, events are how it tells you it changed.** We need both in this design.
- Player health isn't in `RefreshAll` because the player is spawned by the GameManager and announces its own stats in `Start`, which happens after this runs.
    - This is where game programming can get a little hacky but thats the compromise we have make sometimes.

> [!note] **Note - the Presenter has no idea the bars exist**
> 
> Look back at what we added in Steps 4 and 5 - a whole new component, a target value and an `Update` loop - and none of it shows up here. The Presenter still calls `view.ShowHealth(current, max)` exactly as it would have with text alone.
> 
> That's the MVP split doing its job. Everything about how things look stayed on one side of the line.

---

### **Step 7: Build the UI** - My package/scene should have it built already

1. **In the Canvas**, add TextMeshPro objects for **Health**, **Mana**, **Coins**, **XP** and **Lives**. Arrange them however you like - top-left corner works.
2. **Make the health and mana bars**:
    - `UI -> Slider`. Rename it **Slider - Health**.
    - In the Inspector set **Min Value** to 0 and **Max Value** to 1.
    - Untick **Interactable** so the player can't drag it, and set **Transition** to **None**.
    - Delete the **Handle Slide Area** child - a health bar doesn't need a handle.
    - Select the **Fill** child and colour it red.
    - `Add Component -> UI Bar` on the slider object. Leave the **Slider** field empty, it finds the one on its own object in `Awake`.
    - **Duplicate it** for **Slider - Mana**, colour the fill blue.
3. **Make a game over panel**: an empty UI object with a background image and a TextMeshPro saying GAME OVER. Leave it active for now so you can position it, the View disables it on `Awake`.
4. **Create an empty GameObject** called **HUD** in the Canvas.
    - `Add Component -> HUD View`. Drag each text object onto its field, the two sliders onto **Health Bar** and **Mana Bar**, and the panel onto **Game Over Panel**.
    - `Add Component -> HUD Presenter`. Drag the **HUD** object itself onto the **View** field.

---

### **Step 8: Test It**

1. **Press Play**. Coins, XP and lives show their starting values straight away, health and mana appear when the player spawns.
2. **Collect a coin** - the coin display updates and nothing else does.
3. **Take damage, heal, kill enemies, die** - each one updates its own part of the HUD.
4. **Watch the bars** - the text snaps to the new number straight away, the bar slides to catch up. Change **Speed** on a `UIBar` while playing to find a rate you like.
5. **Lose all your lives** - the game over panel appears, then the level reloads.

Worth checking what you _didn't_ have to do:

6. **Open `GameManager.cs` and `PlayerStats.cs`.** Neither mentions TextMeshPro, the Canvas, or the HUD. They announce that something changed and carry on.
7. **Disable the HUD object** while playing. The game runs exactly the same with no UI attached at all.

---

### **MVP - what the three parts are**

**Model** - `GameManager` and `PlayerStats`. They contain the data and the rules. They announce changes and know nothing about display.

**View** - `HUDView`. Knows how to put things on screen. No game logic.

**Presenter** - `HUDPresenter`. Sits between them. Listens to the model, decides what the view should show.

The thing that makes it worth the extra class is that each part can be replaced on its own:

- Rebuild the UI layout, or swap TextMeshPro for sliders and bars - only the View changes.
- Change how coins are earned - only the Model changes.
- Show health as a percentage rather than a number - only the Presenter changes.

> [!note] **Note - where does `UIBar` sit in all this?**
> 
> It's part of the View side, not a fourth role. `HUDView` is built out of smaller display components, and `UIBar` is one of them - the same way the Player is built out of a controller, stats and a collider.
> 
> MVP describes the three layers. How many classes each layer is made of is up to you.

> [!note] **Note - is a Presenter worth it for five text fields?**
> 
> Not really at this size. You could merge the View and Presenter into one class and nothing bad would happen on a project this small. Same pattern and I tend to do this and just call it `UI_Display`
> 
> It's worth knowing the structure because it can be very useful - if you want two different HUD layouts, or a pause menu showing the same values, or you're going to put the game on a device with a different screen.
> 
> Like most of these patterns, the question is whether you'll need the flexibility. Use your judgement on your own projects. For me its the difference between prototyping and production.

---

### **What we've set up**:

- A **`UIEventManager`** broadcasting changes to health, mana, coins, XP, lives and game over.
- **`GameManager`** and **`PlayerStats`** announcing changes without knowing a UI exists.
- A reusable **`UIBar`** that slides a slider towards a value.
- A **`HUDView`** that displays things and a **`HUDPresenter`** that decides what to display.
- A HUD that updates itself, with no polling every frame.

### **Next**:

In **Part 2** we look at the **Event Bus** - what to do when several unrelated systems all want to know about the same thing, and declaring a named event for each one starts getting tedious.

**Event Bus** is advanced fancy stuff! I don't expect anyone to use this in your projects this semester but I want to tutorialise it here and hook it into the visual effects work.