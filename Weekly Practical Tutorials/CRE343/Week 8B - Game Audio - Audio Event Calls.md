
# BMS AudioManager - Adding Audio Events to the Game

---

### How to Play a Sound

You don't add AudioSources to anything. You raise an audio **event** from your script, and the `AudioManager` listens for it and plays the sound for you.

```csharp
AudioEvent.PlaySFX(SoundId.Pistol_Shot);
```

- **`SoundId`** is a list of every sound in the project. Type `SoundId.` and autocomplete shows them all, so there are no names to misspell.
- **Volume, pitch and random pitch** are set on the sound itself, not in the code. Each sound has a **SoundDefinition** asset in the `SoundDefinitions` folder. Tick **Randomize Pitch** there and every call to that sound gets variation.

These are the only three versions you need:

```csharp
AudioEvent.PlaySFX(SoundId.UI_Beep);                              // plays the sound (2D)
AudioEvent.PlaySFX(SoundId.Debuff, 0.7f);                         // with a volume (0-1)
AudioEvent.PlaySFX(SoundId.Explosion_Short, transform.position);  // in the world at this spot (3D)
```

> **Rule:** if the object is about to be **destroyed** (it dies, explodes or gets picked up), pass `transform.position`, not `transform`. Passing `transform` attaches the sound to the object, so `Destroy()` cuts it off.

### Before You Start

- The **BMS - AudioManager** prefab is in the scene.
- **MasterBank** is in the AudioManager's **Startup Banks** list.

If a sound doesn't play, the Console should tell you why

### How It Works Behind the Scenes

1. `AudioEvent.PlaySFX(...)` raises an event. Your script doesn't know whats listening.
2. The `AudioManager` hears it and looks up the sound's definition.
3. It creates an AudioSource, plays the clip with the definition's settings, and destroys the AudioSource when it's finished.

---

# Audio Events to Add to the Game Scripts

Each call replaces the TODO comment already in the script. 

You can add audio events where you like or think they should be - This is just my first pass as adding audio hooks in the script

## Shoot.cs - `Fire()`

```csharp
  
//TODO - We'll make the sound event here - could use the existing event system but we'll use an audiomanager event setup  
AudioEvent.PlaySFX(SoundId.Pistol_Shot);  // dont foget to find the soundid and set its pitch variation
//or
//AudioEvent.PlaySFX(SoundId.Sci_Fi_Shotgun, 0.8f);  // have to try different sounds and settings
//or  
//AudioEvent.PlaySFX(SoundId.SoundDef_GunShots);
```

> Try `SoundId.SoundDef_GunShots` too. It's one sound with several gunshots grouped inside, and a random one plays each time. You have to go and set pitch variation.

## TwinStickController.cs - dash

```csharp
//TODO test - play dash sound effect here
AudioEvent.PlaySFX(SoundId.Fast_Swish);
```

## PlayerStats.cs

**These are bad sounds but you can pick better ones...**

```csharp
// RestoreHealth()
//todo - restore sound call
AudioEvent.PlaySFX(SoundId.Health_Pickup_1);
```

```csharp
// RestoreMana()
//todo - restore sound call
AudioEvent.PlaySFX(SoundId.SFX_powerUp8);
```

```csharp
// TakeDamage()
//todo - damage sound call
AudioEvent.PlaySFX(SoundId.Debuff, 0.7f);
```

```csharp
// Die() - the player is destroyed straight after, so play it at a position
//todo dead player sound
AudioEvent.PlaySFX(SoundId.Explosion_Flesh, transform.position);
```

## Enemy.cs

```csharp
// Start()
// todo - call sound for enemy spawn
AudioEvent.PlaySFX(SoundId.Spawn_3, transform.position);
```

```csharp
// TakeDamage() - put this at the TOP, before the health check, so it also plays on the killing hit
AudioEvent.PlaySFX(SoundId.Flesh_Hit, transform.position);
```

```csharp
// Die()
//todo call the death sound here
AudioEvent.PlaySFX(SoundId.Explosion_Short, transform.position);
```

## Bullet.cs - `OnCollisionEnter()`

```csharp
//todo sound - inside the IDamagable check
AudioEvent.PlaySFX(SoundId.Slap_Heavy, transform.position);
```

> This plays the same impact for everything the bullet hits. Each object's own `TakeDamage()` adds its specific sound on top (flesh for enemies, an explosion for the crate).

## Coin.cs - `Use()`

```csharp
AudioEvent.PlaySFX(SoundId.SFX_collect_bonus1);
```

---

### Extra - Music

Tracks (music, ambience, dialogue) work the same way, using `PlayTrack`. Add this to `GameManager.Start()`:

```csharp
AudioEvent.PlayTrack(SoundId.Ambient_Vol4_Timelapse_Main,  0.3f); // volume can be set in the event
```
