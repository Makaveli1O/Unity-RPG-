using UnityEngine;
using Unity.Mathematics;

public class Sphere : MonoBehaviour
{
    public int2 pos;
    public BiomePreset biome;
    public Damage damageState;
    
    /*  *   *   *   *   *   *   *
        H   E   A   L   T   H
    *   *   *   *   *   *   *  */
    public Transform HealthBarPrefab;
    private Transform healthBarTransform;
    private HealthBar healthBar;
    public HealthSystem healthSystem;

    private void Start() {
        InitHealthBar(100);
    }
    /// <summary>
    /// Initialize healthbar for crystal
    /// </summary>
    /// <param name="maxHealth">Maximum health</param>
    private void InitHealthBar(int maxHealth){
        healthSystem = new HealthSystem(maxHealth);
        healthBarTransform = Instantiate(HealthBarPrefab, new Vector3(this.transform.position.x, this.transform.position.y + 2f), Quaternion.identity, this.gameObject.transform);
        healthBarTransform.localScale = new Vector3(11,7);

        healthBar = healthBarTransform.GetComponent<HealthBar>();
        healthBar.Setup(healthSystem);
    }

    public void LowDamage(){

    }

    public void MediumDamage(){

    }

    public void HighDamage(){

    }

    public void Destroyed(){
        return;
    }
}


/// <summary>
/// Enum indicating state of key object.
/// </summary>
public enum Damage{
    None, 
    Low,
    Medium,
    High,
    Destroyed
}
