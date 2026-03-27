using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;
using DG.Tweening;


public class Burned : MonoBehaviour, IInteractable
{
    [Header("<<드래그 이동>>")]
    [SerializeField] private float dragLiftScaleMultiplier = 1.08f;
    [SerializeField] private float dragStartThreshold = 0.12f;

    private bool isPointerDown = false;
    private bool hasStartedRealDrag = false;

    private Vector3 dragStartWorldPos;
    private Vector3 dragStartLocalPos;
    private Vector3 dragOffset;
    private float dragScreenZ;
    private Vector3 mouseDownWorldPos;

    private Collider[] cachedColliders;

    public enum BurnedType
    {
        Pane,
        Pasta
    }

    public BurnedType type;

    private HashSet<int> ingredientIDs = new HashSet<int>();

    public bool isBeingTrashed { get; private set; } = false;
    public bool CanBeSelected => true;

    private Vector3 originalScale;
    [SerializeField] private float selectedScaleMultiplier = 1.2f;

    void Start()
    {
        originalScale = transform.localScale;
        cachedColliders = GetComponentsInChildren<Collider>(true);
    }

    public void SetIngredients(HashSet<int> ids)
    {
        if (ids != null)
            ingredientIDs = new HashSet<int>(ids);
    }

    public HashSet<int> GetIngredientSet()
    {
        return ingredientIDs;
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
        if (isBeingTrashed || !CanBeSelected)
            return;

        if (Camera.main == null)
            return;

        isPointerDown = true;
        hasStartedRealDrag = false;

        dragStartWorldPos = transform.position;
        dragStartLocalPos = transform.localPosition;
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
                hasStartedRealDrag = true;
                transform.DOKill();
                transform.localScale = originalScale * dragLiftScaleMultiplier;

                if (cachedColliders != null)
                {
                    foreach (var col in cachedColliders)
                    {
                        if (col != null)
                            col.enabled = false;
                    }
                }
            }
        }

        if (!hasStartedRealDrag)
            return;

        transform.position = currentMouseWorld + dragOffset;
    }

    private void OnMouseUp()
    {
        if (!isPointerDown)
            return;

        isPointerDown = false;

        if (!hasStartedRealDrag)
        {
            Select();
            Debug.Log("탄 음식 선택");
            return;
        }

        hasStartedRealDrag = false;

        bool trashed = TryDropTrashcan();

        if (!trashed)
        {
            transform.position = dragStartWorldPos;
            transform.localPosition = dragStartLocalPos;
            transform.localScale = originalScale;

            if (cachedColliders != null)
            {
                foreach (var col in cachedColliders)
                {
                    if (col != null)
                        col.enabled = true;
                }
            }
        }
    }

    private bool TryDropTrashcan()
    {
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            return false;

        Cooker_Trashcan trashcan = hit.collider.GetComponentInParent<Cooker_Trashcan>();
        if (trashcan != null)
            return trashcan.Interact(this);

        return false;
    }

    public float GetCost()
    {
        if (type == BurnedType.Pane)
        {
            return 3f;
        }

        if (IngredientDatabase.Instance == null)
        {
            return 0f;
        }

        float total = 0f;

        foreach (int id in ingredientIDs)
        {
            IngredientData data = IngredientDatabase.Instance.GetIngredient(id);

            if (data != null)
            {
                total += data.ingredientCost;
            }
        }

        return total;
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Select();
            Debug.Log("탄 음식 선택");
            return true;
        }

        return false;
    }

    private void Select()
    {
        transform.localScale = originalScale * selectedScaleMultiplier;
    }

    public void OnTrashed()
    {
        isBeingTrashed = true;
        isPointerDown = false;
        hasStartedRealDrag = false;
    }


    public void PlayTrashEffect(Transform trashTarget)
    {
        float moveDuration = 0.9f;
        float effectDuration = 0.31f;

        Vector3 targetPos = trashTarget.position;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMove(targetPos, moveDuration)
            .SetEase(Ease.OutQuad));

        seq.Join(transform.DOScale(Vector3.zero, moveDuration)
            .SetEase(Ease.InQuad));


        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        seq.AppendCallback(() =>
        {

        });

        if (sr != null)
        {
            seq.Append(sr.DOFade(0f, effectDuration));
        }

        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void OnDestroy()
    {
        if (!isBeingTrashed) return;

        Cooker_Oven oven = GetComponentInParent<Cooker_Oven>();

        if (oven != null)
        {
            oven.OnBurnedRemoved();
        }
    }


    public void Cancel()
    {
        isPointerDown = false;
        hasStartedRealDrag = false;
        transform.localScale = originalScale;

        if (cachedColliders != null)
        {
            foreach (var col in cachedColliders)
            {
                if (col != null)
                    col.enabled = true;
            }
        }
    }

}
