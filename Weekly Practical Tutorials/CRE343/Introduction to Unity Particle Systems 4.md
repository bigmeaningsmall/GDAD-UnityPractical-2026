---
title: "Introduction to Unity: Particle Systems"
source: "https://www.kodeco.com/138-introduction-to-unity-particle-systems/page/4"
author:
  - "[[anthonyuccello]]"
published:
created: 2026-10-09
description: "Unity’s particle system is both robust and feature packed. In this tutorial, you’ll learn the ins-and-outs of it to create both fire and explosions."
tags:
  - "clippings"
---
# Introduction to Unity Particle Systems 4

## Changing Colour

With the particle system open in the Inspector, click the checkbox next the *Color over Lifetime* module to enable it and expand it. You’ll be greeted by the word *Color* and what looks like a white block next to it. *Click* on the *white block*:

![Color over Lifetime currently set to white](https://koenig-media.raywenderlich.com/uploads/2015/10/Screen-Shot-2015-10-30-at-2.16.41-PM.png)

This opens up the gradient editor:

![Setting a gradient for the Color over Lifetime](https://koenig-media.raywenderlich.com/uploads/2015/09/GradientEditor.png)

The colour change over the lifetime of the particles is represented as a gradient bar. The starting colour is on the far left, and the particles will transition to the colour on the right side:

![Gradient starts at the left and moves to the right over time](https://koenig-media.raywenderlich.com/uploads/2015/09/ColorLifetimeIllustration.png)

The four white arrows at the edges are known as *markers*; click between two existing markers to add a new one. To remove a marker, drag it off the bar:

![Setting markers for the gradient](https://koenig-media.raywenderlich.com/uploads/2015/09/AddingRemovingColorMarkers.gif)

The top markers handle the *Alpha* or opacity of the colour, while the bottom markers manage the *RGB* (Red, Green, Blue) colour values.

![Alpha markers at the top, RGB markers at the bottom](https://koenig-media.raywenderlich.com/uploads/2015/09/GradientMarkers.png)

Click on the right-most alpha marker. The bottom of the Gradient editor now shows the current alpha value:

![Alpha value displayed in the Gradient editor](https://koenig-media.raywenderlich.com/uploads/2015/09/AlphaSlider.png)

Drag the slider all the way to *0*. Now the particles will gradually fade away over the course of their lifetime.

Run your scene once again to see the effect of your changes:

![Color now fades away to nothing at the end of the explosion](https://koenig-media.raywenderlich.com/uploads/2015/09/BombSceneAlphaOverLifetime.gif)


## Where to Go From Here?

- Particles systems usually have multiple parts - try adding embers, smoke, ground charred effects
- Grab a few existing particles systems and deconstruct them - You'll fine you can learn a lot from seeing how different systems are constructed.
	- Combine some parts from different particle examples.
- Magic is a great creative approach to particles. Try making a specific effect.
- VFX Graph is the node editor for building more complex particle systems
	- https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@12.0/manual/index.html