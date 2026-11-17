---
Order: 9-C
---
# **Dynamic and Adaptive Audio**

### Overview

#### **What we are building**:

- **Mixer snapshots** driven from the game - a duck when the player gets hit, and a filter that stays on while health is low.
- **Combat and calm music** that crossfades depending on whether anything is chasing you.
- Both of them as **event bus subscribers**, so the gameplay code doesn't know any of it exists. We're just connecting more effects - this time audio

#### **Why we are doing this**:

- Everything we've done so far plays a sound when something happens. This is audio that responds to the **state** the game is in.
- The AudioMixer is where the sound design and the code meet. You author the states in the mixer, the code asks for them by name.
- Adaptive music/audio is a big topic and we're not covering it properly. This is enough to show the idea so you can take it into your own projects.

#### **Learning Objectives**:

- **AudioMixer snapshots** and `TransitionTo`.
- **Transient and sustained effects** - something that happened versus something that is ongoing.
- **Counting events to track state**, and why a transition needs a hold time.

---

This carries on from the CRE340 game. You'll need the event bus from Week 6 and the enemy states from Week 8 so update from my package if you need the latest clean version.

There are two halves and they're both short. The first is a filter that reacts to the player's health, the second is music that reacts to whether you're being chased.

---

### **Step 1: The Messages**

Everything here is driven from the bus, so the messages come first.

**In `GameMessages.cs`**, add these alongside `EnemyKilled`:

```csharp
// The player's health changed. tookDamage lets effects tell a hit from a heal.
public struct PlayerHealthChanged
{
    public int currentHealth;
    public int maxHealth;
    public bool tookDamage;
}

// An enemy started hunting the player
public struct EnemyChaseStarted
{
    public Transform enemy;
}

// An enemy gave up
public struct EnemyChaseEnded
{
    public Transform enemy;
}
```

**Explanation**:

- `PlayerHealthChanged` carries both numbers rather than a percentage, so each subscriber decides for itself what counts as low.
- The chase messages carry the enemy's transform. We don't use it this week, but an effect that wanted to point at the enemy would need it.
- Same as always - the messages describe what happened and nothing about what should be done about it.

---

### **Step 2: Publish Them**

Three files to edit, and the player one first.

---

**1. In `PlayerStats.cs`, add a method that does the announcing:**

```csharp
    // One place that announces a health change. Called from anywhere health moves,
    // so the message only gets built once.
    private void PublishHealthChanged(bool tookDamage)
    {
        PlayerHealthChanged message = new PlayerHealthChanged();
        message.currentHealth = data.currentHealth;
        message.maxHealth = data.maxHealth;
        message.tookDamage = tookDamage;

        EventBus<PlayerHealthChanged>.Publish(message);
    }
```

**2. At the end of `TakeDamage`**, after the health has changed:

```csharp
        PublishHealthChanged(true);
```

**3. At the end of `RestoreHealth`**, after the health has changed:

```csharp
        PublishHealthChanged(false);
```

`RestoreHealth` is the method `HealthPotion.Use()` calls. Without this line the low health filter comes on and never comes off, because nothing tells it health went back up.

`currentHealth` and `maxHealth` are both on `PlayerData`, so the two lines in the message read it.

---

**4. In `EnemyState_Chase.cs`**, fill in the two empty methods:

```csharp
    public void Enter(Enemy enemy)
    {
        EnemyChaseStarted message = new EnemyChaseStarted();
        message.enemy = enemy.transform;

        EventBus<EnemyChaseStarted>.Publish(message);
    }

    public void Exit(Enemy enemy)
    {
        EnemyChaseEnded message = new EnemyChaseEnded();
        message.enemy = enemy.transform;

        EventBus<EnemyChaseEnded>.Publish(message);
    }
```

---

**5. In `Enemy.cs`**, add an `OnDisable`:

```csharp
    private void OnDisable()
    {
        // An enemy that dies mid-chase never gets Exit called, so the chase
        // would never be reported as finished. Destroying an object fires
        // OnDisable, so we can do it here.
        if (currentState != null)
        {
            currentState.Exit(this);
        }
    }
```

---

**Explanation**:

- **`PublishHealthChanged` is one method called from two places** rather than the same block pasted twice. Add a shield or damage over time later and it's one line, not five.
- **The `bool` is what lets the effects tell the two apart.** Taking a hit should duck the audio, picking up a potion shouldn't, and both of them need to update the low health state.
- **Step 5 is the one that matters.** `SetState` runs `Exit` on the way out of a state, but killing an enemy skips all of that - `Destroy` just removes the object. Without this, every enemy you kill while it's chasing you leaves a chase that never ends, and the music stays in combat for the rest of the session.
- It's a general point about state machines. `Enter` and `Exit` are only reliable if every way out of a state goes through them, and dying usually doesn't.
- No risk of `Exit` running twice. If the enemy transitions out of Chase first, `SetState` runs Chase's `Exit`, and `OnDisable` later runs whatever state it ended up in.

