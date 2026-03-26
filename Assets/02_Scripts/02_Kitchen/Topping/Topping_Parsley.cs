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

    [Header("<<드래그>>")]
    [SerializeField] private float dragScaleMultiplier = 1.08f;

    [Header("<<뿌리기 연출>>")]
    [SerializeField] private float sprinkleRotateAngle = 120f;
    [SerializeField] private float sprinkleRotateDuration = 0.18f;
    [SerializeField] private float sprinkleShakeHeight = 0.08f;
    [SerializeField] private int sprinkleShakeCount = 4;
    [SerializeField] private float sprinkleShakeDuration = 0.18f;
    [SerializeField] private float returnDuration = 0.25f;

    private SpriteRenderer sr;
    private Collider[] cachedColliders;

    private Vector3 originalScale;
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private Transform originalParent;

    private Transform dragStartParent;
    private Vector3 dragStartWorldPos;
    private Vector3 dragStartLocalPos;
    private Quaternion dragStartLocalRot;

    private Vector3 dragOffset;
    private float dragScreenZ;

    private int originalSortingOrder;
    private string originalSortingLayerName;

    private bool isAnimating = false;
    private bool isDragging = false;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => !isAnimating;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cachedColliders = GetComponentsInChildren<Collider>(true);

        originalParent = transform.parent;
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
        originalScale = transform.localScale;

        if (sr != null && originalSprite != null)
            sr.sprite = originalSprite;

        isSelected = false;
    }

    public bool Interact(IInteractable target)
    {
        // 파슬리는 이제 클릭 선택형이 아니라 드래그형
        return false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (Camera.main == null)
            return transform.position;

        Vector3 mouse = Input.mousePosition;
        mouse.z = dragScreenZ;

        Vector3 world = Camera.main.ScreenToWorldPoint(mouse);
        world.z = dragStartWorldPos.z;
        return world;
    }

    private void OnMouseDown()
    {
        if (isAnimating)
            return;

        if (Camera.main == null)
            return;

        isDragging = true;
        isSelected = false;

        dragStartParent = transform.parent;
        dragStartWorldPos = transform.position;
        dragStartLocalPos = transform.localPosition;
        dragStartLocalRot = transform.localRotation;

        dragScreenZ = Camera.main.WorldToScreenPoint(transform.position).z;
        dragOffset = transform.position - GetMouseWorldPosition();

        transform.DOKill();

        transform.localScale = originalScale * dragScaleMultiplier;

        if (sr != null)
        {
            if (selectedSprite != null)
                sr.sprite = selectedSprite;

            originalSortingOrder = sr.sortingOrder;
            originalSortingLayerName = sr.sortingLayerName;
            sr.sortingOrder = 999;
        }

        foreach (var col in cachedColliders)
        {
            if (col != null)
                col.enabled = false;
        }

        transform.SetParent(null, true);
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;

        bool dropped = TryDropTarget();

        if (!dropped)
        {
            RestoreToStartImmediate();
            RestoreVisualState();
        }
    }

    private bool TryDropTarget()
    {
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Debug.Log("파슬리 드롭 실패: 아무 콜라이더도 맞지 않음");
            return false;
        }

        Debug.Log("파슬리 드롭 시 맞은 오브젝트: " + hit.collider.name);

        FinishedPasta finishedPasta = hit.collider.GetComponentInParent<FinishedPasta>();
        if (finishedPasta != null)
        {
            Debug.Log("FinishedPasta 감지됨");
            return finishedPasta.Interact(this);
        }

        BakedPasta bakedPasta = hit.collider.GetComponentInParent<BakedPasta>();
        if (bakedPasta != null)
        {
            Debug.Log("BakedPasta 감지됨");
            return bakedPasta.Interact(this);
        }

        Debug.Log("파슬리 드롭 실패: FinishedPasta / BakedPasta 아님");
        return false;
    }

    public void Sprinkle(Transform pastaPoint, System.Action onSprinkle)
    {
        if (isAnimating)
            return;

        isAnimating = true;
        isSelected = true;

        transform.DOKill();

        // 드래그해서 놓은 "현재 위치"에서 바로 뿌리기
        Vector3 currentDropPos = transform.position;
        Quaternion currentDropRot = transform.rotation;
        Quaternion pourRot = Quaternion.Euler(0f, 0f, sprinkleRotateAngle);

        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            if (sr != null && selectedSprite != null)
                sr.sprite = selectedSprite;
        });

        // 그 자리에서 기울이기
        seq.Append(
            transform.DORotateQuaternion(pourRot, sprinkleRotateDuration)
                     .SetEase(Ease.OutQuad)
        );

        // 프리팹 생성 타이밍은 기존 FinishedPasta/BakedPasta 코드 그대로 사용
        seq.AppendCallback(() =>
        {
            onSprinkle?.Invoke();
        });

        // 그 자리에서 살짝 흔들기
        seq.Append(
            transform.DOMoveY(currentDropPos.y + sprinkleShakeHeight, sprinkleShakeDuration)
                     .SetLoops(sprinkleShakeCount, LoopType.Yoyo)
                     .SetEase(Ease.InOutSine)
        );

        // 다시 세우기
        seq.Append(
            transform.DORotateQuaternion(currentDropRot, sprinkleRotateDuration)
                     .SetEase(Ease.InQuad)
        );

        // 원래 자리로 복귀
        seq.Append(
            transform.DOMove(dragStartWorldPos, returnDuration)
                     .SetEase(Ease.InQuad)
        );

        seq.OnComplete(() =>
        {
            if (dragStartParent != null)
            {
                transform.SetParent(dragStartParent, true);
                transform.localPosition = dragStartLocalPos;
                transform.localRotation = dragStartLocalRot;
            }
            else if (originalParent != null)
            {
                transform.SetParent(originalParent, true);
                transform.localPosition = originalLocalPos;
                transform.localRotation = originalLocalRot;
            }

            transform.localScale = originalScale;

            RestoreVisualState();

            isSelected = false;
            isAnimating = false;
        });
    }

    private void RestoreToStartImmediate()
    {
        if (dragStartParent != null)
        {
            transform.SetParent(dragStartParent, true);
            transform.localPosition = dragStartLocalPos;
            transform.localRotation = dragStartLocalRot;
        }
        else if (originalParent != null)
        {
            transform.SetParent(originalParent, true);
            transform.localPosition = originalLocalPos;
            transform.localRotation = originalLocalRot;
        }
        else
        {
            transform.position = dragStartWorldPos;
        }

        transform.localScale = originalScale;
    }

    private void RestoreVisualState()
    {
        foreach (var col in cachedColliders)
        {
            if (col != null)
                col.enabled = true;
        }

        if (sr != null)
        {
            sr.sortingOrder = originalSortingOrder;
            sr.sortingLayerName = originalSortingLayerName;

            if (originalSprite != null)
                sr.sprite = originalSprite;
        }
    }

    public void Cancel()
    {
        if (isAnimating)
            return;

        isSelected = false;

        transform.DOKill();
        RestoreToStartImmediate();
        RestoreVisualState();
    }
}