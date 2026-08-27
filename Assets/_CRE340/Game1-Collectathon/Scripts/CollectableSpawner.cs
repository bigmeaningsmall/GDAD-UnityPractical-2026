using UnityEngine;  
  
public class CollectibleSpawner : MonoBehaviour  
{  
    //reference to our prefab that we will spawn
    public GameObject collectiblePrefab;  
    
    public int numberOfCollectibles = 10;  
    public Vector3 spawnArea; // x, y, z (width, height, depth) of the spawn area (20,0,20)  
  
    void Awake()  
    {        
        SpawnCollectibles();  // call our spawn function 'before' the game starts using awake
    }  
    
    void SpawnCollectibles()  
    {        
        // loop through the number of collectables we want to create - set a random position in the area - instantiate the collectable
        for (int i = 0; i < numberOfCollectibles; i++)  
        {            
            // random position
            Vector3 randomPosition = new Vector3(  
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),  
                Random.Range(0, spawnArea.y),  
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)  
            );  
            
            //create the collectable
            Instantiate(collectiblePrefab, randomPosition, Quaternion.identity);  // instantiate takes 3 parameters (GameObject, position, rotation)
        }    
    }  
	
    // handy debugging for drawing things in the editor
    void OnDrawGizmosSelected()  
    {        
        Gizmos.color = Color.green;  
        Gizmos.DrawWireCube(transform.position, spawnArea);  
    }
}