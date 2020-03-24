using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/* https://doomwiki.org/wiki/Monster_behavior */ 
public class EnemyAIController : MonoBehaviour
{
    [Header("Navigation")]
    public float hearingDistance = 16;
    public float sightDistance = 64;
    [Range(0,180)]
    public float LOSAngle = 90;
    public float dectectionOffset = 1.2f;
    public LayerMask LOSObstacles; // layers considered to be obstacles
    public LayerMask walkableLayers;
    public float distanceToAvoidObstacles = 4f;
    public float directionChangeInterval = 1f;
    public float injuredStatePause = 0.1f;
    public int minStateChangesBeforeNextAttack = 2;
    public float minStateChangeInterval = 1f;
    public int debugRayAmount = 3;
    public float maxAggro = 10f;
    public float distanceToChargeIntoMelee;
    [Header("Animation FPS")]
    public float shootAnimationFPS = 2f;
    public float meleeAnimationFPS = 2f;
    public float walkAnimationFPS = 4f;
    public float injuryAnimationFPS = 16f;
    public float dethAnimationFPS = 8f;
    public float gibAnimationFPS = 8f;

    [Header("Weapons")]
    public GunController weapon;
    public GunController meleeWeapon;

    public GameObject onDeathDrop;

    public EnemyState state{ get; private set; }

    Animated3DSpriteController animator;
    HealthController healthController;

    HealthController player;
    HealthController target;
    MovementController movementController;
    Vector3 velocity;
    Direction direction;

    Coroutine targetLoop;
    
    float currentAggro;
    float height;
    float radius;
    float stepHeight;
    float directionChangeTimer;
    int stateChangesSincePreviousAttack;
    float stateChangeTimer;
    float distanceToTarget;
    bool hasLOS;

    const float RENABLE_MOVEMENT_CONTROLLER_DISTANCE = 16f; // when the player moves within sightDistance+this we reenable movementController updates in anticipation of needing it, so really far enemies are completely sleeping

    // this is because the player's camera is naturally higher than the entire body. 
    // for fairness, we offset it a bit so the player's camera will always see the monster as the player gets detected.
    Vector3 detectionOffsetV3;

    void Start(){
        if(GlobalLevelVariables.instanceOf != null)
            GlobalLevelVariables.instanceOf.AddEnemy();
        direction = new Direction();
        detectionOffsetV3 = new Vector3(0, dectectionOffset, 0);
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<HealthController>();
        //weapon = transform.GetComponentInChildren<GunController>();
        movementController = transform.GetComponent<MovementController>();
        healthController = transform.GetComponent<HealthController>();
        animator = transform.GetComponentInChildren<Animated3DSpriteController>();

        state = EnemyState.Sleep;
        directionChangeTimer = 0f;
        Sleep();

        Ammo enemyAmmo = new Ammo(AmmoType.Enemy);

        if(weapon != null){
            weapon.SetAmmo(enemyAmmo);
            weapon.projectileDist = sightDistance;
            // only fire once per state change
            weapon.UpdateFireRate(minStateChangeInterval * 60f);
        }
        
        if(meleeWeapon != null){
            meleeWeapon.SetAmmo(enemyAmmo);
            if(meleeWeapon.projectileDist >= weapon.projectileDist)
                meleeWeapon.projectileDist *= 0.25f; // melee weapon needs to be less than the regular weapon for the probabilities to be accurate
            
            meleeWeapon.UpdateFireRate(minStateChangeInterval * 60f);
        }
        

        CharacterController cc = transform.GetComponent<CharacterController>();
        height = cc.height * transform.localScale.y;
        stepHeight = cc.stepOffset + cc.skinWidth + 0.05f;

        if(hearingDistance > sightDistance)
            hearingDistance = sightDistance;

        // only the greater of the two on x or y actually changes the result of the radius of the character controller.
        if(Mathf.Abs(transform.localScale.x) >= Mathf.Abs(transform.localScale.z))
            radius = cc.radius * transform.localScale.x;
        else
            radius = cc.radius * transform.localScale.z;        
    }

    void Update(){
        // always setting target to be whatever it got hit by last
        if(healthController.damagedBy != null)
           SetTarget(healthController.damagedBy);

        if(UpdateState())
           Act();
    }

