### Unity Demo Guide: Modifying the `ObjectSpawner` Class to Use Arrays and Lists

In this guide, you will modify the `ObjectSpawner` class to introduce the use of arrays for spawning different object types and lists to keep track of all spawned objects. This is an important learning step to understand how to manage multiple objects in a dynamic game environment using arrays and lists.

#### Step 1: Review the `ObjectSpawner` Class

The `ObjectSpawner` class uses a single `GameObject` to spawn objects and does not track them after they have been instantiated. Here’s what the original class looks like:

**Original Code:**
```c#
public class ObjectSpawner : MonoBehaviour  
{  
    public GameObject objectPrefab;  // A single prefab to spawn  
    public Vector3 spawnArea;        // x, y, z (width, height, depth) of the spawn area  
    public float spawnHeight = 0.5f; // Height at which to spawn objects
    public float minSpawnInterval = 1f; // Minimum spawn interval (1 second)  
    public float maxSpawnInterval = 3f; // Maximum spawn interval (3 seconds)  
      
    void Start()  
    {  
        // Start invoking the SpawnObject method at a random interval  
        InvokeRepeating("SpawnRandomObject", Random.Range(minSpawnInterval, maxSpawnInterval), Random.Range(minSpawnInterval, maxSpawnInterval));  
    }  
  
    void SpawnRandomObject()  
    {  
        if (objectPrefab == null) return;  // Ensure there is something to spawn  
  
        // Generate a random position within the spawn area  
        Vector3 randomPosition = new Vector3(  
            Random.Range(-spawnArea.x / 2, spawnArea.x / 2),  
            Random.Range(spawnHeight, spawnHeight + spawnArea.y),
            Random.Range(-spawnArea.z / 2, spawnArea.z / 2)  
        );  
  
        // Instantiate the prefab at the random position  
        Instantiate(objectPrefab, randomPosition, Quaternion.identity);  
          
        // Reschedule the next spawn with a new random interval  
        CancelInvoke("SpawnRandomObject");  
        Invoke("SpawnRandomObject", Random.Range(minSpawnInterval, maxSpawnInterval));  
    }  
  
    // Method to visualize the spawn area in the Scene view  
    void OnDrawGizmosSelected()  
    {  
        Gizmos.color = Color.green;  
        Gizmos.DrawWireCube(transform.position, spawnArea);  
    }  
}
```

#### Step 2: Modify the Class to Use Arrays and Lists

Now, we will modify the class to spawn multiple types of objects using an array and keep track of all spawned objects using a list.

- **Copy the code into the existing `ObjectSpawner` as needed or copy the whole thing**

Here are the key modifications:

1. **Array for Prefabs**: Instead of a single prefab, we use an array `GameObject[]` to store multiple object types that can be randomly selected when spawning.
   
2. **List to Track Spawned Objects**: We introduce a list `List<GameObject>` to store references to all the objects that have been spawned. This allows us to manage, count, or remove objects later if needed.

**Modified Code:**
```c#
using UnityEngine;  
using System.Collections.Generic; // to use Lists we need to use the system collections library

public class ObjectSpawner : MonoBehaviour  
{  
    public GameObject[] objectPrefabs;  // Array of prefabs to spawn  
    public Vector3 spawnArea;           // x, y, z (width, height, depth) of the spawn area 
    public float spawnHeight = 0.5f; // Height at which to spawn objects 
    public float minSpawnInterval = 2f; // Minimum spawn interval (2 seconds)  
    public float maxSpawnInterval = 5f; // Maximum spawn interval (5 seconds)  
  
    // List to store references to all spawned objects  
    public List<GameObject> spawnedObjects = new List<GameObject>();  
  
    void Start()  
    {  
        // Start invoking the SpawnObject method at a random interval  
        InvokeRepeating("SpawnRandomObject", Random.Range(minSpawnInterval, maxSpawnInterval), Random.Range(minSpawnInterval, maxSpawnInterval));  
    }  
  
    void SpawnRandomObject()  
    {  
        if (objectPrefabs.Length == 0) return;  // Ensure there is something to spawn  
  
        // Pick a random prefab from the array  
        int randomIndex = Random.Range(0, objectPrefabs.Length);  
        GameObject prefabToSpawn = objectPrefabs[randomIndex];  
  
        // Generate a random position within the spawn area  
        Vector3 randomPosition = new Vector3(  
            Random.Range(-spawnArea.x / 2, spawnArea.x / 2),  
            Random.Range(spawnHeight, spawnHeight + spawnArea.y),
            Random.Range(-spawnArea.z / 2, spawnArea.z / 2)  
        );  
  
        // Instantiate the prefab at the random position  
        GameObject spawnedObject = Instantiate(prefabToSpawn, randomPosition, Quaternion.identity);  
  
        // Add the newly spawned object to the list  
        spawnedObjects.Add(spawnedObject);  
  
        // Reschedule the next spawn with a new random interval  
        CancelInvoke("SpawnRandomObject");  
        Invoke("SpawnRandomObject", Random.Range(minSpawnInterval, maxSpawnInterval));  
    }  
  
    // Method to visualize the spawn area in the Scene view  
    void OnDrawGizmosSelected()  
    {  
        Gizmos.color = Color.green;  
        Gizmos.DrawWireCube(transform.position, spawnArea);  
    }  
  
    // Method to show the number of spawned objects in the list  
    public void ShowSpawnedObjectsCount()  
    {  
        Debug.Log("Number of spawned objects: " + spawnedObjects.Count);  
    }  
}
```

**Key Modifications:**
- The `objectPrefabs[]` array replaces the single `objectPrefab` to allow multiple types of objects to be spawned.
- The `spawnedObjects` list keeps track of every object spawned, which can later be accessed or modified.
- The method `ShowSpawnedObjectsCount()` demonstrates how to access and use the list to check the number of spawned objects.

#### Step 3: Drag Prefabs into the Spawner

1. **Add Prefabs to the Array**: In the **Inspector** window, under the `ObjectSpawner` component, you will now see an `Object Prefabs` array. Drag the `Crate` and `Exploding Crate` prefabs into this array to enable random spawning of these objects.

   - Drag `Crate` into the first slot of the array.
   - Drag `Exploding Crate` into the second slot of the array.

#### Step 4: Testing and Observing the List

1. **Start the Game**: Once you have assigned the prefabs, hit the Play button in Unity. Objects will begin to spawn randomly within the defined spawn area.
   
2. **Check Spawned Object Count**: During gameplay, press the **O** key to see how many objects have been spawned. This uses the `ShowSpawnedObjectsCount()` method, which outputs the count to the console.

---

#### Testing
- Run the game and you should see different crate types spawn at random intervals. You can adjust the parameters of the spawner as needed.