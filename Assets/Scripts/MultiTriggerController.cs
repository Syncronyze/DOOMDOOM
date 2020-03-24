using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/*
 * Multipurpose trigger controller to perform any action on any target once something triggers (moves into or uses) it. Mostly used for doors, switches and pickups.
 */
[RequireComponent(typeof(Collider))]
public class MultiTriggerController : MonoBehaviour
{
    public Action[] actions;
    public bool triggerOnceOnly;
    public bool triggered{ get; private set; }

    void Start(){
        gameObject.tag = "Trigger";
        triggered = false;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerController playerController;

        if(player != null && player.TryGetComponent<PlayerController>(out playerController)){
            for(int i = 0; i < actions.Length; i++){
                Action a = actions[i];
                if(a.messageReceiver == null && a.playerOnly)
                    a.messageReceiver = playerController;
            }
        }
        
    }

    public void Interact(GameObject o){
        for(int i = 0; i < actions.Length; i++){
            Action a = actions[i];

            if( a.messageReceiver == null || 
                a.playerOnly && o.gameObject.tag != "Player" || 
                a.NPCOnly && o.gameObject.tag == "Player" || 
                Time.realtimeSinceStartup < a.previousActionPerformedAt + a.minActionInterval ||
                triggered && triggerOnceOnly ||
                !a.onUseOnly){
                    continue;
            }

            Trigger(a);
        }

        gameObject.SetActive(!(triggered && triggerOnceOnly));
    }

    void OnTriggerEnter(Collider c){
        for(int i = 0; i < actions.Length; i++){
            Action a = actions[i];

            if( a.messageReceiver == null || 
                a.onUseOnly || 
                !a.onTriggerEnter || 
                a.playerOnly && c.gameObject.tag != "Player" || 
                a.NPCOnly && c.gameObject.tag == "Player" ||
                Time.realtimeSinceStartup < (a.previousActionPerformedAt + a.minActionInterval) ||
                triggered && triggerOnceOnly){
                    continue;
            }

             Trigger(a);
        }

        gameObject.SetActive(!(triggered && triggerOnceOnly));
    }
    
    void OnTriggerExit(Collider c){
        for(int i = 0; i < actions.Length; i++){
            Action a = actions[i];

            if( a.messageReceiver == null || 
                a.onUseOnly || 
                !a.onTriggerExit || 
                a.playerOnly && c.gameObject.tag != "Player" || 
                a.NPCOnly && c.gameObject.tag == "Player" ||
                Time.realtimeSinceStartup < (a.previousActionPerformedAt + a.minActionInterval) ||
                triggered && triggerOnceOnly){
                    continue;
            }

             Trigger(a);
        }

        gameObject.SetActive(!(triggered && triggerOnceOnly));
    }

    void OnTriggerStay(Collider c){
        for(int i = 0; i < actions.Length; i++){
            Action a = actions[i];

            if( a.messageReceiver == null || 
                a.onUseOnly || 
                !a.onTriggerStay || 
                a.playerOnly && c.gameObject.tag != "Player" || 
                a.NPCOnly && c.gameObject.tag == "Player" || 
                Time.realtimeSinceStartup < (a.previousActionPerformedAt + a.minActionInterval) ||
                triggered && triggerOnceOnly){
                    continue;
            }
            Trigger(a);
        }

        gameObject.SetActive(!(triggered && triggerOnceOnly));
    }

    void Trigger(Action a){
        MethodInfo mi = a.messageReceiver.GetType().GetMethod(a.action);
        a.previousActionPerformedAt = Time.realtimeSinceStartup;

        if(mi == null){
            Debug.LogError($"Trigger action {a.action} for {a.messageReceiver} is invalid and cannot be retrieved.");
            return;
        }
        
        object[] parameters = null;
        if(a.values.Length > 0)
            parameters = new object[1]{a.values}; // packing in the value array into the object array to be able to unpack it later

        var returnVar = mi.Invoke(a.messageReceiver, parameters);
        

        // if the return var is a boolean, but it came back as false, then the action wasn't completed for one reason or another, so the trigger will need to reattempt if it's a once-only trigger.
        if((returnVar is bool) && !(bool)returnVar){
            return;
        }

        triggered = true;
        if(a.onceOnly){
            a.previousActionPerformedAt = Mathf.Infinity; // will never be able to trigger this action again, but the trigger will be left in tact
        }
    }

    [System.Serializable]
    public class Action{
        public string action;
        public string[] values;
        public MonoBehaviour messageReceiver;
        public bool onTriggerEnter;
        public bool onTriggerExit;
        public bool onTriggerStay;
        public bool playerOnly;
        public bool NPCOnly;
        public bool onUseOnly;
        public bool onceOnly; // action-specific once only

        public float minActionInterval;
        [HideInInspector]
        public float previousActionPerformedAt;
    }


}
