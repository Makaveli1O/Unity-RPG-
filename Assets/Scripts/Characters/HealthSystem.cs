using System;
public class HealthSystem
{
    public event EventHandler OnHealthChanged;
    private int health;
    private int healthMax;

    public HealthSystem(int health){
        this.health = health;
        this.healthMax = health; 
    }

    public int GetHealth(){
        return health;
    }

    public void Damage(int damageAmount){
        this.health -= damageAmount;
        if (health < 0) health = 0;
        //if there are subscribers fire event
        if (OnHealthChanged != null) OnHealthChanged(this, EventArgs.Empty);
    }

    public void Heal(int healAmount){
        this.health += healAmount;
        if (health > healthMax) health = healthMax;
        if (OnHealthChanged != null) OnHealthChanged(this, EventArgs.Empty);
    }

    public void HealMax(){
        Heal(this.healthMax);
    }

    public float GetHealthPercent(){
         return (float) health / healthMax;
    }
}
