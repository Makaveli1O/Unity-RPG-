using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private string game = "SampleScene";
    
    public void StartGame(){
        SceneManager.LoadScene(game);
    }

    public void OpenOptions(){

    }

    public void CloseOptions(){

    }

    public void QuitGame(){
        Application.Quit();
    }
}
