using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Plate_Pane : MonoBehaviour, IInteractable
{
    [Header("<<빠네 스프라이트>>")]
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite selectedSprite;

    [Header("<<선택 연출>>")]
    [SerializeField] private float selectScaleDuration = 0.12f;
    [SerializeField] private float selectedScaleMultiplier = 1.08f;
    [SerializeField] private float pressedScaleMultiplier = 0.97f;

    [Header("<<드래그 설정>>")]
    [SerializeField] private float dragLiftScaleMultiplier = 1.08f;
    [SerializeField] private float dragStartThreshold = 0.12f;

    private SpriteRenderer sr;
    private Collider[] cachedColliders;
    private Vector3 originalScale;
    private bool isAnimating = false;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    private bool isPointerDown = false;
    private bool hasStartedRealDrag = false;
    private bool isDragging = false;

    private Vector3 dragStartWorldPos;
    private Vector3 dragStartLocalPos;
    private Quaternion dragStartLocalRot;
    private Vector3 dragStartLocalScale;
    private Transform dragStartParent;
    private Vector3 mouseDownWorldPos;
    private Vector3 dragOffset;
    private float dragScreenZ;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cachedColliders = GetComponentsInChildren<Collider>(true);

        originalScale = transform.localScale;
        isSelected = false;

        if (sr != null && originalSprite != null)
            sr.sprite = originalSprite;
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("빠네 선택!");
            Select();
            return true;
        }

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
        if (Camera.main == null)
            return;

        isPointerDown = true;
        hasStartedRealDrag = false;
        isDragging = false;

        dragStartWorldPos = transform.position;
        dragStartLocalPos = transform.localPosition;
        dragStartLocalRot = transform.localRotation;
        dragStartLocalScale = transform.localScale;
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
        hasStartedRealDrag = true;
        isDragging = true;
        isAnimating = false;
        isSelected = true;

        transform.DOKill();

        if (sr != null && selectedSprite != null)
            sr.sprite = selectedSprite;

        transform.localScale = originalScale * dragLiftScaleMultiplier;

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

        isDragging = false;
        hasStartedRealDrag = false;

        bool placed = TryDropTarget();

        // 성공/실패 상관없이 항상 원래 자리로 복귀
        transform.SetParent(dragStartParent, true);
        transform.position = dragStartWorldPos;
        transform.localPosition = dragStartLocalPos;
        transform.localRotation = dragStartLocalRot;
        transform.localScale = originalScale;

        if (cachedColliders != null)
        {
            foreach (var col in cachedColliders)
            {
                if (col != null)
                    col.enabled = true;
            }
        }

        isSelected = false;
        transform.DOKill();

        if (sr != null && originalSprite != null)
            sr.sprite = originalSprite;

        if (!placed)
        {
            Debug.Log("빠네 드롭 실패");
        }
    }

    private bool TryDropTarget()
    {
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            return false;

        Cooker_Oven oven = hit.collider.GetComponentInParent<Cooker_Oven>();
        if (oven != null)
        {
            Debug.Log("Oven 감지됨");
            return oven.Interact(this);
        }

        return false;
    }

    void Select()
    {
        if (isAnimating) return;
        if (isSelected) return;

        isAnimating = true;
        isSelected = true;

        transform.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * pressedScaleMultiplier, 0.08f));
        seq.AppendCallback(() =>
        {
            if (sr != null && selectedSprite != null)
                sr.sprite = selectedSprite;
        });
        seq.Append(transform.DOScale(originalScale * selectedScaleMultiplier, selectScaleDuration)
            .SetEase(Ease.OutBack));
        seq.OnComplete(() => isAnimating = false);
    }

    public void Cancel()
    {
        if (isDragging) return;
        if (isAnimating) return;
        if (!isSelected) return;

        isAnimating = true;
        isSelected = false;

        transform.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * pressedScaleMultiplier, 0.08f));
        seq.AppendCallback(() =>
        {
            if (sr != null && originalSprite != null)
                sr.sprite = originalSprite;
        });
        seq.Append(transform.DOScale(originalScale, selectScaleDuration)
            .SetEase(Ease.OutQuad));
        seq.OnComplete(() => isAnimating = false);
    }
}