### Unity Demo Guide: Using `IDamagable` Interface for Testing

This guide explains how the `TestIDamagable` class works and why interfaces are useful for testing in Unity.

- When you import the package you will see the '*Test Scripts*' gameobject in the scene hierarchy
	- `Scene-CodeCommunication-pt2`

#### How the `TestIDamagable` Class Works

```c#
using UnityEngine;  
using System.Linq;  
  
public class TestIDamagable : MonoBehaviour  
{  
    [Range(1,10)]  
    [SerializeField] private int damageAmount = 1;  
    
    void Update()  
    {        
	    if (Input.GetKeyDown(KeyCode.T))  
        {            
	        IDamagable[] damagables = FindObjectsOfType<MonoBehaviour>().OfType<IDamagable>().ToArray();  
  
            foreach (IDamagable damagable in damagables)  
            {                
	            Debug.Log(damagable);  
                damagable.TakeDamage(damageAmount);  
            }  
            
            Debug.Log("Damagables: " + damagables.Length);  
        }    
    }
}
```

1. **Find Objects**: When you press **T**, the script finds all objects in the scene that implement `IDamagable`.
2. **Apply Damage**: It loops through each object and calls `TakeDamage(damageAmount)` to apply variable damage.
3. **Log Output**: The number of damageable objects and their details are printed in the console.

#### Why Interfaces are Useful for Testing

- **Flexibility**: Works with any object that implements `IDamagable`, regardless of its type.
- **Scalability**: Add new objects implementing `IDamagable` without changing the script.
- **Simplified Testing**: Allows quick testing of all objects with damage functionality using a single key press.
- **Code Reuse**: Reduces code duplication, letting you manage different objects consistently.

*Use this tool to ensure your damage system works across all relevant GameObjects!*

You can use a similar approach to build robust testing in game programming. This works especially well with interfaces and event systems. It allows us to create scenarios where we can test and verify and eventually automate the testing.