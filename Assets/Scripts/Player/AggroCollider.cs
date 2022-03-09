using UnityEngine;

public class AggroCollider : MonoBehaviour
{
    PlayerController player;
    private void Awake() {
        player = GetComponentInParent<PlayerController>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Entity")
        {
            EnemyController ec = other.GetComponent<EnemyController>();
            ec.InAggroRadius += EnemyController_InAggroRadius;  //subscribe
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Entity")
        {
            EnemyController ec = other.GetComponent<EnemyController>();
            ec.InAggroRadius -= EnemyController_InAggroRadius;  //unsubscribe
        }
    }

    /// <summary>
    /// Subscribed event function handling aggro
    /// </summary>
    /// <param name="sender">Enemy sender</param>
    /// <param name="e">args</param>
    private void EnemyController_InAggroRadius(object sender, System.EventArgs e){
        
    }
}
