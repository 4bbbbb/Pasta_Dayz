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
        // 1️⃣ 만들 수 있는 메뉴만 필터링 (면 제외)
        var availableMenus = menuDB.menuList
            .Where(menu => menu.IngredientsID
            .All(id => ingredientDB.GetIngredientByID(id)?.isUnlocked == true))
            .ToList();

        if (availableMenus.Count == 0)
        {
            Debug.LogError("해금된 메뉴가 없습니다!");
            return null;
        }

        // 2️⃣ 랜덤 메뉴 선택
        MenuData randomMenu = availableMenus[Random.Range(0, availableMenus.Count)];

        // 3️⃣ 랜덤 면 선택 (해금된 면만)
        List<int> unlockedNoodles = ingredientDB.ingredientList
            .Where(i => i.category == "Noodle" && i.isUnlocked)
            .Select(i => i.id)
            .ToList();

        int randomNoodle = unlockedNoodles[Random.Range(0, unlockedNoodles.Count)];

        // 4️⃣ 랜덤 토핑 선택 (해금된 것 중 메뉴에 포함된 것만, 0~2개)
        List<int> unlockedToppings = ingredientDB.ingredientList
        .Where(i => i.category == "Topping" && i.isUnlocked)
        .Select(i => i.id)
        .ToList();

        List<int> randomToppings = new List<int>();
        int toppingCount = Mathf.Min(Random.Range(0, 3), unlockedToppings.Count);

        for (int i = 0; i < toppingCount; i++)
        {
            int index = Random.Range(0, unlockedToppings.Count);
            randomToppings.Add(unlockedToppings[index]);
            unlockedToppings.RemoveAt(index); // 중복 방지
        }

        // 5️⃣ Order 생성
        return new Order(
            randomMenu,
            randomNoodle,
            randomToppings,
            ordertemplateDB
        );
    }

}
