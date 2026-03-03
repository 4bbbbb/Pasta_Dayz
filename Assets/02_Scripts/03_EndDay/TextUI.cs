using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextUI : MonoBehaviour
{    
    public Text revenueText;
    public Text costText;
    public Text refundText;
    public Text tipText;
    public Text netProfitText;

    void Start()
    {
        revenueText.text = $"ÃÑ ¼öÀÍ: {Gold_Manager.Instance.dailyRevenue}";
        costText.text = $"ÃÑ Àç·áºñ: {Gold_Manager.Instance.dailyCost}";
        refundText.text = $"ÃÑ È¯ºÒ: {Gold_Manager.Instance.dailyRefund}";
        tipText.text = $"ÃÑ ÆÁ: {Gold_Manager.Instance.dailyTip}";
        netProfitText.text = $"¼ø¼öÀÍ: {Gold_Manager.Instance.DailyNetProfit():F1}";
    }
}
