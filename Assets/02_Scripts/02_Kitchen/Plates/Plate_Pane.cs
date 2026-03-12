using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Plate_Pane : MonoBehaviour, IInteractable
{ 
    private SpriteRenderer sr;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;
        

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        isSelected = false;
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("빠네 선택!");
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
