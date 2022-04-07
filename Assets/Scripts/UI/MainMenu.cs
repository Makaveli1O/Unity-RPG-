using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private string game = "SampleScene";
    [SerializeField] private GameObject main;
    [SerializeField] private GameObject load;

    
    public void StartGame(){
        ButtonSfx();
        SceneManager.LoadScene(game);
    }

    public void OpenLoad(){
        ButtonSfx();
        load.SetActive(true);
        main.SetActive(false);
    }

    public void CloseLoad(){
        ButtonSfx();
        load.SetActive(false);
        main.SetActive(true);
    }

    public void QuitGame(){
        ButtonSfx();
        Application.Quit();
    }

    public void ButtonSfx(){
        //TODO
    }
}
