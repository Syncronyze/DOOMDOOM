using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
  *     Custom SpriteLoader for unity to retrieve a sprite out of multiple loaded spritesheets.
  */
public class UISpriteLoader : MonoBehaviour
{

    Dictionary<string, Sprite[]> sprites = new Dictionary<string, Sprite[]>();
    
    public Sprite RetrieveSprite(string name, string texture, bool containsSubstring = false){
        Sprite[] returnSprites;

        if(!sprites.TryGetValue(name, out returnSprites)){
            print($"The sprite sheet {name} doesn't exist or is unloaded.");
            return null;
        }
        
        for(int i = 0; i < returnSprites.Length; i++){
            if(returnSprites[i].name == texture)
                return returnSprites[i];
            else if(containsSubstring && returnSprites[i].name.Contains(texture))
                return returnSprites[i];
        }
        //print($"The sprite {texture} in sprite sheet {name} doesn't exist or is unloaded.");
        return null;
    }

    public void LoadSpriteSheet(string name, string path){
        if(sprites.ContainsKey(name))
            return;

        Sprite[] retrievedSprites = Resources.LoadAll<Sprite>(path);
        if(retrievedSprites.Length == 0){
            print($"Requested Sprite Sheet {name} at {path} doesn't exist, or has no valid sprites.");
            return;
        }
        
        sprites.Add(name, retrievedSprites);
    }

    public int CountMatches(string name, string texture){
        Sprite[] returnSprites;
        int count = 0;
        if(!sprites.TryGetValue(name, out returnSprites)){
            print($"The sprite sheet {name} doesn't exist or is unloaded.");
            return 0;
        }
        
        for(int i = 0; i < returnSprites.Length; i++){
            if(returnSprites[i].name.Contains(texture))
                count++;
        }

        return count;
    }

    public Sprite[] GetMatches(string name, string texture){
        Sprite[] returnSprites;

        if(!sprites.TryGetValue(name, out returnSprites)){
            print($"The sprite sheet {name} doesn't exist or is unloaded.");
            return returnSprites;
        }

        List<Sprite> spriteList = new List<Sprite>();

        for(int i = 0; i < returnSprites.Length; i++){
            if(returnSprites[i].name.Contains(texture))
                spriteList.Add(returnSprites[i]);
        }
        return spriteList.ToArray();
    }
}
