using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Vector3 moveTo;
    public float timeToOpen;
    public float stayOpenFor;

    float doorMoveIntervals = 0.015f;

    Vector3 defaultPos;
    Coroutine moveDoor;

    float openTimer;
    float openFor;

    bool isOpen;

    void Start(){
        isOpen = false;
        defaultPos = transform.position;
        moveTo += defaultPos;
    }

    IEnumerator MoveDoor(){
        openTimer = Mathf.Clamp(openTimer, 0, 1);

        while(true){
            if(isOpen && openTimer < 1){ // door should be open, moving it there
                openTimer += timeToOpen == 0 ? 1 : doorMoveIntervals / timeToOpen;
                transform.position = Vector3.Lerp(defaultPos, moveTo, openTimer);
                if(openTimer > 1){
                    openFor = 0; // once we're done opening, we start the timer for holding
                }
            }
            else if(!isOpen && openTimer > 0){ // door should be closed
                openTimer -= timeToOpen == 0 ? 0 : doorMoveIntervals / timeToOpen;
                transform.position = Vector3.Lerp(defaultPos, moveTo, openTimer);
            }
            else if(isOpen && stayOpenFor > 0){ // holding the door open
                openFor += doorMoveIntervals;
                if(openFor > stayOpenFor){
                    isOpen = false; // then closing it once the timer has run its course
                }
            }
            else{
                yield break; // done moving door
            }

            yield return new WaitForSeconds(doorMoveIntervals);
        }
    }

    void RestartCoroutine(){
        if(moveDoor != null)
            StopCoroutine(moveDoor);
        
        moveDoor = StartCoroutine(MoveDoor());
    }

    public void ToggleDoor(){
        isOpen = !isOpen;
        RestartCoroutine();
    }

    public void OpenDoor(){
        isOpen = true;
        RestartCoroutine();
    }

    public void CloseDoor(){
        isOpen = false;
        RestartCoroutine();
    }

    public void DisableDoor(){
        if(moveDoor != null)
            StopCoroutine(moveDoor);
            
        gameObject.SetActive(false);
    }

    // TODO: When we find a keycard, enable the trigger for the door!!
}
