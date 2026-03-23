using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Gold_Manager : MonoBehaviour
{
    public static Gold_Manager Instance;

    [Header("UI")]
    [HideInInspector] public TextMeshProUGUI goldText;

    [Header("총 골드")]
    public float totalGold = 0f;

    [Header("하루 단위 통계")]
    public float dailyRevenue = 0f;   // 손님에게 받은 금액 합
    public float dailyCost = 0f;      // 장사에 사용한 재료비 합
    public float dailyRefund = 0f;    // 환불 합
    public float dailyTip = 0f;       // 팁 합
    public int dailyTipCount = 0;

    public System.Action OnTipChanged;


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

        dailyTipCount++; 

        OnTipChanged?.Invoke(); 

        UpdateUI();
    }


    // 장사용 재료비
    public void SpendBusinessCost(float amount)
    {
        totalGold -= amount;
        dailyCost += amount;
        UpdateUI();
    }

    // 상점 구매
    public void SpendShop(float amount)
    {
        totalGold -= amount;
        UpdateUI();
    }

    public void Refund(float amount)
    {
        totalGold -= amount;
        dailyRefund += amount;
        UpdateUI();
    }

    public float DailyNetProfit()
    {
        return dailyRevenue + dailyTip - dailyCost - dailyRefund;
    }

    public void SetUIText(TextMeshProUGUI text)
    {
        goldText = text;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (goldText != null)
        {
            goldText.text = $"{totalGold:F2}";
        }
    }

    public void ResetDailyStats()
    {
        dailyRevenue = 0f;
        dailyCost = 0f;
        dailyRefund = 0f;
        dailyTip = 0f;
        dailyTipCount = 0; 
    }
}