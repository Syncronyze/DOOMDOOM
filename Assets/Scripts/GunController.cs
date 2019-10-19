using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using UnityEngine;

public class GunController : MonoBehaviour
{   
    public string gunName;
    public GameObject bulletPrefab;
    public GameObject bulletEndPrefab;

    [HideInInspector]
    public UIViewSpriteController uiViewSprite;

    LayerMask ignoreLayer;
    Gun gun; // holds all of our gun-related variables

    float lastShot;
    float RPS; // rounds per second
    float gunSwapSpeed;

    bool isActiveGun;

    void Awake(){
        if(!RetrieveGunInfo()){
            print($"Invalid gun {gunName}, please check the XML and ensure all parameters are correct.");
            Destroy(gameObject);
            return;
        }
        uiViewSprite = GameObject.FindGameObjectWithTag("ViewSprite").GetComponent<UIViewSpriteController>();

        isActiveGun = false;
        RPS = (60f / gun.fireRate); // gun's fire rate is rounds per minute, we're setting it to be rounds per second for coding purposes
        ignoreLayer = ignoreLayer = 1 << LayerMask.NameToLayer ("Player");
        gunSwapSpeed = uiViewSprite.gunSwapSpeedSeconds;
    }

    void Update(){
        lastShot += Time.deltaTime;
    }

    void FixedUpdate(){
        if(isActiveGun){
            if(Input.GetButton("Fire1"))
                Shoot();
            else
                uiViewSprite.ToggleFiring(false);
        }
            
    }

    /*
     * Resetting shot so you can't spam change weapons to achieve a higher firerate
     */
    public void SetActive(bool active){
        isActiveGun = active;
        lastShot = RPS - gunSwapSpeed;
    }

    /*
     * Passes along the ammo reference to the Gun object.
     */
    public void SetAmmo(ref Ammo _ammo){
        if(_ammo == null || _ammo.type != gun.ammoType)
            return;
        
        gun.SetAmmo(ref _ammo);       
    }

    /*
     * Retrieves the gun ammo type, since enums are not nullable, if it's an unknown ammoType we simply return AmmoType.None.
     */
    public AmmoType GetAmmoType(){
        try{
            return gun.ammoType;
        }
        catch(NullReferenceException){
            print("Ammo type for " + gunName + " is an unknown type. Please check the XML file.");
            return AmmoType.None;
        }

    }

    /*
     * Retrieves the rounds per second of the gun.
     */
    public float GetRPS(){
        return RPS;
    }

    /*
     * Method to fire the gun, does prerequisite checks to ensure the gun can actually be fired, then expends ammo and fires the bullet (raytrace or projectile)
     */
    void Shoot(){
        if(lastShot > RPS){
            if(gun.ExpendAmmo()){
                BulletController bullet = Instantiate(bulletPrefab, transform.position, transform.rotation).GetComponent<BulletController>();
                bullet.SetVariables(gun.projectileSpeed, gun.projectileDist, gameObject.layer, bulletEndPrefab);
                uiViewSprite.ToggleFiring(true);
                uiViewSprite.Fire();
            }
            else{
                uiViewSprite.ToggleFiring(false);
                print("No Ammo.");
            }
            
            lastShot = 0;
        }
    }    

    /*
     *   Retrieving the gun's information based on the gunName parameter, stored in XML. Returns true or false depending if we're dealing with a valid gun.
     */
    bool RetrieveGunInfo(){
        if(!string.IsNullOrEmpty(gunName)){
            XmlDocument doc = new XmlDocument();
            doc.Load(Application.dataPath + "/XML/GunCollection.xml");
            XmlNodeList gunNodes = doc.DocumentElement.SelectNodes("Gun[@name='" + gunName + "']");
            
            if(gunNodes.Count < 1)
                return false;
           
            XmlNode gunNode = gunNodes[0];

            AmmoType ammoType;
            float projectileSpeed, projectileDist, fireRate;

            
    
            if(!Single.TryParse(gunNode.SelectSingleNode("fireRate").InnerText, out fireRate))
                return false;

            if(!Single.TryParse(gunNode.SelectSingleNode("projectileSpeed").InnerText, out projectileSpeed))
                return false;

            if(!Single.TryParse(gunNode.SelectSingleNode("projectileDist").InnerText, out projectileDist))
                return false;

            if(!Enum.TryParse(gunNode.SelectSingleNode("ammoType").InnerText, out ammoType))
                return false;
            
            gun = new Gun(gunName, fireRate, projectileDist, projectileSpeed, ammoType);
            return true;
        }

        return false;
    }
}
