using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Topping : MonoBehaviour, IInteractable
{
    public bool isOliveOil;    

    private SpriteRenderer sr;
    private IngredientIDs ingredientIDs;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    public ToppingType toppingType;

    public enum ToppingType
    {
        Tomato,
        Garlic,
        Barsil,
        Onion,
        Mushroom,
        Bacon,
        Pepperoncino,
        Shrimp,
        Clams,
        Broccoli,
        Mussel,
        Meatball,
        Sausage,
    }

    /// <summary>
    /// ingredientID에 따른 ToppingType을 반환하는 헬퍼 함수
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    static ToppingType GetToppingType(int id)
    {
        if (id < 301 || id > 313)
        {
            Debug.Log($"{id}번 토핑은 존재하지 않습니다.");
            return ToppingType.Tomato;
        }
        return (ToppingType)(id - 301);

    }

    // 초기화 작업이기에 Start() -> Awake() 변경
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        ingredientIDs = GetComponent<IngredientIDs>();
        isSelected = false;
    }

    /// <summary>
    /// 초기화 함수(IconData 적용)
    /// </summary>
    /// <param name="data"></param>
    public void Initialize(IngredientDatabase.IngredientIconData data)
    {
        ingredientIDs.ingredientID = data.id;
        toppingType = GetToppingType(data.id);
        sr.sprite = data.icon;
    }


    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log($"{name} 선택!");
            Select();
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
