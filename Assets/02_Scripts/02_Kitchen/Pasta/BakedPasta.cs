using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;
using static Sauces;

public class BakedPasta : MonoBehaviour, IInteractable
{
    [Header("<<파슬리 프리팹>>")]
    [SerializeField] private GameObject parsleyPrefab;

    [Header("<<파슬리 스폰 위치>>")]
    [SerializeField] private Transform parsleySpawnPoint;

    [Header("<<상태별 크기>>")]
    [SerializeField] private Vector3 inOvenScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 platedScale = new Vector3(0.7f, 0.7f, 1f);

    [Header("<<클릭용 콜라이더>>")]
    [SerializeField] private Collider foodCollider;

    [Header("<<선택 연출>>")]
    [SerializeField] private float selectScaleDuration = 0.12f;
    [SerializeField] private float selectedScaleMultiplier = 1.08f;

    [Header("<<드래그 이동>>")]
    [SerializeField] private float dragLiftScaleMultiplier = 1.08f;
    [SerializeField] private float dragStartThreshold = 0.12f;

    public enum BakedState
    {
        InOven,
        Plated
    }

    private BakedState currentState;

    [System.Serializable]
    public class BakedCheeseSpriteEntry
    {
        public int sauceID;
        public int plateID;
        public int cheeseID;
        public BakedState state;
        public Sprite sprite;
    }

    [SerializeField] private List<BakedCheeseSpriteEntry> bakedCheeseEntries = new List<BakedCheeseSpriteEntry>();

    private SpriteRenderer sr;
    private SpriteRenderer[] cachedRenderers;
    private Collider[] cachedColliders;

    private int[] savedSortingOrders;
    private string[] savedSortingLayers;

    public bool isSelected { get; private set; }
    public bool isBeingTrashed { get; private set; } = false;

    private bool canPick = false;
    public bool CanBeSelected => canPick;

    private bool isPointerDown = false;
    private bool hasStartedRealDrag = false;

    private Vector3 dragStartWorldPos;
    private Vector3 dragStartLocalPos;
    private Transform dragStartParent;
    private Vector3 dragOffset;
    private float dragScreenZ;
    private Vector3 mouseDownWorldPos;

    private HashSet<int> ingredientIDs = new HashSet<int>();

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (foodCollider == null)
            foodCollider = GetComponent<Collider>();

        RefreshDragCaches();

        currentState = BakedState.InOven;
        ApplyStateVisual();

