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

  
    static ToppingType GetToppingType(int id)
    {
        if (id < 301 || id > 313)
        {
            if (id != 402)
                Debug.Log($"{id}번 토핑은 존재하지 않습니다.");
            return ToppingType.Tomato;
        }
        return (ToppingType)(id - 301);

    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        ingredientIDs = GetComponent<IngredientIDs>();
        isSelected = false;
    }

    
    public void Initialize(IngredientDatabase.IngredientIconData data)
    {
        ingredientIDs.ingredientID = data.id;
        toppingType = GetToppingType(data.id);
        sr.sprite = data.icon;

        if (data.id == 402)
        {
            Cheese cheese = gameObject.AddComponent<Cheese>();
            cheese.cheeseType = Cheese.CheeseType.Mozzarella;
            Destroy(this);
        }
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