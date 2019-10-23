using UnityEngine;
using System.Collections;


public class CameraController : MonoBehaviour {

	public Transform mainCamera;

	// headbob variables
	public float headBobSpeed = 4;
	public float bobMovementScale = 0.0625f;
	public float headBobResetSpeed = 8;
	public float headBobDistance = 0.5f;
	// first person look variables
	public float mouseSensitvity = 1;
	public float topAngleView = 89;
	public float bottomAngleView = -89;

	PlayerMovementController playerController;
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

	bool resettingCamera;

	float bobTimer = 0.0f;
	

	void Awake(){
		Cursor.lockState = CursorLockMode.Locked;
		playerController = GetComponent<PlayerMovementController>();
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
		cameraOffset = mainCamera.localPosition.y; // before we begin, we want to maintain the camera's Y offeset
		movingCamHeightFrom = 0;
		movingCamHeightTo = headBobDistance;
		resettingCamera = false;

		xhair = Resources.Load<Texture2D>("Textures/xhair");
		float xhairWidth = 4;
        float xhairHeight = 4;
        float xMin = (Screen.width / 2) - (xhairWidth / 2);
        float yMin = (Screen.height / 2) - (xhairHeight / 2);

        xhairRect = new Rect(xMin, yMin, xhairWidth, xhairHeight);
	}

	void Update(){
		MouseInputMovement();
		RotateCamera();
		if(headBobSpeed > 0)
			HeadBob ();	

	}

	void FixedUpdate(){
		//RotateCamera();
	}

	/*
	 * Applying the headbob to the camera
	 */
	void HeadBob(){
		float currentSpeed = playerController.GetCurrentHorzSpeed();
		float speedPercentage = currentSpeed * bobMovementScale;

		if(!playerController.isGrounded())
			return;
		
		// to smooth the headbob, we're only applying headbob at 50% speed
		if(speedPercentage > 0.5){
			// if we came from camera reset, we have to ensure we're starting fresh
			if(resettingCamera){
				resettingCamera = false;
				bobTimer = 0;
				movingCamHeightFrom = 0;
				movingCamHeightTo = headBobDistance;
			}

			// if the timer has run its course, we flip the variables and reset the timer.
			if (bobTimer > 1){
				float temp = movingCamHeightTo;
				movingCamHeightTo = movingCamHeightFrom;
				movingCamHeightFrom = temp;
				bobTimer = 0;
			}
			
			bobTimer += Time.deltaTime * headBobSpeed * speedPercentage;
		}
		else{
			if(!resettingCamera){
				float currentCamHeight = mainCamera.transform.localPosition.y - cameraOffset;
				movingCamHeightTo = 0;
				movingCamHeightFrom = headBobDistance; 
				// bobTimer represents a decimal percentage of where we are
				// thus, we set where we currently are in relation to the lowest possible camera height in % form
				bobTimer = 1 - Mathf.Abs(currentCamHeight / headBobDistance);
				//print($"Moving Camera from {movingCamHeightFrom} to {movingCamHeightTo}, and at {(bobTimer * 100)}%");
				resettingCamera = true;
			}
			else if(bobTimer > 1.0f){ // if we're not moving, and we've already reset the camera, we're done here
				return;
			}
			bobTimer += Time.deltaTime * headBobResetSpeed;
		}
		
		//print($"Moving Camera from {movingCamHeightFrom} to {movingCamHeightTo}, and at {(bobTimer * 100)}%");
		mainCamera.transform.localPosition = new Vector3(0, Mathf.Lerp(movingCamHeightFrom, movingCamHeightTo, bobTimer) + cameraOffset, 0);
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

	/*
	 * Drawing our crosshair - TODO: DELETE THIS AND MOVE TO PROPER CANVAS OBJECT LATER
	 */
	void OnGUI(){
		if(xhair != null)
			GUI.DrawTexture(xhairRect, xhair, ScaleMode.StretchToFill, true, 0, new Color(0, 1, 0, 0.75f), 6f, 6f);
	}

}
