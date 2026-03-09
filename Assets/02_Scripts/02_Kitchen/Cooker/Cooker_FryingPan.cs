using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;
using static Sauces;
using static Topping;

public class Cooker_FryingPan : MonoBehaviour, IInteractable
{
    [Header("<<가스스토브>>")] 
    [SerializeField] private Cooker_GasStove gasStove;

    [Header("<<스폰위치>>")] 
    [SerializeField] private Transform[] toppingSpawnPoints;
    [SerializeField] private Transform sauceSpawnPoint;
    [SerializeField] private Transform finishedPastaSpawnPoint;
    [SerializeField] private Transform noodleSpawnPoint;
    [SerializeField] private Transform oliveoilSpawnPoint;  

    [Header("<<완성된 파스타 프리팹>>")][SerializeField] private GameObject finishedPastaPrefab;

    private SpriteRenderer sr;

    private bool hasOil = false;
    private bool isCooking = false;

    private HashSet<ToppingType> addedToppings = new HashSet<ToppingType>();
    private HashSet<SauceType> addedSauces = new HashSet<SauceType>();

    public bool CanBeSelected => false;

    private HashSet<int> ingredientIDs = new HashSet<int>();


    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public bool Interact(IInteractable target)
    {
        // 프라이팬이 조리중이므로 아무런 상호작용 X
        if (isCooking) return false;


        // 올리브오일
        if (target is Topping_OliveOil oil)
        {
            hasOil = true;
            gasStove.TurnOn();

            IngredientIDs id = oil.GetComponent<IngredientIDs>();

            if (id != null)
            {
                SpawnIngredientByID(id.GetID(), oliveoilSpawnPoint);
            }            

            return true;
        }

        // 일반 토핑
        if (target is Topping topping)
        {
            if (addedToppings.Count >= 2)
            {
                return false;
            }

            // 이미 추가한 토핑이라면 추가 X
            if (addedToppings.Contains(topping.toppingType))
            {
                return false;
            }

            Transform spawnPoint = GetRandomEmptyToppingPoint();

            if (spawnPoint == null)
            {
                return false;
            }

            IngredientIDs id = topping.GetComponent<IngredientIDs>();

            if (id != null)
            {
                SpawnIngredientByID(id.GetID(), spawnPoint);
            }

            addedToppings.Add(topping.toppingType);

            return true;
        }

        // 소스
        if (target is Sauces sauces)
        {
            // 이미 같은 소스가 있으면 안 넣음
            if (addedSauces.Contains(sauces.sauceType))
                return false;     
            
           IngredientIDs id = sauces.GetComponent<IngredientIDs>();

            if (id != null)
            {
                SpawnIngredientByID(id.GetID(), sauceSpawnPoint);
            }

            // 추가 기록
            addedSauces.Add(sauces.sauceType);

            // 로제 소스 조합 체크
            if (addedSauces.Contains(SauceType.Tomato) && addedSauces.Contains(SauceType.Cream))
            {
                // 기존 소스들 제거
                foreach (Transform child in sauceSpawnPoint)
                {
                    Destroy(child.gameObject);
                }

                addedSauces.Clear();

                // 로제 소스 생성
                // 여기에 로제 소스 프리팹 연결 필요
               /* GameObject rosePrefab =*/ /* 로제 프리팹 참조 변수 */
                //Instantiate(
                //    rosePrefab,
                //    sauceSpawnPoint.position,
                //    Quaternion.identity,
                //    sauceSpawnPoint
                //);

                // 로제 추가
                //addedSauces.Add(SauceType.Rose);
            }           

            return true;
        }

        // 익은 면
        if (target is Noodles_CookedNoodle cookedNoodle)
        {
            if (!hasOil)
            {
                return false;
            }

            if (noodleSpawnPoint.childCount > 0)
            {
                return false;
            }

            IngredientIDs id = cookedNoodle.GetComponent<IngredientIDs>(); ;

            Debug.Log("Object: " + cookedNoodle.name);
            Debug.Log("Has IngredientIDs: " + (id != null));

            if (id == null)
            {
                Debug.Log("cookedNoodle에 IngredientIDs 없음");
                return false;
            }
            else
            {
                Debug.Log("cookedNoodle ID: " + id.GetID());

                SpawnIngredientByID(id.GetID(), noodleSpawnPoint);
            }

            Destroy(cookedNoodle.gameObject);

            StartCoroutine(CookRoutine());
            return true;
        }

        return false;
    }

    void SpawnIngredientByID(int ingredientID, Transform spawnPoint)
    {
        GameObject prefab = Order_Manager.Instance
            .ingredientDB
            .GetPrefab(ingredientID);

        if (prefab == null)
            return;

        Instantiate(
            prefab,
            spawnPoint.position,
            Quaternion.identity,
            spawnPoint
        );

        ingredientIDs.Add(ingredientID);
    }

    // 토핑 랜덤 생성시 겹침 방지
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
        sr.color = Color.grey;

        for (int i = 1; i <= 4; i++)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log($"{i}초... 볶는 중");
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

        ClearPanIngredients();

        isCooking = false;
        sr.color = Color.white;
        gasStove.TurnOff();        
    }

    void ClearPanIngredients()
    {
        foreach (Transform point in toppingSpawnPoints)
        {
            foreach (Transform child in point)
            {
                Destroy(child.gameObject);
            }
        }

        if (oliveoilSpawnPoint.childCount > 0)
        {
            Destroy(oliveoilSpawnPoint.GetChild(0).gameObject);
        }

        if (sauceSpawnPoint.childCount > 0)
        {
            Destroy(sauceSpawnPoint.GetChild(0).gameObject);
        }

        if (noodleSpawnPoint.childCount > 0)
        {
            Destroy(noodleSpawnPoint.GetChild(0).gameObject);
        }

        ResetPanState();
    }

    void ResetPanState()
    {
        addedToppings.Clear();
        addedSauces.Clear();
        ingredientIDs.Clear();
        hasOil = false;
    }

    public void Cancel()
    {

    }

}
    
