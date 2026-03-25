using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class EndDay_UI_Manager : MonoBehaviour
{
    [Header("영수증")]
    [SerializeField] private RectTransform receipt;

    [Header("타이틀")]
    [SerializeField] private CanvasGroup titleCG;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("왼쪽 텍스트")]
    [SerializeField] private TextMeshProUGUI revenueLabel;
    [SerializeField] private TextMeshProUGUI tipLabel;
    [SerializeField] private TextMeshProUGUI refundLabel;
    [SerializeField] private TextMeshProUGUI costLabel;

    [Header("오른쪽 값")]
    [SerializeField] private TextMeshProUGUI revenueValue;
    [SerializeField] private TextMeshProUGUI tipValue;
    [SerializeField] private TextMeshProUGUI refundValue;
    [SerializeField] private TextMeshProUGUI costValue;

    [Header("순수익")]
    [SerializeField] private TextMeshProUGUI netLabel;
    [SerializeField] private TextMeshProUGUI netValue;
    [SerializeField] private CanvasGroup netCG;
    private readonly Color finalNetColor = new Color32(0x72, 0x44, 0x37, 0xFF);

    [Header("버튼")]
    [SerializeField] private RectTransform nextDayButton;
    [SerializeField] private RectTransform homeButton;

    [Header("타자")]
    [SerializeField] private float typingSpeed = 0.03f;
    [SerializeField] private float lineDelay = 0.15f;

    [Header("버튼 클릭 젤리")]
    [SerializeField] private float pressX = 0.96f;
    [SerializeField] private float pressY = 0.94f;
    [SerializeField] private float sceneChangeDelay = 0.24f;

    [Header("버튼 등장")]
    [SerializeField] private float buttonStartScale = 0.65f;
    [SerializeField] private float buttonOvershootScale = 1.12f;
    [SerializeField] private float buttonPopDuration = 0.22f;
    [SerializeField] private float buttonPopInterval = 0.12f;

    [Header("사운드")]
    [SerializeField] private AudioClip typingSFX;
    [SerializeField] private AudioClip profitSFX;
    [SerializeField] private AudioClip lossSFX;
    [SerializeField] private AudioClip buttonSFX;

    private readonly Dictionary<RectTransform, Vector3> originalScales = new Dictionary<RectTransform, Vector3>();
    private Vector2 netValueOriginalAnchoredPos;
    private bool isTransitioning = false;

    void Awake()
    {
        CacheButtonScale(nextDayButton);
        CacheButtonScale(homeButton);

        if (netValue != null)
            netValueOriginalAnchoredPos = netValue.rectTransform.anchoredPosition;
    }

    void Start()
    {
        Time.timeScale = 1f;
        InitUI();
        StartCoroutine(StartSequence());
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        KillAllTweens();
    }

    void CacheButtonScale(RectTransform button)
    {
        if (button == null) return;

        if (!originalScales.ContainsKey(button))
            originalScales.Add(button, button.localScale);
    }

    void KillAllTweens()
    {
        if (receipt != null) receipt.DOKill();
        if (titleCG != null) titleCG.DOKill();
        if (netCG != null) netCG.DOKill();

        if (nextDayButton != null)
        {
            nextDayButton.DOKill();
            CanvasGroup cg = nextDayButton.GetComponent<CanvasGroup>();
            if (cg != null) cg.DOKill();
        }

        if (homeButton != null)
        {
            homeButton.DOKill();
            CanvasGroup cg = homeButton.GetComponent<CanvasGroup>();
            if (cg != null) cg.DOKill();
        }

        if (netValue != null)
        {
            netValue.transform.DOKill();
            netValue.rectTransform.DOKill();
        }
    }

    void InitUI()
    {
        if (receipt != null)
        {
            receipt.gameObject.SetActive(true);
            receipt.pivot = new Vector2(0.5f, 1f);
            receipt.localScale = new Vector3(1f, 0.05f, 1f); // 완전 0 말고 얇게 시작
            receipt.localRotation = Quaternion.identity;
        }

        titleCG.alpha = 0;

        revenueLabel.text = "";
        tipLabel.text = "";
        refundLabel.text = "";
        costLabel.text = "";

        revenueValue.text = "";
        tipValue.text = "";
        refundValue.text = "";
        costValue.text = "";

        netLabel.text = "";
        netValue.text = "";
        netCG.alpha = 0;

        nextDayButton.gameObject.SetActive(false);
        homeButton.gameObject.SetActive(false);
    }    

    CanvasGroup GetOrAddCanvasGroup(RectTransform target)
    {
        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = target.gameObject.AddComponent<CanvasGroup>();

        return cg;
    }

  IEnumerator StartSequence()
    {
        yield return StartCoroutine(PlayReceiptAnimation());

        yield return new WaitForSecondsRealtime(0.2f);

        yield return StartCoroutine(PlayRoutine());

        yield return new WaitForSecondsRealtime(0.3f);

        yield return StartCoroutine(ShowButtons());
    }

    IEnumerator PlayReceiptAnimation()
    {
        if (receipt == null)
            yield break;

        receipt.DOKill();
        receipt.gameObject.SetActive(true);
        receipt.localRotation = Quaternion.identity;
        receipt.localScale = new Vector3(1f, 0.05f, 1f);

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        seq.Append(
            receipt.DOScaleY(1.03f, 0.45f)
                   .SetEase(Ease.OutCubic)
        );

        seq.Append(
            receipt.DOScaleY(1f, 0.18f)
                   .SetEase(Ease.OutQuad)
        );

        yield return seq.WaitForCompletion();
    }

    IEnumerator PlayRoutine()
    {
        if (titleText != null && Day_Manager.Instance != null)
            titleText.text = $"{Day_Manager.Instance.day}일차 정산";

        if (titleCG != null)
            yield return titleCG.DOFade(1f, 0.5f).SetUpdate(true).WaitForCompletion();

        yield return new WaitForSecondsRealtime(0.3f);

        if (Gold_Manager.Instance != null)
        {
            yield return StartCoroutine(TypeLine(revenueLabel, "○  전체 수익 : ", revenueValue, Gold_Manager.Instance.dailyRevenue));
            yield return StartCoroutine(TypeLine(tipLabel, "○  팁 : ", tipValue, Gold_Manager.Instance.dailyTip));
            yield return StartCoroutine(TypeLine(refundLabel, "○  환불 : ", refundValue, Gold_Manager.Instance.dailyRefund, true));
            yield return StartCoroutine(TypeLine(costLabel, "○  재료비 : ", costValue, Gold_Manager.Instance.dailyCost, true));
        }

        yield return new WaitForSecondsRealtime(0.3f);

        yield return StartCoroutine(TypeSingle(netLabel, "순 수익"));
        yield return new WaitForSecondsRealtime(0.2f);

        float net = 0f;

        if (Gold_Manager.Instance != null)
            net = Gold_Manager.Instance.DailyNetProfit();

        if (netCG != null)
            netCG.alpha = 0f;

        if (netValue != null)
        {
            netValue.text = "0.0";
            netValue.transform.localScale = Vector3.one;
            netValue.rectTransform.anchoredPosition = netValueOriginalAnchoredPos;
            netValue.color = finalNetColor;
        }

        if (netCG != null)
            yield return netCG.DOFade(1f, 0.3f).SetUpdate(true).WaitForCompletion();

        yield return StartCoroutine(CountUpMoney(net));

        if (net >= 0f)
            PlayProfitEffect();
        else
            PlayLossEffect();
    }

    IEnumerator TypeLine(TextMeshProUGUI label, string labelText, TextMeshProUGUI value, float valueNum, bool showMinus = false)
    {
        if (label == null || value == null)
            yield break;

        label.text = "";
        value.text = "";

        TextAlignmentOptions originalAlignment = value.alignment;
        value.alignment = TextAlignmentOptions.Left;

        for (int i = 0; i <= labelText.Length; i++)
        {
            label.text = labelText.Substring(0, i);
            PlayTypingSound();
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        string valueText = showMinus
            ? $"-{Mathf.Abs(valueNum):F1}"
            : valueNum.ToString("F1");

        for (int i = 0; i <= valueText.Length; i++)
        {
            value.text = valueText.Substring(0, i);
            PlayTypingSound();
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        value.text = valueText;
        value.alignment = originalAlignment;

        yield return new WaitForSecondsRealtime(lineDelay);
    }

    IEnumerator TypeSingle(TextMeshProUGUI textUI, string text)
    {
        if (textUI == null)
            yield break;

        textUI.text = "";

        for (int i = 0; i <= text.Length; i++)
        {
            textUI.text = text.Substring(0, i);
            PlayTypingSound();
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }

    IEnumerator CountUpMoney(float target)
    {
        if (netValue == null)
            yield break;

        float duration = 0.6f;
        float current = 0f;
        float timer = 0f;

        Color upColor = new Color(0.2f, 0.8f, 0.3f);
        Color downColor = new Color(0.9f, 0.2f, 0.2f);

        Color runningColor = target >= 0f ? upColor : downColor;

        netValue.transform.DOKill();
        netValue.transform.localScale = Vector3.one;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);

            current = Mathf.Lerp(0f, target, t);
            netValue.text = current.ToString("F1");
            netValue.color = runningColor;

            yield return null;
        }

        netValue.text = target.ToString("F1");
        netValue.color = finalNetColor;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(netValue.transform.DOScale(1.08f, 0.12f).SetEase(Ease.OutQuad));
        seq.Append(netValue.transform.DOScale(1f, 0.10f).SetEase(Ease.InQuad));

        yield return seq.WaitForCompletion();
    }

    void PlayProfitEffect()
    {
        if (netValue == null) return;

        netValue.transform.DOKill();
        netValue.rectTransform.DOKill();
        netValue.transform.localScale = Vector3.one;
        netValue.rectTransform.anchoredPosition = netValueOriginalAnchoredPos;
        netValue.color = finalNetColor;

        if (SoundManager.Instance != null && profitSFX != null)
            SoundManager.Instance.PlaySFX(profitSFX);
    }

    void PlayLossEffect()
    {
        if (netValue == null) return;

        netValue.transform.DOKill();
        netValue.rectTransform.DOKill();
        netValue.transform.localScale = Vector3.one;
        netValue.rectTransform.anchoredPosition = netValueOriginalAnchoredPos;
        netValue.color = finalNetColor;

        if (SoundManager.Instance != null && lossSFX != null)
            SoundManager.Instance.PlaySFX(lossSFX);
    }

    void PlayTypingSound()
    {
        if (SoundManager.Instance != null && typingSFX != null)
            SoundManager.Instance.PlaySFX(typingSFX);
    }

    IEnumerator ShowButtons()
    {
        yield return StartCoroutine(PopButton(homeButton));
        yield return new WaitForSecondsRealtime(buttonPopInterval);
        yield return StartCoroutine(PopButton(nextDayButton));
    }

    IEnumerator PopButton(RectTransform btn)
    {
        if (btn == null)
            yield break;

        CanvasGroup cg = GetOrAddCanvasGroup(btn);

        if (!originalScales.TryGetValue(btn, out Vector3 originalScale))
            originalScale = btn.localScale;

        Vector3 startScale = originalScale * buttonStartScale;
        Vector3 overshootScale = originalScale * buttonOvershootScale;

        btn.DOKill();
        cg.DOKill();

        btn.gameObject.SetActive(true);
        btn.localScale = startScale;
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        if (SoundManager.Instance != null && buttonSFX != null)
            SoundManager.Instance.PlaySFX(buttonSFX);

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(
            btn.DOScale(overshootScale, buttonPopDuration * 0.65f)
               .SetEase(Ease.OutBack)
        );
        seq.Join(
            cg.DOFade(1f, buttonPopDuration * 0.55f)
              .SetEase(Ease.Linear)
        );
        seq.Append(
            btn.DOScale(originalScale, buttonPopDuration * 0.35f)
               .SetEase(Ease.OutQuad)
        );

        yield return seq.WaitForCompletion();

        btn.localScale = originalScale;
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    void PlayButtonJelly(RectTransform target)
    {
        if (target == null) return;

        if (!originalScales.TryGetValue(target, out Vector3 originalScale))
            originalScale = target.localScale;

        if (SoundManager.Instance != null && buttonSFX != null)
            SoundManager.Instance.PlaySFX(buttonSFX);

        target.DOKill();
        target.localScale = originalScale;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(
            target.DOScale(
                new Vector3(originalScale.x * pressX, originalScale.y * pressY, originalScale.z),
                0.1f
            ).SetEase(Ease.OutCubic)
        );
        seq.Append(
            target.DOScale(originalScale, 0.14f)
                  .SetEase(Ease.OutQuad)
        );
    }

    void SetButtonsInteractable(bool value)
    {
        SetButtonInteractable(nextDayButton, value);
        SetButtonInteractable(homeButton, value);
    }

    void SetButtonInteractable(RectTransform button, bool value)
    {
        if (button == null) return;

        CanvasGroup cg = GetOrAddCanvasGroup(button);
        cg.interactable = value;
        cg.blocksRaycasts = value;
    }

    public void OnClickNextDayBtn()
    {
        if (isTransitioning) return;
        StartCoroutine(HandleSceneChange(nextDayButton, 1));
    }

    public void OnClickHomeBtn()
    {
        if (isTransitioning) return;
        StartCoroutine(HandleSceneChange(homeButton, 0));
    }

    IEnumerator HandleSceneChange(RectTransform clickedButton, int sceneIndex)
    {
        isTransitioning = true;
        SetButtonsInteractable(false);

        Time.timeScale = 1f;
        PlayButtonJelly(clickedButton);

        yield return new WaitForSecondsRealtime(sceneChangeDelay);

        if (Gold_Manager.Instance != null)
            Gold_Manager.Instance.ResetDailyStats();

        if (Day_Manager.Instance != null)
            Day_Manager.Instance.ResetForNextDay();

        SceneManager.LoadScene(sceneIndex);
    }
}