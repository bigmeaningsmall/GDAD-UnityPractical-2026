using System.Collections;              // for coroutines
using UnityEngine;
using UnityEngine.SceneManagement;     // for reloading the scene

public class GameManager : MonoBehaviour
{
    #region Singleton

    // The one and only GameManager.
    // 'private' so nothing outside this class can overwrite it.
    // 'static' means it belongs to the CLASS, not to any one object - there is only ever one of these.
    private static GameManager instance;

    // The public way in. Any script can use GameManager.Instance
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Nothing has set it yet, so look for one in the scene
                instance = FindFirstObjectByType<GameManager>();

                if (instance == null)
                {
                    Debug.LogError("There is no GameManager in the scene. Add one.");
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        // If there is already a GameManager then this one is a duplicate, so remove it
        if (instance != null && instance != this)
        {
            Debug.LogWarning("A second GameManager was found. Destroying it.");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    #endregion

    #region Player Spawning

    [Header("Player")]
    public GameObject playerPrefab;        // drag the Player prefab here
    public Transform playerSpawnPoint;     // drag the Player Spawn Point object here
    public float respawnDelay = 2f;        // seconds before the player comes back
    
    [Header("Lives")]
    [SerializeField] private int lives = 3;

    public int Lives
    {
        get { return lives; }
    }

    private void OnEnable()
    {
        EventBus<EnemyKilled>.Subscribe(HandleEnemyKilled);
    }

    private void OnDisable()
    {
        EventBus<EnemyKilled>.Unsubscribe(HandleEnemyKilled);
    }

    private void HandleEnemyKilled(EnemyKilled message)
    {
        AddXP(message.xpValue);
    }
    
    private void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null || playerSpawnPoint == null)
        {
            Debug.LogError("GameManager is missing the player prefab or the spawn point.");
            return;
        }

        Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);

        // We don't need to do anything with the new player here -
        // PlayerStats registers itself with us in its own Awake.
    }
    
    // Called by PlayerStats when the player's health hits zero
    public void PlayerDied()
    {
        lives = lives - 1;

        UIEventManager.LivesChanged(lives);

        if (lives > 0)
        {
            StartCoroutine(RespawnAfterDelay());
        }
        else
        {
            GameOver();
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        SpawnPlayer();
    }

    #endregion
    
    #region Game State

    [Header("Game State")]
    [SerializeField] private bool isGameOver = false;
    public float restartDelay = 3f;

    public bool IsGameOver
    {
        get { return isGameOver; }
    }

    public void GameOver()
    {
        isGameOver = true;

        UIEventManager.GameOver();

        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);

        // Reload the scene we're currently in. Everything resets - enemies, items, etc...
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion
    
    #region Progression

    [Header("Progression")]
    [SerializeField] private int coins = 0;
    [SerializeField] private int xp = 0;

    // Other scripts can READ these but not change them.
    // The only way to change them is through the two methods below.
    public int Coins
    {
        get { return coins; }
    }

    public int XP
    {
        get { return xp; }
    }

    public void AddCoins(int amount)
    {
        coins = coins + amount;

        // Announce it. No idea what's listening and we don't have to know
        UIEventManager.CoinsChanged(coins);
    }

    public void AddXP(int amount)
    {
        xp = xp + amount;

        //same thing we just announce the value has changed
        UIEventManager.XPChanged(xp);
    }

    #endregion

    #region Player Reference

    // The player registers itself here so anything that needs it can ask us
    private PlayerStats player;

    public PlayerStats Player
    {
        get { return player; }
    }

    public void RegisterPlayer(PlayerStats newPlayer)
    {
        player = newPlayer;
    }

    #endregion
}