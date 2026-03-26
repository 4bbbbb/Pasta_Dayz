using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;
using DG.Tweening;


public class Burned : MonoBehaviour, IInteractable
{
    public enum BurnedType
    {
        Pane,
        Pasta
    }

    public BurnedType type;

    private HashSet<int> ingredientIDs = new HashSet<int>();

    public bool isBeingTrashed { get; private set; } = false;
    public bool CanBeSelected => true;

    private Vector3 originalScale;
    [SerializeField] private float selectedScaleMultiplier = 1.2f;

    private bool isSelected = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void SetIngredients(HashSet<int> ids)
    {
        if (ids != null)
            ingredientIDs = new HashSet<int>(ids);
    }

    public HashSet<int> GetIngredientSet()
    {
        return ingredientIDs;
    }

    public float GetCost()
    {
        if (type == BurnedType.Pane)
        {
            return 3f;
        }

        if (IngredientDatabase.Instance == null)
        {
            return 0f;
        }

        float total = 0f;

        foreach (int id in ingredientIDs)
        {
            IngredientData data = IngredientDatabase.Instance.GetIngredient(id);

            if (data != null)
            {
                total += data.ingredientCost;
            }
        }

        return total;
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Select();
            Debug.Log("탄 음식 선택");
            return true;
        }

        return false;
    }

    private void Select()
    {
        isSelected = true;
        transform.localScale = originalScale * selectedScaleMultiplier;
    }

    public void OnTrashed()
    {
        isBeingTrashed = true;
    }


    public void PlayTrashEffect(Transform trashTarget)
    {
        float moveDuration = 0.9f;
        float effectDuration = 0.31f;

        Vector3 targetPos = trashTarget.position;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMove(targetPos, moveDuration)
            .SetEase(Ease.OutQuad));

        seq.Join(transform.DOScale(Vector3.zero, moveDuration)
            .SetEase(Ease.InQuad));


        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        seq.AppendCallback(() =>
        {

        });

        if (sr != null)
        {
            seq.Append(sr.DOFade(0f, effectDuration));
        }

        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void OnDestroy()
    {
        if (!isBeingTrashed) return;

        Cooker_Oven oven = GetComponentInParent<Cooker_Oven>();

        if (oven != null)
        {
            oven.OnBurnedRemoved();
        }
    }


    public void Cancel()
    {
        isSelected = false;
        transform.localScale = originalScale;
    }

}
