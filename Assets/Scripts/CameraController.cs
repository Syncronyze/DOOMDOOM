using UnityEngine;
using System.Collections;


public class CameraController : MonoBehaviour {

	public Transform mainCamera;

	public float mouseSensitvity;
	private float rotationYVelocity, cameraXVelocity;

	float wantedYRotation;
	float currentYRotation;
	float wantedCameraXRotation;
	float currentCameraXRotation;

	// max view angles
	public float topAngleView = 89;
	public float bottomAngleView = -89;

	//[HideInInspector]
	//public GameObject weapon; // Current weapon that player carries

    Texture2D xhair;
    Rect xhairRect;

	[Range(0, 4)]
	public float headBobSpeed;
	public float lowestHeadHeight = -0.001f;
	float defaultHeadHeight = 0;
	float cameraOffset;
	static float bobTimer = 0.0f;
	

	void Awake(){
		Cursor.lockState = CursorLockMode.Locked;
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
		cameraOffset = mainCamera.localPosition.y; // before we begin, we want to maintain the camera's Y offeset

		xhair = Resources.Load<Texture2D>("Textures/xhair");
		float xhairWidth = 4;
        float xhairHeight = 4;
        float xMin = (Screen.width / 2) - (xhairWidth / 2);
        float yMin = (Screen.height / 2) - (xhairHeight / 2);

        xhairRect = new Rect(xMin, yMin, xhairWidth, xhairHeight);
	}

	void Update(){
		MouseInputMovement();
		HeadBob ();	

	}

	void FixedUpdate(){
		RotateCamera();
	}

	/*
	 * Applying the headbob to the camera
	 */
	void HeadBob(){
		mainCamera.transform.localPosition = new Vector3(0, Mathf.Lerp(defaultHeadHeight, lowestHeadHeight, bobTimer) + cameraOffset, 0);
		// multiplying the timer by the speed of the player, halved and clamped with a max to ensure we can disable if we need.
		// setting headBobSpeed to 0 will disable head bob entirely.
        bobTimer += Mathf.Clamp(GetComponent<PlayerMovementController>().GetCurrentHorzSpeed() * 0.5f, 0, headBobSpeed) * Time.deltaTime;

		// if the timer has run its course, we flip the variables and reset the timer.
        if (bobTimer > 1.0f)
        {
            float temp = lowestHeadHeight;
            lowestHeadHeight = defaultHeadHeight;
            defaultHeadHeight = temp;
            bobTimer = 0.0f;
        }
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
