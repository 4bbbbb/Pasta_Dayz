using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderGenerator : MonoBehaviour
{
    public Order GenerateOrder()
    {
        if (OrderTemplateDatabase.Instance == null)
        {
            Debug.LogError("OrderTemplateDatabase.Instance가 null입니다.");
            return null;
        }

        if (MenuDatabase.Instance == null)
        {
            Debug.LogError("MenuDatabase.Instance가 null입니다.");
            return null;
        }

        if (IngredientDatabase.Instance == null)
        {
            Debug.LogError("IngredientDatabase.Instance가 null입니다.");
            return null;
        }

        var availableMenus = MenuDatabase.Instance.menuList
            .Where(menu => MenuDatabase.Instance.IsMenuUnlocked(menu.menuID, IngredientDatabase.Instance))
            .ToList();

        if (availableMenus.Count == 0)
        {
            Debug.LogError("해금된 메뉴가 없습니다!");
            return null;
        }

        MenuData randomMenu = availableMenus[Random.Range(0, availableMenus.Count)];

        int randomNoodle = IngredientDatabase.Instance.GetRandomNoodle();
        if (randomNoodle == -1)
        {
            Debug.LogError("해금된 면이 없습니다!");
            return null;
        }

        List<int> randomToppings = IngredientDatabase.Instance.GetRandomToppings();

        return new Order(
            randomMenu,
            randomNoodle,
            randomToppings,
            OrderTemplateDatabase.Instance
        );
    }
}