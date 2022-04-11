using System;
//TODO docs
public class HealthSystem
{
    public event EventHandler<ShieldEventArgs> OnHealthChanged;
    private int health;
    private int healthMax;
    public bool IsFull
    {
        get {return health == healthMax;}
    }

    public HealthSystem(int health){
        this.health = health;
        this.healthMax = health; 
    }

    public void SetHealth(int amount){
        this.health = amount;
        if (OnHealthChanged != null){
            //args customizaton
            ShieldEventArgs args = new ShieldEventArgs();
            args.ChangeHealthType = ShieldEventArgs.Type.Damage;
            //fire to subscribers
            OnHealthChanged(this, args);
        }
        return;
    }

    /// <summary>
    /// Get current health of healthSystem.
    /// </summary>
    /// <returns>Integer health value.</returns>
    public int GetHealth(){
        return health;
    }

    /// <summary>
    /// Damage health system by given amount.
    /// </summary>
    /// <param name="damageAmount">Damage to be done.</param>
    public void Damage(int damageAmount){
        this.health -= damageAmount;
        if (health < 0) health = 0;
        //if there are subscribers fire event
        if (OnHealthChanged != null){
            //args customizaton
            ShieldEventArgs args = new ShieldEventArgs();
            args.ChangeHealthType = ShieldEventArgs.Type.Damage;
            //fire to subscribers
            OnHealthChanged(this, args);
        }
    }

    /// <summary>
    /// Heal healthSystem by given amount.
    /// </summary>
    /// <param name="healAmount">Amount to be healed.</param>
    public void Heal(int healAmount){
        this.health += healAmount;
        if (health > healthMax) health = healthMax;
        if (OnHealthChanged != null){
            //args customizaton
            ShieldEventArgs args = new ShieldEventArgs();
            args.ChangeHealthType = ShieldEventArgs.Type.Heal;
            //fire to subscribers
            OnHealthChanged(this, args);
        }
    }

    /// <summary>
    /// Fully heal this healthSystem.
    /// </summary>
    public void HealMax(){
        Heal(this.healthMax);
    }

    /// <summary>
    /// Get current health percentage.
    /// </summary>
    /// <returns>Float health percentage value.</returns>
    public float GetHealthPercent(){
         return (float) health / healthMax;
    }
}
