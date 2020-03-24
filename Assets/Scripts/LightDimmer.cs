using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LightDimmer : MonoBehaviour
{
    public Light[] lights;
    public float timeToFadeOut = 1f;
    public float minIntensity = 0f;
    public float maxIntensity = 1f;
    [Range(0, 1)]
    public float dimChance = 1f;
    public float minTimeBetweenLoops = 0f;
    public float dimStepIntervals = 0.015f; // time in between doing steps, higher number will be more choppy
    public bool twoWayDim = true;

    
    float dimTimer;

    // NOTE: Flicker effect can be achieved by higher dimStepInterval and 0 timeToFadeOut
    void Start(){
        StartCoroutine(DimLoop());
    }
    
    IEnumerator DimLoop(){
        if(!twoWayDim && timeToFadeOut == 0)
            Debug.LogWarning($"Light dimmer {gameObject.name} at {transform.position} has lights always off, as it has no timeToFadeOut and no twoWayDim.");
        
        while(true){
            bool startDimProcess = dimChance > Random.value;
            //print("start dimming process? " + startDimProcess);
            if(startDimProcess){
                dimTimer = 1f; // assuming lights are starting fully brightened
                while(dimTimer > 0f){
                    //print(dimTimer);
                    dimTimer -= timeToFadeOut > 0 ? (dimStepIntervals / timeToFadeOut) : 1;
                    for(int i = 0; i < lights.Length; i++){
                        lights[i].intensity = Mathf.Lerp(minIntensity, maxIntensity, dimTimer);
                    }

                    yield return new WaitForSeconds(dimStepIntervals);
                }

                if(twoWayDim){
                    while(dimTimer < 1f){
                        dimTimer += timeToFadeOut > 0 ? (dimStepIntervals / timeToFadeOut) : 1;
                        for(int i = 0; i < lights.Length; i++){
                            lights[i].intensity = Mathf.Lerp(minIntensity, maxIntensity, dimTimer);
                        }

                        yield return new WaitForSeconds(dimStepIntervals);
                    }
                }
            }

            yield return new WaitForSeconds(minTimeBetweenLoops);
        }
    }
}
