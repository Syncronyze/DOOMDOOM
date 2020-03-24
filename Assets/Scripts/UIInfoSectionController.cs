using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInfoSectionController : MonoBehaviour
{
    public float messageTimeActive;
    //public float testMessageTimer = 5f;
    //float testTimer;

    List<Message> messageQueue;
    UIFontController[] controllers;

    bool syncMessages;

    void Start(){
        messageQueue = new List<Message>();
        controllers = transform.GetComponentsInChildren<UIFontController>();
        syncMessages = true;

        if(controllers.Length == 0){
            Debug.LogError("UI information messages require children with attached UIFontControllers.");
        }
    }

    void Update(){
        if(controllers.Length == 0)
            return;
        
        MessageExpiry();
        SyncMessages();
    }

    
    // public void TestMessage(){
    //     testTimer += Time.deltaTime;
    //     if(testTimer < testMessageTimer)
    //         return;

    //     testTimer = 0;
    //     string[] testCharacters = new string[9]{"pick ", "up ", "42 ", "shells ", "shotgun ", "keycard ", "wow ", "random ", "   ~!@#$%^&*()_++:[]\\,./   "};
    //     string testmsg = "";
    //     float characterAmount = Random.Range(1f, 4f);
    //     for(int i = 0; i < characterAmount; i++){
    //         testmsg += testCharacters[(int)Random.Range(0f, testCharacters.Length)];
    //     }

    //     SetMessage(testmsg);
    // }

    public void SetMessage(string message){
        Message msg = new Message(message);
        messageQueue.Add(msg);
        syncMessages = true;
    }

    void MessageExpiry(){
        if(messageQueue.Count == 0)
            return;

        Message m = messageQueue[0];
        // it's possible there's two expiring at the same time, still doing the oldest no matter what
        if(m.expiry > 0 && Time.time > m.expiry){
            //print($"Message expired {m.message} at {m.expiry}");
            messageQueue.RemoveAt(0);
            syncMessages = true;
        }
    }

    void SyncMessages(){
        if(!syncMessages)
            return;

        for(int i = 0; i < controllers.Length; i++){
            if(i < messageQueue.Count){
                // setting the expiry as it comes up on screen
                if(messageQueue[i].expiry == -1)
                    messageQueue[i].expiry = Time.time + messageTimeActive;

                controllers[i].SetValue(messageQueue[i].message);
            }
            else{
                controllers[i].SetValue("");
            }
        }
    }

    class Message{
        public string message{ get; private set; }
        public float expiry;

        public Message(string _message){
            message = _message;
            expiry = -1;
        }

        public void SetExpiry(float _expiry){
            expiry = _expiry;
        }
    }
}
