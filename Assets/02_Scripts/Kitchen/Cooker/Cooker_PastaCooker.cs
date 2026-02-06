using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static IInteractableScript;

public class Cooker_PastaCooker : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
   
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("면을 선택해주세요");            
            return false;
        }

        return false;
       
    }    
    public void Cancel()
    {

    }

    public void OnBowling()
    {
        sr.color = Color.cyan;
    }

    public void StopBowling()
    {
        sr.color = Color.white;
        Debug.Log("면이 다 익었습니다 !");
    }

}
