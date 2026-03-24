using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Noodles : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;

    public bool isSelected { get; private set; }

    [Header("스프라이트")]
    public Sprite originalSprite;
    public Sprite selectedSprite;

    public bool CanBeSelected => true;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = originalSprite;
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
        sr.sprite = selectedSprite;
    }

    public void Cancel()
    {
        isSelected = false;
        sr.sprite = originalSprite;
    }
}
