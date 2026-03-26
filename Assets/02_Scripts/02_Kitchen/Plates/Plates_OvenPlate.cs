using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Plates_OvenPlate : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    private Collider plateCollider;

    [Header("선택 연출")]
    [SerializeField] private float selectedScaleMultiplier = 1.08f;

    public bool isSelected { get; private set; }
    public bool isBeingTrashed { get; private set; } = false;

    // 빈 접시도 선택 가능
    public bool CanBeSelected => !isBeingTrashed;

    private bool hasPasta = false;
    private int plateID;
    private IngredientIDs ingredientIDs;
    private HashSet<int> ingredients = new HashSet<int>();

    private Vector3 originalScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        plateCollider = GetComponent<Collider>();
        isSelected = false;
        originalScale = transform.localScale;

        ingredientIDs = GetComponent<IngredientIDs>();

        if (ingredientIDs != null)
        {
            plateID = ingredientIDs.GetID();
            ingredients.Add(plateID);   // 접시 ID만 먼저 넣어둠
        }
        else
        {
            Debug.LogWarning($"{name}: IngredientIDs가 없습니다.");
        }
    }

    public bool Interact(IInteractable target)
    {
        if (isBeingTrashed)
            return false;

        // 빈 접시도 선택 가능
        if (target == null)
        {
            Select();
            return true;
        }

        if (target is FinishedPasta finishedPasta)
        {
            if (hasPasta)
            {
                Debug.Log("이미 파스타가 담겨 있어요!");
                return false;
            }

            // oven plate는 hasPane 개념 없음
            if (!finishedPasta.CanMoveToPlate(plateID, false))
            {
                Debug.Log("옮길 수 없습니다.");
                return false;
            }

            finishedPasta.transform.SetParent(transform, true);
            finishedPasta.transform.localPosition = Vector3.zero;
            finishedPasta.transform.localRotation = Quaternion.identity;
            finishedPasta.transform.localScale = Vector3.one;

            HashSet<int> finalIngredients = new HashSet<int>(ingredients);
            foreach (int id in finishedPasta.GetIngredientSet())
            {
                finalIngredients.Add(id);
            }

            finishedPasta.SetIngredients(finalIngredients);
            ingredients = new HashSet<int>(finalIngredients);

            hasPasta = true;
            isSelected = false;
            transform.localScale = originalScale;

            // 원래 오븐 접시 비주얼 숨김
            if (sr != null)
                sr.enabled = false;

            if (plateCollider != null)
                plateCollider.enabled = false;

            finishedPasta.OnMovedToPlate();
            PrintIngredients();
            return true;
        }

        return false;
    }

    // 빈 접시는 비용 0원
    public float GetCost()
    {
        if (!hasPasta)
            return 0f;

        if (IngredientDatabase.Instance == null)
            return 0f;

        float total = 0f;

        foreach (int id in ingredients)
        {
            IngredientData data = IngredientDatabase.Instance.GetIngredient(id);
            if (data != null)
            {
                total += data.ingredientCost;
            }
        }

        return total;
    }

    public void OnTrashed()
    {
        isBeingTrashed = true;
        isSelected = false;
        transform.localScale = originalScale;

        if (plateCollider != null)
            plateCollider.enabled = false;
    }

    public void PlayTrashEffect(Transform trashTarget)
    {
        float moveDuration = 0.9f;
        float fadeDuration = 0.31f;
        Vector3 targetPos = trashTarget.position;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMove(targetPos, moveDuration).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(Vector3.zero, moveDuration).SetEase(Ease.InQuad));

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            if (r != null)
                seq.Join(r.DOFade(0f, fadeDuration));
        }

        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void Select()
    {
        isSelected = true;
        transform.localScale = originalScale * selectedScaleMultiplier;
    }

    public void AddIngredient(int id)
    {
        if (!ingredients.Contains(id))
        {
            ingredients.Add(id);
        }
    }

    public HashSet<int> GetIngredientSet()
    {
        return new HashSet<int>(ingredients);
    }

    public void PrintIngredients()
    {
        foreach (int id in ingredients)
        {
            Debug.Log("Plate에 포함된 ID: " + id);
        }
    }

    public void Cancel()
    {
        isSelected = false;
        transform.localScale = originalScale;
    }
}