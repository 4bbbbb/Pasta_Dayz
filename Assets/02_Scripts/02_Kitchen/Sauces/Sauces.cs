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

    [Header("드래그")]
    [SerializeField] private Vector3 mouseFollowOffset = new Vector3(0.35f, -0.15f, 0f);
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
    private Quaternion ladleOriginalLocalRot;
    private Transform ladleOriginalParent;
    private Color ladleOriginalColor;
    private Sequence ladleSequence;

    private bool isAnimating = false;
    private bool isPouring = false;
    private bool isDragging = false;
    private bool isInitialized = false;

    private float dragScreenZ;
    private int originalSortingOrder;
    private string originalSortingLayerName;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => !isPouring;

    private void Awake()
    {
        InitializeLadle();
    }

    private void InitializeLadle()
    {
        if (isInitialized) return;

        if (ladleRenderer != null)
        {
            ladleOriginalParent = ladleRenderer.transform.parent;
            ladleOriginalLocalPos = ladleRenderer.transform.localPosition;
            ladleOriginalLocalRot = ladleRenderer.transform.localRotation;
            ladleOriginalColor = ladleRenderer.color;
            originalSortingOrder = ladleRenderer.sortingOrder;
            originalSortingLayerName = ladleRenderer.sortingLayerName;

            ResetLadleVisual();
        }

        isInitialized = true;
    }

    public bool Interact(IInteractable target)
    {
        // 이제 클릭 선택형이 아니라 드래그형
        return false;
    }

    private void OnMouseDown()
    {
        InitializeLadle();

        if (ladleRenderer == null) return;
        if (isPouring || isAnimating) return;
        if (Camera.main == null) return;

        KillSequence();

        isDragging = true;
        isSelected = true;
        isAnimating = false;

        ladleRenderer.gameObject.SetActive(true);

        // 부모 분리해서 마우스를 따라다니게
        ladleRenderer.transform.SetParent(null, true);

        if (scoopedLadleSprite != null)
            ladleRenderer.sprite = scoopedLadleSprite;

        Color visibleColor = ladleOriginalColor;
        visibleColor.a = 1f;
        ladleRenderer.color = visibleColor;

        ladleRenderer.sortingOrder = 999;

        dragScreenZ = Camera.main.WorldToScreenPoint(transform.position).z;

        UpdateLadleFollowMouse();
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;
        UpdateLadleFollowMouse();
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;

        bool droppedSuccessfully = TryDropTarget();

        if (!droppedSuccessfully)
        {
            Cancel();
        }
    }

    private void UpdateLadleFollowMouse()
    {
        if (ladleRenderer == null || Camera.main == null)
            return;

        Vector3 mouse = Input.mousePosition;
        mouse.z = dragScreenZ;

        Vector3 world = Camera.main.ScreenToWorldPoint(mouse);
        world.z = transform.position.z;

        ladleRenderer.transform.position = world + mouseFollowOffset;
    }

    private bool TryDropTarget()
    {
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Debug.Log("소스 드롭 실패: 아무 콜라이더도 맞지 않음");
            return false;
        }

        Debug.Log("소스 드롭 시 맞은 오브젝트: " + hit.collider.name);

        // 후라이팬만 성공 처리
        Cooker_FryingPan pan = hit.collider.GetComponentInParent<Cooker_FryingPan>();
        if (pan != null)
        {
            Debug.Log("후라이팬 감지됨");
            return pan.Interact(this);
        }

        Debug.Log("소스 드롭 실패: 후라이팬 아님");
        return false;
    }

    public void PlayPourToPanAnimation(Vector3 panWorldPos)
    {
        InitializeLadle();

        if (ladleRenderer == null)
        {
            isSelected = false;
            isAnimating = false;
            isPouring = false;
            isDragging = false;
            return;
        }

        KillSequence();

        isAnimating = true;
        isPouring = true;
        isDragging = false;

        ladleRenderer.gameObject.SetActive(true);
        ladleRenderer.transform.SetParent(null, true);

        Color visibleColor = ladleOriginalColor;
        visibleColor.a = 1f;
        ladleRenderer.color = visibleColor;

        ladleRenderer.sortingOrder = 999;

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
            isDragging = false;
        });
    }

    public void Cancel()
    {
        InitializeLadle();

        if (isPouring)
            return;

        if (ladleRenderer == null)
        {
            isSelected = false;
            isAnimating = false;
            isDragging = false;
            return;
        }

        KillSequence();

        isSelected = false;
        isDragging = false;

        if (!ladleRenderer.gameObject.activeSelf)
        {
            isAnimating = false;
            return;
        }

        isAnimating = true;

        if (emptyLadleSprite != null)
            ladleRenderer.sprite = emptyLadleSprite;

        Vector3 homeWorldPos = ladleOriginalParent != null
            ? ladleOriginalParent.TransformPoint(ladleOriginalLocalPos)
            : ladleOriginalLocalPos;

        ladleSequence = DOTween.Sequence();

        ladleSequence.Append(
            ladleRenderer.transform.DOMove(homeWorldPos, cancelDuration)
                .SetEase(Ease.InSine)
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

        if (ladleOriginalParent != null)
            ladleRenderer.transform.SetParent(ladleOriginalParent, false);

        ladleRenderer.transform.localPosition = ladleOriginalLocalPos;
        ladleRenderer.transform.localRotation = ladleOriginalLocalRot;

        if (emptyLadleSprite != null)
            ladleRenderer.sprite = emptyLadleSprite;

        Color c = ladleOriginalColor;
        c.a = 0f;
        ladleRenderer.color = c;

        ladleRenderer.sortingOrder = originalSortingOrder;
        ladleRenderer.sortingLayerName = originalSortingLayerName;

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
            float angle = Mathf.PI * 0.5f - t * Mathf.PI * 2f;
            path[i] = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        }

        return path;
    }
}