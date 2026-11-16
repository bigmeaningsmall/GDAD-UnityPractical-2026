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

// The player's health changed. tookDamage lets effects tell a hit from a heal.
public struct PlayerHealthChanged
{
    public int currentHealth;
    public int maxHealth;
    public bool tookDamage;
}

// An enemy started hunting the player
public struct EnemyChaseStarted
{
    public Transform enemy;
}

// An enemy gave up
public struct EnemyChaseEnded
{
    public Transform enemy;
}