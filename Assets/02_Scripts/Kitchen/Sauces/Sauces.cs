using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Sauces : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    public bool isSelected { get; private set; }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("소스 선택!");
            Select();
            return true;
        }

        return false;

        // 후라이팬
    }
    void Select()
    {
        isSelected = true;
        sr.color = Color.yellow;
    }

    public void Cancel()
    {
        isSelected = false;
        sr.color = Color.white;
    }

}
