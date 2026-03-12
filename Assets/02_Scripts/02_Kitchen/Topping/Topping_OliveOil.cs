using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Topping_OliveOil : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite selectedSprite;

    private SpriteRenderer sr;
    private bool isAnimating = false;

    private Vector3 originalPos;
    private Vector3 originalScale;
    public bool isSelected { get; private set; }
    public bool isOliveOil = true;
    public bool CanBeSelected => true;



    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalPos = transform.localPosition;
        originalScale = transform.localScale;

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
        seq.Append(transform.DOScale(originalScale * 0.98f, 0.08f));
        seq.Join(transform.DOLocalMove(originalPos + new Vector3(0f, 0.12f, 0f), 0.08f));
        seq.AppendCallback(() =>
        {
            sr.sprite = selectedSprite;
        });
        seq.Append(transform.DOScale(originalScale, 0.12f));
        seq.Join(transform.DOLocalMove(originalPos, 0.12f));
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
        seq.Append(transform.DOScale(originalScale * 0.98f, 0.08f));
        seq.Join(transform.DOLocalMove(originalPos + new Vector3(0f, 0.06f, 0f), 0.08f));
        seq.AppendCallback(() =>
        {
            sr.sprite = originalSprite;
        });
        seq.Append(transform.DOScale(originalScale, 0.12f));
        seq.Join(transform.DOLocalMove(originalPos, 0.12f));
        seq.OnComplete(() => isAnimating = false);
    }
   
}
