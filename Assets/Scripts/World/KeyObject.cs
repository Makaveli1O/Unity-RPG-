using UnityEngine;
using Unity.Mathematics;

public class KeyObject : MonoBehaviour
{
    public int2 pos;
    public BiomePreset biome;
    public State state;
    
}


/// <summary>
/// Enum indicating state of key object.
/// </summary>
public enum State{
    Idle,
    Opened,
    Looted
}
