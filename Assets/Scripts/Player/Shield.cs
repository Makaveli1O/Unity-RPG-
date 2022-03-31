using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField]private Transform mainCamera;
    private Vector3 cameraSize;
    private Vector3 worldSize;
    public Transform HealthBarPrefab;
    private Transform healthBarTransform;
    private HealthBar healthBar;
    public HealthSystem healthSystem;
    public int shieldHealth;
    private const float REGEN_DEFAULT = 3f;
    private float regenTimer;
    private int regenRate = 2;

    [SerializeField] private ParticleSystem particles;
    public bool isActive{
        get{return particles.isPlaying;}
    }

    private void Awake() {
        cameraSize = new Vector3(80,40);
        worldSize = new Vector3(Const.WORLD_HEALTHBAR_WIDTH, Const.WORLD_HEALTHBAR_HEIGHT);
        healthSystem = new HealthSystem(shieldHealth);
        healthBarTransform = Instantiate(HealthBarPrefab, new Vector3(13.55f,-7.77f), Quaternion.identity, mainCamera);
        healthBarTransform.localScale = cameraSize;
        
        healthBar = healthBarTransform.GetComponent<HealthBar>();
        healthBar.Setup(healthSystem, Color.cyan);
        healthBar.HideHealthBar();
        healthBar.OnHealthChanged += Shield_OnHealthChanged;
        //trn off particle system for shield
        var emission = healthBar.GetComponent<ParticleSystem>().emission;
        emission.enabled = false;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        //if shield is damaged, prepare to regenerate
        if (!healthSystem.IsFull)
        {
            //regen time counter subtraction
            if (regenTimer > 0)
            {
                regenTimer -= Time.deltaTime;
            }else{  //ready to regenerate
                Regenerate();
            }
        }
    }


    /// <summary>
    /// Subscribed to shield's healthbar, so if shield gets damaged, it is notified and timer 
    /// for regeneration is set to default reset value.
    /// </summary>
    /// <param name="sender">Object sender</param>
    /// <param name="e">event arguments(empty in this case)</param>
    private void Shield_OnHealthChanged(object sender, System.EventArgs e){
       regenTimer = REGEN_DEFAULT;
    }

    /// <summary>
    /// Regenerates shield to full health.
    /// </summary>
    private void Regenerate(){
        healthSystem.Heal(regenRate);
        DamagePopup.Create(transform.position, regenRate.ToString(), DamagePopup.Type.Shield);
    }

    /// <summary>
    /// Activates shield.
    /// </summary>
    public void ActivateShield(){
        particles.Play();
        healthBar.ShowHealthBar(cameraSize);
    }

    /// <summary>
    /// Deactivates shield.
    /// </summary>
    public void DeactivateShield(){
        if (isActive) particles.Stop(); 
        healthBar.HideHealthBar();
        return;
    }
}
