using UnityEngine;
using PrimeTween;   // needed for everything in this session

public class TweenTest : MonoBehaviour
{
    private void Start()
    {
        // Scale this object up to double size over one second
        Tween.Scale(transform, endValue: Vector3.one * 2f, duration: 1f, ease: Ease.OutBack);
    }
}