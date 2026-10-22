using UnityEngine;

// The PRESENTER - it listens to the game and tells the view what to display.
// It knows about GameManager and PlayerStats, but nothing about text or canvases.
public class HUDPresenter : MonoBehaviour
{
    public HUDView view;   // drag the HUDView component here - OR itg could be on the same object and get it on awake

    private void OnEnable()
    {
        UIEventManager.OnHealthChanged += HandleHealthChanged;
        UIEventManager.OnManaChanged += HandleManaChanged;
        UIEventManager.OnCoinsChanged += HandleCoinsChanged;
        UIEventManager.OnXPChanged += HandleXPChanged;
        UIEventManager.OnLivesChanged += HandleLivesChanged;
        UIEventManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        UIEventManager.OnHealthChanged -= HandleHealthChanged;
        UIEventManager.OnManaChanged -= HandleManaChanged;
        UIEventManager.OnCoinsChanged -= HandleCoinsChanged;
        UIEventManager.OnXPChanged -= HandleXPChanged;
        UIEventManager.OnLivesChanged -= HandleLivesChanged;
        UIEventManager.OnGameOver -= HandleGameOver;
    }

    private void Start()
    {
        // Events only tell us about CHANGES. At startup nothing has changed yet,
        // so we ask for the current values once to fill the display in. We cant really avoid having to initialise the UI
        RefreshAll();
    }

    private void RefreshAll()
    {
        view.ShowCoins(GameManager.Instance.Coins);
        view.ShowXP(GameManager.Instance.XP);
        view.ShowLives(GameManager.Instance.Lives);
    }

    private void HandleHealthChanged(int current, int max)
    {
        view.ShowHealth(current, max);
    }

    private void HandleManaChanged(int current, int max)
    {
        view.ShowMana(current, max);
    }

    private void HandleCoinsChanged(int value)
    {
        view.ShowCoins(value);
    }

    private void HandleXPChanged(int value)
    {
        view.ShowXP(value);
    }

    private void HandleLivesChanged(int value)
    {
        view.ShowLives(value);
    }

    private void HandleGameOver()
    {
        view.ShowGameOver();
    }
}