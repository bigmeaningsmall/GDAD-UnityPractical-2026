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
        // string message = name + " was destroyed!";
        string message = "<color=red>" + name + " was destroyed!</color>";

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