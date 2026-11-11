using UnityEngine;

public class Coin : Item
{
    [Header("Coin")]
    public int coinValue = 1;

    protected override void Awake()
    {
        base.Awake();   // always call the base version first

        data = new ItemData("Coin", "Worth something to somebody.");
    }

    // The player parameter is here because the base class asks for it.
    // This item doesn't need it - coins go to the GameManager, not the player.
    public override void Use(PlayerStats player)
    {
        Debug.Log("Picked up a coin worth " + coinValue);
        
        //todo coin soundzzz
        AudioEvent.PlaySFX(SoundId.SFX_collect_bonus1);

        GameManager.Instance.AddCoins(coinValue);
    }
}
