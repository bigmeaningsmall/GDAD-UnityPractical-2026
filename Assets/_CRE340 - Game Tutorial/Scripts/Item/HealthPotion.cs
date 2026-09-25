using UnityEngine;

// Derived class HealthPotion - it IS an Item, plus it restores health
public class HealthPotion : Item
{
    [Header("Health Potion")]
    public int healthRestoreAmount;      // Amount of health this potion restores
    public int minRestoreAmount = 30;    // Minimum restore amount for the random range
    public int maxRestoreAmount = 70;    // Maximum restore amount for the random range

    // 'override' our base class Awake to set up this specific item type
    protected override void Awake()
    {
        base.Awake();   // ALWAYS call the base version first, so shared setup still runs - its a bit of boilerplate overhead to keep things in order

        // Create our identity using ItemData's parameterised constructor
        data = new ItemData("Health Potion", "A potion that restores health.");

        // Roll a random restore amount within the range set in the Inspector
        healthRestoreAmount = Random.Range(minRestoreAmount, maxRestoreAmount);

        Debug.Log("HealthPotion: random restore amount set to " + healthRestoreAmount);
    }

    // 'override' replaces the base class version of Use() with this one
    public override void Use(PlayerStats player)
    {
        Debug.Log("Health Potion used - restoring " + healthRestoreAmount + " health.");
        player.RestoreHealth(healthRestoreAmount);
    }
}