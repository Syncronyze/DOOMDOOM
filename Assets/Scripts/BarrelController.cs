using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthController))]
public class BarrelController : MonoBehaviour
{
    public GameObject explosionDamage;
    public GameObject endParticles;
    public int damage;
    public float damageAreaSize = 1f;
    

    HealthController health;
    void Start(){
        health = GetComponent<HealthController>();
    }

    void Update(){
        if(health.health <= 0){
            if(explosionDamage != null){
                GameObject dmgGO = Instantiate(explosionDamage, transform.position, transform.rotation);
                dmgGO.transform.localScale *= damageAreaSize;
                ExplosionDmgController dmgC;

                if(!dmgGO.TryGetComponent<ExplosionDmgController>(out dmgC)){
                    Debug.LogWarning($"Barrel ({this.transform.name}) damage prefab doesn't have an attached Damage Controller.");
                    Destroy(dmgGO);
                }
                else{
                    dmgC.damageFallOff = true;
                    dmgC.LOSCheck = true;
                    dmgC.damagePerTick = damage;
                    dmgC.origin = null;
                }
            }

            if(endParticles != null){
                GameObject particleGO = Instantiate(endParticles, transform.position, transform.rotation);
                ParticleController particleC;

                if(!particleGO.TryGetComponent<ParticleController>(out particleC)){
                    Debug.LogWarning($"Barrel ({this.transform.name}) particle prefab doesn't have an attached Particle Controller.");
                    Destroy(particleC);
                }
                else{
                    particleC.MoveParticle();
                }
            }

            Destroy(gameObject);
        }
    }
}
