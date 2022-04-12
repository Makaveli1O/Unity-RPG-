using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Collections;
using System;

public class EnemyController : MonoBehaviour, CombatInterface
{
    /*
        Components
    */
    public bool twoDirEntity;   //defines whenever sprites are only left-right or left-top-right-bot
    public RuntimeAnimatorController[] aControllers;    //0 -> 2dir, 1 -> 4dir
    private GameObject player;
    public EnemyPreset preset = null;
    public Vector3 anchorPoint;    //spawned position
    private TDTile anchorTile;
    private SpriteRenderer sr;
    private PathFinding pf;
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private CharacterAnimationController animationController;
    private Map mapRef;
    private UnityEngine.U2D.Animation.SpriteLibrary sla;
    private Animator animator;
    public UnityEngine.U2D.Animation.SpriteLibraryAsset assetsLibrary;  //sprite asset library list of possible asset libraries

    public enum State{
        Normal,
        Attacking,
        Hurting,
        Dieing
    }

    public State state;
    private bool animating;
    private float timer;
    /*  *   *   *   *   *   *   *   *   *
        M   O   V   E   M   E   N   T
    *   *   *   *   *   *   *  *    *   */
    private Vector3 lastSeen; // last seen position of player to follow-up to
    private List<Vector3> pathVectorList = null;
    private int currentPathIndex;
    public event EventHandler InAggroRadius;
    public float observeTime = 2;
    public float wanderTime = 5; //time before changes direction
    public float movementSpeed = 3f;    //speed same as player
    private Vector3 moveDir;    //movement direction
    private int wanderRadius = 5;     //movement circle around spawned point
    private int OnEnableCount = 0; //skip first onEnable(that after awake)
    public bool IsMoving{get;set;}
    private float movementThresHold = 2f;

    /*  *   *   *   *   *   *   *
        C   O   M   B   A   T
    *   *   *   *   *   *   *  */
    private static List<EnemyController> enemyList; //targeting system
    public int health{get;set;}
    public int armor{get;set;}
    public bool InCombat{get;set;}
    public bool IsDead{get;set;}
    public float attackRange{get;set;}
    public float attackTime{get;set;}
    public int bottomDamage;
    public int topDamage;
    private bool noRush = true;

    /*  *   *   *   *   *   *   *
        H   E   A   L   T   H
    *   *   *   *   *   *   *  */
    public Transform HealthBarPrefab;
    private Transform healthBarTransform;
    private HealthBar healthBar;
    public HealthSystem healthSystem;
    
