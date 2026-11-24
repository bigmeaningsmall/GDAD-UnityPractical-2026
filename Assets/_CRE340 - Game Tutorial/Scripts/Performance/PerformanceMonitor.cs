using UnityEngine;
using TMPro;

// Shows what the game is actually costing, on screen, while it runs.
// Put this on a TextMeshProUGUI object in the Canvas.
public class PerformanceMonitor : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI performanceText;

    [Header("Settings")]
    public float sampleTime = 0.5f;     // how often the display updates, in seconds

    // This sample window - reset every time we update the display
    private int framesThisWindow;
    private float timeThisWindow;
    private float worstFrameThisWindow;

    // The whole session
    private int totalFrames;
    private float totalTime;

    // Garbage collection
    private int lastCollectionCount;

    private void Start()
    {
        if (performanceText == null)
        {
            performanceText = GetComponent<TextMeshProUGUI>();
        }

        lastCollectionCount = System.GC.CollectionCount(0);
    }

    private void Update()
    {
        // unscaledDeltaTime, so pausing the game with Time.timeScale doesn't change the readings
        float frameTime = Time.unscaledDeltaTime;

        framesThisWindow = framesThisWindow + 1;
        timeThisWindow = timeThisWindow + frameTime;

        totalFrames = totalFrames + 1;
        totalTime = totalTime + frameTime;

        // Keep the slowest frame in this window - the spikes are what you feel
        if (frameTime > worstFrameThisWindow)
        {
            worstFrameThisWindow = frameTime;
        }

        if (timeThisWindow >= sampleTime)
        {
            UpdateDisplay();

            framesThisWindow = 0;
            timeThisWindow = 0f;
            worstFrameThisWindow = 0f;
        }
    }

    private void UpdateDisplay()
    {
        if (performanceText == null)
        {
            return;
        }

        float fps = framesThisWindow / timeThisWindow;
        float averageFps = totalFrames / totalTime;

        float averageFrameMs = (timeThisWindow / framesThisWindow) * 1000f;
        float worstFrameMs = worstFrameThisWindow * 1000f;

        // Managed memory - what our C# objects are using. This is what the collector cleans up.
        float memoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f);

        // How many times the collector has run. A number that keeps climbing means
        // something is creating objects and throwing them away.
        int collections = System.GC.CollectionCount(0);
        int newCollections = collections - lastCollectionCount;
        lastCollectionCount = collections;

        performanceText.text =
            "FPS: " + fps.ToString("F0") + "   (avg " + averageFps.ToString("F0") + ")\n" +
            "Frame: " + averageFrameMs.ToString("F1") + " ms   worst " + worstFrameMs.ToString("F1") + " ms\n" +
            "Memory: " + memoryMB.ToString("F1") + " MB\n" +
            "GC runs: " + collections + "   (+" + newCollections + ")";
    }
}