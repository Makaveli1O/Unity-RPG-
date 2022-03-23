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
    bool InCombat{
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
}