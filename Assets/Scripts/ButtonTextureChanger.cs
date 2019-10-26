using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (InteractController))]
public class ButtonTextureChanger : MonoBehaviour
{
    public int indexToChange;
    public Material onMaterial;
    public Material offMaterial;

     MeshRenderer r;
    InteractController button;

    bool previousState;

    void Start(){
        button = GetComponent<InteractController>();
        r = GetComponent<MeshRenderer>();
        previousState = button.isOn;
    }

    void Update(){
        if(button.isOn == previousState) // checking if the state has changed
            return;
        
        if(button.isOn)
            ChangeMaterials(indexToChange, onMaterial);
        
        else
            ChangeMaterials(indexToChange, offMaterial);
        

        previousState = button.isOn;
    }

    void ChangeMaterials(int index, Material material){
        Material[] copiedMatArray = r.materials;
        copiedMatArray[index] = material;
        r.materials = copiedMatArray;
    }
}
