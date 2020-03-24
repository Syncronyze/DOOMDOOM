using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

/**
 *      Class to store all of the gun and ammo related information. All ammo is stored within the inventory, as it's shared between guns.
 */
public class PlayerInventoryController : MonoBehaviour
{
    public GameUIController uiController;
    public UIViewSpriteController uiViewSprite;

    public bool inventoryUpdated{ get; private set; }
    
    List<GunController> guns;
    Ammo[] ammos;
    UIFontController[] ammoDisplays;
    int ammoTypeCount;

    UIFontController activeAmmoDisplay;
    Ammo activeAmmo;
    GunController activeGun;

    int activeGunIndex;
    int ammoMultiplier;
    bool valid;
    bool updateMaxAmmo;


    void Start(){
        activeGunIndex = -1;
        
        if(!uiController.HasControllers())
            uiController.CollectUIControllers();

        activeAmmoDisplay = uiController.GetFontController("UI_AmmoPos");

        for(int i = 0; i < ammos.Length; i++){
            ammoDisplays[i] = uiController.GetFontController("UI_Ammo" + ammos[i].type.ToString() + "Pos");
        }

        inventoryUpdated = true;
        updateMaxAmmo = true;
    }

    void Update(){
        if(Time.timeScale > 0){
            //RefreshInventory();
            if(UserWeaponSwitch()){
                inventoryUpdated = true;
                SetActiveGun();
            }
        }
    }

    public void FireActiveGun(bool fire){
        if(activeGun == null || uiViewSprite == null)
            return;
        
        activeGun.ToggleFiring(fire);
        uiViewSprite.ToggleFiring(activeGun.firing);

        if(activeGun.BulletFiredOnFrame()){
            inventoryUpdated = true;
            uiViewSprite.Fire();
        }
    }

    /*
     *  Adding ammo based on type.
     */
    public bool AddAmmo(AmmoType type, int amountToAdd){
        for (int i = 0; i < ammos.Length; i++){
            Ammo a = ammos[i];
            if(a.type == type){
                inventoryUpdated = true;
                return a.AddAmmo(amountToAdd); 
            }
        }
        
        print($"Cannot add ammo of type {type.ToString()} as it doesn't exist in our inventory!");
        return false;
    }

    // a gun may be found in the world already contained in the inventory of the player; must add the relevant ammo.
    public bool AddGun(string gunName, int ammo){
        GunController gun = null;
        bool addedGun = false;
        for (int i = 0; i < guns.Count; i++){
            GunController gc = guns[i];
            if(gc.gunName == gunName){
                gun = gc;
                break;
            }
        }

        // if this gun wasn't found, we create it and add it to our inventory
        if(gun == null){
            gun = CreateGun(gunName);
            if(gun == null)
                return false;
            
            guns.Add(gun);
            
            uiController.ChangeStyle("UI_Arms" + (guns.Count - 1) + "Pos", FontStyle.Go);
            addedGun = true;
        }

        AmmoType ammoType = gun.GetAmmoType();
        // if we added ammo, OR if the gun wasn't already in our inventory
        addedGun = (AddAmmo(ammoType, ammo) || addedGun);
        inventoryUpdated = addedGun;

        return addedGun;
    }

    /*
     * Increases max ammo by a multiplier given.
     */
    public bool IncreaseMaxAmmo(int multiplier){
        if(multiplier <= 0 || ammos[0].multiplier == multiplier)
            return false;
        
        for (int i = 0; i < ammos.Length; i++){
            ammos[i].IncreaseMaxAmmo(multiplier);
            ammoMultiplier = multiplier;
            updateMaxAmmo = true;
        }

        inventoryUpdated = true;
        return true;
    }

    public void SaveGlobalVariables(){
        GlobalPlayerVariables.save.guns = new string[guns.Count];
        for (int i = 0; i < guns.Count; i++){
            GlobalPlayerVariables.save.guns[i] = guns[i].gunName;
        }

        GlobalPlayerVariables.save.ammos = ammos;
    }

    public void LoadGlobalVariables(){
        guns = new List<GunController>();
        ammoTypeCount = Enum.GetNames(typeof(AmmoType)).Length;
        ammoDisplays = new UIFontController[ammoTypeCount];
        ammos = GlobalPlayerVariables.save.ammos;

        string[] loadGuns = GlobalPlayerVariables.save.guns;

        for (int i = 0; i < loadGuns.Length; i++){
            AddGun(loadGuns[i], 0);
        }
    }