    void Act(){
        movementController.SetMovementVector(Vector3.zero);

        if(state == EnemyState.Sleep){
            weapon.ToggleFiring(false);
            if(meleeWeapon != null)
                meleeWeapon.ToggleFiring(false);

            Sleep();
        }
        else if(state == EnemyState.Dead){
            DETH();
        }
        else if(state == EnemyState.Injured){
            animator.ChangeAnimation("hit", true, false, injuryAnimationFPS, false);
        }
        else{
            UpdateAggro();

            if(state == EnemyState.RangedAttack){
                weapon.transform.LookAt(target.transform);
                UpdateDirection(false);
                
                if(!weapon.firing){
                    animator.ChangeAnimation("sleep", true, false);
                    weapon.DisableShot(0.33f);
                }

                weapon.ToggleFiring(true);
                
                if(weapon.BulletFiredOnFrame())
                    animator.ChangeAnimation("shoot", true, false, shootAnimationFPS);
                //else
                //    animator.ChangeAnimation("sleep", true, false);
            }
            else if(state == EnemyState.MeleeAttack){
                meleeWeapon.transform.LookAt(target.transform);
                UpdateDirection(false);
                meleeWeapon.ToggleFiring(true);

                if(meleeWeapon.BulletFiredOnFrame())
                    animator.ChangeAnimation("melee", true, false, meleeAnimationFPS);
                //else
                //    animator.ChangeAnimation("sleep", true, false);
            }
            else if(state == EnemyState.Chase){
                weapon.ToggleFiring(false);

                if(meleeWeapon != null)
                    meleeWeapon.ToggleFiring(false);

                UpdateDirection(true);
                Move();
            }
        }
    }

    bool UpdateState(){
        EnemyState previousState = state;

        if(healthController.health <= 0){
            state = EnemyState.Dead;
            return true; // forcing a state change always
        }

        if(target == null || currentAggro <= 0 || target.health <= 0){
            state = EnemyState.Sleep;
            return previousState == state;
        }

        if(healthController.damagedBy != null){
            state = EnemyState.Injured;
            stateChangeTimer = minStateChangeInterval - injuredStatePause;
            return previousState == state;
        }

        stateChangeTimer += Time.deltaTime;
        
        if(minStateChangeInterval > stateChangeTimer)
            return previousState == state;
        
        stateChangeTimer = 0;
        stateChangesSincePreviousAttack++;
        
        if(minStateChangesBeforeNextAttack >= stateChangesSincePreviousAttack){
            state = EnemyState.Chase;
            return previousState == state;
        }

        stateChangesSincePreviousAttack = 0;
        hasLOS = LOSCheck();
        distanceToTarget = DistanceToTarget();

        if(hasLOS){
            if(distanceToTarget < sightDistance){
                // first we decide if we can use melee at all
                if(meleeWeapon != null){
                    // then if we're already in range of melee, we simply melee attack
                    if(distanceToTarget < meleeWeapon.projectileDist){
                        state = EnemyState.MeleeAttack;
                    }
                    else if(weapon == null){
                        state = EnemyState.Chase; // not in range of melee, and no ranged weapon, so always run
                    }
                    else if(distanceToTarget < distanceToChargeIntoMelee){ // checking if we care to charge into melee
                        float chanceToRunForMelee = Mathf.Pow(distanceToTarget / distanceToChargeIntoMelee, 4.0f);
                        //print($"Distance to target {distanceToTarget}, distance the enemy can charge {distanceToChargeIntoMelee}, chance to chase for melee {1 - chanceToRunForMelee}");

                        if(Random.value > chanceToRunForMelee){
                            state = EnemyState.Chase;
                        }
                        else{
                            state = EnemyState.RangedAttack;
                        }
                    } // otherwise just use ranged attack
                    else{
                        state = EnemyState.RangedAttack;
                    }
                }
                else{
                    state = EnemyState.RangedAttack;
                }
            }
            else{ // not in sight range, so must chase to fire again
                state = EnemyState.Chase;
            }
        }
        else{
            state = EnemyState.Chase; // has a valid target with aggro and no LOS
        }

        return previousState == state;
    }

