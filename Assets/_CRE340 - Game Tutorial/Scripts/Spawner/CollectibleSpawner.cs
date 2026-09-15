using UnityEngine;

public class CollectableSpawner : MonoBehaviour
{
    // reference to the prefab we will spawn
    public GameObject collectablePrefab;

    public int numberOfCollectables = 10;
    public Vector3 spawnArea; // x, y, z (width, height, depth) of the spawn area (e.g. 20, 0, 20) - set in the editor
    public int yOffset = 1; // how high above the ground to spawn the collectables (e.g. 1 unit above the ground)

    void Awake()
    {
        SpawnCollectables(); // call our spawn function 'before' the game starts using Awake
    }

    void SpawnCollectables()
    {
        // loop through the number of collectables we want - pick a random position in the area - instantiate the collectable
        for (int i = 0; i < numberOfCollectables; i++)
        {
            // random position
            Vector3 randomPosition = new Vector3(
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                Random.Range(0, spawnArea.y) + yOffset,
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
            );

            // create the collectable
            GameObject collectable = Instantiate(collectablePrefab, randomPosition, Quaternion.identity); // Instantiate takes 3 parameters (GameObject, position, rotation)
            
            // parent them to the spawner for the craic! - this is optional but it keeps the hierarchy clean and organized
            collectable.transform.parent = this.transform;
            
        }
    }

    // handy debugging for drawing things in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, spawnArea);
    }
}