using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Pseudo3DSpriteController : MonoBehaviour
{

    //         f     1
    //          \ 0 /
    //         f     2
    //          / 4 \
    //         f     3
    // 0 - -22.5 & 22.5
    // 1 - 22.5 & 67.5
    // 2 - 67.5 & 112.5
    // 3 - 112.5 & 157.5
    // 4 - 157.5 & -157.5
    SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    public bool oppositeFlip;
    public bool pauseUpdate;
    int currentFrame = 0;
    bool forceFrameUpdate;

    void Awake(){
        spriteRenderer = GetComponent<SpriteRenderer>();    
        if(sprites.Length != 5)
            Debug.LogWarning("Pseudo sprite controller requires exactly 5 sprites.");
    }

    void LateUpdate(){
        if(sprites.Length != 5)
            return;
        
        if(!pauseUpdate)
            UpdateFrame();

        forceFrameUpdate = false;
    }

    public void ForceUpdateFrame(){
        forceFrameUpdate = true;
    }

    void UpdateFrame(){
        float thisY = UseNegativeDegrees(transform.localEulerAngles.y);
        int previousFrame = currentFrame;

        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.red, 1f);

        // flip only if we're not at 0 or 1, as we never flip behind or infront.
        if(oppositeFlip)
            spriteRenderer.flipX = (thisY > 0) && !(currentFrame == 0 || currentFrame == 4);
        else
            spriteRenderer.flipX = (thisY < 0) && !(currentFrame == 0 || currentFrame == 4);
            
        thisY = Mathf.Abs(thisY);

        if(thisY < 22.5)
            currentFrame = 0;
        else if(thisY < 67.5)
            currentFrame = 1;
        else if(thisY < 112.5)
            currentFrame = 2;
        else if(thisY < 157.5)
            currentFrame = 3;
        else
            currentFrame = 4;
            
        if(previousFrame != currentFrame || forceFrameUpdate)
            spriteRenderer.sprite = sprites[currentFrame];
    }

    float UseNegativeDegrees(float input){
        if(input > 180){
            return input - 360;
        }

        return input;
    }
}
