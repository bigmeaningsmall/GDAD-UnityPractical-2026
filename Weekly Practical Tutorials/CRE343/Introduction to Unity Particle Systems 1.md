https://www.kodeco.com/138-introduction-to-unity-particle-systems - Adapted from here...

# Introduction to Unity Particle Systems 1

Particle systems are like salt; just a small amount can add extra “pizzazz” to whatever you’re cooking up. Modern games that don’t use particle systems in some manner can feel quite bland.

In the old days, you needed to use custom graphics programming to create even a single wisp of smoke. Modern engines, like Unity, make creating particle systems quite simple with a modular, built-in particle system named *Shuriken*, which is easy to learn but lets you create complex effects.

> [!note]
> - **Shuriken Particles:** Most particle systems can be created with the Shuriken editor windows. Its the best place to start for understanding how emission, shapes, forces and lifecycles work.
> 
> - **VFX Graph** is the node editor for particles systems and visual effects. It is simple to create basic particles systems but is much more capable of large scale optimised particles or combining with Shader Graph

In this tutorial, you’ll learn the following:
- *How to add a new particle system in Unity.*
- *What the most commonly used particle system modules do and how to use them.*

This tutorial has two main parts; in the first part, you’ll build the flames of a torch. In the second part, you’ll create a bomb explosion effect. They are simple concepts but demonstrate the common usage of particles.

## Getting Started with Particle Systems - TODO Fix this

Open up the *Tutorial Project* in Unity. The Week 4 scene has 

- *Materials*: Holds the fire material.
- *Models*: Contains the torch and bomb models and their materials.
- *Prefabs*: Holds the bomb prefab.
- *Scenes*: Contains the Torch and Bomb scenes.
- *Scripts*: Holds the initial scripts.
- *Textures*: Contains the texture for the fire material.

## Adding a Particle System

A particle system generally emits particles in random positions within a predefined space, which can have a shape like a sphere or a cone. The system determines the lifetime of the particle itself, and when that lifetime expires, the system destroys the particle.

One nice thing about particle systems is that they are components that you can add to any GameObject in the scene. 

Open up the *Week 4* scene

