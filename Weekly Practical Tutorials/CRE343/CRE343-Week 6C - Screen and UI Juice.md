---
Order: 6-E
---

# **CRE343 Week 6 - Part 3 : Screen and UI Juice**

### Overview

#### **What we are building**:
- A **damage flash** using the post-processing vignette, tweened up and back.
- **Chromatic aberration** layered on top of it.
- A **desaturation** on game over.
- A **punch** on the coin counter when it changes.
#### **Why we are doing this**:
- Post-processing is the cheapest way to make the whole screen respond to something. One value tweened, and the game looks like it reacted.
- The UI is where the player's eye already is, so a small amount of movement there goes a long way.
- Layering several effects on one event is where feel work starts getting interesting, and it's the same fan-out the event bus was built for.
#### **Learning Objectives**:
- **Reaching post-processing values from code** through a Volume profile.
- **Tweening arbitrary values** with `Tween.Custom` and sequences.
- **Layering** - several effects responding to one event.
- **Direction of change** - telling damage from healing when the event doesn't.

---

---

# **Part 3: Reacting On Screen**

Carry on in the same project. The camera reactions from Part 2 should be working.

---

### **Step 1: Check the Volume**

The provided scene has a **Global Volume** with a profile on it.

1. **Select the Global Volume** and look at its profile in the Inspector.
2. It should have **Vignette**, **Chromatic Aberration** and **Color Adjustments** overrides added, all with their values at or near zero.
3. If any are missing, **Add Override** and set them to neutral so nothing is visible at the start.

We're leaving them at zero and driving them from code.

> [!note] **Note - how you reach these from a script**
> 
> Post-processing settings don't live on a component you can grab with `GetComponent`. They're overrides inside a **profile**, which is an asset.
> 
> You get at one with `volume.profile.TryGet(out Vignette vignette)`, which hands you the override if the profile has it. Do that once in `Awake` and keep the reference 

---

### **Step 2: The Damage Flash**

1. **Create a script called `ScreenDamageEffects.cs`** and attach it to an empty GameObject called **Screen Effects**:

```csharp
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;   // Vignette, ChromaticAberration, ColorAdjustments
using PrimeTween;

// Flashes the screen when the player takes damage.
// Listens to the UI health event - nothing in the game knows this exists.
public class ScreenDamageEffects : MonoBehaviour
{
    public Volume volume;

    [Header("Damage Flash")]
    public float flashStrength = 0.6f;
    public float flashUp = 0.05f;      // fast in
    public float flashDown = 0.4f;    // slower out

    private Vignette vignette;
    private int lastHealth = -1;

    private void Awake()
    {
        if (volume != null)
        {
            // Dig the override out of the profile once and keep it
            volume.profile.TryGet(out vignette);
        }
    }

    private void OnEnable()
    {
        UIEventManager.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        UIEventManager.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int current, int max)
    {
        // The event fires for damage AND healing. Only flash when it went down.
        if (lastHealth >= 0 && current < lastHealth)
        {
            FlashDamage();
        }

        lastHealth = current;
    }

    private void FlashDamage()
    {
        if (vignette == null)
        {
            return;
        }

        // Snap up fast, fade out slower - that shape is what reads as a hit
        Sequence.Create()
            .Chain(Tween.Custom(0f, flashStrength, duration: flashUp,
                onValueChange: value => vignette.intensity.value = value))
            .Chain(Tween.Custom(flashStrength, 0f, duration: flashDown,
                onValueChange: value => vignette.intensity.value = value));
    }
}
```

2. **Drag the Global Volume** onto the **Volume** field.
3. **Set the Vignette colour to red** in the profile so the flash reads as damage.
4. **Press Play** and let an enemy hit you.

**Explanation**:

- **`lastHealth` is an important bit of logic here** `OnHealthChanged` tells you the value changed, not which way, so we remember the previous value and compare. Without it, drinking a health potion would flash the screen as if you'd been shot.
- `lastHealth` starts at -1 so the first event - the player spawning with full health - doesn't flash.
- **Fast in, slow out.** 0.05 seconds up and 0.4 down. Make both of them 0.2 and it reads as a pulse rather than an impact. 

