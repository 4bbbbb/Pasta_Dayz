using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cheese: MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    public bool isSelected { get; private set; }

    public bool CanBeSelected => true;

    public CheeseType cheeseType;
    public enum CheeseType
    {
        Parmesan,
        Mozzarella,
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log($"{name} º±≈√!");
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
