using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_GasStove: MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    public GameObject fryingPan;
    private bool isFireOn = false;
    bool isCooking = false;

    public bool CanBeSelected => false;


    void Start()
    {        
        sr = GetComponent<SpriteRenderer>();
        fryingPan.SetActive(false);
        isFireOn = false;
    }
    public bool Interact(IInteractable target)
    {
        if(isCooking)
        {
            Debug.Log("이미 후라이팬이 있습니다 !");
            return false;
        }

        if (target == null)
        {
            Debug.Log("후라이팬이 준비 되었습니다 !");
            isCooking = true;
            fryingPan.SetActive(true);
            return false;
        }
        return false;
    }

    public void TurnOn()
    {
        Debug.Log("가스에 불이 켜집니다 타닥타닥..!!");
        isFireOn = true;
        sr.color = Color.blue;
    }

    public void TurnOff()
    {
        Debug.Log("조리 완료! 가스가 꺼집니다.");
        isFireOn = false;
        sr.color = Color.white;        
    }

    public void DestroyFryingPan()
    {
        isCooking = false;
        fryingPan.SetActive(false);
    }

    public void Cancel()
    {

    }
}
