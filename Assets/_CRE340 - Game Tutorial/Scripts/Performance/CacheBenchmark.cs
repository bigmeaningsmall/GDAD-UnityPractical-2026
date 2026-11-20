using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

// Measures the cost of looking a component up every time against looking it up once.
// Put this on any object with a Renderer, then call RunBenchmarks from a UI Button.
public class CacheBenchmark : MonoBehaviour
{
    public int iterations = 100000;

    private Renderer cachedRenderer;    // looked up once and kept

    private float total;                // stops the compiler deciding the loop does nothing

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();

        if (cachedRenderer == null)
        {
            Debug.LogError("CacheBenchmark needs a Renderer on the same object.");
        }
    }

    public void RunBenchmarks()
    {
        if (cachedRenderer == null)
        {
            return;
        }

        // Run both once and throw the results away. The first run of any method
        // includes the time to compile it, so whichever went first would look slow.
        Warmup();

        double notCachedMs = TimeNotCached();
        double cachedMs = TimeCached();

        Debug.Log("Looked up every time : " + notCachedMs.ToString("F2") + " ms");
        Debug.Log("Cached once          : " + cachedMs.ToString("F2") + " ms");
        Debug.Log("Caching was " + (notCachedMs / cachedMs).ToString("F1") + " times faster over "
                  + iterations + " iterations.");
    }

    private void Warmup()
    {
        for (int i = 0; i < 1000; i++)
        {
            total = total + GetComponent<Renderer>().bounds.size.x;
            total = total + cachedRenderer.bounds.size.x;
        }
    }

    private double TimeNotCached()
    {
        Stopwatch watch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            // The lookup happens every single time round the loop
            Renderer found = GetComponent<Renderer>();

            total = total + found.bounds.size.x;
        }

        watch.Stop();

        return watch.Elapsed.TotalMilliseconds;
    }

    private double TimeCached()
    {
        Stopwatch watch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            // Exactly the same work, but the lookup was done once in Awake
            total = total + cachedRenderer.bounds.size.x;
        }

        watch.Stop();

        return watch.Elapsed.TotalMilliseconds;
    }
}