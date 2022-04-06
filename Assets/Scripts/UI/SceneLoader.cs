using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingslider;
    public void LoadScene(int sceneIndex){
        StartCoroutine(LoadSceneAsynchronously(sceneIndex));
    }

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
