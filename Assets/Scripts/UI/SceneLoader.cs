using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    public GameObject loadingScreen;
    public Slider loadingslider;
    [SerializeField]private GameObject tmpro;
    public TextMeshProUGUI textMesh;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        this.textMesh = tmpro.GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Asynchronously loads scene with given index.
    /// </summary>
    /// <param name="sceneIndex"></param>
    public void LoadScene(int sceneIndex){
        StartCoroutine(LoadSceneAsynchronously(sceneIndex));
    }

    /// <summary>
    /// Updates loading slider value, for better GUI experience.
    /// </summary>
    /// <param name="value">precentual value</param>
    public void UpdateSliderValue(float value){
        loadingslider.value = value;
        return;
    }

    /// <summary>
    /// Sets new  loading parameters (tmp and slider reset)
    /// </summary>
    /// <param name="name">Text above slider.</param>
    public void NewLoading(string name){
        textMesh.SetText(name);
        loadingslider.value = 0f;
        return;
    }

    /// <summary>
    /// Coroutine to load scene async.
    /// </summary>
    /// <param name="sceneIndex">Index of the scene to be loaded.</param>
    /// <returns></returns>
    public IEnumerator LoadSceneAsynchronously(int sceneIndex){
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        loadingScreen.SetActive(true);
        while (!operation.isDone)
        {
            loadingslider.value = operation.progress;
            yield return null;//waior for the next frame (loading is not done)
        }
    }
}
