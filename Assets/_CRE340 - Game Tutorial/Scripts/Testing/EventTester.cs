using UnityEngine;

public class EventTester : MonoBehaviour
{
    public int testValueA = 10;
    public int testValueB = 20;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            UIEventManager.HealthChanged(testValueA, testValueB);
        }
    }
}
