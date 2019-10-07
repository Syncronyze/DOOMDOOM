using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour {

	Rigidbody rb;
	[HideInInspector]
	public Transform cameraMain;

	[Header("Player Physics Variables")]
	public float maxSpeed;
	public float timeToTopSpeed;
	public float accelerationSpeed;
	public float groundingDistance;
	[Range(1, 3)]
	public float runMultiplier; // if we're holding shift (running)
	[Range(0, 1)]
	public float airMultiplier; // if we're in mid air, 1 is full control, 0 is none.

	Vector3 currentVelRef;
	float currentMaxVelRef;
	float currentMaxSpeed;
	float currentAirSpeed;

	bool grounded;

	private LayerMask ignoreLayer;

	void Awake(){
		rb = GetComponent<Rigidbody>();
		cameraMain = transform.Find("Main Camera").transform;
		ignoreLayer = 1 << LayerMask.NameToLayer ("Player");
	}

	void FixedUpdate(){
		PlayerMaxSpeed();
		PlayerMovementLogic();
	}

	/*
	 *  Setting the player's max speed
	 */
	void PlayerMaxSpeed(){

		float targetSpeed = maxSpeed;

		if(Input.GetKey(KeyCode.LeftShift))
			targetSpeed *= runMultiplier;

		// if we're not grounded and this is the first frame we're in air, then we update the "current air speed" which is going to be the max speed throughout the flight
		if(!isGrounded()){
			if(currentAirSpeed == 0)
				currentAirSpeed = targetSpeed;
			else
				targetSpeed = currentAirSpeed;
		}
		else{
			currentAirSpeed = 0;

			if(Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
				targetSpeed = 0;
		}

		// current max speed is the speed which the player can *at most* be going (horizontally)
		// this doesn't mean the player is going this speed, ie. if they're touching a wall or still continually accelerating.
		currentMaxSpeed = Mathf.SmoothDamp(currentMaxSpeed, targetSpeed, ref currentMaxVelRef, timeToTopSpeed);
	}

	/*
	 * Doing only horizontal movement calculations, the gravity is left up to unity.
	 */
	void PlayerMovementLogic(){

		float accelMultiplier = accelerationSpeed * rb.mass * (isGrounded() ? 1 : airMultiplier);

        rb.AddRelativeForce(Input.GetAxisRaw("Horizontal") * accelMultiplier, 0, Input.GetAxisRaw("Vertical") * accelMultiplier);

		Vector2 horizontalMovement = new Vector2 (rb.velocity.x, rb.velocity.z);
		// checking the magnitude of just the horizontal movement, if it exceeds our max speed, then we'll have to clamp it
		if (horizontalMovement.magnitude > currentMaxSpeed){
			horizontalMovement = horizontalMovement.normalized;
			horizontalMovement *= currentMaxSpeed;
		}
        // clamped velocity
		rb.velocity = new Vector3 (horizontalMovement.x, rb.velocity.y, horizontalMovement.y);

	}

	public float GetCurrentSpeed(){
		return rb.velocity.magnitude;
	}

	public float GetCurrentHorzSpeed(){
		return new Vector2 (rb.velocity.x, rb.velocity.z).magnitude;
	}

	/*
	 * All encompassing check if we're actually grounded.
	 */
	public bool isGrounded(){
		if(grounded)
			return true;
		// otherwise we use a raycast to check if the collision isn't correct.
		grounded = isGroundedRaycast();
		return grounded;
	}
	
	/*
	 * Raycast double check to see if we're actually grounded
	 */
	bool isGroundedRaycast(){
		RaycastHit rcHit;
		if(Physics.Raycast(transform.position, Vector3.down, out rcHit, groundingDistance, ~ignoreLayer)){
			if(rcHit.transform != null)
				return true;
			else
				return false;
		}
		return false;
	}

	/*
	 * Our player is grounded if the angle we're touching < 60.
	 */
	void OnCollisionStay(Collision other){
		foreach(ContactPoint contact in other.contacts){
			if(Vector2.Angle(contact.normal, Vector3.up) < 60){
				grounded = true;
				break;
			}
		}
	}
	
	/*
	 * On zero collisions, we can assume we're not grounded.
	 */
	void OnCollisionExit (){
		grounded = false;
	}

	/*
	 * 	TODO: REMOVE, ONLY FOR DEBUGGING.
	 */
	void OnGUI(){
		GUI.Label(new Rect(10, 10, 400, 80), 
		"Speed: " + System.Math.Round(GetCurrentSpeed(), 4) + 
		"\nX/Y Speed: " + System.Math.Round(GetCurrentHorzSpeed(), 4) + 
		"\nMax Speed: " + System.Math.Round(currentMaxSpeed, 3) + 
		(isGrounded() ? "\nGrounded" : ""));
	}


}

