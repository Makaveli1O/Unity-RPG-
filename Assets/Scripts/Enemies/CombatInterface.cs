using UnityEngine;
using System;

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
}