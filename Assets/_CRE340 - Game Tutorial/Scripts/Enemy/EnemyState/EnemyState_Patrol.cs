using UnityEngine;

// Enemy walks to a random point nearby, then stands still again.
public class EnemyState_Patrol : IEnemyState
{
    public string Name { get { return "Patrol"; } }

    private float patrolRange = 5f;       // how far it will wander from where it starts
    private float patrolSpeed = 1.5f;     // slower than the chase
    private float arriveDistance = 0.2f;  // close enough to count as arrived

    private Vector3 patrolTarget;

    public void Enter(Enemy enemy)
    {
        // Pick a point near wherever the enemy happens to be standing
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        patrolTarget = enemy.transform.position + new Vector3(randomX, 0f, randomZ);
    }

    public void Update(Enemy enemy)
    {
        // Chase still takes priority
        if (enemy.IsPlayerInRange())
        {
            enemy.SetState(enemy.ChaseState);
            return;
        }

        enemy.transform.position = Vector3.MoveTowards(
            enemy.transform.position,
            patrolTarget,
            patrolSpeed * Time.deltaTime
        );

        // Arrived, so stand about for a bit before picking another point
        if (Vector3.Distance(enemy.transform.position, patrolTarget) < arriveDistance)
        {
            enemy.SetState(enemy.IdleState);
        }
    }

    public void Exit(Enemy enemy)
    {
    }
}