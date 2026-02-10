using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;
using static Sauces;
using static Topping;

public class Cooker_FryingPan : MonoBehaviour, IInteractable
{
    [Header("<<가스스토브>>")] [SerializeField] private Cooker_GasStove gasStove;
    [Header("<<토핑스폰위치>>")] [SerializeField] private Transform[] toppingSpawnPoints;
    [Header("<<소스스폰위치>>")] [SerializeField] private Transform sauceSpawnPoint;
    [Header("<<완성된파스타스폰위치>>")][SerializeField] private Transform finishedPastaSpawnPoint;
    [Header("<<익은면스폰위치>>")] [SerializeField] private Transform noodleSpawnPoint;
    [Header("<<올리브오일스폰위치>>")] [SerializeField] private Transform oliveoilSpawnPoint;
    [Header("<<완성된파스타>>")][SerializeField] private GameObject finishedPastaPrefab;

    private SpriteRenderer sr;
    
    private bool hasOil = false;
    private bool isCooking = false;

    private HashSet<ToppingType> addedToppings = new HashSet<ToppingType>();
    private HashSet<SauceType> addedSauces = new HashSet<SauceType>();

    public bool CanBeSelected => false;
    

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public bool Interact(IInteractable target)
    {
        if (isCooking) return false;


        // 올리브오일
        if (target is Topping_OliveOil oil)
        {
            hasOil = true;
            gasStove.TurnOn();

            Instantiate(
                oil.oliveOilPrefab,
                oliveoilSpawnPoint.position,
                Quaternion.identity,
                oliveoilSpawnPoint
            );

            return true;
        }

        // 일반 토핑
        if (target is Topping topping)
        {
            if (addedToppings.Contains(topping.toppingType))
                return false;

            Transform spawnPoint = GetRandomEmptyToppingPoint();
            if (spawnPoint == null) return false;

            GameObject obj = Instantiate(
                topping.toppingPrefab,
                spawnPoint.position,
                Quaternion.identity,
                spawnPoint
            );

            obj.transform.localScale *= 0.5f;

            addedToppings.Add(topping.toppingType);
            return true;
        }

        // 소스
        if (target is Sauces sauces)
        {
            // 이미 같은 소스가 있으면 안 넣음
            if (addedSauces.Contains(sauces.sauceType))
                return false;

            // 소스 스폰
            Instantiate(
                sauces.saucePrefab,
                sauceSpawnPoint.position,
                Quaternion.identity,
                sauceSpawnPoint
            );

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

            cookedNoodle.transform.SetParent(noodleSpawnPoint);
            cookedNoodle.transform.position = noodleSpawnPoint.position;

            StartCoroutine(CookRoutine());
            return true;
        }

        return false;
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

        ClearPanIngredients();

        Instantiate(
            finishedPastaPrefab,
            finishedPastaSpawnPoint.position,
            Quaternion.identity,
            noodleSpawnPoint
        );

        isCooking = false;
        sr.color = Color.white;
        gasStove.TurnOff();        
    }

    void ClearPanIngredients()
    {
        foreach (Transform point in toppingSpawnPoints)
        {
            if (point.childCount > 0)
            {
                Destroy(point.GetChild(0).gameObject);
            }
                
        }

        if (oliveoilSpawnPoint.childCount > 0)
        {
            Destroy(oliveoilSpawnPoint.GetChild(0).gameObject);
        }
            

        if (noodleSpawnPoint.childCount > 0)
        {
            Destroy(noodleSpawnPoint.GetChild(0).gameObject);
        }            

        addedToppings.Clear();
        hasOil = false;
    }



    public void Cancel()
    {

    }

}
    
