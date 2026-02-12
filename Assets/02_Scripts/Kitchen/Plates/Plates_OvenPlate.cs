using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Plates_OvenPlate : MonoBehaviour, IInteractable
{
    [Header("<<완성된 파스타 스폰위치>>")]
    [SerializeField] private Transform pastaSpawnPoint;    

    private Collider plateCollider;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    void Start()
    {
        plateCollider = GetComponent<Collider>();
        isSelected = false;
    }

    public bool Interact(IInteractable target)
    {
        if (pastaSpawnPoint.childCount > 0)
        {
            Debug.Log("이미 파스타가 담겨 있어요!");
            return false;
        }

        if (target == null)
        {
            Debug.Log("완성된 파스타를 옮겨주세요!");
            return true;            
        }

        if (target is FinishedPasta finishedPasta)
        {                           
            finishedPasta.OnMovedToPlate();

            finishedPasta.transform.SetParent(pastaSpawnPoint);
            finishedPasta.transform.position = pastaSpawnPoint.position;

            return true;
        }

        return false;
    }

    public void Cancel()
    {

    }
}
