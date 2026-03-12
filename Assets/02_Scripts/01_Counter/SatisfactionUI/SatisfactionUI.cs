using UnityEngine;
using UnityEngine.UI;

public class SatisfactionUI : MonoBehaviour
{
    public Image satisfactionBar;

    void Update()
    {
        if (CustomerSatisfaction_Manager.Instance == null)
        {
            return;
        }

        float ratio = CustomerSatisfaction_Manager.Instance.GetSatisfactionRatio();
        satisfactionBar.fillAmount = ratio;

        if (ratio >= 0.8f)
        {
            satisfactionBar.color = Color.green;
        }
        else if (ratio >= 0.6f)
        {
            satisfactionBar.color = Color.yellow;
        }
        else if (ratio >= 0.4f)
        {
            satisfactionBar.color = new Color(1, 0.65f, 0);
        }
        else
        {
            satisfactionBar.color = Color.red;
        }
    }
}