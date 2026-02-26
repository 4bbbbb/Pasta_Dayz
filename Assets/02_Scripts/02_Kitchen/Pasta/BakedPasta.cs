using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;
using static Sauces;

public class BakedPasta : MonoBehaviour, IInteractable
{
    [Header("<<파슬리 프리팹>>")]
    [SerializeField] private GameObject parsleyPrefab;

    [Header("<<파슬리 스폰 위치>>")]
    [SerializeField] Transform parsleySpawnPoint; 

    private SpriteRenderer sr;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    private HashSet<int> ingredientIDs = new HashSet<int>();

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetIngredients(HashSet<int> ids)
    {
        ingredientIDs = new HashSet<int>(ids);
    }
    public HashSet<int> GetIngredientSet()
    {
        return ingredientIDs;
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("잘 구워진 파스타 선택!");
            Select();
            return true;
        }

        if (target is Topping_Parsley parsley)
        {
            Debug.Log("파슬리를 뿌렸어요");
            Instantiate(
                parsleyPrefab,
                parsleySpawnPoint.position,
                Quaternion.identity,
                parsleySpawnPoint
                );

            IngredientIDs id = parsley.GetComponent<IngredientIDs>();
            if (id != null)
            {
                ingredientIDs.Add(id.GetID());   // 파슬리 ID 추가
            }

            return true;
        }

        return false;
    }

    void Select()
    {
        isSelected = true;
        sr.color = Color.red;
    }

    public void Cancel()
    {
        isSelected = false;
        sr.color = Color.white;
    }
}
