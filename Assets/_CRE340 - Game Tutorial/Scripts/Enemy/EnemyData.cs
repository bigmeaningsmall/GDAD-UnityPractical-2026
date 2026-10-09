using UnityEngine;

// [CreateAssetMenu] adds this to Unity's Create menu so we can make assets from it.
// fileName is the default name for a new one, menuName is where it appears in the menu.
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "CRE340 - Game Tutorial/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Bad Guy";
    public Color enemyColor = Color.red;

    [Header("Stats")]
    public int startingHealth = 10;   // health this enemy type STARTS with
    public float moveSpeed = 2f;      // how fast it chases the player
    public int contactDamage = 5;     // damage it deals by touching the player

    [Header("Rewards")]
    public int scoreValue = 10;       // TODO (Week 5) - XP awarded when this enemy dies
}