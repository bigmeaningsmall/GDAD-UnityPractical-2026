using System;

// A generic event bus.
// The <T> means this class works with ANY type - we decide what T is when we use it.
//
// EventBus<EnemyKilled> and EventBus<CoinCollected> are two completely separate
// classes as far as C# is concerned, each with its own event. One bus per message type.

public static class EventBus<T>
{
    // Action<T> is a built-in delegate - a method that takes one T and returns void.
    // Saves us declaring our own delegate every time.
    private static Action<T> OnEvent;

    public static void Subscribe(Action<T> handler)
    {
        OnEvent += handler;
    }

    public static void Unsubscribe(Action<T> handler)
    {
        OnEvent -= handler;
    }

    public static void Publish(T message)
    {
        if (OnEvent != null)
        {
            OnEvent(message);
        }
    }
}




