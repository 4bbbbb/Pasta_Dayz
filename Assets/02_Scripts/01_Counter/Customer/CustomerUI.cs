using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerUI : MonoBehaviour
{
    [System.Serializable]
    public class CustomerSpriteSet
    {
        public Sprite happy;
        public Sprite angry;
    }

    [Header("Customer Sprites")]
    public List<CustomerSpriteSet> customerSprites;

    [Header("UI References")]
    public Image customerImage;     // 프리팹에 미리 연결된 이미지
    public GameObject bubbleObject; // 말풍선 오브젝트
    public Text orderText;          // 주문 텍스트
    public GameObject yesButton;    // 네 버튼
    public GameObject autoButton;   // 자동 완성 버튼

    private int currentIndex = -1;

    void Awake()
    {
        bubbleObject.SetActive(false);
        yesButton.SetActive(false);
        autoButton.SetActive(false);
    }

    // 손님 등장
    public void Appear()
    {
        gameObject.SetActive(true);
        bubbleObject.SetActive(false);
        yesButton.SetActive(false);
        autoButton.SetActive(false);

    }

    //  손님 스프라이트 설정 (처음 등장 시 호출)
    public void SetCustomerSprite(int index)
    {
        currentIndex = index;
        customerImage.sprite = customerSprites[index].happy;
    }

    //  감정 변경 (성공/실패에 따라)
    public void SetEmotion(bool success)
    {
        Debug.Log("currentIndex: " + currentIndex);

        if (currentIndex < 0 || currentIndex >= customerSprites.Count)
            return;

        if (success)
        {
            customerImage.sprite = customerSprites[currentIndex].happy;
        }
        else
        {
            customerImage.sprite = customerSprites[currentIndex].angry;

        }
    }

    public void ShowOrder(string message)
    {
        orderText.text = message;
        StartCoroutine(ShowBubbleDelay());        
    }

    IEnumerator ShowBubbleDelay()
    {
        yield return new WaitForSeconds(1.5f);
        bubbleObject.SetActive(true);
        yesButton.SetActive(true);
        autoButton.SetActive(true);

    }

    public void HideBubble()
    {
        bubbleObject.SetActive(false);
        yesButton.SetActive(false);
        autoButton.SetActive(false);

    }

    public void ShowResult(string result)
    {
        orderText.text = result;

        bubbleObject.SetActive(true);
        yesButton.SetActive(false);
        autoButton.SetActive(false);
    }
}