---

---

# **Part 1: Dynamic Effects**

---

### **Step 3: Mixer and Snapshots**

This part is Inspector work.

1. **Create an AudioMixer** - _Assets -> Create -> Audio -> Audio Mixer_. Call it **GameMixer**. - or use my mixer BMS - Mixer
2. **Open the Audio Mixer window** and select the **Master** group.
3. **Add a Lowpass effect** - the **Add..** button at the bottom of the group's strip in the Inspector, then **Lowpass Simple**.
4. **Route your audio into it.** Any AudioSource with no **Output** group set bypasses the mixer completely, so nothing will happen until this is done. Set the Output on the sources you want affected - for BGM coming from the Audio Manager, that's the track sources it uses.
5. **Make three snapshots.** The snapshot list is the top left panel of the mixer window. You start with one - rename it **Normal**, then right click to duplicate it twice for **Hit** and **LowHealth**.
6. **Set each one up.** Click a snapshot to select it, then change the values - the snapshot stores whatever the mixer looks like while it's selected.
    - **Normal** - cutoff at 22000, volume where you want it.
    - **Hit** - cutoff around 800, volume down a few dB. Should sound muffled and knocked back.
    - **LowHealth** - cutoff around 1500, a bit less extreme. This one sits there for a while so it shouldn't be unpleasant.
7. **Right click Normal and set it as the start snapshot** so the game begins in a sensible state.

> [!warning] **Note - Edit in Play Mode**
> 
> There's an **Edit in Play Mode** button at the top of the mixer window. If it's on, the mixer uses whatever you're looking at in the editor and overwrites anything your script does, so your snapshot transitions appear to do nothing at all.
> 
> Turn it off before you test. 

---

### **Step 4: The Effects Subscriber**

**Create a C# script called `DynamicAudioEffects.cs`** and put it on an empty GameObject called **Dynamic Audio**.

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// Moves the mixer between snapshots based on what is happening to the player.
// Subscribes to the bus - nothing in the game knows this exists.
public class DynamicAudioEffects : MonoBehaviour
{
    [Header("Snapshots")]
    public AudioMixerSnapshot normal;
    public AudioMixerSnapshot hit;
    public AudioMixerSnapshot lowHealth;

    [Header("Hit Duck")]
    public float duckInTime = 0.05f;      // fast on the way in so it reads as an impact
    public float duckOutTime = 0.6f;      // slower on the way out

    [Header("Low Health")]
    [Range(0f, 1f)]
    public float lowHealthFraction = 0.3f;   // below this and the filter stays on
    public float stateChangeTime = 1f;       // how long low health takes to come on or off

    private bool isLowHealth;

    private void OnEnable()
    {
        EventBus<PlayerHealthChanged>.Subscribe(HandleHealthChanged);
    }

    private void OnDisable()
    {
        EventBus<PlayerHealthChanged>.Unsubscribe(HandleHealthChanged);
    }

    private void Start()
    {
        normal.TransitionTo(0f);
    }

    private void HandleHealthChanged(PlayerHealthChanged message)
    {
        // Guard against a divide by zero if max health hasn't been set up
        if (message.maxHealth <= 0)
        {
            return;
        }

        float fraction = (float)message.currentHealth / message.maxHealth;

        isLowHealth = fraction <= lowHealthFraction && message.currentHealth > 0;

        if (message.tookDamage)
        {
            // Stop any duck already running so a second hit restarts it
            StopAllCoroutines();
            StartCoroutine(HitDuck());
        }
        else
        {
            // Healed, so just move to whichever state we should be resting in
            GoToRestingSnapshot(stateChangeTime);
        }
    }

    // Drop into the hit snapshot, then come back to wherever we should be
    private IEnumerator HitDuck()
    {
        hit.TransitionTo(duckInTime);

        yield return new WaitForSeconds(duckInTime);

        GoToRestingSnapshot(duckOutTime);
    }

