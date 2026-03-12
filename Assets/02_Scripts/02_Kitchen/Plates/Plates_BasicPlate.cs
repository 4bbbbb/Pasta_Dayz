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
            ingredients.Add(ingredientIDs.GetID());   // 🔥 Plate ID 추가
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
                       
            finishedPasta.OnMovedToPlate();

            finishedPasta.transform.SetParent(pastaSpawnPoint);
            finishedPasta.transform.position = pastaSpawnPoint.position;

            ingredients = new HashSet<int>(finishedPasta.GetIngredientSet());

            // 🔥 기존 재료 초기화
            ingredients.Clear();

            // 🔥 Plate 자기 ID 먼저 추가
            if (ingredientIDs != null)
                ingredients.Add(ingredientIDs.GetID());

            // 🔥 FinishedPasta 재료 하나씩 복사
            foreach (int id in finishedPasta.GetIngredientSet())
            {
                ingredients.Add(id);
            }            

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

            IngredientIDs id = bakedPane.GetComponent<IngredientIDs>();
            if (id != null)
                ingredients.Add(id.GetID());

            PrintIngredients();

            hasPane = true;

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
