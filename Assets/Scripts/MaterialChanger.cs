using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MaterialChanger : MonoBehaviour
{
    public Material[] materialCycle;
    public int targetMaterialIndex;
    public float materialChangeInterval;
    public bool forceCycleOnly;

    Material[] currentMaterials;
    MeshRenderer meshRenderer;
    int materialIndex;
    int currentMaterialIndex;
    

    Coroutine matChangeLoop;

    void Start(){
        meshRenderer = GetComponent<MeshRenderer>();
        currentMaterials = meshRenderer.materials;
        if(!forceCycleOnly)
            matChangeLoop = StartCoroutine(MatChangeLoop()); 
    }

    IEnumerator MatChangeLoop(){
        while(!(forceCycleOnly && materialIndex >= materialCycle.Length)){
            CycleMaterial();
            yield return new WaitForSeconds(materialChangeInterval);
        }
        yield break;
    }

    public void ForceNewCycle(){
        if(matChangeLoop != null)
            StopCoroutine(matChangeLoop);

        materialIndex = 0;
        SetMaterialByIndex(materialIndex);
        matChangeLoop = StartCoroutine(MatChangeLoop());
    }

    public void CycleMaterial(){
        materialIndex++;

        if(materialIndex >= materialCycle.Length)
            materialIndex = 0;
        
        SetMaterialByIndex(materialIndex);
    }

    public void SetMaterialByIndex(int index){
        if(index == currentMaterialIndex)
            return;

        currentMaterials[targetMaterialIndex] = materialCycle[index];
        currentMaterialIndex = index;
        meshRenderer.materials = currentMaterials;
    }
}
