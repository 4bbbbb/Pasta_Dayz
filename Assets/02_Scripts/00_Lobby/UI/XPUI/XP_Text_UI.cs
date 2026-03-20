using UnityEngine;
using UnityEngine.UI;

public class XP_Text_UI : MonoBehaviour
{
    public Text lvText;
    public Text xpText;
    public Image xpImage;

    void Update()
    {
        if (Level_Manager.Instance == null) return;

        Level_Manager.Instance.GetXPInfo(out int level, out float current, out float max);

        lvText.text = $"Lv {level}";
        xpText.text = $"{current}/{max}";
        xpImage.fillAmount = current / max;
    }
}
