### Step-by-Step Guide to Implementing the Patrol State in Enemy Behaviour

The goal is to add a **Patrol State** to create a more dynamic enemy that moves around even when the player isn't within range.

We'll add the `EnemyState_Patrol` class and modify the existing `Idle` and `Chase` states to allow smooth transitions between these behaviours.

### 1. **Folder Setup and Class Organisation**

Ensure you have the following structure under **Scripts/Enemy**:
- `IEnemyState`: An interface defining core state methods.
- `EnemyState_Idle`: Idle behaviour, where the enemy waits for the player.
- `EnemyState_Chase`: Chase behaviour, where the enemy follows the player.
- `EnemyState_Patrol`: **(New)** Patrol behaviour, where the enemy wanders within a set area.

### 2. **Creating the Patrol State Class (`EnemyState_Patrol`)**

This state allows the enemy to roam within a certain radius. Here's how it works:

``` csharp
using UnityEngine;

public class EnemyState_Patrol : IEnemyState
{
    private Vector3 patrolCenter;
    private Vector3 patrolTarget;
    private float patrolRange = 5f;  // Range within which the enemy will patrol
    private float patrolSpeed = 1.5f; // Speed while patrolling
    private float targetReachedThreshold = 0.2f; // How close the enemy needs to get to a point before switching to the next target
    private float idleProbability = 0.001f; // Probability of switching back to Idle

    // Called when transitioning into Patrol State
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Patrol State");
        patrolCenter = enemy.transform.position; // Set the patrol's center to the enemy's current position
        SetNewPatrolTarget(enemy); // Define a target location within the patrol range
    }

    // Called each frame to update patrol behaviour
    public void Update(Enemy enemy)
    {
        // Transition to Chase if the player is within chase range
        if (enemy.target != null && Vector3.Distance(enemy.transform.position, enemy.target.position) < enemy.chaseRange)
        {
            enemy.SetState(new EnemyState_Chase());
            return;
        }

        // Move towards the patrol target
        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, patrolTarget, patrolSpeed * Time.deltaTime);

        // Check if the target is reached and set a new patrol target if necessary
        if (Vector3.Distance(enemy.transform.position, patrolTarget) < targetReachedThreshold)
        {
            SetNewPatrolTarget(enemy);
        }

        // Random chance to switch to Idle for variety in behaviour
        if (Random.value < idleProbability)
        {
            enemy.SetState(new EnemyState_Idle());
        }
    }

    // Called when exiting Patrol State
    public void Exit(Enemy enemy)
    {
        Debug.Log("Exiting Patrol State");
    }

    // Sets a random patrol target within the patrol range
    private void SetNewPatrolTarget(Enemy enemy)
    {
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        patrolTarget = patrolCenter + new Vector3(randomX, 0, randomZ);
    }
}
```

**Explanation:**
- `Enter`: Sets up the initial patrol position and target.
- `Update`: Controls movement towards the patrol target, checks proximity to the player, and occasionally transitions back to Idle.
- `Exit`: Handles any cleanup (currently used for logging).
- `SetNewPatrolTarget`: Picks a random target point within the patrol range around the initial patrol center.

### 3. **Updating the Idle State (`EnemyState_Idle`)**

The Idle state now includes a chance to transition into the Patrol state to create variety in the enemy's actions.

``` csharp
public class EnemyState_Idle : IEnemyState
{
    private float patrolProbability = 0.001f; // Probability to start patrolling

    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Idle State");
    }

    public void Update(Enemy enemy)
    {
        // Transition to Chase if the player is within range
        if (enemy.target != null && Vector3.Distance(enemy.transform.position, enemy.target.position) < enemy.chaseRange)
        {
            enemy.SetState(new EnemyState_Chase());
            return;
        }

        // Random chance to enter Patrol State
        if (Random.value < patrolProbability)
        {
            enemy.SetState(new EnemyState_Patrol());
        }
    }

    public void Exit(Enemy enemy)
    {
        Debug.Log("Exiting Idle State");
    }
}
```

### 4. **Updating the Chase State (`EnemyState_Chase`)**

In the Chase state, if the player moves out of range, the enemy should return to Idle or Patrol. Here we leave the option to directly transition to Idle for simplicity.

``` csharp
public class EnemyState_Chase : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Chase State");
    }

    public void Update(Enemy enemy)
    {
        // Move towards the player
        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, enemy.target.position, enemy.speed * Time.deltaTime);

        // Return to Idle if the player is out of chase range
        if (Vector3.Distance(enemy.transform.position, enemy.target.position) > enemy.chaseRange)
        {
            enemy.SetState(new EnemyState_Idle());
        }
    }

    public void Exit(Enemy enemy)
    {
        Debug.Log("Exiting Chase State");
    }
}
```

### 5. **Testing and Debugging the Patrol State**

1.  **Test the Patrol Movement**: Ensure the enemy roams within a set area while in the Patrol state. Adjust the `patrolRange` and `patrolSpeed` variables for optimal movement and responsiveness.

2.  **Verify Transitions**:

    - Transition from `Idle` to `Patrol` after a random interval.
    - Transition to `Chase` when the player enters the chase range.
    - Transition back to `Idle` if the player leaves the chase range while the enemy is in `Chase`.

3.  **Adjust the `patrolProbability` and `idleProbability`**: These parameters control how often the enemy switches behaviours. Experiment to find settings that create a natural, dynamic feel.

4.  **Debugging Tips**:

    - Use `Debug.Log()` statements in each `Enter`, `Update`, and `Exit` method to trace transitions.
    - Experiment with patrol speeds, patrol range, and target reached threshold to optimise for your game's needs.

------------------------------------------------------------------------
