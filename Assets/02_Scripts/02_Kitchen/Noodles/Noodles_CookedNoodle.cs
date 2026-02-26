using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Noodles_CookedNoodle : MonoBehaviour, IInteractable
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
            isSelected = true;
            sr.color = Color.red;
            return true;
        }
        return false;
    }

    public void Cancel()
    {
        isSelected = false;
        sr.color = Color.white;
    }
}
