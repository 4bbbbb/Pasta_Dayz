using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class TextUI : MonoBehaviour
{
    public static Day_Manager Instance;

    public Text dayText;
    public Text revenueText;
    public Text costText;
    public Text refundText;
    public Text tipText;
    public Text netProfitText;

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
