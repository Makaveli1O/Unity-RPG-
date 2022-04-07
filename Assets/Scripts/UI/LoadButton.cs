using UnityEngine;
using TMPro;
public class LoadButton : MonoBehaviour
{
    public GameObject textObj;
    public TextMeshProUGUI text;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        //text = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        //text = GetComponent<TextMeshProUGUI>();
    }
}
