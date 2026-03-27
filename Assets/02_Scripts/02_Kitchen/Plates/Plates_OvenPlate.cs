using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Plates_OvenPlate : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    private Collider plateCollider;
    private Collider[] ownColliders;

    [Header("선택 연출")]
    [SerializeField] private float selectScaleDuration = 0.12f;
    [SerializeField] private float selectedScaleMultiplier = 1.08f;

    [Header("드래그 설정")]
    [SerializeField] private Vector3 mouseFollowOffset = Vector3.zero;
    [SerializeField] private float returnDuration = 0.2f;

    public bool isSelected { get; private set; }
    public bool isBeingTrashed { get; private set; } = false;
    public bool CanBeSelected => !isBeingTrashed;

    private bool hasPasta = false;

    private int plateID = -1;
    private IngredientIDs ingredientIDs;
    private HashSet<int> ingredients = new HashSet<int>();

    private Vector3 originalScale;

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
        plateCollider = GetComponent<Collider>();
        ownColliders = GetComponentsInChildren<Collider>(true);

        isSelected = false;
        originalScale = transform.localScale;

        ingredientIDs = GetComponent<IngredientIDs>();
        if (ingredientIDs == null)
            ingredientIDs = GetComponentInChildren<IngredientIDs>(true);

        ingredients.Clear();

        if (ingredientIDs != null)
        {
            plateID = ingredientIDs.GetID();
            if (plateID != -1)
                ingredients.Add(plateID);
        }
        else
        {
            Debug.LogWarning($"{name}: IngredientIDs가 없습니다.");
        }
    }

    private void OnDisable()
    {
        transform.DOKill();
    }

    public bool Interact(IInteractable target)
    {
        if (isBeingTrashed)
            return false;

        if (target == null)
        {
            Select();
            return true;
        }

        if (target is FinishedPasta finishedPasta)
        {
            if (hasPasta)
            {
                Debug.Log("이미 파스타가 담겨 있어요!");
                return false;
            }

            // plate ID 보장
            if (plateID != -1)
                ingredients.Add(plateID);

            // oven plate는 hasPane 개념 없음
            if (!finishedPasta.CanMoveToPlate(plateID, false))
            {
                Debug.Log("옮길 수 없습니다.");
                return false;
            }

            finishedPasta.transform.SetParent(transform, true);
            finishedPasta.transform.localPosition = Vector3.zero;
            finishedPasta.transform.localRotation = Quaternion.identity;
            finishedPasta.transform.localScale = Vector3.one;

            HashSet<int> finalIngredients = new HashSet<int>(ingredients);
            foreach (int id in finishedPasta.GetIngredientSet())
            {
                finalIngredients.Add(id);
            }

            finishedPasta.SetIngredients(finalIngredients);
            ingredients = new HashSet<int>(finalIngredients);

            hasPasta = true;
            isSelected = false;
            transform.localScale = originalScale;

            // 기존 오븐 접시 비주얼 숨김
            if (sr != null)
                sr.enabled = false;

            if (plateCollider != null)
                plateCollider.enabled = false;

            finishedPasta.OnMovedToPlate();
            PrintIngredients();
            return true;
        }

        return false;
    }

    public float GetCost()
    {
        if (!hasPasta)
            return 0f;

        if (IngredientDatabase.Instance == null)
            return 0f;

        float total = 0f;

        foreach (int id in ingredients)
        {
            IngredientData data = IngredientDatabase.Instance.GetIngredient(id);
            if (data != null)
                total += data.ingredientCost;
        }

        return total;
    }

    public void OnTrashed()
    {
        isBeingTrashed = true;
        isSelected = false;
        isDragging = false;
        transform.localScale = originalScale;

        SetOwnCollidersEnabled(false);
    }

    public void PlayTrashEffect(Transform trashTarget)
    {
        float moveDuration = 0.9f;
        float fadeDuration = 0.31f;
        Vector3 targetPos = trashTarget.position;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMove(targetPos, moveDuration).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(Vector3.zero, moveDuration).SetEase(Ease.InQuad));

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            if (r != null)
                seq.Join(r.DOFade(0f, fadeDuration));
        }

        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void Select()
    {
        isSelected = true;
        transform.DOKill();
        transform.DOScale(originalScale * selectedScaleMultiplier, selectScaleDuration)
                 .SetEase(Ease.OutBack);
    }

    public void AddIngredient(int id)
    {
        if (!ingredients.Contains(id))
            ingredients.Add(id);
    }

    public HashSet<int> GetIngredientSet()
    {
        return new HashSet<int>(ingredients);
    }

    public void PrintIngredients()
    {
        foreach (int id in ingredients)
        {
            Debug.Log("OvenPlate에 포함된 ID: " + id);
        }
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

            SetOwnCollidersEnabled(true);

            if (sr != null)
            {
                sr.sortingOrder = dragStartSortingOrder;
                sr.sortingLayerName = dragStartSortingLayerName;
            }

            transform.localScale = originalScale;
            isAnimating = false;
        });
    }

    private void OnMouseDown()
    {
        if (isBeingTrashed || isDragging || isAnimating)
            return;

        // 파스타가 올라간 뒤엔 접시 비주얼/콜라이더를 꺼두므로 사실상 드래그 안 됨.
        // 그래도 안전하게 막아둠.
        if (hasPasta)
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
            Cancel();
        else
            CompleteSuccessfulDrag();
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

        SetOwnCollidersEnabled(true);

        if (sr != null)
        {
            sr.sortingOrder = dragStartSortingOrder;
            sr.sortingLayerName = dragStartSortingLayerName;
        }

        transform.localScale = originalScale;
    }
}