![](https://koenig-media.raywenderlich.com/uploads/2015/09/FirstRunScene.png)

There’s not much going on at the moment. The torch is there, but there’s no fire. You’ll need to add a particle system first.

Add a GameObject *TorchFireParticles* inside the Hierarchy attached to the torch. Inside the Inspector, click the *Add Component* button. Search for *Particle System* and click to add it:

![The Add Component screen with Particle System in the search field](https://files.koenig.kodeco.com/uploads/2016/11/particlesystem.png)

*Note:* You might have noticed you didn’t add the particle system directly to the MedievalTorch GameObject. This is because the particles would be emitted from the middle of the torch, instead of from the fuel container at the top.

Play your scene or 'Play' the particles; you’ll see you have particles emitting already:

They wont look like this but you can scale the values to get close to the below effect. NOTE: There is no right or wrong here. Just learn what the different settings can do.
- Lifetime : 1-2
- Start Speed : 0.5
- Shape : Change the Radius to 0.1
- Size Over Lifetime : Make a Bezier curve to have the particles scale in and out of existence.

![Torch with particles emitting from it](https://koenig-media.raywenderlich.com/uploads/2017/11/particlestart.gif)

![Correct settings from the particle system's Renderer section](https://koenig-media.raywenderlich.com/uploads/2017/11/pinkfix.gif)

*Note:* You might see dark outlines on the default particles instead of white.  If you’d like to fix it now, click the *Renderer* section of the particle system, click the *Dot* next to the *Material* field and double-click *Default-Particle* in the window that pops up.

![Correct settings from the particle system's Renderer section](https://koenig-media.raywenderlich.com/uploads/2017/11/pinkfix.gif)

When you select a GameObject with an attached particle system, you’ll notice a black dialog in the lower right-hand corner of the scene view. This dialog lets you simulate or stop the particle system. Clicking *Simulate* activates your particle system and changes the button to a *Pause* button. To stop the simulation, click the “Stop” button.

![Dialog with stop, start and pause buttons](https://koenig-media.raywenderlich.com/uploads/2017/10/4.png)

This dialog is useful for designing particle systems that run on a fixed timeline, such as explosions.

## A Closer Look at a Particle System (NOTE: Some settings have change from this image - principle is the same)
#### The values here may differ for you - you'll have to tweak them to get the effect you are trying to create

Take a look at the Inspector. You’ll notice the particle system Component you added has several subsections:

![How the particle system Component looks in the Inspector](https://files.koenig.kodeco.com/uploads/2017/10/2.png)

Each of these subsections is called a *Module*. These Modules contain the settings for the particle system. The Module expanded by default is known as the *Main* module:

![Main module showing the particle system](https://files.koenig.kodeco.com/uploads/2017/10/3.png)

The Main module has the most common general particle settings:

- Set the lifetime to *2-4* seconds; this ensures the flame won’t be too tall.

- Set the speed to *0.5 - 0.75*; this will make the fire effect slower and more dense.

- *Duration*: The length of time in seconds for the particle system to run. Leave this at the default value of *5.00* but you can use a low value that makes sense.
- *Looping*: Repeatedly emit particles until the particle system stops. The cycle restarts once the *Duration* time is reached. The fire needs to burn continuously, so leave this enabled.
- *Prewarm*: Only used when *Looping* is enabled. The Particle System will act as if it’s already completed a full cycle on start-up.
	[![Difference between prewarm on (left) and off (right)](https://koenig-media.raywenderlich.com/uploads/2015/09/Prewarm.gif)](https://koenig-media.raywenderlich.com/uploads/2015/09/Prewarm.gif)
- *Start Delay*: The delay in seconds before the particle system starts emitting. Leave this at the default value of *0*.
- *Start Lifetime*: The initial lifetime in seconds for the particles. The particle is destroyed after this elapsed time.
	[![Start lifetime of 8 seconds (top) and 3 seconds (bottom)](https://koenig-media.raywenderlich.com/uploads/2015/09/LifeTime.gif)](https://koenig-media.raywenderlich.com/uploads/2015/09/LifeTime.gif)
	Top: 8 seconds  
	Bottom: 3 seconds
	Set the lifetime to *4* seconds; this ensures the flame won’t be too tall.
- *Start Speed*: The initial speed of the particles. The greater the speed of the particles, the more spread out they will be.
	[![Difference between a start speed of 2 (top) and 10 (bottom)](https://koenig-media.raywenderlich.com/uploads/2015/09/StartSpeed.gif)](https://koenig-media.raywenderlich.com/uploads/2015/09/StartSpeed.gif)
	Top: 2  
	Bottom: 10
	Set the speed to *0.75*; this will make the fire effect slower and more dense.

On & off

[![Difference between prewarm on (left) and off (right)](https://koenig-media.raywenderlich.com/uploads/2015/09/Prewarm.gif)](https://koenig-media.raywenderlich.com/uploads/2015/09/Prewarm.gif)

On & off

[![Start lifetime of 8 seconds (top) and 3 seconds (bottom)](https://koenig-media.raywenderlich.com/uploads/2015/09/LifeTime.gif)](https://koenig-media.raywenderlich.com/uploads/2015/09/LifeTime.gif)

Top: 8 seconds  
Bottom: 3 seconds

[![Difference between a start speed of 2 (top) and 10 (bottom)](https://koenig-media.raywenderlich.com/uploads/2015/09/StartSpeed.gif)](https://koenig-media.raywenderlich.com/uploads/2015/09/StartSpeed.gif)

Top: 2  
Bottom: 10

*Note:* As you modify the settings of the particle system, you’ll see a preview of it in the Game Window. Keep an eye on this preview while following along with the tutorial.

Before continuing, run your scene to see the effect of your changes:

![Torch emitting particles after the changes you made above](https://koenig-media.raywenderlich.com/uploads/2017/11/particlesupdate2.gif)

You’ve set up the torch, but the fire effect still leaves a lot to be desired. The Main module has additional options to further refine the shape and behavior of your torch.