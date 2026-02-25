using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gold_Manager : MonoBehaviour
{
    public static Gold_Manager Instance;    

    [Header("UI")]
    [HideInInspector] public Text goldText; // 인스펙터에서 연결

    [Header("총 골드")]
    public float totalGold = 0f;

    [Header("하루 단위 통계")]
    public float dailyRevenue = 0f;     // 손님에게 받은 금액 합
    public float dailyCost = 0f;        // 사용한 재료비 합
    public float dailyRefund = 0f;      // 환불 합
    public float dailyTip = 0f;         // 팁 합


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
        UpdateUI();
    }

    public void Earn(float amount)
    {
        totalGold += amount;
        dailyRevenue += amount;
        UpdateUI();
    }
    public void EarnTip(float amount)
    {
        totalGold += amount;
        dailyTip += amount;
        UpdateUI();
    }

    public void Spend(float amount)
    {
        totalGold -= amount;
        dailyCost += amount; // 사용한 재료비 누적
        UpdateUI();
    }

    public void Refund(float amount)
    {
        totalGold -= amount;
        dailyRefund += amount; // 환불 누적
        UpdateUI();
    }

    public float DailyNetProfit()
    {
        // 순수익 = revenue - cost - refund
        return dailyRevenue + dailyTip - dailyCost - dailyRefund;
    }

    public void SetUIText(Text text)
    {
        goldText = text;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (goldText != null)
        {
            goldText.text = $"Gold: {totalGold:F1}";

        }
    }

    public void ResetDailyStats()
    {
        dailyRevenue = 0f;
        dailyCost = 0f;
        dailyRefund = 0f;
    }
}
