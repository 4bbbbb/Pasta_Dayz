using System.Collections;
using UnityEngine;
using DG.Tweening;

public class CustomerButton : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private AudioClip clickSFX;

    [Header("딜레이")]
    [SerializeField] private float clickDelay = 0.3f;

    [Header("Auto 버튼 비용")]
    [SerializeField] private float autoButtonCost = 5f;

    [Header("경고 패널")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private float warningDuration = 1.5f;

    [Header("경고 패널 연출")]
    [SerializeField] private float warningMoveDistance = 30f;
    [SerializeField] private float warningShowDuration = 0.25f;
    [SerializeField] private float warningHideDuration = 0.2f;

    private bool isProcessing = false;
    private Coroutine warningCoroutine;

    private RectTransform warningRect;
    private CanvasGroup warningCanvasGroup;
    private Vector2 warningOriginalPos;

    private void Awake()
    {
        if (warningPanel != null)
        {
            warningRect = warningPanel.GetComponent<RectTransform>();
            warningCanvasGroup = warningPanel.GetComponent<CanvasGroup>();

            if (warningCanvasGroup == null)
                warningCanvasGroup = warningPanel.AddComponent<CanvasGroup>();

            if (warningRect != null)
                warningOriginalPos = warningRect.anchoredPosition;

            warningCanvasGroup.alpha = 0f;
            warningPanel.SetActive(false);
        }
    }

    public void OnClickYesBtn()
    {
        if (isProcessing) return;

        isProcessing = true;
        PlayClickSFX();
        StartCoroutine(OnClickYesBtnRoutine());
    }

    public void OnClickAutoButton()
    {
        if (isProcessing) return;

        if (Gold_Manager.Instance == null)
        {
            Debug.LogError("Gold_Manager Instance가 없습니다.");
            return;
        }

        if (!Gold_Manager.Instance.CanAfford(autoButtonCost))
        {
            ShowWarningPanel();
            return;
        }

        isProcessing = true;
        PlayClickSFX();
        StartCoroutine(OnClickAutoButtonRoutine());
    }

    private IEnumerator OnClickYesBtnRoutine()
    {
        yield return new WaitForSeconds(clickDelay);

        Order_Manager manager = FindObjectOfType<Order_Manager>();

        if (manager == null)
        {
            Debug.LogError("OrderManager를 찾을 수 없음!");
            isProcessing = false;
            yield break;
        }

        if (manager.dayManager != null && manager.dayManager.isTakingOrder)
        {
            manager.GetPrice();
        }
        else
        {
            Debug.Log("영업 종료! 주문 불가");
        }

        isProcessing = false;
    }

    private IEnumerator OnClickAutoButtonRoutine()
    {
        yield return new WaitForSeconds(clickDelay);

        Order_Manager manager = FindObjectOfType<Order_Manager>();

        if (manager == null)
        {
            Debug.LogError("OrderManager를 찾을 수 없음!");
            isProcessing = false;
            yield break;
        }

        if (manager.dayManager != null && manager.dayManager.isTakingOrder)
        {
            manager.OnClickAutoButton();
        }
        else
        {
            Debug.Log("영업 종료! 자동 주문 불가");
        }

        isProcessing = false;
    }

    void PlayClickSFX()
    {
        if (SoundManager.Instance != null && clickSFX != null)
        {
            SoundManager.Instance.PlaySFX(clickSFX);
        }
    }

    void ShowWarningPanel()
    {
        if (warningPanel == null)
        {
            Debug.LogWarning("warningPanel이 연결되지 않았습니다.");
            return;
        }

        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);

        if (warningRect != null)
            warningRect.DOKill();

        if (warningCanvasGroup != null)
            warningCanvasGroup.DOKill();

        warningCoroutine = StartCoroutine(ShowWarningPanelRoutine());
    }

    IEnumerator ShowWarningPanelRoutine()
    {
        warningPanel.SetActive(true);

        if (warningRect != null)
            warningRect.anchoredPosition = warningOriginalPos + Vector2.down * warningMoveDistance;

        if (warningCanvasGroup != null)
            warningCanvasGroup.alpha = 0f;

        Sequence showSeq = DOTween.Sequence();

        if (warningRect != null)
            showSeq.Join(
                warningRect.DOAnchorPos(warningOriginalPos, warningShowDuration)
                           .SetEase(Ease.OutQuad)
            );

        if (warningCanvasGroup != null)
            showSeq.Join(
                warningCanvasGroup.DOFade(1f, warningShowDuration)
            );

        yield return showSeq.WaitForCompletion();

        yield return new WaitForSeconds(warningDuration);

        Sequence hideSeq = DOTween.Sequence();

        if (warningCanvasGroup != null)
            hideSeq.Join(
                warningCanvasGroup.DOFade(0f, warningHideDuration)
            );

        yield return hideSeq.WaitForCompletion();

        warningPanel.SetActive(false);
        warningCoroutine = null;
    }
}