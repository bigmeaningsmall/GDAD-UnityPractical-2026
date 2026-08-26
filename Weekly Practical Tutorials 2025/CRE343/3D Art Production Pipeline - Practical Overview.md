# Open Unity and Blender

#### **Update Unity Project - Import Shared Assets From Package**

1.  **Open Unity** and load your project.
2.  Navigate to **Assets \> Import Package \> Custom Package** to import the shared assets or drag the package to your project
3.  Select the package provided and import all relevant files.
    1.  Take note of how the package imported works and **always preview and check** the assets you import
    2.  Wait patiently...
4.  Confirm that the imported assets are visible in your **Project window** under the correct folder.
5.  Navigate the assets in the project to get an idea of what has been imported
    1.  Keeping track of assets and understanding what your game project is build from is an important skill that is vital when things go wrong

------------------------------------------------------------------------

# Unity Asset Pipeline

- **The asset pipeline describes how you get the art and game assets into the engine and ready to use in the game world.**
  - Understanding the pipeline is very important skill in game production
  - Game productions have custom pipelines to ensure assets are correctly:
    - Prepared
    - Imported
    - Optimised
    - Rendered in the game
  - Technical artists and designers should develop a deep understanding of the art and asset pipeline

### **3D Art Asset Pipeline - Physically Based Rendering (PBR) Workflow (Blender to Unity)**

#### **Step 1 - Export from Blender (Cube & Crate)**

1.  **Open Blender** and create your models (Cube or Crate).
2.  Select your model and navigate to **File \> Export \> FBX**.
    - Ensure **Selected Objects** is ticked in the export options.
3.  Save the FBX file in your desired location.

#### **Step 2 - Import into Unity**

1.  In **Unity**, drag your exported FBX files into the **Assets** folder.
2.  Select the FBX file in the **Project** window.
3.  Under the **Inspector**, check the **Import Settings**:
    - Adjust the **Scale Factor** if needed (1.0 is default).
    - Make sure **Generate Colliders** is ticked for physical interactions.

#### **Step 3 - Materials and Textures for Crate**

1.  **Import Textures**: Drag your texture files (e.g., Diffuse, Normal, etc.) into Unity's **Assets** folder.
2.  Select each texture and adjust the **Texture Type** (e.g., Normal Map for normal textures).
3.  **Create a New Material**:
    - Right-click in the **Assets** window, select **Create \> Material**.
    - Apply textures by dragging them onto the respective slots in the material (Albedo, Normal, etc.).
4.  **Assign the Material** to your crate by dragging the material onto the model.

#### **Step 4 - Add Objects to Unity Scene**

1.  **Add the Crate Model** to the scene by dragging it from the **Project** window into the **Scene** view.
2.  **Create Prefab**:
    - Drag the model from the **Hierarchy** window back into the **Project** window to create a prefab.
3.  **Add Unity Primitives**: Add Unity cubes or spheres into the scene to test other materials.

#### \*\*Step 5 - Iterate

1.  **Spend some time** getting used to the process of creating materials in the editor
2.  **Add More Unity Primitives**: Test different material and textures

**Goal : Understand how Physically Based Rendering (PBR) works from the material and texture setup**
- *This is the default state of a game engine material and renderer setup*
- You should develop a deep understanding of these components for working with 3D games
- Textures and Materials
- Diffuse/Albedo
- Normal maps
- Specular and smoothness

------------------------------------------------------------------------

## Decals

**Decals are projected textures with alpha transparency that are used to add variation and details to 3D environments**
- Example: cracks on a concrete wall

*By default Unity's Universal Render Pipeline (URP) does not have decals enabled*
\#### **Step 1 - Decals Setup**
1. **URP Settings**: Navigate to **Edit \> Project Settings \> Graphics** and ensure **Decals** are enabled in the URP pipeline. or check in the URP renderer setting if Decals are added as a '*Render Feature*'
2. **Add Decals**:
- Drag a decal material from the **Decals Folder** onto objects in the scene.
- Optional: Import your own decal textures and create new decals using **Create \> Material \> Decal**.

<figure>
<img
src="3D%20Art%20Production%20Pipeline%20Practical-media/f7602a6150decfb6ba81f519c9275be2bbf3ec20.png"
title="wikilink" alt="RendererSettingsDecals.png" />
<figcaption aria-hidden="true">RendererSettingsDecals.png</figcaption>
</figure>

------------------------------------------------------------------------

# Scene Prototyping

#### **Step 1 - Open Scene**

1.  Go to **Assets \> CRE343 \> Scenes** and open **Scene2-3D-Environment-Setup**.

#### **Step 2 - Setup Third-Person Character**

1.  Add the third-person character model from the **Assets**.
2.  Set up the **camera to follow** the character:
    - Attach a camera to the character, or use **Cinemachine** for smooth follow behavior.

    *or - use the scene that is already setup with the third-person character*

#### **Step 3 - Continue Environment Prototyping**

- After the lecture and during the afternoon session continue developing your environment
- **Goal**
  - Gain a feel for how geometry, materials and textures (and lighting) combine to make an environment
    - This is the basis for learning how to develop realistic real-time graphics
    - Understanding the PBR workflow is standard practice
    - Stylised render workflows and creative effects are easier to achieve with a good grounding in PBR