> [!note] **Note - this is a listener deciding what a message means**
> 
> `PlayerStats` announces two numbers. This class decides that a drop means damage and damage means a red flash.
> 
> Another listener could look at the same message and decide something completely different. That's why the event stays neutral - the moment it starts saying "damage happened" rather than "health changed" it starts making decisions for everyone listening.

---

### **Step 3: Layer the Aberration**

One effect reads as flat. Two reads as a hit.

Add to the same class:

```csharp
    [Header("Aberration")]
    public float aberrationStrength = 0.6f;

    private ChromaticAberration aberration;
```

In `Awake`, next to the vignette:

```csharp
            volume.profile.TryGet(out aberration);
```

And at the end of `FlashDamage`:

```csharp
        if (aberration != null)
        {
            Sequence.Create()
                .Chain(Tween.Custom(0f, aberrationStrength, duration: flashUp,
                    onValueChange: value => aberration.intensity.value = value))
                .Chain(Tween.Custom(aberrationStrength, 0f, duration: flashDown * 1.5f,
                    onValueChange: value => aberration.intensity.value = value));
        }
```

**Press Play** and take a hit.

**Explanation**:

- Two sequences running at once, on two different values, from one method call.
- The aberration fades out slightly slower than the vignette. Effects that end at exactly the same moment feel mechanical, and a small offset is enough to fix it.
- Try turning each one off on its own to see what the other is doing. The idea is - the vignette is _you got hit_, the aberration is _it hurts_. 

> [!note] **Note - restraint, again**
> 
> Whatever you set `aberrationStrength` to first will be too high. Chromatic aberration is one of those effects that looks great in a screenshot and makes people feel sick over ten minutes.
> 
> Halve it. Then play a full round and see if you miss it. If you use it in a project keep it minimal for a key effect or find an alternative like a shader or render effect. The event principle will be the same, changing a value over time based on an event.

---

### **Step 4: Game Over Desaturation**

A slower effect, on a different event.

Add to the same class:

```csharp
    [Header("Game Over")]
    public float desaturateTime = 1.2f;

    private ColorAdjustments colorAdjustments;
```

In `Awake`:

```csharp
            volume.profile.TryGet(out colorAdjustments);
```

Subscribe in `OnEnable` and unsubscribe in `OnDisable`:

```csharp
        UIEventManager.OnGameOver += HandleGameOver;
```

```csharp
        UIEventManager.OnGameOver -= HandleGameOver;
```

And the handler:

```csharp
    private void HandleGameOver()
    {
        if (colorAdjustments == null)
        {
            return;
        }

        // Drain the colour out of the scene. -100 is fully grey.
        Tween.Custom(0f, -100f, duration: desaturateTime,
            onValueChange: value => colorAdjustments.saturation.value = value);
    }
```

**Press Play**, lose all three lives, and watch the colour drain while the game over panel sits there. *WASTED!*

**Explanation**:

- Same class, same pattern, different event. Nothing new to learn, which is the point - once you know how to subscribe and tween, every new effect is the same two steps.
- A long duration here against the short ones for damage. Slow effects read as consequences, fast ones read as impacts.

> [!note] **Note - the timings are fighting each other**
> 
> The GameManager reloads the scene after `restartDelay`, which is 3 seconds by default. The desaturation takes 1.2.
> 
> Try setting `desaturateTime` to 5 and see what happens - the effect gets cut off halfway when the scene reloads. **Effects have to fit in the window the game gives them**, and that window is set by gameplay code
> 
> Either shorten the effect or lengthen the delay. 

---

### **Step 5: Punch the Coin Counter**

UI is the perfect place to use tween animation effects

1. **Create a script called `UIPunch.cs`** and attach it to the HUD object (Its the UI GameObject):

