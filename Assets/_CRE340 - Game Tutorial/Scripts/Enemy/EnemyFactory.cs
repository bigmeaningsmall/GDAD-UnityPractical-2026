using UnityEngine;

// One place that knows how to build an enemy.
// Callers hand it a data asset and a position, and get a working enemy back.
public static class EnemyFactory
{
    public static Enemy CreateEnemy(EnemyData data, Vector3 position)
    {
        if (data == null || data.enemyPrefab == null)
        {
            Debug.LogError("EnemyFactory is missing data or a prefab.");
            return null;
        }

        // Make the object
        GameObject instance = Object.Instantiate(data.enemyPrefab, position, Quaternion.identity);

        Enemy enemy = instance.GetComponent<Enemy>();

        if (enemy == null)
        {
            Debug.LogError("The prefab on " + data.name + " has no Enemy component.");
            Object.Destroy(instance);
            return null;
        }

        // Tell it what it is
        enemy.Initialise(data);

        return enemy;
    }
}