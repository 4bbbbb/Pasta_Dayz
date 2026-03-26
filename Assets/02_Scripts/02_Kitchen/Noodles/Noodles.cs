using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Noodles : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    private Vector3 originalScale;
    private bool isAnimating = false;

    public bool isSelected { get; private set; }

    [Header("스프라이트")]
    public Sprite originalSprite;
    public Sprite selectedSprite;

    [Header("선택 연출")]
    [SerializeField] private float selectScaleDuration = 0.12f;
    [SerializeField] private float selectedScaleMultiplier = 1.08f;
    [SerializeField] private float pressedScaleMultiplier = 0.97f;

    public bool CanBeSelected => true;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        if (sr != null && originalSprite != null)
            sr.sprite = originalSprite;

        isSelected = false;
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Select();
            return true;
        }

        return false;
    }

    void Select()
    {
        if (isAnimating) return;
        if (isSelected) return;

        isAnimating = true;
        isSelected = true;

        transform.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * pressedScaleMultiplier, 0.08f));
        seq.AppendCallback(() =>
        {
            if (sr != null && selectedSprite != null)
                sr.sprite = selectedSprite;
        });
        seq.Append(transform.DOScale(originalScale * selectedScaleMultiplier, selectScaleDuration)
            .SetEase(Ease.OutBack));
        seq.OnComplete(() => isAnimating = false);
    }

    public void Cancel()
    {
        if (isAnimating) return;
        if (!isSelected) return;

        isAnimating = true;
        isSelected = false;

        transform.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * pressedScaleMultiplier, 0.08f));
        seq.AppendCallback(() =>
        {
            if (sr != null && originalSprite != null)
                sr.sprite = originalSprite;
        });
        seq.Append(transform.DOScale(originalScale, selectScaleDuration)
            .SetEase(Ease.OutQuad));
        seq.OnComplete(() => isAnimating = false);
    }
}