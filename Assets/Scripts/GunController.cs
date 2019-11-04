using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using UnityEngine;

public class GunController : MonoBehaviour
{   

    public LayerMask ignoreLayer;

    public AmmoType ammoType;

    public GameObject bulletPrefab;
    public GameObject bulletEndPrefab;

    public string gunName;

    public float viewSpriteHeight = 40.0f;
    public float fireRate = 150f;

    public float projectileDist = 1024f;
    public float projectileSpeed = -1;
    public int projectileAmount = 1;

    public float minDamage = 5f;
    public float maxDamage = 15f;
    public float bulletSpread = 5.5f;

    public int spreadOnNthBullet = 0;
    

    [HideInInspector]
    public UIViewSpriteController uiViewSprite;

    Ammo ammo;

    float lastShot;
    float RPS; // rounds per second
    float gunSwapSpeed;

    int bulletsFired;

    bool isActiveGun;

    void Awake(){
        uiViewSprite = GameObject.FindGameObjectWithTag("ViewSprite").GetComponent<UIViewSpriteController>();
        isActiveGun = false;
        RPS = (60f / fireRate); // gun's fire rate is rounds per minute, we're setting it to be rounds per second for coding purposes
        gunSwapSpeed = uiViewSprite.gunSwapSpeed;
        bulletSpread /= 2;
    }

    void Update(){
        lastShot += Time.deltaTime;
    }

    void FixedUpdate(){
        if(isActiveGun){
            if(Input.GetButton("Fire1")){
                Shoot();
            }
            else{
                uiViewSprite.ToggleFiring(false);
                bulletsFired = 0;
            }
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
    public void SetAmmo(Ammo _ammo){
        if(_ammo == null || _ammo.type != ammoType)
            return;
        
        ammo = _ammo;      
    }

    /*
     * Retrieves the gun ammo type, since enums are not nullable, if it's an unknown ammoType we simply return AmmoType.None.
     */
    public AmmoType GetAmmoType(){
        try{
            return ammoType;
        }
        catch(NullReferenceException){
            print("Ammo type for " + gunName + " is an unknown type. Please check the XML file.");
            return AmmoType.None;
        }

    }

    /*
     * Retrieves the rounds per second of the 
     */
    public float GetRPS(){
        return RPS;
    }

    /*
     * Method to fire the gun, does prerequisite checks to ensure the gun can actually be fired, then expends ammo and fires the bullet (raytrace or projectile)
     */
    void Shoot(){
        if(lastShot > RPS){
            if(ExpendAmmo()){
                uiViewSprite.ToggleFiring(true);
                uiViewSprite.Fire();

                for(int i = 0; i < projectileAmount; i++){
                    float randomSpreadX = 0;
                    float randomSpreadY = 0;

                    if(bulletsFired >= spreadOnNthBullet){
                        randomSpreadX = UnityEngine.Random.Range(-bulletSpread, bulletSpread);
                        randomSpreadY = UnityEngine.Random.Range(-bulletSpread, bulletSpread);
                    }

                    BulletController bullet = Instantiate(bulletPrefab, transform.position, transform.rotation * Quaternion.Euler (randomSpreadX, randomSpreadY, 0f)).GetComponent<BulletController>();
                    bullet.SetVariables(projectileSpeed, projectileDist, ignoreLayer, bulletEndPrefab);
                    bulletsFired++;
                }
            }
            else{
                uiViewSprite.ToggleFiring(false);
                print("No Ammo.");
            }
            
            lastShot = 0;
        }
    }

    bool ExpendAmmo(){
        if(ammo == null)
            return false;

        return ammo.ExpendAmmo();
    }
} 