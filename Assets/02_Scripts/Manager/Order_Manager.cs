using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderManager : MonoBehaviour
{

    [SerializeField]
    public OrderGenerator generator;
    public DayManager dayManager;
    
    public GameObject customerPrefab;
    public Transform counterPoint;

    private Customer currentCustomer;

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

    void Start()
    {
        StartService();   
    }

    public void StartService()
    {
        totalMoney = 0;
        SpawnCustomer(); 
    }

    public void EndService()
    {
        Debug.Log("오늘 총 수익: " + totalMoney);
    }

    void SpawnCustomer()
    {
        if (!dayManager.isTakingOrder)
            return;

        currentState = ServiceState.TakingOrder;

        GameObject obj = Instantiate(
            customerPrefab,
            counterPoint.position,
            Quaternion.identity
        );

        currentCustomer = obj.GetComponent<Customer>();
        currentCustomer.Appear(); // 띠용 등장

        // ?? 0.2초 뒤 주문 생성
        StartCoroutine(GenerateOrderAfterDelay(0.2f));
    }

    public void SubmitDish(Dish playerDish)
    {
        if (currentOrder == null)
            return;

        currentState = ServiceState.Cooking;

        bool success = CompareDish(playerDish, currentOrder);

        if (success)
        {
            int reward = CalculateReward(currentOrder);
            totalMoney += reward;
            Debug.Log("성공! +" + reward);
        }
        else
        {
            Debug.Log("실패! 환불!");
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
            dayManager.EndDay();
            yield break;
        }

        SpawnCustomer();
    }

    bool CompareDish(Dish dish, Order order)
    {
        if (dish.menuData.menuID != order.menuData.menuID)
            return false;

        if (dish.noodleID != order.noodleID)
            return false;

        if (!dish.toppingIDs.OrderBy(x => x)
            .SequenceEqual(order.toppingIDs.OrderBy(x => x)))
            return false;

        return true;
    }

    IEnumerator GenerateOrderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        currentOrder = generator.GenerateOrder();

        string message = currentOrder.GetOrderText(generator.ingredientDB);

        currentCustomer.ShowOrder(message); // ?? 말풍선 표시
    }

    int CalculateReward(Order order)
    {
        return 100; // 나중에 확장
    }
}
