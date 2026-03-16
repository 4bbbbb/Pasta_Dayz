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

    private int plateID;
    private IngredientIDs ingredientIDs;
    private HashSet<int> ingredients = new HashSet<int>();

    void Start()
    {
        plateCollider = GetComponent<Collider>();
        isSelected = false;

        ingredientIDs = GetComponent<IngredientIDs>();

        if (ingredientIDs != null)
        {
            plateID = ingredientIDs.GetID();   
        }
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
            if (!finishedPasta.CanMoveToPlate(plateID))
            {
                Debug.Log("옮길수없습니다.");
                return false;
            }

            finishedPasta.transform.SetParent(pastaSpawnPoint);
            finishedPasta.transform.localPosition = Vector3.zero;
            finishedPasta.transform.localRotation = Quaternion.identity;
            finishedPasta.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

            // 현재 접시 재료 유지 (접시 ID 포함)
            HashSet<int> finalIngredients = new HashSet<int>(ingredients);

            // 파스타 재료 추가
            foreach (int id in finishedPasta.GetIngredientSet())
            {
                finalIngredients.Add(id);
            }

            // FinishedPasta에도 최종 재료 반영
            finishedPasta.SetIngredients(finalIngredients);

            // 접시 쪽도 동일하게 저장
            ingredients = new HashSet<int>(finalIngredients);

            // 마지막에 호출
            finishedPasta.OnMovedToPlate();

            PrintIngredients();
            return true;
        }

        return false;
    }

    public void AddIngredient(int id)
    {
        if (!ingredients.Contains(id))
        {
            ingredients.Add(id);
        }
    }

    public HashSet<int> GetIngredientSet()
    {
        return new HashSet<int>(ingredients);
    }

    public void PrintIngredients()
    {
        foreach (int id in ingredients)
        {
            Debug.Log("Plate에 포함된 ID: " + id);
        }
    }

    public void Cancel()
    {
    }
}