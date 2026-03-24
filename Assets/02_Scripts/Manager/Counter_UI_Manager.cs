using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Counter_UI_Manager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    private float pauseOpenedTime;

    [Header("일시정지 버튼")]
    [SerializeField] private RectTransform pauseButton;

    [Header("일시정지 창")]
    [SerializeField] private GameObject pausePanel;

    [Header("클릭 복귀 딜레이")]
    [SerializeField] private float resumeClickDelay = 0.15f;

    [Header("열기 버튼")]
    [SerializeField] private RectTransform settingOpenButton;
    [SerializeField] private RectTransform bookOpenButton;
    [SerializeField] private RectTransform homeOpenButton;
    [SerializeField] private RectTransform homeYesButton;

    [Header("닫기 버튼")]
    [SerializeField] private RectTransform settingCloseButton;
    [SerializeField] private RectTransform bookCloseButton;
    [SerializeField] private RectTransform homeCloseButton;

    [Header("버튼 젤리")]
    [SerializeField] private float pressX = 0.96f;
    [SerializeField] private float pressY = 0.94f;

    [Header("Pause 메뉴 버튼 등장")]
    [SerializeField] private float menuButtonStartScale = 0.65f;
    [SerializeField] private float menuButtonOvershootScale = 1.12f;
    [SerializeField] private float menuButtonPopDuration = 0.22f;
    [SerializeField] private float menuButtonPopInterval = 0.08f;

    [Header("패널 등장/퇴장")]
    [SerializeField] private float buttonAnimDelay = 0.3f;
    [SerializeField] private float panelStartScale = 0.98f;
    [SerializeField] private float panelFadeDuration = 0.4f;
    [SerializeField] private float panelCloseDuration = 0.3f;

    [Header("Setting")]
    [SerializeField] private CanvasGroup settingCanvasGroup;
    [SerializeField] private RectTransform settingPanel;

    [Header("Book")]
    [SerializeField] private CanvasGroup bookCanvasGroup;
    [SerializeField] private RectTransform bookPanel;

    [Header("Home")]
    [SerializeField] private CanvasGroup homeCanvasGroup;
    [SerializeField] private RectTransform homePanel;

    [Header("사운드")]
    [SerializeField] private AudioClip buttonClickSFX;

    private Dictionary<RectTransform, Vector3> originalScales = new Dictionary<RectTransform, Vector3>();
    private Coroutine pauseMenuButtonsRoutine;

    void Awake()
    {
        CacheButtonScale(pauseButton);
        CacheButtonScale(settingOpenButton);
        CacheButtonScale(bookOpenButton);
        CacheButtonScale(homeOpenButton);
        CacheButtonScale(homeYesButton);

        CacheButtonScale(settingCloseButton);
        CacheButtonScale(bookCloseButton);
        CacheButtonScale(homeCloseButton);

        InitPanel(settingCanvasGroup, settingPanel);
        InitPanel(bookCanvasGroup, bookPanel);
        InitPanel(homeCanvasGroup, homePanel);
    }

    void CacheButtonScale(RectTransform button)
    {
        if (button == null) return;

        if (!originalScales.ContainsKey(button))
            originalScales.Add(button, button.localScale);
    }

    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        IsPaused = true;
        pauseOpenedTime = Time.unscaledTime;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        PreparePauseMenuButtons();
        PlayButtonJelly(pauseButton);

        Time.timeScale = 0f;

        if (pauseMenuButtonsRoutine != null)
            StopCoroutine(pauseMenuButtonsRoutine);

        pauseMenuButtonsRoutine = StartCoroutine(ShowPauseMenuButtonsRoutine());
    }

    public void OnPausePanelClicked()
    {
        if (!IsPaused)
            return;

        if (Time.unscaledTime - pauseOpenedTime < resumeClickDelay)
            return;

        ResumeGame();
    }

    public void ResumeGame()
    {
        StopPauseMenuButtonsAnimation();

        IsPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void OnDisable()
    {
        StopPauseMenuButtonsAnimation();

        Time.timeScale = 1f;
        IsPaused = false;
    }

    void InitPanel(CanvasGroup cg, RectTransform panel)
    {
        if (cg == null || panel == null) return;

        cg.gameObject.SetActive(false);
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        panel.localScale = Vector3.one * panelStartScale;
    }

    public void OnClickButtonSFXOnly()
    {
        PlayButtonClickSFXOnly();
    }

    void PlayButtonClickSFXOnly()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(buttonClickSFX);
    }

    void PlayButtonJelly(RectTransform target)
    {
        if (target == null) return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(buttonClickSFX);

        if (!originalScales.TryGetValue(target, out Vector3 originalScale))
            originalScale = target.localScale;

        target.DOKill();
        target.localScale = originalScale;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(
            target.DOScale(new Vector3(originalScale.x * pressX, originalScale.y * pressY, originalScale.z), 0.1f)
                  .SetEase(Ease.OutCubic)
        );
        seq.Append(
            target.DOScale(originalScale, 0.14f)
                  .SetEase(Ease.OutQuad)
        );
    }

    void PreparePauseMenuButtons()
    {
        PreparePauseMenuButton(settingOpenButton);
        PreparePauseMenuButton(bookOpenButton);
        PreparePauseMenuButton(homeOpenButton);
    }

    void PreparePauseMenuButton(RectTransform target)
    {
        if (target == null) return;

        CanvasGroup cg = GetOrAddCanvasGroup(target);

        if (!originalScales.TryGetValue(target, out Vector3 originalScale))
            originalScale = target.localScale;

        target.DOKill();
        cg.DOKill();

        target.gameObject.SetActive(true);
        target.localScale = originalScale * menuButtonStartScale;

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    IEnumerator ShowPauseMenuButtonsRoutine()
    {
        yield return StartCoroutine(PopPauseMenuButton(settingOpenButton));
        yield return new WaitForSecondsRealtime(menuButtonPopInterval);

        yield return StartCoroutine(PopPauseMenuButton(bookOpenButton));
        yield return new WaitForSecondsRealtime(menuButtonPopInterval);

        yield return StartCoroutine(PopPauseMenuButton(homeOpenButton));

        pauseMenuButtonsRoutine = null;
    }

    IEnumerator PopPauseMenuButton(RectTransform target)
    {
        if (target == null)
            yield break;

        CanvasGroup cg = GetOrAddCanvasGroup(target);

        if (!originalScales.TryGetValue(target, out Vector3 originalScale))
            originalScale = target.localScale;

        Vector3 startScale = originalScale * menuButtonStartScale;
        Vector3 overshootScale = originalScale * menuButtonOvershootScale;

        target.localScale = startScale;
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(
            target.DOScale(overshootScale, menuButtonPopDuration * 0.65f)
                  .SetEase(Ease.OutBack)
        );
        seq.Join(
            cg.DOFade(1f, menuButtonPopDuration * 0.55f)
              .SetEase(Ease.Linear)
        );
        seq.Append(
            target.DOScale(originalScale, menuButtonPopDuration * 0.35f)
                  .SetEase(Ease.OutQuad)
        );

        yield return seq.WaitForCompletion();

        cg.interactable = true;
        cg.blocksRaycasts = true;
        target.localScale = originalScale;
        cg.alpha = 1f;
    }

    void StopPauseMenuButtonsAnimation()
    {
        if (pauseMenuButtonsRoutine != null)
        {
            StopCoroutine(pauseMenuButtonsRoutine);
            pauseMenuButtonsRoutine = null;
        }

        ResetPauseMenuButton(settingOpenButton);
        ResetPauseMenuButton(bookOpenButton);
        ResetPauseMenuButton(homeOpenButton);
    }

    void ResetPauseMenuButton(RectTransform target)
    {
        if (target == null) return;

        CanvasGroup cg = GetOrAddCanvasGroup(target);

        if (!originalScales.TryGetValue(target, out Vector3 originalScale))
            originalScale = target.localScale;

        target.DOKill();
        cg.DOKill();

        target.localScale = originalScale;
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    CanvasGroup GetOrAddCanvasGroup(RectTransform target)
    {
        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = target.gameObject.AddComponent<CanvasGroup>();

        return cg;
    }

    public void OpenSettingWithDelay()
    {
        PlayButtonJelly(settingOpenButton);
        StartOpen(settingCanvasGroup, settingPanel);
    }

    public void CloseSettingWithDelay()
    {
        PlayButtonJelly(settingCloseButton);
        StartClose(settingCanvasGroup, settingPanel);
    }

    public void OpenBookWithDelay()
    {
        PlayButtonJelly(bookOpenButton);
        StartOpen(bookCanvasGroup, bookPanel);
    }

    public void CloseBookWithDelay()
    {
        PlayButtonJelly(bookCloseButton);
        StartClose(bookCanvasGroup, bookPanel);
    }

    public void OpenHomeWithDelay()
    {
        PlayButtonJelly(homeOpenButton);
        StartOpen(homeCanvasGroup, homePanel);
    }

    public void CloseHomeWithDelay()
    {
        PlayButtonJelly(homeCloseButton);
        StartClose(homeCanvasGroup, homePanel);
    }

    void StartOpen(CanvasGroup cg, RectTransform panel)
    {
        if (cg == null || panel == null) return;

        DOVirtual.DelayedCall(buttonAnimDelay, () =>
        {
            cg.DOKill();
            panel.DOKill();

            cg.transform.SetAsLastSibling();

            cg.gameObject.SetActive(true);
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            panel.localScale = Vector3.one * panelStartScale;

            cg.DOFade(1f, panelFadeDuration).SetUpdate(true);
            panel.DOScale(Vector3.one, panelFadeDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                });

        }).SetUpdate(true);
    }

    void StartClose(CanvasGroup cg, RectTransform panel)
    {
        if (cg == null || panel == null) return;

        DOVirtual.DelayedCall(buttonAnimDelay, () =>
        {
            cg.DOKill();
            panel.DOKill();

            cg.interactable = false;
            cg.blocksRaycasts = false;

            cg.DOFade(0f, panelCloseDuration).SetUpdate(true);
            panel.DOScale(Vector3.one * panelStartScale, panelCloseDuration)
                .SetEase(Ease.InCubic)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    cg.gameObject.SetActive(false);
                });

        }).SetUpdate(true);
    }
}