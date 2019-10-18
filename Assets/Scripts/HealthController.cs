using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    public GameUIController uiController;

    public int health{ get; private set; }
    public int armor{ get; private set; }
    public ArmorType armorType{ get; private set; }

    void Start(){
        
    }

    void Update(){
        //if(health == 0)
            // DEATH
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


}
