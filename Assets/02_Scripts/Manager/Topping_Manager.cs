using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToppingManager : MonoBehaviour
{
    public static ToppingManager Instance;

    public GameObject sixToppingGroup;
    public GameObject tenToppingGroup;
    public GameObject thirteenToppingGroup;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(RefreshAfterOneFrame());
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RefreshAfterOneFrame());
    }

    IEnumerator RefreshAfterOneFrame()
    {
        yield return null;
        RefreshToppingUI();
    }

    public void RefreshToppingUI()
    {
        if (IngredientDatabase.Instance == null)
            return;

        if (sixToppingGroup != null) sixToppingGroup.SetActive(false);
        if (tenToppingGroup != null) tenToppingGroup.SetActive(false);
        if (thirteenToppingGroup != null) thirteenToppingGroup.SetActive(false);

        List<IngredientData> list = IngredientDatabase.Instance.ingredientList
            .Where(t => t.isUnlocked && t.categoryType == IngredientData.CategoryType.Topping)
            .ToList();

        list.AddRange(
            IngredientDatabase.Instance.ingredientList
            .Where(t => t.isUnlocked && t.id == 402)
        );

        GameObject activeGroup;

        if (list.Count <= 6)
            activeGroup = sixToppingGroup;
        else if (list.Count <= 10)
            activeGroup = tenToppingGroup;
        else
            activeGroup = thirteenToppingGroup;

        if (activeGroup == null)
            return;

        activeGroup.SetActive(true);

        foreach (Transform child in activeGroup.transform)
        {
            child.gameObject.SetActive(false);
        }

        Topping[] toppings = activeGroup.GetComponentsInChildren<Topping>(true);

        for (int i = 0; i < list.Count && i < toppings.Length; i++)
        {
            toppings[i].gameObject.SetActive(true);

            IngredientDatabase.IngredientIconData iconData =
                IngredientDatabase.Instance.GetIngredientIconData(list[i].id);

            toppings[i].Initialize(iconData);
        }
    }
}