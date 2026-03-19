using UnityEngine;
using static IInteractableScript;

public class Plate_BakedPane : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
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