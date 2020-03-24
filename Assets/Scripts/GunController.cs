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
    public GameObject bulletEndWithDamagePrefab;
    GameObject damagePrefab;
    Transform gunShooter;

    public string gunName;

    public float viewSpriteHeight = 40.0f;
    public float viewSpriteFPS = 12.0f;
    public float fireRate = 150f;

    public float projectileDist = 1024f;
    public float projectileSpeed = -1;
    public int projectileAmount = 1;

    public float explosionSize = 1f; // only for rockets

    public float minDamage = 5f;
    public float maxDamage = 15f;
    public float bulletSpread = 5.5f;

    public int spreadOnNthBullet = 0;
    
    
    public bool firing{ get; private set; }
    public float RPS{ get; private set; } // rounds per second

    //[HideInInspector]
    //public UIViewSpriteController uiViewSprite;

    Ammo ammo;

    float lastShot;    
    int bulletsFired;
    bool bulletFired;

    void Awake(){
        if(projectileSpeed > 0)
            damagePrefab = Resources.Load($"Prefabs/guns/ProjectileDamagePrefab", typeof(GameObject)) as GameObject;
        RPS = (60f / fireRate); // gun's fire rate is rounds per minute, we're setting it to be rounds per second for coding purposes
        //gunSwapSpeed = uiViewSprite.gunSwapSpeed;
        bulletSpread /= 2;
    }

    void Start(){
        gunShooter = transform.root;
    }

    void Update(){
        lastShot += Time.deltaTime;

        if(firing)
            Fire();
        else
            bulletsFired = 0;
    }

    void LateUpdate(){
        bulletFired = false;
    }

    public void UpdateFireRate(float _fireRate){
        fireRate = _fireRate;
        RPS = (60.0f / fireRate);
    }

    public bool BulletFiredOnFrame(){
        //we're checking if we did, OR we're going to fire a bullet
       return bulletFired || (firing && lastShot >= RPS && ammo.CanExpendAmmo());
    }

    public void ToggleFiring(bool toggle){
        firing = toggle;
    }

    /*
     * Disabling shooting for weapon swaps and anything else that may require disabling the gun.
     */
    public void DisableShot(float disableFor){
        lastShot = RPS - disableFor;
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
            print("Ammo type for " + gunName + " is an unknown type.");
            return AmmoType.None;
        }

    }

    /*
     * Method to fire the gun, does prerequisite checks to ensure the gun can actually be fired, then expends ammo and fires the bullet (raytrace or projectile)
     */
    void Fire(){
        //print($"Last fired at {lastShot}, RPS {RPS}, lastShot >= RPS ? {(lastShot >= RPS)}");
        if(lastShot >= RPS){
            if(ExpendAmmo()){
                //uiViewSprite.ToggleFiring(true);
                //uiViewSprite.Fire();
                firing = true;
                bulletFired = true;
                for(int i = 0; i < projectileAmount; i++){
                    float randomSpreadX = 0;
                    float randomSpreadY = 0;

                    if(bulletsFired >= spreadOnNthBullet){
                        randomSpreadX = UnityEngine.Random.Range(-bulletSpread, bulletSpread);
                        randomSpreadY = UnityEngine.Random.Range(-bulletSpread, bulletSpread);
                    }

                    BulletController bullet = Instantiate(bulletPrefab, transform.position, transform.rotation * Quaternion.Euler (randomSpreadX, randomSpreadY, 0f)).GetComponent<BulletController>();
                    int damageTo = (int)UnityEngine.Random.Range(minDamage, maxDamage);
                    bullet.SetVariables(projectileSpeed, projectileDist, explosionSize, damageTo, ignoreLayer, bulletEndPrefab, damagePrefab, bulletEndWithDamagePrefab, gunShooter);
                    bulletsFired++;
                }
            }
            else{
                firing = false;
                //uiViewSprite.ToggleFiring(false);
                print("No Ammo.");
            }
            lastShot = 0;
        }
    }

    bool CanExpendAmmo(){
        if(ammo == null)
            return false;
        
        return ammo.CanExpendAmmo();
    }

    bool ExpendAmmo(){
        if(ammo == null)
            return false;

        return ammo.ExpendAmmo();
    }
}


/**
  * Ammo class to store the ammo information, since ammo is shared between guns.
  */
[System.Serializable]
public class Ammo{
    public int max;//{ get; private set; }
    public int multiplier;//{ get; private set; }
    public int count;//{ get; private set;}
    public AmmoType type;//{ get; }

    public Ammo(AmmoType _type){
        type = _type;
        count = 0;
        multiplier = 1;
        SetMaxAmmo();
    }

    /*
     * Adds ammo to the current pool. Returns true if it successfully added ammo.
     */
    public bool AddAmmo(int ammoToAdd){
        // checking if it's type none first, which cannot be added ammo, and shouldn't ever be picked up, but just in case.
        // we should never be picking up max that is -1, as it should be infinite 
        // but to prevent issues, we'll check for it anyway.
        if(type == AmmoType.None || (count >= max && max != -1 && count != -1))
            return false;
        
        if(count + ammoToAdd > max)
            count = max;
        else
            count += ammoToAdd;
            
        return true;
    }

    /*
     * Expends ammo, returns true if it successfully expended ammo. 
     */
    public bool ExpendAmmo(){
        // -1 means it will expend ammo successfully no matter what, and we still return as true without subtracting ammo.
        if(count == -1)
            return true;

        if(count == 0)
            return false;

        count--;
        return true;
    }

    public bool CanExpendAmmo(){
        if(count == -1)
            return true;

        if(count == 0)
            return false;
            
        return true;
    }

    public void IncreaseMaxAmmo(int _multiplier){
        if(_multiplier > 0){
            multiplier = _multiplier;
            SetMaxAmmo();
        }
    }

    /*
     * Sets the max ammo based on the type of ammo, defaults to -1 (infinite) if the ammo is undefined.
     */
    void SetMaxAmmo(){
        switch(type){
            case AmmoType.None: count = -1; max = -1; break;
            case AmmoType.Enemy: count = -1; max = -1; break;
            case AmmoType.Bullet: max = 200 * multiplier; break;
            case AmmoType.Shell: max = 50 * multiplier; break;
            case AmmoType.Rocket: max = 50 * multiplier; break;
            case AmmoType.Cell: max = 300 * multiplier; break;
        }
    }

    public override string ToString(){
        return string.Format("Type: {0} Count: {1}, Max Ammo: {2} ", type, count, max);
    }
}