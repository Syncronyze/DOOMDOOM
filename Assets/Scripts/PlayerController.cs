using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 *  A centralized & catch-all controller that entities will interact with directly, instead of the individual player components
 */
public class PlayerController : MonoBehaviour
{
    CameraController cameraController;
    HealthController healthController;
    PlayerInventoryController inventoryController;
    MovementController movementController;
    GameUIController uiController;
    UIColorOverlayController colorOverlay;
    UIInfoSectionController infoSection;

    [Range(0, 1)]
    public float timeSlowTest;
    
    public Color pickupScreenColor;

    bool slowTime;
    bool pause;

    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        cameraController = transform.GetComponent<CameraController>();
        healthController = transform.GetComponent<HealthController>();
        movementController = transform.GetComponent<MovementController>();
        
        // not a direct component of player due to it needing to be attached to MainCamera, but managed as though it were.
        inventoryController = transform.GetComponentInChildren<PlayerInventoryController>();

        // ui components minus anything relating directly to the inventory (ammo, guns)
        uiController = transform.GetComponentInChildren<GameUIController>();
        colorOverlay = transform.GetComponentInChildren<UIColorOverlayController>();
        infoSection = transform.GetComponentInChildren<UIInfoSectionController>();
        
        LoadPlayerState();

        //Application.targetFrameRate = 15;
    }

    void FixedUpdate(){
        if(!pause){
            UpdateMovementInput();
        }  
    }

    void Update(){
        if(!pause){
            UpdateUI();
            UpdateFiringInput();
        }
        

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f; // fps counter delete later

        if(Input.GetKeyDown(KeyCode.Escape)){
            if(!pause){
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                pause = true;
            }
            else{
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                pause = false;
            }
        }

        if(slowTime)
            Time.timeScale = timeSlowTest;
            
        if(Input.GetKeyDown(KeyCode.T)){
            if(!slowTime){
                slowTime = true;
            }
            else{
                Time.timeScale = 1f;
                slowTime = false;
            }
        }

        if(healthController.health <= 0)
            DETH();

        
            // a hit with a higher percent of current health will make it increasingly red
            // and a hit with higher percent of total normal health will make it last longer
        if(healthController.damagedBy != null){
            colorOverlay.SetColor(  Color.red, 
                                    1 - ((float)healthController.health / (float)(healthController.health + healthController.damageAmountHealth)), 
                                    healthController.damageAmountHealth / 100f, 0, 0.33f);
        }
    }

    public bool AddAmmo(AmmoType type, int ammoToAdd, string sourceName){
        bool pickedUp = inventoryController.AddAmmo(type, ammoToAdd);
        if(pickedUp){
            colorOverlay.SetColor(pickupScreenColor, .25f, .25f, .25f, 1);
            infoSection.SetMessage($"Picked up {sourceName}.");
        }

        return pickedUp;
    }

    public bool AddGun(string gunName, int ammoToAdd, string sourceName){
        bool pickedUp = inventoryController.AddGun(gunName, ammoToAdd);
        if(pickedUp){
            colorOverlay.SetColor(pickupScreenColor, .25f, .25f, .25f, 1);
            infoSection.SetMessage($"You got a {sourceName}!");
        }

        return pickedUp;
    }

    public bool AddBackpack(){
        bool pickedUp = inventoryController.IncreaseMaxAmmo(2) || 
                inventoryController.AddAmmo(AmmoType.Bullet, 10) ||
                inventoryController.AddAmmo(AmmoType.Shell, 4) ||
                inventoryController.AddAmmo(AmmoType.Rocket, 1) ||
                inventoryController.AddAmmo(AmmoType.Cell, 20);

        if(pickedUp){
            colorOverlay.SetColor(pickupScreenColor, .25f, .25f, .25f, 1);
            infoSection.SetMessage($"You got a backpack!");
        }

        return  pickedUp;
    }

    public bool HealPlayer(int health){
        if(healthController.health >= healthController.maxHealth)
            return false;
        
        infoSection.SetMessage($"Picked up a health bonus.");
        colorOverlay.SetColor(pickupScreenColor, .25f, .25f, .25f, 1);
        healthController.AddHealth(health);
        return true;
    }

    public bool AddArmor(int armor){
        if(armor >= healthController.maxArmor)
            return false;

        infoSection.SetMessage($"Picked up an armor bonus.");
        colorOverlay.SetColor(pickupScreenColor, .25f, .25f, .25f, 1);
        healthController.AddArmor(armor);
        return true;
    }

    public bool PlayerInvunerability(){
        return healthController.Invunerability();
    }

    public void SavePlayerState(){
        healthController.SaveGlobalVariables();
        inventoryController.SaveGlobalVariables();

        GlobalPlayerVariables.save = true;
    }

    public void LoadPlayerState(){
        healthController.LoadGlobalVariables();
        inventoryController.LoadGlobalVariables();
    }

    void UpdateFiringInput(){
        inventoryController.FireActiveGun(Input.GetButton("Fire1"));
    }

    void UpdateUI(){
        uiController.SetValue("UI_HealthPos", (int)Mathf.Clamp(healthController.health, 0f, 999f) + "%");
        uiController.SetValue("UI_ArmorPos", healthController.armor + "%");
        inventoryController.UpdateGUI();
    }

    void DETH(){

    }

    void UpdateMovementInput(){
        if(Input.GetButton("Run")){
            movementController.SetSpeedMultiplier(2);
        }
        else{
            movementController.SetSpeedMultiplier(1);
        }
        
        movementController.SetMovementVector(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")));
    }

    
    /*
	 * 	TODO: REMOVE, ONLY FOR DEBUGGING.
	 */
     
	float deltaTime = 0.0f;
	void OnGUI(){
		GUI.Label(new Rect(10, 600, 400, 80), 
		"Speed: " + System.Math.Round(movementController.GetCurrentSpeed(), 5) + 
		"\nX/Y Speed: " + System.Math.Round(movementController.GetCurrentHorzSpeed(), 5) +
		(movementController.isGrounded() ? "\nGrounded" : "\n"));

        // fps stuff
		int w = Screen.width, h = Screen.height;
 
		GUIStyle style = new GUIStyle();
 
		Rect rect = new Rect(0, 0, w, -h * 2 / 100);
		style.alignment = TextAnchor.UpperCenter;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
	}
}
