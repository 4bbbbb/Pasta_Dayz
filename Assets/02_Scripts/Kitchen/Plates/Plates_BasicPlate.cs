using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Plates_BasicPlate : MonoBehaviour, IInteractable
{
    [Header("<<완성된 파스타 스폰위치>>")]
    [SerializeField] private Transform pastaSpawnPoint;

    [Header("<<구워진 빠네 스폰위치>>")]
    [SerializeField] private Transform paneSpawnPoint;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;
    private bool hasPasta = false;
    private bool hasPane = false;

    public Collider plateCollider;

    void Start()
    {        
        plateCollider = GetComponent<Collider>();
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
            if (hasPasta)
            {
                Debug.Log("이미 파스타가 담겨 있어요!");
                return false;
            }
                       
            finishedPasta.OnMovedToPlate();

            finishedPasta.transform.SetParent(pastaSpawnPoint);
            finishedPasta.transform.position = pastaSpawnPoint.position;
            hasPasta = true;

            return true;
        }        
        
        if (target is Plate_BakedPane bakedPane)
        {
            if(hasPasta)
            {
                Debug.Log("지금은 빠네를 추가할 수 없어요ㅠㅜ");
                return false;
            }

            if(hasPane)
            {
                Debug.Log("이미 빠네가 준비되었어요 !");
                return false;
            }

            bakedPane.transform.SetParent(paneSpawnPoint);
            bakedPane.transform.position = paneSpawnPoint.position;
            plateCollider.enabled = false;
            hasPane = true;

            return true;


        }

        return false;
    }
    
    
    public void Cancel()
    {
               
    }
}
