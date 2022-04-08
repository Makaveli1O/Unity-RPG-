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
        SceneIntel.seed = (int) slider.value;
    }


    public void ChangeValue(){
        textMesh.SetText(slider.value.ToString());
        SceneIntel.seed = (int) slider.value;
        return;
    }

}
