using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    [SerializeField] public OrderGenerator generator;
    [SerializeField] public DayManager dayManager;
    [SerializeField] public IngredientDatabase ingredientDB;

    [Header("UI")]
    public GameObject customerUIPrefab;
    private Transform canvasTransform;    

    [Header("손님 프리팹")]
    private CustomerUI currentCustomer;

    [Header("연출용 파스타박스 프리팹")]
    [SerializeField] GameObject serveBoxPrefab;

    public enum ServiceState
    {
        WaitingForOrder,        // 손님을 기다리는 상태
        TakingOrder,            // 주문을 받는 상태
        Cooking,                // 요리 중인 상태
        ServingDish,            // 요리를 서빙하는 상태 (손님에게 요리 전달)
        DayEnded                // 하루가 종료된 상태
    }

    public ServiceState currentState;

    public Order currentOrder;
    private bool? pendingResult = null;

    void Awake()
    {        
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        SetState(ServiceState.WaitingForOrder); // 처음 상태는 주문 대기
    }

    public void SetState(ServiceState state)
    {
        currentState = state;

        switch (currentState)
        {
            case ServiceState.WaitingForOrder:
                break;

            case ServiceState.TakingOrder:
                break;

            case ServiceState.Cooking:
                break;

            case ServiceState.ServingDish:
                break;

            case ServiceState.DayEnded:                
                break;
        }
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
        if (scene.name == "01_Counter")
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            canvasTransform = canvas.transform;

            currentCustomer = FindObjectOfType<CustomerUI>();
            if (currentCustomer == null)
            {
                GameObject obj = Instantiate(customerUIPrefab, canvasTransform);
                currentCustomer = obj.GetComponent<CustomerUI>();
            }

            if (pendingResult.HasValue)
            {
                currentCustomer.Appear();
                currentCustomer.HideBubble();
                StartCoroutine(ServeDishAndGoToNextCustomer(pendingResult.Value));
                pendingResult = null;
            }
            else
            {
                StartService();
            }
        }        
    }

    public void StartService()
    {
        SpawnCustomer();
    }

    void SpawnCustomer()
    {
        if (!dayManager.isTakingOrder)
        {
            CheckDayEndCondition();
            return;
        }

        currentState = ServiceState.TakingOrder;        

        // Canvas 찾기
        if (canvasTransform == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();

            if (canvas != null)
            {
                canvasTransform = canvas.transform;
            }                
            else
            {
                Debug.LogError("Canvas를 찾을 수 없음!");
                return;
            }
        }

        // CustomerUI 없으면 새로 생성
        if (currentCustomer == null)
        {
            GameObject obj = Instantiate(customerUIPrefab, canvasTransform);
            currentCustomer = obj.GetComponent<CustomerUI>();
        }

        currentOrder = generator.GenerateOrder();

        if (currentOrder == null)
        {
            return;
        }

        DebugIngredientSet(currentOrder, "손님 주문");

        string message = currentOrder.GetOrderText(generator.ingredientDB);

        currentCustomer.Appear();
        currentCustomer.ShowOrder(message);

        // 버튼은 숨김
        if (currentCustomer.yesButton != null)
            currentCustomer.yesButton.SetActive(false);
    }

    public void GetPrice()
    {
        if (currentOrder == null)
        {
            return; 
        }            

        // Price 계산
        float menuPrice = currentOrder.Price(generator.ingredientDB);

        // GoldManager에 추가
        Gold_Manager.Instance.Earn(menuPrice);

        Debug.Log($"손님 주문 완료! 받은 금액: {menuPrice}, 총 골드: {Gold_Manager.Instance.totalGold}");

        // 요리하러 이동
        GoToKitchen();
    }

    public void GoToKitchen()
    {
        SetState(ServiceState.Cooking);

        // 현재 카운터에 있는 손님 Destroy
        if (currentCustomer != null)
        {
            Destroy(currentCustomer.gameObject);
            currentCustomer = null;
        }

        // 씬 전환
        SceneManager.LoadScene(2);
    }

    public void SubmitDish(PastaBox pastaBox)
    {
        if (currentOrder == null)
        {
            return;
        }            

        bool success = IsCorrect(pastaBox, currentOrder);

        Debug.Log(success ? "성공!" : "실패!");

        // 메뉴 가격, 재료비
        HashSet<int> usedIngredients = pastaBox.GetIngredientSet(); // 플레이어가 사용한 재료
        HashSet<int> correctIngredients = currentOrder.GetIngredientSet(); // 손님 주문 재료

        float totalingredientCost = 0f;        // 실제 차감될 재료비
        float refund = 0f;           // 환불될 재료비

        // 1. 실제 사용 재료 확인
        foreach (int id in usedIngredients)
        {
            var ingredient = ingredientDB.GetIngredientByID(id);
            if (ingredient == null) continue;

            totalingredientCost += ingredient.ingredientCost;  // 실제 사용한 재료비 차감
        }

        // 2. 빠진 재료 확인 (손님 주문 재료 - 실제 사용 재료)
        foreach (int id in correctIngredients)
        {
            if (!usedIngredients.Contains(id))
            {
                var ingredient = ingredientDB.GetIngredientByID(id);
                if (ingredient == null) continue;

                refund += ingredient.price;  // 손님에게 이미 받은 금액 일부 환불
            }
        }

        // 3. Gold 처리
        Gold_Manager.Instance.Spend(totalingredientCost);  // 사용한 재료비 차감
        if (refund > 0f)
        {
            Gold_Manager.Instance.Refund(refund);      // 빠진 재료에 대한 환불
            Debug.Log($"빠진 재료 환불: {refund}");
        }

        Debug.Log($"재료비 차감: {totalingredientCost}, 현재 골드: {Gold_Manager.Instance.totalGold}");

        currentOrder = null;

        pendingResult = success;
    }

    IEnumerator ServeDishAndGoToNextCustomer(bool success)
    {
        yield return new WaitForSeconds(1f);

        // PastaBox 생성
        GameObject box = Instantiate(serveBoxPrefab, canvasTransform);

        yield return new WaitForSeconds(1f);

        // 결과 연출
        string resultMessage = success ? "성공!" : "실패!";
        currentCustomer.ShowResult(resultMessage);                   

        yield return new WaitForSeconds(2f);

        // 손님 제거
        if (currentCustomer != null)
        {
            Destroy(currentCustomer.gameObject);
            currentCustomer = null;
        }

        // PastaBox 제거
        Destroy(box);

        yield return new WaitForSeconds(2f);

        // 새 손님
        SpawnCustomer();

        CheckDayEndCondition();
    }

    public bool IsCorrect(IHasIngredients a, IHasIngredients b)
    {
        return a.GetIngredientSet()
                .SetEquals(b.GetIngredientSet());
    }

    public void OnOrderTimeEnded()
    {
        Debug.Log("영업 종료! 새 주문 불가");

        // 아직 yes 안 누른 상태라면
        if (currentState == ServiceState.TakingOrder)
        {            
            Destroy(currentCustomer.gameObject);
            currentCustomer = null;
            currentOrder = null;

            CheckDayEndCondition();
        }
    }

    void CheckDayEndCondition()
    {
        if (!dayManager.isTakingOrder && currentOrder == null && currentCustomer == null)
        {
            dayManager.EndDay();
        }
    }

    void DebugIngredientSet(IHasIngredients target, string label)
    {
        HashSet<int> set = target.GetIngredientSet();

        string result = string.Join(", ", set);

        Debug.Log($"{label} 재료 HashSet: [{result}]");

        if (target is Order order)
        {
            float menuPrice = order.Price(ingredientDB);           // 손님에게 받는 가격
            float ingredientCost = order.Ingredient_Cost(ingredientDB); // 재료 비용

            Debug.Log($"{label} - 메뉴 총 가격: {menuPrice} / 재료 비용: {ingredientCost}");
        }
    }
}