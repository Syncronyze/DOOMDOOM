using UnityEngine;
using System.Collections;


public class CameraController : MonoBehaviour {

	public Transform mainCamera;

	// headbob variables
	public float headBobSpeed = 4;
	public float headBobResetSpeed = 8;
	public float headBobDistance = 0.5f;
	public bool headBob = true;
	// first person look variables
	public float mouseSensitvity = 1;
	public float topAngleView = 89;
	public float bottomAngleView = -89;

	

	MovementController playerController;
	Texture2D xhair;
    Rect xhairRect;

	// first person look variables
	float wantedYRotation;
	float currentYRotation;
	float wantedCameraXRotation;
	float currentCameraXRotation;
	float rotationYVelocity;
	float cameraXVelocity;
	// headbob variables
	float movingCamHeightTo;
	float movingCamHeightFrom;
	float cameraOffset;

	bool bobbing;

	float bobTimer;
	

	void Awake(){
		playerController = GetComponent<MovementController>();
		cameraOffset = mainCamera.localPosition.y; // before we begin, we want to maintain the camera's Y offeset
		movingCamHeightFrom = 0;
		movingCamHeightTo = headBobDistance;
		bobbing = false;
		bobTimer = 0;

		// xhair = Resources.Load<Texture2D>("Textures/xhair");
		// float xhairWidth = 4;
        // float xhairHeight = 4;
        // float xMin = (Screen.width / 2) - (xhairWidth / 2);
        // float yMin = (Screen.height / 2) - (xhairHeight / 2);

        // xhairRect = new Rect(xMin, yMin, xhairWidth, xhairHeight);
	}

	void Update(){
		if(Time.timeScale == 0) // still records mouse input even while paused
			return;

		MouseInputMovement();
		if(headBob && headBobSpeed > 0){
			HeadBob();	
		}

	}

	void FixedUpdate(){
		RotateCamera();
	}

	/*
	 * Applying the headbob to the camera
	 */
	void HeadBob(){
		float speedPercentage = playerController.GetCurrentSpeedPercentage();

		if(!playerController.isGrounded())
			return;
		
		// to smooth the headbob, we're only applying headbob at 10% speed
		if(speedPercentage > 0.2){
			// if we're not at "default" position, then we skip this and continue with the current bobTimer
			if(!bobbing && mainCamera.localPosition.y == cameraOffset){
				bobbing = true;
				bobTimer = 0;
				movingCamHeightFrom = 0;
				movingCamHeightTo = headBobDistance;
			}

			// if the timer has run its course, we flip the variables and reset the timer.
			if(bobTimer > 1){
				float temp = movingCamHeightFrom;
				// we've finished moving TO zero, then we move the opposite direction
				if(movingCamHeightTo == 0)
					temp *= -1;

				movingCamHeightFrom = movingCamHeightTo;
				movingCamHeightTo = temp;
				bobTimer = 0;
			}
			
			bobTimer += Time.deltaTime * headBobSpeed;// * speedPercentage;
		}
		else{
			if(bobbing){
				bobbing = false;
				// if we're already moving to zero, then we just continue moving that way
				if(movingCamHeightTo != 0){
					movingCamHeightFrom = movingCamHeightTo;
					movingCamHeightTo = 0;
					bobTimer = 1 - bobTimer;
				}
			}
			
			if(bobTimer > 1.0f){ // if we're not moving, and we've already reset the camera, we're done here
				return;
			}

			bobTimer += Time.deltaTime * headBobResetSpeed;
		}

		mainCamera.localPosition = new Vector3(0, (Mathf.Lerp(movingCamHeightFrom, movingCamHeightTo, bobTimer)  * speedPercentage) + cameraOffset, 0);

		//if(bobTimer >= 1)
		//	print($"Moving Camera from {movingCamHeightFrom} to {movingCamHeightTo}, and at {(bobTimer * 100)}% ({mainCamera.localPosition.y})");
	}

	/*
	 * Calculating the user's wanted mouse movement; doesn't update the camera yet. Gimbal locks up & down to prevent weirdness from occuring.
	 */
	void MouseInputMovement(){

		wantedYRotation += Input.GetAxisRaw("Mouse X") * mouseSensitvity;
		wantedCameraXRotation -= Input.GetAxisRaw("Mouse Y") * mouseSensitvity;
		// clamping X rotation to prevent weirdness
		wantedCameraXRotation = Mathf.Clamp(wantedCameraXRotation, bottomAngleView, topAngleView);

	}

	/*
	 * Calculating the camera rotation
	 */
	void RotateCamera(){

		currentYRotation = Mathf.SmoothDamp(currentYRotation, wantedYRotation, ref rotationYVelocity, 0);
		currentCameraXRotation = Mathf.SmoothDamp(currentCameraXRotation, wantedCameraXRotation, ref cameraXVelocity, 0);

		transform.rotation = Quaternion.Euler(0, currentYRotation, 0);
		mainCamera.localRotation = Quaternion.Euler(currentCameraXRotation, 0, 0);

	}
}
