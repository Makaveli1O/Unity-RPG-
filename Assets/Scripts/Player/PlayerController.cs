using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class PlayerController : MonoBehaviour, CombatInterface
{
    /*
        Variables, initializations etc.
        Character sprites have pivot anchored x: 0.5 and y: 0.1 (normalized)
        because of topdown angle
    */
    public AudioClip playerAttack;
    private float movementSpeed = 3f;
    private bool dash = false;

    [SerializeField] private LayerMask dashLayerMask; //layers for colision detection of dash
    private float treshold = 0.5f; //movement treshold (for handling mouse controls)
    public Vector3 moveDir;
    private Vector3 lookingDir;
    private Vector3 targetPos; //clicked position
    private Vector3 mousePos;
    private Rigidbody2D rigidbody2d;
    private ControllerUtilities controllerUtilities;
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
        

        rigidbody2d.MovePosition(dashPosition);
        //this.transform.position = dashPosition; //otherwise character will walk back to its last "pressed" position
        rigidbody2d.velocity = dashDir * dashAmount;
        dash = false;
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
}