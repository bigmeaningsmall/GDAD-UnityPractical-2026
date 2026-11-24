---
Order: 10-A
---
# **Week 10 : Performance Tools and Checks**

## **Performance Monitor**

Goes on a **TextMeshProUGUI** object in the Canvas. Updates twice a second.

| Line        | What it is                                                                       | What to watch for                                                                                                                     |
| ----------- | -------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| **FPS**     | Frames per second over the last half second                                      | The number everyone quotes. Least useful of the four                                                                                  |
| **avg**     | FPS across the whole session                                                     | Tells you whether a drop was a blip or the new normal                                                                                 |
| **Frame**   | Average milliseconds per frame in that half second                               | 16.7 ms is 60 FPS, 33 ms is 30 FPS. Easier to reason about than FPS because it adds up - two things costing 8 ms each is a full frame |
| **worst**   | The slowest single frame in that half second                                     | The one that matters. A game averaging 60 FPS with occasional 90 ms frames feels worse than a steady 40                               |
| **Memory**  | Managed memory our C# objects are using                                          | Should settle and stay roughly flat. Climbing forever means something is being kept that shouldn't be                                 |
| **GC runs** | How many times the garbage collector has run, and how many since the last update | **This is the one for this week**                                                                                                     |

**Why GC runs is the useful one.** The collector runs when enough objects have been created and abandoned. Every time it runs you get one frame that takes much longer than the others, which is the stutter you feel but don't see in an average.

Hold the fire button with pooling off and the number should climb. Turn pooling on and it mostly stops. FPS will barely move either way, which is exactly why FPS on its own is a poor way to judge this.

**Averages hide problems.** The reason there's a `worst` column is that a stutter is one bad frame among sixty good ones. Average it out and it vanishes from the reading while still being obvious to anyone playing.

> [!note] **Note - what this doesn't tell you**
> 
> It doesn't split CPU from GPU time. Separating them properly needs `FrameTimingManager` or the Profiler, because the frame duration you can read in `Update` is just how long the whole frame took.
> 
> This tells you **that** something is slow. The **Profiler** (Window → Analysis → Profiler) tells you **what**. Use this one to notice, and the Profiler to investigate.

---

## **Cache Benchmark**

Goes on anything with a Renderer. Wire `RunBenchmarks` to a UI Button's **OnClick** and press it in Play mode.

It runs the same loop twice - once calling `GetComponent<Renderer>()` on every iteration, once using a reference looked up in `Awake` - and prints both times plus the ratio.

**What caching means.** Looking something up and keeping the result, instead of looking it up again every time you need it. You already do this without thinking about it:

```csharp
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();   // once
    }
```

Every script we've written this term does that. `Bullet`, `Enemy`, `PlayerStats`. The benchmark just puts a number on why.

**Things to know:**

- **`GetComponent` searches.** It walks the components on the object looking for a type match. Cheap once, not cheap a hundred times a frame.
- **`GameObject.Find` and `FindFirstObjectByType` are much worse** - they search the whole scene. Fine in `Awake` or `Start`, never in `Update`. `Shoot` uses `FindFirstObjectByType` to get the bullet pool, which is why it's in `Start` and not where it's needed.
- **`transform` is already cached** by Unity, so `transform.position` needs no cache.
- **There's a warmup pass before the timing starts.** The first run of any method includes compiling it, so without the warmup whichever benchmark ran first would look slower than it is. That's a general rule for benchmarking anything.

---

## **Using Them Together**

The order to do things in, which is the actual point of the week:

1. **Notice** something feels bad - a stutter, a slow patch.
2. **Measure** it, so you know whether it's real and how bad.
3. **Find** the cause with the Profiler, rather than guessing.
4. **Fix** that one thing.
5. **Measure again** to check it worked.

Most optimisation work goes wrong at step 3. People guess, rewrite something that was never the problem, and make the code worse for no gain. The scripts here cover steps 2 and 5.

The Unity Profiler is the way to do optimisation analysis so thats where most of the work takes place.