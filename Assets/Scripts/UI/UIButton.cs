using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Class handling cooldown icon UIs.
/// </summary>
public class UIButton : MonoBehaviour
{
    public bool IsReady{
        get{
            return this.spellReady;
        }
    }
    [SerializeField] private Image cooldownImage;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private float COOLDOWN;
    private bool spellReady;

    private void Start() {
        Init();
    }

    /// <summary>
    /// Initializes button.
    /// </summary>
    private void Init(){
        spellReady = true;
        cooldownImage.gameObject.SetActive(true);
        cooldownImage.fillAmount = 0.0f;
        cooldownText.text = Mathf.RoundToInt(COOLDOWN).ToString();
    }

    /// <summary>
    /// Starts corouotine of button. Cooldown visibility.
    /// </summary>
    public void UseButton(){
        StartCoroutine(CooldownLoading(COOLDOWN));
    }

    /// <summary>
    /// Handles functionality of cooldown. Count down, and visuals.
    /// </summary>
    /// <param name="secondsToWait">Seconds fot cooldown to last.</param>
    /// <returns></returns>
    private IEnumerator CooldownLoading(float secondsToWait){
        spellReady = false;
        cooldownText.gameObject.SetActive(true);
        float actualSeconds = 0f;
        while (actualSeconds < secondsToWait)
        {
            yield return new WaitForSeconds(0.1f);
            actualSeconds += 0.1f;
            //reversed counting
            cooldownText.text = Mathf.RoundToInt(secondsToWait - actualSeconds).ToString();
            cooldownImage.fillAmount = 1f - (actualSeconds / secondsToWait);
        }
        //cooldown done
        cooldownText.gameObject.SetActive(false);
        cooldownImage.fillAmount = 0.0f;
        spellReady = true;
        //TODO sfx
    }
}
