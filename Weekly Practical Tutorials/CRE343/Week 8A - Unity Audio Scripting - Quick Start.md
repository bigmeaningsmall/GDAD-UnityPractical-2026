# **Playing Audio from Script - Quick Start**

A short guide to using an `AudioSource` from code. Not a full tutorial  - just to get sound firing from script and to see what the properties do while the game is running.

---

### **Clip and Source**

Two different things:

- An **AudioClip** is the file. The `wav` or `ogg` sitting in your project.
- An **AudioSource** is the speaker. A component on a GameObject that plays clips, at a position, with a volume and a pitch.

One clip can be played by any number of sources. A source can only play one clip at a time through `Play`, though it can fire extra clips over the top with `PlayOneShot`.

---

### **Scene Setup**

Use the tutorial/game scene or make a new empty scene

1. **Create an empty GameObject** called **Audio Demo** and drop it somewhere you can see it.
2. **Add Component -> Audio Source.**
3. **Drag a clip** into the Audio Source's **AudioClip** field.
	1. Use the clips from the folder **Audio**
4. **Untick Play On Awake.** It's on by default, so the sound fires the moment you press Play - Useful if you are attaching audio to a GameObject rather than scripting it
5. **Add the `AudioDemo` script** to the same object.
6. **Drag two or three more clips** into the script's **Clips** array. Keys 2 and 9 pick from these at random.

Press Play and keep an eye on the Console - every key logs what it did.

Note: You'll have to try different clips from one shots to loops to get the different functions.

---

### **The Keys**

| Key | What it does            | Worth noticing                                                               |
| --- | ----------------------- | ---------------------------------------------------------------------------- |
| 1   | `Play()`                | Press once then Press it twice quickly - it restarts rather than overlapping |
| 2   | `PlayOneShot()`         | Press at will. These stack on top of each other                              |
| 3   | `Stop()`                | Back to the beginning                                                        |
| 4   | `Pause()` / `UnPause()` | Keeps its place, unlike Stop                                                 |
| 5   | Random pitch, then play | The cheapest way to stop a repeated sound sounding fake (Pitch Variation)    |
| 6   | Volume down a quarter   | Wraps back to full                                                           |
| 7   | Loop on / off           | Takes effect next time it plays                                              |
| 8   | 2D / 3D toggle          | Walk away/Move the Audio Listener away from it in each mode                  |
| 9   | `PlayClipAtPoint()`     | Drops a sound out in the world with no source of ours                        |
| R   | Reset                   | Volume, pitch and loop back to where they started                            |

---

### **The Script**

