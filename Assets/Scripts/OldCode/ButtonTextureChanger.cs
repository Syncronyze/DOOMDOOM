// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ButtonTextureChanger : MonoBehaviour
// {
//     public int indexToChange;
//     public Material onMaterial;
//     public Material offMaterial;

//     MeshRenderer r;

//     bool previousState;

//     void Start(){
//         r = GetComponent<MeshRenderer>();
//         previousState = button.isOn;
//         UpdateButtonTexture();
//     }

//     void Update(){
//         if(button.isOn == previousState) // checking if the state has changed
//             return;

//         UpdateButtonTexture();
//         previousState = button.isOn;
//     }

//     void UpdateButtonTexture(){
//         if(button.isOn)
//             ChangeMaterials(indexToChange, onMaterial);
//         else
//             ChangeMaterials(indexToChange, offMaterial);
//     }

//     void ChangeMaterials(int index, Material material){
//         Material[] copiedMatArray = r.materials;
//         copiedMatArray[index] = material;
//         r.materials = copiedMatArray;
//     }
// }
