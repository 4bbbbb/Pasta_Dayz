using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Cheese : MonoBehaviour, IInteractable
{
    [Header("<<파마산치즈 스프라이트>>")]
    [SerializeField] private Sprite parmesanSprite;
    [SerializeField] private Sprite parmesanselectedSprite;

    [Header("<<파마산 선택 연출>>")]
    [SerializeField] private float selectScaleDuration = 0.12f;
    [SerializeField] private float selectedScaleMultiplier = 1.08f;
    [SerializeField] private float pressedScaleMultiplier = 0.97f;

    [Header("<<파마산 드래그>>")]
    [SerializeField] private float dragScaleMultiplier = 1.08f;

    private SpriteRenderer sr;
    private Vector3 originalScale;
    private bool isAnimating = false;

    private Collider[] cachedColliders;

    // 드래그용
    private bool isDragging = false;
    private Transform originalParent;
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;

    private Transform dragStartParent;
    private Vector3 dragStartWorldPos;
    private Vector3 dragStartLocalPos;
    private Quaternion dragStartLocalRot;

    private Vector3 dragOffset;
    private float dragScreenZ;

    private int originalSortingOrder;
    private string originalSortingLayerName;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    public CheeseType cheeseType;
    public enum CheeseType
    {
        Parmesan,
        Mozzarella,
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        cachedColliders = GetComponentsInChildren<Collider>(true);

        originalParent = transform.parent;
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
    }

    public bool Interact(IInteractable target)
    {
        // 파마산은 클릭선택이 아니라 드래그형
        if (cheeseType == CheeseType.Parmesan)
            return false;

        // 모짜렐라는 기존 방식 유지
        if (target == null)
        {
            Select();
            return true;
        }

        return false;
    }

    void Select()
    {
        if (isAnimating) return;
        if (isSelected) return;

        isSelected = true;

        if (cheeseType == CheeseType.Parmesan)
        {
            isAnimating = true;
            transform.DOKill();

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(originalScale * pressedScaleMultiplier, 0.08f));
            seq.AppendCallback(() =>
            {
                if (sr != null && parmesanselectedSprite != null)
                    sr.sprite = parmesanselectedSprite;
            });
            seq.Append(transform.DOScale(originalScale * selectedScaleMultiplier, selectScaleDuration)
                .SetEase(Ease.OutBack));
            seq.OnComplete(() => isAnimating = false);
        }
        else
        {
            sr.color = Color.red;
        }
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
        if (cheeseType != CheeseType.Parmesan)
            return;

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
            if (parmesanselectedSprite != null)
                sr.sprite = parmesanselectedSprite;

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
        if (cheeseType != CheeseType.Parmesan)
            return;

        if (!isDragging)
            return;

        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        if (cheeseType != CheeseType.Parmesan)
            return;

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
            Debug.Log("파마산 드롭 실패: 아무 콜라이더도 맞지 않음");
            return false;
        }

        Debug.Log("파마산 드롭 시 맞은 오브젝트: " + hit.collider.name);

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

        Debug.Log("파마산 드롭 실패: FinishedPasta / BakedPasta 아님");
        return false;
    }

    public void Sprinkle(Transform pastaPoint, System.Action onSprinkle)
    {
        if (cheeseType != CheeseType.Parmesan)
            return;

        if (isAnimating)
            return;

        isAnimating = true;
        isSelected = true;

        transform.DOKill();

        // 드래그해서 놓은 현재 위치에서 바로 뿌리기
        Vector3 currentDropPos = transform.position;
        Quaternion currentDropRot = transform.rotation;
        Quaternion pourRot = Quaternion.Euler(0f, 0f, 120f);

        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            if (sr != null && parmesanselectedSprite != null)
                sr.sprite = parmesanselectedSprite;
        });

        // 제자리에서 기울이기
        seq.Append(transform.DORotateQuaternion(pourRot, 0.2f));

        // 기존 프리팹 생성 코드 실행
        seq.AppendCallback(() =>
        {
            onSprinkle?.Invoke();
        });

        // 제자리에서 흔들기
        seq.Append(
            transform.DOMoveY(currentDropPos.y + 0.08f, 0.18f)
            .SetLoops(4, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
        );

        // 다시 세우기
        seq.Append(transform.DORotateQuaternion(currentDropRot, 0.2f));

        // 원위치 복귀
        seq.Append(transform.DOMove(dragStartWorldPos, 0.35f).SetEase(Ease.InQuad));

        seq.AppendCallback(() =>
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

            if (cheeseType == CheeseType.Parmesan && parmesanSprite != null)
                sr.sprite = parmesanSprite;
        }
    }

    public void Cancel()
    {
        if (cheeseType == CheeseType.Parmesan)
        {
            if (isAnimating) return;

            isSelected = false;
            isDragging = false;

            transform.DOKill();
            RestoreToStartImmediate();
            RestoreVisualState();
        }
        else
        {
            isSelected = false;
            sr.color = Color.white;
        }
    }
}