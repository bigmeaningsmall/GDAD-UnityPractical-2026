using UnityEngine;
using PrimeTween;

// Pops an object up to full size when it appears.
// Drop it on any prefab - enemies, items, the player, anything spawned.
// It knows nothing about what it's attached to.
public class SpawnPop : MonoBehaviour
{
    [Header("Pop")]
    public float duration = 0.3f;
    public Ease ease = Ease.OutBack;
    public float startDelay = 0f;      // useful for staggering a group

    private Vector3 fullSize;

    private void Awake()
    {
        // Remember the size set on the prefab, then shrink to nothing
        fullSize = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        
        //Tween.Scale(transform, endValue: fullSize, duration: duration, ease: ease, startDelay: startDelay);
        
        Sequence.Create()
            .Chain(Tween.Scale(transform, endValue: fullSize * 1.3f, duration: duration * 0.6f, ease: Ease.OutQuad))
            .Chain(Tween.Scale(transform, endValue: fullSize, duration: duration * 0.4f, ease: Ease.OutQuad));
    }
}