using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public InteractController button;
    public Vector3 moveTo;
    public float timeToOpen;

    Vector3 defaultPos;

    float openTimer;

    bool previousState;

    void Start(){
        previousState = button.isOn;
        defaultPos = transform.position;
        moveTo += defaultPos;
    }

    void Update(){
        if(button.isOn && openTimer < 1){
            openTimer = Mathf.Clamp((Time.deltaTime + openTimer) / timeToOpen, 0, 1);
            transform.position = Vector3.Lerp(defaultPos, moveTo, openTimer);
        }
        else if(!button.isOn && openTimer > 0){
            openTimer = Mathf.Clamp((openTimer - Time.deltaTime) / timeToOpen, 0, 1);
            transform.position = Vector3.Lerp(defaultPos, moveTo, openTimer);
        }
    }
}
