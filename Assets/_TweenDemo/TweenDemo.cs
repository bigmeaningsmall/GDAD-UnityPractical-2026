using UnityEngine;
using PrimeTween;

// Nialls demo script - stick it on a Cube and press the number keys.
//
// 1  Scale
// 2  Move
// 3  Rotate
// 4  Colour
// 5  Punch
// 6  Shake
// 7  Sequence
// R  Reset
//
// Change Duration and Ease at the top and press the keys again to feel the difference.

public class TweenDemo : MonoBehaviour
{
    [Header("Shared - change and try the keys again")]
    public float duration = 0.5f;
    public Ease ease = Ease.OutQuad;

    [Header("1 - Scale (tweens TO this size)")]
    public Vector3 scaleTo = new Vector3(2f, 2f, 2f);

    [Header("2 - Move (tweens BY this each press)")]
    public Vector3 moveBy = new Vector3(2f, 0f, 0f);

    [Header("3 - Rotate (rotates BY this each press)")]
    public Vector3 rotateBy = new Vector3(0f, 180f, 0f);

    [Header("4 - Colour")]
    public Color colourTo = Color.red;

    [Header("5 - Punch")]
    public float punchStrength = 0.4f;

    [Header("6 - Shake")]
    public float shakeStrength = 0.5f;
    public float shakeFrequency = 10f;

    private Renderer rend;

    // Where we started, so R can put everything back
    private Vector3 startPosition;
    private Vector3 startScale;
    private Quaternion startRotation;
    private Color startColour;

    private void Awake()
    {
        rend = GetComponent<Renderer>();

        startPosition = transform.position;
        startScale = transform.localScale;
        startRotation = transform.rotation;
        startColour = rend.material.color;
    }

    private void Update()
    {
        // 1 - SCALE ----------
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Tween.Scale(transform, endValue: scaleTo, duration: duration, ease: ease);
        }

        //  2 - MOVE ----------
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Tween.Position(transform,
                startValue: transform.position,
                endValue: transform.position + moveBy,
                duration: duration, ease: ease);
        }

        //  3 - ROTATE ----------
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Tween.EulerAngles(transform,
                startValue: transform.eulerAngles,
                endValue: transform.eulerAngles + rotateBy,
                duration: duration, ease: ease);
        }

        // 4 - COLOUR ----------
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // Tween.Custom can animate ANY value - give it a start, an end, and what to do with each step.
            // (Tween.Color(rend.material, ...) is the shortcut, this version works in any render pipeline)
            Tween.Custom(rend.material.color, colourTo, duration: duration, ease: ease,
                onValueChange: newColour => rend.material.color = newColour);
        }

        //  5 - PUNCH ----------
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            // strength is an OFFSET from current scale, not a target size
            Tween.PunchScale(transform, strength: Vector3.one * punchStrength, duration: duration);
        }

        //  6 - SHAKE ----------
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Tween.ShakeLocalPosition(transform,
                strength: Vector3.one * shakeStrength,
                duration: duration,
                frequency: shakeFrequency);
        }

        //  7 - SEQUENCE ----------
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            // Chain runs one after another. Group would run them at the same time.
            Sequence.Create()
                .Chain(Tween.Scale(transform, endValue: scaleTo, duration: duration, ease: ease))
                .Chain(Tween.EulerAngles(transform,
                    startValue: transform.eulerAngles,
                    endValue: transform.eulerAngles + rotateBy,
                    duration: duration, ease: ease))
                .Chain(Tween.Scale(transform, endValue: startScale, duration: duration, ease: ease));
        }

        //  R - RESET ----------
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCube();
        }
    }

    private void ResetCube()
    {
        // Stop anything still running, otherwise it carries on and overrides us
        Tween.StopAll();

        transform.position = startPosition;
        transform.localScale = startScale;
        transform.rotation = startRotation;
        rend.material.color = startColour;
    }
}