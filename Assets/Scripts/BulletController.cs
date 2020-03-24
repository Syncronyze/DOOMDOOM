using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BulletController : MonoBehaviour
{
    public bool forceSpawnEndParticles = false;
    public float moveEndParticle = 0.5f;

    Vector3 startPos;
    Vector3 endPos;
    Rigidbody rb;
    GameObject endPrefab;
    GameObject damagePrefab;
    GameObject endWithDamagePrefab;
    Transform firedFrom;
    RaycastHit rcHit;

    float dist;
    float timeActive;
    float damageAreaSize;

    int damage;

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
            hit = Physics.Raycast(startPos, transform.TransformDirection(Vector3.forward), out rcHit, projectileDist, ~(ignoreLayer));

            if(hit){
                endPos = rcHit.point + (rcHit.normal * moveEndParticle);
                dist = rcHit.distance;       
            }
            else{
                endPos = transform.TransformDirection(Vector3.forward) * projectileDist;
                dist = projectileDist;
            }

            // Debug.DrawRay(  startPos, 
            //                 transform.TransformDirection(Vector3.forward) * dist, 
            //                 hit ? Color.green : Color.red, 
            //                 5f);
        }
        else{
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
        //print($"{(timeActive == 0 ? "Raycasted" : "Lasted " + timeActive + " s")}, travelling {dist}u, and hit something? {hit}");
        //print($"Bullet orignated at {startPos} and ended {endPos}.");
        bool damagedSomething = false;
        if(hit){
            if(raycast){
                HealthController hcTest;
                if(rcHit.transform.TryGetComponent<HealthController>(out hcTest)){
                    hcTest.TakeDamage(firedFrom, damage);
                    damagedSomething = true;
                }
            }
            else{    
                if(damagePrefab != null){ // if not a raycast, then we have to know what we hit.
                    GameObject dmg = Instantiate(damagePrefab, endPos, transform.rotation);
                    dmg.transform.localScale *= damageAreaSize;
                    //dmg.transform.SetParent(gameObject.transform);
                    ExplosionDmgController dmgC;
                    
                    if(!dmg.TryGetComponent<ExplosionDmgController>(out dmgC)){
                        Debug.LogWarning($"Non-raycasted bullet fired from {firedFrom.name} has a damage prefab, but no damage controller attached.");
                        Destroy(dmg);
                    }
                    else{
                        dmgC.damageFallOff = true;
                        dmgC.LOSCheck = true;
                        dmgC.damagePerTick = damage;
                        dmgC.origin = firedFrom;
                    }
                }
                else{
                    Debug.LogWarning($"Missing or invalid explosion damage controller, bullet will not apply damage - from {firedFrom.name}");
                }
            }

        }

        // if we hit, we spawn particles no matter what
        if(endPrefab != null && (hit || forceSpawnEndParticles)){
            GameObject particleGO;
            if(!damagedSomething || endWithDamagePrefab == null)
                particleGO = Instantiate(endPrefab, endPos, transform.rotation);
            else
                particleGO = Instantiate(endWithDamagePrefab, endPos, transform.rotation);

            ParticleController particleC;

            if(!particleGO.TryGetComponent<ParticleController>(out particleC)){
                Debug.LogWarning($"Bullet fired from {firedFrom.name} has an endPrefab particle, but no particle controller attached.");
                Destroy(particleC);
            }
            else{
                particleC.MoveParticle();
            }
        }
        // destroy is asynchronous, so just making bullet invalid after we've "ended"
        valid = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Destroy(gameObject, 0.05f);
    }

    /*
     * Once spawned, the bullet needs to become valid based on its projectile speed & distance.
     */
    public void SetVariables(float _projectileSpeed, float _projectileDist, float _damageAreaSize, int _damage, LayerMask _ignoreLayer, GameObject _endPrefab, GameObject _damagePrefab, GameObject _endWithDamagePrefab, Transform _firedFrom){
        if(rb == null)
            Destroy(gameObject);
        
        damage = _damage;
        firedFrom = _firedFrom;
        damagePrefab = _damagePrefab;
        endPrefab = _endPrefab;
        endWithDamagePrefab = _endWithDamagePrefab;
        projectileSpeed = _projectileSpeed;
        projectileDist = _projectileDist;
        ignoreLayer = _ignoreLayer;
        damageAreaSize = _damageAreaSize;

        raycast = _projectileSpeed <= 0;

        if(projectileDist <= 0)
            projectileDist = 128;

        valid = true;
    }

    void OnTriggerEnter(Collider col){
        // we're only checking this if its a valid bullet.
        if(valid && moving && firedFrom != col.transform){
            Vector3 hitPoint = col.ClosestPointOnBounds(transform.position);
            RaycastHit rcHit;

            //Debug.DrawRay(hitPoint + transform.TransformDirection(Vector3.back), transform.TransformDirection(Vector3.forward) * 2f, Color.blue, 5f);
            // moving the closest point on bounds backwards, then moving out forwards x2, this way no matter our bullet size we'll always be able to get the normal and move the endPos back slightly from the face normal
            if (Physics.Raycast(hitPoint + transform.TransformDirection(Vector3.back), transform.TransformDirection(Vector3.forward), out rcHit, 2f, ~(ignoreLayer))){
                //print("Raycast ending success, adjusting endpos slightly by normal of object");
                endPos = rcHit.point + (rcHit.normal * moveEndParticle);
            }
            else{
                //print("Raycast ending unsuccessful, no adjustment by hit object's normal");
                // backup incase the raycast doesn't find anything
                endPos = hitPoint;
            }

            hit = true;
        }
    }

}
