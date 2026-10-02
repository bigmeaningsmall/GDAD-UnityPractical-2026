// IDamagable.cs
// An interface is a CONTRACT. Any class that implements it MUST provide these members.
// There is no code here - it is a promise that the code exists somewhere.

public interface IDamagable
{
    // An interface CANNOT hold a variable - 'public int health = 10;' is not allowed here.
    // But it CAN demand a PROPERTY. This says: you must provide a way to READ your health.
    int Health { get; }

    void TakeDamage(int damage);   // reduce health / apply damage
    void ShowHitEffect();          // show some visual feedback for the hit
}