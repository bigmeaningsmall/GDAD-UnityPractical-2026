using UnityEngine;
using System.Linq;

public class InterfaceTester : MonoBehaviour
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
                damagable.TakeDamage(damageAmount); // will kill player too 
            }

            Debug.Log("Damagables: " + damagables.Length);
        }
    }
}