using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Noodles : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;

    [Header("스프라이트")]
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite selectedSprite;

    [Header("드래그용 오브젝트(GameObject 자식)")]
    [SerializeField] private GameObject dragNoodleObject;

    [Header("드래그 설정")]
    [SerializeField] private Vector3 mouseFollowOffset = new Vector3(0.2f, -0.1f, 0f);
    [SerializeField] private float cancelDuration = 0.2f;

    private Transform dragNoodleTransform;
    private SpriteRenderer dragNoodleRenderer;
    private Sprite dragOriginalSprite;

    private bool isAnimating = false;
    private bool isDragging = false;

    private Transform dragOriginalParent;
    private Vector3 dragOriginalLocalPos;
    private Quaternion dragOriginalLocalRot;
    private Vector3 dragOriginalLocalScale;

    private Color dragOriginalColor;
    private int dragOriginalSortingOrder;
    private string dragOriginalSortingLayerName;

    private float dragScreenZ;
    private Sequence dragSequence;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => !isAnimating && !isDragging;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        isSelected = false;

        if (sr != null && originalSprite != null)
            sr.sprite = originalSprite;

        // 자동 찾기
        if (dragNoodleObject == null)
        {
            Transform child = transform.Find("DragNoodleVisual");
            if (child != null)
                dragNoodleObject = child.gameObject;
        }

        if (dragNoodleObject != null)
        {
            dragNoodleTransform = dragNoodleObject.transform;
            dragNoodleRenderer = dragNoodleObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogWarning($"{name}: dragNoodleObject가 연결되지 않았습니다.");
        }

        InitDragVisual();
        ResetDragVisual();
    }

    private void InitDragVisual()
    {
        if (dragNoodleObject == null || dragNoodleTransform == null)
            return;

        dragOriginalParent = dragNoodleTransform.parent;
        dragOriginalLocalPos = dragNoodleTransform.localPosition;
        dragOriginalLocalRot = dragNoodleTransform.localRotation;
        dragOriginalLocalScale = dragNoodleTransform.localScale;

        if (dragNoodleRenderer != null)
        {
            dragOriginalColor = dragNoodleRenderer.color;
            dragOriginalSortingOrder = dragNoodleRenderer.sortingOrder;
            dragOriginalSortingLayerName = dragNoodleRenderer.sortingLayerName;
            dragOriginalSprite = dragNoodleRenderer.sprite;
        }
    }

    public bool Interact(IInteractable target)
    {
        // Noodles도 이제 클릭 선택형이 아니라 드래그형
        return false;
    }

    private void OnMouseDown()
    {
        if (isAnimating || isDragging)
            return;

        if (Camera.main == null)
            return;

        if (dragNoodleObject == null)
        {
            Debug.LogWarning($"{name}: dragNoodleObject가 null입니다.");
            return;
        }

        if (dragNoodleTransform == null)
            dragNoodleTransform = dragNoodleObject.transform;

        if (dragNoodleRenderer == null)
            dragNoodleRenderer = dragNoodleObject.GetComponent<SpriteRenderer>();

        KillDragSequence();

        isDragging = true;
        isSelected = true;

        if (sr != null && selectedSprite != null)
            sr.sprite = selectedSprite;

        dragNoodleObject.SetActive(true);
        dragNoodleTransform.SetParent(null, true);

        if (dragNoodleRenderer != null)
        {
            if (dragOriginalSprite != null)
                dragNoodleRenderer.sprite = dragOriginalSprite;

            Color c = dragOriginalColor;
            c.a = 1f;
            dragNoodleRenderer.color = c;
            dragNoodleRenderer.sortingOrder = 999;
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
        if (dragNoodleTransform == null || Camera.main == null)
            return;

        Vector3 mouse = Input.mousePosition;
        mouse.z = dragScreenZ;

        Vector3 world = Camera.main.ScreenToWorldPoint(mouse);
        world.z = transform.position.z;

        dragNoodleTransform.position = world + mouseFollowOffset;
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
            if (behaviour == null) continue;
            if (behaviour.gameObject == gameObject) continue;

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

        if (sr != null && originalSprite != null)
            sr.sprite = originalSprite;

        ResetDragVisual();
    }

    private void ResetDragVisual()
    {
        if (dragNoodleObject == null || dragNoodleTransform == null)
            return;

        if (dragOriginalParent != null)
            dragNoodleTransform.SetParent(dragOriginalParent, false);

        dragNoodleTransform.localPosition = dragOriginalLocalPos;
        dragNoodleTransform.localRotation = dragOriginalLocalRot;
        dragNoodleTransform.localScale = dragOriginalLocalScale;

        if (dragNoodleRenderer != null)
        {
            if (dragOriginalSprite != null)
                dragNoodleRenderer.sprite = dragOriginalSprite;
            else if (originalSprite != null)
                dragNoodleRenderer.sprite = originalSprite;

            Color c = dragOriginalColor;
            c.a = 0f;
            dragNoodleRenderer.color = c;

            dragNoodleRenderer.sortingOrder = dragOriginalSortingOrder;
            dragNoodleRenderer.sortingLayerName = dragOriginalSortingLayerName;
        }

        dragNoodleObject.SetActive(false);
    }

    private void KillDragSequence()
    {
        if (dragSequence != null)
        {
            dragSequence.Kill();
            dragSequence = null;
        }
    }

    public void Cancel()
    {
        if (dragNoodleObject == null || dragNoodleTransform == null)
        {
            isSelected = false;
            isAnimating = false;
            isDragging = false;
            return;
        }

        KillDragSequence();

        isSelected = false;
        isDragging = false;

        if (!dragNoodleObject.activeSelf)
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
            dragNoodleTransform.DOMove(homeWorldPos, cancelDuration)
                .SetEase(Ease.InSine)
        );

        if (dragNoodleRenderer != null)
        {
            dragSequence.Join(
                dragNoodleRenderer.DOFade(0f, cancelDuration)
            );
        }

        dragSequence.OnComplete(() =>
        {
            if (sr != null && originalSprite != null)
                sr.sprite = originalSprite;

            ResetDragVisual();
            isAnimating = false;
        });
    }
}