// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.Serialization;
//
// public class TopDownController_Smooth : MonoBehaviour
// {
// 	public float moveSpeed = 10;
// 	public float lookSpeed = 10;
// 	
// 	public float dashSpeed = 2f;
// 	public float dashTime = 0.2f;
// 	private float startDashTime;
// 	public float dashCooldown = 2f;
// 	private bool isDashing = false;
// 	private float lastDashTime = -100f;
//
// 	public InputMap inputMap;//this is my input map for taking C# events via the new input system
// 	
// 	Rigidbody myRigidbody;
// 	Vector3 velocity;
// 	
// 	public Vector2 movementInput;
// 	public Vector2 lookInput;
// 	public bool dashInput;
//
// 	void Start (){
// 		inputMap = GetComponent<InputMap>(); // i may move this to an event with all inputs in the future
// 		
// 		myRigidbody = GetComponent<Rigidbody> ();
// 	}
// 	
// 	void Update () {
// 		
// 		// Get the movement and look and dash input from the input map
// 		movementInput = inputMap.movementInput;
// 		lookInput = inputMap.lookInput;
// 		dashInput = inputMap.aButton;
// 		
// 		
// 		// Update velocity for movement
// 		velocity = new Vector3 (movementInput.x, 0, movementInput.y) * moveSpeed;
//
// 		// Convert the lookInput from Vector2 to Vector3 and update lookDirection only if lookInput is not zero
// 		if (lookInput != Vector2.zero) {
// 			Vector3 lookDirection = new Vector3(lookInput.x, 0, lookInput.y);
//
// 			// Use the lookDirection for the LookAt function
// 			Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
// 			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookSpeed);
// 		}
// 		
// 		
// 		// Check if the player has pressed the dash button and enough time has passed since the last dash
// 		if (dashInput && Time.time > lastDashTime + dashCooldown) {
// 			isDashing = true;
// 			startDashTime = Time.time;
// 			lastDashTime = Time.time;
// 		}
//
// 		// Check if the dash should end
// 		if (isDashing && Time.time > startDashTime + dashTime) {
// 			isDashing = false;
// 		}
// 		
// 		
// 	}
//
// 	void FixedUpdate() {
// 		// If the player is dashing, increase the speed
// 		if (isDashing) {
// 			myRigidbody.MovePosition(myRigidbody.position + velocity * dashSpeed * Time.fixedDeltaTime);
// 		} else {
// 			myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
// 		}
// 	}
// 	
// 	
// }
