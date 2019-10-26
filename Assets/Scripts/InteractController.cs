using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A controller that keeps track of the toggle behaviour of doors & buttons.
 */
 [RequireComponent(typeof(Collider))]
public class InteractController : MonoBehaviour
{
    public float holdFor;

    [HideInInspector]
    public bool isOn;

    float holdTimer;

    void LateUpdate(){
        // if holdFor is less than or equal to 0, it's a togglable door.
        if(holdFor > 0 && holdTimer <= 0)
            isOn = false;
        
        if(holdTimer > 0)
            holdTimer -= Time.deltaTime;
    }

    public void Interact(){
        print("interacting");
        isOn = !isOn;
        holdTimer = holdFor;
    }
}
