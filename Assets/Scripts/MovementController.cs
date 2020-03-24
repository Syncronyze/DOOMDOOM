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

    public float totalAirTime{ get; private set; }

    //use t = 10, v = 32 for enemies (near 0 acceleration), t = 120, v = 16.666f for players
    // they both top out at 16u/s, only friction is changed.
    public float T_max = 120f;
    public float V_max = 16.666f;
    float friction;
    float acceleration;
    float airTime;

	LayerMask ignoreLayer;
    CharacterController cc;

    Vector3 velocity;
    Vector3 input;

    bool fly;
    //float velocity;

    void Start(){
        cc = GetComponent<CharacterController>();
        ignoreLayer = 1 << gameObject.layer;
        friction = 5 / T_max;
        acceleration = friction * V_max;
        fly = false;
    }

    public void Fly(bool _fly){
        fly = _fly;
        velocity.y = 0;
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

    public void ResetAirTime(){
        totalAirTime = 0;
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
        return Mathf.Clamp(GetCurrentHorzSpeed() / 32.0f, 0, 1);
    }

    public IEnumerator DisableOnGrounded(){
        while(!isGrounded()){
            yield return new WaitForSeconds(0.25f);
        }
        yield break;
    }
    
    void CalculateMovement(){
        //float distance = 0;
        //Vector3 input = GetInput();
        if(cc.isGrounded || fly){
            //velocity += GetInput() * acceleration * Time.deltaTime * speedMultiplier;
            //velocity -= friction * Time.deltaTime * velocity;
            if(airTime > 0)
                totalAirTime = airTime;

            airTime = 0;
            velocity += InputVector() * (acceleration * Time.deltaTime) * speedMultiplier;
            velocity -= (velocity * friction);
            //distance = velocity * Time.deltaTime + (0.5f * acceleration * Mathf.Pow(Time.deltaTime, 2));

            //velocity += (acceleration * GetInput()) - (friction * velocity);
            //velocity -= friction * velocity;
        }
        else{
            airTime += Time.deltaTime;
        }

        if(!fly){
            velocity.y -= gravity * Time.deltaTime;
            velocity.y = Mathf.Clamp(velocity.y, -maxFallSpeed, 0);
        }
        
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
