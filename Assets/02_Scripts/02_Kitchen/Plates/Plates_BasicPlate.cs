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

    private IngredientIDs ingredientIDs;
    private HashSet<int> ingredients = new HashSet<int>();

    void Start()
    {
        plateCollider = GetComponent<Collider>();
        isSelected = false;

        ingredientIDs = GetComponent<IngredientIDs>();

        if (ingredientIDs != null)
        {
            ingredients.Add(ingredientIDs.GetID());   // 접시 ID
        }
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

            finishedPasta.transform.SetParent(pastaSpawnPoint);
            finishedPasta.transform.localPosition = Vector3.zero;
            finishedPasta.transform.localRotation = Quaternion.identity;
            finishedPasta.transform.localScale = Vector3.one;

            // 접시가 현재 가지고 있던 재료 유지
            // (접시 ID + 빠네 601 포함 가능)
            HashSet<int> finalIngredients = new HashSet<int>(ingredients);

            // 파스타 재료 추가
            foreach (int id in finishedPasta.GetIngredientSet())
            {
                finalIngredients.Add(id);
            }

            // 최종 재료 세트를 FinishedPasta에도 반영
            finishedPasta.SetIngredients(finalIngredients);

            // 접시 쪽도 동일하게 갱신
            ingredients = new HashSet<int>(finalIngredients);

            // 마지막에 호출해야 접시 스프라이트가 정확히 바뀜
            finishedPasta.OnMovedToPlate();

            hasPasta = true;

            PrintIngredients();
            return true;
        }

        if (target is Plate_BakedPane bakedPane)
        {
            if (hasPasta)
            {
                Debug.Log("지금은 빠네를 추가할 수 없어요ㅠㅜ");
                return false;
            }

            if (hasPane)
            {
                Debug.Log("이미 빠네가 준비되었어요!");
                return false;
            }

            bakedPane.transform.SetParent(paneSpawnPoint);
            bakedPane.transform.position = paneSpawnPoint.position;

            plateCollider.enabled = false;

            IngredientIDs id = bakedPane.GetComponent<IngredientIDs>();
            if (id != null)
                ingredients.Add(id.GetID());   // 601 추가

            hasPane = true;

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