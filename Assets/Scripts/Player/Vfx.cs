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
    private bool movement;
    private float radius;
    private Vector3 initialPosition;
    private float moveSpeed = 3f;

    private EnemyController enemy;


    /// <summary>
    /// type of vfx preset
    /// </summary>
    public enum ElementType
    {
        Wind,
        Fire,
        Holy
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
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        /*if (movement)
        {
            float distance = Vector3.Distance(transform.position, initialPosition);
            //destroy tornado after out of radius
            if (distance > radius)
            {
                Object.Destroy(this.gameObject);
            }
        }*/
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
            enemy.Damage(this.transform.position, "50");
        }
    }

    /// <summary>
    /// Perform movement of attached gameobject
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="radius"></param>
    public void Move(Vector3 direction, float time){
        StartCoroutine(Moving(direction, time));
    }

    /// <summary>
    /// Coroutine moving gameobject, after time is done destroy object
    /// </summary>
    /// <param name="direction">direction of gameobject</param>
    /// <param name="time">life time</param>
    /// <returns></returns>
    private IEnumerator Moving(Vector3 direction, float time){
        rb.AddForce(direction * moveSpeed, ForceMode2D.Impulse);
        yield return new WaitForSeconds(time);
        Object.Destroy(this.gameObject);
    }
}




