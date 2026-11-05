using UnityEngine;

// Enemy stands still for a few seconds, then wanders off.
public class EnemyState_Idle : IEnemyState
{
    public string Name { get { return "Idle"; } }

    private float idleDuration = 2f;    // how long to wait before patrolling
    private float timeIdle;

    public void Enter(Enemy enemy)
    {
        timeIdle = 0f;
    }

    public void Update(Enemy enemy)
    {
        // Chase takes priority over anything else
        if (enemy.IsPlayerInRange())
        {
            enemy.SetState(enemy.ChaseState);
            return;
        }

        timeIdle = timeIdle + Time.deltaTime;

        if (timeIdle >= idleDuration)
        {
            enemy.SetState(enemy.PatrolState);
        }
    }

    public void Exit(Enemy enemy)
    {
    }
}