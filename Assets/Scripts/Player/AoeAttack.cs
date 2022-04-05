using UnityEngine;
using System.Collections.Generic;

public class AoeAttack : MonoBehaviour
{
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private ParticleSystem ps;
    private Vfx vfxScript;
    public bool isActive{
        get{return ps.isPlaying;}
    }

    private float radius;
    public float aoeAttackDuration = 0.65f;

    private void Awake() {
        vfxScript = vfxPrefab.GetComponent<Vfx>();
        //radius seems to be half of the scale I've entered in inspector
        if (ps != null) radius = ps.transform.localScale.x / 2f;
    }

    /// <summary>
    /// Performs aoe attack on enemies within spell radius.
    /// </summary>
    /// <param name="enemiesWithinRadius">List of enemies within radius</param>
    /// <param name="damage">Damage amount</param>
    public void Perform(List<EnemyController> enemiesWithinRadius, int damage){

        foreach (EnemyController enemy in enemiesWithinRadius)
        {
            //skip dead enemies
            if (enemy.IsDead) continue;
            GameObject vfxObj = Instantiate(vfxPrefab, this.transform.position, Quaternion.identity);
            vfxObj.transform.parent = this.transform;
            Vfx slash = vfxObj.GetComponent<Vfx>();
            slash.SetVFXType(Vfx.ElementType.Slash);
            //set slash's dmg
            slash.SetDamageAmount(damage);
            //spawn on top of enemy head and destroy
            slash.EnemyPerform(enemy.transform.position, aoeAttackDuration);
            //TODO sfx
        }
    }


    /// <summary>
    /// Shows radius of aoe around player
    /// </summary>
    public void ShowRadius(){
        ps.Play();
        GameAssets.Instance.cursorHandler.SetCursorByType(CursorType.Apply);
    }

    /// <summary>
    /// Hide aoe radius around player
    /// </summary>
    public void HideRadius(){
        //hide radius circle
        ps.Stop(); 
        ps.Clear();
        //change cursor type back to  basic
        GameAssets.Instance.cursorHandler.SetCursorByType(CursorType.Basic); 
        return;
    }

}
