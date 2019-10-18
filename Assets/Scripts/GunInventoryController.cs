using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

/**
 *      Class to store all of the gun and ammo related information. All ammo is stored within the inventory, as it's shared between guns.
 */
public class GunInventoryController : MonoBehaviour
{
    GameUIController uiController;
    UIViewSpriteController uiViewSprite;
    
    List<GunController> guns;
    List<Ammo> ammos;
    List<UIFontController> ammoDisplays;
    UIFontController activeAmmoDisplay;
    Ammo activeAmmo;

    int activeGunIndex;
    bool valid;
    bool updateMaxAmmo;


    // when the inventory has an update and needs to be refreshed
    [HideInInspector]
    public bool inventoryRefresh;

    void Awake(){
        guns = new List<GunController>();
        ammoDisplays = new List<UIFontController>();
        uiController = GameObject.FindGameObjectWithTag("GameUI").GetComponent<GameUIController>();
        uiViewSprite = GameObject.FindGameObjectWithTag("ViewSprite").GetComponent<UIViewSpriteController>();
        activeGunIndex = -1;
        inventoryRefresh = true;

        if(!uiController.HasControllers())
            uiController.CollectUIControllers();

        activeAmmoDisplay = uiController.GetFontController("UI_AmmoPos");
        CreateAmmos();
        
        // TODO: Remove, just for testing
        AddAmmo(AmmoType.Bullet, 500);
        AddAmmo(AmmoType.Rocket, 500);
        CreateGun("fists");
        CreateGun("pistol");
        CreateGun("rocket");
        CreateGun("chaingun");
    }

    void Start(){
        LoadInventory();
        //SetActiveGun();
    }

    void Update(){
        RefreshInventory();
        if(UserWeaponSwitch())
            SetActiveGun();
        
        UpdateGUI();
    }

    /*
     *  Adding ammo based on type.
     */
    public bool AddAmmo(AmmoType type, int amountToAdd){
        foreach(Ammo a in ammos){
            if(a.type == type)
                return a.AddAmmo(amountToAdd);
            
        }
        
        print($"Cannot add ammo of type {type.ToString()} as it doesn't exist in our inventory!");
        return false;
    }

    /*
     * Increases max ammo by a multiplier given.
     */
    public void IncreaseMaxAmmo(int multiplier){
        foreach(Ammo a in ammos){
            a.IncreaseMaxAmmo(multiplier);
            updateMaxAmmo = true;
        }
    }

    public void SaveInventory(){
        GlobalPlayerVariables.guns = new List<string>();
        foreach(GunController gc in guns){
            GlobalPlayerVariables.guns.Add(gc.gunName);
        }

        GlobalPlayerVariables.ammos = ammos;
    }

    public void LoadInventory(){
        if(GlobalPlayerVariables.ammos != null)
            ammos = GlobalPlayerVariables.ammos;
        
        if(GlobalPlayerVariables.guns == null)
            return;

        List<string> loadGuns = GlobalPlayerVariables.guns;

        foreach(string gunName in loadGuns){
            CreateGun(gunName);
        }
            
    }

    void CreateGun(string gunName){
        GameObject gunPrefab = (GameObject)AssetDatabase.LoadAssetAtPath($"Assets/Prefabs/{gunName}Prefab.prefab", typeof(GameObject));
            if(gunPrefab != null){
                GameObject gun = Instantiate(gunPrefab, transform.position, transform.rotation);
                GunController gunController;
                if(gun.TryGetComponent<GunController>(out gunController)){
                    gunController.gunName = gunName;
                    gun.transform.parent = this.transform;
                }
                else{
                    Destroy(gun);
                    print($"Gun {gunName} couldn't be loaded because it doesn't have an attached Gun Controller.");
                }
                
            }
            else{
                print($"Gun {gunName} couldn't be loaded because it doesn't have a relevant prefab.");
            }
                
        
        inventoryRefresh = true;
    }

    
    /*
     * Creates the ammos, and tried to retrieve the relevant font controller attached.
     */
    void CreateAmmos(){
        // We do the ammo in the inventory because the gun ammo is NOT tied to the gun in Doom, it is tied to the overall inventory.
        ammos = new List<Ammo>();
        updateMaxAmmo = true;

        foreach(AmmoType type in Enum.GetValues(typeof(AmmoType))){
            Ammo ammo = new Ammo(type);
            ammos.Add(ammo);

            UIFontController fc = uiController.GetFontController("UI_Ammo" + type.ToString() + "Pos");
            ammoDisplays.Add(fc); 
        }
    }

    /*
     *  Goes through and updates all the Ammo UI pieces, SetValue doesn't update if the value is identical.
     */
    void UpdateGUI(){
        int iterator = 1;

        foreach(UIFontController cont in ammoDisplays){
            if(cont == null)
                continue;

            Ammo ammo = ammos[iterator];

            cont.SetValue(ammo.count + "");

            // we're only updating max ammo once, possibly twice if the player has picked up a backpack
            // thus it's not important as important to store the relevant font controllers
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
                gun.SetActive(true);
                activeAmmo = ammos[(int)gun.GetAmmoType()];
                uiViewSprite.ChangeGun(gun.gunName);
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
     * Refreshes the gun list when we pickup weapons
     */
    void RefreshInventory(){
        if(inventoryRefresh){
            int iterator = 0;
            guns = new List<GunController>();
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
                    gc.uiViewSprite = uiViewSprite;
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
    //void OnGUI(){
	//	GUI.Label(new Rect(10, 90, 750, 1600), $"Current Active Weapon: {guns[activeGunIndex].name} ({activeGunIndex}) \n Guns: \n {ListToString(guns)}");//\n {ListToString(ammos)}");
	//}

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

