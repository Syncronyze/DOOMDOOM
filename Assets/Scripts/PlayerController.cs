using System.Collections;
using System.Collections.Generic;
using System;
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

    public float useDistance;

    bool slowTime;
    bool noclip;

    void Start(){
        #if UNITY_EDITOR
        if(GlobalPlayerVariables.save == null){
            print("Loading fake debug save..");
            GlobalPlayerVariables.LoadFakeDebugSave();
        }
        #endif
        
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
        uiController.SetValue("UI_HealthPos", (int)Mathf.Clamp(healthController.health, 0f, 999f) + "%");
        uiController.SetValue("UI_ArmorPos", (int)Mathf.Clamp(healthController.armor, 0f, 999f) + "%");
        inventoryController.UpdateGUI();
        //Application.targetFrameRate = 15;
    }

    void FixedUpdate(){
        if(Time.timeScale > 0f){
            UpdateMovementInput();
        }  
    }

    void Update(){
        if(Time.timeScale > 0f){
            UpdateUI();
            UpdateFiringInput();
            UpdateMiscInput();
        }

        if(Input.GetKeyDown(KeyCode.Escape)){
            if(UIMainMenuController.instanceOf == null)
                Time.timeScale = Time.timeScale > 0f ? 0f : 1f;
            else
                UIMainMenuController.instanceOf.TogglePauseMenu();
        }
        

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f; // fps counter delete later

        if(healthController.health <= 0)
            DETH();

        // a hit with a higher percent of current health will make it increasingly red
        // and a hit with higher percent of total normal health will make it last longer
        if(healthController.damagedBy != null){
            colorOverlay.SetColor(  Color.red, 
                                    1 - ((float)healthController.health / (float)(healthController.health + healthController.damageAmountHealth)), 
                                    healthController.damageAmountHealth / 100f, 0, 0.33f);
        }

        if(movementController.totalAirTime > 0.2f){
            // sound effect OOF needs to be here
            print("OOF");
            movementController.ResetAirTime();
        }
    }

    public void EndLevel(){
        if(UIMainMenuController.instanceOf == null){
            Debug.LogWarning("There's no main menu to fall back on meaning there is no known next level.");
            return;
        }
        SavePlayerState();
        GlobalLevelVariables.instanceOf.LevelEnded();
        UIMainMenuController.instanceOf.EndLevel();
        movementController.enabled = false;
        cameraController.enabled = false;
        healthController.enabled = false;
        this.enabled = false;
    }

    public bool AddAmmo(string[] parameters){
        AmmoType type;
        int ammoToAdd;

        if(parameters.Length != 3){
            Debug.LogError($"Couldn't pickup 'ammo' due to incorrect parameter amount; requires 3 but contains {parameters.Length}");
            return false;
        }

        try{
            type = (AmmoType)Enum.Parse(typeof(AmmoType), parameters[0]);
            ammoToAdd = Int32.Parse(parameters[1]);
        }
        catch(Exception e){
            Debug.LogError($"Couldn't parse pickup 'ammo' \"{parameters[0]}, {parameters[1]}, {parameters[2]}\" \n {e.Message} \n\n {e.StackTrace}");
            return false;
        }
        bool pickupAcquired = AddAmmo(type, ammoToAdd, parameters[2]);
        if(pickupAcquired)
            GlobalLevelVariables.instanceOf.PickupAcquired();
            
        return pickupAcquired;
    }

    bool AddAmmo(AmmoType type, int ammoToAdd, string sourceName){
        if(inventoryController.AddAmmo(type, ammoToAdd)){
            colorOverlay.SetColor(pickupScreenColor, .25f, .25f, .25f, 1);
            infoSection.SetMessage($"Picked up {sourceName}.");
            return true;
        }

        return false;
    }

    public bool AddGun(string[] parameters){
        int ammoToAdd;

        if(parameters.Length != 3){
            Debug.LogError($"Couldn't pickup 'gun' due to incorrect parameter amount requires 3 but contains {parameters.Length}");
            return false;
        }

        try{
            ammoToAdd = Int32.Parse(parameters[1]);
        }
        catch(Exception e){
            Debug.LogError($"Couldn't parse pickup 'gun' \"{parameters[0]}, {parameters[1]}, {parameters[2]}\" \n {e.Message} \n\n {e.StackTrace}");
            return false;
        }
        bool pickupAcquired = inventoryController.AddGun(parameters[0], ammoToAdd);

        if(pickupAcquired){
            colorOverlay.SetColor(pickupScreenColor, .25f, .25f, .25f, 1);
            infoSection.SetMessage($"You got a {parameters[2]}!");
            GlobalLevelVariables.instanceOf.PickupAcquired();
        }
            
        return pickupAcquired;
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
            GlobalLevelVariables.instanceOf.PickupAcquired();
        }

        return pickedUp;
    }

    public bool HealPlayer(string[] healthStr){
        int health;

        if(healthStr.Length == 0 || !Int32.TryParse(healthStr[0], out health)){
            Debug.LogError($"Couldn't pickup 'heal' as there are no parameters on the trigger or they cannot be parsed.");
            return false;
        }

        bool pickupAcquired = HealPlayer(health);
        if(pickupAcquired)
            GlobalLevelVariables.instanceOf.PickupAcquired();

        return pickupAcquired;
    }

    bool HealPlayer(int health){
        if(healthController.health >= healthController.maxHealth)
            return false;
        
        infoSection.SetMessage("Picked up a health bonus.");
        colorOverlay.SetColor(pickupScreenColor, .25f, .25f, .25f, 1);
        healthController.AddHealth(health);
        return true;
    }

    public bool AddArmor(string[] armorStr){
        int armor;

        if(armorStr.Length == 0 || !Int32.TryParse(armorStr[0], out armor)){
            Debug.LogError("Couldn't pickup 'armor' as there are no parameters or they cannot be parsed.");
            return false;
        }
        bool pickupAcquired = AddArmor(armor);
        if(pickupAcquired)
            GlobalLevelVariables.instanceOf.PickupAcquired();

        return pickupAcquired;
    }

    bool AddArmor(int armor){
        
        if(healthController.armor >= healthController.maxArmor)
            return false;

        infoSection.SetMessage("Picked up an armor bonus.");
        colorOverlay.SetColor(pickupScreenColor, .25f, .25f, .25f, 1);
        healthController.AddArmor(armor);
        return true;
    }

    public bool ChangeArmor(string[] armorStr){
        ArmorType type;
        if(armorStr.Length == 0 || !Enum.TryParse<ArmorType>(armorStr[0], true, out type)){
            Debug.LogError("Couldn't pickup 'armor' as there are no parameters or they cannot be parsed.");
            return false;
        }

        bool pickupAcquired = healthController.ChangeArmor(type);
        if(pickupAcquired)
            GlobalLevelVariables.instanceOf.PickupAcquired();
        
        if(pickupAcquired){
            infoSection.SetMessage("Picked up the armor.");
            colorOverlay.SetColor(pickupScreenColor, .25f, .25f, .25f, 1);
        }
        
        return pickupAcquired;
    }

    public bool PlayerInvunerability(){
        return healthController.Invunerability();
    }

    public void SavePlayerState(){
        healthController.SaveGlobalVariables();
        inventoryController.SaveGlobalVariables();
    }

    public void LoadPlayerState(){
        healthController.LoadGlobalVariables();
        inventoryController.LoadGlobalVariables();
    }

    void UpdateFiringInput(){
        inventoryController.FireActiveGun(Input.GetButton("Fire1"));
    }

    void UpdateUI(){
        if(healthController.healthOrArmorChanged){
            uiController.SetValue("UI_HealthPos", (int)Mathf.Clamp(healthController.health, 0f, 999f) + "%");
            uiController.SetValue("UI_ArmorPos", (int)Mathf.Clamp(healthController.armor, 0f, 999f) + "%");
        }
        
        inventoryController.UpdateGUI();
    }

    void DETH(){

    }

    void UpdateMiscInput(){
        if(Input.GetButtonDown("Use")){
            //colorOverlay.SetColor(Color.red, 1, 1f);
            RaycastHit[] hits = Physics.RaycastAll(cameraController.mainCamera.transform.position, cameraController.mainCamera.transform.TransformDirection(Vector3.forward), useDistance, ~(1 << gameObject.layer));

            for(int i = 0; i < hits.Length; i++){
                if(hits[i].transform.tag == "Trigger"){
                    hits[i].collider.GetComponent<MultiTriggerController>().Interact(gameObject);
                    break;
                }
            }
        }


        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.X)){
            if(!noclip){
                noclip = true;
                gameObject.layer = LayerMask.NameToLayer("NoCollision");
                movementController.Fly(true);
                cameraController.headBob = false;
            }
            else{
                noclip = false;
                gameObject.layer = LayerMask.NameToLayer("Player");
                movementController.Fly(false);
                cameraController.headBob = true;
            }
        }
        #endif
    }

    void UpdateMovementInput(){
        if(Input.GetButton("Run")){
            movementController.SetSpeedMultiplier(2);
        }
        else{
            movementController.SetSpeedMultiplier(1);
        }
        Vector3 movementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        
        if(noclip && movementInput != Vector3.zero){ // if the input is zeroed out, we want to come to a complete stop in noclip
            if(movementInput.z != 0){ // if we're only moving horizontally, it doesn't make sense to change height
                if(movementInput.z < 0) // if we're moving backwards, we move vertically in the opposite direction the camera is facing
                    movementInput.y = cameraController.mainCamera.TransformDirection(Vector3.back).y;
                else
                    movementInput.y = cameraController.mainCamera.TransformDirection(Vector3.forward).y;
            }

            movementController.SetMovementVector(movementInput);
        }
        else{
            movementController.SetMovementVector(movementInput);
        }
            
    
    }

    
    /*
	 * 	TODO: REMOVE, ONLY FOR DEBUGGING.
	 */
     
	float deltaTime = 0.0f;
    
	// void OnGUI(){
	// 	GUI.Label(new Rect(10, 600, 400, 80), 
	// 	"Speed: " + System.Math.Round(movementController.GetCurrentSpeed(), 5) + 
	// 	"\nX/Y Speed: " + System.Math.Round(movementController.GetCurrentHorzSpeed(), 5) +
	// 	(movementController.isGrounded() ? "\nGrounded" : "\n"));

    //     // fps stuff
	// 	int w = Screen.width, h = Screen.height;
 
	// 	GUIStyle style = new GUIStyle();
 
	// 	Rect rect = new Rect(0, 0, w, -h * 2 / 100);
	// 	style.alignment = TextAnchor.UpperCenter;
	// 	style.fontSize = h * 2 / 100;
	// 	style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
	// 	float msec = deltaTime * 1000.0f;
	// 	float fps = 1.0f / deltaTime;
	// 	string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
	// 	GUI.Label(rect, text, style);
	// }
}