    void UpdateAggro(){  
        if(!hasLOS){
            //print("No LOS to target, reducing aggro");
            currentAggro -= Time.deltaTime;
        }
        else{ // resetting the aggro amount everytime we're back in LOS to effectively chase the target, but giving up after some attempts to regain LOS
            currentAggro = maxAggro;
        }
    }

    bool LOSCheck(){
        return !Physics.Linecast(transform.position + detectionOffsetV3, target.transform.position, LOSObstacles);
    }
    
    void Move(){
        if(directionChangeTimer != 0 && !TryMove(direction)) // checking if the move is still valid if we haven't checked this frame
            return;

        movementController.SetMovementVector(Vector3.forward);
        animator.ChangeAnimation("walk", true, true, walkAnimationFPS);
    }

    void UpdateDirection(bool checkMove){
        directionChangeTimer += Time.deltaTime;

        if((directionChangeTimer < directionChangeInterval))
            return;

        Direction newDirection = new Direction();
        float angleToTarget = AngleToTarget(target.transform);
        newDirection.SetDirection(angleToTarget);

        // if we're just changing direction, but not actually checking if the direciton we're facing is a valid move forward, then we stop here
        if(!checkMove || !movementController.isGrounded()){
            direction = newDirection;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, (float)direction.GetDirection() * 45, transform.eulerAngles.z);
            return;
        }

        directionChangeTimer = 0f;

        Directions oppositeDirection = newDirection.GetOppositeDirection();

