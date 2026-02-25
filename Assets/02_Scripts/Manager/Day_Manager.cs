using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    public float dayDuration = 180f; // 3분
    private float timer;

    public bool isDayActive = false;
    public bool isTakingOrder = true;

    public OrderManager orderManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartDay();
    }

    void Update()
    {
        if (!isDayActive) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            timer = 0;
            StopTakingOrders();
        }
    }

    void StopTakingOrders()
    {
        if (!isTakingOrder) return;

        isTakingOrder = false;
        Debug.Log("3분 종료!");

        orderManager.OnOrderTimeEnded();
    }

    public float GetRemainingTime()
    {
        return timer;
    }


    void StartDay()
    {
        timer = dayDuration;
        isDayActive = true;
        isTakingOrder = true;
        orderManager.SetState(OrderManager.ServiceState.WaitingForOrder);
    }         

    public void EndDay()
    {
        isDayActive = false;
        orderManager.SetState(OrderManager.ServiceState.DayEnded);  // 하루 종료 상태로 전환
        Debug.Log("하루 종료!");

        // 정산 씬으로 넘어가는 로직
        SceneManager.LoadScene(3);

        // 하루 통계 출력
        Debug.Log($"===== 하루 정산 =====");
        Debug.Log($"총 수익: {Gold_Manager.Instance.dailyRevenue}");
        Debug.Log($"총 재료비: {Gold_Manager.Instance.dailyCost}");
        Debug.Log($"총 환불: {Gold_Manager.Instance.dailyRefund}");
        Debug.Log($"순수익: {Gold_Manager.Instance.DailyNetProfit()}");
        Debug.Log("===================");
    }
}
