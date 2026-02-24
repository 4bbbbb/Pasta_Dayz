using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CustomerUI : MonoBehaviour
{
    [Header("UI References")]
    public Image customerImage;     // 프리팹에 미리 연결된 이미지
    public GameObject bubbleObject; // 말풍선 오브젝트
    public Text orderText;          // 주문 텍스트
    public GameObject yesButton;    // 네 버튼

    void Awake()
    {
        bubbleObject.SetActive(false);
        yesButton.SetActive(false);
    }

    // 손님 등장
    public void Appear()
    {
        gameObject.SetActive(true);   // 손님 이미지 바로 표시
        bubbleObject.SetActive(false);
        yesButton.SetActive(false);
    }   

    public void ShowOrder(string message)
    {
        orderText.text = message;
        StartCoroutine(ShowBubbleDelay());        
    }

    IEnumerator ShowBubbleDelay()
    {
        yield return new WaitForSeconds(1f);
        bubbleObject.SetActive(true);
        yesButton.SetActive(true);
    }

    public void HideBubble()
    {
        bubbleObject.SetActive(false);
        yesButton.SetActive(false);
    }

    public void ShowResult(string result)
    {
        orderText.text = result;

        bubbleObject.SetActive(true);
        yesButton.SetActive(false);
    }
}