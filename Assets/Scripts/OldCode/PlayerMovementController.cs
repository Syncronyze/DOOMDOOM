// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// [RequireComponent(typeof(CharacterController))]
// public class PlayerMovementController : MonoBehaviour
// {
//     //public float friction = 0.25f;
//     //public float acceleration = 5.25f;
//     public float gravity = 1.0f;
//     public float maxFallSpeed = 0.5f;
//     public float runMultiplier = 2.0f;
// 	public float isGroundedCheckDist = 3.0f;

//     float speedMultiplier;
//     float T_max = 40f;
//     float V_max = 18.5f;
//     float friction;
//     float acceleration;

// 	LayerMask ignoreLayer;
//     CharacterController cc;

//     Vector3 velocity;
//     //float velocity;

//     void Start(){
//         cc = GetComponent<CharacterController>();
//         ignoreLayer = 1 << LayerMask.NameToLayer ("Player");
//         friction = 5 / T_max;
//         acceleration = friction * V_max;
//     }

//     void Update(){
//         if(Input.GetButton("Run")){
//             speedMultiplier = 2;
//         }
//         else{
//             speedMultiplier = 1;
//         }
//     }

//     void FixedUpdate(){
//         CalculateMovement();
//     }

//     public bool isGrounded(){
//         if(cc.isGrounded)
//             return true;
        
//         return isGroundedRaycast();
//     }

// 	public float GetCurrentSpeed(){
// 		return cc.velocity.magnitude;
// 	}

// 	public float GetCurrentHorzSpeed(){
// 		return new Vector2(cc.velocity.x, cc.velocity.z).magnitude;
// 	}

//     public float GetCurrentSpeedPercentage(){
//         return Mathf.Clamp(GetCurrentHorzSpeed() / 16.0f, 0, 1);
//     }
    
//     void CalculateMovement(){
//         //float distance = 0;
//         //Vector3 input = GetInput();
//         if(cc.isGrounded){
//             //velocity += GetInput() * acceleration * Time.deltaTime * speedMultiplier;
//             //velocity -= friction * Time.deltaTime * velocity;
            
//             velocity += GetInput() * (acceleration * Time.deltaTime) * speedMultiplier;
//             velocity -= (velocity * friction);
//             //distance = velocity * Time.deltaTime + (0.5f * acceleration * Mathf.Pow(Time.deltaTime, 2));

//             //velocity += (acceleration * GetInput()) - (friction * velocity);
//             //velocity -= friction * velocity;
//         }

//         velocity.y -= gravity * Time.deltaTime;
//         velocity.y = Mathf.Clamp(velocity.y, -maxFallSpeed, 0);

//         cc.Move(velocity);
//     }

//     Vector3 GetInput(){
//         return transform.TransformDirection(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"))).normalized;
//     }

    
//     bool isGroundedRaycast(){
// 		RaycastHit rcHit;
// 		if(Physics.Raycast(transform.position, Vector3.down, out rcHit, isGroundedCheckDist, ~ignoreLayer)){
// 			if(rcHit.transform != null)
// 				return true;
// 			else
// 				return false;
// 		}
// 		return false;
// 	}

//     /*
// 	 * 	TODO: REMOVE, ONLY FOR DEBUGGING.
// 	 */
// 	void OnGUI(){
// 		GUI.Label(new Rect(10, 600, 400, 80), 
// 		"Speed: " + System.Math.Round(GetCurrentSpeed(), 5) + 
// 		"\nX/Y Speed: " + System.Math.Round(GetCurrentHorzSpeed(), 5) +
// 		(isGrounded() ? "\nGrounded" : "\n"));
// 	}
// }
