using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Cooker_Oven;
using static IInteractableScript;

public class Cooker_PassTable : MonoBehaviour, IInteractable
{
    [Header("<< 스폰위치 >>")]
    [SerializeField] private Transform plateSpawnPoint;

    [Header("<< 박스 프리팹 >>")]
    [SerializeField] private GameObject boxPrefab;

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
                StartCoroutine(ServePastaWithDelay(plateTransform, finishedPasta));
                return true;
            }

            Debug.Log("접시에 담아서 가져오세요!");
            return false;
        }

        if (target is BakedPasta bakedPasta)
        {
            if (bakedPasta.GetComponentInParent<Cooker_PlateTable>() != null)
            {
                // BakedPasta의 경우에도 음식 올리고 1초 뒤에 PastaBox 생성
                StartCoroutine(ServeBakedPastaWithDelay(bakedPasta));
                return true;
            }

            Debug.Log("플레이트 테이블 위에 올려주세요!");
            return false;
        }

        return false;
    }

    // PastaBox를 생성하고 1초 뒤에 씬 전환
    IEnumerator ServePastaWithDelay(Transform plateTransform, FinishedPasta finishedPasta)
    {
        // 음식 올리기
        plateTransform.SetParent(plateSpawnPoint);
        plateTransform.localPosition = Vector3.zero;
        Debug.Log("음식을 올렸습니다.");

        // 1초 대기 후 PastaBox 생성
        yield return new WaitForSeconds(1f);

        HashSet<int> finalSet = new HashSet<int>(finishedPasta.GetIngredientSet());

        // Plate ID 추가
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

        // PastaBox 생성
        PastaBox box = Instantiate(boxPrefab).GetComponent<PastaBox>();
        box.SetIngredients(finalSet);

        DebugFinalSet(box.GetIngredientSet(), "PastaBox 재료");

        // OrderManager에 전달
        OrderManager orderManager = FindObjectOfType<OrderManager>();
        orderManager.SubmitDish(box);

        Debug.Log("완성된 파스타를 서빙합니다!");

        // 1초 대기 후 씬 전환
        yield return new WaitForSeconds(1f);

        // 씬 전환 (1번 씬으로 돌아가기)
        SceneManager.LoadScene(1); 
    }

    // BakedPasta의 경우 1초 후에 PastaBox 생성하고 씬 전환
    IEnumerator ServeBakedPastaWithDelay(BakedPasta bakedPasta)
    {
        // BakedPasta 올리기
        bakedPasta.transform.SetParent(plateSpawnPoint);
        bakedPasta.transform.position = plateSpawnPoint.position;

        Debug.Log("BakedPasta를 올렸습니다.");

        // 1초 대기 후 PastaBox 생성
        yield return new WaitForSeconds(1f);

        HashSet<int> finalSet = bakedPasta.GetIngredientSet();
        DebugFinalSet(finalSet, "최종 서빙 파스타");

        // PastaBox 생성 후 OrderManager에 전달
        PastaBox pastaBox = Instantiate(boxPrefab).GetComponent<PastaBox>();  // 여기서 box로 생성
        pastaBox.SetIngredients(finalSet);

        OrderManager orderManager = FindObjectOfType<OrderManager>();
        orderManager.SubmitDish(pastaBox);

        // 1초 대기 후 씬 전환
        yield return new WaitForSeconds(1f);

        // 씬 전환 (1번 씬으로 돌아가기)
        SceneManager.LoadScene(1);  // "Scene1"은 1번 씬의 이름
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
