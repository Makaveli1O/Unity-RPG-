using UnityEngine;
public class UIHandler : MonoBehaviour
{
    /// <summary>
    /// Returns whenever dash is ready or not.
    /// </summary>
    /// <value></value>
    public bool IsDashReady{
        get{
            return dashScript.IsReady;
        }
    }

    public bool IsAoeReady{
        get{
            return aoeScript.IsReady;
        }
    }
    
    [SerializeField]private Transform DashIcon;
    [SerializeField]private Transform AoeIcon;
    private UIButton dashScript;
    private UIButton aoeScript;
    private void Start() {
        dashScript = DashIcon.GetComponent<UIButton>();
        aoeScript = AoeIcon.GetComponent<UIButton>();
    }

    /// <summary>
    /// Performs cooldwn visuals for dash button.
    /// </summary>
    public void DashCooldown(){
        dashScript.UseButton();
    }

    public void AoeCooldown(){
        aoeScript.UseButton();
    }

}
