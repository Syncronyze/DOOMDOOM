using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    CharacterController cc;
    public float speed = 6.0f;
    public float gravity = 20.0f;
    public float maxFallSpeed = 40.0f;
    public float runMultiplier = 2.0f;
    public float airMultiplier = 0.75f;

    private Vector3 lastVelocity;
    private Vector3 moveDirection;
	private LayerMask ignoreLayer;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        ignoreLayer = 1 << LayerMask.NameToLayer ("Player");
    }

    void Update()
    {
        float targetSpeed = speed;

        if(Input.GetKey(KeyCode.LeftShift))
            targetSpeed *= runMultiplier;

        // gravity
        if(isGrounded()){
            moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection).normalized;
            moveDirection *= targetSpeed;
        }
        else{
            moveDirection = lastVelocity;
            moveDirection.y -= Mathf.Clamp(gravity * Time.deltaTime, 0, maxFallSpeed);
        }

        lastVelocity = moveDirection;

        cc.Move(moveDirection * Time.deltaTime);
    }

    public bool isGrounded(){
        if(cc.isGrounded)
            return true;
        
        return isGroundedRaycast();
    }

    bool isGroundedRaycast(){
		RaycastHit rcHit;
		if(Physics.Raycast(transform.position, Vector3.down, out rcHit, 2f, ~ignoreLayer)){
			if(rcHit.transform != null)
				return true;
			else
				return false;
		}
		return false;
	}

    
	public float GetCurrentSpeed(){
		return cc.velocity.magnitude;
	}

	public float GetCurrentHorzSpeed(){
		return new Vector2 (cc.velocity.x, cc.velocity.z).magnitude;
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
