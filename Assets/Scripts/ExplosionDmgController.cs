using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ExplosionDmgController : MonoBehaviour
{
    public bool damageFallOff;
    public bool LOSCheck;
    public int damagePerTick;
    public int ticksPerTimeStep;
    public Transform origin;
    public LayerMask ignoreLayer;

    bool damageApplied;

    List<Collider> collisions;

    void Awake(){
        collisions = new List<Collider>();
        Collider c = GetComponent<Collider>();

        if(origin == null)
            origin = transform.root;

        if(!c.isTrigger)
            c.isTrigger = true;
        
        damageApplied = false;
    }

    void FixedUpdate(){
        if(damageApplied)
            return;

        DamageAll();
        damageApplied = true;
        Destroy(gameObject);

    }

    void DamageAll(){
        for (int i = 0; i < collisions.Count; i++)
        {
            Collider c = collisions[i];
            HealthController hc = c.gameObject.GetComponent<HealthController>();
            float dmgToApply = damagePerTick;
            Vector3 closestPoint = c.ClosestPointOnBounds(transform.position);
            float distance = Vector3.Distance(closestPoint, transform.position);

            if(damageFallOff)
                dmgToApply *= 1 - Mathf.Clamp01(distance / transform.localScale.y);

            // if no hits ignoring all layers except level parts.
            if(LOSCheck && Physics.Raycast(transform.position, closestPoint - transform.position, distance, 1 << LayerMask.NameToLayer("LevelParts"))){
                dmgToApply = 0;
                //Debug.DrawRay(transform.position, (closestPoint - transform.position).normalized * distance, Color.red, 9f);
                //print("Explosion out of LOS.");
            }            

            if(dmgToApply > 0)
                hc.TakeDamage(origin, Mathf.RoundToInt(dmgToApply));   
        }
    }

    void OnTriggerEnter(Collider otherC){
        if(otherC.gameObject.GetComponent<HealthController>() != null)
            collisions.Add(otherC);
        
    }
}



