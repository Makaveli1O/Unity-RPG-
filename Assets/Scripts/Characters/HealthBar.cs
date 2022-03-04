using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private HealthSystem healthSystem;
    public void Setup(HealthSystem hs){
        this.healthSystem = hs;

        healthSystem.OnHealthChanged += HealthSystem_OnHealthChanged;   //subscribe
    }

    private void HealthSystem_OnHealthChanged(object sender, System.EventArgs e){
        transform.Find("Bar").localScale = new Vector3(healthSystem.GetHealthPercent() , 1);
    }

    private void Update() {
        
    }
}
