# Introduction to Unity Particle Systems 3

## Changing Size Over Lifetime - Using Bezier Curves

With the *Size over Lifetime* module, you can create particles that grow or shrink during their lifetime, or even pulsate like fireflies in a forest.

In your particle system’s module list, look for *Size over Lifetime*. It’s not enabled by default, so tick on the checkbox next to the module name to enable it:

![checkbox next to the Size over Lifetime checkbox](https://koenig-media.raywenderlich.com/uploads/2015/09/SizeOverLifetimeCheckbox.png) />

Expand the Size over Lifetime module by clicking on its name. This will display a dark gray background with a flat *Curve* running along the top:

![How Size over Lifetime module looks when it's expanded](https://files.koenig.kodeco.com/uploads/2017/11/sizeoverlife.png)

Click on the *dark gray background* to open the curves editor at the bottom of the Inspector. The horizontal axis depicts the lifetime of the particle, while the vertical axis depicts its size:

![](https://files.koenig.kodeco.com/uploads/2017/11/curves.png)

You can move the keys at either end of the red line to edit the curve; you can also add additional keys by double-clicking anywhere on the curve. To delete keys, right-click on the key to remove and select *Delete Key*. You can also select one of the preset curves at the bottom:

![Grid with a straight, 45-degree line between size and lifetime](https://koenig-media.raywenderlich.com/uploads/2017/11/curvesdemo.gif)

If you think about flames in the real world, they tend to shrink as the particles rise. To mimic this, select the third preset from the right to create a downward-facing slope:

![Variations in the line between size and lifetime](https://files.koenig.kodeco.com/uploads/2017/11/selectcurve.png)


![Torch with a realistic-looking flame](https://koenig-media.raywenderlich.com/uploads/2017/11/finalflame.gif)

#### Colour over Lifetime
- Try playing around with other settings. 
- Using *Colour Over Lifetime* you can have your particles fade out using the alpha property

## Building a Bomb (Effect)

Making an exploding effect is quite simple in Unity. Once you know how to instantiate particles at will, you can use this effect for things such as car wheels sparking when they scrape the ground, or balloons that pop and shower confetti.

Set Active the *Bomb* in the hierarchy:

![empty bomb scene](https://koenig-media.raywenderlich.com/uploads/2015/09/BombSceneFirstRun.png)

#### Note: You can follow the steps here to have the bombs drop and instantiate the particles or just skip ahead and create the particle effect

To spawn bombs, drag the *Bomb* Prefab to the *Bomb Emitter* prefab slot:

[![Correct settings in the DragBombPrefabToEmitter](https://koenig-media.raywenderlich.com/uploads/2015/10/DragBombPrefabToEmitter.gif)](https://koenig-media.raywenderlich.com/uploads/2015/10/DragBombPrefabToEmitter.gif)

Play the scene again to see your bombs appear:

![bombs droping from ceiling and falling through floor](https://koenig-media.raywenderlich.com/uploads/2015/09/BombSceneSecondRun.gif)

The emitter creates a new bomb every two seconds. To put a neat *spin* on things, you’ll add some rotational force to the bomb when it spawns.

Open up the *Bomb* script inside the *Scripts* folder in your Project Window.

Add the following code to `Start()`:

```csharp
void Start()
{
    float randomX = UnityEngine.Random.Range (10f, 100f);
    float randomY = UnityEngine.Random.Range (10f, 100f);
    float randomZ = UnityEngine.Random.Range (10f, 100f);

    Rigidbody bomb = GetComponent<Rigidbody> ();
    bomb.AddTorque (randomX, randomY, randomZ);
}
```

The first three lines generate random float values between 10 and 100 for the x, y and z axes. Next, you get a reference to the bomb’s Rigidbody component and apply torque to it. This causes the bomb to rotate in a random direction. Save your script changes, return to Unity and run the scene.

![Bombs spinning as they fall](https://koenig-media.raywenderlich.com/uploads/2015/09/BombRotating.gif)

The bombs now rotate nicely while they fall.

In the Hierarchy, press the *Create* button and select *Create Empty*. Click on the newly created GameObject and name it *ExplosionParticles*. Next, add a new particle system to the GameObject. 

With your particle system in place, drag the *ExplosionParticles* GameObject from the Hierarchy to the *Prefabs* folder in the Project Browser. After that’s done, delete the *ExplosionParticles* GameObject from the Project Hierchy.

![How to delete the ExplosionParticles GameObject from the Hierarchy](https://koenig-media.raywenderlich.com/uploads/2017/11/newprefab.gif)

Next, select the *Bomb* Prefab inside the *Prefabs* folder and drag the *ExplosionParticles* Prefab to the *Bomb* ‘s Explosion Particles Prefab slot like so:

![How to drag the ExplosionParticles Prefab to the Bomb's Explosion Particles Prefab slot](https://koenig-media.raywenderlich.com/uploads/2017/11/prefabset.gif)

Now, a new Explosion Particles GameObject will spawn when a bomb touches the ground.

Play your scene to see how the explosion looks. Needs a texture and setting changed.

![Explosion particles appearing when the bomb reaches the floor](https://koenig-media.raywenderlich.com/uploads/2017/11/bombsaway.gif)

Nothing near an explosion yet!

As with the torch, you’ll be using the Fire material for the particle system.

Select the *ExplosionParticles* Prefab in the Project Window, then expand the *Renderer* Module in the Inspector. Drag the *FireMaterial* from the Materials folder in the Project Window to the *Material* slot as shown below:

![How to drag the FireMaterial to the Material slot](https://koenig-media.raywenderlich.com/uploads/2017/11/firematerialset.gif)

To complete the effect, you’ll have to modify the following settings in the Main module (May not be exactly these settings):

![Where to modify the Main module's settings](https://files.koenig.kodeco.com/uploads/2017/11/BombSettings.png)

1. Set the *Duration* to *0.70*.
2. *Looping* should be disabled. The particles should emit just once. This is the difference between a One Shot animation or looped animations
3. Set the *Start Lifetime* to *0.7*.
4. Set the *Start Speed* to *10*.
5. Set *Start Size* to *2*.
6. Set the *Gravity Modifier* to *1*. This will make the particles drop slightly at the end.

Run your bomb scene to see what you’ve built:

![Red fire-like particles emitting when the bomb reaches the floor](https://koenig-media.raywenderlich.com/uploads/2015/09/BombScenePreEmission.gif)

## Building the Explosion

To improve the explosion, you’ll alter the properties of one of the particle system’s modules. 

Expand the *Emission Module*. Rate is the number of particles spawned per second. For this explosion, you won’t want a steady flow of particles, but rather a sudden burst.

Set the *Rate over Time* to *0*. Now look beneath the *Rate over Distance*, and you’ll see a list of *Bursts* that’s empty by default:

![Where to see the list of Bursts](https://files.koenig.kodeco.com/uploads/2017/11/emissionrate.png)

A Burst is a collection of particles emitted all at once at a particular point in time.

Click on the *\+ button* at the bottom right to add a new Burst. You’ll see two fields: *Time* and *Count*:

![How to add a new burst](https://files.koenig.kodeco.com/uploads/2017/11/bursts.png)

Leave the *Time* at *0*, and set *Count* to *150*. These settings will make the particle system emit 150 particles all at once at the start of the system.

Play your scene; how do things look now?

![Bomb falling with a much more satisfying explosion](https://koenig-media.raywenderlich.com/uploads/2015/09/BombSceneEmission.gif)

Now it looks like an explosion! While this explosion looks better, the shape is still an awkward cone and the particles don’t fade out — they simply disappear. You’ll need to mold your explosion to give it that final touch.

To get started, expand the *Shape* Module:

![The expanded Shape Module](https://files.koenig.kodeco.com/uploads/2017/11/shapeoutline.png)

You’ve already used this module for the torch’s fire shape, but there are several more shapes to choose from. Click on the dropdown box that says *Cone* to see all options available to you:

![Various shape options for your explosion](https://koenig-media.raywenderlich.com/uploads/2017/11/shapeoptions.png)

Each shape affects the emitter in a different way. Each animation below shows the same emitter, with only the shape changed:

*Sphere*

![](https://koenig-media.raywenderlich.com/uploads/2015/09/ShapeSphere.gif)

*HemiSphere*

![](https://koenig-media.raywenderlich.com/uploads/2015/09/ShapeHemiSphere.gif)

*Cone*

![](https://koenig-media.raywenderlich.com/uploads/2015/09/ShapeCone.gif)

*Box*

![](https://koenig-media.raywenderlich.com/uploads/2015/09/ShapeBox.gif)

*Mesh (Cube)*

![](https://koenig-media.raywenderlich.com/uploads/2015/09/ShapeMeshCube.gif)

*Circle*

![](https://koenig-media.raywenderlich.com/uploads/2015/09/ShapeCircle.gif)  
*Edge*

![](https://koenig-media.raywenderlich.com/uploads/2015/09/ShapeEdge.gif)

You can get many different effects from the same system - simply by changing the shape! To create a realistic explosion, set the shape to *Sphere*.

Run the scene. *Boom!*:

![Cool looking explosion when the bomb hits the ground](https://koenig-media.raywenderlich.com/uploads/2015/09/BombSceneSphere.gif)


While the explosion looks good, there’s one small problem. The particles simply disappear. This is a jarring effect and doesn’t look natural at all. Rather than just disappear, the particles should fade over time to make the explosion fade away.