using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Animated3DSpriteController : MonoBehaviour
{
    // sprite naming convention
    // SPRITENAME_ANIMATIONNAME<ANIMATIONSTEP>_<ANIMATIONDIRECTION>
    // if missing an animation direction parameter then it's assumed to not be 3D
    Pseudo3DSpriteController spriteController;
    
    SpriteRenderer spriteRenderer;
    List<AnimationProperties> animations;
    
    Coroutine animationLoop;

    AnimationProperties currentAnimation;
    AnimationProperties nextAnimation;

    public string spriteName;
    [Min(0)]
    public float defaultFPS = 4;
    public AnimationProperties defaultAnimation;
    public bool debugPrint = false;
    
    
    int currentAnimationIndex;
    bool goingBackwards;
    int animationStep;
    bool hasLooped;
    bool canBe3D;

    void Awake(){
        canBe3D = TryGetComponent<Pseudo3DSpriteController>(out spriteController);
        spriteRenderer = GetComponent<SpriteRenderer>();
        // currentAnimationProperties = new List<Sprite>();
        // currentAnimationProperties3D = new List<Sprite[]>();
        UISpriteLoader.instance.LoadSpriteSheet(spriteName, $"Textures/{spriteName}");
        animations = new List<AnimationProperties>();
        AnimationProperties blank = new AnimationProperties("", null, null, 0, -1, true, false, false, false);
        animations.Add(blank);
        currentAnimation = blank;

        if(!string.IsNullOrEmpty(defaultAnimation.name)){
            ChangeAnimation(defaultAnimation.name, defaultAnimation.is3D, defaultAnimation.loopingAnimation, defaultAnimation.FPS, defaultAnimation.interruptableAnimation, defaultAnimation.twoWayAnimation);
        }
    }

    public void DeleteCache(){
        animations = null;
    }

    IEnumerator AnimationLoop(){
        while(  currentAnimation.loopingAnimation || // current animation is looping
                (!currentAnimation.loopingAnimation && animationStep < currentAnimation.frameCount) || // if its not looping, loop until it ends
                !string.IsNullOrEmpty(nextAnimation.name) && animationStep == currentAnimation.frameCount){ // if it is looping, but there's a next animation, wait until current ends
            
            NextFrame();
            yield return new WaitForSeconds(1 / currentAnimation.FPS);
        }

        if(!string.IsNullOrEmpty(nextAnimation.name)){
            AnimationProperties newAnimation = nextAnimation;
            nextAnimation = animations[0]; // MoveToAnimation cancels this loop, need to set the NextAnimation to blank beforehand
            MoveToAnimation(newAnimation);
        }

        yield break;
    }

    public void ChangeAnimation(string newAnimationName, bool _is3D = false, bool _loopingAnimation = false, float _FPS = -1, bool _interruptableAnimation = true, bool _twoWayAnimation = false){
        if(newAnimationName == currentAnimation.name || newAnimationName == nextAnimation.name || !canBe3D && _is3D)
            return;
    
        //print($"Current animation {currentAnimation.name}, wanting {newAnimationName}, current animation is interruptable? {currentAnimation.interruptableAnimation} on {transform.parent.gameObject.name}");

        if(string.IsNullOrEmpty(newAnimationName)){
            Debug.LogError($"Cannot set animation on {gameObject.name} as it is a null or empty string; resuming previous animation.");
            return;
        }

        bool isCached = false;
        AnimationProperties requestedAnimation = animations[0];

        for(int i = 0; i < animations.Count; i++){
            if(animations[i].name == newAnimationName){
                requestedAnimation = animations[i];
                isCached = true;
                //print("Cached animation. " + newAnimationName);
                break;
            }
        }

        if(!isCached){
            //print("Not cached; creating sprite list");
            Sprite[] spriteList = null;
            Sprite[] spriteList3D = null;
            Sprite[][] spriteList3DArr = null;
            int frameCount;
            float FPS = _FPS <= 0 ? defaultFPS : _FPS;
            
            if(_is3D){
                spriteList3D = UISpriteLoader.instance.GetMatches(spriteName, @"^" + spriteName + "_" + newAnimationName + "\\d_\\d\\d$", true);
                spriteList3DArr = new Sprite[spriteList3D.Length / 5][];

                if(spriteList3D.Length % 5 != 0){
                    Debug.LogError($"Animation \"{newAnimationName}\" has an incorrect amount of sprites for a 3D sprite - requires 5 directional sprites for every frame; resuming previous animation.");
                    return;
                }

                for(int i = 0; i < spriteList3D.Length / 5; i++){
                    Sprite[] arr = new Sprite[5];
                    for(int j = 0; j < 5; j++){
                        arr[j] = spriteList3D[j + (5 * i)];
                    }
                    spriteList3DArr[i] = arr;
                }

                frameCount = spriteList3D.Length / 5;
            }
            else{
                spriteList = UISpriteLoader.instance.GetMatches(spriteName, @"^" + spriteName + "_" + newAnimationName + "\\d$", true);
                frameCount = spriteList.Length;
            }
            
            if(frameCount == 0){
                Debug.LogWarning($"Animation \"{newAnimationName}\" for {spriteName} doesn't exist or has a discrepancy being marked as 3D when it's not; resuming previous animation.");
                return;
            }

            requestedAnimation = new AnimationProperties(newAnimationName, spriteList, spriteList3DArr, frameCount, FPS, _interruptableAnimation, _is3D, _loopingAnimation, _twoWayAnimation);
            animations.Add(requestedAnimation);
        }

        if(string.IsNullOrEmpty(requestedAnimation.name)){
            Debug.LogWarning($"Animation \"{newAnimationName}\" for {spriteName} is invalid.");
            return;
        }

        // if it was a completed uninterruptable animation with no nextAnimation, the AnimationLoop would've already ended so we just move on
        if(currentAnimation.interruptableAnimation || (animationStep >= currentAnimation.frameCount)){
            MoveToAnimation(requestedAnimation);
        }
        else{
            nextAnimation = requestedAnimation;
        }
        
    }

    void MoveToAnimation(AnimationProperties toAnimation){
        // print($"Changing animation from {currentAnimation.name} to {toAnimation.name} on {transform.parent.gameObject.name} at {Time.realtimeSinceStartup}");
        // print(toAnimation);

        if(animationLoop != null)
                StopCoroutine(animationLoop);

        currentAnimation = toAnimation;
        animationStep = 0;
        if(canBe3D)
            spriteController.pauseUpdate = !currentAnimation.is3D;
        animationLoop = StartCoroutine(AnimationLoop());
    }

    void NextFrame(){
        if(animationStep >= currentAnimation.frameCount){ // end of current animation
            if(currentAnimation.twoWayAnimation){
                animationStep = currentAnimation.frameCount - 2; 
                goingBackwards = true;
            }
            else{
                animationStep = 0;
            }
        }
        else if(animationStep < 0){
            goingBackwards = false;
            animationStep = 1;
        }

        #if UNITY_EDITOR
            if(debugPrint)
               print($"Current animation: {currentAnimation}, current frame going to {animationStep}, animation length {currentAnimation.frameCount} at {Time.realtimeSinceStartup}");
        #endif

        if(currentAnimation.is3D && canBe3D){
            Sprite[] animationNextStep;
            animationNextStep = currentAnimation.spriteList3D[animationStep];
            spriteController.sprites = animationNextStep;
            spriteController.ForceUpdateFrame();

        }
        else{
            Sprite animationNextStep;
            animationNextStep = currentAnimation.spriteList[animationStep];
            spriteRenderer.sprite = animationNextStep;
        }

        if(goingBackwards)
            animationStep--;
        else
            animationStep++;
    }
}

[System.Serializable]
public struct AnimationProperties{
    public string name;
    public Sprite[] spriteList { get; private set; }
    public Sprite[][] spriteList3D { get; private set; }
    public int frameCount { get; private set; }
    public float FPS;
    public bool interruptableAnimation;
    public bool is3D;
    public bool loopingAnimation;
    public bool twoWayAnimation;

    public AnimationProperties(string _name, Sprite[] _spriteList, Sprite[][] _spriteList3D, int _frameCount, float _FPS, bool _interruptableAnimation, bool _is3D, bool _loopingAnimation, bool _twoWayAnimation){
        name = _name;
        spriteList = _spriteList;
        spriteList3D = _spriteList3D;
        frameCount = _frameCount;
        FPS = _FPS;
        interruptableAnimation = _interruptableAnimation;
        is3D = _is3D;
        loopingAnimation = _loopingAnimation;
        twoWayAnimation = _twoWayAnimation;
    }

    public override string ToString(){
        return $"{(loopingAnimation ? "Looping" : "Not looping")} {(is3D ? "3D" : "")} animation {name} with {frameCount} frames. FPS is {FPS}, and {(interruptableAnimation ? "is" : "isn\'t")} interruptable.";
    }
}