```csharp
using UnityEngine;
using PrimeTween;

// Gives a UI element a quick scale punch when a value changes.
public class UIPunch : MonoBehaviour
{
    public Transform coinTarget;    // the coin text
    public Transform xpTarget;      // the XP text

    [Header("Punch")]
    public float punchStrength = 0.25f;   // how far PAST normal size it goes
    public float duration = 0.3f;
    public float frequency = 5f;          // how many wobbles before it settles

    private void OnEnable()
    {
        UIEventManager.OnCoinsChanged += HandleCoinsChanged;
        UIEventManager.OnXPChanged += HandleXPChanged;
    }

    private void OnDisable()
    {
        UIEventManager.OnCoinsChanged -= HandleCoinsChanged;
        UIEventManager.OnXPChanged -= HandleXPChanged;
    }

    private void HandleCoinsChanged(int value)
    {
        Punch(coinTarget);
    }

    private void HandleXPChanged(int value)
    {
        Punch(xpTarget);
    }

    private void Punch(Transform target)
    {
        if (target == null)
        {
            return;
        }

        // If a punch is already running on this element, stop it and reset,
        // otherwise fast pickups stack up and the scale drifts
        Tween.StopAll(onTarget: target);
        target.localScale = Vector3.one;

        Tween.PunchScale(target, strength: Vector3.one * punchStrength,
            duration: duration, frequency: frequency);
    }
}
```

2. **Drag the coin and XP text objects** onto the two fields.
3. **Press Play**, collect a coin and kill an enemy.

**Explanation**:

- **`strength` is an offset, not a target size.** `0.25` means it goes 25% past its normal scale at the peak, then comes back. Put `1.2` in there expecting a 1.2x punch and it'll go to nearly three times the size.
- **`frequency` is how many times it wobbles** on the way back. Low numbers give a single clean punch, high numbers buzz. Start around 5 and tune it.
- `PunchScale` returns the element to where it started on its own - you don't write the way back.
- The stop-and-reset handles fast pickups. Kill three enemies in a second and without it you'd get punches overlapping each other.
- The HUD from CRE340 is untouched. `HUDView` and `HUDPresenter` have no idea about the effects. They are totally independent.

> [!note] **Note - what a punch actually is**
> 
> `PunchScale` is one line, but just to see what it does. The long version is a sequence like the other examples but punch scale is made for this type of effect:
> 
> csharp
> 
> ```csharp
> Sequence.Create()
>     .Chain(Tween.Scale(target, endValue: Vector3.one * 1.25f,
>         duration: 0.1f, ease: Ease.OutQuad))
>     .Chain(Tween.Scale(target, endValue: Vector3.one,
>         duration: 0.2f, ease: Ease.OutBack));
> ```
> 
> That's the same shape as the damage flash in Step 2 - **fast in, slow out** - which is the pattern behind most things that need to feel like an impact.
> 
> `PunchScale` adds the wobble on the way back and saves you writing it. 

> [!note] **Note - this assumes the element sits at scale 1**
> 
> `target.localScale = Vector3.one` resets to full size, which is right for UI text and almost everything on a Canvas. UI Elements should be scaled to 1 and width/height used in the Rect Transform
> 
> If you punch something that's been scaled to anything else in the Inspector, store its starting scale in `Awake` and reset to that instead. Same thing `SpawnPop` does in Part 1.
---


---
### **What we've set up**:

- A **damage flash** on the vignette, with chromatic aberration layered over it.
- A **desaturation** on game over.
- **UI punches** on coins and XP.
- Several unrelated effects responding to the same events, none of them aware of each other.

### **Session summary**

- **Part 1** - PrimeTween, easing, and self-contained effects that animate themselves.
- **Part 2** - the camera reacting to gameplay events, with impulse for moments and noise for states.
- **Part 3** - the screen and the UI reacting, layered.

The gameplay code added **one struct and four lines** across the whole session. Everything else is listeners that could be deleted with no effect on whether the game works.

**The point of all of this is to identify the effects we want to make or the properties we want to manipulate or dynamically change. This could be shaders, lights, particles, volumes, transforms, UI or any other component. If it has a number it is controllable!  We can make various effect scripts and one step further is to drive then via game code or events. You might also so this with Unity events or animator events but the principle is the same** 

**We have:**
- *the game and mechanics*
- *The overall visual look*
- *The dynamic effect*
- *Some events to link it all together*