using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Topping_OliveOil : MonoBehaviour, IInteractable
{
    [Header("<<올리브오일 스프라이트>>")]
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

    public void PlayPourToPanAnimation(Vector3 targetPos)
    {
        if (isAnimating) return;
        isAnimating = true;

        Transform tr = transform;

        Vector3 startPos = tr.position;
        Quaternion startRot = tr.rotation;

        transform.DOKill();

        Sequence seq = DOTween.Sequence();

        // 1. 팬 위치로 이동
        seq.Append(tr.DOMove(targetPos, 0.4f).SetEase(Ease.OutQuad));

        // 2. 기울이면서 스프라이트 변경
        seq.Append(tr.DORotate(new Vector3(0, 0, 25f), 0.25f)
            .SetEase(Ease.OutQuad));

        seq.AppendCallback(() =>
        {
            SetPressedSprite(); // 여기로 이동
        });

        // 3. 살짝 유지 (붓는 느낌)
        seq.AppendInterval(0.15f);

        // 4. 다시 원래 각도
        seq.Append(tr.DORotate(Vector3.zero, 0.25f).SetEase(Ease.InQuad));

        // 5. 원래 위치 복귀
        seq.Append(tr.DOMove(startPos, 0.4f).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            SetNormalSprite();
            tr.rotation = startRot;

            isAnimating = false;
        });
    }

    void SetPressedSprite()
    {
        if (sr != null && selectedSprite != null)
            sr.sprite = selectedSprite;
    }

    void SetNormalSprite()
    {
        if (sr != null && originalSprite != null)
            sr.sprite = originalSprite;
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
