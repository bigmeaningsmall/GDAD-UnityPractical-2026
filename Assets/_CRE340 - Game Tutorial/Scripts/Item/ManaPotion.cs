using UnityEngine;

// Derived class ManaPotion - it IS an Item, plus it restores mana 
public class ManaPotion : Item // 
{
    [Header("Mana Potion")]
    public int manaRestoreAmount;       // Amount of mana this potion restores
    public int minRestoreAmount = 20;   // Minimum restore amount for the random range
    public int maxRestoreAmount = 50;   // Maximum restore amount for the random range

    protected override void Awake()
    {
        base.Awake();   // always call the base version first

        // Same constructor, different identity
        data = new ItemData("Mana Potion", "A potion that restores mana.");

        manaRestoreAmount = Random.Range(minRestoreAmount, maxRestoreAmount);

        Debug.Log("ManaPotion: random restore amount set to " + manaRestoreAmount);
    }

    // Same method name, same signature, completely different behaviour
    public override void Use(PlayerStats player)
    {
        Debug.Log("Mana Potion used - restoring " + manaRestoreAmount + " mana.");
        player.RestoreMana(manaRestoreAmount);
    }
}