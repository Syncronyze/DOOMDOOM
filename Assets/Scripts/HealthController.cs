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

    bool invunerable;
    bool isPlayer;

    const float INVUNERABILITYLENGTH = 10f;

    void Start(){
        isPlayer = gameObject.tag == "Player";
        if(!isPlayer)
            health = maxHealth;
    }

    void LateUpdate(){
        damagedBy = null;
        damageAmountHealth = 0;
        damageAmountArmor = 0;
    }

    public void AddHealth(int healthToAdd){
        health += healthToAdd;
    }

    public void AddArmor(int armorToAdd){
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
                    return true;
                }
            break;
            case ArmorType.Green:
                if(armor < 100){
                    armor = 100;
                    armorType = type;
                    return true;
                }
            break;
            case ArmorType.None:
                armor = 0;
                armorType = type;
                return true;
        }
        return false;
    }

    public void TakeDamage(Transform _damagedBy, int damage){
        if(invunerable)
            return;

        damagedBy = _damagedBy;
        
        float armorDamage = armorType == ArmorType.None ? 0 : damage * (1 / (int)armorType);
        float healthDamage = damage - armorDamage;
        float armorOverkill = 0;

        int roundedArmorDmg = Mathf.RoundToInt(armorDamage);

        armor -= roundedArmorDmg;
        damageAmountArmor = roundedArmorDmg;

        if(armor <= 0)
            armorOverkill = Mathf.Abs(armor);
        
        int roundedHealthDmg = Mathf.RoundToInt(healthDamage + armorOverkill);

        damageAmountHealth = roundedHealthDmg;
        health -= roundedHealthDmg;
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
    }

    public void LoadGlobalVariables(){
        if(!isPlayer)
            return;

        maxHealth = GlobalPlayerVariables.MaxHP;
        maxArmor = GlobalPlayerVariables.MaxAP;
        health = GlobalPlayerVariables.HP;
        armor = GlobalPlayerVariables.AP;
        armorType = GlobalPlayerVariables.aType;
    }

    public void SaveGlobalVariables(){
        if(!isPlayer)
            return;

        GlobalPlayerVariables.HP = health;
        GlobalPlayerVariables.AP = armor;
        GlobalPlayerVariables.aType = armorType;
    }


}
