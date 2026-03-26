using System.Collections;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Noodles_CookedNoodle : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    private Cooker_PastaCooker pastaCooker;
    private Coroutine animRoutine;

    public bool isSelected { get; private set; }
    public bool isBeingTrashed { get; private set; } = false;
    public bool CanBeSelected => !isLocked && !isBeingTrashed;

    private bool isLocked = true;

    [Header("기본 스프라이트")]
    [SerializeField] private Sprite normalSprite;

    [Header("면 선택 연출")]
    [SerializeField] private Vector3 normalScale = new Vector3(0.7f, 0.7f, 1f);
    [SerializeField] private Vector3 selectedScale = new Vector3(0.82f, 0.82f, 1f);
    [SerializeField] private Vector3 selectedLocalOffset = new Vector3(0f, 0.12f, 0f);
    [SerializeField] private float animDuration = 0.2f;

    private Vector3 originalLocalPos;
    private Collider[] cachedColliders;
    private IngredientIDs ingredientIDComp;

    public void SetPastaCooker(Cooker_PastaCooker cooker)
    {
        pastaCooker = cooker;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalLocalPos = transform.localPosition;
        cachedColliders = GetComponentsInChildren<Collider>(true);
        ingredientIDComp = GetComponent<IngredientIDs>();
    }

    void Start()
    {
        sr.sprite = normalSprite;
        transform.localScale = normalScale;
        transform.localPosition = originalLocalPos;
    }

    public bool Interact(IInteractable target)
    {
        if (isLocked || isBeingTrashed) return false;
        if (target != null) return false;

        isSelected = true;
        PlayNoodleAnimation(true);

        return true;
    }

    public void Unlock()
    {
        isLocked = false;
    }

    public float GetCost()
    {
        if (IngredientDatabase.Instance == null || ingredientIDComp == null)
            return 0f;

        IngredientData data = IngredientDatabase.Instance.GetIngredient(ingredientIDComp.GetID());
        if (data == null)
            return 0f;

        return data.ingredientCost;
    }

    public void OnTrashed()
    {
        isBeingTrashed = true;
        isSelected = false;
        isLocked = true;

        if (animRoutine != null)
        {
            StopCoroutine(animRoutine);
            animRoutine = null;
        }

        foreach (var col in cachedColliders)
        {
            col.enabled = false;
        }

        if (sr != null)
            sr.color = Color.white;
    }

    public void PlayTrashEffect(Transform trashTarget)
    {
        float moveDuration = 0.9f;
        float fadeDuration = 0.31f;

        Vector3 targetPos = trashTarget.position;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMove(targetPos, moveDuration)
            .SetEase(Ease.OutQuad));

        seq.Join(transform.DOScale(Vector3.zero, moveDuration)
            .SetEase(Ease.InQuad));

        if (sr != null)
        {
            seq.Append(sr.DOFade(0f, fadeDuration));
        }

        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
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