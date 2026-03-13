using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

public class Lobby_UI_Manager : MonoBehaviour
{
    [Header("열기 버튼")]
    [SerializeField] private RectTransform settingOpenButton;
    [SerializeField] private RectTransform shopOpenButton;
    [SerializeField] private RectTransform profileOpenButton;

    [Header("닫기 버튼")]
    [SerializeField] private RectTransform settingCloseButton;
    [SerializeField] private RectTransform shopCloseButton;
    [SerializeField] private RectTransform profileCloseButton;

    [Header("버튼 젤리")]
    [SerializeField] private float pressX = 0.96f;
    [SerializeField] private float pressY = 0.94f;

    [Header("패널 등장/퇴장")]
    [SerializeField] private float buttonAnimDelay = 0.3f;
    [SerializeField] private float panelStartScale = 0.98f;
    [SerializeField] private float panelFadeDuration = 0.4f;
    [SerializeField] private float panelCloseDuration = 0.3f;

    [Header("Setting")]
    [SerializeField] private CanvasGroup settingCanvasGroup;
    [SerializeField] private RectTransform settingPanel;

    [Header("Shop")]
    [SerializeField] private CanvasGroup shopCanvasGroup;
    [SerializeField] private RectTransform shopPanel;

    [Header("Profile")]
    [SerializeField] private CanvasGroup profileCanvasGroup;
    [SerializeField] private RectTransform profilePanel;

    private Dictionary<RectTransform, Vector3> originalScales = new Dictionary<RectTransform, Vector3>();

    void Awake()
    {
        CacheButtonScale(settingOpenButton);
        CacheButtonScale(shopOpenButton);
        CacheButtonScale(profileOpenButton);

        CacheButtonScale(settingCloseButton);
        CacheButtonScale(shopCloseButton);
        CacheButtonScale(profileCloseButton);

        InitPanel(settingCanvasGroup, settingPanel);
        InitPanel(shopCanvasGroup, shopPanel);
        InitPanel(profileCanvasGroup, profilePanel);
    }
    void CacheButtonScale(RectTransform button)
    {
        if (button == null) return;

        if (!originalScales.ContainsKey(button))
            originalScales.Add(button, button.localScale);
    }

    void InitPanel(CanvasGroup cg, RectTransform panel)
    {
        if (cg == null || panel == null)
        {
            return;
        }

        cg.gameObject.SetActive(false);
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        panel.localScale = Vector3.one * panelStartScale;
    }

    void PlayButtonJelly(RectTransform target)
    {
        if (target == null)
        {
            return;
        }

        if (!originalScales.TryGetValue(target, out Vector3 originalScale))
            originalScale = target.localScale;

        target.DOKill();
        target.localScale = originalScale;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            target.DOScale(
                new Vector3(originalScale.x * pressX, originalScale.y * pressY, originalScale.z),
                0.1f
            ).SetEase(Ease.OutCubic)
        );

        seq.Append(
            target.DOScale(originalScale, 0.14f).SetEase(Ease.OutQuad)
        );
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

    public void OpenShopWithDelay()
    {
        PlayButtonJelly(shopOpenButton);
        StartOpen(shopCanvasGroup, shopPanel);
    }

    public void CloseShopWithDelay()
    {
        PlayButtonJelly(shopCloseButton);
        StartClose(shopCanvasGroup, shopPanel);
    }

    public void OpenProfileWithDelay()
    {
        PlayButtonJelly(profileOpenButton);
        StartOpen(profileCanvasGroup, profilePanel);
    }

    public void CloseProfileWithDelay()
    {
        PlayButtonJelly(profileCloseButton);
        StartClose(profileCanvasGroup, profilePanel);
    }

    void StartOpen(CanvasGroup cg, RectTransform panel)
    {
        if (cg == null || panel == null)
        {
            return;
        }

        DOVirtual.DelayedCall(buttonAnimDelay, () =>
        {
            cg.DOKill();
            panel.DOKill();

            cg.gameObject.SetActive(true);
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            panel.localScale = Vector3.one * panelStartScale;

            cg.DOFade(1f, panelFadeDuration);
            panel.DOScale(Vector3.one, panelFadeDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                });
        });
    }

    void StartClose(CanvasGroup cg, RectTransform panel)
    {
        if (cg == null || panel == null)
        {
            return;
        }

        DOVirtual.DelayedCall(buttonAnimDelay, () =>
        {
            cg.DOKill();
            panel.DOKill();

            cg.interactable = false;
            cg.blocksRaycasts = false;

            cg.DOFade(0f, panelCloseDuration);
            panel.DOScale(Vector3.one * panelStartScale, panelCloseDuration)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    cg.gameObject.SetActive(false);
                });
        });
    }
}