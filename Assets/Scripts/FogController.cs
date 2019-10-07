using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogController : MonoBehaviour
{
    public bool enableFog;
    public Color fogColor;
    public FogMode fogMode;
    public float fogDestiny;
    public float fogStartDistance;
    public float fogEndDistance;

    bool refresh;

    void Start(){
        refresh = true;
    }

    void Update(){
        if(!refresh)
            return;

        if(fogStartDistance > fogEndDistance)
            fogEndDistance = fogStartDistance * 2;

        RenderSettings.fog = enableFog;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogDensity = fogDestiny;
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;

        refresh = false;
    }

    public void refreshFog(){
        refresh = true;
    }
}
