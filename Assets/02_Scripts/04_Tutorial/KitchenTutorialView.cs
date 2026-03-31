using System.Collections;
using TMPro;
using UnityEngine;

public class KitchenTutorialView : MonoBehaviour
{
    public enum KitchenHighlight
    {
        None,
        PastaCooker,
        GasStove,
        PlateTable,
        PassTable,
        Parmesan,
        Parsley
    }

    [Header("공통")]
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject messagePanelRoot;
    [SerializeField] private TMP_Text messageText;

    [Header("Next")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private CanvasGroup nextButtonCanvasGroup;

    [Header("포인트")]
    [SerializeField] private GameObject pastaCookerPoint;
    [SerializeField] private GameObject gasStovePoint;
    [SerializeField] private GameObject plateTablePoint;
    [SerializeField] private GameObject passTablePoint;
    [SerializeField] private GameObject parmesanPoint;
    [SerializeField] private GameObject parsleyPoint;

    [Header("타이핑")]
    [SerializeField] private float typeInterval = 0.04f;
    [SerializeField] private float nextFadeDuration = 0.2f;
    [SerializeField] private AudioClip typingSFX;

    private Coroutine typingCoroutine;
    private Coroutine nextFadeCoroutine;

    private void Awake()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.RegisterKitchenView(this);

        HideNextImmediate();
    }

    private void OnDestroy()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.UnregisterKitchenView(this);
    }

    private void OnDisable()
    {
        StopAllCoroutinesSafe();
    }

    public void ResetView()
    {
        StopAllCoroutinesSafe();

        if (root != null)
            root.SetActive(true);

        if (messagePanelRoot != null)
            messagePanelRoot.SetActive(true);

        HideAllHighlights();

        if (messageText != null)
        {
            messageText.text = string.Empty;
            messageText.maxVisibleCharacters = 99999;
        }

        HideNextImmediate();
    }

    public void HideAll()
    {
        StopAllCoroutinesSafe();

        if (root != null)
            root.SetActive(false);

        HideAllHighlights();
        HideNextImmediate();
    }

    public void ShowStep(string msg, KitchenHighlight highlight, bool showNextAfterTyping, bool showMessagePanel = true)
    {
        ResetView();

        if (messagePanelRoot != null)
            messagePanelRoot.SetActive(showMessagePanel);

        ShowHighlight(highlight);

        if (showMessagePanel)
            PlayTyping(msg, showNextAfterTyping);
        else
            HideNextImmediate();
    }

    public void OnClickNextButton()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.OnClickKitchenNext();
    }

    public void HideMessagePanelOnly()
    {
        if (messagePanelRoot != null)
            messagePanelRoot.SetActive(false);

        if (nextButton != null)
            nextButton.SetActive(false);

        if (nextButtonCanvasGroup != null)
        {
            nextButtonCanvasGroup.alpha = 0f;
            nextButtonCanvasGroup.interactable = false;
            nextButtonCanvasGroup.blocksRaycasts = false;
        }
    }

    private void ShowHighlight(KitchenHighlight highlight)
    {
        HideAllHighlights();

        switch (highlight)
        {
            case KitchenHighlight.PastaCooker:
                if (pastaCookerPoint != null) pastaCookerPoint.SetActive(true);
                break;
            case KitchenHighlight.GasStove:
                if (gasStovePoint != null) gasStovePoint.SetActive(true);
                break;
            case KitchenHighlight.PlateTable:
                if (plateTablePoint != null) plateTablePoint.SetActive(true);
                break;
            case KitchenHighlight.PassTable:
                if (passTablePoint != null) passTablePoint.SetActive(true);
                break;
            case KitchenHighlight.Parmesan:
                if (parmesanPoint != null) parmesanPoint.SetActive(true);
                break;
            case KitchenHighlight.Parsley:
                if (parsleyPoint != null) parsleyPoint.SetActive(true);
                break;
        }
    }

    private void HideAllHighlights()
    {
        if (pastaCookerPoint != null) pastaCookerPoint.SetActive(false);
        if (gasStovePoint != null) gasStovePoint.SetActive(false);
        if (plateTablePoint != null) plateTablePoint.SetActive(false);
        if (passTablePoint != null) passTablePoint.SetActive(false);
        if (parmesanPoint != null) parmesanPoint.SetActive(false);
        if (parsleyPoint != null) parsleyPoint.SetActive(false);
    }

    private void PlayTyping(string msg, bool showNextAfterTyping)
    {
        StopAllCoroutinesSafe();
        HideNextImmediate();

        if (messageText == null)
            return;

        messageText.text = msg;
        messageText.maxVisibleCharacters = 0;
        messageText.ForceMeshUpdate();

        typingCoroutine = StartCoroutine(TypeRoutine(showNextAfterTyping));
    }

    private IEnumerator TypeRoutine(bool showNextAfterTyping)
    {
        if (messageText == null)
            yield break;

        messageText.ForceMeshUpdate();
        int total = messageText.textInfo.characterCount;

        for (int i = 1; i <= total; i++)
        {
            messageText.maxVisibleCharacters = i;

            char c = messageText.textInfo.characterInfo[i - 1].character;
            if (!char.IsWhiteSpace(c) && SoundManager.Instance != null && typingSFX != null)
                SoundManager.Instance.PlaySFX(typingSFX);

            yield return new WaitForSecondsRealtime(typeInterval);
        }

        messageText.maxVisibleCharacters = total;
        typingCoroutine = null;

        if (showNextAfterTyping)
            nextFadeCoroutine = StartCoroutine(FadeNextRoutine());
    }

    private IEnumerator FadeNextRoutine()
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

        float t = 0f;
        while (t < nextFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            nextButtonCanvasGroup.alpha = Mathf.Clamp01(t / nextFadeDuration);
            yield return null;
        }

        nextButtonCanvasGroup.alpha = 1f;
        nextButtonCanvasGroup.interactable = true;
        nextButtonCanvasGroup.blocksRaycasts = true;
        nextFadeCoroutine = null;
    }

    private void HideNextImmediate()
    {
        if (nextButton != null)
            nextButton.SetActive(false);

        if (nextButtonCanvasGroup != null)
        {
            nextButtonCanvasGroup.alpha = 0f;
            nextButtonCanvasGroup.interactable = false;
            nextButtonCanvasGroup.blocksRaycasts = false;
        }
    }

    private void StopAllCoroutinesSafe()
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
}
