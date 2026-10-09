---
Order: 4-B
---
# **Week 4 - Part 2 : The Event Logger**

### Overview

#### **What we are building**:
- We'll extend the `EventListener` so that instead of only writing to the Console, it displays a running **log in the game UI**.
- The log keeps the last few messages and drops the oldest ones as new events come in.
- We'll use a **`List<string>`** to hold the lines, which is a nice entry to the data structures we look at in Part 3 and an example of what lists are good for.
#### **Why we are doing this**:
- It's a small, useful system that proves the event architecture work. Nothing fires the log directly - it just listens.
- A debug log overlay like this can be handy during development or you can use it in a project. For example, multiplayer games might use this to show all the events to each player.
- It also gives us a reason to use a List for something other than spawning, so the data structure isn't just one idea but we use Lists for different things.
#### **Learning Objectives**:
- **TextMeshProUGUI** - updating UI text from code. This will be the start of our Observer pattern UI.
- **`List<string>`** - adding, removing and counting items in a collection.
- **`string.Join`** - turning a collection into a single display string. 
- **Listeners doing their job** - the same event now drives two different outputs.

---
---

# **Part 2: Logging Events to the UI**

Carry on in the same scene. The scene already has a UI Canvas with a `Text - Log` object set up as a child, ready to use.

---

### **Step 1: Find the Log Text in the Scene**

1. In the **Hierarchy**, open up the **UI** GameObject.
2. You should find a child called **Text - Log** with a `TextMeshProUGUI` component on it.
3. Clear out any placeholder text so it starts empty.

If it isn't there, add one - `UI -> Text - TextMeshPro` inside the Canvas - and anchor it somewhere out of the way, usually bottom-left or bottom-right. Make the font fairly small, we'll be putting ten lines in it.

---

### **Step 2: Rename the Class (a small refactor)**

Before we add anything, we're going to fix a name. I called this class `EventListener` back in Week 3 and it was too generic. This is my fault, and it's better to correct now so it has a clear meaningful name.

**Why it's a poor name**: "Listener" describes _how the class is wired up_, not _what it does_. In an event-driven project almost everything is a listener, so the word doesn't tell you anything useful. By Week 6 we'll have one listening for health changes, one for the score, one for the inventory - if they're all called `SomethingListener` then the names stop helping you find anything.

What this class actually does is **log events to the screen**. So:

> `EventListener` → **`EventLogger`**

**A rule of thumb for naming a class**: _name it for the job it does, not for how it's connected to everything else._ Subscribing to an event is plumbing. Showing a log is the job.

This also ties back to the **S** in SOLID from last week's lecture slides - **Single Responsibility**. A class should do one job, and the name is where you say what that job is. If you can't name a class without using "and", it might be doing too much.

#### How to rename it safely in Unity - `Refactoring`

Renaming a MonoBehaviour is one of those things Unity makes annoying, so to do it properly.

**The easy way - let your IDE do it:**
- **Visual Studio**: click the class name and press `F2`
- **Rider**: right-click the class name → `Refactor` → `Rename`, or `Ctrl+R, Ctrl+R`

Type the new name and it renames the class, the file, and every reference to it across the project in one go. This is what the **Rename refactor** is for and it's much safer than doing it by hand. Its the process of updating something across the whole codebase. 

**The manual way:**
1. In Unity's **Project window**, rename `EventListener.cs` to `EventLogger.cs`.
2. Open it and change `public class EventListener` to `public class EventLogger`.

Both have to match or the script won't load I think - match the names always anyway.

**Then tidy the scene:**
- Select the **EventListener** GameObject in the Hierarchy and rename it to **Event Logger** as well. The object and the component should say the same thing in this example.

> [!warning] **Note - rename inside Unity, never in Explorer or Finder.**
> 
> Every script in your project has a hidden `.meta` file sitting beside it with a unique **GUID** in it. That GUID is how Unity knows which script is attached to which GameObject - not the file name.
> 
> Rename the file **inside Unity** and the `.meta` is renamed with it, so the component on your GameObject stays connected and nothing breaks.
> 
> Rename it in **Explorer or Finder** and the `.meta` gets left behind. Unity generates a fresh one with a new GUID, the old link is lost, and your GameObject shows **"Missing (Mono Script)"** with all its Inspector values gone. _Same goes for moving scripts around - do it in the Project window._

> [!note] **Note - what 'refactoring' actually means.**
> 
> **Refactoring is changing the structure of code without changing what it does.** Press Play before and after this rename and the game behaves identically. Nothing was fixed or added.
> 
> That sounds pointless until you come back to a project after a few months. Names are how you navigate a codebase you've forgotten, and a vague name is a problem when you go looking for something.
> 
> The other reason to do it the moment you notice: _bad names get copied_. If I'd left this one, the Week 6 UI classes would have been named to match it and we'd have multiple vague names.

---

### **Step 3: Extend the `EventLogger`**

We're adding three things - a reference to the text, a List to hold the lines, and a method to update the display.

Replace the contents of `EventLogger.cs` with the following:

