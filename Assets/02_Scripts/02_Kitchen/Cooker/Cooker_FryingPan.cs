using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;
using static Sauces;
using static Topping;

public class Cooker_FryingPan : MonoBehaviour, IInteractable
{
    [Header("가스스토브")]
    [SerializeField] private Cooker_GasStove gasStove;

    [Header("스폰 위치")]
    [SerializeField] private Transform[] toppingSpawnPoints;
    [SerializeField] private Transform sauceSpawnPoint;
    [SerializeField] private Transform finishedPastaSpawnPoint;
    [SerializeField] private Transform noodleSpawnPoint;

    [Header("오일 상태")]
    [SerializeField] private GameObject oilOffSprite;
    [SerializeField] private GameObject oilOnSprite;

    [Header("완성 파스타")]
    [SerializeField] private GameObject finishedPastaPrefab;

    private bool hasOil = false;
    private bool isCooking = false;

    private HashSet<ToppingType> addedToppings = new HashSet<ToppingType>();
    private HashSet<SauceType> addedSauces = new HashSet<SauceType>();
    private HashSet<int> ingredientIDs = new HashSet<int>();

    public bool CanBeSelected => false;

    void Start()
    {
        oilOffSprite.SetActive(true);
        oilOnSprite.SetActive(false);
    }

    public bool Interact(IInteractable target)
    {
        if (isCooking)
        {
            return false;
        }

        if (target is Topping_OliveOil oil)
        {
            return AddOil(oil);
        }

        if (target is Topping topping)
        {
            return AddTopping(topping);
        }

        if (target is Sauces sauce)
        {
            return AddSauce(sauce);
        }

        if (target is Noodles_CookedNoodle noodle)
        {
            return AddNoodle(noodle);
        }

        return false;
    }

    bool AddOil(Topping_OliveOil oil)
    {
        if (hasOil) return false;

        hasOil = true;
        gasStove.TurnOn();

        oilOffSprite.SetActive(false);
        oilOnSprite.SetActive(true);

        IngredientIDs id = oil.GetComponent<IngredientIDs>();
        if (id != null)
            ingredientIDs.Add(id.GetID());

        return true;
    }

    bool AddTopping(Topping topping)
    {
        if (addedToppings.Count >= 2) return false;
        if (addedToppings.Contains(topping.toppingType)) return false;

        Transform spawnPoint = GetRandomEmptyToppingPoint();
        if (spawnPoint == null) return false;

        IngredientIDs id = topping.GetComponent<IngredientIDs>();

        if (id != null)
            SpawnIngredientByID(id.GetID(), spawnPoint);

        addedToppings.Add(topping.toppingType);
        return true;
    }

    bool AddSauce(Sauces sauce)
    {
        IngredientIDs id = sauce.GetComponent<IngredientIDs>();
        if (id == null) return false;

        int newID = id.GetID();

        // 토마토 + 크림 -> 로제
        if (ingredientIDs.Contains(202) && newID == 203)
        {
            ingredientIDs.Remove(202);
            ingredientIDs.Add(204);

            addedSauces.Clear();
            addedSauces.Add(SauceType.Rose);

            Debug.Log("로제소스됨");

            return true;
        }

        // 크림 + 토마토 -> 로제
        if (ingredientIDs.Contains(203) && newID == 202)
        {
            ingredientIDs.Remove(203);
            ingredientIDs.Add(204);

            addedSauces.Clear();
            addedSauces.Add(SauceType.Rose);

            Debug.Log("로제소스됨");


            return true;
        }

        // 로제 이미 있으면 추가 불가
        if (ingredientIDs.Contains(204))
            return false;

        // 일반 소스
        if (addedSauces.Count >= 1)
        {
            return false;
        }

        SpawnIngredientByID(newID, sauceSpawnPoint);
        addedSauces.Add(sauce.sauceType);

        return true;
    }

    bool AddNoodle(Noodles_CookedNoodle cookedNoodle)
    {
        if (!hasOil) return false;
        if (noodleSpawnPoint.childCount > 0) return false;

        IngredientIDs id = cookedNoodle.GetComponent<IngredientIDs>();
        if (id == null) return false;

        SpawnIngredientByID(id.GetID(), noodleSpawnPoint);

        Destroy(cookedNoodle.gameObject);

        StartCoroutine(CookRoutine());
        return true;
    }

    void SpawnIngredientByID(int ingredientID, Transform spawnPoint)
    {
        GameObject prefab = Order_Manager.Instance
            .ingredientDB
            .GetPrefab(ingredientID);

        if (prefab == null) return;

        Instantiate(
            prefab,
            spawnPoint.position,
            Quaternion.identity,
            spawnPoint
        );

        ingredientIDs.Add(ingredientID);
    }

    Transform GetRandomEmptyToppingPoint()
    {
        List<Transform> empty = new List<Transform>();

        foreach (var point in toppingSpawnPoints)
        {
            if (point.childCount == 0)
                empty.Add(point);
        }

        if (empty.Count == 0) return null;

        return empty[Random.Range(0, empty.Count)];
    }

    IEnumerator CookRoutine()
    {
        isCooking = true;

        Vector3 originalPos = transform.localPosition;
        float cookTime = 4f;
        float elapsed = 0f;

        float radiusX = 0.12f;   // 가로 반경
        float radiusY = 0.08f;   // 세로 반경
        float angularSpeed = 4f; // 회전 속도

        while (elapsed < cookTime)
        {
            float angle = elapsed * angularSpeed;

            float x = Mathf.Cos(angle) * radiusX;
            float y = Mathf.Sin(angle) * radiusY;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }


        //yield return new WaitForSeconds(4f);


        GameObject finishedPasta = Instantiate(
            finishedPastaPrefab,
            finishedPastaSpawnPoint.position,
            Quaternion.identity,
            finishedPastaSpawnPoint
        );

        FinishedPasta pasta = finishedPasta.GetComponent<FinishedPasta>();

        pasta.SetIngredients(new HashSet<int>(ingredientIDs));
        pasta.Init(gasStove);

        ClearPan();

        isCooking = false;
        gasStove.TurnOff();
    }

    void ClearPan()
    {
        foreach (Transform point in toppingSpawnPoints)
        {
            foreach (Transform child in point)
                Destroy(child.gameObject);
        }

        if (sauceSpawnPoint.childCount > 0)
            Destroy(sauceSpawnPoint.GetChild(0).gameObject);

        if (noodleSpawnPoint.childCount > 0)
            Destroy(noodleSpawnPoint.GetChild(0).gameObject);

        ResetState();
    }

    void ResetState()
    {
        addedToppings.Clear();
        addedSauces.Clear();
        ingredientIDs.Clear();

        hasOil = false;

        oilOffSprite.SetActive(true);
        oilOnSprite.SetActive(false);
    }
    public void Cancel() 
    {
    }
}
