using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DayTextUI : MonoBehaviour
{
    public TextMeshProUGUI dayText;

    void Update()
    {
        if (Day_Manager.Instance == null || dayText == null) return;

        dayText.text = $"{Day_Manager.Instance.day}ÀÏÂ÷";
    }
}
