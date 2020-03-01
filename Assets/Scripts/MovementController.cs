using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    //public float friction = 0.25f;
    //public float acceleration = 5.25f;
    public float gravity = 1.0f;
    public float maxFallSpeed = 0.5f;
    //public float runMultiplier = 2.0f;
	public float isGroundedCheckDist = 3.0f;
    public float speedMultiplier = 1f;

    //float T_max = 40f;
    float T_max = 120f;
    float V_max = 16.5f;
    float friction;
    float acceleration;

	LayerMask ignoreLayer;
    CharacterController cc;

    Vector3 velocity;
    Vector3 input;
    //float velocity;

    void Start(){
        cc = GetComponent<CharacterController>();
        ignoreLayer = 1 << gameObject.layer;
        friction = 5 / T_max;
        acceleration = friction * V_max;
    }

    void FixedUpdate(){
        if(cc.enabled)
            CalculateMovement();
    }

    public void SetMovementVector(Vector3 vector){
        input = vector;
    }

    public void SetSpeedMultiplier(float multiplier){
        speedMultiplier = multiplier;
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
		return new Vector2(cc.velocity.x, cc.velocity.z).magnitude;
	}

    public float GetCurrentSpeedPercentage(){
        return Mathf.Clamp(GetCurrentHorzSpeed() / 16.0f, 0, 1);
    }
    
    void CalculateMovement(){
        //float distance = 0;
        //Vector3 input = GetInput();
        if(cc.isGrounded){
            //velocity += GetInput() * acceleration * Time.deltaTime * speedMultiplier;
            //velocity -= friction * Time.deltaTime * velocity;
            
            velocity += InputVector() * (acceleration * Time.deltaTime) * speedMultiplier;
            velocity -= (velocity * friction);
            //distance = velocity * Time.deltaTime + (0.5f * acceleration * Mathf.Pow(Time.deltaTime, 2));

            //velocity += (acceleration * GetInput()) - (friction * velocity);
            //velocity -= friction * velocity;
        }

        velocity.y -= gravity * Time.deltaTime;
        velocity.y = Mathf.Clamp(velocity.y, -maxFallSpeed, 0);
        cc.Move(velocity);
    }

    Vector3 InputVector(){
        return transform.TransformDirection(input).normalized;
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
}
