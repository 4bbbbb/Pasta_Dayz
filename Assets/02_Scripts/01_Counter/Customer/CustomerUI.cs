using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public Image customerImage;
    public GameObject bubbleObject;
    public CanvasGroup cg;
    public TextMeshProUGUI orderText;
    public GameObject yesButton;
    public GameObject autoButton;

    [Header("Button Pop Animation")]
    [SerializeField] private float buttonPopStartScale = 0.7f;
    [SerializeField] private float buttonPopOvershootScale = 1.12f;
    [SerializeField] private float buttonPopDuration = 0.18f;
    [SerializeField] private float buttonPopInterval = 0.06f;

    [Header("Final Button Scale")]
    [SerializeField] private Vector3 yesButtonTargetScale = new Vector3(1f, 1.4125f, 1f);
    [SerializeField] private Vector3 autoButtonTargetScale = new Vector3(1f, 1.4125f, 1f);

    private int currentIndex = -1;

    private Coroutine typingRoutine;
    private Coroutine buttonPopRoutine;

    void Awake()
    {
        bubbleObject.SetActive(false);
        yesButton.SetActive(false);
        autoButton.SetActive(false);

        cg = bubbleObject.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = bubbleObject.AddComponent<CanvasGroup>();

        ResetButtonScale();
    }

    public void Appear()
    {
        gameObject.SetActive(true);
        bubbleObject.SetActive(false);
        yesButton.SetActive(false);
        autoButton.SetActive(false);

        ResetButtonScale();
    }

    public void SetCustomerSprite(int index)
    {
        currentIndex = index;
        customerImage.sprite = customerSprites[index].happy;
    }

    public void SetEmotion(bool success)
    {
        Debug.Log("currentIndex: " + currentIndex);

        if (currentIndex < 0 || currentIndex >= customerSprites.Count)
            return;

        if (success)
            customerImage.sprite = customerSprites[currentIndex].happy;
        else
            customerImage.sprite = customerSprites[currentIndex].angry;
    }

    public void ShowOrder(string message)
    {
        StopAllUIRoutines();
        typingRoutine = StartCoroutine(ShowBubbleDelay(message));
    }

    IEnumerator ShowBubbleDelay(string message)
    {
        yield return new WaitForSeconds(0.7f);

        Transform bubble = bubbleObject.transform;

        bubble.localScale = Vector3.one * 0.8f;
        bubble.localRotation = Quaternion.identity;
        cg.alpha = 0f;

        bubbleObject.SetActive(true);
        yesButton.SetActive(false);
        autoButton.SetActive(false);
        ResetButtonScale();

        float time = 0f;
        float duration = 0.5f;

        Vector3 startScale = Vector3.one * 0.8f;
        Vector3 targetScale = Vector3.one * 1.12f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float eased = 1 - Mathf.Pow(1 - t, 2);

            bubble.localScale = Vector3.Lerp(startScale, targetScale, eased);
            cg.alpha = eased;

            yield return null;
        }

        bubble.localScale = targetScale;
        cg.alpha = 1f;

        yield return StartCoroutine(TypeText(message));

        buttonPopRoutine = StartCoroutine(ShowButtonsPop());
    }

    IEnumerator TypeText(string message)
    {
        orderText.text = "";

        foreach (char c in message)
        {
            orderText.text += c;
            yield return new WaitForSeconds(0.03f);
        }
    }

    IEnumerator ShowButtonsPop()
    {
        yield return StartCoroutine(PopButton(yesButton, yesButtonTargetScale));
        yield return new WaitForSeconds(buttonPopInterval);
        yield return StartCoroutine(PopButton(autoButton, autoButtonTargetScale));
    }

    IEnumerator PopButton(GameObject buttonObj, Vector3 targetScale)
    {
        if (buttonObj == null)
            yield break;

        Transform tr = buttonObj.transform;

        buttonObj.SetActive(true);

        Vector3 startScale = targetScale * buttonPopStartScale;
        Vector3 overshootScale = targetScale * buttonPopOvershootScale;

        tr.localScale = startScale;

        float halfDuration = buttonPopDuration * 0.5f;
        float time = 0f;

        while (time < halfDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / halfDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            tr.localScale = Vector3.Lerp(startScale, overshootScale, eased);
            yield return null;
        }

        tr.localScale = overshootScale;

        time = 0f;

        while (time < halfDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / halfDuration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);

            tr.localScale = Vector3.Lerp(overshootScale, targetScale, eased);
            yield return null;
        }

        tr.localScale = targetScale;
    }

    private void ResetButtonScale()
    {
        if (yesButton != null)
            yesButton.transform.localScale = yesButtonTargetScale;

        if (autoButton != null)
            autoButton.transform.localScale = autoButtonTargetScale;
    }

    private void StopAllUIRoutines()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (buttonPopRoutine != null)
        {
            StopCoroutine(buttonPopRoutine);
            buttonPopRoutine = null;
        }
    }

    public void HideBubble()
    {
        StopAllUIRoutines();

        bubbleObject.SetActive(false);
        yesButton.SetActive(false);
        autoButton.SetActive(false);

        ResetButtonScale();
    }

    public void ShowResult(string result)
    {
        StopAllUIRoutines();

        bubbleObject.SetActive(true);
        yesButton.SetActive(false);
        autoButton.SetActive(false);

        ResetButtonScale();

        typingRoutine = StartCoroutine(TypeText(result));
    }
}