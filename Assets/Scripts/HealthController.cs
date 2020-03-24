using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    public int maxHealth;
    public int maxArmor;

    public int health{ get; private set; }
    public int armor{ get; private set; }
    public ArmorType armorType{ get; private set; }
    public Transform damagedBy{ get; private set; }
    public int damageAmountHealth{ get; private set; }
    public int damageAmountArmor{ get; private set; }
    public bool healthOrArmorChanged{ get; private set; }

    bool invunerable;
    bool isPlayer;

    const float INVUNERABILITYLENGTH = 10f;

    void Start(){
        isPlayer = gameObject.tag == "Player";
        if(!isPlayer)
            health = maxHealth;
    }

    void LateUpdate(){
        healthOrArmorChanged = false;
        damagedBy = null;
        damageAmountHealth = 0;
        damageAmountArmor = 0;
    }

    public void AddHealth(int healthToAdd){
        healthOrArmorChanged = true;
        health += healthToAdd;
    }

    public void AddArmor(int armorToAdd){
        healthOrArmorChanged = true;
        armor += armorToAdd;
    }

    public bool ChangeArmor(ArmorType type){
        // doom overwrites blue armor with green if it's under 100
        // even though green is objectively worse
        switch(type){
            case ArmorType.Blue: 
                if(armor < 200){
                    armor = 200;
                    armorType = type;
                    healthOrArmorChanged = true;
                    return true;
                }
            break;
            case ArmorType.Green:
                if(armor < 100){
                    armor = 100;
                    armorType = type;
                    healthOrArmorChanged = true;
                    return true;
                }
            break;
            case ArmorType.None:
                armor = 0;
                armorType = type;
                healthOrArmorChanged = true;
                return true;
        }
        return false;
    }

    public void TakeDamage(Transform _damagedBy, int damage){
        if(invunerable)
            return;

        damagedBy = _damagedBy;
        float armorDamage = armorType == ArmorType.None ? 0 : damage * (1 / (float)armorType);
        float healthDamage = damage - armorDamage;
        float armorOverkill = 0;
        
        int roundedArmorDmg = Mathf.RoundToInt(armorDamage);
        
        if(roundedArmorDmg > armor){
            armorOverkill = roundedArmorDmg - armor;
            armor = 0;
        }
        else{
            armor -= roundedArmorDmg;
        }

        int roundedHealthDmg = Mathf.RoundToInt(healthDamage + armorOverkill);
        damageAmountHealth = roundedHealthDmg;
        health -= roundedHealthDmg;
        healthOrArmorChanged = true;
    }

    public bool Invunerability(){
        if(invunerable)
            return false;

        invunerable = true;
        StartCoroutine(InvunerabilityTimer());
        return true;
    }

    IEnumerator InvunerabilityTimer(){
        yield return new WaitForSeconds(INVUNERABILITYLENGTH);
        invunerable = false;
        yield break;
    }

    public void LoadGlobalVariables(){
        if(!isPlayer)
            return;

        maxHealth = GlobalPlayerVariables.save.MaxHP;
        maxArmor = GlobalPlayerVariables.save.MaxAP;
        health = GlobalPlayerVariables.save.HP;
        armor = GlobalPlayerVariables.save.AP;
        armorType = GlobalPlayerVariables.save.armorType;
        healthOrArmorChanged = true;
    }

    public void SaveGlobalVariables(){
        if(!isPlayer)
            return;

        GlobalPlayerVariables.save.HP = health;
        GlobalPlayerVariables.save.AP = armor;
        GlobalPlayerVariables.save.armorType = armorType;
        healthOrArmorChanged = true;
    }


}
