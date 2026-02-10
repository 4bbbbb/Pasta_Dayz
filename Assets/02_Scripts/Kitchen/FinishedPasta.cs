using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class FinishedPasta : MonoBehaviour, IInteractable
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
            Debug.Log("완성된 파스타 선택!");
            Select();
            return true;
        }

        return false;
    }
    void Select()
    {
        isSelected = true;
        sr.color = Color.red;
    }

    public void Cancel()
    {
        isSelected = false;
        sr.color = Color.white;
    }
}
