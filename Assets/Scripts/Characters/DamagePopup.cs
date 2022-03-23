using UnityEngine;
using TMPro; 

/// <summary>
/// Damage popup handler and creator.
/// </summary>
public class DamagePopup : MonoBehaviour
{
    public enum Type
    {
        Damage,
        Miss,
        Heal,
        Critical
    }

    private Type damageType;
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
    public static DamagePopup Create(Vector3 position, string damageAmount, Type type = Type.Damage){
        Transform damagePopupTransform = Instantiate(GameAssets.Instance.pfDamagePopup, position, Quaternion.identity);
        DamagePopup damagePopup = damagePopupTransform.GetComponent<DamagePopup>();
        damagePopup.Setup(damageAmount, type);

        return damagePopup;
    }

    /// <summary>
    /// Set correct text to mesh
    /// </summary>
    /// <param name="damageAmount">Damage amoung</param>
    public void Setup(string damageAmount, Type type){
        textMesh.SetText(damageAmount);
        textMesh.fontSize = 10;

        switch (type){
            case Type.Damage:
                textMesh.color = Color.cyan;
                break;
            case Type.Critical:
                textMesh.color = Color.red;
                break;
            case Type.Miss:
                textMesh.color = Color.white;
                break;
            case Type.Heal:
                textMesh.color = Color.green;
                break;
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
