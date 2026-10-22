using UnityEngine;
using UnityEngine.UI;   // needed for Slider

// A single bar that slides towards a value.
// Put it on any Slider - player health, mana, an enemy's health, a boss bar,
// a cooldown. It has no idea what the number means.
public class UIBar : MonoBehaviour
{
    public Slider slider;
    public float speed = 8f;      // how fast it catches up - set it high for instant

    private float target = 1f;    // where we're heading. 0 is empty, 1 is full

    private void Awake()
    {
        // If nothing was dragged in, look for a Slider on this object
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
    }

    // Give it a current and a max and it works out the rest
    public void SetValue(int current, int max)
    {
        if (max <= 0)
        {
            target = 0f;   // avoid dividing by zero
            return;
        }

        // The (float) casts matter - int divided by int is integer division,
        // so 50 / 100 would give us 0 rather than 0.5 //maths and programming!
        target = (float)current / (float)max;
    }

    private void Update()
    {
        if (slider == null)
        {
            return;
        }

        // Move a little closer to the target every frame
        slider.value = Mathf.Lerp(slider.value, target, Time.deltaTime * speed);
    }
}