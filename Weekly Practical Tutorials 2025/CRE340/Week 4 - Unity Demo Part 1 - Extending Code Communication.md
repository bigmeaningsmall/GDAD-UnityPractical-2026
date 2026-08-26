### Unity Demo Guide: Updating the Event Handling System

In this guide, we'll update the existing event handling system in our shooting demo. The focus will be on modifying the `HealthEventManager`, `Crate`, `ExplodingCrate`, `Enemy`, and `EventListener` classes. This change will introduce passing the name of the damaged or destroyed object through the event system, allowing more detailed tracking.

#### Step 1: Modify `HealthEventManager`

We’ll begin by modifying the `HealthEventManager` class to include the object’s name in the event parameters.

**Original Code:**

```c#
public delegate void HealthEvent(int currentHealth);

```

**Modified Code:**

```c#
public delegate void HealthEvent(string name, int currentHealth);

```

- The delegate `HealthEvent` now includes a `string` parameter for the object's name, in addition to the `int` representing the current health.
  
This change allows us to pass the name of any object (such as a `Crate`, `ExplodingCrate`, or `Enemy`) when it is damaged or destroyed.

#### Step 2: Modify `Crate` Class

Next, we update the `Crate` class to pass the name of the crate to the event system.

**Original Code:**

```c#
HealthEventManager.OnObjectDamaged?.Invoke(health);
HealthEventManager.OnObjectDestroyed?.Invoke(health);
```

**Modified Code:**

```c#
HealthEventManager.OnObjectDamaged?.Invoke(gameObject.name, health);
HealthEventManager.OnObjectDestroyed?.Invoke(gameObject.name, health);
```

- The events now pass `gameObject.name` to the event system, allowing us to track which object is taking damage or being destroyed.

#### Step 3: Modify `ExplodingCrate` Class

Similarly, we modify the `ExplodingCrate` class to pass the object’s name when an event is triggered.

**Original Code:**

```c#
HealthEventManager.OnObjectDamaged?.Invoke(health);
HealthEventManager.OnObjectDestroyed?.Invoke(health);
```

**Modified Code:**

```c#
HealthEventManager.OnObjectDamaged?.Invoke(gameObject.name, health);
HealthEventManager.OnObjectDestroyed?.Invoke(gameObject.name, health);
```

- The same change applies here to track the name of the exploding crate when it takes damage or is destroyed.

#### Step 4: Modify `Enemy` Class (Without Scriptable Object Reference)

For the `Enemy` class, we’ll update the event handling system similarly, while **excluding** the `ScriptableObject` reference (*to be covered in a future guide*).

**Original Code:**

```c#
HealthEventManager.OnObjectDamaged?.Invoke(health);
HealthEventManager.OnObjectDestroyed?.Invoke(health);
```

**Modified Code:**

```c#
HealthEventManager.OnObjectDamaged?.Invoke(gameObject.name, health);
HealthEventManager.OnObjectDestroyed?.Invoke(gameObject.name, health);
```

This will allow us to track which enemy object is being damaged or destroyed, using the name of the game object.

#### Step 5: Modify `EventListener` Class (Without TextMeshPro Logging)

We will also update the `EventListener` class to handle the modified event parameters, without introducing the logging to TextMeshPro (covered in a future guide).

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

```c#
private void HandleObjectDamaged(string name, int remainingHealth)
{
    string message = $"An object called {name} was damaged! Remaining Health: {remainingHealth}";
    Debug.Log(message);
}

private void HandleObjectDestroyed(string name, int remainingHealth)
{
    string message = $"An object called {name} was destroyed!";
    Debug.Log(message);
}
```

- The `HandleObjectDamaged` and `HandleObjectDestroyed` methods now include the name of the object, allowing us to print more specific log messages.

#### Step 6: Testing in Unity Editor

1. **Assign Event Listeners**: Make sure you have an `EventListener` component attached to an empty GameObject or UI element in your scene to listen to the `HealthEventManager` events.
  
2. **Prefab Setup**: Verify that the prefabs for `Crate`, `ExplodingCrate`, and `Enemy` have their `IDamagable` components properly configured. They should be set up to receive damage and trigger the appropriate events.

3. **Play Mode Testing**: Run the game in Play mode, and watch the console for log messages indicating when objects are damaged or destroyed. You should see messages that include the name of the objects being acted upon.

---

In this guide, we’ve updated the event handling system to include object names in the delegate calls. This provides clearer tracking of damaged and destroyed objects without altering other aspects of the event system. 

#### Testing
- When you run the game you should see the shooting events logged to the console



