---
title: "Introduction to Unity: Particle Systems"
source: "https://www.kodeco.com/138-introduction-to-unity-particle-systems/page/2"
author:
  - "[[anthonyuccello]]"
published:
created: 2026-10-09
description: "Unity’s particle system is both robust and feature packed. In this tutorial, you’ll learn the ins-and-outs of it to create both fire and explosions."
tags:
  - "clippings"
---
# Introduction to Unity Particle Systems 2

## More Main Module Properties

Select the *TorchFireParticles* GameObject in the Hierarchy and scroll down to the particle system. Under the Main Module, take a look at the following properties:

Top: 0.2  
Bottom: 1

Set the size to *3*; this is a manageable size that lets you see the individual particles more clearly.

Top: 0°  
Bottom: 45°

Keep the rotation at *0°*; the particles are round so you will hardly notice the difference.

![Particles falling downwards due to the gravity effect](https://koenig-media.raywenderlich.com/uploads/2015/09/GravityModifier.gif)  
The image above is from a system with the gravity modifier set to *1*, which makes the particles flow down like a waterfall. Leave the gravity in your system at *0*; the particles will move upwards with the velocity you set in *Start Speed*.

Top: local space  
Bottom: world space

Leave your simulation set to *Local Space*. The effect will only be visible once the particle system is moved.

- *Start Size*: the initial size of the particles.
	[![Difference between a particle size of 0.2 (top) and 1 (bottom)](https://koenig-media.raywenderlich.com/uploads/2015/09/StartSize.gif)](https://koenig-media.raywenderlich.com/uploads/2015/09/StartSize.gif)
	Top: 0.2  
	Bottom: 1
	Set the size to *3*; this is a manageable size that lets you see the individual particles more clearly.
- *Start Rotation*: The initial rotation angle of the particles.
	[![Difference between a start rotation of 0° (top) and 45° (bottom)](https://files.koenig.kodeco.com/uploads/2015/09/StartRotation.gif)](https://koenig-media.raywenderlich.com/uploads/2015/09/StartRotation.gif)
	Top: 0°  
	Bottom: 45°
	Keep the rotation at *0°*; the particles are round so you will hardly notice the difference.
- *Start Color*: The initial color of the particles. Keep the color at the default value of *pure white (255,255,255)*; you’ll color the fire via a texture.
- *Gravity Modifier*: Scales the *gravity* value set in Unity’s *Physics Manager* window. If it’s set to 0, the gravity will be turned off.
	![Particles falling downwards due to the gravity effect](https://koenig-media.raywenderlich.com/uploads/2015/09/GravityModifier.gif)  
	The image above is from a system with the gravity modifier set to *1*, which makes the particles flow down like a waterfall. Leave the gravity in your system at *0*; the particles will move upwards with the velocity you set in *Start Speed*.
- *Simulation Space*: Moves particles in *Local Space* along with the particle system. While in *World Space*, particles move freely once emitted.
	[![Difference between the local space effect (top) and world space effect (bottom)](https://koenig-media.raywenderlich.com/uploads/2015/09/WorldVsLocalSpace.gif)](https://koenig-media.raywenderlich.com/uploads/2015/09/WorldVsLocalSpace.gif)
	Top: local space  
	Bottom: world space
	Leave your simulation set to *Local Space*. The effect will only be visible once the particle system is moved.
- *Play On Awake*: Starts emitting immediately when enabled. If this is turned off, you have to manually start the particle system via script or an animation system. Leave this setting on, as you want the fire to start when the scene starts playing.
- *Max Particles*: The maximum number of particles the system may have alive at once. If you try to emit more particles than this, they won’t be emitted at all. This setting is primarily for performance reasons, and the default value of *1000* particles is more than enough in this case.

[![Difference between a particle size of 0.2 (top) and 1 (bottom)](https://koenig-media.raywenderlich.com/uploads/2015/09/StartSize.gif)](https://koenig-media.raywenderlich.com/uploads/2015/09/StartSize.gif)

Top: 0.2  
Bottom: 1

[![Difference between a start rotation of 0° (top) and 45° (bottom)](https://files.koenig.kodeco.com/uploads/2015/09/StartRotation.gif)](https://koenig-media.raywenderlich.com/uploads/2015/09/StartRotation.gif)

Top: 0°  
Bottom: 45°

[![Difference between the local space effect (top) and world space effect (bottom)](https://koenig-media.raywenderlich.com/uploads/2015/09/WorldVsLocalSpace.gif)](https://koenig-media.raywenderlich.com/uploads/2015/09/WorldVsLocalSpace.gif)

Top: local space  
Bottom: world space

There are a lot of settings. No right or wrong way to set them up. Its really just an animation system for points.

You should have something close to the gif below.  You could pass this off a smoke, mist or several other effect without changing much more than default settings.

![Torch with particles that look much more like fire](https://koenig-media.raywenderlich.com/uploads/2017/11/particlesupdate3.gif)


It needs actual particles though. To do that, you need to change the *emission* of the system.
## Introducing the Emission Module

The *Emission* module handles the number and timing of emitted particles in the system to create anything from a continuous flow, to a sudden burst of particles, depending on your needs.

While still in the particle system’s Inspector, click on the Emission module title:

![Emission module title in the particle system's Inspector](https://koenig-media.raywenderlich.com/uploads/2015/09/ClickOnEmission.png)

This opens up the Emission module:

![Emission module in the particle system's Inspector](https://files.koenig.kodeco.com/uploads/2017/11/emissions.png)

*Rate over Time* represents the number of particles emitted per second. Set *Rate over Time* to *15*.

Run your scene again; your particle system looks a little more like a burning flame now.

![Torch with light that looks a lot like smoke... closer to flame](https://koenig-media.raywenderlich.com/uploads/2017/11/updatedparticles4.gif)

It still looks like smoke but we can add a custom texture.

## Adding a Custom Texture

All particles have a particle material and a texture that defines how they look. There’s only so much you can do with the default texture. By changing the texture, you can create effects like magic stars, smoke and of course, fire.

Changing the particle texture is quite easy. Up to this point, the particles have been drawn on the screen using the *Default-Particle* material, which is a particle material with a circular gradient texture:

![Default-Particle setting](https://files.koenig.kodeco.com/uploads/2017/11/rendertexture.png)

To change the material, select the *TorchFireParticles* GameObject inside the Hierarchy. Then find the the particle system component in the Inspector and open up the particle system’s *Renderer* module.

You'll have to create a *FireMaterial Material* to put in the *Material* property and add the fire texture to its base map:
- Create a Material - Name it - Sets its Shader to  '*Universal Render Pipeline/Particles/Simple Lit*' - Particles are the same as other rendered object. They use shaders and materials.

![FireMaterial Material in the Material property](https://koenig-media.raywenderlich.com/uploads/2017/11/texturedrag.gif)

Finally, run the scene to see your custom texture in use:

![Torch with light that has the right color for flame](https://koenig-media.raywenderlich.com/uploads/2017/11/firsttorch.gif)

The flame’s may be a bit too wide though; to fix that, you’ll have to change the shape of the particle system.

## Changing the Particle System’s Shape

The *Shape* module, as the name implies, controls the shape and the behavior of particles in that shape. You can choose from several different shapes; each has their own particular settings. This lets you create particles in a box, a sphere, or even in your own custom mesh!

Expand the *Shape* module in the Inspector:

![Shape module in the Inspector](https://files.koenig.kodeco.com/uploads/2017/11/shapemodule-1.png)

The *shape* of your particle system is set to *cone*, which means particles emit from the *base* and move outwards at an *angle*:

![How articles move in a cone shape](https://koenig-media.raywenderlich.com/uploads/2015/09/Cone.png)

In the example above, the base is colored *blue*, the angle is *green* and the particles are *red*. Also notice that while you have the Shape Module expanded, you’re presented with a handy preview of the cone in the Scene view:

![Cone preview](https://koenig-media.raywenderlich.com/uploads/2015/09/ConeSceneView1.png)

Changing the *Angle* changes the size of the cone to make it wider or narrower. Set this to *7*; you’ll get a nice tight flame that widens slightly as the particles rise.

Changing the *Radius* changes the size of the base. The larger this value is, the more the particles will scatter as they emit. Set this value to *0.2*; this ensures the flames will start inside the fuel holder of the torch.

Run your scene and see how the shape of the flame has changed:

![Torch with a much-improved flame shape](https://koenig-media.raywenderlich.com/uploads/2017/11/newflame.gif)

That’s starting to look like a real flame! The finishing touch is to change the size of the particles over the course of their lifetime.