    /*  *   *   *   *   *   *   *   *   *   *
        F   U   N   C   T   I   O   N   S
    *   *   *   *   *   *   *  *    *   *   */
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        pf = GetComponent<PathFinding>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        sla = GetComponent<UnityEngine.U2D.Animation.SpriteLibrary>();
        animationController = GetComponent<CharacterAnimationController>();
        rb.freezeRotation = true;
        var m = GameObject.FindGameObjectWithTag("Map");
        enemyList = new List<EnemyController>();
        try
        {
            mapRef = m.GetComponent<Map>();
            player = mapRef.player; //get player reference
        }
        catch
        {
            Debug.Log("Map reference not found in Enemy controller.");
        }
    }

    /// <summary>
    /// Performs on activation
    /// </summary>
    private void OnEnable() {
        //start wandering right after enable
        if (OnEnableCount != 0)
        {
            //set correct assetsLibrary (animations and sprites)
            sla.spriteLibraryAsset = assetsLibrary;
            //set correct animation controller
            this.twoDirEntity = this.preset.twoDirSprite;
            animator.runtimeAnimatorController = (this.twoDirEntity == true) ? aControllers[0] : aControllers[1];
            //set random observe time
            observeTime = GetRandomObserveTime();
            //set to alive
            Alive();
            //add to enemyList for targeting
            enemyList.Add(this);
        }else{
            //initialize healthbar on first enable
            InitHealthBar(100);
        }
        OnEnableCount++;
    }

    private void OnDisable() {
        enemyList.Remove(this);
    }

    private void Start() {
        float noRushTime = 5f;
        //get anchor tile
        int2 coords = new int2((int)anchorPoint.x, (int)anchorPoint.y);
        anchorTile =  mapRef.GetTile(mapRef.TileRelativePos(coords), mapRef.TileChunkPos(coords));
        //events turned off
        animator.fireEvents = false;
        StartCoroutine(NoRushAggro(noRushTime));
    }

    /// <summary>
    /// Coroutine handling, instant aggro of enemies, while spawned in aggroradius.
    /// </summary>
    /// <param name="time">Time to wait until attack after spawn.</param>
    /// <returns></returns>
    public IEnumerator NoRushAggro(float time){
        yield return new WaitForSeconds(time);
        noRush = false;
    }

    /// <summary>
    /// On collision with either object or entity, change direction
    /// </summary>
    /// <param name="other">Collidet object collision</param>
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Entity" || other.gameObject.tag == "Object")
        {
            ChangeDirection(true);
        }
    }

    private void Update() {
        if (this.IsDead) return;
        //if nor attacking or hurting, movement handling is in place
        if (state == State.Attacking)
        {
            if (animating == false) HandleAttack();             
        }else if (state == State.Hurting){
            if (animating == false) Hurt(); 
        }else{
            HandleMovement();
        }
    }

    private void FixedUpdate() {
        rb.velocity = moveDir * movementSpeed;
        if (rb.velocity.Equals(Vector3.zero)) IsMoving = false;
        else IsMoving = true;
    }

    /// <summary>
    /// Handling entity's movement
    /// </summary>
    void HandleMovement(){
        //player in aggro radius
        if (InAggroRadius != null && !noRush)
        {
            if (Vector3.Distance(player.transform.position, this.transform.position) >= movementThresHold)
            {
                InAggroRadius(this, EventArgs.Empty);   //notify player
                FollowPlayer(player.transform.position);
            }else{  //threshold activated, standing next to player
                animationController.CharacterDirection(player.transform.position - this.transform.position);
                moveDir = Vector3.zero; //stop
                HandleAttack();
            }

        //follow to last known position
        }else if(lastSeen != Vector3.zero && !IsDead){
            FollowPlayer(lastSeen, true);
        //normal behaviour (finish following to last seen position first)
        }else{
            if (IsMoving)
            {
                animationController.CharacterMovement(player.transform.position - this.transform.position);
            }else{
                animationController.CharacterDirection(player.transform.position - this.transform.position);
            }
            
            if (!IsDead)
            {
                Wander();
            }
        }
    }


    /// <summary>
    /// Initializes this unit's healthbar 
    /// </summary>
    /// <param name="maxHealth"></param>
    private void InitHealthBar(int maxHealth){
        healthSystem = new HealthSystem(maxHealth);
        healthBarTransform = Instantiate(HealthBarPrefab, new Vector3(this.transform.position.x, this.transform.position.y + 2f), Quaternion.identity, this.gameObject.transform);
        healthBarTransform.localScale = new Vector3(Const.WORLD_HEALTHBAR_WIDTH, Const.WORLD_HEALTHBAR_HEIGHT);

        healthBar = healthBarTransform.GetComponent<HealthBar>();
        healthBar.Setup(healthSystem, Color.red);
    }

    /// <summary>
    /// Enemy is wandering around it's spawn point. Waits for a short period of time(observe time) and then
    /// wanders again.
    /// </summary>
    private void Wander(){
        if (wanderTime > 0)
        {
            //change direction from non walkable tile
            TileController();
            wanderTime -= Time.deltaTime;
        }else{
            //stop and observer after wander
            if (observeTime > 0)
            {
                //animate idle
                animationController.CharacterDirection(moveDir, twoDirEntity);
                moveDir = Vector3.zero; //stop
                observeTime -= Time.deltaTime;
            //then find new direction towards anchor point and move
            }else{
                observeTime = GetRandomObserveTime();
                wanderTime = Random.Range(1.0f, 6.0f);
                ChangeDirection();
            }
        }
    }

    /// <summary>
    /// Change direction for wandering. If opposite is true, meaning enemy hit non walkable tile,
    /// movedir is changed to reflection vector, not to go to that direction again.
    /// </summary>
    /// <param name="opposite">Is reflection required</param>
    private void ChangeDirection(bool opposite = false){
        if (!opposite)
        {
            Vector3 targetPos = RandomPointInRadius();
            moveDir = new Vector3(targetPos.x-this.gameObject.transform.position.x, targetPos.y - this.gameObject.transform.position.y, 0f).normalized;
            animationController.CharacterMovement(moveDir, twoDirEntity);
        }else{
            if (moveDir.x > 0) //moving right, deflect to the left
            {
                moveDir.x = -moveDir.x;
            }else{//moving left, deflect to the right
                moveDir.x = -1*moveDir.x;
            }
            if (moveDir.y > 0) //moving top, deflect to the bottom
            {
               moveDir.y = -moveDir.y;
            } else{//moving bottom, deflect to the top
               moveDir.y = -1*moveDir.y;
            }
        }
        //animate movement
        animationController.CharacterMovement(moveDir,twoDirEntity);
    }

    /// <summary>
    /// Picks random point in given guard radius.
    /// </summary>
    /// <returns>Random point in radius</returns>
    private Vector3 RandomPointInRadius(){
        return new Vector3(anchorPoint.x + Random.Range(-wanderRadius,wanderRadius), anchorPoint.y + Random.Range(-wanderRadius,wanderRadius));
    }

    /// <summary>
    /// Random time between declared floats (0.0, 0.5)
    /// </summary>
    /// <returns>Random time</returns>
    private float GetRandomObserveTime(){
        return Random.Range(0.0f, 5.0f);
    }

    /// <summary>
    /// Checks, if picked random point is on walkable tile or not.
    /// </summary>
    private void TileController(){
        int2 coords = new int2((int)this.gameObject.transform.position.x,(int)this.gameObject.transform.position.y);
        TDTile tile = mapRef.GetTile(mapRef.TileRelativePos(coords), mapRef.TileChunkPos(coords));
        if (!tile.IsWalkable || tile == null)
        {
            ChangeDirection(true);
        }   
        return;
    }

    /// <summary>
    /// Get position of this enemy.
    /// </summary>
    /// <returns>Position of enemy</returns>
    public Vector3 GetPosition(){
        return this.transform.position;
    }

    /// <summary>
    /// Find closest enemy of spawned enemies BFS
    /// </summary>
    /// <param name="position">Given position</param>
    /// <param name="range">Range fro mplayer</param>
    /// <returns>Closest enemy</returns>
    public static EnemyController GetClosestEnemy(Vector3 position, float range){
        //no enemies yet
        if (enemyList == null) return null;

        EnemyController closestEnemy = null;

        for (int i = 0; i < enemyList.Count; i++)
        {
            EnemyController testEnemy = enemyList[i];
            //too far or dead skip
            if (Vector3.Distance(position, testEnemy.GetPosition()) > range || testEnemy.IsDead) continue;
            //no closest enemy assigned yet
            if (closestEnemy == null)
            {
                closestEnemy = testEnemy;
            }else{
                //already has closest enemy decide closer one
                if (Vector3.Distance(position, testEnemy.GetPosition()) < Vector3.Distance(position, closestEnemy.GetPosition()))
                {
                    closestEnemy = testEnemy;
                }
            }
        }
        return closestEnemy;
    }

    /// <summary>
    /// Get closest enemy that is not in the skiplist
    /// </summary>
    /// <param name="position">Position of the plaayer</param>
    /// <param name="range">Radius range</param>
    /// <param name="skipList">List of gameobject instances id's to skip</param>
    /// <returns></returns>
   public static EnemyController GetClosestEnemyNotInList(Vector3 position, float range, List<Int32> skipList){
        //no enemies yet
        if (enemyList == null) return null;

        EnemyController closestEnemy = null;

        for (int i = 0; i < enemyList.Count; i++)
        {
            EnemyController testEnemy = enemyList[i];
            //skip id in list
            if (skipList.Contains(testEnemy.gameObject.GetInstanceID()))
            {
                continue;
            }
            //too far skip
            if (Vector3.Distance(position, testEnemy.GetPosition()) > range) continue;
            //no closest enemy assigned yet
            if (closestEnemy == null)
            {
                closestEnemy = testEnemy;
            }else{
                //already has closest enemy decide closer one
                if (Vector3.Distance(position, testEnemy.GetPosition()) < Vector3.Distance(position, closestEnemy.GetPosition()))
                {
                    closestEnemy = testEnemy;
                }
            }
        }
        return closestEnemy;
    }

    /// <summary>
    /// Finds all enemies within given radius from given's position
    /// </summary>
    /// <param name="position">Target position</param>
    /// <param name="radius">radiusaroun position</param>
    /// <returns>Found list of enemies within given position in given radius.</returns>
    public static List<EnemyController> GetEnemiesWithinRadius(Vector3 position, float radius){
        List<EnemyController> enemiesWithinRadius = new List<EnemyController>();
        List<Int32> enemyInstances = new List<Int32>();
        EnemyController enemyFound = GetClosestEnemy(position, radius);
        while (enemyFound != null && !enemyInstances.Contains(enemyFound.gameObject.GetInstanceID()))
        {
            enemyInstances.Add(enemyFound.gameObject.GetInstanceID());
            enemiesWithinRadius.Add(enemyFound);
            enemyFound = GetClosestEnemyNotInList(position, radius, enemyInstances);
        }
        
        if (enemiesWithinRadius.Count == 0 || enemiesWithinRadius == null)
        {
            return null;
        }else{
            return enemiesWithinRadius;
        }
        
    }

    /// <summary>
    /// Follow player on entering the collider
    /// </summary>
    public void FollowPlayer(Vector3 pos, bool notInRadius = false){
        this.lastSeen = pos; //last seen posotion of the player( to follow toward )
        if (!notInRadius)
        {
            //path not found
            if(!SetTargetPosition(pos)){
                Wander();
                return;
            }
        }
        if (pathVectorList != null && pathVectorList.Count > 0)
            {
                //exception handling
                Vector3 targetPos = pathVectorList[currentPathIndex];
                
                if (Vector3.Distance(transform.position, targetPos) >= 0.5f)
                {
                    this.moveDir = (targetPos - transform.position).normalized;
                    this.wanderTime = 0f;
                    float distanceBefore = Vector3.Distance(transform.position, targetPos);
                            animationController.CharacterMovement(moveDir, twoDirEntity);
                }else{
                    currentPathIndex++;
                    //destination reached
                    if (currentPathIndex >= pathVectorList.Count)
                    {
                        StopMoving();
                        animationController.CharacterMovement(moveDir, twoDirEntity);
                        lastSeen = Vector3.zero;
                    }

                }
        
        }
    }

    private void StopMoving(){
        lastSeen = Vector3.zero;
        pathVectorList = null;
        this.moveDir = Vector3.zero;
    }

    /// <summary>
    /// Set target position for pathfinding, to follow up towards.
    /// </summary>
    /// <param name="targetPosition">Actual position of target</param>
    /// <returns>Bool true or false if path is found</returns>
   public bool SetTargetPosition(Vector3 targetPosition){
        currentPathIndex = 0;
        pathVectorList = pf.FindPathVector(this.transform.position ,targetPosition);

        //path not found (path going through unloaded chunk etc.)
        if (pathVectorList == null)
        {
            return false;
        }
        
        if (pathVectorList != null && pathVectorList.Count > 1) {
            pathVectorList.RemoveAt(0);
        }

        return true;
    }

    /*  *   *   *   *   *   *   *   *
        C   O   M   B   A   T
    *   *   *   *   *   *   *   *   */

    /// <summary>
    /// Set this entity's stats mathincg correct preset stats.
    /// </summary>
    private void SetPresetStats(){
        this.health = preset.health;
        this.armor = preset.armor;
        this.attackRange = preset.attackRange;
        this.attackTime = 0f;
        this.bottomDamage = preset.bottomDamage;
        this.topDamage = preset.topDamage;
        this.movementSpeed = preset.movementSpeed;
        return;
    }

    /// <summary>
    /// Set dead conditions
    /// </summary>
    public void Die(){
        this.IsDead = true;
        SoundManager.PlaySound(SoundManager.Sound.Death, transform.position, GetPresetAudioClip(SoundManager.Sound.Death));
        this.moveDir = Vector3.zero;
        StartCoroutine(Animating(0.3f, State.Dieing));
        //disable collision and move to background
        col.enabled = !enabled;
        sr.sortingOrder = 0;
        return;
    }

    /// <summary>
    /// Handles hurt state of entity.
    /// </summary>
    public void Hurt(){
        //trigger animation
        this.animating = true;
        //trigger start animation events here
        this.state = State.Hurting;
        SoundManager.PlaySound(SoundManager.Sound.Hit, transform.position, GetPresetAudioClip(SoundManager.Sound.Hurt));
        StartCoroutine(Animating(0.3f, State.Hurting));
        return;
    } 

    /// <summary>
    /// Handles entity's attack coordination.
    /// </summary>
    void HandleAttack(){
        //attack rate counter, not yet ready to attack
        if (attackTime > 0)
        {
            attackTime -= Time.deltaTime;  
            return;
        }else{
            //ready to attack, restart attackRate counter
            attackTime = preset.attackTime;
        }
        //trigger animation
        this.animating = true;
        
        //trigger start animation events here
        string damageAmount = Random.Range(bottomDamage, topDamage).ToString();
        this.state = State.Attacking;
        StartCoroutine(Animating(this.preset.attackAnimationDuration, State.Attacking, damageAmount));
        return;
    }

    /// <summary>
    /// Coroutine handling combat actions.
    /// </summary>
    /// <returns>enumerator</returns>
    private IEnumerator Animating(float time, State action, string dmg = "0"){
        // trigger the stop animation events here
        switch (action)
        {
            case State.Attacking:
                animationController.CharacterAttack(player.transform.position - this.transform.position);
                //sfxeffect
                StartCoroutine(AttackSfxDelay());
                break;
            case State.Hurting:
                animationController.HurtAnimation(moveDir, twoDirEntity);
                break;
            case State.Dieing:
                animationController.DeadAnimation(moveDir, twoDirEntity);
                break;
        }
        yield return new WaitForSeconds(time);
        //attack player
        if (action.Equals(State.Attacking)) player.GetComponent<PlayerController>().Damage(transform.position, dmg, this.preset.attackRange);
        //call end animation event
        EndAnimation();
    }

    /// <summary>
    /// Delays sfx effect according to presed invoked by.
    /// </summary>
    /// <returns></returns>
    public IEnumerator AttackSfxDelay(){
        yield return new WaitForSeconds(preset.attackSfxDelay);
        SoundManager.PlaySound(SoundManager.Sound.Hit, transform.position, GetPresetAudioClip(SoundManager.Sound.Attack));
    }

    /// <summary>
    /// Set alive conditions
    /// </summary>
    public void Alive(){
        //set preset stats (health, damage etc.)
        SetPresetStats();
        this.IsDead = false;
        if (!IsMoving) animationController.CharacterDirection(moveDir, twoDirEntity);
        else animationController.CharacterMovement(moveDir, twoDirEntity);
        //disable collision and move to background
        col.enabled = enabled;
        sr.sortingOrder = 1;
        healthSystem.HealMax();
        return;
    }

    /// <summary>
    /// Damages this entity
    /// </summary>
    /// <param name="attackerPosition">Position of attacker</param>
    /// <retrn>True if entity died, false otherwise.</return>
    public bool Damage(Vector3 attackerPosition, string damageAmount, float attackRange = 0f){
        //knockback
        Vector3 dirFromAttacker = (transform.position - attackerPosition).normalized;
        float knockbackDistance = 0.5f;
        transform.position += dirFromAttacker * knockbackDistance;
        //spawn blood
        //healthsystem damage
        healthSystem.Damage(int.Parse(damageAmount));
        //sfx
        SoundManager.PlaySound(SoundManager.Sound.Hit, transform.position, GetPresetAudioClip(SoundManager.Sound.Hit));
        
        //damage popup
        DamagePopup.Create(transform.position, damageAmount);
        //if health is zero die
        if (healthSystem.GetHealthPercent() == 0){
            if (!this.IsDead) this.Die();
            
            return true;
        }else{
            this.Hurt();
            return false;
        } 
    }

    /// <summary>
    /// Finds proper given "sound" from within enemy presets.
    /// </summary>
    /// <param name="sound">Desired sound effect</param>
    /// <returns>Audio clip represented by Sound</returns>
    public AudioClip GetPresetAudioClip(SoundManager.Sound sound){
        foreach(GameAssets.SoundAudioClip soundAudioClip in preset.sfx){
            if (soundAudioClip.sound == sound)
            {
                return soundAudioClip.audioClip;
            }
        }

        return null;
    }

    /*
        Animation events functions.
    */
    public void EndAnimation(){
        this.state = State.Normal;
        animating = false;
    }
}
