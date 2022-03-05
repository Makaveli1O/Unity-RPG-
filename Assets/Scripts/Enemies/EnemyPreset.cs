using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Preset", menuName = "New Enemy Preset")]
public class EnemyPreset : ScriptableObject{
    public string _name;
    public int id;
    public Sprite sprite;
    public int defaultHealth;
    public int defaultArmor;
    public float lvlMultiplier;  // power multiplier increased by level
    public UnityEngine.U2D.Animation.SpriteLibraryAsset assetsLibrary;
}