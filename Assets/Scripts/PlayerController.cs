using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    CameraController cameraController;
    HealthController healthController;
    GunInventoryController inventoryController;
    PlayerMovementController movementController;

    void Start(){
        cameraController = GetComponent<CameraController>();
        healthController = GetComponent<HealthController>();
        //inventoryController = GetComponent<GunInventoryController>();
        movementController = GetComponent<PlayerMovementController>();
    }

    public bool AddAmmo(AmmoType type, int ammoToAdd){
        // TODO: play sound here
        return true;//inventoryController.AddAmmo(type, ammoToAdd);
    }

}
