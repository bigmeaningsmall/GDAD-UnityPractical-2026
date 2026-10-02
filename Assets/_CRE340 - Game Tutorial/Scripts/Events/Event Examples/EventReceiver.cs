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
        // Unsubscribe to avoid memory leaks - same discipline as Part 3
        EventSender.OnFire -= HandleFireEvent;
    }

    private void Start()
    {
        originalScale = transform.localScale;
    }

    // Matches the delegate shape exactly: void return, two floats - THIS IS WHAT THE EVENT CONNECTS TOO!
    private void HandleFireEvent(float scale, float speed)
    {
        // Use the values we received to drive the animation
        StartCoroutine(ScaleObject(scale, speed));
    }

//------------------ This below is just animation stuff - THE ABOVE IS THE EVENT - BELOW IS WHAT HAPPENS AFTER
    // Coroutine to scale the object up and back down using Lerp
    private IEnumerator ScaleObject(float targetScale, float speed)
    {
        Vector3 targetSize = originalScale * targetScale;
        float elapsedTime = 0f;
        float duration = 1f / speed; // higher speed = shorter duration

        // Scale up
        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetSize, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // wait one frame
        }

        transform.localScale = targetSize; // make sure we land exactly on target

        elapsedTime = 0f;

        // Scale back down
        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(targetSize, originalScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale; // reset exactly
    }
}