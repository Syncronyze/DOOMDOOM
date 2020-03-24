using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class GlobalPlayerVariables
{
    public static SaveVariables[] saves = new SaveVariables[SAVE_AMOUNT];
    public static SaveVariables save;
    public static bool savesExist = false;
    static int saveIndex = -1;
    

    public static bool loadedSaves{ get; private set; } = false;
    static bool dontWriteFiles = false;
    const int SAVE_AMOUNT = 3;

    public static void Save(){
        if(!loadedSaves)
            return;

        Debug.Log("Saving...");
        WriteSaveFiles();
    }

    public static void LoadAll(){
        if(loadedSaves)
            return;

        ReadSaveFiles();
    }

    public static void Load(int _saveIndex){
        if(!loadedSaves)
            LoadAll();
            
        Debug.Log("Loading save save" + _saveIndex + ".json ...");
        SaveVariables saveToLoad = saves[_saveIndex];
        if(saveToLoad != null){
            save = saveToLoad;
            saveIndex = _saveIndex;
        }
        else{
            Debug.LogError($"Couldn't load {_saveIndex}.");
        }
    }

    public static void New(int _saveIndex){
        if(!loadedSaves)
            return;

        Debug.Log("Overwriting save" + _saveIndex + " with a new variable set...");
        saveIndex = _saveIndex;
        save = DefaultSave();
        saves[saveIndex] = save;
        savesExist = true;
        WriteSaveFiles();
    }

    #if UNITY_EDITOR
    public static void LoadFakeDebugSave(){
        dontWriteFiles = true;
        int HP = 99999;
        int MaxHP = 99999;
        int AP = 99999;
        int MaxAP = 99999;
        int level = 1;
        ArmorType aType = ArmorType.Blue;
        string[] guns = new string[5]{"fists", "pistol", "shotgun", "rocket", "chaingun"};
        
        AmmoType[] ammoTypes = (AmmoType[])Enum.GetValues(typeof(AmmoType));
        Ammo[] ammos = new Ammo[ammoTypes.Length];
        for(int i = 0; i < ammoTypes.Length; i++){
            AmmoType type = ammoTypes[i];
            Ammo ammo = new Ammo(type);
            ammo.AddAmmo(999);
            ammos[i] = ammo;
        }

        save = new SaveVariables(HP, MaxHP, AP, MaxAP, level, aType, guns, ammos);
        saveIndex = 0;
    }
    #endif
    static SaveVariables DefaultSave(){
        int HP = 100;
        int MaxHP = 200;
        int AP = 0;
        int MaxAP = 100;
        int level = 1;
        ArmorType aType = ArmorType.Green;
        string[] guns = new string[2]{"fists", "pistol"};
        
        AmmoType[] ammoTypes = (AmmoType[])Enum.GetValues(typeof(AmmoType));
        Ammo[] ammos = new Ammo[ammoTypes.Length];
        for(int i = 0; i < ammoTypes.Length; i++){
            AmmoType type = ammoTypes[i];
            Ammo ammo = new Ammo(type);
            if(type == AmmoType.Bullet)
                ammo.AddAmmo(75);
            ammos[i] = ammo;
        }

        return new SaveVariables(HP, MaxHP, AP, MaxAP, level, aType, guns, ammos);
    }

    static void ReadSaveFiles(){
        Debug.Log("Reading file...");
        for(int i = 0; i < saves.Length; i++){
            TextAsset saveText = Resources.Load<TextAsset>("Saves/Save" + i);
            if(saveText != null){
                savesExist = true;
                saves[i] = JsonUtility.FromJson<SaveVariables>(saveText.text);
                Debug.Log("File Saves/Save" + i + " was successfully loaded.");
            }
        }
        
        loadedSaves = true;
    }

    static void WriteSaveFiles(){
        if(dontWriteFiles)
            return;

        for(int i = 0; i < saves.Length; i++){
            if(saves[i] == null)
                continue;

            try{
                File.WriteAllText(Application.dataPath + "/Resources/Saves/Save" + i + ".json", JsonUtility.ToJson(saves[i]));
                Debug.Log("File " + Application.dataPath + "/Resources/Saves/Save" + i + ".json was successfully written.");
            }
            catch(Exception e){
                Debug.LogError($"Save {i} cannot be created and therefore nothing will be saved.\n{e.Message}\n{e.StackTrace}");
                continue;
            }
        }
        
    }

}

public class SaveVariables{
    public int HP;
    public int MaxHP;
    public int AP;
    public int MaxAP;
    public int level;
    public ArmorType armorType;
    public string[] guns;
    public Ammo[] ammos;

    public SaveVariables(int _HP, int _MaxHP, int _AP, int _MaxAP, int _level, ArmorType _armorType, string[] _guns, Ammo[] _ammos){
        HP = _HP;
        MaxHP = _MaxHP;
        AP = _AP;
        MaxAP = _MaxHP;
        armorType = _armorType;
        guns = _guns;
        ammos = _ammos;
        level = _level;
    }

    public override string ToString(){
        return $"HP: {HP}, MaxHP: {MaxHP}, AP: {AP}, MaxAP: {MaxAP}, ArmorType: {armorType}";
    }
}
