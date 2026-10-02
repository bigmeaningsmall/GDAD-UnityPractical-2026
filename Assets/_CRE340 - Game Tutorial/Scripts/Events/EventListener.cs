using UnityEngine;

public class EventListener : MonoBehaviour
{
    private void OnEnable()
    {
        // SUBSCRIBE - hand our methods to the events
        // '+=' means ADD this method to the list of things to call
        HealthEventManager.OnObjectDamaged += HandleObjectDamaged;
        HealthEventManager.OnObjectDestroyed += HandleObjectDestroyed;
    }

    private void OnDisable()
    {
        // UNSUBSCRIBE - always undo what you did in OnEnable
        // '-=' removes our method from the list
        HealthEventManager.OnObjectDamaged -= HandleObjectDamaged;
        HealthEventManager.OnObjectDestroyed -= HandleObjectDestroyed;
    }

    // This method MATCHES the delegate shape: returns void, takes one int
    private void HandleObjectDamaged(int remainingHealth)
    {
        Debug.Log("EVENT LISTENER SAYS: An object was damaged! Remaining Health: " + remainingHealth);
    }

    private void HandleObjectDestroyed(int remainingHealth)
    {
        Debug.Log("EVENT LISTENER SAYS: An object was destroyed!");
    }
}