```csharp
using System.Collections.Generic;  // needed for List<>
using UnityEngine;
using TMPro;                       // needed for TextMeshProUGUI

public class EventLogger : MonoBehaviour
{
    [Header("UI Log")]
    public TextMeshProUGUI logText;  // drag the 'Text - Log' object onto this in the Inspector
    public int maxLines = 10;        // how many messages to keep on screen

    // A LIST of strings holding the messages currently on screen.
    // A List can grow and shrink while the game runs, which is exactly what we need here.
    private List<string> logLines = new List<string>();

    private void OnEnable()
    {
        // Subscribe to events
        HealthEventManager.OnObjectDamaged += HandleObjectDamaged;
        HealthEventManager.OnObjectDestroyed += HandleObjectDestroyed;
    }

    private void OnDisable()
    {
        // Unsubscribe from events to avoid memory leaks
        HealthEventManager.OnObjectDamaged -= HandleObjectDamaged;
        HealthEventManager.OnObjectDestroyed -= HandleObjectDestroyed;
    }

    private void HandleObjectDamaged(string name, int remainingHealth)
    {
        string message = name + " was damaged - health now " + remainingHealth;

        Debug.Log(message);     // still goes to the Console
        AddToLog(message);      // and now also to the UI
    }

    private void HandleObjectDestroyed(string name, int remainingHealth)
    {
        string message = name + " was destroyed!";

        Debug.Log(message);
        AddToLog(message);
    }

    // Add a message to the log, keeping only the most recent lines
    private void AddToLog(string message)
    {
        if (logText == null)
        {
            return; // nothing assigned in the Inspector, so there's nothing to update
        }

        // Add the new message to the end of the list
        logLines.Add(message);

        // If we now have too many lines, remove the OLDEST one (index 0 is the first item)
        if (logLines.Count > maxLines)
        {
            logLines.RemoveAt(0);
        }

        // Join every line in the list into one string, separated by line breaks
        logText.text = string.Join("\n", logLines);
    }
}
```

**Explanation**:
- **`List<string> logLines`** holds the messages. A List is a collection that can grow and shrink while the game is running, so we can keep adding and removing without worrying about size.
- **`logLines.Add(message)`** puts the new message on the end.
- **`logLines.RemoveAt(0)`** removes the first item, which is the oldest message. Everything else shuffles down a position.
- **`logLines.Count`** tells us how many items are currently in the list.
- **`string.Join("\n", logLines)`** takes the whole list and makes one string out of it, putting a line break between each item. `\n` is the escape character for a new line.

> [!note] **Note - the `if (logText == null) return;` at the top.**
> 
> This is called an **early return** or a guard clause. If there's no text assigned we leave the method immediately rather than wrapping the whole thing in an `if` block.
> 
> It keeps the main body of the method un-indented and saves a `NullReferenceException` if you forget to drag the text in. Handy habit for any method that depends on a reference being set.

---

### **Step 4: Assign the Text in the Inspector**

1. **Select the Event Logger GameObject** in the Hierarchy (the one you just renamed).
2. Drag the **Text - Log** object from the Hierarchy onto the **Log Text** field.
3. Set **Max Lines** to 10 to start with.

> [!note] **Note - if the component is showing as missing, the rename went wrong.**
> 
> Re-add it with `Add Component -> Event Logger` and you're fine - there was nothing set on it yet. 

---

### **Step 5: Test It**

1. **Press Play** and shoot some crates.
2. The log should fill up in the corner of the screen as things take damage.
3. Once you have more than ten messages, the oldest one drops off the top as each new one arrives.
4. Try changing **Max Lines** to 3 or 20 while the game is running and keep shooting - you'll see the log resize itself as new messages come in.

---

### **Step 6: Prove the Decoupling (optional  test of the concept)**

This step isn't really about the log, it's about what the log demonstrates.

1. **Duplicate the Event Logger GameObject.**
2. On the copy, **leave the Log Text field empty**.
3. Press Play and shoot something.

You now have two listeners - one writing to the UI and the Console, one writing to the Console only. Both respond to the same event.

Now think about what we **didn't** touch to for that to happen. We didn't open `Crate.cs`, `Enemy.cs`, `Bullet.cs` or `HealthEventManager.cs`. The crate has no idea there are two listeners, or that one of them has a UI attached.

> [!note] **Note - this is the Observer pattern doing a real job.**
> 
> We are naming the pattern in Week 3 and we'll build it properly in Week 6, but this is what it does. Adding a whole new output - a UI overlay - cost nothing in the classes that generate the events.
> 
> Think about how you'd have done this without events. The crate would need a reference to the UI. So would the enemy, and the exploding crate. Change the UI and you'd be editing all three. 

---

### **Step 7: Optional - Colour the Messages**

TextMeshPro understands rich text tags, so you can colour individual lines. It's a nice touch and it makes the log much easier to read at a glance.

In `HandleObjectDestroyed`, change the message to:

```csharp
        string message = "<color=red>" + name + " was destroyed!</color>";
```

Destroyed messages now show in red while damage messages stay white. You can use any colour name or a hex value like `<color=#FF8800>`.

---

### **What we've done**:

- **Renamed `EventListener` to `EventLogger`** - a refactor that changed nothing about how the game runs, and made the code easier to read and work with.
- Extended it to display a running log in the UI.
- Used a **`List<string>`** to hold a collection that changes while the game runs.
- Added a second output to the event system without touching anything that fires events.

### **Where this is going**:

The List is the first of the data structures we're looking at this week. In **Part 3** we'll put Lists next to **Arrays**, see where each one fits, and use both to rebuild our spawning so the game has enemies arriving continuously rather than being placed.