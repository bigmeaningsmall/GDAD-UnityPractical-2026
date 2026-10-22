using UnityEngine;
using TMPro;

// The VIEW - it knows how to display things and nothing else.
// No events, no GameManager, no game logic. Give it a number and it shows the number.
public class HUDView : MonoBehaviour
{
    [Header("Player - Text")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI manaText;

    [Header("Player - Bars")]
    public UIBar healthBar;
    public UIBar manaBar;

    [Header("Progression")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI livesText;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    private void Awake()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void ShowHealth(int current, int max)
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + current + " / " + max;
        }

        if (healthBar != null)
        {
            healthBar.SetValue(current, max);
        }
    }

    public void ShowMana(int current, int max)
    {
        if (manaText != null)
        {
            manaText.text = "Mana: " + current + " / " + max;
        }

        if (manaBar != null)
        {
            manaBar.SetValue(current, max);
        }
    }

    public void ShowCoins(int value)
    {
        if (coinsText != null)
        {
            coinsText.text = "Coins: " + value;
        }
    }

    public void ShowXP(int value)
    {
        if (xpText != null)
        {
            xpText.text = "XP: " + value;
        }
    }

    public void ShowLives(int value)
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + value;
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}