using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 *  Updated version of UIWeaponController utilizing a different solution.
 */
public class UIViewSpriteController : MonoBehaviour
{

    public float weaponScaling;
    public float FPS;

    UISpriteLoader spriteLoader;
    Image image;
    RectTransform rt;

    string currentSprite;

    bool firing;
    bool fire;

    int firingLoopBegin;
    int firingLoopEnd;
    int currentIndex;

    string gunName;

    float previousFrame;

    void Awake(){
        spriteLoader = GameObject.FindGameObjectWithTag("SpriteLoader").GetComponent<UISpriteLoader>();
        spriteLoader.LoadSpriteSheet("weapons", "Textures/weapons");
        image = gameObject.GetComponent<Image>();
        rt = gameObject.GetComponent<RectTransform>();
        FPS = FPS / 60; // converting to FPS.
    }

    void Update(){
        previousFrame += Time.deltaTime;

        if(previousFrame >= FPS)
            NextFrame();
    }

    public void ChangeGun(string _gunName){
        gunName = _gunName;
        Sprite[] sprites = spriteLoader.GetMatches("weapons", $"weapons_{gunName}");
        image.enabled = true;
        fire = false;
        firing = false;
        currentIndex = 0;
        firingLoopBegin = 0;
        firingLoopEnd = sprites.Length - 1;
        previousFrame = FPS; // allowing for instant refresh of the frame
        // finding the beginning of the firing loop (which is likely 0)
        for(int i = 0; i < sprites.Length; i++){
            if(sprites[i].name[sprites[i].name.Length - 1] == 'f'){
                    firingLoopBegin = i;
                    break;
            }
        }
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

        SetSprite(spriteLoader.RetrieveSprite("weapons", $"weapons_{gunName}_{currentIndex}", true));
        previousFrame = 0;
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
