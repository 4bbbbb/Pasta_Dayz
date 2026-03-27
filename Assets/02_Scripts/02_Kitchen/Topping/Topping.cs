using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Topping : MonoBehaviour, IInteractable
{
    public bool isOliveOil;

    private SpriteRenderer sr;
    private IngredientIDs ingredientIDs;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    public ToppingType toppingType;

    [Header("<<드래그 비주얼>>")]
    [SerializeField] private float dragStartThresholdPixels = 12f;
    [SerializeField] private float dragVisualScaleMultiplier = 3f;
    [SerializeField] private float dragVisualAlpha = 0.95f;

    private bool isPointerDown = false;
    private bool isDragging = false;
    private Vector3 mouseDownScreenPos;
    private float dragScreenZ;

    private GameObject dragVisual;
    private SpriteRenderer dragVisualRenderer;

    public enum ToppingType
    {
        Tomato,
        Garlic,
        Barsil,
        Onion,
        Mushroom,
        Bacon,
        Pepperoncino,
        Shrimp,
        Clams,
        Broccoli,
        Mussel,
        Meatball,
        Sausage,
    }

    static ToppingType GetToppingType(int id)
    {
        if (id < 301 || id > 313)
        {
            if (id != 402)
                Debug.Log($"{id}번 토핑은 존재하지 않습니다.");
            return ToppingType.Tomato;
        }
        return (ToppingType)(id - 301);
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        ingredientIDs = GetComponent<IngredientIDs>();
        isSelected = false;
    }

    public void Initialize(IngredientDatabase.IngredientIconData data)
    {
        ingredientIDs.ingredientID = data.id;
        toppingType = GetToppingType(data.id);
        sr.sprite = data.icon;

        if (data.id == 402)
        {
            Cheese cheese = gameObject.AddComponent<Cheese>();
            cheese.cheeseType = Cheese.CheeseType.Mozzarella;
            Destroy(this);
        }
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log($"{name} 선택!");
            Select();
            return true;
        }

        return false;
    }

    void Select()
    {
        isSelected = true;
    }

    public void Cancel()
    {
        isSelected = false;
    }

    private void OnMouseDown()
    {
        if (Camera.main == null)
            return;

        isPointerDown = true;
        isDragging = false;
        mouseDownScreenPos = Input.mousePosition;
        dragScreenZ = Camera.main.WorldToScreenPoint(transform.position).z;
    }

    private void OnMouseDrag()
    {
        if (!isPointerDown)
            return;

        if (!isDragging)
        {
            float dragDistance = Vector3.Distance(Input.mousePosition, mouseDownScreenPos);
            if (dragDistance >= dragStartThresholdPixels)
            {
                BeginDrag();
            }
        }

        if (!isDragging)
            return;

        UpdateDragVisualPosition();
    }

    private void OnMouseUp()
    {
        if (!isPointerDown)
            return;

        isPointerDown = false;

        if (!isDragging)
        {
            Select();
            Debug.Log($"{name} 선택!");
            return;
        }

        TryDropToFryingPan();
        EndDrag();
    }

    private void BeginDrag()
    {
        isDragging = true;
        Cancel();
        CreateDragVisual();
        UpdateDragVisualPosition();
    }

    private void EndDrag()
    {
        isDragging = false;
        DestroyDragVisual();
    }

    private void CreateDragVisual()
    {
        DestroyDragVisual();

        dragVisual = new GameObject($"{name}_DragVisual");
        dragVisualRenderer = dragVisual.AddComponent<SpriteRenderer>();

        dragVisualRenderer.sprite = GetDragSprite();
        dragVisualRenderer.sortingLayerName = sr != null ? sr.sortingLayerName : "Default";
        dragVisualRenderer.sortingOrder = 9999;

        Color c = Color.white;
        c.a = dragVisualAlpha;
        dragVisualRenderer.color = c;

        dragVisual.transform.rotation = transform.rotation;
        dragVisual.transform.localScale = transform.lossyScale * dragVisualScaleMultiplier;
    }

    private void DestroyDragVisual()
    {
        if (dragVisual != null)
            Destroy(dragVisual);

        dragVisual = null;
        dragVisualRenderer = null;
    }

    private void UpdateDragVisualPosition()
    {
        if (dragVisual == null || Camera.main == null)
            return;

        Vector3 mouse = Input.mousePosition;
        mouse.z = dragScreenZ;

        Vector3 world = Camera.main.ScreenToWorldPoint(mouse);
        world.z = transform.position.z;

        dragVisual.transform.position = world;
    }

    private Sprite GetDragSprite()
    {
        if (ingredientIDs != null && IngredientDatabase.Instance != null)
        {
            GameObject prefab = IngredientDatabase.Instance.GetPrefab(ingredientIDs.GetID());
            if (prefab != null)
            {
                SpriteRenderer prefabSR = prefab.GetComponentInChildren<SpriteRenderer>();
                if (prefabSR != null && prefabSR.sprite != null)
                    return prefabSR.sprite;
            }
        }

        return sr != null ? sr.sprite : null;
    }

    private void TryDropToFryingPan()
    {
        if (Camera.main == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            return;

        Cooker_FryingPan pan = hit.collider.GetComponentInParent<Cooker_FryingPan>();
        if (pan != null)
        {
            pan.Interact(this);
            Cancel();
        }
    }

    private void OnDisable()
    {
        isPointerDown = false;
        isDragging = false;
        DestroyDragVisual();
    }

    private void OnDestroy()
    {
        DestroyDragVisual();
    }
}