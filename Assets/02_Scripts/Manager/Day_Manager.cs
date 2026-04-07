using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Day_Manager : MonoBehaviour
{
    public Order_Manager orderManager;
    public static Day_Manager Instance;

    public float dayDuration = 180f;
    private float timer;

    public int day;                
    private int completedDay = 0;  

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
        if (scene.name == "01_Counter")
        {
            orderManager = FindFirstObjectByType<Order_Manager>();
        }

        if (scene.name == "01_Counter" && !isDayActive && orderManager != null)
        {
            StartDay();
        }
    }

    void Update()
    {
        if (!isDayActive)
            return;

        // 튜토리얼 중에는 시간 정지
        if (IsTutorialTimeFrozen())
            return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            timer = 0;
            StopTakingOrders();
        }
    }

    private bool IsTutorialTimeFrozen()
    {
        return TutorialController.Instance != null &&
               TutorialController.Instance.IsTutorialActive;
    }

    void StopTakingOrders()
    {
        if (!isTakingOrder)
            return;

        isTakingOrder = false;

        if (hasEndedDay)
            return;

        if (orderManager != null)
            orderManager.OnOrderTimeEnded();
    }

    public float GetRemainingTime()
    {
        return timer;
    }

    void StartDay()
    {
        day = completedDay + 1;   
        timer = dayDuration;
        isDayActive = true;
        isTakingOrder = true;
        hasEndedDay = false;

        if (orderManager != null)
            orderManager.SetState(Order_Manager.ServiceState.WaitingForOrder);
    }

    public int GetDay()
    {
        return day;
    }

    public int GetCompletedDay()
    {
        return completedDay;
    }

    public void EndDay()
    {
        if (hasEndedDay)
            return;

        hasEndedDay = true;
        isDayActive = false;

        completedDay = day;

        if (orderManager != null)
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

        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.SaveGame();
        }
    }

    public void ResetForNextDay()
    {
        timer = dayDuration;
        isDayActive = false;
        isTakingOrder = true;
        hasEndedDay = false;

        day = completedDay;

        if (orderManager != null)
            orderManager.SetState(Order_Manager.ServiceState.WaitingForOrder);
    }

    public void LoadDayData(int savedCompletedDay)
    {
        completedDay = Mathf.Max(0, savedCompletedDay);
        day = completedDay;
        timer = dayDuration;
        isDayActive = false;
        isTakingOrder = true;
        hasEndedDay = false;
    }
}
