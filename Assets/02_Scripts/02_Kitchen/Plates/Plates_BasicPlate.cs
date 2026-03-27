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

    [Header("<<드래그 설정>>")]
    [SerializeField] private Vector3 mouseFollowOffset = new Vector3(0.2f, -0.1f, 0f);
    [SerializeField] private float cancelDuration = 0.2f;

    private SpriteRenderer sr;
    private Collider plateCollider;

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

    // 드래그용 내부 참조
    private GameObject runtimeDragVisual;
    private Transform dragPlateTransform;
    private SpriteRenderer dragPlateRenderer;

    private Transform dragOriginalParent;
    private Vector3 dragOriginalLocalPos;
    private Quaternion dragOriginalLocalRot;
    private Vector3 dragOriginalLocalScale;

    private Color dragOriginalColor;
    private int dragOriginalSortingOrder;
    private string dragOriginalSortingLayerName;
    private Sprite dragOriginalSprite;

    private float dragScreenZ;
    private Sequence dragSequence;

    private bool isAnimating = false;
    private bool isDragging = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        plateCollider = GetComponent<Collider>();
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

        CreateRuntimeDragVisual();
        InitDragVisual();
        ResetDragVisual();
    }

    private void OnDisable()
    {
        KillDragSequence();
        transform.DOKill();

        if (runtimeDragVisual != null)
            Destroy(runtimeDragVisual);
    }

    private void CreateRuntimeDragVisual()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        runtimeDragVisual = new GameObject($"{name}_DragVisual");
        dragPlateTransform = runtimeDragVisual.transform;
        dragPlateRenderer = runtimeDragVisual.AddComponent<SpriteRenderer>();

        dragPlateRenderer.sprite = sr != null ? sr.sprite : null;
        dragPlateRenderer.sortingLayerName = sr != null ? sr.sortingLayerName : "Default";
        dragPlateRenderer.sortingOrder = sr != null ? sr.sortingOrder : 0;
        dragPlateRenderer.color = new Color(1f, 1f, 1f, 0f);

        dragPlateTransform.SetParent(transform, false);
        dragPlateTransform.localPosition = Vector3.zero;
        dragPlateTransform.localRotation = Quaternion.identity;
        dragPlateTransform.localScale = Vector3.one;

        runtimeDragVisual.SetActive(false);
    }

    private void InitDragVisual()
    {
        if (runtimeDragVisual == null || dragPlateTransform == null)
            return;

        dragOriginalParent = dragPlateTransform.parent;
        dragOriginalLocalPos = dragPlateTransform.localPosition;
        dragOriginalLocalRot = dragPlateTransform.localRotation;
        dragOriginalLocalScale = dragPlateTransform.localScale;

        if (dragPlateRenderer != null)
        {
            dragOriginalColor = dragPlateRenderer.color;
            dragOriginalSortingOrder = dragPlateRenderer.sortingOrder;
            dragOriginalSortingLayerName = dragPlateRenderer.sortingLayerName;
            dragOriginalSprite = dragPlateRenderer.sprite;
        }
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

            // 플레이트 정보 먼저 합쳐줌
            HashSet<int> finalIngredients = new HashSet<int>(ingredients);
            foreach (int id in finishedPasta.GetIngredientSet())
            {
                finalIngredients.Add(id);
            }

            finishedPasta.SetIngredients(finalIngredients);

            // 원래 잘 되던 배치 방식 유지
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

            // 혹시 OnMovedToPlate 내부에서 transform 건드렸어도
            // 마지막으로 한 번 더 스폰포인트 기준으로 고정
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
        if (isBeingTrashed || isAnimating || isDragging)
            return;

        if (hasPasta)
            return;

        if (Camera.main == null)
            return;

        if (runtimeDragVisual == null)
            CreateRuntimeDragVisual();

        if (dragPlateTransform == null)
            dragPlateTransform = runtimeDragVisual.transform;

        if (dragPlateRenderer == null)
            dragPlateRenderer = runtimeDragVisual.GetComponent<SpriteRenderer>();

        KillDragSequence();

        isDragging = true;
        isSelected = true;

        if (runtimeDragVisual == null)
            CreateRuntimeDragVisual();

        if (dragPlateTransform == null)
            dragPlateTransform = runtimeDragVisual.transform;

        if (dragPlateRenderer == null)
            dragPlateRenderer = runtimeDragVisual.GetComponent<SpriteRenderer>(); dragPlateTransform.SetParent(null, true);

        if (dragPlateRenderer != null)
        {
            Color c = dragOriginalColor;
            c.a = 1f;
            dragPlateRenderer.color = c;
            dragPlateRenderer.sortingOrder = 999;
            dragPlateRenderer.sortingLayerName = dragOriginalSortingLayerName;
        }

        dragScreenZ = Camera.main.WorldToScreenPoint(transform.position).z;
        UpdateDragVisualPosition();
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        UpdateDragVisualPosition();
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;

        bool droppedSuccessfully = TryDropTarget();

        if (!droppedSuccessfully)
            Cancel();
        else
            CompleteSuccessfulDrag();
    }

    private void UpdateDragVisualPosition()
    {
        if (dragPlateTransform == null || Camera.main == null)
            return;

        Vector3 mouse = Input.mousePosition;
        mouse.z = dragScreenZ;

        Vector3 world = Camera.main.ScreenToWorldPoint(mouse);
        world.z = transform.position.z;

        dragPlateTransform.position = world + mouseFollowOffset;
    }

    private bool TryDropTarget()
    {
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            return false;

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

        return false;
    }

    private void CompleteSuccessfulDrag()
    {
        KillDragSequence();

        isSelected = false;
        isAnimating = false;
        isDragging = false;

        ResetDragVisual();
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

        transform.SetParent(null, true);   // 추가

        if (plateCollider != null)
            plateCollider.enabled = false;

        ResetDragVisual();
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

    public void Cancel()
    {
        if (runtimeDragVisual == null || dragPlateTransform == null)
        {
            isSelected = false;
            isAnimating = false;
            isDragging = false;
            return;
        }

        KillDragSequence();

        isSelected = false;
        isDragging = false;

        if (!runtimeDragVisual.activeSelf)
        {
            isAnimating = false;
            return;
        }

        isAnimating = true;

        Vector3 homeWorldPos = dragOriginalParent != null
            ? dragOriginalParent.TransformPoint(dragOriginalLocalPos)
            : dragOriginalLocalPos;

        dragSequence = DOTween.Sequence();

        dragSequence.Append(
            dragPlateTransform.DOMove(homeWorldPos, cancelDuration)
                .SetEase(Ease.InSine)
        );

        if (dragPlateRenderer != null)
        {
            dragSequence.Join(
                dragPlateRenderer.DOFade(0f, cancelDuration)
            );
        }

        dragSequence.OnComplete(() =>
        {
            ResetDragVisual();
            isAnimating = false;
        });
    }

    private void ResetDragVisual()
    {
        if (runtimeDragVisual == null || dragPlateTransform == null)
            return;

        if (dragOriginalParent != null)
            dragPlateTransform.SetParent(dragOriginalParent, false);

        dragPlateTransform.localPosition = dragOriginalLocalPos;
        dragPlateTransform.localRotation = dragOriginalLocalRot;
        dragPlateTransform.localScale = dragOriginalLocalScale;

        if (dragPlateRenderer != null)
        {
            if (sr != null)
                dragPlateRenderer.sprite = sr.sprite;
            else if (dragOriginalSprite != null)
                dragPlateRenderer.sprite = dragOriginalSprite;

            Color c = dragOriginalColor;
            c.a = 0f;
            dragPlateRenderer.color = c;

            dragPlateRenderer.sortingOrder = dragOriginalSortingOrder;
            dragPlateRenderer.sortingLayerName = dragOriginalSortingLayerName;
        }

        runtimeDragVisual.SetActive(false);
    }

    private void KillDragSequence()
    {
        if (dragSequence != null)
        {
            dragSequence.Kill();
            dragSequence = null;
        }
    }
}