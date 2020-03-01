using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PickupController : MonoBehaviour
{
    public PickupType pickup;
    public string pickupName;
    public string key;
    public string value;

    void OnTriggerEnter(Collider col){
        PlayerController pc;

        if(col.gameObject.TryGetComponent<PlayerController>(out pc))
            ApplyPickup(pc);
        
    }

    void ApplyPickup(PlayerController player){
        bool destroy = false;
        switch(pickup){
            case PickupType.Health: destroy = player.HealPlayer(Int32.Parse(value)); break;
            case PickupType.Armor: destroy = player.AddArmor(Int32.Parse(value)); break;
            case PickupType.Weapon: destroy = player.AddGun(key, Int32.Parse(value), pickupName); break;
            case PickupType.Ammo: destroy = player.AddAmmo((AmmoType)Enum.Parse(typeof(AmmoType), key), Int32.Parse(value), pickupName); break;
            case PickupType.Key:break;
            case PickupType.Backpack: destroy = player.AddBackpack(); break;
        }
        //print($"Destroying pickup? {destroy}. Type {pickup.ToString()}, key {key}, value {value}");
        if(destroy)
            Destroy(gameObject);
    }
}
