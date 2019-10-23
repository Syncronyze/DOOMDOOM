using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    
    public int health{ get; private set; }
    public int armor{ get; private set; }
    public ArmorType armorType{ get; private set; }

    GameUIController uiController;
    
    bool invunerable;

    const float INVUNERABILITYLENGTH = 10f;

    void Start(){
        uiController = GameObject.FindGameObjectWithTag("GameUI").GetComponent<GameUIController>();
    }

    void Update(){
        //if(health == 0)
            // DEATH
        uiController.SetValue("UI_ArmorPos", health + "%");
        uiController.SetValue("UI_HealthPos", armorType == ArmorType.None ? "" : armor + "%");
    }

    public bool AddHealth(int healthToAdd){
        if(health >= 200)
            return false;
        
        health += healthToAdd;
        return true;
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

    public void TakeDamage(int damage){
        if(invunerable)
            return;
        // if armorType is none, then our armor takes 0 damage, otherwise it takes a multiplier of how much it prevents
        // eg, blue is 2 so we take half damage: damage * (0.5);
        float armorDamage = damage * (1 / (int)armorType);
        float healthDamage = damage - armorDamage;
        float armorOverkill = 0;
        armor -= Mathf.RoundToInt(armorDamage);

        if(armor <= 0){
            armorOverkill = Mathf.Abs(armor);
            ChangeArmor(ArmorType.None);
        }
        
        health -= Mathf.RoundToInt(healthDamage + armorOverkill);

        // for the UI
        if(health < 0)
            health = 0;

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
        health = GlobalPlayerVariables.HP;
        armor = GlobalPlayerVariables.AP;
        armorType = GlobalPlayerVariables.aType;
    }

    public void SaveGlobalVariables(){
        GlobalPlayerVariables.HP = health;
        GlobalPlayerVariables.AP = armor;
        GlobalPlayerVariables.aType = armorType;
    }


}
