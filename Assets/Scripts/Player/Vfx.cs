using UnityEngine;
using System.Collections;

/// <summary>
/// Handles Vfx for spells. Divided into two categories, characters and ground effects.
/// Character effects performs on top of the character, while ground on the tiles itself.
/// </summary>
public class Vfx : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    public VfxBundle[] vfxs;
    private bool moving;
    private float radius;
    private Vector3 initialPosition;
    private float moveSpeed = 3f;
    private EnemyController enemy;
    private int damage;


    /// <summary>
    /// type of vfx preset
    /// </summary>
    public enum ElementType
    {
        Wind,
        Fire,
        Holy,
        Slash
    }
    /// <summary>
    /// Bundle of type of vfx and assetsLibrary
    /// </summary>
    [System.Serializable]
    public class VfxBundle{
        public ElementType type;
        public UnityEngine.U2D.Animation.SpriteLibraryAsset assetsLibrary;
    }


    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        this.rb.freezeRotation = true;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Entity")
        {
            enemy = other.gameObject.GetComponent<EnemyController>();
            enemy.Damage(this.transform.position, damage.ToString());
        }
    }

    public void SetVFXType(ElementType type){
        foreach (var item in vfxs)
        {
            if (item.type == type)
            {
                var sla = GetComponent<UnityEngine.U2D.Animation.SpriteLibrary>();
                sla.spriteLibraryAsset = item.assetsLibrary;
            }
        }
    }

    public void SetDamageAmount(int damageAmount){
        this.damage = damageAmount;
    }

    /// <summary>
    /// Perform movement of attached gameobject, and sets damage of this object.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="radius"></param>
    public void Move(Vector3 direction, float time){
        moving = true;
        StartCoroutine(Moving(direction, time));
    }

    /// <summary>
    /// Coroutine moving gameobject, after time is done, destroy continuously.
    /// </summary>
    /// <param name="direction">direction of gameobject</param>
    /// <param name="time">life time</param>
    /// <returns></returns>
    private IEnumerator Moving(Vector3 direction, float time){
        rb.AddForce(direction * moveSpeed, ForceMode2D.Impulse);
        yield return new WaitForSeconds(time);
        Object.Destroy(this.gameObject);
    }

    /// <summary>
    /// Performs only on top of the enemy and destroy.
    /// </summary>
    public void EnemyPerform(Vector3 enemyPos, float time){
        this.transform.position = enemyPos;
        StartCoroutine(Performing(time));
        return;
    }

    private IEnumerator Performing(float time){
        yield return new WaitForSeconds(time);
        Object.Destroy(this.gameObject);
    }
}




