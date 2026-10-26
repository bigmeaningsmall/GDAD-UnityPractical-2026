using UnityEngine;
using PrimeTween;

// Gives a UI element a quick scale punch when a value changes.
public class UIPunch : MonoBehaviour
{
    public Transform coinTarget;    // the coin text
    public Transform xpTarget;      // the XP text

    [Header("Punch")]
    public float punchStrength = 0.25f;   // how far PAST normal size it goes
    public float duration = 0.3f;
    public float frequency = 5f;          // how many wobbles before it settles

    private void OnEnable()
    {
        UIEventManager.OnCoinsChanged += HandleCoinsChanged;
        UIEventManager.OnXPChanged += HandleXPChanged;
    }

    private void OnDisable()
    {
        UIEventManager.OnCoinsChanged -= HandleCoinsChanged;
        UIEventManager.OnXPChanged -= HandleXPChanged;
    }

    private void HandleCoinsChanged(int value)
    {
        Punch(coinTarget);
    }

    private void HandleXPChanged(int value)
    {
        Punch(xpTarget);
    }

    private void Punch(Transform target)
    {
        if (target == null)
        {
            return;
        }

        // If a punch is already running on this element, stop it and reset,
        // otherwise fast pickups stack up and the scale drifts
        Tween.StopAll(onTarget: target);
        target.localScale = Vector3.one;

        Tween.PunchScale(target, strength: Vector3.one * punchStrength,
            duration: duration, frequency: frequency);
    }
}