using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;
using static Sauces;

public class BakedPasta : MonoBehaviour, IInteractable
{
    [Header("<<파슬리 프리팹>>")]
    [SerializeField] private GameObject parsleyPrefab;

    [Header("<<파슬리 스폰 위치>>")]
    [SerializeField] Transform parsleySpawnPoint;

    [Header("<<상태별 크기>>")]
    [SerializeField] private Vector3 inOvenScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 platedScale = new Vector3(0.7f, 0.7f, 1f);

    [Header("<<클릭용 콜라이더>>")]
    [SerializeField] private Collider foodCollider;

    [Header("<<선택 연출>>")]
    [SerializeField] private float selectScaleDuration = 0.12f;
    [SerializeField] private float selectedScaleMultiplier = 1.08f;

    public enum BakedState
    {
        InOven,
        Plated
    }

    private BakedState currentState;

    [System.Serializable]
    public class BakedCheeseSpriteEntry
    {
        public int sauceID;
        public int plateID;
        public int cheeseID;
        public BakedState state;
        public Sprite sprite;
    }

    [SerializeField] private List<BakedCheeseSpriteEntry> bakedCheeseEntries = new List<BakedCheeseSpriteEntry>();

    private SpriteRenderer sr;

    public bool isSelected { get; private set; }
    public bool isBeingTrashed { get; private set; } = false;

    private bool canPick = false;
    public bool CanBeSelected => canPick;

    private HashSet<int> ingredientIDs = new HashSet<int>();

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (foodCollider == null)
            foodCollider = GetComponent<Collider>();

        currentState = BakedState.InOven;
        ApplyStateVisual();

        // 처음 생성될 때는 오븐 안에 있으므로 클릭 막기
        SetPickable(false);
    }

    public void SetIngredients(HashSet<int> ids)
    {
        ingredientIDs = new HashSet<int>(ids);
        UpdateBakedSprite();
    }

    public HashSet<int> GetIngredientSet()
    {
        return new HashSet<int>(ingredientIDs);
    }

    public float GetCost()
    {
        if (IngredientDatabase.Instance == null)
            return 0f;

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
        if (!canPick || isBeingTrashed) return false;

        if (target == null)
        {
            Debug.Log("잘 구워진 파스타 선택!");
            Select();
            return true;
        }

        if (target is Topping_Parsley parsley)
        {
            Debug.Log("파슬리를 뿌렸어요");
            parsley.Sprinkle(parsleySpawnPoint, () =>
            {
                Instantiate(
                    parsleyPrefab,
                    parsleySpawnPoint.position,
                    Quaternion.identity,
                    parsleySpawnPoint
                );
            });

            IngredientIDs id = parsley.GetComponent<IngredientIDs>();
            if (id != null)
                ingredientIDs.Add(id.GetID());

            return true;
        }

        return false;
    }

    public void SetState(BakedState state)
    {
        currentState = state;
        ApplyStateVisual();
        UpdateBakedSprite();
    }

    public void SetPickable(bool value)
    {
        canPick = value;

        Collider[] cols = GetComponentsInChildren<Collider>(true);
        foreach (var col in cols)
        {
            col.enabled = value;
        }
    }

    public void OnTrashed()
    {
        isBeingTrashed = true;
        isSelected = false;
        SetPickable(false);

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

    private Vector3 GetBaseScale()
    {
        return currentState == BakedState.InOven ? inOvenScale : platedScale;
    }

    private void ApplyStateVisual()
    {
        transform.DOKill();

        Vector3 baseScale = GetBaseScale();
        transform.localScale = isSelected
            ? baseScale * selectedScaleMultiplier
            : baseScale;
    }

    public void AddIngredient(int id)
    {
        ingredientIDs.Add(id);
        UpdateBakedSprite();
    }

    private int GetSauceID()
    {
        if (ingredientIDs.Contains(202)) return 202;
        if (ingredientIDs.Contains(203)) return 203;
        if (ingredientIDs.Contains(204)) return 204;
        if (ingredientIDs.Contains(205)) return 205;
        if (ingredientIDs.Contains(201)) return 201;
        return -1;
    }

    private int GetPlateID()
    {
        if (ingredientIDs.Contains(502)) return 502;
        return -1;
    }

    private int GetCheeseID()
    {
        if (ingredientIDs.Contains(402)) return 402;
        return -1;
    }

    private void UpdateBakedSprite()
    {
        int sauceID = GetSauceID();
        int plateID = GetPlateID();
        int cheeseID = GetCheeseID();

        if (sauceID == -1) return;

        foreach (var entry in bakedCheeseEntries)
        {
            if (entry.sauceID == sauceID &&
                entry.plateID == plateID &&
                entry.cheeseID == cheeseID &&
                entry.state == currentState)
            {
                sr.sprite = entry.sprite;
                return;
            }
        }

        Debug.Log($"state={currentState}, sauce={sauceID}, plate={plateID}, cheese={cheeseID}");
    }

    void Select()
    {
        isSelected = true;
        transform.DOKill();

        Vector3 baseScale = GetBaseScale();
        transform.DOScale(baseScale * selectedScaleMultiplier, selectScaleDuration)
                 .SetEase(Ease.OutBack);
    }

    public void Cancel()
    {
        isSelected = false;
        transform.DOKill();

        Vector3 baseScale = GetBaseScale();
        transform.DOScale(baseScale, selectScaleDuration)
                 .SetEase(Ease.OutQuad);
    }
}