        // if both the old direction and new direction is invalid moves, we need to find another direction
        if(!TryMove(newDirection)){ // trying new direction first incase there's a more direct route to the target besides our old direction (target moved)
            if(!TryMove(direction)){
                int directionTest = (int)direction.GetDirection();
                bool goClockwise = LocalAngleToTarget(target.transform) < 0;
                goClockwise = Random.value < .21 ? !goClockwise : goClockwise; // accurate to doom, we have an approx 21% to go the opposite direction that's most direct to target

                //print("Going clockwise? " + goClockwise);
                // since there's always going to be 8 directions, 
                // we can just assume we're going through 7 directions at most.
                for(int i = 0; i < 7; i++){
                    if(goClockwise)
                        directionTest--;
                    else
                        directionTest++;
                    
                    newDirection.SetDirection(directionTest);
                    //print(newDirection.GetDirection() + ", direction supposed to be " + directionTest);
                    //print("Current attempt " + newDirection.GetDirection() + " , opposite direction is " + oppositeDirection + " old direction " + direction.GetDirection());
                    // we don't want to move directly backwards from the target, and we've already checked the previous direction, so skip that
                    if(newDirection.GetDirection() == oppositeDirection || newDirection.GetDirection() == direction.GetDirection())
                        continue;

                    if(TryMove(newDirection)){
                        direction.SetDirection(newDirection.GetDirection());
                        transform.eulerAngles = new Vector3(transform.eulerAngles.x, (float)direction.GetDirection() * 45, transform.eulerAngles.z);
                        return;
                    }
                } 
            }
            else{
                return; // old direction worked, no need to change anything.
            }
        }
        else{
           // print("Direct to player, no obstacles detected.");
            direction.SetDirection(newDirection.GetDirection());
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, (float)direction.GetDirection() * 45, transform.eulerAngles.z);
            return;
        }

        // trying to move backwards
        newDirection.SetDirection(oppositeDirection);
        if(TryMove(newDirection)){
            direction.SetDirection(newDirection.GetDirection());
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, (float)direction.GetDirection() * 45, transform.eulerAngles.z);
            return;
        }
        
        //Debug.LogError($"No possible move, stuck enemy ({transform.name}) at {transform.position}");
        //Sleep();
    }
    
    IEnumerator AquireTargetLoop(){
        movementController.enabled = false;
        while(target == null){
            //print(gameObject.name);
            float distanceToPlayer = DistanceToTransform(player.transform);
            if(distanceToPlayer > sightDistance + RENABLE_MOVEMENT_CONTROLLER_DISTANCE){
                yield return new WaitForSeconds(1f); // if the player is way too far, there's no point in checking again for awhile
            }
            else if(distanceToPlayer > sightDistance){
                movementController.enabled = true; 
                yield return new WaitForSeconds(0.5f);
            }

            float angleToPlayer = LocalAngleToTarget(player.transform);

            RaycastHit rchit;
            
            if(!Physics.Linecast(transform.position + detectionOffsetV3, player.transform.position, out rchit, LOSObstacles)){
                //print(rchit.transform.gameObject);
                if(distanceToPlayer <= hearingDistance){
                    SetTarget(player.transform);
                }
                else if(distanceToPlayer <= sightDistance && (angleToPlayer >= -LOSAngle && angleToPlayer <= LOSAngle)){
                    //Debug.DrawLine(transform.position + detectionOffsetV3, player.transform.position, Color.magenta, 0.00833333333f * debugRayAmount);
                    SetTarget(player.transform);
                    
                }
            }
            // in range, but cannot see the player yet
            yield return new WaitForSeconds(0.25f);
        }

        yield break;
    }

    bool TryMove(Direction d){
        // checking the width and height, if any of these fail, we absolutely cannot move forward.
        if( RaycastWithDebug(transform.position + new Vector3(0, (height / 2), 0), d.GetV3Direction(), (radius + distanceToAvoidObstacles), (walkableLayers), Color.green) || // middle
            RaycastWithDebug(transform.position + d.GetV3Direction(d.GetDirectionAt(2)) * radius, d.GetV3Direction(), (radius + distanceToAvoidObstacles), (walkableLayers), Color.cyan) || // shifting to the right by adding the direction 2 to the right + radius (eg, north + 2 would be west)
            RaycastWithDebug(transform.position + d.GetV3Direction(d.GetDirectionAt(-2)) * radius, d.GetV3Direction(), (radius + distanceToAvoidObstacles), (walkableLayers), Color.cyan)){ // left
                return false;
        }

        // check if the enemy can move forward and fit on the lower bounds of where we're trying to move to
        if( !RaycastWithDebug(transform.position + new Vector3(0, -(height / 2) + radius, 0), d.GetV3Direction(), (radius + distanceToAvoidObstacles), (walkableLayers), Color.red)){
                        // begins at the bottom of the character controller plus the radius otherwise causes issues, then moved forward by a quarter of the distance to avoid obstacles
                return RaycastWithDebug(transform.position + new Vector3(0, -(height / 2) + radius, 0) + (d.GetV3Direction() * (radius + (distanceToAvoidObstacles * 0.25f))), Vector3.down, stepHeight + radius, (walkableLayers), Color.blue);
        }

        // // offsetting the checks by the step height to see if there's enough room to step up onto whatever is infront, if either check is a failure, this move is invalid
        if( !RaycastWithDebug(transform.position + new Vector3(0, -(height / 2) + stepHeight, 0), d.GetV3Direction(), (radius + distanceToAvoidObstacles), (walkableLayers), Color.yellow)){
                return RaycastWithDebug(transform.position + new Vector3(0, -(height / 2) + stepHeight, 0) + d.GetV3Direction() * (radius + distanceToAvoidObstacles), Vector3.down, stepHeight, (walkableLayers), Color.white); // checking walk space
            }

        //print("Not a valid move in direction " + d.GetDirection());
        return false;
    }

    bool RaycastWithDebug(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, Color color){
        Debug.DrawRay(origin, direction * maxDistance, color, 0.00833333333f * debugRayAmount);
        return Physics.Raycast(origin, direction, maxDistance, layerMask);;
    }

    float AngleToTarget(Transform _target){
        Vector3 from = new Vector3(_target.position.x, 0, _target.position.z);
        Vector3 to = new Vector3(transform.position.x, 0, transform.position.z);
        float angle = Vector3.SignedAngle(from - to, Vector3.forward, Vector3.down);

        if(angle < 0)
            angle += 360;

        return angle;
    }

    float LocalAngleToTarget(Transform _target){
        Vector3 from = new Vector3(_target.position.x, 0, _target.position.z);
        Vector3 to = new Vector3(transform.position.x, 0, transform.position.z);
        return Vector3.SignedAngle(from - to, transform.forward, Vector3.down);
    }

    float DistanceToTransform(Transform _target){
        Vector3 toPos = new Vector3(_target.position.x, transform.position.y, _target.position.z);
        return Vector3.Distance(transform.position, toPos);
    }

    float DistanceToTarget(){
        if(target == null)
            return Mathf.Infinity;

        return DistanceToTransform(target.transform);
    }

    void Sleep(){
        if(targetLoop != null)
            StopCoroutine(targetLoop);
        currentAggro = 0;
        target = null;
        targetLoop = StartCoroutine(AquireTargetLoop());
        animator.ChangeAnimation("sleep", true, false);
    }

    void DETH(){
        if(onDeathDrop != null){
            Vector3 dropPos = transform.position;
            dropPos += transform.forward * 1.25f;
            dropPos.y -= (height / 2);
            dropPos.y += 0.5f;
            Instantiate(onDeathDrop, dropPos, transform.rotation);
        }

        if(GlobalLevelVariables.instanceOf != null)
            GlobalLevelVariables.instanceOf.EnemyKilled();

        if(healthController.health <= -35)
            animator.ChangeAnimation("gib", false, false, gibAnimationFPS, false);
        else
            animator.ChangeAnimation("deth", false, false, dethAnimationFPS, false);

        if(weapon != null)
            weapon.enabled = false;

        if(meleeWeapon != null)
            meleeWeapon.enabled = false;

        gameObject.layer = LayerMask.NameToLayer("DeadEnemy"); // still interacts with the environment but not with the player or other enemies.
        healthController.enabled = false;
        animator.DeleteCache();
        this.enabled = false;
    }

    void SetTarget(Transform _target){
        if(_target == null || _target == transform || !_target.TryGetComponent<HealthController>(out target))
            return;
        
        stateChangesSincePreviousAttack = 0;
        directionChangeTimer = directionChangeInterval;
        currentAggro = maxAggro;
        movementController.enabled = true;
        if(targetLoop != null)
            StopCoroutine(targetLoop);
    }

    #if UNITY_EDITOR
        void OnDrawGizmosSelected(){
            Handles.color = new Color(0.5f, 0.5f, 1f, 0.25f);
            Handles.DrawSolidArc(transform.position + detectionOffsetV3, transform.up, transform.forward, LOSAngle, sightDistance);
            Handles.DrawSolidArc(transform.position + detectionOffsetV3, transform.up, transform.forward, -LOSAngle, sightDistance);
            Handles.color = new Color(0.5f, 1f, 0.5f, 0.25f);
            Handles.DrawSolidDisc(transform.position, Vector3.up, hearingDistance);
            Handles.DrawWireDisc(transform.position, Vector3.up, sightDistance + RENABLE_MOVEMENT_CONTROLLER_DISTANCE);
        }
    #endif
}


