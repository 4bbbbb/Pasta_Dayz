using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrderManager : MonoBehaviour
{
    [SerializeField] public OrderGenerator generator;
    public DayManager dayManager;

    [Header("UI")]
    public GameObject customerUIPrefab;
    private Transform canvasTransform;

    private CustomerUI currentCustomer;

    public enum ServiceState
    {
        WaitingCustomer,
        TakingOrder,
        Cooking,
        WaitingNextCustomer
    }

    public ServiceState currentState;
    public float nextCustomerDelay = 2f;

    private Order currentOrder;
    private int totalMoney = 0;

    void Awake()
    {        
        DontDestroyOnLoad(gameObject);
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
        if (scene.name == "02_Counter")
        {            
            canvasTransform = FindObjectOfType<Canvas>().transform;
            StartService();
        }
    }

    public void StartService()
    {
        totalMoney = 0;
        SpawnCustomer();
    }

    void SpawnCustomer()
    {
        if (!dayManager.isTakingOrder)
            return;       

        currentState = ServiceState.TakingOrder;

        GameObject obj = Instantiate(customerUIPrefab, canvasTransform);
        currentCustomer = obj.GetComponent<CustomerUI>();
        Debug.Log("canvasTransform: " + canvasTransform);
        currentCustomer.Appear();

        StartCoroutine(GenerateOrderAfterDelay(0.2f));
    }

    IEnumerator GenerateOrderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentCustomer == null)
            yield break;

        currentOrder = generator.GenerateOrder();
        DebugIngredientSet(currentOrder, "주문");
        if (currentOrder == null)
            yield break;

        string message = currentOrder.GetOrderText(generator.ingredientDB);

        if (currentCustomer != null)
            currentCustomer.ShowOrder(message);
    }

    public void SubmitDish(Dish playerDish)
    {
        if (currentOrder == null)
            return;

        currentState = ServiceState.Cooking;

        bool success = IsCorrect(playerDish, currentOrder);

        if (success)
        {
            int reward = CalculateReward(currentOrder);
            totalMoney += reward;
            Debug.Log("성공! +" + reward);
        }
        else
        {
            Debug.Log("실패!");
        }

        currentOrder = null;
        currentState = ServiceState.WaitingNextCustomer;

        currentCustomer.HideOrder();
        currentCustomer.Disappear();

        StartCoroutine(NextCustomerRoutine());
    }

    IEnumerator NextCustomerRoutine()
    {
        yield return new WaitForSeconds(nextCustomerDelay);

        if (!dayManager.isTakingOrder)
        {
            EndService();          // 🔥 다시 추가
            dayManager.EndDay();
            yield break;
        }

        SpawnCustomer();
    }

    public bool IsCorrect(IHasIngredients a, IHasIngredients b)
    {
        return a.GetIngredientSet()
                .SetEquals(b.GetIngredientSet());
    }

    int CalculateReward(Order order)
    {
        return 100;
    }
    public void EndService()
    {
        Debug.Log("오늘 총 수익: " + totalMoney);
    }



    void DebugIngredientSet(IHasIngredients target, string label)
    {
        HashSet<int> set = target.GetIngredientSet();

        string result = string.Join(", ", set);

        Debug.Log($"{label} 재료 HashSet: [{result}]");
    }
}