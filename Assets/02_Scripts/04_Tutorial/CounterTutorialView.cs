using System.Collections;
using TMPro;
using UnityEngine;

public class CounterTutorialView : MonoBehaviour
{
    [Header("공통")]
    [SerializeField] private GameObject TutorialPanel;
    [SerializeField] private TMP_Text messageText;

    [Header("Next 버튼")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private CanvasGroup nextButtonCanvasGroup;

    [Header("입력 차단")]
    [SerializeField] private GameObject inputBlocker;

    [Header("루트")]
    [SerializeField] private GameObject satisfactionRoot;
    [SerializeField] private GameObject pausePanelPreview;
    [SerializeField] private GameObject settingPanelPreview;
    [SerializeField] private GameObject bookPanelPreview;
    [SerializeField] private GameObject homePanelPreview;

    [Header("포인트")]
    [SerializeField] private GameObject pointDay;
    [SerializeField] private GameObject pointSatisfaction;
    [SerializeField] private GameObject pointPause;
    [SerializeField] private GameObject pointSetting;
    [SerializeField] private GameObject pointBook;
    [SerializeField] private GameObject pointHome;

    [Header("타자 효과")]
    [SerializeField] private float typeInterval = 0.04f;
    [SerializeField] private float nextButtonFadeDuration = 0.25f;

    [Header("미리보기 패널")]
    [SerializeField] private float previewHoldDuration = 1.0f;
    [SerializeField] private float previewFadeDuration = 0.25f;

    [Header("사운드")]
    [SerializeField] private AudioClip typingSFX;

    private Coroutine typingCoroutine;
    private Coroutine nextFadeCoroutine;
    private Coroutine previewCoroutine;

    private bool keepPausePanelVisible = false;

    private void Awake()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.RegisterCounterView(this);

        HideNextButtonImmediate();
        SetInputBlocker(false);
        ForceHideAllPreviewPanels();
    }

