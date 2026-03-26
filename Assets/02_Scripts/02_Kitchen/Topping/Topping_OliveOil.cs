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

    [Header("드래그")]
    [SerializeField] private float dragScaleMultiplier = 1.08f;

    private SpriteRenderer sr;
    private Collider[] cachedColliders;

    private bool isAnimating = false;
    private bool isDragging = false;

    private Transform originalParent;
    private Vector3 originalLocalPos;
    private Vector3 originalScale;
    private Quaternion originalLocalRot;

    private Transform dragStartParent;
    private Vector3 dragStartWorldPos;
    private Vector3 dragStartLocalPos;
    private Vector3 dragOffset;
    private float dragScreenZ;
    private int originalSortingOrder;

    public bool isSelected { get; private set; }
    public bool isOliveOil = true;
    public bool CanBeSelected => !isAnimating;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cachedColliders = GetComponentsInChildren<Collider>(true);

        originalParent = transform.parent;
        originalLocalPos = transform.localPosition;
        originalScale = transform.localScale;
        originalLocalRot = transform.localRotation;

        if (sr != null && originalSprite != null)
            sr.sprite = originalSprite;

        isSelected = false;
    }

    public bool Interact(IInteractable target)
    {
        // 클릭 선택 방식은 사실상 안 씀
        if (target == null)
            return false;

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
        dragScreenZ = Camera.main.WorldToScreenPoint(transform.position).z;
        dragOffset = transform.position - GetMouseWorldPosition();

        transform.DOKill();

        transform.localScale = originalScale * dragScaleMultiplier;
        SetPressedSprite();

        if (sr != null)
        {
            originalSortingOrder = sr.sortingOrder;
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
            RestoreToDragStart();
            RestoreColliderAndSorting();
            SetNormalSprite();
        }
        // dropped == true면
        // 후라이팬 쪽 AddOil -> PlayPourOnPanAnimation() 에서 마무리 처리
    }

    private bool TryDropTarget()
    {
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Debug.Log("오일 드롭 실패: 아무 콜라이더도 맞지 않음");
            return false;
        }

        Debug.Log("오일 드롭 시 맞은 오브젝트: " + hit.collider.name);

        Cooker_FryingPan fryingPan = hit.collider.GetComponentInParent<Cooker_FryingPan>();
        if (fryingPan != null)
        {
            Debug.Log("후라이팬 감지됨");
            return fryingPan.Interact(this);
        }

        Debug.Log("오일 드롭 실패: 후라이팬이 아님");
        return false;
    }

    public void PlayPourOnPanAnimation()
    {
        if (isAnimating)
            return;

        isAnimating = true;
        isSelected = true;

        transform.DOKill();

        // 원래 부모로 다시 붙이되 현재 월드 위치는 유지
        if (dragStartParent != null)
            transform.SetParent(dragStartParent, true);
        else if (originalParent != null)
            transform.SetParent(originalParent, true);

        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            SetPressedSprite();
        });

        // 드롭된 현재 위치에서 바로 기울이기
        seq.Append(transform.DORotate(new Vector3(0f, 0f, 25f), 0.18f).SetEase(Ease.OutQuad));

        // 붓는 시간
        seq.AppendInterval(0.15f);

        // 다시 세우기
        seq.Append(transform.DORotate(Vector3.zero, 0.18f).SetEase(Ease.InQuad));

        // 시작 위치로 복귀
        seq.Append(transform.DOMove(dragStartWorldPos, 0.25f).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            if (dragStartParent != null)
                transform.SetParent(dragStartParent, true);
            else if (originalParent != null)
                transform.SetParent(originalParent, true);

            transform.localPosition = dragStartParent != null ? dragStartLocalPos : originalLocalPos;
            transform.localScale = originalScale;
            transform.localRotation = originalLocalRot;

            SetNormalSprite();
            RestoreColliderAndSorting();

            isSelected = false;
            isAnimating = false;
        });
    }

    private void RestoreToDragStart()
    {
        if (dragStartParent != null)
        {
            transform.SetParent(dragStartParent, true);
            transform.position = dragStartWorldPos;
            transform.localPosition = dragStartLocalPos;
        }
        else if (originalParent != null)
        {
            transform.SetParent(originalParent, true);
            transform.localPosition = originalLocalPos;
        }

        transform.localScale = originalScale;
        transform.localRotation = originalLocalRot;
    }

    private void RestoreColliderAndSorting()
    {
        foreach (var col in cachedColliders)
        {
            if (col != null)
                col.enabled = true;
        }

        if (sr != null)
            sr.sortingOrder = originalSortingOrder;
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

    public void CancelImmediate()
    {
        transform.DOKill();

        isSelected = false;
        isDragging = false;
        isAnimating = false;

        if (originalParent != null)
            transform.SetParent(originalParent, true);

        transform.localPosition = originalLocalPos;
        transform.localScale = originalScale;
        transform.localRotation = originalLocalRot;

        SetNormalSprite();
        RestoreColliderAndSorting();
    }

    public void Cancel()
    {
        CancelImmediate();
    }
}