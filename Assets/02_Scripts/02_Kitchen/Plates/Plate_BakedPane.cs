using UnityEngine;
using static IInteractableScript;

public class Plate_BakedPane : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;

    [Header("클릭용 콜라이더")]
    [SerializeField] private Collider2D foodCollider;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (foodCollider == null)
            foodCollider = GetComponent<Collider2D>();

        // 처음 오븐에서 생성될 때는 클릭 막기
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

    public void SetPickable(bool canPick)
    {
        if (foodCollider != null)
            foodCollider.enabled = canPick;
    }

    void Select()
    {
        isSelected = true;

        if (sr != null)
            sr.color = Color.red;
    }

    public void Cancel()
    {
        isSelected = false;

        if (sr != null)
            sr.color = Color.white;
    }
}