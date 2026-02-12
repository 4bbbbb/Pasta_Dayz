using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_PassTable : MonoBehaviour, IInteractable
{
    [Header("<<스폰위치>>")]
    [SerializeField] private Transform plateSpawnPoint;

    public bool CanBeSelected => false;
     

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("옮길 그릇을 선택해주세요 !!");
            return true;
        }

        if (target is FinishedPasta finishedPasta)
        {
            var basicPlate = finishedPasta.GetComponentInParent<Plates_BasicPlate>();
            var ovenPlate = finishedPasta.GetComponentInParent<Plates_OvenPlate>();

            Transform plateTransform = null;

            if (basicPlate != null)
            {
                plateTransform = basicPlate.transform;
            }
            else if (ovenPlate != null)
            {
                plateTransform = ovenPlate.transform;

            }

            if (plateTransform != null)
            {
                Debug.Log("완성된 파스타를 서빙합니다!");

                plateTransform.SetParent(plateSpawnPoint);
                plateTransform.localPosition = Vector3.zero;

                return true;
            }

            Debug.Log("접시에 담아서 가져오세요!");
            return false;
        }

        if (target is BakedPasta bakedPasta)
        {
            if (bakedPasta.GetComponentInParent<Cooker_PlateTable>() != null)
            {
                Debug.Log("완성된 파스타를 서빙합니다!");

                bakedPasta.transform.SetParent(plateSpawnPoint);
                bakedPasta.transform.position = plateSpawnPoint.position;

                return true;
            }

            Debug.Log("플레이트 테이블 위에 올려주세요!");
            return false;
        }

        return false;
    }
    public void Cancel()
    {

    }
}
