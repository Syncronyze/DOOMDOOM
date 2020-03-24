using UnityEngine;

public class GlobalLevelVariables : MonoBehaviour{
    
    public static GlobalLevelVariables instanceOf;
    public Transform pickupsParent;
    public Transform secretsParent;
    public string levelName;
    public float levelPar;

    float levelBegin;
    public float levelTotal{ get; private set; }
    
    int enemiesTotal;
    int enemiesKilled;
    
    int secretsTotal;
    int secretsFound;
    
    int pickupsTotal;
    int pickupsAcquired;

    void Awake(){
        if(instanceOf == null){
            instanceOf = this;
        }
        else{
            Destroy(this);
            return;
        }
    }

    void Start(){
        levelBegin = Time.realtimeSinceStartup;
        secretsTotal = secretsParent.childCount;
        pickupsTotal = pickupsParent.childCount;
        print("secrets:" + secretsTotal + ", pickups:" + pickupsTotal);
    }

    public void AddEnemy(){
        enemiesTotal++;
    }

    public void EnemyKilled(){
        enemiesKilled++;
    }

    public void SecretFound(){
        secretsFound++;
    }

    public void PickupAcquired(){
        pickupsAcquired++;
    }

    public int PercentageOfEnemiesKilled(){
        if(enemiesTotal == 0)
            return 100;

        return (int)((float)enemiesKilled / (float)enemiesTotal * 100);
    }

    public int PercentageOfPickupsAcquired(){
        if(pickupsTotal == 0)
            return 100;

        return (int)((float)pickupsAcquired / (float)pickupsTotal * 100);
    }

    public int PercentageOfSecretsFound(){
        if(secretsTotal == 0)
            return 100;

        return (int)((float)secretsFound / (float)secretsTotal * 100);
    }

    public void LevelEnded(){
        levelTotal = Time.realtimeSinceStartup - levelBegin;
    }
}