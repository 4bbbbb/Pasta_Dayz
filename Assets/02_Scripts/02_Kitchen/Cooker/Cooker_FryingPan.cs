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
    [SerializeField] private Transform finishedPastaSpawnPoint;
    [SerializeField] private Transform noodleSpawnPoint;

    [Header("오일 스프라이트")]
    [SerializeField] private GameObject oilOffSprite;
    [SerializeField] private GameObject oilOnSprite;

    [Header("소스 스프라이트")]
    [SerializeField] private GameObject tomatoSauceSprite;   // 202
    [SerializeField] private GameObject creamSauceSprite;    // 203
    [SerializeField] private GameObject roseSauceSprite;     // 204
    [SerializeField] private GameObject vongoleSauceSprite;  // 205

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
        ResetState();
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

            ShowSauceSprite(204);
            oilOffSprite.SetActive(false);
            oilOnSprite.SetActive(false);

            return true;
        }

        // 크림 + 토마토 -> 로제
        if (ingredientIDs.Contains(203) && newID == 202)
        {
            ingredientIDs.Remove(203);
            ingredientIDs.Add(204);

            addedSauces.Clear();
            addedSauces.Add(SauceType.Rose);

            ShowSauceSprite(204);
            oilOffSprite.SetActive(false);
            oilOnSprite.SetActive(false);

            return true;
        }

        // 로제 이미 있으면 추가 불가
        if (ingredientIDs.Contains(204))
        {
            return false;
        }

        // 일반 소스
        if (addedSauces.Count >= 1)
        {
            return false;
        }

        addedSauces.Add(sauce.sauceType);
        ingredientIDs.Add(newID);

        ShowSauceSprite(newID);
        oilOffSprite.SetActive(false);
        oilOnSprite.SetActive(false);

        return true;
    }

    void ShowSauceSprite(int sauceID)
    {
        tomatoSauceSprite.SetActive(false);
        creamSauceSprite.SetActive(false);
        roseSauceSprite.SetActive(false);
        vongoleSauceSprite.SetActive(false);

        switch (sauceID)
        {
            case 202:
                tomatoSauceSprite.SetActive(true);
                break;
            case 203:
                creamSauceSprite.SetActive(true);
                break;
            case 204:
                roseSauceSprite.SetActive(true);
                break;
            case 205:
                vongoleSauceSprite.SetActive(true);
                break;
        }
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
        Vector3 originalPanPos = transform.localPosition;
        Vector3 panMoveDir = transform.localRotation * Vector3.up;

        float cycleCount = 4.25f;
        float cycleDuration = 0.95f;
        float totalTime = cycleCount * cycleDuration;

        float panMoveAmount = 0.12f;
        float elapsed = 0f;

        Dictionary<Transform, Vector3> ingredientOriginalLocalPos = new Dictionary<Transform, Vector3>();

        foreach (Transform point in toppingSpawnPoints)
        {
            foreach (Transform child in point)
            {
                ingredientOriginalLocalPos[child] = child.localPosition;
            }
        }

        foreach (Transform child in noodleSpawnPoint)
        {
            ingredientOriginalLocalPos[child] = child.localPosition;
        }

        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / totalTime);

            float angle = t * cycleCount * Mathf.PI * 2f;

            float envelope = 1f;
            if (t > 0.7f)
            {
                float fadeT = (t - 0.7f) / 0.3f;
                envelope = Mathf.SmoothStep(1f, 0f, fadeT);
            }

            // 팬 본체 흔들기
            float panOffset = Mathf.Sin(angle) * panMoveAmount * envelope;
            transform.localPosition = originalPanPos + panMoveDir * panOffset;

            // 토핑 + 면만 살짝 출렁이기
            foreach (var pair in ingredientOriginalLocalPos)
            {
                Transform ingredient = pair.Key;
                if (ingredient == null) continue;

                Vector3 basePos = pair.Value;

                // 각 재료마다 미세하게 다르게
                float phase = ingredient.GetInstanceID() * 0.01f;

                float offsetY = Mathf.Sin(angle - 0.6f + phase) * 0.03f * envelope;
                float offsetX = Mathf.Cos(angle * 1.2f + phase) * 0.015f * envelope;

                ingredient.localPosition = basePos + new Vector3(offsetX, offsetY, 0f);
            }

            yield return null;
        }

        // 자연스럽게 끝난 뒤 정확히 원위치 복귀
        transform.localPosition = originalPanPos;

        foreach (var pair in ingredientOriginalLocalPos)
        {
            if (pair.Key != null)
                pair.Key.localPosition = pair.Value;
        }


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

        tomatoSauceSprite.SetActive(false);
        creamSauceSprite.SetActive(false);
        roseSauceSprite.SetActive(false);
        vongoleSauceSprite.SetActive(false);
    }

    public void Cancel() 
    {
    }
}
