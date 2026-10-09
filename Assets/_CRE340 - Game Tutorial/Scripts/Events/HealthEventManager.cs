// HealthEventManager.cs
// A static class - we never create an instance of it, we just use it directly.
// It holds the events. It does not react to them.

public static class HealthEventManager
{
    // A DELEGATE defines the SHAPE of a method:
    // "returns void, takes a string and an int" - any method matching that shape can subscribe
    public delegate void HealthEvent(string name, int currentHealth);

    // Called when any object implementing IDamagable takes damage
    public static HealthEvent OnObjectDamaged;

    // Called when any object implementing IDamagable is destroyed
    public static HealthEvent OnObjectDestroyed;
}