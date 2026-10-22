// UIEventManager.cs
// A static class holding the UI events. It stores nothing and does no work.
// Things that change announce it here, and anything interested subscribes.

public static class UIEventManager
{
    // ---------- Player ----------

    public delegate void StatEvent(int current, int max);

    public static StatEvent OnHealthChanged;
    public static StatEvent OnManaChanged;

    public static void HealthChanged(int current, int max)
    {
        if (OnHealthChanged != null)
        {
            OnHealthChanged(current, max);
        }
    }

    public static void ManaChanged(int current, int max)
    {
        if (OnManaChanged != null)
        {
            OnManaChanged(current, max);
        }
    }

    // ---------- Progression ----------

    public delegate void ValueEvent(int value);

    public static ValueEvent OnCoinsChanged;
    public static ValueEvent OnXPChanged;
    public static ValueEvent OnLivesChanged;

    public static void CoinsChanged(int value)
    {
        if (OnCoinsChanged != null)
        {
            OnCoinsChanged(value);
        }
    }

    public static void XPChanged(int value)
    {
        if (OnXPChanged != null)
        {
            OnXPChanged(value);
        }
    }

    public static void LivesChanged(int value)
    {
        if (OnLivesChanged != null)
        {
            OnLivesChanged(value);
        }
    }

    // ---------- Game State ----------

    public delegate void SimpleEvent();

    public static SimpleEvent OnGameOver;

    public static void GameOver()
    {
        if (OnGameOver != null)
        {
            OnGameOver();
        }
    }
}