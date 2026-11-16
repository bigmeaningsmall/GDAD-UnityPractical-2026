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