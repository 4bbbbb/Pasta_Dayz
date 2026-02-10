using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;
using static Topping;

public class Plate : MonoBehaviour, IInteractable
{    
    private SpriteRenderer sr;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    public PlateType plateType;
    public enum PlateType
    {
        BasicPlate,
        OvenPlate,
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
            Debug.Log("기본 그릇 선택!");
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