class Direction{
    Directions direction;

    public float GetAngle(){
        return (int)direction * 45;
    }

    public Directions GetDirection(){
        return direction;
    }

    public Directions GetOppositeDirection(){
        int _direction = (int)direction + 4;
        return IntegerToDirection(_direction);
    }

    public Directions GetDirectionAt(int directionsAway){
        int _direction = (int)direction + directionsAway;
        return IntegerToDirection(_direction);
    }

    public void SetDirection(float angle){
        SetDirection((int)RoundToNearestOrdinalDirection(angle));
    }

    public void SetDirection(int _direction){
        direction = IntegerToDirection(_direction);
    }

    public void SetDirection(Directions _direction){
        direction = _direction;
    }

    public Vector3 GetV3Direction(){
        return GetV3Direction(direction);
    }

    public Vector3 GetV3Direction(Directions _direction){
        switch(_direction){
            case Directions.North: return Vector3.forward;
            case Directions.NorthWest: return new Vector3(0.7f, 0, 0.7f);
            case Directions.West: return Vector3.right;
            case Directions.SouthWest: return new Vector3(0.7f, 0, -0.7f);
            case Directions.South: return Vector3.back;
            case Directions.SouthEast: return new Vector3(-0.7f, 0, -0.7f);
            case Directions.East: return Vector3.left;
            case Directions.NorthEast: return new Vector3(-0.7f, 0, 0.7f);
        }
        return Vector3.forward;
    }

    Directions IntegerToDirection(int _direction){
        if(_direction > 7)
            _direction %= 8;
        else if(_direction < 0)
            _direction = 8 - ((_direction * -1) % 8);
        
        return (Directions)_direction;
    }

