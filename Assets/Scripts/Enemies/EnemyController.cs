using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Mathematics;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour, CombatInterface
{
    /*
        Components
    */
    public Transform HealthBarPrefab;
    public EnemyPreset preset = null;
    public Vector3 anchorPoint;    //spawned position
    private TDTile anchorTile;
    private SpriteRenderer sr;
    private PathFinding pf;
    private Rigidbody2D rb;
    private CharacterAnimationController animationController;
    private Map mapRef;
    private UnityEngine.U2D.Animation.SpriteLibrary sla;
    private Animator animator;
    public UnityEngine.U2D.Animation.SpriteLibraryAsset[] spriteLibraries;  //sprite asset library list of possible asset libraries

    //wander
    public float observeTime = 2;
    public float wanderTime = 5; //time before changes direction
    public float movementSpeed = 3f;    //speed same as player
    private Vector3 moveDir;    //movement direction
    private int wanderRadius = 5;     //movement circle around spawned point
    private int OnEnableCount = 0; //skip first onEnable(that after awake)

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
    private Transform healthBarTransform;
    private HealthBar healthBar;
    public HealthSystem healthSystem;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        pf = GetComponent<PathFinding>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sla = GetComponent<UnityEngine.U2D.Animation.SpriteLibrary>();
        animationController = GetComponent<CharacterAnimationController>();
        rb.freezeRotation = true;
        var m = GameObject.FindGameObjectWithTag("Map");
        enemyList = new List<EnemyController>();
        try
        {
            mapRef = m.GetComponent<Map>();
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
        //when creating object pool this is performed, resulting in null
        if (preset != null)
        {
            sr.sprite = preset.sprite;
        }
        //start wandering right after enable
        if (OnEnableCount != 0)
        {
            observeTime = GetRandomObserveTime();
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
        if (!IsDead)
        {
            Wander();
        }
        
    }

    private void FixedUpdate() {
        rb.velocity = moveDir * movementSpeed;
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
                animationController.CharacterDirection(moveDir);
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
            animationController.CharacterMovement(moveDir);
        }else{
            if (moveDir.x > 0) //moving right, deflect to the left
            {
                moveDir.x = -moveDir.x;
               // moveDir.x = Random.Range(-1.0f, 0f);
            }else{//moving left, deflect to the right
                moveDir.x = -1*moveDir.x;
                //moveDir.x = Random.Range(0f, 1.0f);
            }
            if (moveDir.y > 0) //moving top, deflect to the bottom
            {
               moveDir.y = -moveDir.y;
               //moveDir.y = Random.Range(-1.0f, 0f);
            } else{//moving bottom, deflect to the top
               moveDir.y = -1*moveDir.y;
               //moveDir.y = Random.Range(0f, 1.0f);
            }
        }
        //animate movement
        animationController.CharacterMovement(moveDir);
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

    /*  *   *   *   *   *   *   *   *
        C   O   M   B   A   T
    *   *   *   *   *   *   *   *   */
    public void Die(){
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
        return;
    }
}