```csharp
using UnityEngine;

// Live demo for playing and changing an AudioSource from script.
// Put this on a GameObject that also has an AudioSource.
// Press 1-9 while the game is running. R puts everything back.
public class AudioDemo : MonoBehaviour
{
    [Header("Source")]
    public AudioSource source;          // the speaker - drag it in, or leave empty and it finds one

    [Header("Clips")]
    public AudioClip[] clips;           // extra clips for the one shots and the random picks

    [Header("Pitch Variation")]
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    [Header("Drop a Sound")]
    public float dropRange = 10f;       // how far away key 9 drops a sound

    private float startVolume;
    private float startPitch;

    private void Awake()
    {
        if (source == null)
        {
            source = GetComponent<AudioSource>();
        }

        // Remember where we started so R can get back to it
        startVolume = source.volume;
        startPitch = source.pitch;
    }

    private void Update()
    {
        // 1 - Play. Restarts the source from the beginning every time.
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            source.Play();

            Debug.Log("1 - Play");
        }

        // 2 - PlayOneShot. Fires a clip THROUGH the source without taking it over, so these stack up.
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AudioClip clip = PickRandomClip();

            if (clip != null)
            {
                source.PlayOneShot(clip);

                Debug.Log("2 - PlayOneShot : " + clip.name);
            }
        }

        // 3 - Stop. Back to the start, nothing playing.
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            source.Stop();

            Debug.Log("3 - Stop");
        }

        // 4 - Pause and UnPause. Unlike Stop, this keeps its place in the clip.
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (source.isPlaying)
            {
                source.Pause();

                Debug.Log("4 - Pause");
            }
            else
            {
                source.UnPause();

                Debug.Log("4 - UnPause");
            }
        }

        // 5 - Random pitch, then play. Stops a repeated sound feeling mechanical.
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            source.pitch = Random.Range(minPitch, maxPitch);
            source.Play();

            Debug.Log("5 - Pitch " + source.pitch.ToString("0.00"));
        }

        // 6 - Step the volume down a quarter at a time, wrapping back to full.
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            source.volume = source.volume - 0.25f;

            if (source.volume <= 0f)
            {
                source.volume = 1f;
            }

            Debug.Log("6 - Volume " + source.volume.ToString("0.00"));
        }

        // 7 - Loop on and off. Takes effect the next time it plays.
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            source.loop = !source.loop;

            Debug.Log("7 - Loop " + source.loop);
        }

        // 8 - 2D or 3D. 0 is the same volume everywhere, 1 is positioned in the world.
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (source.spatialBlend < 0.5f)
            {
                source.spatialBlend = 1f;

                Debug.Log("8 - 3D (positioned)");
            }
            else
            {
                source.spatialBlend = 0f;

                Debug.Log("8 - 2D (everywhere)");
            }
        }

        // 9 - Drop a one-off sound out in the world without using our own source at all.
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            AudioClip clip = PickRandomClip();

            if (clip != null)
            {
                Vector3 spot = transform.position + new Vector3(
                    Random.Range(-dropRange, dropRange),
                    0f,
                    Random.Range(-dropRange, dropRange)
                );

                AudioSource.PlayClipAtPoint(clip, spot);

                Debug.Log("9 - PlayClipAtPoint at " + spot);
            }
        }

        // R - back to how it started
        if (Input.GetKeyDown(KeyCode.R))
        {
            source.Stop();

            source.volume = startVolume;
            source.pitch = startPitch;
            source.loop = false;

            Debug.Log("R - Reset");
        }
    }

    // Grab a random clip from the array, or fall back to whatever is already on the source
    private AudioClip PickRandomClip()
    {
        if (clips.Length == 0)
        {
            return source.clip;
        }

        int index = Random.Range(0, clips.Length);

        return clips[index];
    }
}
```

---

### **Properties Worth Knowing**

Everything in the Audio Source Inspector can be set from script, with the same name in lowercase camel case. These are the ones you'll probably use:

|Property|Range|What it's for|
|---|---|---|
|`volume`|0 to 1|Level of this source|
|`pitch`|-3 to 3|Speed and pitch together. Negative plays backwards|
|`loop`|true / false|Repeat when it reaches the end|
|`spatialBlend`|0 to 1|0 is 2D, 1 is fully positioned in 3D|
|`minDistance`|metres|Inside this, it's at full volume|
|`maxDistance`|metres|Outside this, you hear nothing|
|`clip`|AudioClip|Swap what the source will play next|
|`time`|seconds|Where it is in the clip. Writable, so you can scrub|
|`isPlaying`|read only|Useful for not restarting something already going|

---

### **Three Things That Catch People Out**

**Play On Awake is ticked by default.** Your sound fires as the scene loads and you end up debugging something when its autoplaying

**`spatialBlend` starts at 0.** A brand new Audio Source is 2D, so all the 3D Sound Settings - rolloff, min and max distance, spread - do nothing at all until you raise it. This is the single most common reason 3D audio "doesn't work".

**`Play` interrupts, `PlayOneShot` doesn't.** If a footstep cuts off the last footstep, you're using `Play`. One source can only hold one clip through `Play`, but `PlayOneShot` fires clips over the top and lets them finish. Volume and pitch on the source still apply to them.

---

### **Where This Goes Next**

`AudioSource.PlayClipAtPoint` on key 9 is handy and also a bit of a cheat - it creates a GameObject with an AudioSource on it, plays the clip, then destroys itself. Fine for a demo, less fine when you're firing fifty of them a second.

That's one of the reasons to write an **AudioManager** rather than scattering audio calls through your game: one place that has a set of sources, hands them out, and decides what gets heard, when and how. It depends on what you need to do with audio when you'd need to go to a bigger manager or when you'd need to go to a middleware.