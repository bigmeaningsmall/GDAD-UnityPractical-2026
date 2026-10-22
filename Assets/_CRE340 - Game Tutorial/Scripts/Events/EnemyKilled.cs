using UnityEngine;

// Messages that get sent around the game.
// These are plain data - no MonoBehaviour, no methods, just the facts about what happened.

public struct EnemyKilled
{
    public string enemyName;
    public int xpValue;
    public Vector3 position;
}