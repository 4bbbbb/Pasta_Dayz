using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class FinishedPasta : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class PastaPanSpriteEntry
    {
        public int noodleID;      // 100번대
        public int sauceID; // 201, 202, 203, 204, 205
        public Sprite sprite;
    }

    [Header("<<후라이팬>>")]
    [SerializeField] private Cooker_FryingPan fryingPan;

    [Header("<<가스 스토브>>")]
    [SerializeField] private Cooker_GasStove gasStove;


    [Header("<<치즈 프리팹>>")]
    [SerializeField] private GameObject parmesanCheesePrefab;
    [SerializeField] private GameObject mozzarellaCheesePrefab;

    [Header("<<파슬리 프리팹>>")]
    [SerializeField] private GameObject parsleyPrefab;

    [Header("<<치즈, 파슬리 스폰 위치>>")]
    [SerializeField] Transform cheeseSpawnPoint;
    [SerializeField] Transform parsleySpawnPoint;

    [SerializeField] private List<PastaPanSpriteEntry> panSpriteEntries = new List<PastaPanSpriteEntry>();


    private SpriteRenderer sr;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    public bool isOnPlate { get; private set; }
    private bool hasCheese = false;

    private HashSet<int> ingredientIDs = new HashSet<int>();

    private Cheese.CheeseType? addedCheeseType = null;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();       
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("완성된 파스타 선택!");
            Select();
            return true;
        }

        // 치즈
        if (target is Cheese cheese)
        {
            if (!isOnPlate)
            {
                Debug.Log("그릇 위에 올려진 파스타에만 치즈를 추가할 수 있어요!");
                return false;
            }

            if (hasCheese)
            {
                Debug.Log("이미 치즈가 추가되어 있어요!");
                return false;
            }

            GameObject cheesePrefab = cheese.cheeseType switch
            {
                Cheese.CheeseType.Parmesan => parmesanCheesePrefab,
                Cheese.CheeseType.Mozzarella => mozzarellaCheesePrefab,
                _ => null
            };

            if (cheesePrefab == null)
            {
                return false;
            }

            Instantiate(
                cheesePrefab,
                cheeseSpawnPoint.position,
                Quaternion.identity,
                cheeseSpawnPoint
            );

            IngredientIDs id = cheese.GetComponent<IngredientIDs>();
            if (id != null)
            {
                ingredientIDs.Add(id.GetID());   // 치즈 ID 추가
            }

            hasCheese = true;
            addedCheeseType = cheese.cheeseType;

            return true;
        }

        if (isOnPlate && target is Plates_BasicPlate)
        {
            return false;
        }

        if (isOnPlate && target is Plates_OvenPlate)
        {
            return false;
        }

        if (target is Topping_Parsley parsley)
        {
            if (!isOnPlate)
            {
                Debug.Log("그릇 위에 올려진 파스타에만 파슬리를 추가할 수 있어요!");
                return false;
            }

            if (!hasCheese)
            {
                Debug.Log("치즈를 먼저 뿌려주세요!");
                return false;
            }

            Debug.Log("파슬리를 뿌렸어요");
            Instantiate(
                parsleyPrefab,
                parsleySpawnPoint.position,
                Quaternion.identity,
                parsleySpawnPoint
                );

            IngredientIDs id = parsley.GetComponent<IngredientIDs>();
            if (id != null)
            {
                ingredientIDs.Add(id.GetID());   // 파슬리 ID 추가
            }
            return true;
        }

        return false;
    }
    void Select()
    {
        isSelected = true;
        sr.color = Color.red;
    }
    public void Init(Cooker_GasStove stove)
    {
        gasStove = stove;
    }

    public void OnMovedToPlate()
    {
        gasStove.DestroyFryingPan();
        isOnPlate = true;
        Debug.Log("완성된 파스타를 그릇에 담았어요 !!");
        PrintIngredients();
    }
    public bool IsOnOvenPlate()
    {
        return isOnPlate && GetComponentInParent<Plates_OvenPlate>() != null;
    }

    public bool HasMozzarella()
    {
        return addedCheeseType == Cheese.CheeseType.Mozzarella;
    }       

    public void SetIngredients(HashSet<int> ids)
    {
        ingredientIDs = new HashSet<int>(ids);
        UpdatePanSprite();
    }

    private int GetNoodleID()
    {
        foreach (int id in ingredientIDs)
        {
            if (id >= 100 && id < 200)
                return id;
        }

        return -1;
    }

    private int GetSauceID()
    {
        if (ingredientIDs.Contains(202)) return 202; // 토마토
        if (ingredientIDs.Contains(203)) return 203; // 크림
        if (ingredientIDs.Contains(204)) return 204; // 로제
        if (ingredientIDs.Contains(205)) return 205; // 봉골레

        if (ingredientIDs.Contains(201)) return 201; // 알리오올리오

        return -1;
    }

    public void UpdatePanSprite()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        int noodleID = GetNoodleID();
        int sauceID = GetSauceID();

        if (noodleID == -1 || sauceID == -1)
        {
            Debug.LogWarning($"면 또는 소스 ID를 찾지 못함. noodleID={noodleID}, sauceID={sauceID}");
            return;
        }

        foreach (var entry in panSpriteEntries)
        {
            if (entry.noodleID == noodleID && entry.sauceID == sauceID)
            {
                if (entry.sprite != null)
                {
                    sr.sprite = entry.sprite;
                    Debug.Log($"스프라이트 변경 완료: noodle={noodleID}, sauce={sauceID}");
                }
                else
                {
                    Debug.LogWarning($"매칭은 됐지만 sprite가 비어있음: noodle={noodleID}, sauce={sauceID}");
                }
                return;
            }
        }

        Debug.LogWarning($"일치하는 스프라이트 없음: noodle={noodleID}, sauce={sauceID}");
    }

    public HashSet<int> GetIngredientSet()
    {
        return new HashSet<int>(ingredientIDs);
    }

    public void PrintIngredients()
    {
        Debug.Log("현재 ingredientIDs 개수: " + ingredientIDs.Count);

        foreach (int id in ingredientIDs)
        {
            Debug.Log("Plate에 포함된 ID: " + id);
        }
    }

    public void Cancel()
    {
        isSelected = false;
        sr.color = Color.white;
    }
}
