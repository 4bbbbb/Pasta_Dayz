using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;
using static Topping;

public class Plate : MonoBehaviour, IInteractable
{
    [Header("<<기본 그릇 스프라이트>>")]
    [SerializeField] private Sprite original501Sprite;
    [SerializeField] private Sprite selected501Sprite;

    [Header("<<오븐 그릇 스프라이트>>")]
    [SerializeField] private Sprite original502Sprite;
    [SerializeField] private Sprite selected502Sprite;

    [Header("<<선택 연출>>")]
    [SerializeField] private float selectScaleDuration = 0.12f;
    [SerializeField] private float selectedScaleMultiplier = 1.08f;
    [SerializeField] private float pressedScaleMultiplier = 0.97f;

    private SpriteRenderer sr;
    private Vector3 originalScale;
    private bool isAnimating = false;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    public PlateType plateType;
    public enum PlateType
    {
        BasicPlate,
        OvenPlate,
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        isSelected = false;

        if (plateType == PlateType.BasicPlate)
        {
            sr.sprite = original501Sprite;
        }
        else if (plateType == PlateType.OvenPlate)
        {
            sr.sprite = original502Sprite;
        }
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
            if (plateType == PlateType.BasicPlate && selected501Sprite != null)
                sr.sprite = selected501Sprite;
            else if (plateType == PlateType.OvenPlate && selected502Sprite != null)
                sr.sprite = selected502Sprite;
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
            if (plateType == PlateType.BasicPlate && original501Sprite != null)
                sr.sprite = original501Sprite;
            else if (plateType == PlateType.OvenPlate && original502Sprite != null)
                sr.sprite = original502Sprite;
        });
        seq.Append(transform.DOScale(originalScale, selectScaleDuration)
            .SetEase(Ease.OutQuad));
        seq.OnComplete(() => isAnimating = false);
    }
}