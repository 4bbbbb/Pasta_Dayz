using System.Collections;
using UnityEngine;

public class CustomerSatisfaction_Manager : MonoBehaviour
{
    public static CustomerSatisfaction_Manager Instance;
    public Order_Manager orderManager;

    [Header("만족도 설정")]
    public float maxSatisfaction = 100f;
    public float decreasePerSecond = 1f;

    private float currentSatisfaction;
    private bool isRunning = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
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
            // 튜토리얼 중에는 만족도 감소 멈춤
            while (IsTutorialFrozen())
            {
                yield return null;
            }

            yield return new WaitForSeconds(1f);

            // 기다리는 도중 튜토리얼이 시작됐으면 이번 감소도 스킵
            if (IsTutorialFrozen())
                continue;

            currentSatisfaction -= decreasePerSecond;

            if (currentSatisfaction < 0)
            {
                currentSatisfaction = 0;
            }

            if (currentSatisfaction == 0)
            {
                if (Order_Manager.Instance != null)
                {
                    Order_Manager.Instance.SatisfactionZero();
                }
            }
        }

        isRunning = false;
    }

    private bool IsTutorialFrozen()
    {
        return TutorialController.Instance != null &&
               TutorialController.Instance.IsTutorialActive;
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
            isRunning = false;
        }

        StartCoroutine(SatisfactionTimer());
    }

    public float GetCurrentSatisfaction()
    {
        return currentSatisfaction;
    }
}