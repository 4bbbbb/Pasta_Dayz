using UnityEngine;
using UnityEngine.UI;

public class DayTextUI : MonoBehaviour
{
    public Text dayText;

    void Update()
    {
        if (Day_Manager.Instance == null || dayText == null) return;

        dayText.text = $"{Day_Manager.Instance.day}ÀÏÂ÷";
    }
}
