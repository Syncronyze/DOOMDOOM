// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class UseController : MonoBehaviour
// {
//     public float useDistance;

//     LayerMask ignoreLayer;

//     void Start(){
//         ignoreLayer = gameObject.layer;
//     }

//     void Update(){
//         if(Input.GetButtonDown("Use")){
//             RaycastHit rcHit;

//             if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out rcHit, useDistance, ~(1 << ignoreLayer))){
//                 InteractController controller;
//                 if(rcHit.collider.gameObject.TryGetComponent<InteractController>(out controller)){
//                     controller.Interact();
//                 }
//                 else{
//                     // TODO: Add sound here on fail
//                     print("Interaction failed; not a button.");
//                 }
//             }
//         }
//     }
// }
