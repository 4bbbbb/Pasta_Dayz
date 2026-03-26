using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Plates_BasicPlate : MonoBehaviour, IInteractable
{
    [Header("<<완성된 파스타 스폰위치>>")]
    [SerializeField] private Transform pastaSpawnPoint;

    [Header("<<구워진 빠네 스폰위치>>")]
    [SerializeField] private Transform paneSpawnPoint;

    [Header("<<구워진 빠네 프리팹>>")]
    [SerializeField] private GameObject paneOnPlatePrefab;

    [Header("<<선택 연출>>")]
    [SerializeField] private float selectedScaleMultiplier = 1.08f;

    private SpriteRenderer sr;
    private Collider plateCollider;

    public bool isSelected { get; private set; }
    public bool isBeingTrashed { get; private set; } = false;

    // 빈 접시도 선택 가능
    public bool CanBeSelected => !isBeingTrashed;

    private GameObject currentPaneVisual;

    private bool hasPasta = false;
    private bool hasPane = false;

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

        ingredients.Clear();

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
            if (hasPasta)
            {
                Debug.Log("이미 파스타가 올라가 있습니다.");
                return false;
            }

            Select();
            return true;
        }

        // 1) 완성 파스타를 접시에 올릴 때
        if (target is FinishedPasta finishedPasta)
        {
            if (hasPasta)
            {
                Debug.Log("이미 파스타가 담겨 있어요!");
                return false;
            }

            if (!finishedPasta.CanMoveToPlate(plateID, hasPane))
            {
                Debug.Log("옮길 수 없습니다.");
                return false;
            }

            finishedPasta.transform.SetParent(pastaSpawnPoint, true);
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

            // 빠네 비주얼 제거
            if (currentPaneVisual != null)
            {
                Destroy(currentPaneVisual);
                currentPaneVisual = null;
            }

            // plate 자체 비주얼은 숨기고, 클릭도 막아줌
            if (sr != null)
                sr.enabled = false;

            if (plateCollider != null)
                plateCollider.enabled = false;

            finishedPasta.OnMovedToPlate();

            PrintIngredients();
            return true;
        }

        // 2) 구워진 빠네를 접시에 올릴 때
        if (target is Plate_BakedPane bakedPane)
        {
            if (hasPasta)
            {
                Debug.Log("지금은 빠네를 추가할 수 없어요.");
                return false;
            }

            if (hasPane)
            {
                Debug.Log("이미 빠네가 준비되어 있어요!");
                return false;
            }

            IngredientIDs id = bakedPane.GetComponent<IngredientIDs>();
            if (id != null)
                ingredients.Add(id.GetID());
            else
                ingredients.Add(601);

            hasPane = true;
            isSelected = false;
            transform.localScale = originalScale;

            if (paneOnPlatePrefab != null && paneSpawnPoint != null)
            {
                currentPaneVisual = Instantiate(paneOnPlatePrefab, paneSpawnPoint);
                currentPaneVisual.transform.localPosition = Vector3.zero;
                currentPaneVisual.transform.localRotation = Quaternion.identity;
                currentPaneVisual.transform.localScale = Vector3.one;
            }
            else
            {
                Debug.LogWarning("paneOnPlatePrefab 또는 paneSpawnPoint가 비어 있습니다.");
            }

            Destroy(bakedPane.gameObject);

            PrintIngredients();
            return true;
        }

        return false;
    }

    // 완전히 빈 접시는 비용 0원
    public float GetCost()
    {
        if (!hasPane && !hasPasta)
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