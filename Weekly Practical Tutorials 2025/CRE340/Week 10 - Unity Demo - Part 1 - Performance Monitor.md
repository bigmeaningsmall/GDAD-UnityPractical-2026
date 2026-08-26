
### **Guide to Writing a Performance Monitor in Unity**

Monitoring and benchmarking performance metrics in Unity is crucial to understanding how efficiently your game runs and identifying bottlenecks. This guide walks you through creating a **Performance Monitor** script, explains key performance metrics, and demonstrates benchmarking a heavy computation.

---

### **Performance Metrics**

1. **Frames Per Second (FPS)**:
   - **What It Measures**: The number of frames rendered per second.
   - **Why It's Important**:
     - Indicates how smooth the gameplay feels.
     - A low FPS suggests that the game is demanding too many resources or the hardware is underperforming.
     - Maintaining a steady FPS is essential for a good player experience.

2. **Average FPS**:
   - **What It Measures**: The average frames per second over the entire gameplay session.
   - **Why It's Important**:
     - Gives an overview of performance trends over time.
     - Helps identify if performance issues are persistent or temporary spikes.

3. **CPU Frame Time**:
   - **What It Measures**: The time (in milliseconds) the CPU takes to process a single frame.
   - **Why It's Important**:
     - High CPU frame times indicate heavy computational loads, such as physics calculations, AI processing, or game logic execution.

4. **GPU Frame Time**:
   - **What It Measures**: The time (in milliseconds) the GPU takes to render a single frame.
   - **Why It's Important**:
     - High GPU frame times indicate rendering bottlenecks, such as too many draw calls, high-resolution textures, or expensive shaders.

5. **Memory Usage**:
   - **What It Measures**: The amount of memory (RAM) currently used by the game.
   - **Why It's Important**:
     - High memory usage can lead to crashes or slowdowns, especially on devices with limited memory.
     - Helps identify memory leaks or overly large data structures.

---

### **Creating the Performance Monitor Script**

#### **Step 1: Write the Script**

1. **Purpose**: The script displays FPS, frame times, and memory usage on a **TextMeshPro** UI component in real-time.
2. **Script**: Use the provided script below.

```csharp
using UnityEngine;
using TMPro;
using System.Diagnostics;

public class PerformanceMonitor : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI performanceText; // Assign a TextMeshProUGUI component in the Inspector

    private float deltaTime = 0.0f;
    private int frameCount = 0;
    private float timeElapsed = 0.0f;
    private float totalTime = 0.0f;
    private int totalFrames = 0;

    private Stopwatch stopwatch;

    void Start()
    {
        stopwatch = new Stopwatch();
    }

    void Update()
    {
        // FPS Calculation
        deltaTime += Time.unscaledDeltaTime;
        timeElapsed += Time.unscaledDeltaTime;
        frameCount++;
        totalFrames++;

        if (deltaTime >= 1.0f)
        {
            float fps = frameCount / deltaTime;
            float avgFps = totalFrames / totalTime;

            // Memory Usage
            long memoryUsed = System.GC.GetTotalMemory(false); // In bytes

            // Frame Times
            float cpuFrameTime = Time.deltaTime * 1000f; // CPU frame time in milliseconds
            float gpuFrameTime = Time.unscaledDeltaTime * 1000f; // Estimated GPU frame time

            // Display Metrics
            performanceText.text =
                $"FPS: {fps:F2} \nAvg FPS: {avgFps:F2}\n" +
                $"CPU Frame Time: {cpuFrameTime:F2} ms \nGPU Frame Time: {gpuFrameTime:F2} ms\n" +
                $"Memory: {memoryUsed / (1024f * 1024f):F2} MB";

            // Reset for the next interval
            deltaTime = 0.0f;
            frameCount = 0;
        }

        totalTime += Time.unscaledDeltaTime;
    }


//NOT NEEDED IF YOU JUST WANT A UI PERFORMANCE MONITOR!!!!
    public void BenchmarkMethod()
    {
        UnityEngine.Debug.Log("Starting benchmark...");
        stopwatch.Reset();
        stopwatch.Start();

        PerformHeavyCalculation();

        stopwatch.Stop();
        UnityEngine.Debug.Log($"Benchmark completed. Execution Time: {stopwatch.ElapsedMilliseconds} ms");
    }

//NOT NEEDED IF YOU JUST WANT A UI PERFORMANCE MONITOR!!!!
    private void PerformHeavyCalculation()
    {
        int[] numbers = new int[10000000];
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = Random.Range(0, 10000); // Fill the array with random numbers

            // Log progress at every 100,000 iterations
            if (i % 100000 == 0)
            {
                UnityEngine.Debug.Log($"Processing... {i} iterations completed.");
            }
        }

        // Sort the array as part of the heavy calculation
        System.Array.Sort(numbers);
        UnityEngine.Debug.Log("Sorting completed.");
    }
}
```

---

#### **Step 2: Connect the Script to the UI**
1. Drag the script onto the **Performance Metrics** TextMeshPro object.
2. Assign the TextMeshPro component to the `performanceText` field in the Inspector.

---

### **Benchmarking Heavy Calculations**

#### **Purpose of the Heavy Calculation**
The heavy calculation simulates a computationally expensive operation, such as sorting large data sets. This reflects real-world scenarios like:
- Pathfinding for large AI groups.
- Massive physics simulations.
- Complex mathematical operations for shaders or visual effects.

#### **Connecting to the Button**
1. Add the function `BenchmarkMethod` to the **Benchmark:Math** button:
   - Drag the **Performance Metrics** object into the button’s **OnClick** event.
   - Select the `PerformanceMonitor > BenchmarkMethod()` function.
2. Click the button during Play mode to observe:
   - Lag during execution.
   - Benchmark results in the Console.

---

### **Impact of Heavy Computations**
- **Frame Drops**: When heavy calculations run in the main thread, they delay the next frame, causing stuttering or reduced FPS.
- **Player Experience**: Excessive lag disrupts gameplay and frustrates players.
- **Optimisation Strategy**:
  - Move heavy computations to background threads or use Unity’s **Job System**.
  - Break calculations into smaller tasks and spread them over multiple frames.

---

### **Key Takeaways**
- **Why Measure Performance**:
  - Helps identify bottlenecks in your game.
  - Provides insights into whether CPU, GPU, or memory is the limiting factor.
- **Practical Application**:
  - Ensures smooth gameplay by optimising resource-heavy operations.
- **Next Steps**:
  - Experiment with the performance monitor to see how gameplay changes impact metrics.




