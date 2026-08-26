# **Object Pool Tutorial**

_You have been given a scene with GameObjects already placed. Follow the steps and add the scripts below._

---

# **1. Create the Object Pool**

### **Step 1: Add this script to the ObjectPool GameObject**

### **MyObjectPool.cs**

```csharp
using System.Collections.Generic;
using UnityEngine;

public class MyObjectPool : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject prefab;
    public int poolSize = 10;

    private List<GameObject> pool = new List<GameObject>();

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject GetObject()
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
                return obj;
        }

        return null;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
    }
}
```

**What this does:**  
Creates and stores a list of disabled objects ready to use.  
Lets you get and return objects.

---

# **2. Add a Lifetime Script to the Pooled Prefab**

### **Step 2: Add this script to your pooled object prefab**

### **MyPooledObject.cs**

```csharp
using UnityEngine;

public class MyPooledObject : MonoBehaviour
{
    public float lifetime = 2f;

    private float timer;
    private MyObjectPool pool;

    private void OnEnable()
    {
        timer = lifetime;
    }

    public void SetPool(MyObjectPool p)
    {
        pool = p;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (pool != null)
            {
                pool.ReturnObject(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
```

**What this does:**  
Every object has a timer. When the timer ends, it disables itself and returns to the pool.

---

# **3. Test the Pool by Spawning Objects**

### **Step 3: Add this script to the Tester GameObject**

### **MyPoolTester.cs**

```csharp
using UnityEngine;

public class MyPoolTester : MonoBehaviour
{
    public MyObjectPool pool;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject obj = pool.GetObject();

            if (obj != null)
            {
                obj.transform.position = new Vector3(
                    Random.Range(-5f, 5f),
                    1f,
                    Random.Range(-5f, 5f)
                );

                obj.GetComponent<MyPooledObject>().SetPool(pool);

                obj.SetActive(true);
            }
            else
            {
                Debug.Log("Pool empty");
            }
        }
    }
}
```

**What this does:**  
Press **Space** to spawn an object from the pool.  
It appears, lives for a short time, then returns.

---

# **4. Hook Everything Up**

### **In the Hierarchy:**

1. Select **ObjectPool**
    - Drag your prefab into **Prefab**
    - Set **Pool Size**
2. Select your pooled prefab
    - Make sure it has **MyPooledObject** attached
3. Select **PoolTester**
    - Drag **ObjectPool** into the **Pool** field
4. Press Play
    - Press **Space** to spawn recycled objects

**Thats it
	Objects get instantiated into a pool when the game starts
	used when needed and put back in the pool when not needed**
