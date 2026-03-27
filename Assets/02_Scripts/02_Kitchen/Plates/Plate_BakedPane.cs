using System;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Plate_BakedPane : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    private Vector3 originalScale;
    private Collider[] ownColliders;

    public bool isSelected { get; private set; }
    public bool isBeingTrashed { get; private set; } = false;

    private bool canPick = false;
    public bool CanBeSelected => false;
    private bool CanDrag => canPick && !isBeingTrashed;

    [Header("<<선택 연출>>")]
    [SerializeField] private float selectScaleDuration = 0.12f;
    [SerializeField] private float selectedScaleMultiplier = 1.08f;

    [SerializeField] private float paneCost = 3f;

    [Header("<<드래그 설정>>")]
    [SerializeField] private Vector3 mouseFollowOffset = Vector3.zero;
    [SerializeField] private float returnDuration = 0.2f;

    private bool isDragging = false;
    private bool isAnimating = false;

    private Vector3 dragStartWorldPos;
    private Transform dragStartParent;
    private int dragStartSortingOrder;
    private string dragStartSortingLayerName;
    private float dragScreenZ;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        ownColliders = GetComponentsInChildren<Collider>(true);

        SetPickable(false);
    }

    private void OnDisable()
    {
        transform.DOKill();
    }

    public bool Interact(IInteractable target)
    {
        return false;
    }

    public void SetPickable(bool value)
    {
        canPick = value;

        if (!isDragging)
            SetOwnCollidersEnabled(value);

        Debug.Log($"[Pane] Pickable = {value}, Collider 개수 = {ownColliders.Length}");
    }

    private void SetOwnCollidersEnabled(bool value)
    {
        if (ownColliders == null)
            return;

        foreach (var col in ownColliders)
        {
            if (col != null)
                col.enabled = value;
        }
    }

    public float GetCost()
    {
        return paneCost;
    }

    public void OnTrashed()
    {
        isBeingTrashed = true;
        isSelected = false;
        isDragging = false;
        SetPickable(false);
    }

    public void PlayTrashEffect(Transform trashTarget)
    {
        float moveDuration = 0.9f;
        float fadeDuration = 0.31f;

        Vector3 targetPos = trashTarget.position;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMove(targetPos, moveDuration)
            .SetEase(Ease.OutQuad));

        seq.Join(transform.DOScale(Vector3.zero, moveDuration)
            .SetEase(Ease.InQuad));

        if (sr != null)
            seq.Append(sr.DOFade(0f, fadeDuration));

        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void OnMouseDown()
    {
        if (!CanDrag || isDragging || isAnimating)
            return;

        Kitchen_Manager.Instance?.ClearSelection(this);

        if (Camera.main == null)
            return;

        transform.DOKill();

        isDragging = true;
        isSelected = true;

        dragStartWorldPos = transform.position;
        dragStartParent = transform.parent;

        if (sr != null)
        {
            dragStartSortingOrder = sr.sortingOrder;
            dragStartSortingLayerName = sr.sortingLayerName;
            sr.sortingOrder = 999;
        }

        SetOwnCollidersEnabled(false);

        transform.SetParent(null, true);
        transform.localScale = originalScale * selectedScaleMultiplier;

        dragScreenZ = Camera.main.WorldToScreenPoint(transform.position).z;
        UpdateDragPosition();
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        UpdateDragPosition();
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;

        bool dropped = TryDropTarget();

        if (!dropped)
        {
            Cancel();
        }
        else
        {
            CompleteSuccessfulDrag();
        }
    }

    private void UpdateDragPosition()
    {
        if (Camera.main == null)
            return;

        Vector3 mouse = Input.mousePosition;
        mouse.z = dragScreenZ;

        Vector3 world = Camera.main.ScreenToWorldPoint(mouse);
        world.z = dragStartWorldPos.z;

        transform.position = world + mouseFollowOffset;
    }

    private bool TryDropTarget()
    {
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        if (hits == null || hits.Length == 0)
            return false;

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.collider == null)
                continue;

            bool isOwnCollider = false;
            foreach (var ownCol in ownColliders)
            {
                if (ownCol == hit.collider)
                {
                    isOwnCollider = true;
                    break;
                }
            }

            if (isOwnCollider)
                continue;

            MonoBehaviour[] behaviours = hit.collider.GetComponentsInParent<MonoBehaviour>(true);

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null)
                    continue;

                if (behaviour.gameObject == gameObject)
                    continue;

                if (behaviour is IInteractable interactable)
                {
                    bool accepted = interactable.Interact(this);
                    if (accepted)
                        return true;
                }
            }
        }

        return false;
    }

    private void CompleteSuccessfulDrag()
    {
        isSelected = false;
        isAnimating = false;

        SetOwnCollidersEnabled(canPick);

        if (sr != null)
        {
            sr.sortingOrder = dragStartSortingOrder;
            sr.sortingLayerName = dragStartSortingLayerName;
        }

        transform.localScale = originalScale;
    }

    public void Cancel()
    {
        isSelected = false;
        isDragging = false;
        isAnimating = true;

        transform.DOKill();
        transform.localScale = originalScale;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(dragStartWorldPos, returnDuration).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(originalScale, returnDuration).SetEase(Ease.OutQuad));

        seq.OnComplete(() =>
        {
            if (dragStartParent != null)
                transform.SetParent(dragStartParent, true);

            SetOwnCollidersEnabled(canPick);

            if (sr != null)
            {
                sr.sortingOrder = dragStartSortingOrder;
                sr.sortingLayerName = dragStartSortingLayerName;
            }

            transform.localScale = originalScale;
            isAnimating = false;
        });
    }
}