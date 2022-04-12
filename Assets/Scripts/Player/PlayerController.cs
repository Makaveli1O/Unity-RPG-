using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using System.Collections.Generic;
public class PlayerController : MonoBehaviour, CombatInterface
{
    /*
        Variables, initializations etc.
        Character sprites have pivot anchored x: 0.5 and y: 0.1 (normalized)
        because of topdown angle
    */    
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
    private UIHandler uiHandler;
    public Shield shield;
    private Map mapRef;
    public Transform HealthBarPrefab;
    private PathFinding pathFinding;
    //pathfinding
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
    public float attackRange{get;set;}
    public float attackTime{get;set;}
    public enum State{
        Normal,
        Attacking,
        Interacting, // casting is the same state
        Blocking
    }
    private AoeSpell aoeSpell;
    private AoeAttack aoeAttack;
    public bool animating = false;
    private bool aoeReady = false;    //ready state before spell (showing radius and cursor type changed)
    public float aoeSpellDuration = 1f;
    public int aoeSpellDamage = 5;
    public float aoeAttackRadius;
    public int aoeAttackDamage = 7;
    public int healSpellAmount = 20;
    private State state;    //current state
    private bool interruptCoroutine = false;
    private void Awake() {
        uiHandler = GetComponent<UIHandler>();
        Alive();
        shield = GetComponentInChildren<Shield>();
        aoeSpell = GetComponentInChildren<AoeSpell>();
        aoeAttack = GetComponentInChildren<AoeAttack>();
        dashDust = GetComponent<ParticleSystem>();
        var m = GameObject.FindGameObjectWithTag("Map");
        try
        {
            mapRef = m.GetComponent<Map>();
        }
        catch
        {
            Debug.Log("Map reference not found in Enemy player controller.");
        }
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
        //interacting
        if (state == State.Interacting){
            if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f)
            {
                SoundManager.PlaySound(SoundManager.Sound.Error, this.transform.position);
                state = State.Normal;
                interruptCoroutine = true;
            }else{
                return;
            }   
        } 

        //Attack1
        if (Input.GetMouseButtonDown(0) && state == State.Normal){
            state = State.Attacking;
            HandleAttack();
        //Shield
        }else if (Input.GetMouseButton(1)){ 
            state = State.Blocking;
            shield.ActivateShield();
            HandleShielding();
            //ui effects
            uiHandler.ShieldCooldown();
            //SoundManager.PlaySound(SoundManager.Sound.ShieldToggle);
        }else if(Input.GetMouseButtonUp(1)){
            state = State.Normal;
            shield.DeactivateShield();
            //ui effects
            uiHandler.ShieldCooldownDeactivate();
            //SoundManager.PlaySound(SoundManager.Sound.ShieldToggle);
        //aoe spell
        }else if(Input.GetKeyDown(KeyCode.F)){
            if (aoeReady){
                aoeReady = false;
                aoeSpell.HideGuidelines();
            }else if(uiHandler.IsAoeReady){
                aoeReady = true;
                aoeSpell.ShowGuidelines();
            }else{
                SoundManager.PlaySound(SoundManager.Sound.Error, this.transform.position);
                state = State.Normal;
            }
        //aoe attack
        }else if(Input.GetKeyDown(KeyCode.E)){
            if (uiHandler.IsSlashReady)
            {
                AoeAttack();
            }else{
                SoundManager.PlaySound(SoundManager.Sound.Error, this.transform.position);
                state = State.Normal; 
            }   
        //healing spell
        }else if (Input.GetKeyDown(KeyCode.Q)){
            if (uiHandler.IsHealReady)
            {
                HealSpell();
            }else{
                SoundManager.PlaySound(SoundManager.Sound.Error, this.transform.position);
                state = State.Normal; 
            }  
        }

