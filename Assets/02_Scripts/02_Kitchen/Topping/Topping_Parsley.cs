using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Topping_Parsley : MonoBehaviour, IInteractable
{
    [Header("<<파슬리 스프라이트>>")]
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite selectedSprite;

    [Header("<<선택 연출>>")]
    [SerializeField] private float selectScaleDuration = 0.12f;
    [SerializeField] private float selectedScaleMultiplier = 1.08f;
    [SerializeField] private float pressedScaleMultiplier = 0.97f;

    private SpriteRenderer sr;
    private Vector3 originalScale;
    private bool isAnimating = false;

    public bool isSelected { get; private set; }
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

    public void Sprinkle(Transform pastaPoint, System.Action onSprinkle)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 targetPos = pastaPoint.position + new Vector3(2f, 2f, 0);
        Quaternion pourRot = Quaternion.Euler(0, 0, 120f);

        isAnimating = true;
        transform.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            if (sr != null && selectedSprite != null)
                sr.sprite = selectedSprite;
        });

        // 파스타 쪽으로 이동
        seq.Append(transform.DOMove(targetPos, 0.35f).SetEase(Ease.OutQuad));

        // 기울기
        seq.Append(transform.DORotateQuaternion(pourRot, 0.2f));

        // 흔들기 + 파슬리 생성
        seq.AppendCallback(() =>
        {
            onSprinkle?.Invoke();
        });

        seq.Append(
            transform.DOMoveY(targetPos.y + 0.08f, 0.18f)
                .SetLoops(4, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
        );

        // 다시 세우기
        seq.Append(transform.DORotateQuaternion(startRot, 0.2f));

        // 원위치 복귀
        seq.Append(transform.DOMove(startPos, 0.35f).SetEase(Ease.InQuad));

        seq.AppendCallback(() =>
        {
            if (sr != null && originalSprite != null)
                sr.sprite = originalSprite;

            transform.localScale = originalScale;
            isSelected = false;
            isAnimating = false;
        });
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