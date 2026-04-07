using UnityEngine;
using UnityEngine.UI;

public class Tip_UI : MonoBehaviour
{
    public Image tipImage;
    public Sprite[] tipSprites; 

    void Start()
    {
        if (Gold_Manager.Instance != null)
        {
            Gold_Manager.Instance.OnTipChanged += UpdateTipUI;
            UpdateTipUI();
        }
    }

    void UpdateTipUI()
    {
        int count = Gold_Manager.Instance.dailyTipCount;

        int index = 0;

        if (count >= 5)
        {
            index = 3;

        }
        else if (count >= 3)
        {
            index = 2;
        }
        else if (count >= 1)
        {
            index = 1;
        }

        tipImage.sprite = tipSprites[index];
    }

    private void OnDestroy()
    {
        Gold_Manager.Instance.OnTipChanged -= UpdateTipUI;
    }
}
