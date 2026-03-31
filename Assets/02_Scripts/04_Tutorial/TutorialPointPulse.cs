using UnityEngine;
using UnityEngine.UI;

public class TutorialPointPulse : MonoBehaviour
{
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private RectTransform targetRect;

    [Header("색상")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(0.7f, 1f, 0.2f, 1f);

    [Header("모션")]
    [SerializeField] private float blinkSpeed = 4f;
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float moveDistance = 12f;

    private Vector2 baseAnchoredPos;

    private void Awake()
    {
        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();

        if (targetRect != null)
            baseAnchoredPos = targetRect.anchoredPosition;
    }

    private void OnEnable()
    {
        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();

        if (targetRect != null)
            targetRect.anchoredPosition = baseAnchoredPos;

        if (targetGraphic != null)
            targetGraphic.color = normalColor;
    }

    private void OnDisable()
    {
        if (targetRect != null)
            targetRect.anchoredPosition = baseAnchoredPos;

        if (targetGraphic != null)
            targetGraphic.color = normalColor;
    }

    private void Update()
    {
        float blinkT = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * blinkSpeed * Mathf.PI * 2f);
        float moveT = Mathf.Sin(Time.unscaledTime * moveSpeed * Mathf.PI * 2f) * moveDistance;

        if (targetGraphic != null)
            targetGraphic.color = Color.Lerp(normalColor, highlightColor, blinkT);

        if (targetRect != null)
        {
            Vector2 moveDir = GetLocalRightDirection(targetRect);
            targetRect.anchoredPosition = baseAnchoredPos + moveDir * moveT;
        }
    }

    private Vector2 GetLocalRightDirection(RectTransform rect)
    {
        float z = rect.localEulerAngles.z * Mathf.Deg2Rad;

        // -> 모양 기준 local right 방향
        Vector2 dir = new Vector2(Mathf.Cos(z), Mathf.Sin(z));
        return dir.normalized;
    }
}