    // Low health and normal are the two states we sit in. The hit is temporary.
    private void GoToRestingSnapshot(float time)
    {
        if (isLowHealth)
        {
            lowHealth.TransitionTo(time);
        }
        else
        {
            normal.TransitionTo(time);
        }
    }
}
```

**Drag the three snapshots** from the mixer's snapshot list into the three fields.

**Explanation**:

- **`TransitionTo(time)`** moves the whole mixer to that snapshot over that many seconds, interpolating every value in it. One call handles the cutoff, the volume, and anything else you put in the snapshot.
- **Two kinds of effect in one script.** The hit is **transient** - it happens, it recovers. Low health is **sustained** - it stays until something changes it. `GoToRestingSnapshot` is what keeps them from fighting each other: after a hit we go back to whichever state we should actually be in, not blindly to normal.
- **The script has no mixer reference.** Snapshots are assets and they know which mixer they belong to.
- **Fast in, slow out.** 0.05 seconds in reads as an impact. Half a second would read as a fade, which is a different thing entirely.

> [!note] **Note - the mixer as a handover point**
> 
> Everything about how this sounds is in the mixer. The cutoff frequencies, the volumes, how extreme each state is. The script only names snapshots and says how long to take.
> 
> That means whoever is doing the sound can retune the whole feel of it without anyone touching code, and without needing to know what an event bus is. It's worth being deliberate about this in your own projects - decide what belongs in the asset and what belongs in the script, and keep them seperate

---

### **Step 5: Test It**

1. **Press Play** and let an enemy hit you. Everything should duck and muffle briefly, then come back.
2. **Take hits until you're low.** The lowpass should come on and stay on.
3. **Pick up a health item.** It should come back off.
4. **Watch the mixer window while you play.** The snapshot transitions are visible on the sliders.

If nothing happens at all, check the Output group is set on your audio sources and that Edit in Play Mode is off.

---

---

# **Part 2: Adaptive Music**

Adaptive music is a whole subject - layers that come in and out, transitions timed to the bar, stingers over the top. We're doing the simplest version of it: two tracks and a crossfade.

---

### **Step 6: Tracking Combat**

The issue is what tells us we're in combat? Enemies being alive isn't it - in an area there are always enemies alive. **Being chased** is the thing we can react to, and we have messages for it.

**1. Use three tracks** in the Audio Manager. 

This is my premade setup. Not documented but already setup (its a manager that uses scriptable objects for audio properties. Its a track based audio system so when you call something it automatically fades or transitions from the previous.

- You should have a **SoundDefinition** for each of the three intensities - calm, building, and the full one. Were using audio tracks called *Ambient Vol4 Timelapse*
- On **all three**, set **Fade Type** to **Crossfade** and give them a **Fade Duration** of a couple of seconds.
- They are all on the **BGM** track type.

The transition style is on the asset, so playing a track is all the code has to do. The BGM channel crossfades whatever is already on it, so we never have to stop anything first.

**2. Take the music out of the `GameManager`.** In `GameManager.Start()`, comment this line if you have it:

```csharp
        //todo could start musis here 
        AudioEvent.PlayTrack(SoundId.Ambient_Vol4_Timelapse_Main,  0.3f);
```

The music is about to be driven by what's happening in the game, so it belongs with the thing that decides that. Two scripts both starting tracks is how you end up with music that fights against itself.

**3. Create a C# script called `MusicIntensity.cs`** and put it on an empty GameObject called **Music Intensity**.

```csharp
using UnityEngine;

// Picks the music based on how many enemies are hunting the player.
public class MusicIntensity : MonoBehaviour
{
    [Header("Tracks - calm first, most intense last")]
    [SerializeField] private SoundId[] intensityTracks;

    [Header("Settings")]
    public float musicVolume = 0.3f;
    public float dropHoldTime = 5f;    // how long to wait before stepping the music back down

    private int chasingCount;          // how many enemies are hunting us right now
    private int currentLevel = -1;     // -1 so the first level change always happens
    private float timeSinceDrop;

    private void OnEnable()
    {
        EventBus<EnemyChaseStarted>.Subscribe(HandleChaseStarted);
        EventBus<EnemyChaseEnded>.Subscribe(HandleChaseEnded);
    }

    private void OnDisable()
    {
        EventBus<EnemyChaseStarted>.Unsubscribe(HandleChaseStarted);
        EventBus<EnemyChaseEnded>.Unsubscribe(HandleChaseEnded);
    }

    private void Start()
    {
        // The music starts here rather than in the GameManager, so there is one
        // place that decides what is playing.
        SetLevel(0);
    }

    private void HandleChaseStarted(EnemyChaseStarted message)
    {
        chasingCount = chasingCount + 1;
    }

    private void HandleChaseEnded(EnemyChaseEnded message)
    {
        chasingCount = chasingCount - 1;

        // Never let it go negative, whatever happens
        if (chasingCount < 0)
        {
            chasingCount = 0;
        }
    }

