using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIEndScreenController : MonoBehaviour
{
    public UIFontController levelNameDisplay;

    public UIFontController killPercentageDisplay;
    public UIFontController pickupPercentageDisplay;
    public UIFontController secretPercentageDisplay;

    public UIFontController timeDisplay;
    public UIFontController parDisplay;

    public float countingSpeedInterval = 0.04f;
    public int countingInterval = 4;
    public float pauseBetweenAnimations = 0.5f;

    IEnumerator[] animationCoroutines;
    bool activeCoroutine;
    bool skipActiveCorutine;
    float currentValue;
    

    void Update(){
        if(!activeCoroutine)
            return;

        if(Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)){
            skipActiveCorutine = true;
        }  
    }

    public IEnumerator BeginAnimations(){
        activeCoroutine = false;
        levelNameDisplay.SetValue(GlobalLevelVariables.instanceOf.levelName);
        animationCoroutines = new IEnumerator[5]{   CountPercentageAnimation(killPercentageDisplay, GlobalLevelVariables.instanceOf.PercentageOfEnemiesKilled()),
                                                    CountPercentageAnimation(pickupPercentageDisplay, GlobalLevelVariables.instanceOf.PercentageOfSecretsFound()),
                                                    CountPercentageAnimation(secretPercentageDisplay, GlobalLevelVariables.instanceOf.PercentageOfPickupsAcquired()),
                                                    CountTimeAnimation(timeDisplay, GlobalLevelVariables.instanceOf.levelTotal),
                                                    CountTimeAnimation(parDisplay, GlobalLevelVariables.instanceOf.levelPar)};

        for(int i = 0; i < animationCoroutines.Length; i++){
            activeCoroutine = true;
            skipActiveCorutine = false;
            yield return new WaitForSeconds(pauseBetweenAnimations);
            StartCoroutine(animationCoroutines[i]);
            yield return new WaitWhile(() => activeCoroutine);
        }

        yield break;
    }


    IEnumerator CountPercentageAnimation(UIFontController controller, int max){
        int currentVal = 0;
        while(max >= currentVal){
            if(skipActiveCorutine)
                break;
            

            controller.SetValue(currentVal + "%");
            currentVal += countingInterval;

            yield return new WaitForSeconds(countingSpeedInterval);
        }

        controller.SetValue(max + "%");
        activeCoroutine = false;
        yield break;
    }

    IEnumerator CountTimeAnimation(UIFontController controller, float maxInSeconds){
        int currentVal = 0;
        while(maxInSeconds >= currentVal){
            if(skipActiveCorutine)
                break;
            

            controller.SetValue(ConvertTime(currentVal));
            currentVal += countingInterval;

            yield return new WaitForSeconds(countingSpeedInterval);
        }
        
        controller.SetValue(ConvertTime(maxInSeconds));
        activeCoroutine = false;
        yield break;
    }

    string ConvertTime(float timeInSeconds){
        TimeSpan t = TimeSpan.FromSeconds(timeInSeconds);
        return string.Format("{0:00}:{1:00}", t.Minutes, t.Seconds);
    }
}