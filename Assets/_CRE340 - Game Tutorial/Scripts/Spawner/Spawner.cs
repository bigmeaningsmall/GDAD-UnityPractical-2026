using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A general purpose spawner.
// The same script handles items, enemies, or anything else - we just configure it differently.
public class Spawner : MonoBehaviour
{
    [Header("What to spawn")]
    // ARRAY - the set of prefabs this spawner is allowed to use.
    // We decide this in the editor and it doesn't change while the game runs.
    public GameObject[] prefabsToSpawn;

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

    // LIST - everything this spawner has created and that is still alive.
    // This changes constantly while the game runs, so it has to be a List.
    [SerializeField]
    private List<GameObject> spawnedObjects = new List<GameObject>();

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

            // Tidy the list before we check how many are alive
            CleanUpList();

            if (spawnedObjects.Count < maxAlive)
            {
                SpawnOne();
            }
        }
    }

    // Spawn a single random prefab from the array
    private void SpawnOne()
    {
        // Nothing assigned in the Inspector, so there's nothing we can do
        if (prefabsToSpawn.Length == 0)
        {
            Debug.LogWarning("Spawner on " + gameObject.name + " has no prefabs assigned!");
            return;
        }

        // Pick a random slot from the array.
        // Random.Range with ints is EXCLUSIVE of the upper number, so Length is correct here.
        int randomIndex = Random.Range(0, prefabsToSpawn.Length);
        GameObject prefabToSpawn = prefabsToSpawn[randomIndex];

        // Pick a random position inside the spawn area
        Vector3 randomPosition = new Vector3(
            Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
            spawnHeight,
            Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
        );

        // Offset by this spawner's own position so the area follows the object
        randomPosition = randomPosition + transform.position;

        // Create it and remember it
        GameObject newObject = Instantiate(prefabToSpawn, randomPosition, Quaternion.identity);
        spawnedObjects.Add(newObject);
    }

    // Remove anything from the list that has been destroyed
    private void CleanUpList()
    {
        // Loop BACKWARDS when removing from a list - see the note below for why
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
            }
        }
    }

    // Handy for debugging - how many of our objects are still alive
    public int GetAliveCount()
    {
        CleanUpList();
        return spawnedObjects.Count;
    }

    // Draw the spawn area in the editor so we can see it
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, spawnArea);
    }
}