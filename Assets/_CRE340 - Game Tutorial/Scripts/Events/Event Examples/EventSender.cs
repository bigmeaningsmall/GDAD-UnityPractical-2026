using UnityEngine;

public class EventSender : MonoBehaviour
{
    // Define the delegate type for the OnFire event
    // This one takes TWO floats - the shape the receiver must match
    public delegate void FireEventHandler(float scale, float speed);

    // Define the event based on the delegate
    // 'static' so receivers can subscribe via the class name without a reference to this object
    public static event FireEventHandler OnFire; // i don;t usually write the comments - just have the delegate and event on two lines together

    [Header("Parameters to pass with the event")]
    public float scale = 2.0f;
    public float speed = 10.0f;

    private void Update()
    {
        // Using the old input system here for simplicity - this is a mechanism demo, not game code - might change later
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Only fire if something is actually subscribed
            if (OnFire != null)
            {
                OnFire(scale, speed); // fire the event, passing both values
            }
        }
    }
}