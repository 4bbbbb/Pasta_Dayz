using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cheese: MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite parmesanSprite;
    [SerializeField] private Sprite parmesanselectedSprite;

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
            Select();
            return true;
        }

        return false;
    }

    void Select()
    {
        isSelected = true;
        if(cheeseType == CheeseType.Parmesan)
        {
            sr.sprite = parmesanselectedSprite;

        }
        else
        {
            sr.color = Color.red;

        }
    }

    public void Cancel()
    {
        isSelected = false;
        if (cheeseType == CheeseType.Parmesan)
        {
            sr.sprite = parmesanSprite;

        }
        else
        {
            sr.color = Color.white;

        }
    }
}
