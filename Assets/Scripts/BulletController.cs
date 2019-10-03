using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    Vector3 startPos;
    Vector3 endPos;
    Rigidbody rb;

    float dist;
    float timeActive;

    bool raycast;
    bool hit;
    bool valid;
    bool moving;
    
    LayerMask ignoreLayer;

    float projectileSpeed, projectileDist;
    const float MAXTIMEACTIVE = 10; // maximum amount of time a bullet may exist, in case of a really slow projectile and long distance (in seconds)

    void Awake(){
        startPos = transform.position;
        timeActive = 0;
        dist = 0;
        moving = false;
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void FixedUpdate(){
        // we're going to only begin the bullet process once we've got a valid bullet.
        if(!valid)
            return;
        
        // if it's valid and we're already moving, we'll not move again.
        if(!moving)
            Move();
        
        if(raycast || hit || dist > projectileDist || timeActive > MAXTIMEACTIVE)
            End();
        

        dist = Vector3.Distance(startPos, transform.position);
        timeActive += Time.deltaTime;
        
    }

    /*
     *  Begins the movement of the bullet either raycast or physics-based.
     */
    void Move(){
        if(raycast){
            RaycastHit rcHit;
            hit = Physics.Raycast(startPos, transform.TransformDirection(Vector3.forward), out rcHit, projectileDist, ~ignoreLayer);

            if(hit){
                endPos = rcHit.point;
                dist = rcHit.distance;
            }
            else{
                endPos = transform.TransformDirection(Vector3.forward) * projectileDist;
                dist = projectileDist;
            }

            Debug.DrawRay(  startPos, 
                            transform.TransformDirection(Vector3.forward) * dist, 
                            hit ? Color.green : Color.red, 
                            0.2f);
        }
        else{
            Physics.IgnoreLayerCollision(ignoreLayer, LayerMask.NameToLayer("Bullet"));
            rb.AddRelativeForce(Vector3.forward * projectileSpeed);
        }

        moving = true;
    }

    /*
     * This is the end of the bullet, will be able to spawn a prefab at the endPos of the bullet
     * which will allow for decal application or explosions (or both)
     * ultimately destroys this gameObject.
     */
    void End(){
        print($"{(timeActive == 0 ? "Raycasted" : "Lasted " + timeActive + " s")}, travelling {dist}u, and hit something? {hit}");
        print($"Bullet orignated at {startPos} and ended {endPos}.");

        // TODO: spawn bullet "end" prefab
        

        Destroy(gameObject);
    }

    /*
     * Once spawned, the bullet needs to become valid based on its projectile speed & distance.
     */
    public void SetVariables(float _projectileSpeed, float _projectileDist, LayerMask _ignoreLayer){
        if(rb == null)
            Destroy(gameObject);

        projectileSpeed = _projectileSpeed;
        projectileDist = _projectileDist;
        // this is the layer that is firing this bullet, to ignore whatever is firing it.
        ignoreLayer = _ignoreLayer;

        raycast = _projectileSpeed <= 0;

        if(projectileDist <= 0)
            projectileDist = 128;

        valid = true;
    }

    void OnTriggerEnter(Collider col){
        // we're only checking this if its a valid bullet..
        if(valid && moving){
            endPos = transform.position;
            hit = true;
        }
    }

}