    GunController CreateGun(string gunName){
        GameObject gunPrefab = Resources.Load($"Prefabs/guns/{gunName}Prefab", typeof(GameObject)) as GameObject;
        GunController gunController;
            if(gunPrefab != null){
                GameObject gun = Instantiate(gunPrefab, transform.position, transform.rotation);
                if(gun.TryGetComponent<GunController>(out gunController)){
                    gunController.gunName = gunName;
                    gun.transform.parent = this.transform;
                    Ammo a = GetAmmo(gunController.ammoType);
                    if(a != null)
                        gunController.SetAmmo(a);
                }
                else{
                    Destroy(gun);
                    print($"Gun {gunName} couldn't be loaded because it doesn't have an attached Gun Controller.");
                    return null;
                }
                
            }
            else{
                print($"Gun {gunName} couldn't be loaded because it doesn't have a relevant prefab.");
                return null;
            }

        //inventoryRefresh = true;
        return gunController;
    }

    Ammo GetAmmo(AmmoType type){
        for(int i = 0; i < ammos.Length; i++){
            if(ammos[i].type == type){
                return ammos[i];
            }
        }

        Debug.LogError($"AmmoType {type} doesn't exist in the player's inventory!");
        return null;
    }
    
    /*
     * Creates the ammos, and tried to retrieve the relevant font controller attached.
     */
    // void CreateAmmos(){
    //     // We do the ammo in the inventory because the gun ammo is NOT tied to the gun in Doom, it is tied to the overall inventory.
    //     updateMaxAmmo = true;
    //     AmmoType[] types = (AmmoType[])Enum.GetValues(typeof(AmmoType));

    //     for(int i = 0; i < ammoTypeCount; i++){
    //         AmmoType type = types[i];
    //         Ammo ammo = new Ammo(type);
    //         ammos[i] = ammo;

    //         UIFontController fc = uiController.GetFontController("UI_Ammo" + type.ToString() + "Pos");
    //         ammoDisplays[i] = fc; 
    //     }
    // }

    /*
     *  Goes through and updates all the Ammo UI pieces
     */
    public void UpdateGUI(){
        if(!inventoryUpdated)
            return;
        
        inventoryUpdated = false;
        int iterator = 1;

        for (int i = 0; i < ammoDisplays.Length; i++){
            UIFontController cont = ammoDisplays[i];
            if(cont == null)
                continue;

            Ammo ammo = ammos[iterator];

            cont.SetValue(ammo.count + "");

            // we're only updating max ammo once, possibly twice if the player has picked up a backpack
            // so it's not as important to store the all relevant font controllers
            if(updateMaxAmmo)
                uiController.SetValue("UI_Ammo" + ammo.type.ToString() + "MaxPos", ammo.max < 0 ? "" : ammo.max + "");

            iterator++;
        }
        
        updateMaxAmmo = false;
        if(activeAmmo != null)
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
                activeGun = gun;
                activeAmmo = ammos[(int)gun.GetAmmoType()];
                uiViewSprite.ChangeGun(gun.gunName, gun.viewSpriteHeight, gun.viewSpriteFPS);
                gun.DisableShot(uiViewSprite.gunSwapSpeed);
            }
            else{
                gun.ToggleFiring(false);
                uiViewSprite.ToggleFiring(false);
            }
        }
    }

    /*
     * Runs every frame and checks if the player switched weapons using scrollwheel or numbers.
     */
    bool UserWeaponSwitch(){
        // if we're at -1 and we have no guns, we don't change becuase there's nothing to change
        if(activeGunIndex == -1){
            if(guns.Count == 0)
                return false;

            activeGunIndex = 0;
            return true;
        }

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
     * FOR DEBUGGING ONLY
     */
    // void OnGUI(){
	// 	GUI.Label(new Rect(10, 90, 750, 1600), $"Current Active Weapon: {guns[activeGunIndex].name} ({activeGunIndex}) \n Guns: \n {ListToString(guns)}");//\n {ListToString(ammos)}");
	// }

    /*
     * Quick and dirty print list function
     */
    // public static string ListToString<T>(List<T> objs){
	// 	string returnStr = "";

	// 	foreach (T o in objs) {
	// 		returnStr += o.ToString() + "\n ";
	// 	}
	// 	return returnStr;
	// }

    
}

