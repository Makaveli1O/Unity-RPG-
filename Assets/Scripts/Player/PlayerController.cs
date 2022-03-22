using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerController : MonoBehaviour, CombatInterface
{
    /*
        Variables, initializations etc.
        Character sprites have pivot anchored x: 0.5 and y: 0.1 (normalized)
        because of topdown angle
    */
    public AudioClip playerAttack;
    private float movementSpeed = 3f;
    private bool dash = false;  //when dash is true, player is invincble
    private bool isInvincible;

    [SerializeField] private LayerMask dashLayerMask; //layers for colision detection of dash
    private float treshold = 0.5f; //movement treshold (for handling mouse controls)
    public Vector3 moveDir;
    private Vector3 lookingDir;
    private Vector3 targetPos; //clicked position
    private Vector3 mousePos;
    private Rigidbody2D rigidbody2d;
    private ControllerUtilities controllerUtilities;
    private ParticleSystem dashDust;
    private CharacterAnimationController characterAnimationController;
    private CharacterMovementController characterMovementController;
    public Transform HealthBarPrefab;
    private PathFinding pathFinding;
    //pathfinding
    private List<Vector3> pathVectorList = null;
    private int currentPathIndex;
    private bool findPath = false;
    /*  *   *   *   *   *   *   *
        H   E   A   L   T   H
    *   *   *   *   *   *   *  */
    private Transform healthBarTransform;
    private HealthBar healthBar;
    public HealthSystem healthSystem;

    /*  *   *   *   *   *   *   *
        C   O   M   B   A   T
    *   *   *   *   *   *   *  */
    public int health{get;set;}
    public int armor{get;set;}
    public bool InCombat{get;set;}
    public bool IsDead{get;set;}
    public enum State{
        Normal,
        Attacking
    }
    bool animating = false;
    public State state;    //current state

    private void Awake() {
        healthSystem = new HealthSystem(100);
        healthBarTransform = Instantiate(HealthBarPrefab, new Vector3(-12.7f,-8.9f), Quaternion.identity, GameObject.FindGameObjectWithTag("MainCamera").transform);
        healthBarTransform.localScale = new Vector3(80,40);
        healthBar = healthBarTransform.GetComponent<HealthBar>();
        healthBar.Setup(healthSystem);
        dashDust = GetComponent<ParticleSystem>();
    }
    private void Start()
    {
        //make healthbar on top of the player
        //healthBarTransform.position = new Vector3(this.transform.position.x + 0.25f, 0);
        this.characterAnimationController = GetComponent<CharacterAnimationController>();
        this.characterMovementController = GetComponent<CharacterMovementController>();
        this.controllerUtilities = GetComponent<ControllerUtilities>();
        this.pathFinding = GetComponent<PathFinding>();
        this.rigidbody2d = GetComponent<Rigidbody2D>();
        this.rigidbody2d.freezeRotation = true;
        gameObject.tag = "Player";
        state = State.Normal;
    }
    private void Update(){

        if (Input.GetMouseButtonDown(0)) state = State.Attacking;

        if (state != State.Attacking)
        {
            HandleMovement();
        }
        
        HandleAttack();
    }

    // Fixed Update -> work with rigidbody here
    private void FixedUpdate()
    {
        rigidbody2d.velocity = moveDir * movementSpeed;
        //dash
        if(dash){
            Dash();
        }
    }

    /*  *   *   *   *   *   *   *   *   *   *
        M   O   V   E   M   E   N   T
    *   *   *   *   *   *   *   *   *   *   */
    /// <summary>
    /// Follow Mouse on hold.
    /// Gets mouse position and converts it into camera pixel points.

    /// *note:
    /// Pivot point is anchored on y: 0.1 and x: 0.5. So for better mouse
    /// control, considering middle of the model is anchor would be more
    /// advisible.
    /// </summary>
     private void MoveMouse(){
        /* look direction*/
        mousePos = controllerUtilities.GetMouseWorldPosition();
        Vector3 playerPos = this.transform.position;
        playerPos.y += 0.4f; //pivot is normalized to y: 0.1 but for controls purpose consider as middle y: 0.5

        this.lookingDir = controllerUtilities.GetLookingDir(mousePos);
        /*movement direction on hold*/
        if (Input.GetMouseButton(0)){
            targetPos = mousePos;
            targetPos.z = 0; //for some reason z is always set to -9

            float x = mousePos.x - playerPos.x;
            float y = mousePos.y - playerPos.y;
            
            //walk towards held mouse
            if (Vector3.Distance(playerPos, targetPos) >=treshold){
                this.moveDir = new Vector3(x,y).normalized;
                //animation
                characterAnimationController.CharacterMovement(moveDir);
                treshold = 0.5f;
            }else{
                this.moveDir = Vector3.zero;
                //animation
                characterAnimationController.CharacterDirection(lookingDir);
                treshold = 1f;
            }
        }else{ 
            /* move to last known held/clicked position */
            if (Vector3.Distance(playerPos, targetPos) >=treshold && characterMovementController.IsMoving){
                this.moveDir = new Vector3(targetPos.x-playerPos.x, targetPos.y - playerPos.y, 0f).normalized;
                characterAnimationController.CharacterMovement(moveDir);
            }else{
                this.moveDir = Vector3.zero;
                //idle animation
                characterAnimationController.CharacterDirection(lookingDir);
            }
        } 
    }

    /// <summary>
    /// Handles keyboard movement and animations
    /// </summary>
    private void KeyboardMovement(){
        //when idle look at mouse
        mousePos = controllerUtilities.GetMouseWorldPosition();
        this.lookingDir = controllerUtilities.GetLookingDir(mousePos);

        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");
        this.moveDir = new Vector3(moveHorizontal, moveVertical, 0f ).normalized;
        if (this.moveDir.Equals(Vector3.zero)){  //idle animation
            characterAnimationController.CharacterDirection(lookingDir);  
        }else{    //movement animation
            characterAnimationController.CharacterMovement(moveDir);
            SoundManager.PlaySound(SoundManager.Sound.GrassStep, this.transform.position);
        }
    }

    /// <summary>
    /// Movement simulation and animation handling.
    /// </summary>
    private void HandleMovement(){
        //dash roll
        if(Input.GetKeyDown(KeyCode.Space)){
            dash = true;
        } 
        
        /* REGULAR MOVEMENT */
        if (!findPath){
            this.KeyboardMovement();
            //this.MoveMouse();
            this.characterMovementController.characterPathExpand();
            this.characterMovementController.characterMovementDetection();
        }else{
            this.FindPath();
            //FIXME remove this later huge performance issue
            //pathFinding.DrawPath(this.transform.position,mousePos);
        }

        /* PATHFINDING MOVEMENT (right click)*/
        if (Input.GetMouseButtonDown(1))
        {
            findPath = true;
            mousePos = controllerUtilities.GetMouseWorldPosition();
            SetTargetPosition(mousePos);
        }
    }

    /// <summary>
    /// Dash function. Teleports RB in the mouse direction when. This function
    /// workis with physics so must be used within FixedUpdate(). Cast Raycast before actual
    /// dashing, to prevent going through the walls. If wall is detected, move character to 
    /// collided raycast point instead.
    /// </summary>
    private void Dash(){
        StartCoroutine(BecomeTemporarilyInvincible());
        float dashAmount = 5f;
        Vector3 dashDir = Vector3.zero;
        //player is not moving, so use lookingDir vector instead of moveDir
        if(Vector3.Distance(moveDir, Vector3.zero) == 0){
            dashDir = lookingDir;
        }else{
            dashDir = moveDir;
        }
        // dashPosition is position where player should land
        Vector3 dashPosition = transform.position + dashDir * dashAmount;

        RaycastHit2D raycast = Physics2D.Raycast(transform.position, dashDir, dashAmount, dashLayerMask);
        if (raycast.collider != null){
            dashPosition = raycast.point;
        }
        //Spawn visual effect here
        DashDustEffect();

        rigidbody2d.MovePosition(dashPosition);
        SoundManager.PlaySound(SoundManager.Sound.Dash, transform.position);
        //this.transform.position = dashPosition; //otherwise character will walk back to its last "pressed" position
        rigidbody2d.velocity = dashDir * dashAmount;
        dash = false;
    }

    /// <summary>
    /// Handle dust particless effect when dashing in various directions.
    /// </summary>
    private void DashDustEffect(){
        var shape = dashDust.shape;
        shape = dashDust.shape;
        //left or right
        if (this.moveDir.Equals(Vector3.left) || this.moveDir.Equals(Vector3.right)){
            shape.scale = new Vector3(6f, 1.5f, 0f);
            shape.rotation = new Vector3(0f, 0f, 0f);
            dashDust.Play();
        //up or down
        }else if(this.moveDir.Equals(Vector3.up) || this.moveDir.Equals(Vector3.down)){
            shape.rotation = new Vector3(0f, 0f, 0f);
            shape.scale = new Vector3(1.5f, 6f, 0f);
            dashDust.Play();
        //diagonals
        }else{
            Vector3 rightUp = new Vector3(0.71f, 0.71f);
            Vector3 rightDown = new Vector3(0.71f, -0.71f);
            Vector3 leftUp = new Vector3(-0.71f, 0.71f);
            Vector3 leftDown = new Vector3(-0.71f, -0.71f);
            shape.scale = new Vector3(6f, 1.5f, 0f);
            if (Vector3.Angle(moveDir, rightUp) == 0f){
                shape.rotation = new Vector3(0f, 0f, 45f);
                dashDust.Play();
            }else if(Vector3.Angle(moveDir, rightDown) == 0f){
                shape.rotation = new Vector3(0f, 0f, 315f);
                dashDust.Play();
            }else if(Vector3.Angle(moveDir, leftUp) == 0f){
                shape.rotation = new Vector3(0f, 0f, 315f);
                dashDust.Play();
            }else if(Vector3.Angle(moveDir, leftDown) == 0f){
                shape.rotation = new Vector3(0f, 0f, 45f);
                dashDust.Play();
            }
        }
    }

    /// <summary>
    /// Invincibility coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator BecomeTemporarilyInvincible(){
        float invincibilityDuration = 0.15f;
        //Debug.Log("Player turned invincible!");
        isInvincible = true;

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
        //Debug.Log("Player is no longer invincible!");
    }

    /*  *   *   *   *   *   *   *   *   *   *
            P A T H  F I N D I N G
    *   *   *   *   *   *   *   *   *   *   */
    /// <summary>
    /// Uses Pathfinding controller to find shortest path to clicked position.
    /// </summary>
    private void FindPath(){
        if (findPath == false)
        {
            return;
        }
        mousePos = controllerUtilities.GetMouseWorldPosition();
        Vector3 playerPos = this.transform.position;
        playerPos.y += 0.4f; //pivot is normalized to y: 0.1 but for controls purpose consider as middle y: 0.5

        this.lookingDir = controllerUtilities.GetLookingDir(mousePos);

        if (pathVectorList != null)
        {
            //exception handling
            Vector3 targetPos = pathVectorList[currentPathIndex];
            
            if (Vector3.Distance(transform.position, targetPos) >= treshold)
            {
                this.moveDir = (targetPos - transform.position).normalized;
                float distanceBefore = Vector3.Distance(transform.position, targetPos);
                characterAnimationController.CharacterMovement(moveDir);
            }else{
                currentPathIndex++;
                if (currentPathIndex >= pathVectorList.Count)
                {
                    StopMoving();
                    characterAnimationController.CharacterDirection(lookingDir);
                    findPath = false;
                }

            }
        }
    }

    /// <summary>
    /// Used to stop movement when using pathfinding algorithm.
    /// </summary>
    private void StopMoving(){
        pathVectorList = null;
        this.moveDir = Vector3.zero;
    }

    /// <summary>
    /// Set pathfinding target position to reach.
    /// </summary>
    /// <param name="targetPosition"></param>
    public void SetTargetPosition(Vector3 targetPosition){
        currentPathIndex = 0;
        pathVectorList = pathFinding.FindPathVector(this.transform.position ,targetPosition);

        if (pathVectorList != null && pathVectorList.Count > 1) {
            pathVectorList.RemoveAt(0);
        }
    }

    /// <summary>
    /// Attack simulation and animation handling. Sets state action to Attacking
    /// </summary>    
    private void HandleAttack(){
        //only one attack at time
        if(state == State.Attacking && animating == false){
            float attackOffset = 1.5f;
            this.mousePos = controllerUtilities.GetMouseWorldPosition();
            this.lookingDir = controllerUtilities.GetLookingDir(mousePos);
            Vector3 attackPosition = transform.position + lookingDir * attackOffset;

            Vector3 attackDir = lookingDir;

            float attackRange = 1.5f;
            EnemyController targetEnemy = EnemyController.GetClosestEnemy(attackPosition, attackRange);
            if (targetEnemy != null)
            {
                targetEnemy.Damage(transform.position, 20);
                attackDir = (targetEnemy.GetPosition() - transform.position).normalized;
            }
            //state = State.Attacking;
            //FIXME stop on attack or not? depends on testing
            moveDir = Vector3.zero;
            animating = true;   //performing animation
            SoundManager.PlaySound(SoundManager.Sound.Attack, transform.position);
            characterAnimationController.CharacterAttack(attackDir); //reset state after complete
            float dashDistance = 1f;
            transform.position += attackDir * dashDistance;
        }
    }

    /// <summary>
    /// Event handler(animator)
    /// </summary>
    private void EndAttackAnimation(){
        state = State.Normal;
        animating = false;
    }

    //FIXME do these in cahracters class or interface, and apply to both enemy and palyer
    public bool Damage(Vector3 attackerPosition, int damageAmount){
        if (isInvincible) return false;

        //knockback
        Vector3 dirFromAttacker = (transform.position - attackerPosition).normalized;
        float knockbackDistance = 0.5f;
        transform.position += dirFromAttacker * knockbackDistance;
        //spawn blood
        //healthsystem damage
        healthSystem.Damage(damageAmount);
        //sfx
        //SoundManager.PlaySound(SoundManager.Sound.Hit, transform.position, GetPresetAudioClip(SoundManager.Sound.Hit));
        //damage popup
        DamagePopup.Create(transform.position, damageAmount);
        //if health is zero die
        if (healthSystem.GetHealthPercent() == 0){
            if (!this.IsDead) {//this.Die();
            }
            
            return true;
        }else{
            //sthis.Hurt();
            return false;
        } 
    }
}