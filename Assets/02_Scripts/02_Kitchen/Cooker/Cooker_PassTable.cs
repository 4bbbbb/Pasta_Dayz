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
            if (!HasPlateID(finishedPasta))
            {
                Debug.Log("접시에 담아서 가져오세요!");
                return false;
            }

            StartCoroutine(ServePastaWithDelay(finishedPasta));
            return true;
        }

        if (target is BakedPasta bakedPasta)
        {
            if (!bakedPasta.IsPlated())
            {
                Debug.Log("플레이트 위에 올려진 baked pasta만 서빙할 수 있어요!");
                return false;
            }

            StartCoroutine(ServeBakedPastaWithDelay(bakedPasta));
            return true;
        }

        return false;
    }

    private bool HasPlateID(FinishedPasta finishedPasta)
    {
        if (finishedPasta == null)
            return false;

        HashSet<int> set = finishedPasta.GetIngredientSet();
        if (set == null)
            return false;

        return set.Contains(501) || set.Contains(502);
    }

    IEnumerator ServePastaWithDelay(FinishedPasta finishedPasta)
    {
        finishedPasta.transform.SetParent(plateSpawnPoint);
        finishedPasta.transform.localPosition = Vector3.zero;

        Debug.Log("음식을 올렸습니다.");

        yield return new WaitForSeconds(1f);

        HashSet<int> finalSet = new HashSet<int>(finishedPasta.GetIngredientSet());

        PastaBox box = Instantiate(boxPrefab).GetComponent<PastaBox>();
        box.SetIngredients(finalSet);
        box.SetBaked(false);

        DebugFinalSet(box.GetIngredientSet(), "PastaBox 재료");

        Order_Manager orderManager = FindObjectOfType<Order_Manager>();
        if (orderManager != null)
        {
            orderManager.SubmitDish(box);
        }
        else
        {
            Debug.LogWarning("Order_Manager를 찾지 못했습니다.");
        }

        Debug.Log("완성된 파스타를 서빙합니다!");

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(1);
    }

    IEnumerator ServeBakedPastaWithDelay(BakedPasta bakedPasta)
    {
        bakedPasta.transform.SetParent(plateSpawnPoint);
        bakedPasta.transform.position = plateSpawnPoint.position;

        Debug.Log("BakedPasta를 올렸습니다.");

        yield return new WaitForSeconds(1f);

        HashSet<int> finalSet = new HashSet<int>(bakedPasta.GetIngredientSet());
        DebugFinalSet(finalSet, "최종 서빙 파스타");

        PastaBox pastaBox = Instantiate(boxPrefab).GetComponent<PastaBox>();
        pastaBox.SetIngredients(finalSet);
        pastaBox.SetBaked(true);

        Order_Manager orderManager = FindObjectOfType<Order_Manager>();
        if (orderManager != null)
        {
            orderManager.SubmitDish(pastaBox);
        }
        else
        {
            Debug.LogWarning("Order_Manager를 찾지 못했습니다.");
        }

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(1);
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