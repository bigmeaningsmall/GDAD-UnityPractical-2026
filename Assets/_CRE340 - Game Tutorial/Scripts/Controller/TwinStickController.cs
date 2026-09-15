
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
    public float dashCooldown = 2f;     // minimum seconds between dashes
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

        // ButtonSouth (the "A" button) is an InputActionState. Pressed() is true ONLY on the frame the
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