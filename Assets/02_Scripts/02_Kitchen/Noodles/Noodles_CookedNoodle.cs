using System.Collections;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Noodles_CookedNoodle : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    private Cooker_PastaCooker pastaCooker;
    private Coroutine animRoutine;

    public bool isSelected { get; private set; }
    public bool isBeingTrashed { get; private set; } = false;
    public bool CanBeSelected => !isLocked && !isBeingTrashed;

    private bool isLocked = true;

    [Header("기본 스프라이트")]
    [SerializeField] private Sprite normalSprite;

    [Header("면 선택 연출")]
    [SerializeField] private Vector3 normalScale = new Vector3(0.7f, 0.7f, 1f);
    [SerializeField] private Vector3 selectedScale = new Vector3(0.82f, 0.82f, 1f);
    [SerializeField] private Vector3 selectedLocalOffset = new Vector3(0f, 0.12f, 0f);
    [SerializeField] private float animDuration = 0.2f;

    [Header("드래그")]
    [SerializeField] private float dragScaleMultiplier = 1.08f;

    [Header("쓰레기 이펙트")]
    [SerializeField] private float trashEffectDuration = 0.22f;
    [SerializeField] private float trashFinalScaleMultiplier = 0.2f;
    [SerializeField] private Vector3 trashFadeOffset = new Vector3(0f, -0.15f, 0f);

    private Vector3 originalLocalPos;
    private Collider[] cachedColliders;
    private IngredientIDs ingredientIDComp;

    private bool isDragging = false;
    private Transform dragStartParent;
    private Vector3 dragStartWorldPos;
    private Vector3 dragStartLocalPos;
    private Vector3 dragOffset;
    private float dragScreenZ;
    private int originalSortingOrder;

    public void SetPastaCooker(Cooker_PastaCooker cooker)
    {
        pastaCooker = cooker;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalLocalPos = transform.localPosition;
        cachedColliders = GetComponentsInChildren<Collider>(true);
        ingredientIDComp = GetComponent<IngredientIDs>();
    }

    void Start()
    {
        sr.sprite = normalSprite;
        transform.localScale = normalScale;
        transform.localPosition = originalLocalPos;
    }

    public bool Interact(IInteractable target)
    {
        if (isLocked || isBeingTrashed) return false;

        // 이제 클릭 선택 방식 안 씀
        if (target == null)
            return false;

        return false;
    }

    public void Unlock()
    {
        isLocked = false;
    }

    public float GetCost()
    {
        if (IngredientDatabase.Instance == null || ingredientIDComp == null)
            return 0f;

        IngredientData data = IngredientDatabase.Instance.GetIngredient(ingredientIDComp.GetID());
        if (data == null)
            return 0f;

        return data.ingredientCost;
    }

    public void OnTrashed()
    {
        isBeingTrashed = true;
        isSelected = false;
        isLocked = true;
        isDragging = false;

        if (animRoutine != null)
        {
            StopCoroutine(animRoutine);
            animRoutine = null;
        }

        foreach (var col in cachedColliders)
        {
            if (col != null)
                col.enabled = false;
        }

        if (sr != null)
            sr.color = Color.white;
    }

    public void PlayTrashEffect(Transform trashTarget)
    {
        transform.DOKill();

        if (trashTarget != null)
            transform.position = trashTarget.position + trashFadeOffset;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            transform.DOScale(normalScale * trashFinalScaleMultiplier, trashEffectDuration)
                     .SetEase(Ease.InQuad)
        );

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            if (r != null)
                seq.Join(r.DOFade(0f, trashEffectDuration));
        }

        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    public void Cancel()
    {
        isSelected = false;
        PlayNoodleAnimation(false);
    }

    private void PlayNoodleAnimation(bool selected)
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        Vector3 targetScale = selected ? selectedScale : normalScale;
        Vector3 targetPos = selected ? originalLocalPos + selectedLocalOffset : originalLocalPos;

        animRoutine = StartCoroutine(AnimateNoodle(targetScale, targetPos));
    }

    private IEnumerator AnimateNoodle(Vector3 targetScale, Vector3 targetPos)
    {
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.localPosition;

        float time = 0f;

        while (time < animDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / animDuration);
            t = t * t * (3f - 2f * t);

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        transform.localScale = targetScale;
        transform.localPosition = targetPos;
        animRoutine = null;
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
        if (isLocked || isBeingTrashed)
            return;

        if (Camera.main == null)
            return;

        if (animRoutine != null)
        {
            StopCoroutine(animRoutine);
            animRoutine = null;
        }

        isDragging = true;
        isSelected = false;

        dragStartParent = transform.parent;
        dragStartWorldPos = transform.position;
        dragStartLocalPos = transform.localPosition;
        dragScreenZ = Camera.main.WorldToScreenPoint(transform.position).z;
        dragOffset = transform.position - GetMouseWorldPosition();

        transform.DOKill();
        transform.localScale = normalScale * dragScaleMultiplier;

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
            transform.SetParent(dragStartParent, true);
            transform.position = dragStartWorldPos;
            transform.localPosition = dragStartLocalPos;
            transform.localScale = normalScale;
        }

        foreach (var col in cachedColliders)
        {
            if (col != null)
                col.enabled = !isBeingTrashed;
        }

        if (sr != null && !isBeingTrashed)
            sr.sortingOrder = originalSortingOrder;
    }

    private bool TryDropTarget()
    {
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Debug.Log("면 드롭 실패: 아무 콜라이더도 맞지 않음");
            return false;
        }

        Debug.Log("면 드롭 시 맞은 오브젝트: " + hit.collider.name);

        Cooker_FryingPan fryingPan = hit.collider.GetComponentInParent<Cooker_FryingPan>();
        if (fryingPan != null)
        {
            Debug.Log("후라이팬 감지됨");
            return fryingPan.Interact(this);
        }

        Cooker_Trashcan trashcan = hit.collider.GetComponentInParent<Cooker_Trashcan>();
        if (trashcan != null)
        {
            Debug.Log("쓰레기통 감지됨");
            return trashcan.Interact(this);
        }

        Debug.Log("면 드롭 실패: 후라이팬/쓰레기통이 아님");
        return false;
    }
}