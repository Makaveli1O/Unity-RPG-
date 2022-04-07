using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public GameObject textObj;

    private TextMeshProUGUI textMesh;
    private Slider slider;

    private void Awake() {
        textMesh = textObj.GetComponent<TextMeshProUGUI>();
        slider = GetComponent<Slider>();
    }


    public void ChangeValue(){
        textMesh.SetText(slider.value.ToString());
        return;
    }

}
