using UnityEngine;

// Enemy moves straight at the player until the player gets clear.
public class EnemyState_Chase : IEnemyState
{
    public string Name { get { return "Chase"; } }

    public void Enter(Enemy enemy)
    {
    }

    public void Update(Enemy enemy)
    {
        // There might not be a player - not spawned yet, or dead - so check before using it
        if (GameManager.Instance.Player == null)
        {
            enemy.SetState(enemy.IdleState);
            return;
        }

        Transform player = GameManager.Instance.Player.transform;

        // This is the chase that used to be in Enemy.Update
        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, player.position, enemy.enemyData.moveSpeed * Time.deltaTime);

        // Give up once the player is out of range
        if (Vector3.Distance(enemy.transform.position, player.position) > enemy.enemyData.chaseRange)
        {
            enemy.SetState(enemy.IdleState);
        }
    }

    public void Exit(Enemy enemy)
    {
    }
}