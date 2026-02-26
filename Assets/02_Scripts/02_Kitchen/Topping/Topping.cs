using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Topping : MonoBehaviour, IInteractable
{
    public bool isOliveOil;    

    private SpriteRenderer sr;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    public ToppingType toppingType;

    public enum ToppingType
    {
        Tomato,
        Garlic,
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();        
        isSelected = false;
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
