### Event Sender and Receiver: Events in Unity

In this example, you'll learn how to set up a basic event-driven communication system in Unity using two separate classes: an **Event Sender** and an **Event Receiver**.

The **Event Sender** will raise an event (`OnFire`) when the space bar is pressed, passing two float parameters (`scale` and `speed`). The **Event Receiver** will subscribe to this event, respond by scaling an object up and down using `Lerp`, and apply the received parameters to control the scale size and speed.

This setup helps illustrate how to use delegates and events for clean, decoupled communication between classes. By the end of this example, you'll understand how to trigger events, pass data, and create flexible interactions between different objects in your Unity scene.

The event (`OnFire`) will pass two `float` parameters: `scale` and `speed`. The receiver will use these values to scale an object up and then back to its original size using `Lerp`.

\***Note - Scale and Speed are used as examples. An event can send any combination of parameters so long as the receiver is set up to accept them.**

*Events are one to many and one single event can be received by any number of classes.*

### Step 1: Create the `EventSender` Class

This class will send the `OnFire` event when the space bar is pressed.

``` csharp
using UnityEngine;  
  
public class EventSender : MonoBehaviour  
{  
    // Define the delegate type for the OnFire event  
    public delegate void FireEventHandler(float scale, float speed);  
    // Define the event based on the delegate  
    public static event FireEventHandler OnFire;  
  
    [Header("Parameters to pass with the event")]  
    public float scale = 2.0f;  
    public float speed = 10.0f;  
    
    private void Update()  
    {        // Check for space bar input  
        if (Input.GetKeyDown(KeyCode.Space))  
        {            // If there are any subscribers, invoke the event and pass parameters  
            if (OnFire != null){  
                OnFire(scale, speed); // Example: scale=2.0, speed=3.0  
            }  
        }    
    }     
}
```

### Step 2: Create the `EventReceiver` Class

This class will listen for the `OnFire` event and respond by scaling an object up and then back down using `Lerp`.

``` csharp
using System.Collections;
using UnityEngine;

public class EventReceiver : MonoBehaviour
{
    private Vector3 originalScale;

    private void OnEnable()
    {
        // Subscribe to the OnFire event
        EventSender.OnFire += HandleFireEvent;
    }

    private void OnDisable()
    {
        // Unsubscribe from the OnFire event to avoid memory leaks
        EventSender.OnFire -= HandleFireEvent;
    }

    private void Start()
    {
        // Store the original scale of the object
        originalScale = transform.localScale;
    }

    // Method that handles the OnFire event
    private void HandleFireEvent(float scale, float speed)
    {
        // Start a coroutine to scale the object up and back down
        StartCoroutine(ScaleObject(scale, speed));
    }

    // Coroutine to scale the object using Lerp
    private IEnumerator ScaleObject(float targetScale, float speed)
    {
        Vector3 targetSize = originalScale * targetScale;
        float elapsedTime = 0f;
        float duration = 1f / speed; // Calculate duration based on speed

        // Scale up
        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetSize, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final scale is exact
        transform.localScale = targetSize;

        // Reset elapsed time for scaling back
        elapsedTime = 0f;

        // Scale back down
        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(targetSize, originalScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the scale is reset to the original size
        transform.localScale = originalScale;
    }
}
```

### Explanation:

1.  **EventSender Class**:
    - Defines a delegate `FireEventHandler` with two `float` parameters: `scale` and `speed`.
    - Declares a static event `OnFire` using this delegate.
    - Listens for the space bar input and triggers the `OnFire` event when the space bar is pressed, passing `scale` and `speed` values as arguments.
2.  **EventReceiver Class**:
    - Subscribes to the `OnFire` event using `EventSender.OnFire += HandleFireEvent`.
    - When the event is received, the `HandleFireEvent` method is called, which starts a coroutine (`ScaleObject`).
    - `ScaleObject` uses `Lerp` to smoothly scale the object up to the target size and then back down to its original size based on the provided `scale` and `speed`.

### Setup in Unity:

1.  **Create Two GameObjects**:
    - Create one GameObject and name it `EventSender`. Attach the `EventSender` script to it.
    - Create another GameObject and name it `EventReceiver`. Attach the `EventReceiver` script to it.
      - Note: You can duplicate the receiver GameObject many times and receive the event in each GameObject
2.  **Test the Interaction**:
    - Press the space bar while in play mode. The `EventSender` will send the event, and the `EventReceiver` will respond by scaling itself up and then back to its original size using the parameters provided (`scale = 2.0f`, `speed = 3.0f`).

### Key Learning Points:

1.  **Event Communication**: This setup shows how to communicate between separate classes using events.
2.  **Parameter Passing**: Demonstrates passing parameters through events to dynamically control behaviors.
3.  **Decoupled Architecture**: The sender and receiver don't have direct references to each other, illustrating a decoupled event-driven approach.