        //Movement
        if (state == State.Normal)
        {
            HandleMovement();
        }   
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
            if (!uiHandler.IsDashReady)
            {
                SoundManager.PlaySound(SoundManager.Sound.Error, this.transform.position);
            }else{
                dash = true;
            }
        } 
        
        this.KeyboardMovement();
    }

    //TODO
    public void Die(){
        return;
    }
    /// <summary>
    /// Set initial healthbar stats.
    /// </summary>
    public void Alive(){
        healthSystem = new HealthSystem(100);
        healthBarTransform = Instantiate(HealthBarPrefab, new Vector3(-Const.CAMERA_BAR_X, Const.CAMERA_BAR_Y), Quaternion.identity, GameObject.FindGameObjectWithTag("MainCamera").transform);
        healthBarTransform.localScale = new Vector3(80,40);
        healthBar = healthBarTransform.GetComponent<HealthBar>();
        healthBar.Setup(healthSystem, Color.red);
        return;
    }

    public void LoadedHealth(int amount){
        healthSystem.SetHealth(amount);
    }

    public void LoadedShieldHealth(int amount){
        shield.healthSystem.SetHealth(amount);
    }

    public void Hurt(){
        return;
    }

    /// <summary>
    /// Function handling player's getting damage. If player is invincible no damage is recieved,
    /// as well as if palyer is too far away. Spawns popup, sound effect and does damage to health
    /// system.
    /// </summary>
    /// <param name="attackerPosition">Position of attacker</param>
    /// <param name="damageAmount">Damage amount to player</param>
    /// <param name="attackRange">Range of attacking entity</param>
    /// <returns>True if player is hit, false otherwise.</returns>
    public bool Damage(Vector3 attackerPosition, string damageAmount, float attackRange){
        //if in dash -> invincible
        if (isInvincible) return false;

        //miss detection ( beyond entity's reach )
        if (Vector3.Distance(attackerPosition, this.transform.position) > attackRange)
        {
            DamagePopup.Create(attackerPosition, "MISS", DamagePopup.Type.Miss);
            SoundManager.PlaySound(SoundManager.Sound.Miss, attackerPosition);
            return false;
        }else if(state == State.Blocking){
            int damage = (shield.healthSystem.GetHealth() - int.Parse(damageAmount));
            shield.healthSystem.Damage(int.Parse(damageAmount));
            //damaged below shield's health
            if (damage < 0)
            {
                damageAmount = damage.ToString();
                //sfx to break shield
            //full damage consumed by shield
            }else{
                DamagePopup.Create(transform.position, "ABSORB", DamagePopup.Type.Shield);
                return false;
            }
            SoundManager.PlaySound(SoundManager.Sound.ShieldHit);
        }else{
            //sfx
            SoundManager.PlaySound(SoundManager.Sound.Hit, transform.position);
        }
        //healthsystem damage
        healthSystem.Damage(int.Parse(damageAmount));
        //damage popup
        DamagePopup.Create(transform.position, damageAmount);
        //knockback
        Vector3 dirFromAttacker = (transform.position - attackerPosition).normalized;
        float knockbackDistance = 0.5f;
        transform.position += dirFromAttacker * knockbackDistance;
        //if health is zero die
        if (healthSystem.GetHealthPercent() == 0){
            //TODO game over screen .. 
            if (!this.IsDead) {//this.Die();
            }
            return true;
        }else{
            return false;
        } 
    }

    /*  *   *   *   *   *   *   *
        C   O   M   B   A   T
    *   *   *   *   *   *   *  */

    public void HandleAttack(){
        if (!aoeReady){
            SimpleAttack();
        }else{
            AoeSpell();
        }
    }
    /// <summary>
    /// Attack simulation and animation handling. Sets state action to Attacking
    /// </summary>    
    private void SimpleAttack(){
        float attackLength = 0.35f;
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
                targetEnemy.Damage(transform.position, "20");
                attackDir = (targetEnemy.GetPosition() - transform.position).normalized;
            }
            state = State.Attacking;
            StartCoroutine(Attacking(attackLength));
            moveDir = Vector3.zero;
            animating = true;   //performing animation
            SoundManager.PlaySound(SoundManager.Sound.Attack, transform.position);
            characterAnimationController.CharacterAttack(attackDir); //reset state after complete
            float dashDistance = 1f;
            transform.position += attackDir * dashDistance;
        }
    }

    /// <summary>
    /// Perfors aoe attack from AoeSpell.cs
    /// </summary>
    private void AoeSpell(){
        uiHandler.AoeCooldown();    //UI visuals
        float attackLength = 0.75f;
        aoeSpell.HideGuidelines();
        aoeSpell.Perform(aoeSpellDuration, aoeSpellDamage);
        SoundManager.PlaySound(SoundManager.Sound.WindSpell, this.transform.position);
        StartCoroutine(Attacking(attackLength));
        characterAnimationController.CharacterAttack3(lookingDir);
        moveDir = Vector3.zero;
        aoeReady = false;
        animating = true;
    }

    private void AoeAttack(){
        uiHandler.SlashCooldown();
        state = State.Attacking;
        float aoeAttackLength = 0.75f;
        moveDir = Vector3.zero;
        animating = true;
        List<EnemyController> enemiesWithinRadius = EnemyController.GetEnemiesWithinRadius(transform.position, aoeAttackRadius);
        if (enemiesWithinRadius != null)
        {
            aoeAttack.Perform(enemiesWithinRadius, aoeAttackDamage);
        }
        StartCoroutine(Attacking(aoeAttackLength));
        characterAnimationController.CharacterAttack2(lookingDir);
        SoundManager.PlaySound(SoundManager.Sound.SmashGround, this.transform.position);

    }

    private IEnumerator Attacking(float time){
        yield return new WaitForSeconds(time);
        state = State.Normal;
        animating = false;
        //EndAttackAnimation();
    }

    /// <summary>
    /// Event handler(animator)
    /// </summary>
    private void EndAttackAnimation(){
        //state = State.Normal;
        //animating = false;
        return;
    }

    /// <summary>
    /// Handles shied movement and animation.
    /// </summary>
    private void HandleShielding(){
        characterAnimationController.CharacterShield(lookingDir);
        moveDir = Vector3.zero;
    }

    /// <summary>
    /// Invincibility coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator BecomeTemporarilyInvincible(){
        float invincibilityDuration = 0.15f;
        isInvincible = true;

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
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
        //deactivate shield, since after playing dash particles,
        //shield also triggers
        shield.DeactivateShield();
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

        //check landing tile's z-index
        int2 dashInt2 = new int2((int)dashPosition.x, (int)dashPosition.y);
        int2 playerInt2 = new int2((int)transform.position.x, (int)transform.position.y);
        TDTile landTile = mapRef.GetTile(mapRef.TileRelativePos(dashInt2), mapRef.TileChunkPos(dashInt2));
        TDTile playerTile = mapRef.GetTile(mapRef.TileRelativePos(playerInt2), mapRef.TileChunkPos(playerInt2));

        if (playerTile.z_index != landTile.z_index)
        {
            SoundManager.PlaySound(SoundManager.Sound.Error, transform.position);
            dash = false;
            dashPosition = this.transform.position;
            return;
        }else{
            //UI cooldown
            uiHandler.DashCooldown();
        }

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

    public void HealSpell(){
        float castingTime = 5f;
        state = PlayerController.State.Interacting;
        animating = true;
        characterAnimationController.CharacterInteraction(lookingDir);

        StartCoroutine(Healing(castingTime));
    }

    private IEnumerator Healing(float time){
        SoundManager.casting = true;
        StartCoroutine(SoundManager.Loop(time, SoundManager.Sound.Casting, this.transform.position));
        for( ; time >= 0 ; time -= Time.deltaTime )
        {
            if( interruptCoroutine )
            {
                SoundManager.casting = false;
                interruptCoroutine = false;
                animating = false;
                yield break ;
            }
            yield return null ;
        }
        SoundManager.casting = false;
        SoundManager.PlaySound(SoundManager.Sound.Heal, this.transform.position);
        state = PlayerController.State.Normal;
        animating = false;
        uiHandler.HealCooldown();
        healthSystem.Heal(healSpellAmount);
        //damage popup
        DamagePopup.Create(transform.position, healSpellAmount.ToString(), DamagePopup.Type.Heal);
    }

    /// <summary>
    /// Interaction simulation with keeystone.
    /// </summary>
    /// <param name="interactionTime">TIme interacting with the keystone.</param>
    public void Interaction(float interactionTime, GameObject keystone){
        state = PlayerController.State.Interacting;
        animating = true;
        characterAnimationController.CharacterInteraction(lookingDir);

        StartCoroutine(Interacting(interactionTime, keystone));
    }

    /// <summary>
    /// Interacting coroutine, interruptable via interruptCoroutine.
    /// </summary>
    /// <param name="time">Time to interact.</param>
    /// <param name="keystone">Keystone interacting with.</param>
    /// <returns></returns>
    private IEnumerator Interacting(float time, GameObject keystone){
        SoundManager.casting = true;
        StartCoroutine(SoundManager.Loop(time, SoundManager.Sound.Casting, this.transform.position));
        for( ; time >= 0 ; time -= Time.deltaTime )
        {
            if( interruptCoroutine )
            {
                SoundManager.casting = false;
                interruptCoroutine = false;
                animating = false;
                yield break ;
            }
            yield return null ;
        }
        SoundManager.casting = false;
        state = PlayerController.State.Normal;
        animating = false;
        keystone.GetComponent<KeyObject>().Completed();
    }
}