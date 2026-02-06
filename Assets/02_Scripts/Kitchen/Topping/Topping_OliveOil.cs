using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Topping_OliveOil : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    public bool isSelected { get; private set; }

    public bool isOliveOil = true;

    public GameObject toppingPrefab;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        isSelected = false;
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("올리브오일 선택!");
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
