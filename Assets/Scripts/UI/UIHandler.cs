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
    
    [SerializeField]private Transform DashIcon;
    private UIButton dashScript;
    private void Start() {
        dashScript = DashIcon.GetComponent<UIButton>();
    }

    /// <summary>
    /// Performs cooldwn visuals for dash button.
    /// </summary>
    public void DashCooldown(){
        dashScript.UseButton();
    }

}
