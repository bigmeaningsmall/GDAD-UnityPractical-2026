---
Order: 1-A
---
### **Twin Stick Player Setup - Part 1 Overview**

#### **What we are building**:

- This is **Part 1** of Week 1. We set up the **player** and get it moving with a **Twin Stick Controller**.
- The player is a simple **capsule** that moves with the left stick, aims with the right stick, and dashes with the **A** button.
- Input comes from a **custom InputManager** that is already in the project - we just drag it onto the controller.
	- https://github.com/bigmeaningsmall/BMS-InputManager 
		- This is my custom extension for using the input manager via C# events 
		- It is in the Unity project under: *Assets/_Extensions/BMS InputManager* - Example scenes show how it works (`See 'Assets/_Extensions' Folder`).
		- It works with gamepad, mouse, keyboard etc..
- We finish by turning the player into a **Prefab** so we can build on it every week from here on.

#### **Why we do it this way**:

- We are making **one game project** that grows week by week, so Part 1 is the foundation everything else sits on.
- Getting the player set up as a reusable **Prefab** now means later weeks (collectibles, inventory, save/load, etc.) all extend the same player.

#### **Learning Objectives**:

- **Unity Components**: Add and configure a `Rigidbody`, `Collider`, and a custom script on a GameObject.
- **Physics Setup**: Use Rigidbody **constraints** to keep the player upright while still letting it turn to aim.
- **Input Handling**: Understand how our custom **InputManager** feeds input into a controller (polling, not the old `Input.GetAxis`).
- **Prefabs**: Create a Prefab so the player can be reused and extended across the whole project.

---
---

# **Part 1: Setting up the Twin Stick Player in Unity**

This guide walks you through setting up the player character for our Twin Stick game. We'll add the controller, wire up the input, configure the physics, and save the player as a Prefab so we can build on it in later weeks.

---

# Download the starter unity project from GitHub using GitHub Desktop

#### We are using the tag - CRE340-Week1

##### CRE340-Week1

