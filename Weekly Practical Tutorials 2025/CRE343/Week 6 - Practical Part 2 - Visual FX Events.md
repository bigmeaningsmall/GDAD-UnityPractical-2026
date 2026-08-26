
---
### FX_EventManager: Triggering Camera Shake and Chromatic Aberration

`FX_EventManager` is a centralised system for triggering visual effects, allowing us to add reactions (like camera shake or chromatic aberration) with simple calls. 

Instead of accessing each effect script directly, we use events in `FX_EventManager` to handle these effects in one place. 

**Note : a `CameraShaker` script has been added to the `Cinemachine Camera` game object  and a `ChromaticAbberationEffect` script has been added to the `Global Volume` game object**
	- Our FX just calls these scripts by sending values in the events

Here’s how each call works, with example C# code for clarity:

1. **Camera Shake Effect**
   - **Example Call**:
     ```csharp
     // Add a camera shake effect when the player is hit
     FX_EventManager.ShakeCamera(10f, 4f, 1f);
     ```
   - **Purpose**: This call activates a camera shake effect, for example, when the player takes damage.
   - **Parameters**:
     - `10f` - **Frequency**: Controls the intensity of the shake (higher values mean faster shake).
     - `4f` - **Amplitude**: Controls the magnitude of the shake (higher values mean a more pronounced shake).
     - `1f` - **Duration**: Specifies how long the shake effect lasts in seconds.
   
   `ShakeCamera` triggers the camera shake script, which uses these values to adjust the noise settings of the `Cinemachine` camera, creating a quick, intense visual feedback.

2. **Chromatic Aberration Effect**
   - **Example Call**:
     ```csharp
     // Add a chromatic aberration lerp effect when the player is hit
     FX_EventManager.ChromaticAberrationLerp(1f, 1.0f);
     ```
   - **Purpose**: This call activates a chromatic aberration effect, creating a brief, distorted visual effect.
   - **Parameters**:
     - `1f` - **Intensity**: Sets the strength of the chromatic aberration (higher values mean a stronger distortion).
     - `1.0f` - **Duration**: Defines how long the effect lasts, gradually reaching the target intensity and returning to its default value within this time.

   `ChromaticAberrationLerp` triggers the aberration effect script on a `Volume` component, using these parameters to create a smooth, pulsing effect that highlights key gameplay moments.

### Summary
Using `FeedbackEventManager`, we trigger visual effects like camera shake and chromatic aberration with single calls, making it easy to add impactful feedback across the game. 

The manager handles the parameters and passes them to the appropriate effect scripts, which adjust frequency, amplitude, intensity, and duration to produce smooth, reactive feedback.


---

# Visual FX Events to add to game scripts

**Notice in the Test Scripts an `TestFX` class has been added to experiment with calling FX events**

## Player.cs

```c#
//TODO - add a camera shake effect when the player is hit  
FX_EventManager.ShakeCamera(6f,4f,1f );
```

```c#
//TODO - add a chromatic aberation lerp effect when the player is hit  
FX_EventManager.ChromaticAberrationLerp(1f, 1.0f);
```

## Shoot.cs
```c#
//TODO - add a camera shake effect when shooting  
FX_EventManager.ShakeCamera(5f, 1f, 0.25f);
```


---

# Other Visual FX and Feel to Add

## 1: Add a trail effect to the bullet

## 2: Experiment with the `CinemachineCamera` components
