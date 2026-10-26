using UnityEngine;

// Messages that get sent around the game.

public struct EnemyKilled
{
    public string enemyName;
    public int xpValue;
    public Vector3 position;
}

public struct WeaponFired
{
    public Vector3 position;
    public Vector3 direction;
}