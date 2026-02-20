using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    public float dayDuration = 180f; // 3분
    private float timer;
    public bool isDayActive = false;
    public bool isTakingOrder = true;

    public OrderManager orderManager;


    void Start()
    {
        StartDay();
    }

    void Update()
    {
        if (!isDayActive) return;

        if (isTakingOrder)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                StopTakingOrders();
            }
        }
    }

    void StartDay()
    {
        timer = dayDuration;
        isDayActive = true;
        isTakingOrder = true;       
    }      

    void StopTakingOrders()
    {
        isTakingOrder = false;
        Debug.Log("주문 마감!");
    }

    public void EndDay()
    {
        isDayActive = false;
        orderManager.EndService();
        Debug.Log("하루 종료!");
    }
}
