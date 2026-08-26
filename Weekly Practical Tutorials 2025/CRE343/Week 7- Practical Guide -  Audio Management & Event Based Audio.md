# BMS-AudioManager 

---
### How to Call Sound Effects in the Event-Driven Audio System

This audio system allows you to play sound effects by raising an event, which the `AudioManager` listens for and handles. 

Sounds are stored in `Resources/Audio/SFX`, and you can play them with specific settings by calling the event.
#### Main Example: How to Call the Event

To play a sound effect, use this line of code to call an effect by name with a volume parameter:

```csharp
AudioEventManager.PlaySFX("Impact Generic", 1.0f);
```

This line triggers the `PlaySFX` event in `AudioEvent`. 

Here's what each parameter does:
- **`new string[] { "Impact Generic" }`** (soundNames): Array of sound file names from `Resources/Audio/SFX`. If multiple sounds are provided, one will be randomly selected.
- **`1.0f`** (volume): The volume level for playback (1.0 is full volume).
- **`1.0f`** (pitch): Sets the playback pitch (1.0 is normal pitch).
- **`true`** (randomizePitch): Adds a slight variation to the pitch to make the sound more realistic.
- **`0.1f`** (pitchRange): The range of pitch randomization if `randomizePitch` is enabled.
- **`0f`** (spatialBlend): Controls 2D/3D audio. Setting this to `0f` makes it a 2D sound (no spatialization), while `1f` would make it fully 3D.
- **`false`** (loop): Whether the sound should loop continuously.
- **`0f`** (delay): How many seconds to wait before playing the sound.
- **`100f`** (percentChanceToPlay): Probability the sound will play (0-100%). Useful for random ambient sounds.
- **`null`** (attachTo): Transform to attach the sound to. If null, uses AudioManager's position.
- **`Vector3.zero`** (position): Custom world position for the sound (only used if attachTo is null).
- **`1f`** (minDistance): For 3D audio, the distance where volume starts to decrease.
- **`500f`** (maxDistance): For 3D audio, the maximum distance the sound can be heard.
- **`""`** (eventName): Optional identifier for debugging or event tracking.

### Detailed Example
```csharp
AudioEventManager.PlaySFX(
    new string[] { "Impact Generic" },  // soundNames: Sound file(s) from Resources/Audio/SFX
    1.0f,                              // volume: Full volume
    1.0f,                              // pitch: Normal pitch  
    true,                              // randomizePitch: Add slight pitch variation
    0.1f,                              // pitchRange: Range for pitch randomization
    0f,                                // spatialBlend: 0 = 2D sound, 1 = 3D sound
    false,                             // loop: Don't loop the sound
    0f,                                // delay: Play immediately
    100f,                              // percentChanceToPlay: Always play (100%)
    null,                              // attachTo: No transform, uses AudioManager position
    Vector3.zero,                      // position: Custom world position (ignored if attachTo is used)
    1f,                                // minDistance: 3D audio minimum distance
    500f,                              // maxDistance: 3D audio maximum distance  
    ""                                 // eventName: Optional identifier
);
```

### How It Works Behind the Scenes
1. **Raising the Event**:
   - When `AudioEventManager.PlaySFX(...)` is called, the `PlaySFX` event is raised with all the provided parameters.

2. **AudioManager Handling the Event**:
   - `AudioManager` listens for this event and executes its `PlaySoundEffect` method, which:
      - **Random Selection**: If multiple sound names are provided, randomly selects one.
      - **Chance Check**: Rolls against the `percentChanceToPlay` - if it fails, the sound doesn't play.
      - **Clip Retrieval**: Gets the sound clip from `Resources/Audio/SFX`.
      - **Positioning**: Creates an AudioSource at the specified position (attachTo transform, custom position, or AudioManager's position).
      - **Configuration**: Sets up volume, pitch (with randomization if enabled), spatial blend, loop settings, and 3D audio distances.
      - **Playback**: Plays the sound with optional delay and automatically destroys non-looped sounds when complete.
      - **Type Tagging**: Tags the AudioSource with `AudioType.SFX` for easy management (pause all SFX, stop all SFX, etc.).

3. **Advanced Features**:
   - **Global SFX Control**: All SFX can be paused, stopped, or have their volume attenuated globally through AudioManager.
   - **Memory Management**: Non-looped sounds auto-destroy when finished. Looped sounds persist until manually stopped.
   - **3D Audio**: Supports full 3D positioning with customizable distance curves for realistic spatial audio.

This extended parameter system provides comprehensive control over sound effects while maintaining the simplicity of a single method call.


---

# Step 1 : Audio to add to prefabs 

- ### Drag audio SFX Clip from project folder to Prefab
- ### Assign the mixer output
- ### Set 'Play On Awake'

**Enemy**
- Digital Riser

**Exploding crate**
- Digital Riser or preferred sound

**PF_Bullet**
- Pistol Shot of other preferred gun sound

**Player**
- Fast Swish of preferred sound

**PF_BonusCollectable, HealthCollectable, WeaponCollectable**
- Alert 1 or 2

****



# Audio SFX Events to add to game scripts

## Player.cs

```c#
//TODO - add and audio feedback when the player dies  
AudioEvent.PlaySFX("Debuff", 1.0f);
```

```c#
//TODO - add an audio feedback when the player is hit  
AudioEvent.PlaySFX("Debuff", 0.7f, true); // with random pitch
```

## Enemy.cs

```c#
//TODO - add and audio feedback when the enemy is hit  
AudioEvent.PlaySFX("Flesh Hit", 1.0f, true); // with random pitch
```

```c#
//TODO - add and audio feedback when the enemy dies  
AudioEvent.PlaySFX("Explosion Flesh", 1.0f, true); // with random pitch
```

## ExplodingCrate.cs
```c#
//TODO - add and audio feedback when the crate explodes 
AudioEvent.PlaySFX("Explosion Short", 1.0f, true); // with random pitch
```


## Bullet.cs
```c#
//TODO - add and audio feedback when hitting an object
AudioEvent.PlaySFX("Slap Heavy", 1.0f, true); // with random pitch
```

## CollectableItem.cs
```c#
//TODO - add and audio feedback when hitting an object
AudioEvent.PlaySFX("Special Powerup", 1.0f, true); // with random pitch
```

## UI_Display.cs
```c#
//TODO - add and audio feedback when hitting an object
AudioEvent.PlaySFX("UI Beep", 1.0f, true); // with random pitch
```