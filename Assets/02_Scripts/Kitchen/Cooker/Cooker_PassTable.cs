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
                HashSet<int> finalSet = new HashSet<int>(finishedPasta.GetIngredientSet());

                // 🔥 Plate ID 추가
                IngredientIDs plateID = plateTransform.GetComponent<IngredientIDs>();
                if (plateID != null)
                {
                    finalSet.Add(plateID.GetID());
                }
                else
                {
                    plateID = plateTransform.GetComponentInChildren<IngredientIDs>();
                    if (plateID != null)
                    {
                        finalSet.Add(plateID.GetID());
                    }
                }

                // 빠네 ID 추가
                Plates_BasicPlate basicPlateComponent = plateTransform.GetComponent<Plates_BasicPlate>();

                if (basicPlateComponent != null)
                {
                    finalSet.UnionWith(basicPlateComponent.GetIngredientSet());
                }

                // 🔥 디버그 출력
                DebugFinalSet(finalSet, "최종 제출 음식");

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

                HashSet<int> finalSet = bakedPasta.GetIngredientSet();
                DebugFinalSet(finalSet, "최종 서빙 파스타");

                return true;
            }

            Debug.Log("플레이트 테이블 위에 올려주세요!");
            return false;
        }

        return false;
    }

    void DebugFinalSet(HashSet<int> set, string label)
    {
        string result = string.Join(", ", set);
        Debug.Log($"{label} 재료 HashSet: [{result}]");
    }

    public void Cancel()
    {

    }
}
