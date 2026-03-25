using System.Collections;
using UnityEngine;
using static IInteractableScript;

public class Noodles_CookedNoodle : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    private Cooker_PastaCooker pastaCooker;
    private Coroutine animRoutine;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    private bool isLocked = true;


    [Header("기본 스프라이트")]
    [SerializeField] private Sprite normalSprite;

    [Header("면 선택 연출")]
    [SerializeField] private Vector3 normalScale = new Vector3(0.7f, 0.7f, 1f);
    [SerializeField] private Vector3 selectedScale = new Vector3(0.82f, 0.82f, 1f);
    [SerializeField] private Vector3 selectedLocalOffset = new Vector3(0f, 0.12f, 0f);
    [SerializeField] private float animDuration = 0.2f;

    private Vector3 originalLocalPos;

    public void SetPastaCooker(Cooker_PastaCooker cooker)
    {
        pastaCooker = cooker;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalLocalPos = transform.localPosition;
    }

    void Start()
    {
        sr.sprite = normalSprite;
        transform.localScale = normalScale;
        transform.localPosition = originalLocalPos;
    }

    public bool Interact(IInteractable target)
    {
        if (isLocked) return false; 

        if (target != null) return false;

        isSelected = true;
        PlayNoodleAnimation(true);

        return true;
    }

    public void Unlock()
    {
        isLocked = false;
    }



    public void Cancel()
    {
        isSelected = false;
        PlayNoodleAnimation(false);       
    }

    private void PlayNoodleAnimation(bool selected)
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        Vector3 targetScale = selected ? selectedScale : normalScale;
        Vector3 targetPos = selected ? originalLocalPos + selectedLocalOffset : originalLocalPos;

        animRoutine = StartCoroutine(AnimateNoodle(targetScale, targetPos));
    }

    private IEnumerator AnimateNoodle(Vector3 targetScale, Vector3 targetPos)
    {
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.localPosition;

        float time = 0f;

        while (time < animDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / animDuration);
            t = t * t * (3f - 2f * t);

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        transform.localScale = targetScale;
        transform.localPosition = targetPos;
        animRoutine = null;
    }
}