[https://github.com/bigmeaningsmall/GDAD-UnityPractical-2026](https://github.com/bigmeaningsmall/GDAD-UnityPractical-2026) -- TODO ADD TAG WHEN DONE

or navigate via the repo we are using across the modules [https://github.com/bigmeaningsmall/GDAD-UnityPractical-2026](https://github.com/bigmeaningsmall/GDAD-UnityPractical-2026)

---

### **Step 1: Open the Scene (The scene is already set up)**

The starter project comes with the scene ready to go - a simple arena, a UI (score and timer), and the player capsule in the middle. We are going to set up the player inside this scene.

### Premade Scene - Assets / _CRE340 / Game Tutorial / Scenes / Scene-Week1_TwinStick-Collectathon

1. **Open the scene**:
    - In the Project window, go to _Assets / _CRE340 / Game Tutorial / Scenes_ and double-click **Scene-Week1_TwinStick-Collectathon** to open it.
2. **Have a look around**:
    - You should see a walled **arena** (the play area), a **Canvas** (`set to "Screen Space - Camera"`) with the Score and Time text (`TextMeshProUI`), and a **Player** capsule sitting in the middle.
    - Press `Play` for a second - nothing moves yet. That's what we are about to fix.

** **Note - The scene naming convention is 'Scene-WeekX_GameName'. Each week works in its own scene so we can keep versions of the game as it grows. Part 1 sets up the player here, and we carry that same player forward.**

---

### **Step 2: Set up the Player Object**

Let's configure the capsule so it can be controlled.

1. **Select the Player**:
    - In the Hierarchy, click the **Player** capsule to select it.
    - (If you are building your own, create one with `GameObject -> 3D Object -> Capsule` and rename it **Player**.)
2. **Set the Tag**:
    - At the top of the Inspector, set the **Tag** dropdown to **Player** (this is a built-in Unity tag).
    - We don't need this yet, but our collectibles will look for the "Player" tag later - so we set it now while we are here.
3. **Check the Collider**:
    - A Capsule already comes with a **Capsule Collider**. This is what lets the player bump into the arena walls.
    - If you made your own object without one, add it with `Add Component -> Capsule Collider`.

---

### **Step 3: Add a Rigidbody**

Our controller moves the player using physics (`Rigidbody.MovePosition`), so the player needs a **Rigidbody**.

1. **Add the component**:
    - With the Player selected, click `Add Component -> Rigidbody`.
2. **Freeze the rotation on X and Z**:
    - In the Rigidbody component, open **Constraints**.
    - Under **Freeze Rotation**, tick **X** and **Z**. Leave **Y** unticked.
    - This stops the capsule from **tipping over** when it bumps a wall, but still lets it **turn to aim** on the Y axis (which our script controls).
3. **Leave the rest on defaults**:
    - **Use Gravity** stays **on** so the player stays on the floor. 

** **Note - Tip: turning on 'Interpolate' (Rigidbody -> Interpolate -> Interpolate) makes physics movement look smoother on screen. Optional, but nice to use on the player as the main controlled object.**

---

### **Step 4: Add the Twin Stick Controller Script**

Now we add the script that reads input and moves the player.

1. **Create the TwinStickController Script**:
    - In the Project window, go to the **Scripts** folder (_Assets / _CRE340 / Game Tutorial / Scripts_), right-click and choose `Create -> C# Script`. Name it `TwinStickController`.
    - Replace the script with the following code:

```csharp

using UnityEngine;
// using UnityEngine.InputSystem; // dont actually need the input system as its handled by the 'InputManager' extension

public class TwinStickController : MonoBehaviour
{
    [Header("Input Manager Reference")]
    // - This is my custom input manager extension I use for all devices and inputs -  See _Extensions/BMS InputManager folder for examples, scenes and usage
    // - Basically - It handles all input using C# events and allows for standard input handling and naming - uses gamepad conventions
    // - We READ (poll) it here every frame - the InputManager does the event listening internally, we just ask it for the current values
    public InputManager inputManager; // Reference to the InputManager script - drag the InputManager object onto this in the Inspector
    
    [Space(10)]
    
    public float moveSpeed = 10;
    public float lookSpeed = 10;
    
    public float dashSpeed = 2f;        // multiplier applied to normal speed while dashing (2 = twice as fast)
    public float dashTime = 0.2f;       // how long a single dash lasts, in seconds
    private float startDashTime;        // the Time.time when the current dash began
    public float dashCooldown = 1f;     // minimum seconds between dashes
    private bool isDashing = false;
    private float lastDashTime = -100f; // start well in the past so the first dash is allowed immediately

    
    Rigidbody myRigidbody;
    Vector3 velocity;
    
    // [SerializeField] so you can watch the live input values in the Inspector while playing
    [SerializeField] Vector2 movementInput;
    [SerializeField] Vector2 lookInput;
    [SerializeField] bool dashInput;

    void Start (){
        myRigidbody = GetComponent<Rigidbody> (); // cache the Rigidbody component for later use
    }
    
    void Update () {
        
        // --- Read input from the custom InputManager ---
        // LeftStickInput / RightStickInput are Vector2 properties the InputManager keeps up to date for us,
        // so we just poll (read) them each frame
        movementInput = inputManager.LeftStickInput;   // left stick  = move
        lookInput = inputManager.RightStickInput;      // right stick = aim / look

        // ButtonSouth (the "A" button) is an InputActionState. Pressed() is true ONLY on the frame the button goes down
        dashInput = inputManager.ButtonSouth.Pressed();
        
        
        // Update velocity for movement (turn the 2D stick into a 3D velocity on the X/Z ground plane)
        velocity = new Vector3 (movementInput.x, 0, movementInput.y) * moveSpeed;

        // Convert the lookInput from Vector2 to Vector3 and update lookDirection only if lookInput is not zero
        if (lookInput != Vector2.zero) {
            Vector3 lookDirection = new Vector3(lookInput.x, 0, lookInput.y);

            // Use the lookDirection for the LookAt function
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookSpeed);
        }
        
        // Start a dash: the button was pressed this frame AND the cooldown has finished
        if (dashInput && Time.time > lastDashTime + dashCooldown) {
            isDashing = true;
            startDashTime = Time.time;
            lastDashTime = Time.time;
        }

        // End the dash once dashTime seconds have passed since it started
        if (isDashing && Time.time > startDashTime + dashTime) {
            isDashing = false;
        }
        
    }

    void FixedUpdate() {
        // Rigidbody movement goes in FixedUpdate (the physics clock), not Update.
        // While dashing we push the same velocity harder by multiplying it by dashSpeed.
        if (isDashing) {
            myRigidbody.MovePosition(myRigidbody.position + velocity * dashSpeed * Time.fixedDeltaTime);
        } else {
            myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
        }
    }
}

```

2. **Attach it to the Player**:
    - Drag the `TwinStickController` script onto the **Player**, or select the Player and use `Add Component -> Twin Stick Controller`.

**Explanation**:
- **Movement and aim** are read from the InputManager every frame - left stick moves, right stick aims.
- **Movement** happens in `FixedUpdate` because we are moving a Rigidbody, and physics runs on its own clock.
- **Dash** is a short speed boost with a cooldown, started when the **A** button is pressed.

---

### **Step 5: Wire up the InputManager (drag it in)**

This is the important bit to set up the player as a prefab we can use and extend each week

Our project uses a custom **InputManager** (from the __Extensions / BMS InputManager_ folder). It handles **all devices** (gamepad, keyboard, etc.) and gives us nicely named inputs using **gamepad conventions** - `LeftStickInput`, `RightStickInput`, `ButtonSouth`, and so on. It does the event listening for us internally, so in our controller we just **read** the values.

1. **Find the 'InputManager' (`Prefab GameObject`) in the scene**:
    - In the Hierarchy, find the **InputManager** object.
2. **Drag it onto the controller**:
    - Select the **Player**.
    - On the **Twin Stick Controller** component, find the **Input Manager** field at the top.
    - Drag the **InputManager** object from the Hierarchy into that field.

** **Note - This is why the InputManager field is a public reference and NOT a `GetComponent` in the script. The InputManager lives on its own object, so we point at it in the Inspector by dragging it in. If this field is left empty you'll get a Null Reference error on Play.**

**Explanation**:
- Old Unity input used `Input.GetAxis("Horizontal")` scattered through every script.
- Our InputManager keeps that in one place and hands us clean, named values. Every controller in the project (this one, the first person one, etc.) reads input the exact same way.

---

### **Step 6: Make the Player a Prefab**

We want to reuse this player in later weeks, so we save it as a **Prefab**.

1. **Create the Prefab**:
    - In the Project window, open _Assets / _CRE340 - Game Tutorial / Prefabs_.
    - Drag the **Player** object from the Hierarchy into the **Prefabs** folder.
    - This creates a **Player** prefab (the object in the Hierarchy turns blue to show it's now a prefab instance).

**Explanation**:
- The Prefab is our **single source of truth** for the player.
- If we open a new scene, we just drop the **Player** prefab in and it already has the controller, Rigidbody, and input wired up.
- If we improve the player later, we edit the Prefab once and every scene gets the update.
	- This is a '**Composite Pattern**'. It is fundamental to how Unity and most engines organise their system and logic - *More on that later in the module!*


** **Note - The InputManager Prefab and reference is part of the Player Prefab Composition - It is technically a per-scene object that may need repointed but we'll cover that when it comes up..**


---

### **Step 7: Test the Player**

1. **Press Play**:
    - Move with the **left stick** (or WASD / your keyboard mapping - the InputManager handles both - or you can set it to not autoswitch).
    - Aim / turn with the **right stick**.
    - Press **A** to dash.
2. **Watch the Inspector**:
    - With the Player selected while playing, you can see `movementInput`, `lookInput`, and `dashInput` updating live in the Twin Stick Controller component. Handy for checking your input is coming through.
3. **Tune it**:
    - Adjust `moveSpeed`, `lookSpeed`, `dashSpeed`, `dashTime`, and `dashCooldown` in the Inspector until it feels right.

---

### **What we've set up**:

- A **Player** with a Rigidbody (rotation frozen on X and Z), a Collider, and the **Twin Stick Controller**.
- Input coming from the shared **InputManager**, dragged in via the Inspector.
- The player saved as a reusable **Prefab**.

This player is the foundation for the rest of the project. In **Part 2** we'll build a throwaway mini-game around it - spawning collectibles, tracking score and time, and setting up the win/lose conditions - all using the player we just made.