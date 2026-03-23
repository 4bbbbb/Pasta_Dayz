using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using TMPro;


public class TextUI : MonoBehaviour
{
    public static Day_Manager Instance;

    public TextMeshProUGUI dayText;
    public TextMeshProUGUI revenueText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI refundText;
    public TextMeshProUGUI tipText;
    public TextMeshProUGUI netProfitText;

    void Start()
    {
        dayText.text = $"{Day_Manager.Instance.day}ÀÏÂ÷ Á¤»ê";
        revenueText.text = $" {Gold_Manager.Instance.dailyRevenue}";  // ÃÑ¼öÀÍ
        costText.text = $"{Gold_Manager.Instance.dailyCost}";  // ÃÑ Àç·áºñ
        refundText.text = $"{Gold_Manager.Instance.dailyRefund}"; // ÃÑ È¯ºÒ
        tipText.text = $"{Gold_Manager.Instance.dailyTip}";  // ÃÑ ÆÁ
        netProfitText.text = $"{Gold_Manager.Instance.DailyNetProfit():F1}";  // ¼ø¼öÀÍ
     }
}
