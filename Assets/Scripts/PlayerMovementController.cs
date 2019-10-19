using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    public float speed = 16.0f;
    public float gravity = 60.0f;
    public float maxFallSpeed = 40.0f;
    public float runMultiplier = 2.0f;
    public float airMultiplier = 0.75f;
    public float timeToMaxSpeed = 0.125f;
	public float isGroundedCheckDist = 3f;

    Vector3 lastVelocity;
    Vector3 moveDirection;
	LayerMask ignoreLayer;
    CharacterController cc;

    float accelerationTimer;
    bool running;

    void Start(){
        cc = GetComponent<CharacterController>();
        ignoreLayer = 1 << LayerMask.NameToLayer ("Player");
        accelerationTimer = 0;
    }

    void Update(){
        running = Input.GetButton("Run");
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

    public float GetMaxHorzSpeed(){
        return speed;
    }

    void CalculateMovement(){
        if(cc.isGrounded){
            moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            if(moveDirection == Vector3.zero){
                moveDirection = lastVelocity.normalized;
                moveDirection *= Mathf.Lerp(0, speed, accelerationTimer);
                accelerationTimer -= Time.deltaTime / timeToMaxSpeed;
            }
            else{
                moveDirection = transform.TransformDirection(moveDirection).normalized;
                moveDirection *= Mathf.Lerp(0, speed, accelerationTimer);
                accelerationTimer += Time.deltaTime / timeToMaxSpeed;
                if(running)
                    moveDirection *= runMultiplier;
            }
            accelerationTimer = Mathf.Clamp(accelerationTimer, 0, 1);
        }
        else{
            moveDirection = lastVelocity;
            moveDirection.y -= Mathf.Clamp(gravity * Time.deltaTime, 0, maxFallSpeed);
        }

        lastVelocity = moveDirection;
        //print(moveDirection);
        cc.Move(moveDirection * Time.deltaTime);
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
		"Speed: " + System.Math.Round(GetCurrentSpeed(), 4) + 
		"\nX/Y Speed: " + System.Math.Round(GetCurrentHorzSpeed(), 4) +
		(isGrounded() ? "\nGrounded" : ""));
	}
}