        // 처음 생성될 때는 오븐 안에 있으므로 클릭 막기
        SetPickable(false);
    }

    private void RefreshDragCaches()
    {
        cachedRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        savedSortingOrders = new int[cachedRenderers.Length];
        savedSortingLayers = new string[cachedRenderers.Length];

        cachedColliders = GetComponentsInChildren<Collider>(true);
    }

    public void SetIngredients(HashSet<int> ids)
    {
        ingredientIDs = new HashSet<int>(ids);
        UpdateBakedSprite();
    }

    public HashSet<int> GetIngredientSet()
    {
        return new HashSet<int>(ingredientIDs);
    }

    public float GetCost()
    {
        if (IngredientDatabase.Instance == null)
            return 0f;

        float total = 0f;

        foreach (int id in ingredientIDs)
        {
            IngredientData data = IngredientDatabase.Instance.GetIngredient(id);
            if (data != null)
                total += data.ingredientCost;
        }

        return total;
    }

    public bool Interact(IInteractable target)
    {
        if (!canPick || isBeingTrashed)
            return false;

        if (target == null)
        {
            Select();
            return true;
        }

        if (target is Topping_Parsley parsley)
        {
            if (currentState != BakedState.Plated)
            {
                Debug.Log("플레이트 위에 올려진 baked pasta에만 파슬리를 추가할 수 있어요!");
                return false;
            }

            Debug.Log("파슬리를 뿌렸어요");

            parsley.Sprinkle(parsleySpawnPoint, () =>
            {
                Instantiate(
                    parsleyPrefab,
                    parsleySpawnPoint.position,
                    Quaternion.identity,
                    parsleySpawnPoint
                );
            });

            IngredientIDs id = parsley.GetComponent<IngredientIDs>();
            if (id != null)
                ingredientIDs.Add(id.GetID());

            return true;
        }

        return false;
    }

    public void SetState(BakedState state)
    {
        currentState = state;
        ApplyStateVisual();
        UpdateBakedSprite();
    }

    public bool IsPlated()
    {
        return currentState == BakedState.Plated;
    }

    public void SetPickable(bool value)
    {
        canPick = value;

        Collider[] cols = GetComponentsInChildren<Collider>(true);
        foreach (var col in cols)
        {
            col.enabled = value;
        }
    }

    public void OnTrashed()
    {
        isBeingTrashed = true;
        isSelected = false;
        isPointerDown = false;
        hasStartedRealDrag = false;

        SetPickable(false);

        if (sr != null)
            sr.color = Color.white;
    }

    public void PlayTrashEffect(Transform trashTarget)
    {
        float moveDuration = 0.9f;
        float fadeDuration = 0.31f;
        Vector3 targetPos = trashTarget.position;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMove(targetPos, moveDuration).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(Vector3.zero, moveDuration).SetEase(Ease.InQuad));

        if (sr != null)
            seq.Join(sr.DOFade(0f, fadeDuration));

        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private Vector3 GetBaseScale()
    {
        return currentState == BakedState.InOven ? inOvenScale : platedScale;
    }

    private void ApplyStateVisual()
    {
        transform.DOKill();

        Vector3 baseScale = GetBaseScale();
        transform.localScale = isSelected
            ? baseScale * selectedScaleMultiplier
            : baseScale;
    }

    public void AddIngredient(int id)
    {
        ingredientIDs.Add(id);
        UpdateBakedSprite();
    }

    private int GetSauceID()
    {
        if (ingredientIDs.Contains(202)) return 202;
        if (ingredientIDs.Contains(203)) return 203;
        if (ingredientIDs.Contains(204)) return 204;
        if (ingredientIDs.Contains(205)) return 205;
        if (ingredientIDs.Contains(201)) return 201;
        return -1;
    }

    private int GetPlateID()
    {
        if (ingredientIDs.Contains(502)) return 502;
        return -1;
    }

    private int GetCheeseID()
    {
        if (ingredientIDs.Contains(402)) return 402;
        return -1;
    }

    private void UpdateBakedSprite()
    {
        int sauceID = GetSauceID();
        int plateID = GetPlateID();
        int cheeseID = GetCheeseID();

        if (sauceID == -1)
            return;

        foreach (var entry in bakedCheeseEntries)
        {
            if (entry.sauceID == sauceID &&
                entry.plateID == plateID &&
                entry.cheeseID == cheeseID &&
                entry.state == currentState)
            {
                sr.sprite = entry.sprite;
                return;
            }
        }

        Debug.Log($"state={currentState}, sauce={sauceID}, plate={plateID}, cheese={cheeseID}");
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
        if (!canPick || isBeingTrashed)
            return;

        if (Camera.main == null)
            return;

        isPointerDown = true;
        hasStartedRealDrag = false;

        dragStartWorldPos = transform.position;
        dragStartLocalPos = transform.localPosition;
        dragStartParent = transform.parent;
        dragScreenZ = Camera.main.WorldToScreenPoint(transform.position).z;

        mouseDownWorldPos = GetMouseWorldPosition();
        dragOffset = transform.position - mouseDownWorldPos;
    }

    private void OnMouseDrag()
    {
        if (!isPointerDown)
            return;

        Vector3 currentMouseWorld = GetMouseWorldPosition();

        if (!hasStartedRealDrag)
        {
            float dragDistance = Vector3.Distance(currentMouseWorld, mouseDownWorldPos);
            if (dragDistance >= dragStartThreshold)
            {
                BeginRealDrag();
            }
        }

        if (!hasStartedRealDrag)
            return;

        transform.position = currentMouseWorld + dragOffset;
    }

    private void BeginRealDrag()
    {
        RefreshDragCaches();

        hasStartedRealDrag = true;
        isSelected = false;

        transform.DOKill();
        transform.localScale = GetBaseScale() * dragLiftScaleMultiplier;

        RaiseAllSortingForDrag();

        if (cachedColliders != null)
        {
            foreach (var col in cachedColliders)
            {
                if (col != null)
                    col.enabled = false;
            }
        }

        transform.SetParent(null, true);
    }

    private void OnMouseUp()
    {
        if (!isPointerDown)
            return;

        isPointerDown = false;

        if (!hasStartedRealDrag)
        {
            Select();
            return;
        }

        hasStartedRealDrag = false;

        bool placed = TryDropTarget();

        if (!placed)
        {
            transform.SetParent(dragStartParent, true);
            transform.position = dragStartWorldPos;
            transform.localPosition = dragStartLocalPos;
            transform.localScale = GetBaseScale();
        }

        if (cachedColliders != null)
        {
            foreach (var col in cachedColliders)
            {
                if (col != null)
                    col.enabled = !isBeingTrashed && canPick;
            }
        }

        if (!placed)
            RestoreAllSortingAfterDrag();
        else
            ApplyPlacedSorting();
    }

    private bool TryDropTarget()
    {
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Debug.Log("BakedPasta 드롭 실패: 아무 콜라이더도 맞지 않음");
            return false;
        }

        Cooker_Trashcan trashcan = hit.collider.GetComponentInParent<Cooker_Trashcan>();
        if (trashcan != null)
            return trashcan.Interact(this);

        Cooker_PlateTable plateTable = hit.collider.GetComponentInParent<Cooker_PlateTable>();
        if (plateTable != null)
            return plateTable.Interact(this);

        Cooker_PassTable passTable = hit.collider.GetComponentInParent<Cooker_PassTable>();
        if (passTable != null)
            return passTable.Interact(this);

        return false;
    }

    private void RaiseAllSortingForDrag()
    {
        if (cachedRenderers == null)
            return;

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] == null)
                continue;

            savedSortingOrders[i] = cachedRenderers[i].sortingOrder;
            savedSortingLayers[i] = cachedRenderers[i].sortingLayerName;

            cachedRenderers[i].sortingLayerName = "Default";
            cachedRenderers[i].sortingOrder = 999 + i;
        }
    }

    private void RestoreAllSortingAfterDrag()
    {
        if (cachedRenderers == null)
            return;

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] == null)
                continue;

            cachedRenderers[i].sortingLayerName = savedSortingLayers[i];
            cachedRenderers[i].sortingOrder = savedSortingOrders[i];
        }
    }

    private void ApplyPlacedSorting()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        sr.sortingLayerName = "Default";
        sr.sortingOrder = 3;
    }

    void Select()
    {
        isSelected = true;
        transform.DOKill();

        Vector3 baseScale = GetBaseScale();
        transform.DOScale(baseScale * selectedScaleMultiplier, selectScaleDuration)
                 .SetEase(Ease.OutBack);
    }

    public void Cancel()
    {
        isSelected = false;
        transform.DOKill();

        Vector3 baseScale = GetBaseScale();
        transform.DOScale(baseScale, selectScaleDuration)
                 .SetEase(Ease.OutQuad);
    }
}