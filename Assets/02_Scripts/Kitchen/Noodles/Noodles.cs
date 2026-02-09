using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Noodles : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    public bool isSelected {  get; private set; }
    public GameObject cookedNoodlePrefab;

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
            Debug.Log("면 선택!");
            Select();
            return true;
        }
        
        if (target is Cooker_PastaCooker pastaCooker)
        {
            Debug.Log("면이 삶아지고 있습니다. 보글보글 oOoOO ....");
            pastaCooker.StartBowling(this);
            Cancel();
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
