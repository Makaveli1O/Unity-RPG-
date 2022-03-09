using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Mathematics;
using System.Collections.Generic;
using System;

public class EnemyController : MonoBehaviour, CombatInterface
{
    /*
        Components
    */
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

    /*  *   *   *   *   *   *   *
        C   O   M   B   A   T
    *   *   *   *   *   *   *  */
    private static List<EnemyController> enemyList;
    public int health{get;set;}
    public int armor{get;set;}
    public bool InCombat{get;set;}
    public bool IsDead{get;set;}

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
        //get anchor tile
        int2 coords = new int2((int)anchorPoint.x, (int)anchorPoint.y);
        anchorTile =  mapRef.GetTile(mapRef.TileRelativePos(coords), mapRef.TileChunkPos(coords));
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
        //player in aggro radius
        if (InAggroRadius != null)
        {
            InAggroRadius(this, EventArgs.Empty);   //notify player
            FollowPlayer(player.transform.position);
        //follow to last known position
        }else if(lastSeen != Vector3.zero && !IsDead){
            FollowPlayer(lastSeen, true);
        //normal behaviour (finish following to last seen position first)
        }else{
            if (!IsDead)
            {
                Wander();
            }
        }
    }

    private void FixedUpdate() {
        rb.velocity = moveDir * movementSpeed;
        if (rb.velocity.Equals(Vector3.zero)) IsMoving = false;
        else IsMoving = true;
    }

    /// <summary>
    /// Initializes this unit's healthbar 
    /// </summary>
    /// <param name="maxHealth"></param>
    private void InitHealthBar(int maxHealth){
        healthSystem = new HealthSystem(maxHealth);
        healthBarTransform = Instantiate(HealthBarPrefab, new Vector3(this.transform.position.x, this.transform.position.y + 2f), Quaternion.identity, this.gameObject.transform);
        healthBarTransform.localScale = new Vector3(11,7);

        healthBar = healthBarTransform.GetComponent<HealthBar>();
        healthBar.Setup(healthSystem);
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
                animationController.CharacterDirection(moveDir, true);
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
            animationController.CharacterMovement(moveDir, true);
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
        animationController.CharacterMovement(moveDir,true);
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
    /// Follow player on entering the collider
    /// </summary>
    public void FollowPlayer(Vector3 pos, bool notInRadius = false){
        this.lastSeen = pos; //last seen posotion of the player( to follow toward )
        if (!notInRadius)
        {
            SetTargetPosition(pos);
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
                            animationController.CharacterMovement(moveDir, true);
                }else{
                    currentPathIndex++;
                    //destination reached
                    if (currentPathIndex >= pathVectorList.Count)
                    {
                        StopMoving();
                        animationController.CharacterMovement(moveDir, true);
                        lastSeen = Vector3.zero;
                    }

                }
        
        }
        //pf.DrawPath(this.transform.position, pos);
    }

    private void StopMoving(){
        lastSeen = Vector3.zero;
        pathVectorList = null;
        this.moveDir = Vector3.zero;
    }

   public void SetTargetPosition(Vector3 targetPosition){
        currentPathIndex = 0;
        pathVectorList = pf.FindPathVector(this.transform.position ,targetPosition);
        
        if (pathVectorList != null && pathVectorList.Count > 1) {
            pathVectorList.RemoveAt(0);
        }
    }

    /*  *   *   *   *   *   *   *   *
        C   O   M   B   A   T
    *   *   *   *   *   *   *   *   */

    /// <summary>
    /// Set dead conditions
    /// </summary>
    public void Die(){
        this.IsDead = true;
        animationController.DeadAnimation(moveDir, true);
        this.moveDir = Vector3.zero;
        //disable collision and move to background
        col.enabled = !enabled;
        sr.sortingOrder = 0;
        return;
    }

    /// <summary>
    /// Set alive conditions
    /// </summary>
    public void Alive(){
        this.IsDead = false;
        if (!IsMoving) animationController.CharacterDirection(moveDir, true);
        else animationController.CharacterMovement(moveDir, true);
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
    public void Damage(Vector3 attackerPosition, int damageAmount){
        //knockback
        Vector3 dirFromAttacker = (transform.position - attackerPosition).normalized;
        float knockbackDistance = 0.5f;
        transform.position += dirFromAttacker * knockbackDistance;
        //spawn blood
        //healthsystem damage
        healthSystem.Damage(damageAmount);
        //if health is zero die
        if (healthSystem.GetHealthPercent() == 0) this.Die();
        return;
    }
}
