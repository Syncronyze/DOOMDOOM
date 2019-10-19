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
    GunInventoryController inventoryController;
    PlayerMovementController movementController;
    // behaviours:
        // death, hit, pickup HP, pickup ammo, pickup weapon, pickup armor, buffs

    void Start(){
        cameraController = GetComponent<CameraController>();
        healthController = GetComponent<HealthController>();
        movementController = GetComponent<PlayerMovementController>();
        // not a direct component of player due to it needing to be attached to MainCamera, but managed as though it were.
        inventoryController = GameObject.FindGameObjectWithTag("PlayerInventory").GetComponent<GunInventoryController>();
        
        LoadPlayerState();
    }

    public bool AddAmmo(AmmoType type, int ammoToAdd){
        return inventoryController.AddAmmo(type, ammoToAdd);
    }

    public bool AddGun(string gunName, int ammoToAdd){
        return inventoryController.AddGun(gunName, ammoToAdd);
    }

    public bool AddBackpack(){
        return  inventoryController.IncreaseMaxAmmo(2) || 
                inventoryController.AddAmmo(AmmoType.Bullet, 10) ||
                inventoryController.AddAmmo(AmmoType.Shell, 4) ||
                inventoryController.AddAmmo(AmmoType.Rocket, 1) ||
                inventoryController.AddAmmo(AmmoType.Cell, 20);
    }

    public bool AddHealth(int health){
        return healthController.AddHealth(health);
    }

    public bool PlayerInvunerability(){
        return healthController.Invunerability();
    }

    public void DamagePlayer(int damage){
        healthController.TakeDamage(damage);
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
}
