using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Day_Manager : MonoBehaviour
{
    public Order_Manager orderManager;
    public static Day_Manager Instance;

    public float dayDuration = 180f; // 3분
    private float timer;

    public int day;

    public bool isDayActive = false;
    public bool isTakingOrder = true;
    public bool hasEndedDay = false;   

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
       
        if (scene.name == "01_Counter" && !isDayActive)
        {
            StartDay();
        }
    }

    void Update()
    {
        if (!isDayActive)
        {
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            timer = 0;
            StopTakingOrders();
        }
    }

    void StopTakingOrders()
    {
        if (!isTakingOrder)
        {
            return;
        }

        isTakingOrder = false;

        if (hasEndedDay)
        {
            return;
        }

        orderManager.OnOrderTimeEnded();
    }


    public float GetRemainingTime()
    {
        return timer;
    }

    void StartDay()
    {
        day++;
        timer = dayDuration;        
        isDayActive = true;
        isTakingOrder = true;        
        orderManager.SetState(Order_Manager.ServiceState.WaitingForOrder);
    }

    public int GetDay()
    {
        return day;
    }

    public void EndDay()
    {
        if (hasEndedDay)
            return;

        hasEndedDay = true;

        isDayActive = false;
        orderManager.SetState(Order_Manager.ServiceState.DayEnded);

        Debug.Log("하루 종료! +20");

        StartCoroutine(EndDayRoutine());
    }

    private IEnumerator EndDayRoutine()
    {
        if (orderManager != null)
        {
            yield return orderManager.PlayCustomerExitForDayEnd();
        }

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(3);

        if (Level_Manager.Instance != null)
            Level_Manager.Instance.EarnXP(20);

        if (Gold_Manager.Instance != null && Gold_Manager.Instance.DailyNetProfit() > 0)
        {
            Level_Manager.Instance.EarnXP(10);
            Debug.Log("흑자 : +10");
        }
    }



    public void ResetForNextDay()
    {
        timer = dayDuration;
        isDayActive = false;
        isTakingOrder = true;
        hasEndedDay = false;

        orderManager.SetState(Order_Manager.ServiceState.WaitingForOrder);
    }

   
}
