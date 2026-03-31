using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Plates_BasicPlate : MonoBehaviour, IInteractable
{
    [Header("<<완성된 파스타 스폰위치>>")]
    [SerializeField] private Transform pastaSpawnPoint;

    [Header("<<구워진 빠네 스폰위치>>")]
    [SerializeField] private Transform paneSpawnPoint;

    [Header("<<구워진 빠네 프리팹>>")]
    [SerializeField] private GameObject paneOnPlatePrefab;

    [Header("<<선택/드래그 설정>>")]
    [SerializeField] private float selectedScaleMultiplier = 1.01f;
    [SerializeField] private Vector3 mouseFollowOffset = new Vector3(0.2f, -0.1f, 0f);
    [SerializeField] private float returnDuration = 0.2f;

    private SpriteRenderer sr;
    private Collider plateCollider;
    private Collider[] ownColliders;

    public bool isSelected { get; private set; }
    public bool isBeingTrashed { get; private set; } = false;
    public bool CanBeSelected => !isBeingTrashed && !isAnimating && !isDragging;

    private GameObject currentPaneVisual;

    private bool hasPasta = false;
    private bool hasPane = false;

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

        // BasicPlate는 클릭 선택형이 아니라 받는 용도 + 드래그 쓰레기통용
        if (target == null)
            return false;

        // 1) 완성 파스타를 접시에 올릴 때
        if (target is FinishedPasta finishedPasta)
        {
            if (!CanAcceptTutorialPasta(finishedPasta))
            {
                Debug.Log("지금은 파스타를 접시에 담을 단계가 아니에요!");
                return false;
            }

            if (hasPasta)
            {
                Debug.Log("이미 파스타가 담겨 있어요!");
                return false;
            }

            if (!finishedPasta.CanMoveToPlate(plateID, hasPane))
            {
                Debug.Log("옮길 수 없습니다.");
                return false;
            }

            if (pastaSpawnPoint == null)
            {
                Debug.LogWarning($"{name}: pastaSpawnPoint가 비어 있습니다.");
                return false;
            }

            HashSet<int> finalIngredients = new HashSet<int>(ingredients);
            foreach (int id in finishedPasta.GetIngredientSet())
            {
                finalIngredients.Add(id);
            }

            finishedPasta.SetIngredients(finalIngredients);

            finishedPasta.transform.SetParent(pastaSpawnPoint, true);
            finishedPasta.transform.localPosition = Vector3.zero;
            finishedPasta.transform.localRotation = Quaternion.identity;
            finishedPasta.transform.localScale = Vector3.one;

            ingredients = new HashSet<int>(finalIngredients);

            hasPasta = true;
            isSelected = false;
            transform.localScale = originalScale;

            if (currentPaneVisual != null)
            {
                Destroy(currentPaneVisual);
                currentPaneVisual = null;
            }

            if (sr != null)
                sr.enabled = false;

            if (plateCollider != null)
                plateCollider.enabled = false;

            finishedPasta.OnMovedToPlate();

            finishedPasta.transform.SetParent(pastaSpawnPoint, true);
            finishedPasta.transform.localPosition = Vector3.zero;
            finishedPasta.transform.localRotation = Quaternion.identity;
            finishedPasta.transform.localScale = Vector3.one;

            PrintIngredients();
            return true;
        }

        // 2) 구워진 빠네를 접시에 올릴 때
        if (target is Plate_BakedPane bakedPane)
        {
            if (!CanAcceptTutorialBakedPane())
            {
                Debug.Log("첫 번째 키친 튜토리얼에서는 빠네를 사용할 수 없어요.");
                return false;
            }

            if (hasPasta)
            {
                Debug.Log("지금은 빠네를 추가할 수 없어요.");
                return false;
            }

            if (hasPane)
            {
                Debug.Log("이미 빠네가 준비되어 있어요!");
                return false;
            }

            IngredientIDs id = bakedPane.GetComponent<IngredientIDs>();
            if (id != null)
                ingredients.Add(id.GetID());
            else
                ingredients.Add(601);

            hasPane = true;
            isSelected = false;
            transform.localScale = originalScale;

            if (paneOnPlatePrefab != null && paneSpawnPoint != null)
            {
                currentPaneVisual = Instantiate(paneOnPlatePrefab, paneSpawnPoint);
                currentPaneVisual.transform.localPosition = Vector3.zero;
                currentPaneVisual.transform.localRotation = Quaternion.identity;
                currentPaneVisual.transform.localScale = Vector3.one;

                Collider[] cols = currentPaneVisual.GetComponentsInChildren<Collider>(true);
                foreach (var col in cols)
                {
                    if (col != null)
                        col.enabled = false;
                }
            }
            else
            {
                Debug.LogWarning("paneOnPlatePrefab 또는 paneSpawnPoint가 비어 있습니다.");
            }

            bakedPane.SetPickable(false);
            Destroy(bakedPane.gameObject);

            PrintIngredients();
            return true;
        }

        return false;
    }

    private void OnMouseDown()
    {
        if (isBeingTrashed || isDragging || isAnimating)
            return;

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

        Vector3 dragStartWorldScale = transform.lossyScale;

        if (sr != null)
        {
            dragStartSortingOrder = sr.sortingOrder;
            dragStartSortingLayerName = sr.sortingLayerName;
            sr.sortingOrder = 999;

            SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var r in childRenderers)
            {
                if (r == null) continue;
                r.sortingLayerName = dragStartSortingLayerName;
                r.sortingOrder = 1000;
            }
        }

        SetOwnCollidersEnabled(false);

        transform.SetParent(null, true);
        transform.localScale = dragStartWorldScale * selectedScaleMultiplier;

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

    public void Cancel()
    {
        isSelected = false;
        isDragging = false;
        isAnimating = true;

        transform.DOKill();

        if (dragStartParent != null)
            transform.SetParent(dragStartParent, true);

        SetOwnCollidersEnabled(true);

        if (sr != null)
        {
            sr.sortingOrder = dragStartSortingOrder;
            sr.sortingLayerName = dragStartSortingLayerName;
        }

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(dragStartWorldPos, returnDuration).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(originalScale, returnDuration).SetEase(Ease.OutQuad));

        seq.OnComplete(() =>
        {
            transform.localScale = originalScale;
            isAnimating = false;
        });
    }

    // 완전히 빈 접시는 비용 0원
    public float GetCost()
    {
        if (!hasPane && !hasPasta)
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

        transform.SetParent(null, true);
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

    public void AddIngredient(int id)
    {
        if (!ingredients.Contains(id))
            ingredients.Add(id);
    }

    public HashSet<int> GetIngredientSet()
    {
        return new HashSet<int>(ingredients);
    }

    public HashSet<int> GetIngredients()
    {
        return new HashSet<int>(ingredients);
    }

    public bool HasPane()
    {
        return hasPane;
    }

    public bool HasPasta()
    {
        return hasPasta;
    }

    public void PrintIngredients()
    {
        foreach (int id in ingredients)
        {
            Debug.Log("Plate에 포함된 ID: " + id);
        }
    }

    private bool IsFirstKitchenTutorialActive()
    {
        return TutorialController.Instance != null
            && TutorialController.Instance.IsTutorialActive
            && TutorialController.Instance.CurrentStep == TutorialController.TutorialStep.Kitchen_FirstCookProgress;
    }

    private bool CanAcceptTutorialPasta(FinishedPasta finishedPasta)
    {
        if (!IsFirstKitchenTutorialActive())
            return true;

        if (TutorialController.Instance == null)
            return true;

        return TutorialController.Instance.IsKitchenActionAllowed(
            TutorialController.KitchenPracticeTarget.DragPastaToPlate
        );
    }

    private bool CanAcceptTutorialBakedPane()
    {
        if (!IsFirstKitchenTutorialActive())
            return true;

        // 첫 번째 키친 튜토리얼에서는 빠네 사용 안 함
        return false;
    }
}
