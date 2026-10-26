
# **CRE343 Week 6 - Part 1 : Tweening and Spawn Feel**

### Overview

#### **What we are building**:
- Getting **PrimeTween** running and writing a few tweens to see what it does.
	- https://github.com/KyryloKuzyk/PrimeTween - go to the asset store to install via the package manager
- A reusable **`SpawnPop`** component that makes anything scale up when it appears.
- Dropping it on the enemies, items and player in the twin stick game.

#### **Why we are doing this**:
- This week is about **reactivity - game feel  - juice** - making the game respond to what's happening. Before we connect effects to gameplay events we need something to connect.
- Tweening is the tool you'll use for most of it. We're going to use it indifferent ways.
- A spawn pop is the cheapest juice there is. Objects appearing instantly at full size looks flat, and one component fixes it across the whole game.

#### **Learning Objectives**:
- **Tweening** - animating a value over time with a library rather than by hand.
	- Animators and animation are great for complex movement - tweens are greats from small reusable animations
- **Easing** - why the curve matters more than the duration.
- **Reusable feel components** - one script, attached to anything.
- **Self-contained vs reactive** - two different kinds of effect, and knowing which you need.

---
---

# **Part 1: Learning the Tool**

You'll be working in the CRE340 twin stick project. Either is is up to date or get my updates from Blackboard

---

### **Step 1: Two Kinds of Effect**

**Self-contained** - the object animates itself. A spawning enemy scaling up, a coin bobbing and spinning, a pickup glowing. Nothing tells it to, it just does it from the moment it exists.

**Reactive** - something happens elsewhere in the game and an effect responds. The camera shakes because an enemy died, the screen flashes because the player got hit. These need an **event**, and the thing that reacts has to be listening.

Part 1 is all self-contained. Parts 2 and 3 are all reactive...but we'll use the event system for that.

---

### **Step 2: Check PrimeTween is Installed**

Go to GitHub or the asset store to get the PrimeTween library
https://github.com/KyryloKuzyk/PrimeTween 

Its free and open source - Tween animation or 'Inbetweening' has been around for a long, long time and is a standard approach to this type of mini-animations.

1. **Window -> Package Manager** and look for **PrimeTween** in the list.
2. If it's missing, it's free on the Asset Store - add it to your account, then **Package Manager -> My Assets -> PrimeTween -> Install**.
3. Any script that uses it needs `using PrimeTween;` at the top. This means its a library we are going to use in scripts.

> [!note] **Note - why a library rather than writing it yourself**
> 
> In CRE340 you wrote the `UIBar` health bar by hand - a target value, an `Update` loop and `Mathf.Lerp`. That's about fifteen lines and is good practice for how animation can be done with interpolation (easing between two values)
> 
> PrimeTween does the same job in one line, and gives you easing curves, delays, sequences and callbacks for free. It also doesn't need an `Update` loop in your script at all. Amazing! We can just call animations in a single line!
> 
> This is a reasonable rule for libraries generally - write it yourself once so you understand what it's doing, then use the library because it handles the forty cases you didn't think of. If we have a lot of little animations or juicy UI work to do then we might use the tween library for all the different things.

---

### **Step 3: Your First Tween**

1. **Make a Cube** in the scene somewhere visible.  *you can turn off spawning to test this first*
2. **Create a script called `TweenTest.cs`** and attach it:

```csharp
using UnityEngine;
using PrimeTween;   // needed for everything in this session

public class TweenTest : MonoBehaviour
{
    private void Start()
    {
        // Scale this object up to double size over one second
        Tween.Scale(transform, endValue: Vector3.one * 2f, duration: 1f, ease: Ease.OutBack);
    }
}
```

3. **Press Play.** The cube grows, overshoots slightly, and settles.

**Explanation**:

- `Tween.Scale` takes arguments for, the thing to animate, where it's going, how long it takes, and the curve it follows.
- The named arguments (`endValue:`, `duration:`) aren't required but they make the line readable. You'll see PrimeTween's own documentation written this way.
- No `Update`, stored target or coroutine. The tween runs itself and cleans up when it's done.

---

### **Step 4: Easing**

The duration decides how long it takes. The **ease** decides how it feels. Have a go al all these and see how they feel. They are basically pre-defined animation curves.

Try swapping the ease in `TweenTest` and pressing Play each time:

|Ease|What it does|
|---|---|
|`Ease.Linear`|Constant speed. Reads as mechanical.|
|`Ease.OutQuad`|Fast then slows. The safe default for most UI.|
|`Ease.InQuad`|Slow then speeds up. Good for things leaving.|
|`Ease.OutBack`|Overshoots and settles back. The "pop".|
|`Ease.OutElastic`|Wobbles to a stop. Fun, easy to overuse.|
|`Ease.OutBounce`|Bounces. Very specific, rarely what you want.|

**`Ease.OutBack` is probably the one you want for things appearing.** The slight overshoot is what makes something feel like it _arrived_ rather than just turned up.

> [!note] **Note - try the same duration with different eases**
> 
> Keep `duration` at 0.3 and cycle through all six. The timing is identical in every case and they feel completely different.
> 
> That's the thing to take from this step. When an effect feels wrong the instinct is to change the duration, and usually the curve is the problem.
> 
> Make the values public and try out different effects - there are lot of things can be done with a single tween line.


---

