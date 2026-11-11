using UnityEngine;

// Live demo for playing and changing an AudioSource from script.
// Put this on a GameObject that also has an AudioSource.
// Press 1-9 while the game is running. R resests things
public class AudioDemo : MonoBehaviour
{
    [Header("Source")]
    public AudioSource source;          // the speaker - drag it in, or leave empty and it finds it

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