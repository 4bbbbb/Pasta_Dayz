using System.Collections;
using UnityEngine;

public class CustomerSatisfaction_Manager : MonoBehaviour
{
    public static CustomerSatisfaction_Manager Instance;
    public OrderManager orderManager;

    [Header("만족도 설정")]
    public float maxSatisfaction = 100f;
    public float decreasePerSecond = 1f;

    private float currentSatisfaction;
    private bool isRunning = false;

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

        StartSatisfaction();
    }

    public void StartSatisfaction()
    {
        currentSatisfaction = maxSatisfaction;
        if (!isRunning)
        {
            StartCoroutine(SatisfactionTimer());
        }
    }

    IEnumerator SatisfactionTimer()
    {
        isRunning = true;

        while (currentSatisfaction > 0)
        {
            yield return new WaitForSeconds(1f);

            currentSatisfaction -= decreasePerSecond;

            if (currentSatisfaction < 0)
            {
                currentSatisfaction = 0;
            }

            if (currentSatisfaction == 0)
            {
                //  만족도 0 시 바로 OrderManager에게 알려주기
                if (OrderManager.Instance != null)
                {
                    OrderManager.Instance.SatisfactionZero();
                }
            }

        }

        isRunning = false;
    }

    public float GetSatisfactionRatio()
    {
        return currentSatisfaction / maxSatisfaction;
    }
    public void ResetSatisfaction()
    {
        currentSatisfaction = maxSatisfaction;
        
        if (isRunning)
        {
            StopAllCoroutines();
        }

        StartCoroutine(SatisfactionTimer());
    }

    public float GetCurrentSatisfaction()
    {
        return currentSatisfaction;
    }
}