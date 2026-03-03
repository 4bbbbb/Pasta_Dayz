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
        revenueText.text = Gold_Manager.Instance.dailyRevenue.ToString();
        costText.text = Gold_Manager.Instance.dailyCost.ToString();
        refundText.text = Gold_Manager.Instance.dailyRefund.ToString();
        tipText.text = Gold_Manager.Instance.dailyTip.ToString();
        netProfitText.text = Gold_Manager.Instance.DailyNetProfit().ToString();
    }
}
