using System.Collections;
using TMPro;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public GameObject bubbleObject;
    public TextMeshPro bubbleText;

    public float popSpeed = 8f;

    void Start()
    {
        bubbleObject.SetActive(false);
    }

    public void Appear()
    {
        transform.localScale = Vector3.zero;
        StartCoroutine(PopIn());
    }

    System.Collections.IEnumerator PopIn()
    {
        while (transform.localScale.x < 1f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.one,
                Time.deltaTime * popSpeed
            );

            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    // 🔥 주문 표시
    public void ShowOrder(string message)
    {
        bubbleObject.SetActive(true);
        bubbleText.text = message;
    }

    public void HideOrder()
    {
        bubbleObject.SetActive(false);
    }

    public void Disappear()
    {
        StartCoroutine(PopOut());
    }

    System.Collections.IEnumerator PopOut()
    {
        while (transform.localScale.x > 0.05f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.zero,
                Time.deltaTime * popSpeed
            );

            yield return null;
        }

        Destroy(gameObject);
    }
}