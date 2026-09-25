// NOTE - we dont need 'using UnityEngine;' needed and NO MonoBehaviour.
// This is a plain C# class. It holds data. It never sits on a GameObject by itself.

// [System.Serializable] lets Unity display this class in the Inspector
// (and later, in Week 9, lets us save it to a file)
// This is very handy as it means we can use the regular classes but make it easy to work with in the editor

[System.Serializable] // Take note of this!!! It means the whole class can be exposed in the editor and serialised in a save system later - It'll make make sense later or comment it to see the difference in the editor
public class PlayerData
{
    public int currentHealth;
    public int maxHealth;
    public int currentMana;
    public int maxMana;

    // DEFAULT CONSTRUCTOR - runs when we write 'new PlayerData()'
    // A brand new player, with default starting values
    public PlayerData()
    {
        currentHealth = 50;
        maxHealth = 100;
        currentMana = 20;
        maxMana = 100;
    }

    // PARAMETERISED CONSTRUCTOR - runs when we write 'new PlayerData(80, 40)'
    // Lets us create a player set up with specific values
    public PlayerData(int startingHealth, int startingMana)
    {
        currentHealth = startingHealth;
        maxHealth = 100;
        currentMana = startingMana;
        maxMana = 100;
    }
}