using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Plates_BasicPlate : MonoBehaviour, IInteractable
{
    [Header("<<완성된 파스타 스폰위치>>")]
    [SerializeField] private Transform pastaSpawnPoint;

    [Header("<<완성된 파스타 프리팹>>")]
    [SerializeField] private GameObject finishedPastaPrefab;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    void Start()
    {        
        isSelected = false;
    }
    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("완성된 파스타를 옮겨주세요!");            
            return true;
        }             

        if (target is FinishedPasta finishedPasta)
        {
            Debug.Log("완성된 파스타를 그릇에 담았어요 !!");
            finishedPasta.OnMovedToPlate();
            Destroy(finishedPasta.gameObject);
            Instantiate(
                finishedPastaPrefab,
                pastaSpawnPoint.position,
                Quaternion.identity,
                pastaSpawnPoint
                );

            return true;
        }
        return false;
    }

    void Select()
    {
                
    }
    public void Cancel()
    {
               
    }
}
