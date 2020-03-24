// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// /*

//     -------------------OLD CODE - NO LONGER IN USE, USE UIVIEWSPRITECONTROLLER


//  */
// public class UIWeaponController : MonoBehaviour
// {   
//     UISpriteLoader spriteLoader;
//     public float weaponScaling;
//     public float frameSpeed;

//     Image image;
//     RectTransform rt;

//     int firingLoopStart;
//     int firingLoopEnd;
//     int currentSpriteIndex;

//     string currentGun;

//     bool firing;
//     bool animating;

//     float RPS;
//     float previousFrameTimer;
//     float animationCycleTimer; // this signals the start of an animation cycle

//     void Awake(){
//         spriteLoader = GameObject.FindGameObjectWithTag("SpriteLoader").GetComponent<UISpriteLoader>();
//         spriteLoader.LoadSpriteSheet("weapons", "Textures/weapons");
//         image = gameObject.GetComponent<Image>();
//         rt = gameObject.GetComponent<RectTransform>();
//         previousFrameTimer = 0;
//     }

//     void Update(){
//         // if the end of the firing loop is index 0, or less than, it means we have 0 or 1 sprites
//         // and thus no animation is needed.
//         if(string.IsNullOrEmpty(currentGun) || firingLoopEnd <= 0)
//             return;

//         AnimationCycle();

//         previousFrameTimer = Mathf.Clamp(previousFrameTimer + Time.deltaTime, 0, 128);
//         animationCycleTimer = Mathf.Clamp(animationCycleTimer + Time.deltaTime, 0, 128);
//     }

//     public void ChangeGun(string gunName, float _RPS){
//         gunName = gunName.ToLower();
//         if(gunName == currentGun)
//             return;
//         currentGun = gunName;
//         RPS = _RPS;

//         currentSpriteIndex = 0;
//         firingLoopStart = 0;
//         firingLoopEnd = spriteLoader.CountMatches("weapons", $"weapons_{currentGun}") - 1;
//         ToggleFiring(false);
//         animating = false;
//         animationCycleTimer = RPS;
//         SetSprite(spriteLoader.RetrieveSprite("weapons", $"weapons_{currentGun}_0", true));
//         // move sprite up
//     }

//     public void ToggleFiring(bool toggle){
//         //print("toggle firing " + (toggle ? "on" : "off"));
//         firing = toggle;
//     }

//     void AnimationCycle(){
//         if(previousFrameTimer > frameSpeed){
//             // if we're on the start of the firing loop, but we're not firing
//             // we cancel the animation.
//             if(animating || firing){
//                 if(currentSpriteIndex != firingLoopStart || (currentSpriteIndex == firingLoopStart && animationCycleTimer >= RPS))
//                     WeaponAnimation();
//             }
//             else if(currentSpriteIndex != 0){
//                 currentSpriteIndex = 0;
//                 animationCycleTimer = RPS;
//                 SetSprite(spriteLoader.RetrieveSprite("weapons", $"weapons_{currentGun}_0", true));
//             }
//         }
//     }

//     void SetSprite(Sprite spriteToSet){
//         print($"Setting sprite of gun {currentGun}, ({currentSpriteIndex}) to {spriteToSet}");
//         if(spriteToSet == null){
//             image.enabled = false;
//             return;
//         }

//         image.enabled = true;
//         image.sprite = spriteToSet;
//         rt.sizeDelta = new Vector2(spriteToSet.rect.size.x * weaponScaling, spriteToSet.rect.size.y * weaponScaling);
//     }

//     void WeaponAnimation(){
//         if(currentSpriteIndex > firingLoopEnd){
//             currentSpriteIndex = firingLoopStart;
//             animationCycleTimer = 0; // completed the animation.
//         }
//         // if we're not firing, but in the middle of the animation, we're still "animating" and the animation must finish.
//         animating = (currentSpriteIndex == firingLoopStart) && (currentSpriteIndex != firingLoopEnd && firing);
        

//         Sprite sprite = spriteLoader.RetrieveSprite("weapons", $"weapons_{currentGun}_{currentSpriteIndex}", true);
//         // as we're passing through, if a sprite is marked with an 'f' at the end, this is the beginning of the firing loop.
//         // otherwise we assume it's 0.
//         if(sprite != null && sprite.name[sprite.name.Length - 1] == 'f')
//                 firingLoopStart = currentSpriteIndex;

//         SetSprite(sprite);
//         currentSpriteIndex++;
//         previousFrameTimer = 0;
//     }
// }
