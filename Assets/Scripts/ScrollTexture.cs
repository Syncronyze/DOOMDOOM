using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ScrollTexture : MonoBehaviour
{
    public Vector2 scrollSpeed;
    public string textureName;

    MeshRenderer r;
    Material material;
    Vector2 offset = Vector2.zero;
    bool valid = false;

    void Start(){
        r = GetComponent<MeshRenderer>();
        valid = TryGetMaterial(textureName, out material);
        if(!valid)
            print($"Scrolling material {textureName} doesn't exist.");

    }

    void FixedUpdate(){
        if(!valid)
            return;
        
        offset += (scrollSpeed * Time.deltaTime);
        material.SetTextureOffset("_MainTex", offset);

        if(offset.x > 1 || offset.y > 1)
            offset = Vector2.zero;
    }

    bool TryGetMaterial(string name, out Material m){
        Material[] materials = r.materials;
        for(int i = 0; i < materials.Length; i++){
            Material current = materials[i];
            if(current.mainTexture.name == name){
                m = current;
                return true;
            }
        }
        m = null;
        return false;
    }
}