### **Step 5: The `SpawnPop` Component**

Now something reusable. This goes on any prefab and makes it scale up when it appears.

1. **Create a script called `SpawnPop.cs`**:

```csharp
using UnityEngine;
using PrimeTween;

// Pops an object up to full size when it appears.
// Drop it on any prefab - enemies, items, the player, anything spawned.
// Knows nothing about what it's attached to.
public class SpawnPop : MonoBehaviour
{
    [Header("Pop")]
    public float duration = 0.3f;
    public Ease ease = Ease.OutBack;
    public float startDelay = 0f;      // useful for staggering a group

    private Vector3 fullSize;

    private void Awake()
    {
        // Remember the size set on the prefab, then shrink to nothing
        fullSize = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        Tween.Scale(transform, endValue: fullSize, duration: duration, ease: ease, startDelay: startDelay);
    }
}
```

2. **Put it on the `Enemy_Green` prefab** and press Play. Enemies now pop in as they spawn.

**Explanation**:
This is a really simple script concept for instantiating GameObjects 
1. Save the original scale value
2. Set it to zero size
3. Animate it back to its original scale with some easing

- `Awake` grabs the scale you set in the Inspector and shrinks the object to zero. The tween then takes it back to where it should have been, so you never have to type the size into the script.
- `Ease` is a public enum, so it shows as a dropdown in the Inspector. Every prefab can have a different feel with no code changes.
- Nothing in here mentions enemies. Put it on a coin and it works the same.

> [!warning] **Note - scaling to zero in `Awake` has one catch**
> 
> If any other script on the object reads `transform.localScale` during `Awake` or `Start`, it'll get zero rather than the real size.
> 
> Nothing in our project does, but it's the kind of thing that causes a confusing bug six later. If there is potential issues store the size in `Awake` and do the shrink in `Start` instead. 

---

### **Step 6: Put It on Everything**

1. **Both enemy prefabs** - `Enemy_Green` and `Enemy_Blue`. Give Blue a slightly longer duration so the heavy one feels heavier.
2. **The items** - `HealthPotion`, `ManaPotion` and `Coin`. Shorter duration, around 0.2, so pickups feel light.
3. **The Player prefab.** Use a longer duration, maybe 0.5, and try `Ease.OutElastic`. The player respawning is a moment worth drawing out.

**Press Play** and watch a full round. Enemies pop in, items pop in, and when you die the player pops back after the respawn delay.

**Explanation**:

- One script, six prefabs, no code written per object. That's the point of a component that does one job.
- The variation is all Inspector values. 
	- A designer with no programming could tune the whole game's spawn feel.
	- A programmer could make a script that exposes a whole load of tween options for designers to work with.

> [!note] **Note - a small thing with a big effect**
> 
> Turn `SpawnPop` off on everything and play for a minute. Then turn it back on.
> 
> Nothing about the game changed - same spawn rate, same positions, same everything. It just stopped feeling like objects were being dumped into the scene.

---

### **Step 7: Sequences**

One tween is might not be enough. A **Sequence** chains them.

- **Side note:**  you could also use co-routines for sequencing tweens and animations. There are times you'll do that but chain allows us to keep it in one function.

Try this version of the pop - overshoot, then settle:

```csharp
    private void Start()
    {
        Sequence.Create()
            .Chain(Tween.Scale(transform, endValue: fullSize * 1.3f, duration: duration * 0.6f, ease: Ease.OutQuad))
            .Chain(Tween.Scale(transform, endValue: fullSize, duration: duration * 0.4f, ease: Ease.OutQuad));
    }
```

**Explanation**:

- `.Chain()` runs a tween **after** the previous one finishes.
- `.Group()` runs it **at the same time** - useful for scaling and spinning together.
- You can chain as many as you like, and add `.ChainDelay()` for a gap between steps.

Whether this is better than the single `OutBack` version is a judgement call. Try both and pick.

> [!note] **Note - two ways to get the same look**
> 
> `Ease.OutBack` on one tween and a two-step sequence produce a similar overshoot. The sequence gives you control over exactly how far and how long each stage runs.
> 
> Start with the single tween. Use a sequence when you need something the curve can't do

---

### **Step 8: Tidy Up**

1. **Delete `TweenTest.cs`** and the test cube.
2. **Check the Console** for warnings about tweens on destroyed objects. PrimeTween stops them automatically when the target is destroyed, so you shouldn't see any, but it's worth knowing that's what would cause them.

> [!note] **Note - tween capacity**
> 
> PrimeTween pre-allocates room for a number of running tweens and warns in the Console if you go past it.
> 
> We'll be firing effects five times a second in Part 2, so if you see that warning, raise it once at startup with `PrimeTweenConfig.SetTweensCapacity(200);`. 


---

### **What we've set up**:

- **PrimeTween** installed and understood - one line to animate a value, with a curve.
- A reusable **`SpawnPop`** component on every spawned object in the game.
- A feel for **easing**, which is most of what makes an effect look good.

### **Next**:

Everything so far is **self-contained** - objects animating themselves with no idea what's going on around them.

- You can also go off and explore the tween library - It does all kinds of animations - wobbles, pops, shakes, positions, rotations etc...

In **Part 2** we connect to the game. The camera will react to firing and to enemies dying, using the event system from CRE340, and the gameplay code won't gain a single reference to Cinemachine.