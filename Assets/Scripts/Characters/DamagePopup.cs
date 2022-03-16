using UnityEngine;
using TMPro; 

/// <summary>
/// Damage popup handler and creator.
/// </summary>
public class DamagePopup : MonoBehaviour
{
    private const float DISAPPEAR_TIMER_MAX = 1f;
    private TextMeshPro textMesh;
    private float timer;    //disappear timer for popup to hide (seconds)
    private Color textColor;
    Vector3 moveVector;

    /// <summary>
    /// Creates instance of damagePop up
    /// </summary>
    /// <param name="position">Position to be spawned </param>
    /// <param name="damageAmount">Text damage</param>
    /// <returns></returns>
    public static DamagePopup Create(Vector3 position, int damageAmount, bool isCriticalHit = false){
        Transform damagePopupTransform = Instantiate(GameAssets.Instance.pfDamagePopup, position, Quaternion.identity);
        DamagePopup damagePopup = damagePopupTransform.GetComponent<DamagePopup>();
        damagePopup.Setup(damageAmount, isCriticalHit);

        return damagePopup;
    }

    /// <summary>
    /// Set correct text to mesh
    /// </summary>
    /// <param name="damageAmount">Damag eamoung</param>
    public void Setup(int damageAmount, bool isCriticalHit){
        textMesh.SetText(damageAmount.ToString());
        if (isCriticalHit)
        {
            textMesh.fontSize = 15;
        }else{
            textMesh.fontSize = 10;
        }
        textColor = textMesh.color;
        timer = DISAPPEAR_TIMER_MAX;
        moveVector = new Vector3(.6f,1f) * 15f;
    }

    private void Awake() {
        textMesh = GetComponent<TextMeshPro>();
    }

    private void Update() {
        //move popup to up
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        //first half of the timer (text getting bigger)
        if (timer > (DISAPPEAR_TIMER_MAX * 0.5f))
        {
            float increaseScaleAmount = 1f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }else{
            float decreaseScaleAmount = 1f;
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            //Start disappearing
            float disapperSpeed = 3f;
            textColor.a -= disapperSpeed * Time.deltaTime;
            textMesh.color = textColor; 
            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
