using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pseudo3DSpriteController : MonoBehaviour
{
    //         f     4
    //          \ 0 /
    //         f     3
    //          / 1 \
    //         f     2
    // 0 - -22.5 & 22.5
    // 4 - 22.5 & 67.5
    // 3 - 67.5 & 112.5
    // 2 - 112.5 & 157.5
    // 1 - 157.5 & -157.5
    SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    int currentFrame = 0;

    void Awake(){
        spriteRenderer = GetComponent<SpriteRenderer>();    
    }

    void Update(){
        if(sprites.Length != 5){
            print("Pseudo sprite controller requires exactly 5 sprites.");
            return;
        }
        UpdateFrame();
    }

    void UpdateFrame(){
        float thisY = UseNegativeDegrees(transform.localEulerAngles.y);
        int previousFrame = currentFrame;

        spriteRenderer.flipX = thisY > 0;
        thisY = Mathf.Abs(thisY);

        if(thisY < 22.5)
            currentFrame = 0;
        else if(thisY < 67.5)
            currentFrame = 4;
        else if(thisY < 112.5)
            currentFrame = 3;
        else if(thisY < 157.5)
            currentFrame = 2;
        else
            currentFrame = 1;
            
        if(previousFrame != currentFrame)
            spriteRenderer.sprite = sprites[currentFrame];
    }

    float UseNegativeDegrees(float input){
        if(input > 180){
            return input - 360;
        }

        return input;
    }
}
