using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private GameObject play;
    [SerializeField] private GameObject save;
    [SerializeField] private GameObject quit;
    [SerializeField] private GameObject map;

    
    public void OnContinuePressed(){
        ButtonSfx();
        CloseMenu();
    }

    public void OpenMenu(){
        gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    public void CloseMenu(){
        gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnSavePressed(){
        ButtonSfx();
        MapController mc = map.GetComponent<MapController>();
        mc.ManualSave();
    }

    public void OnQuitPressed(){
        ButtonSfx();
        OnSavePressed();
        Application.Quit();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
         SoundManager.PlaySound(SoundManager.Sound.ButtonHover);
    }

    private void ButtonSfx(){
        SoundManager.PlaySound(SoundManager.Sound.ButtonPressed);
    }
}
