using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawns enemies at intervals using the factory.
// Same shape as Spawner, but it works from EnemyData assets rather than prefabs.
public class EnemySpawner : MonoBehaviour
{
    [Header("What to spawn")]
    public EnemyData[] enemyTypes;      // data assets, not prefabs

    [Header("Where to spawn")]
    public Vector3 spawnArea = new Vector3(20f, 0f, 20f);  // width, height, depth
    public float spawnHeight = 0.5f;                       // how high off the floor

    [Header("Spawn at the start")]
    public bool spawnOnStart = true;
    public int startingAmount = 5;

    [Header("Keep spawning")]
    public bool spawnContinuously = false;
    public float minSpawnInterval = 2f;
    public float maxSpawnInterval = 5f;
    public int maxAlive = 20;    // stop spawning once we hit this many

    // LIST - every enemy this spawner has created and that is still alive
    [SerializeField]
    private List<Enemy> spawnedEnemies = new List<Enemy>();

    void Start()
    {
        // Spawn a batch straight away
        if (spawnOnStart)
        {
            for (int i = 0; i < startingAmount; i++)
            {
                SpawnOne();
            }
        }

        // Start the loop that keeps spawning over time
        if (spawnContinuously)
        {
            StartCoroutine(SpawnLoop());
        }
    }

    // Coroutine that spawns forever at random intervals
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Wait a random amount of time before the next spawn
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // Stop spawning once the game is over
            if (GameManager.Instance.IsGameOver)
            {
                yield break;   // ends this coroutine completely
            }

            // Tidy the list before we check how many are alive
            CleanUpList();

            if (spawnedEnemies.Count < maxAlive)
            {
                SpawnOne();
            }
        }
    }

    // Spawn a single random enemy type from the array
    private void SpawnOne()
    {
        // Nothing assigned in the Inspector, so there's nothing we can do
        if (enemyTypes.Length == 0)
        {
            Debug.LogWarning("EnemySpawner on " + gameObject.name + " has no enemy types assigned!");
            return;
        }

        // Pick a random slot from the array
        int randomIndex = Random.Range(0, enemyTypes.Length);
        EnemyData chosenType = enemyTypes[randomIndex];

        // Pick a random position inside the spawn area
        Vector3 randomPosition = new Vector3(
            Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
            spawnHeight,
            Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
        );

        // Offset by this spawner's own position so the area follows the object
        randomPosition = randomPosition + transform.position;

        // Ask the factory for an enemy. We don't know or care how it makes one.
        Enemy newEnemy = EnemyFactory.CreateEnemy(chosenType, randomPosition);

        if (newEnemy != null)
        {
            spawnedEnemies.Add(newEnemy);
        }
    }

    // Remove anything from the list that has been destroyed
    private void CleanUpList()
    {
        // Loop BACKWARDS when removing from a list
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    // Handy for debugging - how many of our enemies are still alive
    public int GetAliveCount()
    {
        CleanUpList();
        return spawnedEnemies.Count;
    }

    // Draw the spawn area in the editor so we can see it
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, spawnArea);
    }
}