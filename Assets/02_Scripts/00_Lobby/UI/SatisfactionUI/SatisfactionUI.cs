using UnityEngine;
using UnityEngine.UI;

public class SatisfactionUI : MonoBehaviour
{
    public Image satisfactionImage;

    public Sprite[] satisfactionSprites; 

    void Update()
    {
        if (CustomerSatisfaction_Manager.Instance == null)
        {
            return;
        }

        float ratio = CustomerSatisfaction_Manager.Instance.GetSatisfactionRatio();

        if (ratio >= 0.8f)
        {
            satisfactionImage.sprite = satisfactionSprites[0];
        }
        else if (ratio >= 0.6f)
        {
            satisfactionImage.sprite = satisfactionSprites[1];
        }
        else if (ratio >= 0.4f)
        {
            satisfactionImage.sprite = satisfactionSprites[2];
        }
        else
        {
            satisfactionImage.sprite = satisfactionSprites[3];
        }
    }
}
