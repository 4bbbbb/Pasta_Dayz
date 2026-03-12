using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    public Text lvText;
    public Text xpText;
    public Text dayText;
    public Image xpImage;

    void OnEnable()
    {
        if (Level_Manager.Instance != null)
        {
            // UI가 켜질 때 Level_Manager에 자신 등록
            Level_Manager.Instance.RegisterXPUI(this);
            // 켜질 때 현재 값으로 바로 갱신
            Level_Manager.Instance.UpdateUI();
        }

        if(Day_Manager.Instance != null)
        {
            Day_Manager.Instance.RegisterDayUI(this);
            Day_Manager.Instance.UpdateUI();
        }
    }

    void OnDisable()
    {
        if (Level_Manager.Instance != null)
        {
            // UI가 꺼질 때 연결 해제 (옵션)
            Level_Manager.Instance.RegisterXPUI(null);
        }
        if(Day_Manager.Instance != null)
        {
            Day_Manager.Instance.RegisterDayUI(null); 
        }
    }
}