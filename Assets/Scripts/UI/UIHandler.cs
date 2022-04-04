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

    public bool IsSlashReady{
        get{
            return slashScript.IsReady;
        }
    }
    
    [SerializeField]private Transform DashIcon;
    [SerializeField]private Transform AoeIcon;
    [SerializeField]private Transform ShieldIcon;
    [SerializeField]private Transform SlashIcon;
    private UIButton dashScript;
    private UIButton aoeScript;
    private UIButton shieldScript;
    private UIButton slashScript;
    private void Start() {
        dashScript = DashIcon.GetComponent<UIButton>();
        aoeScript = AoeIcon.GetComponent<UIButton>();
        shieldScript = ShieldIcon.GetComponent<UIButton>();
        slashScript = SlashIcon.GetComponent<UIButton>();
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

    public void ShieldCooldown(){
        shieldScript.UseButton(true);
    }

    public void ShieldCooldownDeactivate(){
        shieldScript.DeactivateButton();
    }

    public void SlashCooldown(){
        slashScript.UseButton();
    }

}
