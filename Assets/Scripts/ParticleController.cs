using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public int[] particleSpriteFrames;
    public float lifeTime;
    public float animationTime;
    public float scale;
    public Vector3 moveTo;

    UISpriteLoader spriteLoader;
    SpriteRenderer spriteRenderer;
    
    int currentSpriteIndex;
    float frameTimer;
    float frameSpeed;
    bool valid;

    void Start(){
        spriteLoader = GameObject.FindGameObjectWithTag("SpriteLoader").GetComponent<UISpriteLoader>();
        spriteLoader.LoadSpriteSheet("particles", "Textures/particle_sprites");
        spriteRenderer = GetComponent<SpriteRenderer>();
        frameTimer = 0;
        currentSpriteIndex = 0;
    }

    void Update(){
        if(!valid)
            return;

        if(lifeTime <= 0){
            Destroy(gameObject);
            return;
        }

        if(frameTimer >= frameSpeed && currentSpriteIndex < particleSpriteFrames.Length)
            NextFrame();

        transform.position = Vector3.MoveTowards(transform.position, moveTo, Time.deltaTime);

        frameTimer += Time.deltaTime;
        lifeTime -= Time.deltaTime;

    }

    public void MoveParticle(){
        if(animationTime > lifeTime)
            lifeTime = animationTime;

        transform.localScale *= scale;
        frameSpeed = animationTime / particleSpriteFrames.Length;
        moveTo += transform.position;
        valid = true;
    }

    void NextFrame(){
        Sprite sprite = spriteLoader.RetrieveSprite("particles", $"particle_sprites_{particleSpriteFrames[currentSpriteIndex]}", true);

        if(sprite == null)
            return;

        spriteRenderer.sprite = sprite;
        currentSpriteIndex++;
        frameTimer = 0;
    }
}
