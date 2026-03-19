using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class FinishedPasta : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class PanSpriteEntry
    {
        public int noodleID;      
        public int sauceID; 
        public Sprite sprite;
    }

    [System.Serializable]
    public class BasicPlateSpriteEntry
    {
        public int noodleID;   
        public int sauceID;   
        public int plateID;
        public bool hasPane;     
        public Sprite sprite;
    }

    [System.Serializable]
    public class OvenPlateSpriteEntry
    {
        public int noodleID;   
        public int sauceID;    
        public int plateID;        
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

    [SerializeField] private List<PanSpriteEntry> panSpriteEntries = new List<PanSpriteEntry>();
    [SerializeField] private List<BasicPlateSpriteEntry> basicplateSpriteEntries = new List<BasicPlateSpriteEntry>();
    [SerializeField] private List<OvenPlateSpriteEntry> ovenPlateSpriteEntries = new List<OvenPlateSpriteEntry>();


    [Header("<<이펙트 속도>>")]
    [SerializeField] private float fadeDuration = 0.15f;


    private SpriteRenderer sr;
    private Coroutine spriteFadeRoutine;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    private bool hasInitializedSprite = false;

    public bool isOnPlate { get; private set; }
    private bool hasCheese = false;

    private HashSet<int> ingredientIDs = new HashSet<int>();

    private Cheese.CheeseType? addedCheeseType = null;

    void Awake()
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

            if (cheese.cheeseType == Cheese.CheeseType.Parmesan)
            {
                cheese.Sprinkle(cheeseSpawnPoint, () =>
                {
                    Instantiate(
                        cheesePrefab,
                        cheeseSpawnPoint.position,
                        Quaternion.identity,
                        cheeseSpawnPoint
                    );
                });
            }

            else
            {
                //  모짜렐라
                Instantiate(
                    cheesePrefab,
                    cheeseSpawnPoint.position,
                    Quaternion.identity,
                    cheeseSpawnPoint
                );
            }

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

    public bool CanMoveToPlate(int plateID, bool targetHasPane)
    {
        int noodleID = GetNoodleID();
        int sauceID = GetSauceID();

        if (noodleID == -1 || sauceID == -1)
            return false;

        // basic plate
        if (plateID == 501)
        {
            foreach (var entry in basicplateSpriteEntries)
            {
                if (entry.noodleID == noodleID &&
                    entry.sauceID == sauceID &&
                    entry.plateID == plateID &&
                    entry.hasPane == targetHasPane)
                {
                    return true;
                }
            }
        }
        // oven plate
        else if (plateID == 502)
        {
            foreach (var entry in ovenPlateSpriteEntries)
            {
                if (entry.noodleID == noodleID &&
                    entry.sauceID == sauceID &&
                    entry.plateID == plateID)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void SetFryingPan(Cooker_FryingPan pan)
    {
        fryingPan = pan;
    }

    public void OnMovedToPlate()
    {
        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }

        if (spriteFadeRoutine != null)
        {
            StopCoroutine(spriteFadeRoutine);
            spriteFadeRoutine = null;
        }

        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, 1f);

        isOnPlate = true;
        UpdatePlateSprite();

        fryingPan?.ClearPanAfterServing();

        if (gasStove != null)
        {
            gasStove.DestroyFryingPan();
        }

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
    }

    public void AddIngredient(int id)
    {
        ingredientIDs.Add(id);
    }

    public HashSet<int> GetIngredientSet()
    {
        return new HashSet<int>(ingredientIDs);
    }

    public void RefreshSprite()
    {
        if (isOnPlate)
            UpdatePlateSprite();
        else
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

    private int GetPlateID()
    {
        if (ingredientIDs.Contains(501)) return 501; // basic plate
        if (ingredientIDs.Contains(502)) return 502; // oven plate

        return -1;
    }

    private bool HasPane()
    {
        return ingredientIDs.Contains(601);
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
                    // 처음 생성됐을 때는 기본 이미지가 보이기 전에 바로 교체
                    if (!hasInitializedSprite)
                    {
                        StopSpriteFadeAndRestoreAlpha();
                        sr.sprite = entry.sprite;
                        hasInitializedSprite = true;
                        Debug.Log($"초기 팬 스프라이트 바로 적용: noodle={noodleID}, sauce={sauceID}");
                        return;
                    }

                    if (sr.sprite == entry.sprite)
                    {
                        StopSpriteFadeAndRestoreAlpha();
                        return;
                    }

                    StopSpriteFadeAndRestoreAlpha();
                    spriteFadeRoutine = StartCoroutine(FadeChangeSprite(entry.sprite));

                    Debug.Log($"팬 스프라이트 변경 시작: noodle={noodleID}, sauce={sauceID}");
                }
                else
                {
                    Debug.LogWarning($"매칭은 됐지만 sprite가 비어있음: noodle={noodleID}, sauce={sauceID}");
                }
                return;
            }
        }

        Debug.LogWarning($"팬 스프라이트 매칭 실패: noodle={noodleID}, sauce={sauceID}");
    }

    private IEnumerator FadeChangeSprite(Sprite newSprite)
    {
        if (sr == null) yield break;

        Color c = sr.color;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            sr.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        sr.color = new Color(c.r, c.g, c.b, 0f);
        sr.sprite = newSprite;

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            sr.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        sr.color = new Color(c.r, c.g, c.b, 1f);
        spriteFadeRoutine = null;
    }

    public void PreparePanSpriteHidden()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (spriteFadeRoutine != null)
        {
            StopCoroutine(spriteFadeRoutine);
            spriteFadeRoutine = null;
        }

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

                    Color c = sr.color;
                    sr.color = new Color(c.r, c.g, c.b, 0f); // 안 보이게 숨김
                }
                else
                {
                    Debug.LogWarning($"매칭은 됐지만 sprite가 비어있음: noodle={noodleID}, sauce={sauceID}");
                }
                return;
            }
        }

        Debug.LogWarning($"팬 스프라이트 매칭 실패: noodle={noodleID}, sauce={sauceID}");
    }

    public IEnumerator FadeInCurrentSprite()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        Color c = sr.color;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            sr.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        sr.color = new Color(c.r, c.g, c.b, 1f);
    }

    public void UpdatePlateSprite()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (spriteFadeRoutine != null)
        {
            StopCoroutine(spriteFadeRoutine);
            spriteFadeRoutine = null;
        }

        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, 1f);

        int noodleID = GetNoodleID();
        int sauceID = GetSauceID();
        int plateID = GetPlateID();
        bool hasPane = HasPane();

        if (noodleID == -1 || sauceID == -1 || plateID == -1)
        {
            Debug.LogWarning($"ID를 찾지 못함. noodleID={noodleID}, sauceID={sauceID}, plateID={plateID}");
            return;
        }

        // basic plate
        if (plateID == 501)
        {
            foreach (var entry in basicplateSpriteEntries)
            {
                if (entry.noodleID == noodleID &&
                    entry.sauceID == sauceID &&
                    entry.plateID == plateID &&
                    entry.hasPane == hasPane)
                {
                    if (entry.sprite != null)
                    {
                        sr.sprite = entry.sprite;
                        hasInitializedSprite = true;
                        Debug.Log($"basic plate 스프라이트 변경 완료: noodle={noodleID}, sauce={sauceID}, plate={plateID}, hasPane={hasPane}");
                    }
                    else
                    {
                        Debug.LogWarning($"basic plate 스프라이트가 비어있음: noodle={noodleID}, sauce={sauceID}, plate={plateID}, hasPane={hasPane}");
                    }
                    return;
                }
            }
        }
        // oven plate
        else if (plateID == 502)
        {
            foreach (var entry in ovenPlateSpriteEntries)
            {
                if (entry.noodleID == noodleID &&
                    entry.sauceID == sauceID &&
                    entry.plateID == plateID)
                {
                    if (entry.sprite != null)
                    {
                        sr.sprite = entry.sprite;
                        hasInitializedSprite = true;
                        Debug.Log($"oven plate 스프라이트 변경 완료: noodle={noodleID}, sauce={sauceID}, plate={plateID}");
                    }
                    else
                    {
                        Debug.LogWarning($"oven plate 스프라이트가 비어있음: noodle={noodleID}, sauce={sauceID}, plate={plateID}");
                    }
                    return;
                }
            }
        }

        Debug.LogWarning($"접시 스프라이트 매칭 실패: noodle={noodleID}, sauce={sauceID}, plate={plateID}, hasPane={hasPane}");
    }

    private void StopSpriteFadeAndRestoreAlpha()
    {
        if (spriteFadeRoutine != null)
        {
            StopCoroutine(spriteFadeRoutine);
            spriteFadeRoutine = null;
        }

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, 1f);
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
