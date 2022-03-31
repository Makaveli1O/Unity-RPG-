using UnityEngine;

/// <summary>
/// Class handling aoe spell
/// </summary>
public class AoeSpell : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;
    public bool isActive{
        get{return ps.isPlaying;}
    }
    public void ShowRadius(){
        //show radius circle
        ps.Play();  
        //change cursor type
        GameAssets.Instance.cursorHandler.SetCursorByType(CursorType.Apply);
    }

    public void HideRadius(){
        if (isActive){
            //hide radius circle
            ps.Stop(); 
            ps.Clear();
            //change cursor type back to  basic
            GameAssets.Instance.cursorHandler.SetCursorByType(CursorType.Basic); 
        }  
        return;
    }
}
