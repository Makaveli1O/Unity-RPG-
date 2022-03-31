using System.Collections.Generic;
using System;

public static class Helper
{

    private static Random rng = new Random();  
    /// <summary>
    /// Shuffles given list randomly using Fisher_Tates shuffle algorithm
    /// described here: n.wikipedia.org/wiki/Fisher–Yates_shuffle
    /// </summary>
    /// <param name="list">Desired list to be shuffled.</param>
    /// <typeparam name="T">Whatever type you like it's generic</typeparam>
    public static void Fisher_Yates_shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}


/// <summary>
/// Custom event args for shield notification and health system notification to
/// distinguish between heal and damage type.
/// </summary>
public class ShieldEventArgs : EventArgs
{
    public enum Type
    {
        Heal,
        Damage,
        Absorb
    }
    public Type ChangeHealthType;
}
