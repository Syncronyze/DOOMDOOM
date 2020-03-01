using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerObjectController : MonoBehaviour
{
    [Range(0, 1)]
    public float flickerChance;
    public float flickerInterval;

    public GameObject[] objects;

    void Start(){
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker(){
        while(true){
            // 1 will always be on, 0 always off
            bool turnOn = Random.value > flickerChance;

            for(int i = 0; i < objects.Length; i++){
                objects[i].SetActive(turnOn);
            }

            

            yield return new WaitForSeconds(flickerInterval);
        }

    }
}