    private void Update()
    {
        int target = GetTargetLevel();

        // Going up happens straight away - things just got worse and the music should say so
        if (target > currentLevel)
        {
            timeSinceDrop = 0f;
            SetLevel(target);
            return;
        }

        // Going down waits, so a short lull doesn't drop the music and bring it straight back
        if (target < currentLevel)
        {
            timeSinceDrop = timeSinceDrop + Time.deltaTime;

            if (timeSinceDrop >= dropHoldTime)
            {
                timeSinceDrop = 0f;

                // Step down one level at a time rather than dropping all the way at once
                SetLevel(currentLevel - 1);
            }
        }
        else
        {
            timeSinceDrop = 0f;
        }
    }

    // One enemy chasing is level 1, two or more is level 2, and so on up to
    // however many tracks we have.
    private int GetTargetLevel()
    {
        int level = chasingCount;

        if (level >= intensityTracks.Length)
        {
            level = intensityTracks.Length - 1;
        }

        return level;
    }

    private void SetLevel(int level)
    {
        if (intensityTracks.Length == 0)
        {
            return;
        }

        currentLevel = level;

        AudioEvent.PlayTrack(intensityTracks[level], musicVolume);

        Debug.Log("Music level " + level);
    }
}
```

**4. Set the array size to 3** in the Inspector and pick the tracks from the dropdowns, calmest first:

|Element|Track|Plays when|
|---|---|---|
|0|Ambient Vol4 Timelapse Intensity 1|Nothing chasing you|
|1|Ambient Vol4 Timelapse Intensity 2|One enemy chasing|
|2|Ambient Vol4 Timelapse Main|Two or more chasing|

**Explanation**:

- **The events tell us about changes, not about the state.** A chase starting and a chase ending are two separate facts, and the thing we actually want - how many things are chasing - has to be worked out by counting them. That's a common shape when you're working from events.
- **The count is the level.** No thresholds to tune, and adding a fourth track is a change to the array size and nothing else. Clamping to `Length - 1` means any number of chasers above the top track just stays on the top track.
- **Up is immediate, down waits.** Those are deliberately different. Something becoming dangerous should be heard right away, and something calming down shouldn't be heard until you're sure. A complex adaptive system does the same.
- **`dropHoldTime` is the important part.** Without it, one enemy losing you for half a second drops the music a level and brings it straight back, and you get a crossfade every couple of seconds. Set it to 0 and listen to what happens - it's worth hearing.
- **Stepping down one level at a time** means going from the full track back to calm passes through the middle one rather than jumping. It sounds a lot less abrupt.
- **`currentLevel` starts at -1** so the first call to `SetLevel(0)` in `Start` actually runs. If it started at 0 the opening track would never play.
- **One line does the transition.** The crossfade and its length are on the SoundDefinition, so `PlayTrack` is the whole thing.

---

### **Step 7: Test It**

1. **Press Play** and stay away from the enemies. Intensity 1 should be playing, and the Console says `Music level 0`.
2. **Walk into one enemy's chase range.** It crosses up to level 1.
3. **Pull a second enemy in.** Level 2, the full track.
4. **Run away from everything.** It steps down a level at a time, waiting the hold time between each.
5. **Kill an enemy while it's chasing you** and make sure the music still comes back down. That's the `OnDisable` fix from Step 2 doing its job - take it out and try again if you want to hear the bug.
6. **Set `dropHoldTime` to 0** and run past a few enemies. This is the thing the hold time is there for.
7. **Watch `Chasing Count` while you play** if you want to see it working - temporarily make the field public and it shows in the Inspector.


---

### **What a real adaptive system adds**

What we've built switches between two finished tracks. For info about what else is normally involved. This is the part that's out of scope here and potentially in scope for your own projects:

- **Vertical layering** - one piece of music written as separate stems - drums, bass, lead, tension - faded in and out rather than swapping whole tracks. Transitions are seamless because it's the same piece throughout.
- **Horizontal re-sequencing** - the music is written in blocks and the game picks which block plays next, so it can only change at musically sensible points.
- **Transitions on the beat** - waiting until the end of the current bar before switching, rather than cutting whenever the game feels like it. Music transitions are in time with music rhythm
- **Stingers** - a short phrase played over the top at the moment something happens, covering the join.

Our version will cross in the middle of a bar and sound like what it is. That's fine for showing the idea and using ambient type music can cover transitions. If you want to go further, we can look at **FMOD** or **Wwise** - both are free for student projects and both are built around this concept. Steep learning curve and generally they are for specialised game sound designer to work in.

---

### **What we've set up**:

- **Three new messages** on the bus - health changes and chases starting and stopping.
- A **mixer with three snapshots**, and a subscriber moving between them.
- A **transient effect** on being hit and a **sustained one** while health is low.
- **Combat and calm music** crossfading off the chase count, with a hold time so it doesn't flicker.
- No gameplay script that knows any of this is happening.
