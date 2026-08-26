### **Guide to Caching in Unity**

Caching is an optimisation technique that involves storing references to frequently used components or objects to avoid repetitive and costly operations, such as calling `GetComponent`. 

Most of your code will use this approach naturally when initialising variables, but it is worth noting there are times when caching or non-caching perform better. In games with large numbers of component calls and references caching is essential for memory management.

This guide explains caching with a practical example, compares non-cached and cached loops, and walks through a benchmarking exercise to measure the performance difference.

---

### **When to Cache**

- **Frequent Access**: Cache components or values that are accessed multiple times in a frame (e.g., inside `Update`, loops, or frequently called methods).
- **Expensive Operations**: Cache results of operations that involve significant overhead, such as:
  - `GetComponent`
  - `Find` methods (e.g., `GameObject.Find`, `Transform.Find`)
  - Repeated calculations (e.g., physics or mathematical operations).
- **Complex Hierarchies**: Cache references to objects or components in deeply nested hierarchies to avoid repeated traversals.
- **Static or Slow-Changing Data**: Cache when the data or component reference does not change often (e.g., a player’s `Rigidbody` or `Renderer`).
- **Performance-Critical Code**: In performance-critical sections of the game, such as rendering, AI calculations, or physics updates.

---

### **When Not to Cache**

- **Rare Access**: Do not cache components or values that are accessed infrequently or only once.
- **Rapidly Changing Data**: Avoid caching data or references that change frequently or unpredictably (e.g., dynamic UI elements that are destroyed and recreated often).
- **Memory Overhead**: Be cautious when caching large data structures or components to avoid excessive memory usage.
- **Premature Optimisation**: Avoid caching unless profiling shows a measurable performance impact. Focus on readability and maintainability first.
- **Unnecessary Duplication**: If a value is already cached by another system (e.g., Unity's `transform` property is inherently fast), additional caching may be redundant. 

---

### **General Rule of Thumb**
Cache when you:
- Need to optimise repetitive and costly operations.
- Are working in high-frequency methods or loops.
- Have identified a performance bottleneck via profiling.

Avoid caching when it adds unnecessary complexity or when the data/component usage is too infrequent to justify the effort.


---

### **Non-Cached vs Cached Loop Example**

Here’s an example comparing **non-cached** and **cached** loops when accessing a `Renderer` component in Unity. This demonstrates how caching a component reference outside the loop (e.g., in `Start()`) reduces overhead.

---

#### **Non-Cached Loop Example**
In this example, the `GetComponent<Renderer>()` call is made inside the loop, repeatedly accessing the `Renderer` component.

```csharp
using UnityEngine;

public class NonCachedLoop : MonoBehaviour
{
    void Update()
    {
        for (int i = 0; i < 100; i++) // Example: Loop 100 times
        {
            // Accessing the Renderer component every iteration
            GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);
        }
    }
}
```

- **Problem**: `GetComponent<Renderer>()` is called 100 times during every `Update()`. This introduces unnecessary overhead as Unity performs a search for the `Renderer` component each time.

---

#### **Cached Loop Example**
In this example, the `Renderer` component is cached in `Start()`, so the loop uses the cached reference.

```csharp
using UnityEngine;

public class CachedLoop : MonoBehaviour
{
    private Renderer cachedRenderer; // Cached reference to the Renderer

    void Start()
    {
        // Cache the Renderer component once
        cachedRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        for (int i = 0; i < 100; i++) // Example: Loop 100 times
        {
            // Use the cached Renderer reference
            cachedRenderer.material.color = new Color(Random.value, Random.value, Random.value);
        }
    }
}
```

- **Benefit**: The `Renderer` component is fetched only once in `Start()`, and the cached reference is reused in the loop. This eliminates redundant component searches, improving performance.

---

### **Performance Impact**

- **Non-Cached**:
  - Each call to `GetComponent<Renderer>()` traverses the object hierarchy to find the `Renderer` component.
  - Increases CPU usage, especially if the loop runs frequently (e.g., in `Update()`).
  - Can lead to noticeable slowdowns in larger, more complex scenes or frequent loops.

- **Cached**:
  - The component is found once and stored, avoiding repeated hierarchy traversal.
  - Reduces CPU overhead and ensures the code is more efficient.

---

### **Why Benchmark Caching?**

Benchmarking allows you to measure execution times and understand the performance benefits of optimisations like caching. It helps quantify the improvement and demonstrates why avoiding repetitive operations is essential in high-frequency code like `Update`.

**Stopwatch** from `System.Diagnostics` is a tool we can use to measure the time taken by specific code blocks. 

---

### **Practical Steps: Caching and Benchmarking the Difference**

#### **Step 1: Add a Sphere to the Scene**
1. In Unity, create a **Sphere** in your scene.
2. Attach the provided `CacheBenchmark` script to the sphere.

#### **Step 2: Add a Button to Trigger the Benchmark**
1. Add a **Button** to the Canvas in your scene.
2. Rename the button to `Benchmark: Cache`.
3. In the Button's **OnClick** event:
   - Drag the Sphere with the script attached into the slot.
   - Select `CacheBenchmark > RunBenchmarks`.

---

#### **Step 3: The `CacheBenchmark` Script**

Here’s the script to attach to the sphere:

```csharp
using UnityEngine;
using System.Diagnostics;

public class CacheBenchmark : MonoBehaviour
{
    private Renderer cachedRenderer; // Cached reference for the Renderer

    void Start()
    {
        // Cache the Renderer component once
        cachedRenderer = GetComponent<Renderer>();
    }

    public void RunBenchmarks()
    {
        BenchmarkNonCachedLoop();
        BenchmarkCachedLoop();
    }
    
    private void BenchmarkNonCachedLoop()
    {
        // Stopwatch to measure execution time
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < 100000; i++) // Increase the iterations for meaningful results
        {
            // Non-cached loop: repeatedly call GetComponent
            GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"Non-Cached Loop Time: {stopwatch.ElapsedMilliseconds} ms");
    }

    private void BenchmarkCachedLoop()
    {
        // Stopwatch to measure execution time
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < 100000; i++) // Same number of iterations as non-cached
        {
            // Cached loop: use the pre-cached reference
            cachedRenderer.material.color = new Color(Random.value, Random.value, Random.value);
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"Cached Loop Time: {stopwatch.ElapsedMilliseconds} ms");
    }
}
```

---

#### **Step 4: Run the Benchmark**
1. Enter Play mode in Unity.
2. Click the `Benchmark:Cache` button.
3. Observe the output in the Console:
   - Example Output:
     ```
     Non-Cached Loop Time: 45 ms
     Cached Loop Time: 25 ms
     ```

---

### **Expected Outcome**
- The **non-cached loop** will take significantly more time because `GetComponent` is repeatedly called in each iteration.
- The **cached loop** will be faster, demonstrating the performance benefit of caching components.

---

### **Key Takeaways**
- **Caching Saves Resources**: Always cache frequently used components to reduce CPU overhead, especially in high-frequency methods like `Update`.
- **Benchmarking Shows Improvement**: Measuring performance helps identify bottlenecks and confirm the effectiveness of optimisations.
- **Practical Application**: This exercise demonstrates how small changes can make a significant impact on performance in real-world scenarios.