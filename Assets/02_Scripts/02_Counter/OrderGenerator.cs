using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderGenerator : MonoBehaviour
{
    public MenuDatabase menuDB;
    public IngredientDatabase ingredientDB;
    public OrderTemplateDatabase ordertemplateDB;

    public Order GenerateOrder()
    {
        // 1️. 만들 수 있는 메뉴만 필터링
        var availableMenus = menuDB.menuList
            .Where(menu => menu.IngredientsID.All(id =>
             {
                var ingredient = ingredientDB.GetIngredientByID(id);

                 if (ingredient == null)
                 {
                    Debug.LogWarning($"ID {id} 재료를 찾을 수 없습니다.");
                    return false;
                    }

                return ingredient.isUnlocked;
            }))
            .ToList();

        if (availableMenus.Count == 0)
        {
            Debug.LogError("해금된 메뉴가 없습니다!");
            return null;
        }

        // 2️. 랜덤 메뉴 선택
        MenuData randomMenu = availableMenus[Random.Range(0, availableMenus.Count)];

        // 3️. 랜덤 면 선택 (해금된 면만)
        int randomNoodle = ingredientDB.GetRandomNoodle();
        if (randomNoodle == -1)
        {
            Debug.LogError("해금된 면이 없습니다!");
            return null;
        }

        // 4️. 랜덤 토핑 선택 (해금된 것 중 메뉴에 포함된 것만, 0~2개)
        List<int> randomToppings = ingredientDB.GetRandomToppings();        

        // 5. Order 생성
        return new Order(
            randomMenu,
            randomNoodle,
            randomToppings,
            ordertemplateDB
        );
    }

}
