using UnityEngine;
using TMPro;
public class LoadButton : MonoBehaviour
{
    public GameObject textObj;
    public TextMeshProUGUI text;
    public int saveSeed;
    [SerializeField] private SceneLoader sceneLoader;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        this.sceneLoader = GameObject.FindGameObjectWithTag("Loader").GetComponent<SceneLoader>();
    }

    public void LoadGame(){
        SceneIntel.seed = this.saveSeed; 
        SceneIntel.loaded = true;
        sceneLoader.LoadScene(1);
    }
}
