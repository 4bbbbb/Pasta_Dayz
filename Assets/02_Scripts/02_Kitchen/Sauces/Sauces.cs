using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Sauces : MonoBehaviour, IInteractable
{
    [Header("국자 렌더러")]
    [SerializeField] private SpriteRenderer ladleRenderer;

    [Header("국자 스프라이트")]
    [SerializeField] private Sprite emptyLadleSprite;
    [SerializeField] private Sprite scoopedLadleSprite;

    [Header("선택 연출")]
    [SerializeField] private float riseDistance = 0.8f;
    [SerializeField] private float riseDuration = 0.25f;
    [SerializeField] private float bounceUpAmount = 0.08f;
    [SerializeField] private float bounceUpDuration = 0.12f;
    [SerializeField] private float bounceDownDuration = 0.1f;
    [SerializeField] private float cancelDuration = 0.2f;

    [Header("붓기 연출")]
    [SerializeField] private float moveToPanDuration = 0.55f;
    [SerializeField] private float circleDuration = 1.45f;
    [SerializeField] private float circleRadius = 0.38f;
    [SerializeField] private float fadeOutDuration = 0.45f;
    [SerializeField] private float circleCenterYOffset = 0.08f;

    public SauceType sauceType;

    public enum SauceType
    {
        None,
        Tomato,
        Cream,
        Rose,
        Vongole,
    }

    private Vector3 ladleOriginalLocalPos;
    private Color ladleOriginalColor;
    private Sequence ladleSequence;

    private bool isAnimating = false;
    private bool isPouring = false;
    private bool isInitialized = false;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    private void Awake()
    {
        InitializeLadle();
    }

    private void InitializeLadle()
    {
        if (isInitialized) return;

        if (ladleRenderer != null)
        {
            ladleOriginalLocalPos = ladleRenderer.transform.localPosition;
            ladleOriginalColor = ladleRenderer.color;
            ResetLadleVisual();
        }

        isInitialized = true;
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

    private void Select()
    {
        InitializeLadle();

        if (ladleRenderer == null) return;
        if (isAnimating || isSelected) return;

        isSelected = true;
        PlayLadleSelectAnimation();
    }

    private void PlayLadleSelectAnimation()
    {
        if (ladleRenderer == null) return;

        KillSequence();

        isAnimating = true;

        ladleRenderer.gameObject.SetActive(true);
        ladleRenderer.transform.localPosition = ladleOriginalLocalPos + Vector3.down * riseDistance;
        ladleRenderer.sprite = emptyLadleSprite;

        Color startColor = ladleOriginalColor;
        startColor.a = 0f;
        ladleRenderer.color = startColor;

        ladleSequence = DOTween.Sequence();

        ladleSequence.Append(
            ladleRenderer.transform.DOLocalMove(ladleOriginalLocalPos, riseDuration)
                .SetEase(Ease.InOutSine)
        );

        ladleSequence.Join(
            ladleRenderer.DOFade(1f, riseDuration)
        );

        ladleSequence.AppendCallback(() =>
        {
            if (scoopedLadleSprite != null)
                ladleRenderer.sprite = scoopedLadleSprite;
        });

        ladleSequence.Append(
            ladleRenderer.transform.DOLocalMove(
                ladleOriginalLocalPos + new Vector3(0f, bounceUpAmount, 0f),
                bounceUpDuration
            ).SetEase(Ease.OutQuad)
        );

        ladleSequence.Append(
            ladleRenderer.transform.DOLocalMove(
                ladleOriginalLocalPos,
                bounceDownDuration
            ).SetEase(Ease.InOutSine)
        );

        ladleSequence.OnComplete(() =>
        {
            isAnimating = false;
        });
    }

    public void PlayPourToPanAnimation(Vector3 panWorldPos)
    {
        InitializeLadle();

        if (ladleRenderer == null)
        {
            isSelected = false;
            isAnimating = false;
            isPouring = false;
            return;
        }

        KillSequence();
        isAnimating = true;
        isPouring = true;

        ladleRenderer.gameObject.SetActive(true);

        Color visibleColor = ladleOriginalColor;
        visibleColor.a = 1f;
        ladleRenderer.color = visibleColor;

        if (scoopedLadleSprite != null)
            ladleRenderer.sprite = scoopedLadleSprite;

        Vector3 circleCenter = panWorldPos + new Vector3(0f, circleCenterYOffset, 0f);
        Vector3 circleStart = circleCenter + Vector3.up * circleRadius;
        Vector3[] circlePath = BuildCirclePath(circleCenter, circleRadius, 16);

        ladleSequence = DOTween.Sequence();

        ladleSequence.Append(
            ladleRenderer.transform.DOMove(circleStart, moveToPanDuration)
                .SetEase(Ease.OutCubic)
        );

        ladleSequence.Append(
            ladleRenderer.transform.DOPath(circlePath, circleDuration, PathType.CatmullRom)
                .SetEase(Ease.Linear)
        );

        float switchTime = moveToPanDuration + circleDuration * 0.72f;
        float fadeStartTime = moveToPanDuration + circleDuration * 0.82f;

        ladleSequence.InsertCallback(switchTime, () =>
        {
            if (emptyLadleSprite != null)
                ladleRenderer.sprite = emptyLadleSprite;
        });

        ladleSequence.Insert(
            fadeStartTime,
            ladleRenderer.DOFade(0f, fadeOutDuration)
        );

        ladleSequence.OnComplete(() =>
        {
            ResetLadleVisual();
            isSelected = false;
            isAnimating = false;
            isPouring = false;
        });
    }

    public void Cancel()
    {
        InitializeLadle();

        if (isPouring)
        {
            return;
        }

        if (ladleRenderer == null)
        {
            isSelected = false;
            isAnimating = false;
            return;
        }

        KillSequence();

        isSelected = false;

        if (!ladleRenderer.gameObject.activeSelf)
        {
            isAnimating = false;
            return;
        }

        isAnimating = true;

        ladleRenderer.sprite = emptyLadleSprite;

        ladleSequence = DOTween.Sequence();

        ladleSequence.Append(
            ladleRenderer.transform.DOLocalMove(
                ladleOriginalLocalPos + Vector3.down * riseDistance,
                cancelDuration
            ).SetEase(Ease.InSine)
        );

        ladleSequence.Join(
            ladleRenderer.DOFade(0f, cancelDuration)
        );

        ladleSequence.OnComplete(() =>
        {
            ResetLadleVisual();
            isAnimating = false;
        });
    }

    private void ResetLadleVisual()
    {
        if (ladleRenderer == null) return;

        ladleRenderer.transform.localPosition = ladleOriginalLocalPos;

        if (emptyLadleSprite != null)
            ladleRenderer.sprite = emptyLadleSprite;

        Color c = ladleOriginalColor;
        c.a = 0f;
        ladleRenderer.color = c;

        ladleRenderer.gameObject.SetActive(false);
    }

    private void KillSequence()
    {
        if (ladleSequence != null)
        {
            ladleSequence.Kill();
            ladleSequence = null;
        }
    }

    private Vector3[] BuildCirclePath(Vector3 center, float radius, int pointCount)
    {
        Vector3[] path = new Vector3[pointCount + 1];

        for (int i = 0; i <= pointCount; i++)
        {
            float t = (float)i / pointCount;
            float angle = Mathf.PI * 0.5f - t * Mathf.PI * 2f; // 위에서 시작, 시계방향
            path[i] = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        }

        return path;
    }
}