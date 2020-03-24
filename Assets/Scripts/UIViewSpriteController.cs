using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 *  Updated version of UIWeaponController utilizing a different solution.
 */
public class UIViewSpriteController : MonoBehaviour
{
    public MovementController playerMovementController;
    public Vector3 defaultSpritePos = new Vector3(0, 65, 0);
    public float weaponScaling = 2;
    public float FPS = 5;
    public float gunSwapSpeed = 0.25f;
    public float spriteBobMovementScale = 0.2f;
    public float spriteBobResetScale = 25;
    public float spriteBobX = -40;

    
    
    Image image;
    RectTransform rt;
    Vector3 spriteMoveTo;
    Vector3 spriteMoveFrom;
    Sprite[] sprites;
    Coroutine frameLoop;

    string currentSprite;

    bool firing;
    bool fire;
    bool gunSwap;
    bool bobbing;

    int firingLoopBegin;
    int firingLoopEnd;
    int currentIndex;
    int bobDireciton;

    string gunName;

    float gunSwapTimer;
    float bobTimer;
    
    void Awake(){
        UISpriteLoader.instance.LoadSpriteSheet("weapons", "Textures/weapons");
        image = gameObject.GetComponent<Image>();
        rt = gameObject.GetComponent<RectTransform>();
    }

    void Update(){
        ApplySpriteMovement();
    }

    IEnumerator FrameLoop(){
        while(true){
            NextFrame();
            yield return new WaitForSeconds((1 / FPS));
        }
    }

    public void ChangeGun(string _gunName, float viewSpriteHeight, float viewSpriteFPS){
        if(gunName == _gunName || string.IsNullOrEmpty(_gunName))
            return;
        
        if(frameLoop != null)
            StopCoroutine(frameLoop);

        // resetting all variables
        gunName = _gunName;
        sprites = UISpriteLoader.instance.GetMatches("weapons", $"weapons_{gunName}");
        image.enabled = true;
        fire = false;
        firing = false;
        currentIndex = 0;
        firingLoopBegin = 0;
        firingLoopEnd = sprites.Length - 1;
        FPS = viewSpriteFPS;
        //previousFrame = FPS; // allowing for instant refresh of the frame
        gunSwapTimer = 0;
        gunSwap = true;
        bobTimer = 1;
        bobbing = false;

        defaultSpritePos.y = viewSpriteHeight;

        // finding the beginning of the firing loop (which is likely 0)
        for(int i = 0; i < sprites.Length; i++){
            if(sprites[i].name[sprites[i].name.Length - 1] == 'f'){
                    firingLoopBegin = i;
                    break;
            }
        }

        frameLoop = StartCoroutine(FrameLoop());
    }

    /*
     * Firing is defined as the user wanting to fire. We can be "firing" without firing a bullet, if the RPS of the weapon is slow enough.
     */
    public void ToggleFiring(bool toggle){
        firing = toggle;
    }

    /*
     * Fire is defined as the using having fired the weapon.
     */
    public void Fire(){
        fire = true;
        currentIndex = firingLoopBegin;
    }

    void ApplySpriteMovement(){
        Vector3 moveTo = defaultSpritePos;
        float speedPercentage = playerMovementController.GetCurrentSpeedPercentage();

        if(gunSwap){
            gunSwapTimer += (Time.deltaTime / gunSwapSpeed);
            moveTo.y = Mathf.Lerp(0, defaultSpritePos.y, gunSwapTimer);
            gunSwap = gunSwapTimer < 1;
            //print($"Moving to {moveTo}, {(gunSwapTimer * 100)}% complete. {gunSwap}");
        }
        else if(speedPercentage > 0.1){
            // this allows for continuation of the previous bob if we're in the middle of one and the player came to a complete stop
            if(!bobbing && rt.anchoredPosition.y == defaultSpritePos.y){ 
                bobbing = true;
                bobTimer = 0;
                spriteBobX *= -1;
                spriteMoveFrom.x = defaultSpritePos.x;
                spriteMoveTo.x = spriteBobX;
            }

            moveTo.x = Mathf.Lerp(spriteMoveFrom.x, spriteMoveTo.x, bobTimer);
            moveTo.y += Mathf.Pow(moveTo.x, 2) * 0.01f;
            bobTimer += Time.deltaTime * speedPercentage * spriteBobMovementScale;

            if(bobTimer > 1){ // resetting bobTimer
                float temp = spriteMoveFrom.x;

                if(spriteMoveFrom.x != defaultSpritePos.x)
                    temp *= -1;
                    
                spriteMoveFrom = spriteMoveTo;
                spriteMoveTo.x = temp;
                bobTimer = 0;
            }
        }
        else{ // otherwise, not moving; resetting viewmodel back
            if(bobbing){
                bobbing = false;
                //print("STARTING RESET-----------");
                // if we're already moving towards default, we just continue on that path, no changes
                if(spriteMoveTo != defaultSpritePos){
                    spriteMoveFrom.x = spriteMoveTo.x;
                    spriteMoveTo = defaultSpritePos;
                    bobTimer = 1 - bobTimer;
                }                
            }

            if(bobTimer >= 1) // wait until we begin moving again
                return;

            //print($"Resetting view sprite, moving to {spriteMoveTo}, from {spriteMoveFrom}, {(bobTimer * 100)}% complete.");
            moveTo.x = Mathf.Lerp(spriteMoveFrom.x, spriteMoveTo.x, bobTimer);
            moveTo.y += Mathf.Pow(moveTo.x, 2) * 0.01f;
            bobTimer += Time.deltaTime * spriteBobResetScale;
        }

        // doom has an interesting behaviour (bug?) that if the gun is firing, we stop the gun, but do not stop the bob counter
        // so we only move to the anchored position if we're not firing - always move if we're gunswapping
        if((!firing && !fire) || gunSwap)
            rt.anchoredPosition = moveTo;
    }

    /*
     * Calculates what the next frame should be.
     */
    void NextFrame(){
        currentIndex++;

        if(fire){ // firing a bullet
            if(currentIndex < firingLoopBegin){ // if we're not yet at the start of the firing animation, we jump to it
                currentIndex = firingLoopBegin;
            }
            else if(currentIndex > firingLoopEnd){ // checking if we're past the amount of animations
                currentIndex = firingLoopBegin;
                fire = false; // once we are past the amount of animations, we toggle off fire and loop back to the start of the loop.
            }
        }
        else if(firing){ // if we're firing but haven't fired a shot, we wait at the beginning of the loop
            currentIndex = firingLoopBegin;
        }
        else{ // otherwise we're going through the idle animation
            if(currentIndex >= firingLoopBegin)
                currentIndex = 0;
            
        }

        SetSprite(sprites[currentIndex]);
    }

    void SetSprite(Sprite spriteToSet){
        //print($"Setting sprite to {spriteToSet}, previous sprite was {currentSprite}");
        if(spriteToSet == null){
            image.enabled = false;
            return;
        }
        else if(spriteToSet.name == currentSprite){
            return;
        }

        image.enabled = true;
        image.sprite = spriteToSet;
        currentSprite = spriteToSet.name;
        rt.sizeDelta = new Vector2(spriteToSet.rect.size.x * weaponScaling, spriteToSet.rect.size.y * weaponScaling);
    }
}
