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

    private IngredientIDs ingredientIDs;
    private HashSet<int> ingredients = new HashSet<int>();

    void Start()
    {
        plateCollider = GetComponent<Collider>();
        isSelected = false;

        ingredientIDs = GetComponent<IngredientIDs>();

        if (ingredientIDs != null)
        {
            ingredients.Add(ingredientIDs.GetID());
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
            finishedPasta.OnMovedToPlate();

            finishedPasta.transform.SetParent(pastaSpawnPoint);
            finishedPasta.transform.position = pastaSpawnPoint.position;

            ingredients = new HashSet<int>(finishedPasta.GetIngredientSet());

            ingredients.Clear();

            // FinishedPasta 재료 복사
            ingredients = new HashSet<int>(finishedPasta.GetIngredientSet());

            // Plate 자기 ID 다시 추가
            if (ingredientIDs != null)
                ingredients.Add(ingredientIDs.GetID());

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

    // 현재 재료 세트 반환 (Order 비교용)
    public HashSet<int> GetIngredientSet()
    {
        return ingredients;
    }

    // 필요하면 디버그용 출력
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
