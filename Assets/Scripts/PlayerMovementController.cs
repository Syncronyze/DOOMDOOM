using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    public float friction = 35.0f;
    public float acceleration = 5.25f;
    public float gravity = 1.0f;
    public float maxFallSpeed = 40.0f;
    public float runMultiplier = 2.0f;
	public float isGroundedCheckDist = 3.0f;

	LayerMask ignoreLayer;
    CharacterController cc;

    Vector3 velocity;

    void Start(){
        cc = GetComponent<CharacterController>();
        ignoreLayer = 1 << LayerMask.NameToLayer ("Player");
    }

    void Update(){
        if(Input.GetButtonDown("Run")){
            acceleration *= runMultiplier;
        }
        else if(Input.GetButtonUp("Run")){
            acceleration /= runMultiplier;
        }

        CalculateMovement();
    }

    public bool isGrounded(){
        if(cc.isGrounded)
            return true;
        
        return isGroundedRaycast();
    }

	public float GetCurrentSpeed(){
		return cc.velocity.magnitude;
	}

	public float GetCurrentHorzSpeed(){
		return new Vector2 (cc.velocity.x, cc.velocity.z).magnitude;
	}

    void CalculateMovement(){
        if(cc.isGrounded){
            velocity += GetInput() * acceleration * Time.deltaTime;
            velocity -= friction * Time.deltaTime * velocity;
            velocity.y = 0;
        }
        else{
            velocity.y -= Mathf.Clamp(gravity * Time.deltaTime, 0, maxFallSpeed);
        }
        
        cc.Move(velocity);
    }

    Vector3 GetInput(){
        return transform.TransformDirection(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"))).normalized;
    }

    
    bool isGroundedRaycast(){
		RaycastHit rcHit;
		if(Physics.Raycast(transform.position, Vector3.down, out rcHit, isGroundedCheckDist, ~ignoreLayer)){
			if(rcHit.transform != null)
				return true;
			else
				return false;
		}
		return false;
	}

    /*
	 * 	TODO: REMOVE, ONLY FOR DEBUGGING.
	 */
	void OnGUI(){
		GUI.Label(new Rect(10, 10, 400, 80), 
		"Speed: " + System.Math.Round(GetCurrentSpeed(), 5) + 
		"\nX/Y Speed: " + System.Math.Round(GetCurrentHorzSpeed(), 5) +
		(isGrounded() ? "\nGrounded" : ""));
	}
}
