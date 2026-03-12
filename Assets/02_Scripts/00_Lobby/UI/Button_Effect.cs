using DG.Tweening;
using UnityEngine;

public class Button_Effect : MonoBehaviour
{
    [SerializeField] private RectTransform target;

    [Header("´­¸² °­µµ")]
    [SerializeField] private float pressX = 0.92f;
    [SerializeField] private float pressY = 0.88f;

    [Header("Æ¨±è °­µµ")]
    [SerializeField] private float bounceX = 1.08f;
    [SerializeField] private float bounceY = 1.12f;

    [Header("¸¶¹«¸® Á©¸®")]
    [SerializeField] private float settleX = 0.98f;
    [SerializeField] private float settleY = 1.02f;

    private Vector3 originalScale;
    private Sequence jellySeq;

    void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        originalScale = target.localScale;
    }

    public void PlayJelly()
    {
        if (target == null) return;

        if (jellySeq != null && jellySeq.IsActive())
            jellySeq.Kill();

        target.localScale = originalScale;

        jellySeq = DOTween.Sequence();

        // 1. ´­¸²
        jellySeq.Append(target.DOScale(
            new Vector3(originalScale.x * pressX, originalScale.y * pressY, originalScale.z),
            0.07f
        ).SetEase(Ease.OutQuad));

        // 2. À§·Î Æ¨±è
        jellySeq.Append(target.DOScale(
            new Vector3(originalScale.x * bounceX, originalScale.y * bounceY, originalScale.z),
            0.12f
        ).SetEase(Ease.OutBack));

        // 3. »ìÂ¦ ¹Ý´ë·Î Âî±×·¯Áü
        jellySeq.Append(target.DOScale(
            new Vector3(originalScale.x * settleX, originalScale.y * settleY, originalScale.z),
            0.08f
        ).SetEase(Ease.InOutSine));

        // 4. ¿ø·¡ Å©±â·Î º¹±Í
        jellySeq.Append(target.DOScale(originalScale, 0.08f).SetEase(Ease.OutSine));
    }
}