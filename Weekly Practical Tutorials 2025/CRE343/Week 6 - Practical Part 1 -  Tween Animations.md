
---
### Tweening and DOTween Overview

**Tweening** is a method of creating smooth animations by gradually changing a property (like scale, position, or rotation) over time. 

Rather than jumping instantly from one value to another, tweening makes transitions feel fluid, which is great for enhancing game feel. For instance, when an object scales up smoothly from zero to full size, it makes that object’s appearance feel much more dynamic and noticeable.

**DOTween** is a popular Unity plugin that simplifies tweening animations. It’s lightweight, flexible, and easy to use, allowing you to quickly animate UI elements, 3D objects, and other properties with a single line of code. 

### Why DOTween is Great for Game Feel

DOTween’s ease of use and powerful animation options make it ideal for adding impactful game feel. With DOTween, you can:
- Animate position, scale, rotation, colours, and more with precise control.
- Add eases (such as bounce, elastic, or linear) to create unique effects.
- Easily create animations for spawning effects, UI feedback, and character movement without complex coding.

https://dotween.demigiant.com/ 

### Example Usage

- **Spawning Animation**  
   When an object spawns, setting up a smooth scale transition can help it stand out visually and feel like it’s “popping” into the world. In your example, you’re using DOTween to scale the player from zero to full size with a bounce effect:

   ```csharp
   using DG.Tweening;
   
   private void OnEnable()
   {
       // Set scale to zero and tween to full size with a bounce
       transform.localScale = Vector3.zero;
       transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce);
   }
   ```
   - **Explanation**: When the object is enabled, its `localScale` is set to zero. Then `DOScale` smoothly scales it to `(1, 1, 1)` over 1 second, using an `Ease.OutBounce` for a bouncy effect. This makes the spawn feel lively and engaging.

- **UI Punch Animation**
   Adding quick feedback to the UI when the score updates can make interactions feel more satisfying. Here, DOTween’s `DOPunchScale` is used to create a pulsing effect when the score text changes:

   ```csharp
   // Punch the scale of the score text for a quick feedback effect
   scoreText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 10, 1f);
   ```
   - **Explanation**: `DOPunchScale` creates a quick, elastic “punch” effect, briefly scaling the text up and down. The parameters:
     - `new Vector3(0.1f, 0.1f, 0.1f)`: Defines the punch intensity.
     - `0.5f`: Duration of the punch effect.
     - `10`: Number of vibrato oscillations.
     - `1f`: Elasticity, controlling how bouncy the punch feels.

   This quick scaling effect gives instant, noticeable feedback to the player, making the score update feel interactive and rewarding.

---

### Summary

DOTween or similar Tween libraries are effective tools for adding subtle, impactful animations that enhance game feel, making interactions visually engaging and satisfying. 

With DOTween’s variety of animation options, you can easily incorporate dynamic, responsive animations into UI elements, character spawns, and other game mechanics, elevating the overall polish and player experience.

### Task
- **Try adding the animation calls to the game objects** 
- **Experiment with different values and animation types**

**Note - Remember to add `using DG.Tweening;` in each script that uses the tween library**
#### Example Tween Animations to add to game scripts

## Player.cs
```c#
// TODO - add a tween animation to play the spawn animation tween  
// Store the original scale so we can return to it later
Vector3 initialScale = transform.localScale;
//scale the crate up from 0 to 1 in 1 second using DOTween  
transform.localScale = Vector3.zero;  
transform.DOScale(initialScale, 1f).SetEase(Ease.OutBounce);
```

## Enemy.cs
```c#
// TODO - add a tween animation to play the spawn animation tween  
// Store the original scale so we can return to it later
Vector3 initialScale = transform.localScale;
//scale the crate up from 0 to 1 in 1 second using DOTween  
transform.localScale = Vector3.zero;  
transform.DOScale(initialScale, 1f).SetEase(Ease.OutBounce);
```

## ExplodingCrate.cs
```c#
// TODO - add a tween animation to play the spawn animation tween  
// Store the original scale so we can return to it later
Vector3 initialScale = transform.localScale;
//scale the crate up from 0 to 1 in 1 second using DOTween  
transform.localScale = Vector3.zero;  
transform.DOScale(initialScale, 1f).SetEase(Ease.OutBounce);
```


## CollectableItem.cs
```c#
// TODO - add a tween animation to play the spawn animation tween  
// Store the original scale so we can return to it later
Vector3 initialScale = transform.localScale;
//scale the crate up from 0 to 1 in 1 second using DOTween  
transform.localScale = Vector3.zero;  
transform.DOScale(initialScale, 1f).SetEase(Ease.OutBounce);
```

## UI_Display.cs
```c#
//TODO - add a health animation effect  
playerHealthText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 10, 1f);
```

```c#
//TODO - add a score animation effect  
scoreText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 10, 1f);
```