using UnityEngine;
using TMPro; // to use TextMeshPro we include the library / namespace

// in this example mini game, GameManager handles score, timer and the UI

public class GameManager_Collectathon : MonoBehaviour
{
    // this makes a static instance of this class publicly available to any other class (public, static, ClassName/Type (self), referenceName)
    public static GameManager_Collectathon instance;

    // text / UI references
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject winText;
    public GameObject gameOverText;

    // logic variables
    private int score = 0;
    private int totalCollectables;
    public float timer = 20f; // 60 = 1 min

    void Awake()
    {
        instance = this; // before the game starts, set this class as the instance (make it globally available to interact with)
    }

    void Start()
    {
        // find all the spawned collectables - the spawner uses 'Awake' to create them, and 'Awake' runs before 'Start'
        totalCollectables = FindObjectsByType<Collectable>(FindObjectsSortMode.None).Length;

        UpdateScoreText(); // set the initial score display
        // set up the UI - hide the end game text
        winText.SetActive(false);
        gameOverText.SetActive(false);
    }

    void Update()
    {
        timer -= Time.deltaTime; // count the timer down (minus equals Time.deltaTime)
        UpdateTimerText();

        // timer logic - do something when the countdown finishes
        if (timer <= 0)
        {
            GameOver();
        }
        if (score >= totalCollectables)
        {
            Win();
        }
    }

    // public function for increasing score and updating the UI - called on collect
    public void IncreaseScore()
    {
        score++;
        UpdateScoreText();
    }

    // UI display functions
    void UpdateScoreText()
    {
        scoreText.text = "Score : " + score.ToString();
    }
    void UpdateTimerText()
    {
        timerText.text = "Time : " + Mathf.Ceil(timer).ToString();
    }

    // game condition functions
    void Win()
    {
        winText.SetActive(true);
        Time.timeScale = 0f;
    }
    void GameOver()
    {
        gameOverText.SetActive(true);
        Time.timeScale = 0f;
    }
}