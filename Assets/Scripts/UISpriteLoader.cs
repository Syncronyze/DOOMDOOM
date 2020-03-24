using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/**
  *     Custom SpriteLoader for unity to retrieve a sprite out of multiple loaded spritesheets.
  */
[ExecuteAlways]
public class UISpriteLoader : MonoBehaviour
{
    public static UISpriteLoader instance;
    Dictionary<string, Sprite[]> sprites = new Dictionary<string, Sprite[]>();

    void Awake(){
        if(!Application.isPlaying)
            return;
            
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSpriteSheet("HUD_Font", "Textures/HUD_Font"); // literally always used- loading it at the start.
        }
        else{
            Destroy(gameObject);
            return;
        }
    }

    void Update(){
        if(Application.isPlaying)
            return;

        instance = this;
        LoadSpriteSheet("HUD_Font", "Textures/HUD_Font");
    }


    
    public Sprite RetrieveSprite(string name, string texture, bool checkSubstring = false){
        Sprite[] returnSprites;

        if(!sprites.TryGetValue(name, out returnSprites)){
            if(Application.isPlaying)
                Debug.LogWarning($"The sprite sheet {name} doesn't exist or is unloaded.");
            return null;
        }
        
        for(int i = 0; i < returnSprites.Length; i++){
            if(returnSprites[i].name == texture)
                return returnSprites[i];
            else if(checkSubstring && returnSprites[i].name.Contains(texture))
                return returnSprites[i];
        }

        Debug.LogWarning($"The sprite {texture} in sprite sheet {name} doesn't exist or is unloaded.");
        return null;
    }

    public bool LoadSpriteSheet(string name, string path){
        if(sprites.ContainsKey(name))
            return true;

        Sprite[] retrievedSprites = Resources.LoadAll<Sprite>(path);
        
        if(retrievedSprites.Length == 0){
            Debug.LogWarning($"Requested Sprite Sheet {name} at {path} doesn't exist, or has no valid sprites.");
            return false;
        }
        
        sprites.Add(name, retrievedSprites);
        return true;
    }

    public int CountMatches(string name, string contains, bool useRegex = false){
        Sprite[] returnSprites;
        int count = 0;
        
        if(!sprites.TryGetValue(name, out returnSprites)){
            Debug.LogWarning($"The sprite sheet {name} doesn't exist or is unloaded.");
            return 0;
        }

        if(useRegex){
            Regex r = new Regex(contains);
            for(int i = 0; i < returnSprites.Length; i++){
                if(r.IsMatch(returnSprites[i].name))
                    count++;
            }
        }
        else{
            for(int i = 0; i < returnSprites.Length; i++){
                if(returnSprites[i].name.Contains(contains))
                    count++;
            }  
        }
        
        return count;
    }


    public Sprite[] GetMatches(string name, string contains, bool useRegex = false){
        Sprite[] returnSprites;
        List<Sprite> spriteList = new List<Sprite>();

        if(!sprites.TryGetValue(name, out returnSprites)){
            Debug.LogWarning($"The sprite sheet {name} doesn't exist or is unloaded.");
            return returnSprites;
        }
        
        if(useRegex){
            Regex r = new Regex(contains);
            for(int i = 0; i < returnSprites.Length; i++){
                if(r.IsMatch(returnSprites[i].name))
                    spriteList.Add(returnSprites[i]);
            }
        }
        else{
            for(int i = 0; i < returnSprites.Length; i++){
                if(returnSprites[i].name.Contains(contains))
                    spriteList.Add(returnSprites[i]);
            }
        }

        return spriteList.ToArray();
    }
}
