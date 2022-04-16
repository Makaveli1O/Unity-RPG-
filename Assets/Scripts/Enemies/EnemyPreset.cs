using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Preset", menuName = "New Enemy Preset")]
public class EnemyPreset : ScriptableObject{
    public bool twoDirSprite;
    public string _name;
    public int id;
    public float attackAnimationDuration;
    public float attackSfxDelay;
    public int health = 100;
    public int armor = 100;
    public int bottomDamage = 100;
    public int topDamage = 100;
    public float attackRange = 5f;
    public float attackTime = 5f;
    public float multiplier;
    public float movementSpeed = 1f;
    public Vector2 colliderOffset;
    public UnityEngine.U2D.Animation.SpriteLibraryAsset assetsLibrary;
    public GameAssets.SoundAudioClip[] sfx;
}