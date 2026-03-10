using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Noodles_CookedNoodle : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
        
    public bool isSelected { get; private set; }

    public bool CanBeSelected => true;

    //[Header("Noodle Sprites")]
    //[SerializeField] private Sprite spaghetti;
    //[SerializeField] private Sprite linguine;
    //[SerializeField] private Sprite penne;
    //[SerializeField] private Sprite farfalle;

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

    //public void SetNoodleSprite(int id)
    //{
    //    switch (id)
    //    {
    //        case 201:
    //            sr.sprite = spaghetti;
    //            break;

    //        case 202:
    //            sr.sprite = linguine;
    //            break;

    //        case 203:
    //            sr.sprite = penne;
    //            break;

    //        case 204:
    //            sr.sprite = farfalle;
    //            break;
    //    }
    //}

    public void Cancel()
    {
        isSelected = false;
        sr.color = Color.white;
    }
}