    private void OnDestroy()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.UnregisterCounterView(this);
    }

    private void OnDisable()
    {
        StopAllRunningEffects();
    }

    public void ResetView()
    {
        StopAllRunningEffects();
        HideAllIndicators();

        if (TutorialPanel != null)
            TutorialPanel.SetActive(true);

        if (messageText != null)
        {
            messageText.text = string.Empty;
            messageText.maxVisibleCharacters = 99999;
        }

        HideNextButtonImmediate();
        SetInputBlocker(true);  
    }

    public void HideAll()
    {
        StopAllRunningEffects();
        keepPausePanelVisible = false;

        if (TutorialPanel != null)
            TutorialPanel.SetActive(false);

        HideAllIndicators();
        HideNextButtonImmediate();
        SetInputBlocker(false);
        ForceHideAllPreviewPanels();
    }

    public void HideAllIndicators()
    {
        if (satisfactionRoot != null) satisfactionRoot.SetActive(false);

        if (pointDay != null) pointDay.SetActive(false);
        if (pointSatisfaction != null) pointSatisfaction.SetActive(false);
        if (pointPause != null) pointPause.SetActive(false);
        if (pointSetting != null) pointSetting.SetActive(false);
        if (pointBook != null) pointBook.SetActive(false);
        if (pointHome != null) pointHome.SetActive(false);

        HideTransientPreviewPanelsImmediate();

        if (keepPausePanelVisible)
            ShowPausePanelImmediate();
        else
            HidePreviewImmediate(pausePanelPreview, false);

        SetInputBlocker(false);
    }

    public void ShowMessage(string msg, bool showNext)
    {
        ResetView();

        if (TutorialPanel != null)
            TutorialPanel.SetActive(true);

        PlayTyping(msg, showNext);
    }

    public void ShowDayInfo(string msg)
    {
        ResetView();

        if (pointDay != null)
            pointDay.SetActive(true);

        PlayTyping(msg, true);
    }

    public void ShowSatisfactionInfo(string msg)
    {
        ResetView();

        if (satisfactionRoot != null)
            satisfactionRoot.SetActive(true);

        if (pointSatisfaction != null)
            pointSatisfaction.SetActive(true);

        PlayTyping(msg, true);
    }

    public void ShowPauseInfo(string msg, bool showNextAfterTyping = true)
    {
        ResetView();

        if (pointPause != null)
            pointPause.SetActive(true);

        SetInputBlocker(false);

        PlayTyping(msg, showNextAfterTyping);
    }

    public void ShowSettingInfo(string msg, bool showNextAfterTyping = true)
    {
        ResetView();

        if (pointSetting != null)
            pointSetting.SetActive(true);

        PlayTyping(msg, showNextAfterTyping);
    }

    public void ShowBookInfo(string msg, bool showNextAfterTyping = true)
    {
        ResetView();

        if (pointBook != null)
            pointBook.SetActive(true);

        PlayTyping(msg, showNextAfterTyping);
    }

    public void ShowHomeInfo(string msg, bool showNextAfterTyping = true)
    {
        ResetView();

        if (pointHome != null)
            pointHome.SetActive(true);

        PlayTyping(msg, showNextAfterTyping);
    }

    public void ShowResumeInfo(string msg, bool showNextAfterTyping = true)
    {
        ResetView();
        PlayTyping(msg, showNextAfterTyping);
    }

    public void KeepPausePanelOpenThenShowNext(string nextGuideMessage)
    {
        StopAllRunningEffects();
        HideNextButtonImmediate();
        keepPausePanelVisible = true;
        HideTransientPreviewPanelsImmediate();
        ShowPausePanelImmediate();
        SetInputBlocker(false);

        if (TutorialPanel != null)
            TutorialPanel.SetActive(true);

        if (!string.IsNullOrEmpty(nextGuideMessage))
            PlayTyping(nextGuideMessage, true);
    }

    public void PlaySettingPreviewThenShowNext(string nextGuideMessage)
    {
        StartTransientPreviewThenGuide(GetPreviewOrFallback(settingPanelPreview), nextGuideMessage);
    }

    public void PlayBookPreviewThenShowNext(string nextGuideMessage)
    {
        StartTransientPreviewThenGuide(GetPreviewOrFallback(bookPanelPreview), nextGuideMessage);
    }

    public void PlayHomePreviewThenShowNext(string nextGuideMessage)
    {
        StartTransientPreviewThenGuide(GetPreviewOrFallback(homePanelPreview), nextGuideMessage);
    }

    public void ClosePersistentPausePanel()
    {
        keepPausePanelVisible = false;
        HidePreviewImmediate(pausePanelPreview, false);
        HideTransientPreviewPanelsImmediate();
        SetInputBlocker(false);
    }

    public void OnClickNextButton()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.OnClickNext();
    }

    public void OnClickPausePractice()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.OnTutorialPausePressed();
    }

    public void OnClickSettingPractice()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.OnTutorialSettingPressed();
    }

    public void OnClickBookPractice()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.OnTutorialBookPressed();
    }

    public void OnClickHomePractice()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.OnTutorialHomePressed();
    }

    public void OnClickResumePractice()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.OnTutorialResumePressed();
    }

    private void StartTransientPreviewThenGuide(GameObject previewObject, string nextGuideMessage)
    {
        StopAllRunningEffects();
        HideNextButtonImmediate();
        SetInputBlocker(true);

        if (TutorialPanel != null)
            TutorialPanel.SetActive(true);

        if (keepPausePanelVisible)
            ShowPausePanelImmediate();

        previewCoroutine = StartCoroutine(TransientPreviewThenGuideRoutine(previewObject, nextGuideMessage));
    }

    private IEnumerator TransientPreviewThenGuideRoutine(GameObject previewObject, string nextGuideMessage)
    {
        HideTransientPreviewPanelsImmediate();

        CanvasGroup previewCanvasGroup = null;

        if (previewObject != null)
        {
            previewObject.SetActive(true);
            previewCanvasGroup = EnsureCanvasGroup(previewObject);
            previewCanvasGroup.alpha = 1f;
            previewCanvasGroup.interactable = false;
            previewCanvasGroup.blocksRaycasts = false;
        }

        yield return new WaitForSecondsRealtime(previewHoldDuration);

        if (previewObject != null && previewCanvasGroup != null)
        {
            float t = 0f;

            while (t < previewFadeDuration)
            {
                t += Time.unscaledDeltaTime;
                previewCanvasGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(t / previewFadeDuration));
                yield return null;
            }

            previewCanvasGroup.alpha = 0f;
            previewObject.SetActive(false);
        }

        SetInputBlocker(false);
        previewCoroutine = null;

        if (!string.IsNullOrEmpty(nextGuideMessage))
            PlayTyping(nextGuideMessage, true);
    }

    private void PlayTyping(string msg, bool showNextAfterTyping)
    {
        StopTypingAndNextFadeOnly();
        HideNextButtonImmediate();

        if (messageText == null)
            return;

        messageText.text = msg;
        messageText.maxVisibleCharacters = 0;
        messageText.ForceMeshUpdate();

        typingCoroutine = StartCoroutine(TypeTextRoutine(showNextAfterTyping));
    }

    private IEnumerator TypeTextRoutine(bool showNextAfterTyping)
    {
        if (messageText == null)
            yield break;

        messageText.ForceMeshUpdate();
        int totalCharacters = messageText.textInfo.characterCount;

        messageText.maxVisibleCharacters = 0;

        for (int i = 1; i <= totalCharacters; i++)
        {
            messageText.maxVisibleCharacters = i;

            char currentChar = messageText.textInfo.characterInfo[i - 1].character;
            if (!char.IsWhiteSpace(currentChar))
                PlayTypingSound();

            yield return new WaitForSecondsRealtime(typeInterval);
        }

        messageText.maxVisibleCharacters = totalCharacters;
        typingCoroutine = null;

        if (showNextAfterTyping)
            nextFadeCoroutine = StartCoroutine(FadeInNextButtonRoutine());
    }

    private IEnumerator FadeInNextButtonRoutine()
    {
        if (nextButton == null)
            yield break;

        nextButton.SetActive(true);

        if (nextButtonCanvasGroup == null)
        {
            nextFadeCoroutine = null;
            yield break;
        }

        nextButtonCanvasGroup.alpha = 0f;
        nextButtonCanvasGroup.interactable = false;
        nextButtonCanvasGroup.blocksRaycasts = false;

        float time = 0f;

        while (time < nextButtonFadeDuration)
        {
            time += Time.unscaledDeltaTime;
            nextButtonCanvasGroup.alpha = Mathf.Clamp01(time / nextButtonFadeDuration);
            yield return null;
        }

        nextButtonCanvasGroup.alpha = 1f;
        nextButtonCanvasGroup.interactable = true;
        nextButtonCanvasGroup.blocksRaycasts = true;
        nextFadeCoroutine = null;
    }

    private void HideNextButtonImmediate()
    {
        if (nextButton == null)
            return;

        nextButton.SetActive(false);

        if (nextButtonCanvasGroup != null)
        {
            nextButtonCanvasGroup.alpha = 0f;
            nextButtonCanvasGroup.interactable = false;
            nextButtonCanvasGroup.blocksRaycasts = false;
        }
    }

    private void StopTypingAndNextFadeOnly()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (nextFadeCoroutine != null)
        {
            StopCoroutine(nextFadeCoroutine);
            nextFadeCoroutine = null;
        }
    }

    private void StopAllRunningEffects()
    {
        StopTypingAndNextFadeOnly();

        if (previewCoroutine != null)
        {
            StopCoroutine(previewCoroutine);
            previewCoroutine = null;
        }
    }

    public void SetInputBlocker(bool isActive)
    {
        if (inputBlocker != null)
            inputBlocker.SetActive(isActive);
    }

    private void ForceHideAllPreviewPanels()
    {
        HidePreviewImmediate(pausePanelPreview, false);
        HidePreviewImmediate(settingPanelPreview, false);
        HidePreviewImmediate(bookPanelPreview, false);
        HidePreviewImmediate(homePanelPreview, false);
    }

    private void HideTransientPreviewPanelsImmediate()
    {
        HidePreviewImmediate(settingPanelPreview, false);
        HidePreviewImmediate(bookPanelPreview, false);
        HidePreviewImmediate(homePanelPreview, false);
    }

    private void ShowPausePanelImmediate()
    {
        if (pausePanelPreview == null)
            return;

        pausePanelPreview.SetActive(true);
        CanvasGroup canvasGroup = EnsureCanvasGroup(pausePanelPreview);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HidePreviewImmediate(GameObject previewObject, bool makeInteractable)
    {
        if (previewObject == null)
            return;

        CanvasGroup canvasGroup = EnsureCanvasGroup(previewObject);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = makeInteractable;
        canvasGroup.blocksRaycasts = makeInteractable;
        previewObject.SetActive(false);
    }

    private CanvasGroup EnsureCanvasGroup(GameObject target)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = target.AddComponent<CanvasGroup>();

        return canvasGroup;
    }

    private GameObject GetPreviewOrFallback(GameObject primaryPreview)
    {
        return primaryPreview != null ? primaryPreview : pausePanelPreview;
    }

    private void PlayTypingSound()
    {
        if (SoundManager.Instance != null && typingSFX != null)
            SoundManager.Instance.PlaySFX(typingSFX);
    }
}
