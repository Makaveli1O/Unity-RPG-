using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour, IPointerEnterHandler
{
    private string menu = "MainMenu";
    public void LoadMainMenu(){
        ButtonSfx();
        CloseMenu();
        SceneManager.LoadScene(menu);
    }

    public void ButtonSfx(){
        SoundManager.PlaySound(SoundManager.Sound.ButtonPressed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
         SoundManager.PlaySound(SoundManager.Sound.ButtonHover);
    }

    public void OpenMenu(){
        gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    public void CloseMenu(){
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }
}
