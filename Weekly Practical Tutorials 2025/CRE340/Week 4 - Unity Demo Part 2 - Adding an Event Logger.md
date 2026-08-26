### Unity Demo Guide: Adding Logging to the `EventListener` Using TextMeshPro

In this guide, we’ll extend the `EventListener` functionality by adding logging to display damage and destruction events in the UI using a `TextMeshProUGUI` component. The GameObject `Text - Log`, which is a child of the UI GameObject, has already been set up for logging.

#### Step 1: Modify the `EventListener` Class

We will update the `EventListener` class to display event logs in the `TextMeshProUGUI` component. Here's how the code changes.

**Original Code:**

```c#
private void HandleObjectDamaged(int remainingHealth)
{
    string message = $"An object called {name} was damaged! Remaining Health: {remainingHealth}";
    Debug.Log(message);
}

private void HandleObjectDestroyed(int remainingHealth)
{
    string message = $"An object called {name} was destroyed!";
    Debug.Log(message);
}
```

**Modified Code:**
- We are adding a TextMeshPro for the UI text - This is already setup in the scene
- We are adding a function to to display the events as a log that is updated in the UI

```c#
using UnityEngine;  
using System.Linq;  
using TMPro;  
  
public class EventListener : MonoBehaviour  
{  
    public TextMeshProUGUI logText; // Reference to the TextMeshProUGUI component  
    public int lineCount = 10; // Number of lines to display in the log  
    
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
        string message = $"An object called {name} was damaged! Remaining Health: {remainingHealth}";  
        Debug.Log(message);  
        UpdateLog(message, lineCount);  
    }  
    
    private void HandleObjectDestroyed(string name, int remainingHealth)  
    {  
        string message = $"An object called {name} was destroyed!";  
        Debug.Log(message);  
        UpdateLog(message, lineCount);  
    }  
    
    // Function to update the log with a limit on the number of lines  
    private void UpdateLog(string message, int maxLines)  
    {  
        if (logText != null)  
        {  
            // Split the current log text into lines  
            var lines = logText.text.Split('\n').ToList();  
  
            // Add the new message  
            lines.Add(message);  
  
            // If the number of lines exceeds the limit, remove the oldest  
            if (lines.Count > maxLines)  
            {  
                lines.RemoveAt(0);  
            }  
  
            // Join the lines back into a single string  
            logText.text = string.Join("\n", lines);  
        }  
    }  
}
```

**Key Changes:**
- We’ve added a public `TextMeshProUGUI` variable (`logText`) to display messages.
- The `UpdateLog` method appends messages to the UI text and limits the number of lines displayed to `lineCount`.

#### Step 2: Assign the Text Object in Unity Editor

1. **Select the EventListener GameObject:**
   - In the Unity Editor, find and select the GameObject with the `EventListener` component attached (likely an empty GameObject or UI element).

2. **Drag and Drop the Log Text:**
   - In the **Inspector** window, you will see a variable called `Log Text` under the `EventListener` component. Drag the `Text - Log` GameObject (a child of your UI GameObject) from the **Hierarchy** into this field to assign it as the target for the log messages.

3. **Verify Settings:**
   - Ensure that the `Text - Log` GameObject has a `TextMeshProUGUI` component. If it's not present, you can add it via the **Add Component** button in the Inspector.

#### Step 3: Test in Play Mode

1. **Enter Play Mode:**
   - Hit the Play button in the Unity Editor. 
   - Once the game starts, any damage or destruction events will now be logged in the UI via the `Text - Log` GameObject.

2. **Observe the Log:**
   - When any object is damaged, a message should appear in the log, e.g., “An object called Crate was damaged! Remaining Health: 5.”
   - When an object is destroyed, a corresponding message will appear, e.g., “An object called Crate was destroyed!”

#### Step 4: Adjust Log Display (Optional)

- You can adjust the `lineCount` in the `EventListener` component to control how many lines of text appear in the log. For example, setting it to 5 will display only the 5 most recent events.

---

By following this guide, you have now integrated a UI-based logging system for events in your Unity demo, displaying the messages in real-time using the `TextMeshProUGUI` component.

#### Testing
- When you run the game you should see the shooting events as an event log in the UI