using UnityEngine;  
using TMPro;  // to use text mesh pro we need to use or include the library / programming interface
  
// in this example mini game. GameManager handles scores, timers and the UI
  
public class GameManager_Collectathon : MonoBehaviour  
{  
	// this makes a static instance of this class publically available to any other class (public, static, ClassName/Type (self), referenceName)
    public static GameManager_Collectathon instance;  
    
    // text/UI references
    public TextMeshProUGUI scoreText;  
    public TextMeshProUGUI timerText;  
    public GameObject winText;  
    public GameObject gameOverText;  
  
	// logic variables
    private int score = 0;  
    private int totalCollectibles;  
    public float timer = 20f; // 60=1min  
  
    void Awake()  
    {        
	    instance = this;  // before the game starts initialise this class as an instance (make it globally available to interact with)
    }  
    void Start()  
    {        
	    // find all the instantiated collectables - we used 'Awake' to instantiate the collectables - 'Awake' executes before 'Start'
	    totalCollectibles = FindObjectsOfType<Collectable>().Length;  
	    
        UpdateScoreText();  // call the score function to set its initial state
        // set up the UI - hide the end game text
        winText.SetActive(false);  
        gameOverText.SetActive(false);  
    }  
    void Update()  
    {        
	    timer -= Time.deltaTime;  // countdown times (minus equals time.deltaTime)
        UpdateTimerText();  
		  
		//timer logic - do something when countdown finishes
        if (timer <= 0)  
        {            
	        GameOver();  
        }  
        if (score >= totalCollectibles)  
        {            
	        Win();  
        }    
	}  
	
	// public function for increasing score and updating UI - called on collect
    public void IncreaseScore()  
    {        
	    score++;  
        UpdateScoreText();  
    }  
    
    // UI Display functions 
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