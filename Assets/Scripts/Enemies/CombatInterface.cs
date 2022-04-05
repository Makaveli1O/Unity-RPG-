using UnityEngine;
public interface CombatInterface
{
    int health{
        get;
        set;
    }
    int armor{
        get;
        set;
    }
    bool IsDead{
        get;
        set;
    }
    float attackRange{
        get;
        set;
    }
    float attackTime{   //higher -> slower attacking rate
        get;
        set;
    }
    public void Die();
    public void Alive();
    public void Hurt();
    public bool Damage(Vector3 attackerPosition, string damageAmount, float attackRange = 0f);
}