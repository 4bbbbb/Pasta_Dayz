using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Plate_BakedPane : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    private Vector3 originalScale;

    public bool isSelected { get; private set; }
    public bool isBeingTrashed { get; private set; } = false;

    private bool canPick = false;
    public bool CanBeSelected => canPick;

    [SerializeField] private float selectedScaleMultiplier = 1.2f;
    [SerializeField] private float paneCost = 3f;   // Burned Pane이랑 동일하게 3원 처리

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        // 처음 생성 시 클릭 막기
        SetPickable(false);
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("구워진 빠네 선택!");
            Select();
            return true;
        }

        return false;
    }

    public void SetPickable(bool value)
    {
        canPick = value;

        Collider[] cols = GetComponentsInChildren<Collider>(true);
        foreach (var col in cols)
        {
            col.enabled = value;
        }

        Debug.Log($"[Pane] Pickable = {value}, Collider 개수 = {cols.Length}");
    }

    public float GetCost()
    {
        return paneCost;
    }

    public void OnTrashed()
    {
        isBeingTrashed = true;
        isSelected = false;
        SetPickable(false);
    }

    public void PlayTrashEffect(Transform trashTarget)
    {
        float moveDuration = 0.9f;
        float fadeDuration = 0.31f;

        Vector3 targetPos = trashTarget.position;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMove(targetPos, moveDuration)
            .SetEase(Ease.OutQuad));

        seq.Join(transform.DOScale(Vector3.zero, moveDuration)
            .SetEase(Ease.InQuad));

        if (sr != null)
        {
            seq.Append(sr.DOFade(0f, fadeDuration));
        }

        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void Select()
    {
        isSelected = true;
        transform.localScale = originalScale * selectedScaleMultiplier;

        if (sr != null)
            sr.color = Color.red;
    }

    public void Cancel()
    {
        isSelected = false;
        transform.localScale = originalScale;

        if (sr != null)
            sr.color = Color.white;
    }
}