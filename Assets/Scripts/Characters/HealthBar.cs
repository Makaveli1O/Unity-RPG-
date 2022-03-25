using UnityEngine;
using System;

public class HealthBar : MonoBehaviour
{
    public event EventHandler OnHealthChanged;  //to notify shield
    private SpriteRenderer barSR;   //bar spriteRenderer
    private ParticleSystem ps;
    private HealthSystem healthSystem;

    /// <summary>
    /// Proper healthbar Setup function with given HealthSystem and colour.
    /// </summary>
    /// <param name="hs">Given health system to attach to.</param>
    /// <param name="color">Custom colour.</param>
    public void Setup(HealthSystem hs, Color color){
        this.healthSystem = hs;
        Transform obj = gameObject.transform.Find("Bar"); 
        obj = obj.transform.Find("BarSprite");
        barSR = obj.GetComponent<SpriteRenderer>();
        barSR.color = color;

        healthSystem.OnHealthChanged += HealthSystem_OnHealthChanged;   //subscribe
    }

    /// <summary>
    /// Send event about chaned health to subscribers.
    /// </summary>
    /// <param name="sender">Sender obj</param>
    /// <param name="e">args</param>
    private void HealthSystem_OnHealthChanged(object sender, ShieldEventArgs e){
        transform.Find("Bar").localScale = new Vector3(healthSystem.GetHealthPercent() , 1);
        //notify the shield for regeneration purposes
        if (OnHealthChanged != null) OnHealthChanged(this, EventArgs.Empty);
        if (e.ChangeHealthType == ShieldEventArgs.Type.Damage) ps.Play();
    }

    private void Awake() {
        this.ps = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Sets prefab's scale to be visible.
    /// </summary>
    public void ShowHealthBar(){
        gameObject.transform.localScale = new Vector3(Const.WORLD_HEALTHBAR_WIDTH, Const.WORLD_HEALTHBAR_HEIGHT);
    }

    /// <summary>
    /// Sets prefab's scale to be invisible.
    /// </summary>
    public void HideHealthBar(){
        gameObject.transform.localScale = new Vector3(0,0);
    }
}
