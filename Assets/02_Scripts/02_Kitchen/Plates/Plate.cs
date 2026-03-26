using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;
using static Topping;

public class Plate : MonoBehaviour, IInteractable
{
    [Header("<<기본 그릇 스프라이트>>")]
    [SerializeField] private Sprite original501Sprite;
    [SerializeField] private Sprite selected501Sprite;

    [Header("<<오븐 그릇 스프라이트>>")]
    [SerializeField] private Sprite original502Sprite;
    [SerializeField] private Sprite selected502Sprite;

    [Header("<<드래그용 오브젝트(GameObject 자식)>>")]
    [SerializeField] private GameObject dragPlateObject;

    [Header("<<드래그 설정>>")]
    [SerializeField] private Vector3 mouseFollowOffset = new Vector3(0.2f, -0.1f, 0f);
    [SerializeField] private float cancelDuration = 0.2f;

    private SpriteRenderer sr;

    // 드래그용 오브젝트 내부 참조
    private Transform dragPlateTransform;
    private Sprite dragOriginalSprite;
    private SpriteRenderer dragPlateRenderer;

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

    public PlateType plateType;
    public enum PlateType
    {
        BasicPlate,
        OvenPlate,
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        isSelected = false;

        if (plateType == PlateType.BasicPlate)
        {
            if (original501Sprite != null)
                sr.sprite = original501Sprite;
        }
        else if (plateType == PlateType.OvenPlate)
        {
            if (original502Sprite != null)
                sr.sprite = original502Sprite;
        }

        // 자동 찾기
        if (dragPlateObject == null)
        {
            Transform child = transform.Find("DragPlateVisual");
            if (child != null)
                dragPlateObject = child.gameObject;
        }

        if (dragPlateObject != null)
        {
            dragPlateTransform = dragPlateObject.transform;
            dragPlateRenderer = dragPlateObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogWarning($"{name}: dragPlateObject가 연결되지 않았습니다.");
        }

        InitDragVisual();
        ResetDragVisual();
    }

    private void InitDragVisual()
    {
        if (dragPlateObject == null || dragPlateTransform == null)
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
        // Plate는 이제 클릭 선택형이 아니라 드래그형
        return false;
    }

    private void OnMouseDown()
    {
        Debug.Log($"[Plate] OnMouseDown 들어옴: {name}");

        if (isAnimating || isDragging)
        {
            Debug.Log("[Plate] 막힘: isAnimating 또는 isDragging");
            return;
        }

        if (Camera.main == null)
        {
            Debug.Log("[Plate] 막힘: Camera.main 없음");
            return;
        }

        if (dragPlateObject == null)
        {
            Debug.Log("[Plate] 막힘: dragPlateObject null");
            return;
        }

        if (dragPlateTransform == null)
            dragPlateTransform = dragPlateObject.transform;

        if (dragPlateRenderer == null)
            dragPlateRenderer = dragPlateObject.GetComponent<SpriteRenderer>();

        KillDragSequence();

        isDragging = true;
        isSelected = true;

        dragPlateObject.SetActive(true);
        dragPlateTransform.SetParent(null, true);

        if (dragPlateRenderer != null)
        {
            Color c = dragOriginalColor;
            c.a = 1f;
            dragPlateRenderer.color = c;
            dragPlateRenderer.sortingOrder = 999;
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
        {
            Debug.Log("Plate 드롭 실패 -> Cancel 실행");
            Cancel();
        }
        else
        {
            CompleteSuccessfulDrag();
        }
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
        {
            Debug.Log("Plate 드롭 실패: 아무 콜라이더도 맞지 않음");
            return false;
        }

        Debug.Log("Plate 드롭 시 맞은 오브젝트: " + hit.collider.name);

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

        Debug.Log("Plate 드롭 실패: 이 Plate를 받는 IInteractable 없음");
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

    public void Cancel()
    {
        if (dragPlateObject == null || dragPlateTransform == null)
        {
            isSelected = false;
            isAnimating = false;
            isDragging = false;
            return;
        }

        KillDragSequence();

        isSelected = false;
        isDragging = false;

        if (!dragPlateObject.activeSelf)
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
        if (dragPlateObject == null || dragPlateTransform == null)
            return;

        if (dragOriginalParent != null)
            dragPlateTransform.SetParent(dragOriginalParent, false);

        dragPlateTransform.localPosition = dragOriginalLocalPos;
        dragPlateTransform.localRotation = dragOriginalLocalRot;
        dragPlateTransform.localScale = dragOriginalLocalScale;

        if (dragPlateRenderer != null)
        {
            if (dragOriginalSprite != null)
                dragPlateRenderer.sprite = dragOriginalSprite;

            Color c = dragOriginalColor;
            c.a = 0f;
            dragPlateRenderer.color = c;

            dragPlateRenderer.sortingOrder = dragOriginalSortingOrder;
            dragPlateRenderer.sortingLayerName = dragOriginalSortingLayerName;
        }

        dragPlateObject.SetActive(false);
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