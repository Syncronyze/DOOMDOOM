﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIColorOverlayController : MonoBehaviour
{
    Color color;
    Image image;
    
    float alpha;
    float minAlpha;
    float maxAlpha;
    float timeToFadeOut;
    float fadeTimer;

    float stepTimer = 0.05f;

    void Start(){
        image = GetComponent<Image>();
    }

    IEnumerator Fade(){
        while(fadeTimer > 0f){
            fadeTimer -= (stepTimer / timeToFadeOut);
            color.a = Mathf.Clamp(Mathf.Lerp(0, alpha, fadeTimer), minAlpha, maxAlpha);
            image.color = color;
            yield return new WaitForSeconds(stepTimer);
        }
        image.enabled = false;
        yield break;
    }

    public void SetColor(Color _color, float _alpha, float _timeToFadeOut){
        SetColor(_color, _alpha, _timeToFadeOut, 0.0f, 1.0f);
    }

    public void SetColor(Color _color, float _alpha, float _timeToFadeOut, float _minAlpha, float _maxAlpha){
        //print($"Setting color overlay to color {_color}, starting an alpha of {_alpha}, and taking {_timeToFadeOut} seconds to fade");
        StopCoroutine(Fade());
        color = _color;
        alpha = _alpha;
        minAlpha = _minAlpha;
        maxAlpha = _maxAlpha;
        timeToFadeOut = _timeToFadeOut;
        fadeTimer = 1.0f;
        image.enabled = true;
        StartCoroutine(Fade());
    }
}
