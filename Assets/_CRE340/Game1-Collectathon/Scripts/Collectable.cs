using UnityEngine;  
  
public class Collectable : MonoBehaviour  
{  
    // check when the object/collectable is hit with a trigger - note the 
    void OnTriggerEnter(Collider other)  
    {        
        // check if the object that hit the trigger ios the Player - Player is a default tag 
        if (other.CompareTag("Player"))  
        {            
            // we are calling the gamemanager to increase the score - using a 'static instance' / 'lazy singleton' (not best practice but does the job!)
            GameManager_Collectathon.instance.IncreaseScore();  
	        
            //destroy the collatable - we are saying 'Destroy and passing this gameobject as the parameter' (Destroy 'self')
            Destroy(this.gameObject);  
        }    
    }   
}