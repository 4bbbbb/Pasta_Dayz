using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CustomerUI : MonoBehaviour
{
    [Header("UI References")]
    public Image customerImage;     // 프리팹에 미리 연결된 이미지
    public GameObject bubbleObject; // 말풍선 오브젝트
    public Text orderText;          // 주문 텍스트

    void Awake()
    {
        bubbleObject.SetActive(false); // 시작 시 말풍선 숨김
    }

    // 🔥 손님 등장
    public void Appear()
    {
        gameObject.SetActive(true);   // 손님 이미지 바로 표시
        bubbleObject.SetActive(false);
    }

    // 🔥 주문 표시 (0.2초 뒤 말풍선 등장)
    public void ShowOrder(string message)
    {
        orderText.text = message;
        StartCoroutine(ShowBubbleDelay());
    }

    IEnumerator ShowBubbleDelay()
    {
        yield return new WaitForSeconds(0.2f);
        bubbleObject.SetActive(true);
    }

    public void HideOrder()
    {
        bubbleObject.SetActive(false);
    }

    public void Disappear()
    {
        Destroy(gameObject);
    }
}