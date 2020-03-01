using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Pseudo3DSpriteController))]
public class Animated3DSpriteController : MonoBehaviour
{
    // sprite naming convention
    // SPRITENAME_ANIMATIONNAME<ANIMATIONSTEP>_<ANIMATIONDIRECTION>
    // if missing an animation direction parameter then it's not 3D
    Pseudo3DSpriteController spriteController;
    UISpriteLoader spriteLoader;
    SpriteRenderer spriteRenderer;
    List<Sprite[]> currentAnimationSpriteList3D;
    List<Sprite> currentAnimationSpriteList;

    public string spriteName;
    [Min(0)]
    public float defaultFPS;
    public string currentAnimation{ get; private set; }
    
    bool loopAnimation;
    
    int animationLength;
    int animationStep;
    float previousFrame;
    bool valid;
    bool is3D;
    bool interruptableAnimation;
    bool hasLooped;
    string nextAnimation;
    int nextAnimationLength;
    bool isNext3D;
    bool isNextInterruptable;
    float currentFPS;
    float nextFPS;

    void Start(){
        spriteController = GetComponent<Pseudo3DSpriteController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentAnimationSpriteList = new List<Sprite>();
        currentAnimationSpriteList3D = new List<Sprite[]>();
        spriteLoader = GameObject.FindGameObjectWithTag("SpriteLoader").GetComponent<UISpriteLoader>();
        valid = spriteLoader.LoadSpriteSheet(spriteName, $"Textures/{spriteName}");
    }

    void Update(){
        if((!loopAnimation && animationStep >= animationLength) || !valid || string.IsNullOrEmpty(currentAnimation))
            return;
        
        previousFrame += Time.deltaTime;
       // print($"current animation is {currentAnimation} previous frame was {previousFrame}s, current fps {currentFPS} (or {(1 / currentFPS)}s between frames), next frame? {previousFrame >= (1 / currentFPS)}");
        if(previousFrame >= (1 / currentFPS)){ // if we have 2 frames per second, we need to change frames every 0.5 seconds
            previousFrame = 0;
            NextFrame();
        }
    }

    public void ChangeAnimation(string animation, bool _is3D, bool _loopAnimation, float _FPS = -1, bool _interruptableAnimation = false){
        if(animation == currentAnimation || nextAnimation == animation)
            return;

        if(string.IsNullOrEmpty(animation)){
            spriteRenderer.sprite = null;
            spriteController.pauseUpdate = true;
            currentAnimation = animation;
            return;
        }
        int spriteCount = 0;
        
        if(_is3D)
            spriteCount = spriteLoader.CountMatches(spriteName, @"^" + spriteName + "_" + animation + "\\d_\\d\\d$", true);
        else
            spriteCount = spriteLoader.CountMatches(spriteName, @"^" + spriteName + "_" + animation + "\\d$", true);
        
        if(spriteCount == 0){
            Debug.LogWarning($"Animation \"{animation}\" for {spriteName} doesn't exist or has a discrepancy being marked as 3D; resuming previous animation.");
            return;
        }
        
        // checking if the sprite is marked for 3D, and is divisible by 5        
        if(_is3D && spriteCount % 5 != 0){
            Debug.LogError($"Animation \"{animation}\" has an incorrect amount of sprites for a 3D sprite - requires 5 directional sprites for every frame; resuming previous animation.");
            return;
        }
        
        // if we're waiting to change animations until the end of the current one, skip this step and do it once the current animation is over
        if(!interruptableAnimation){
            animationStep = 0;
            currentAnimationSpriteList = new List<Sprite>();
            currentAnimationSpriteList3D = new List<Sprite[]>();

            if(_FPS < 0)
                currentFPS = defaultFPS;
            else
                currentFPS = _FPS;
            
            currentAnimation = animation;
            is3D = _is3D;
            animationLength = is3D ? spriteCount / 5 : spriteCount;
            //print(animation + ", " + spriteCount + ", " + (spriteCount / 5) + ", " + animationLength);
            spriteController.pauseUpdate = !is3D;
            interruptableAnimation = _interruptableAnimation;

            previousFrame = currentFPS;
        }
        else{
            if(_FPS < 0)
                nextFPS = defaultFPS;

            nextAnimation = animation;
            isNextInterruptable = _interruptableAnimation;
            isNext3D = _is3D;
            nextAnimationLength = _is3D ? spriteCount / 5 : spriteCount;
        }
        //print(animation + ", " + animationLength);
        loopAnimation = _loopAnimation;
    }

    void NextFrame(){
        //print($"Current animation: {currentAnimation}, current frame going to {animationStep}, animation length {animationLength} at {Time.realtimeSinceStartup}");
        if(animationStep >= animationLength){ // end of current animation
            if(!string.IsNullOrEmpty(nextAnimation)){ // moving to new animation
                //print($"Previous animation ended, changing animation from {currentAnimation} to {nextAnimation} at {Time.realtimeSinceStartup}");

                currentFPS = nextFPS;
                animationStep = 0;
                currentAnimationSpriteList = new List<Sprite>();
                currentAnimationSpriteList3D = new List<Sprite[]>();

                currentAnimation = nextAnimation;
                is3D = isNext3D;
                animationLength = nextAnimationLength;
                spriteController.pauseUpdate = !is3D;
                interruptableAnimation = isNextInterruptable;

                previousFrame = 0;

                nextAnimation = null;
                return;
            }
            else{
                // looping again
                animationStep = 0;
            }
        }
        
        if(is3D){
            Sprite[] animationNextStep;

            if(currentAnimationSpriteList3D.Count < animationLength){
                animationNextStep = spriteLoader.GetMatches(spriteName, $"{spriteName}_{currentAnimation}{(animationStep)}_");
                currentAnimationSpriteList3D.Add(animationNextStep);
            }
            else{
                animationNextStep = currentAnimationSpriteList3D[animationStep];
            }
    
            if(animationNextStep.Length == 5){
                spriteController.sprites = animationNextStep;
                spriteController.ForceUpdateFrame();
            }
            else{
                Debug.LogError($"3D Animation {currentAnimation} has an invalid/missing frame or invalid/missing directional sprite - must be 5 sprites for every direction. ({animationStep}).");
            }
        }
        else{
            Sprite animationNextStep;

            if(currentAnimationSpriteList.Count < animationLength){
                animationNextStep = spriteLoader.RetrieveSprite(spriteName, $"{spriteName}_{currentAnimation}{(animationStep)}");
                currentAnimationSpriteList.Add(animationNextStep);
            }
            else{
                animationNextStep = currentAnimationSpriteList[animationStep];
            }
            
            if(animationNextStep != null)
                spriteRenderer.sprite = animationNextStep;
            else
                Debug.LogError($"Animation {currentAnimation} has an invalid or missing frame ({animationStep}).");
            
        }

        animationStep++;
    }
}
