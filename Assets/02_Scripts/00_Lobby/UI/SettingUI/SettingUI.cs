using UnityEngine;

public class Setting_UI : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private CanvasGroup soundCanvasGroup;
    [SerializeField] private RectTransform soundPanel;

    //[Header("Language")]
    //[SerializeField] private CanvasGroup languageCanvasGroup;
    //[SerializeField] private RectTransform languagePanel;

    [Header("Game")]
    [SerializeField] private CanvasGroup gameCanvasGroup;
    [SerializeField] private RectTransform gamePanel;

    [Header("SFX")]
    [SerializeField] private AudioClip buttonClickSFX;

    private void Start()
    {
        OpenSoundPanel(false);
    }

    public void OpenSoundPanel()
    {
        OpenSoundPanel(true);
    }

    private void OpenSoundPanel(bool playSFX)
    {
        if (playSFX)
            PlayButtonSFX();

        ShowPanel(soundCanvasGroup, soundPanel);
        //HidePanel(languageCanvasGroup, languagePanel);
        HidePanel(gameCanvasGroup, gamePanel);
    }

    //public void OpenLanguagePanel()
    //{
    //    PlayButtonSFX();

    //    HidePanel(soundCanvasGroup, soundPanel);
    //    ShowPanel(languageCanvasGroup, languagePanel);
    //    HidePanel(gameCanvasGroup, gamePanel);
    //}

    public void OpenGamePanel()
    {
        PlayButtonSFX();

        HidePanel(soundCanvasGroup, soundPanel);
        //HidePanel(languageCanvasGroup, languagePanel);
        ShowPanel(gameCanvasGroup, gamePanel);
    }

    private void PlayButtonSFX()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(buttonClickSFX);
        }
    }

    private void ShowPanel(CanvasGroup cg, RectTransform panel)
    {
        if (cg == null || panel == null)
            return;

        cg.gameObject.SetActive(true);
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private void HidePanel(CanvasGroup cg, RectTransform panel)
    {
        if (cg == null || panel == null)
            return;

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        cg.gameObject.SetActive(false);
    }
}