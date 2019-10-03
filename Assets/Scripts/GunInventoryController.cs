using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/**
 *      Class to store all of the gun and ammo related information. All ammo is stored within the inventory, as it's shared between guns.
 */
public class GunInventoryController : MonoBehaviour
{
    public GameUIController uiController;
    
    List<GunController> guns;
    List<Ammo> ammos;
    Ammo activeAmmo;
    int activeGunIndex;
    bool valid;

    List<UIFontController> ammoDisplays;
    UIFontController activeAmmoDisplay;

    // when the inventory has an update and needs to be refreshed
    [HideInInspector]
    public bool inventoryRefresh;

    void Awake(){
        guns = new List<GunController>();
        ammoDisplays = new List<UIFontController>();
        activeGunIndex = 0;
        inventoryRefresh = true;

        if(!uiController.HasControllers())
            uiController.CollectUIControllers();

        activeAmmoDisplay = uiController.GetFontController("UI_AmmoPos");
        CreateAmmos();
        // TODO: Remove, just for testing
        AddAmmo(AmmoType.Bullet, 500);
        AddAmmo(AmmoType.Rocket, 500);
        SetActiveGun(); 
    }

    void Update(){
        RefreshInventory();
        if(UserWeaponSwitch())
            SetActiveGun();
        
        UpdateGUI();
    }

    /*
     * We do the ammo in the inventory because the gun ammo is NOT tied to the gun in Doom, it is tied to the overall inventory.
     */
    void CreateAmmos(){
        ammos = new List<Ammo>();
        foreach(AmmoType type in Enum.GetValues(typeof(AmmoType))){
            Ammo ammo = new Ammo(type);
            string uiString = "UI_Ammo" + type.ToString();
            ammos.Add(ammo);
            
            // if it's less than 0, we display blank since it's infinite ammo
            uiController.SetValue(uiString + "MaxPos", ammo.max < 0 ? "" : ammo.max + "");

            UIFontController fc = uiController.GetFontController(uiString + "Pos");
            ammoDisplays.Add(fc);
            
        }
    }

    /*
     *  Adding ammo based on type.
     */
    public void AddAmmo(AmmoType type, int amountToAdd){
        foreach(Ammo a in ammos){
            if(a.type == type){
                a.AddAmmo(amountToAdd);
                return;
            }
        }
        print($"Cannot add ammo of type {type.ToString()} as it doesn't exist in our inventory!");
    }

    /*
     *  Goes through and updates all the Ammo UI pieces, SetValue doesn't update if the value is identical.
     */
    void UpdateGUI(){
        int iterator = 1;
        if( ammoDisplays == null || 
            ammoDisplays.Count == 0 || 
            activeAmmoDisplay == null || 
            ammos.Count == 0 || 
            activeAmmo == null)
            return;

        foreach(UIFontController cont in ammoDisplays){
            if(cont == null)
                continue;

            cont.SetValue(ammos[iterator].count + "");
            iterator++;
        }

        activeAmmoDisplay.SetValue(activeAmmo.count < 0 ? "" : activeAmmo.count + "");                  
    }

    /*
     *  Setting active gun based on the activeGunIndex, and setting all others to inactive.
     */
    void SetActiveGun(){
        for(int i = 0; i < guns.Count; i++){
            GunController gun = guns[i];
            // deleting a null if this is one.
            if(gun == null){
                guns.RemoveAt(i);
                if(guns.Count == 0)
                    break;
                i--;
                continue;
            }
            
            if(i == activeGunIndex){
                activeGunIndex = i;
                gun.SetActive(true);
                activeAmmo = ammos[(int)gun.GetAmmoType()];
            }
            else{
                gun.SetActive(false);
            }
            
        }
    }

    /*
     * Runs every frame and checks if the player switched weapons using scrollwheel or numbers.
     */
    bool UserWeaponSwitch(){

        // checking scroll wheel first
        if(Input.GetAxis("Mouse ScrollWheel") > 0f){
            // guns start counting at 0, so we subtract 1 and ensure that its still less than that count to get the last gun.
            if(activeGunIndex < guns.Count - 1)
                activeGunIndex++;
            else
                activeGunIndex = 0;
                
            return true;
        }

        if(Input.GetAxis("Mouse ScrollWheel") < 0f){
            // if the active gun is NOT index 0, then we go down 1
            if(activeGunIndex > 0)
                activeGunIndex--;
            else
                activeGunIndex = guns.Count - 1;

            return true;
        }
        // checking for only the numbers we have
        // TODO: in the future, possible rebinding?
        for(int i = 0; i < guns.Count; i++){
            // 1 on the keyboard is 0 on the gun index
            if(Input.GetKeyDown("" + (i + 1))){
                activeGunIndex = i;
                return true;
            }
        }
        return false;
    }

    /*
     * Refreshes the gun list when we pickup weapons
     */
    void RefreshInventory(){
        if(inventoryRefresh){
            int iterator = 0;
            foreach(Transform child in transform){
                GunController gc;
                if(child.TryGetComponent<GunController>(out gc)){
                    try{
                        Ammo ammo = ammos.First(a => a.type == gc.GetAmmoType());
                        gc.SetAmmo(ref ammo);
                    }
                    catch(Exception e){
                        print("No ammo type available for " + gc.gunName + "\n" + e.Message + "\n StackTrace: " + e.StackTrace);
                    }
                    uiController.ChangeStyle("UI_Arms" + iterator + "Pos", FontStyle.Go);
                    guns.Add(gc);
                    iterator++;
                }
            }
            // after we've refreshed the inventory, we'll disable inventory refreshing until we need it again.
            inventoryRefresh = false;
        }
    }

    /*
     * TODO: FOR DEBUGGING ONLY, REMOVE LATER.
     */
    void OnGUI(){
		GUI.Label(new Rect(10, 90, 750, 1600), $"Current Active Weapon: {guns[activeGunIndex].name} ({activeGunIndex}) \n Guns: \n {ListToString(guns)}");//\n {ListToString(ammos)}");
	}

    /*
     * Quick and dirty print list function
     */
    public static string ListToString<T>(List<T> objs){
		string returnStr = "";

		foreach (T o in objs) {
			returnStr += o.ToString() + "\n ";
		}
		return returnStr;
	}

    
}