    float RoundToNearestOrdinalDirection(float toRound){
        float rounded = Mathf.Round((toRound / 45));
        if(rounded == 8)
            return 0;

        return rounded;
    }

}

enum Directions{
    North,
    NorthWest,
    West,
    SouthWest,
    South,
    SouthEast,
    East,
    NorthEast
}


/*
/// actual doom chasedir

void P_NewChaseDir (mobj_t*	actor)
{
    fixed_t	deltax;
    fixed_t	deltay;
    
    dirtype_t	d[3];
    
    int		tdir;
    dirtype_t	olddir;
    
    dirtype_t	turnaround;

    if (!actor->target)
	    I_Error ("P_NewChaseDir: called with no target");
		
    olddir = (dirtype_t)actor->movedir; ///- getting previous direction
    turnaround=opposite[olddir]; ///- this ensures we're not going to be moving backward

    deltax = actor->target->x - actor->x;
    deltay = actor->target->y - actor->y;

    ///- d1 is east/west, if we have a dir both east/west AND south/north, we're moving diagonally.
    if (deltax>10*FRACUNIT)
	    d[1]= DI_EAST;
    else if (deltax<-10*FRACUNIT)
	    d[1]= DI_WEST;
    else
	    d[1]=DI_NODIR;

    if (deltay<-10*FRACUNIT)
	    d[2]= DI_SOUTH;
    else if (deltay>10*FRACUNIT)
	    d[2]= DI_NORTH;
    else
	    d[2]=DI_NODIR;

    // try direct route
    if (d[1] != DI_NODIR
	&& d[2] != DI_NODIR) ///- if both d1, d2 are not nodir (having a direction) then we're moving diag
    {
	    actor->movedir = diags[((deltay<0)<<1)+(deltax>0)];
	    if (actor->movedir != turnaround && P_TryWalk(actor)) ///- we attempt to walk, and if we have, then we're done here
	        return;
    }

    // try other directions
    if (P_Random() > 200 ///- approx 21.5%
	    ||  abs(deltay)>abs(deltax)) 
    {
        ///- tdir = temp dir, which means we're flipping the direction numbers, so if we're going northeast
        ///- north = 2
        ///- east = 0
        ///- d1 was defined as east/west, d2 was defined as north/south at the start
        ///- not sure why we're flipping these ~sometimes~
        tdir=d[1]; 
        d[1]=d[2];
        d[2]=(dirtype_t)tdir;
    }


    ///- unsure if these could literally ever be true
    if (d[1]==turnaround)
	    d[1]=DI_NODIR;
    if (d[2]==turnaround)
	    d[2]=DI_NODIR;
	
    if (d[1]!=DI_NODIR)
    {
	    actor->movedir = d[1];
	    if (P_TryWalk(actor))
	    {
	        // either moved forward or attacked
	        return;
	    }
    }

    if (d[2]!=DI_NODIR)
    {
	    actor->movedir =d[2];

        if (P_TryWalk(actor))
            return;
    }
    ///- if the new direction doesn't work, we try the old one, i guess...???

    // there is no direct path to the player,
    // so pick another direction.
    if (olddir!=DI_NODIR)
    {
	    actor->movedir =olddir;

        if (P_TryWalk(actor))
            return;
    }
    
    ///- generating random boolean
    // randomly determine direction of search
    if (P_Random()&1) 	
    {
        ///- clockwise loop
        for ( tdir=DI_EAST;
            tdir<=DI_SOUTHEAST;
            tdir++ )
        {
            if (tdir!=turnaround)
            {
                actor->movedir =tdir;
            
                if ( P_TryWalk(actor) )
                    return;
            }
        }
    }
    else
    {
        for ( tdir=DI_SOUTHEAST;
            tdir != (DI_EAST-1);
            tdir-- )
        {
            if (tdir!=turnaround)
            {
                actor->movedir =tdir;
            
                if ( P_TryWalk(actor) )
                    return;
            }
	    }
    }

    ///- if we cannot go literally any other direction, we FINALLY go backwards
    if (turnaround !=  DI_NODIR)
    {
	    actor->movedir =turnaround;
	    if ( P_TryWalk(actor) )
	        return;
    }

    actor->movedir = DI_NODIR;	// can not move
}

*/