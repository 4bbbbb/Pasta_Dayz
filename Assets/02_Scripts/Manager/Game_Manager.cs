using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlayerSaveData
{
    public string Nickname = "Player";
    public float Gold = 0f;
    public List<int> UnlockedItemIDs = new List<int>();
    public int Day = 0;
    public int Level = 1;
    public float XP = 0f;
}

public class Game_Manager : MonoBehaviour
{
    public static Game_Manager Instance;

    [Header("참조")]
    public Order_Manager orderManager;
    public Day_Manager dayManager;

    [Header("저장용 현재 값")]
    public string currentNickname = "Player";

    private PlayerSaveData loadedSaveData;
    private bool hasLoadedSave = false;
    private bool hasAppliedCoreData = false;

    private string SavePath => Path.Combine(Application.persistentDataPath, "saveData.json");

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();   
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    IEnumerator Start()
    {
        yield return null;
        ApplyCoreLoadedDataOnce();
        ApplyUnlockDataToDatabase();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
  

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyCoreLoadedDataOnce();  
        ApplyUnlockDataToDatabase(); 
    }

    public void SetNickname(string newNickname)
    {
        if (string.IsNullOrWhiteSpace(newNickname))
            return;

        currentNickname = newNickname.Trim();
    }

    public void SaveGame()
    {
        PlayerSaveData saveData = new PlayerSaveData();

        saveData.Nickname = currentNickname;

        if (Gold_Manager.Instance != null)
            saveData.Gold = Gold_Manager.Instance.totalGold;

        if (Day_Manager.Instance != null)
            saveData.Day = Day_Manager.Instance.GetCompletedDay();

        if (Level_Manager.Instance != null)
        {
            saveData.Level = Level_Manager.Instance.currentLevel;
            saveData.XP = Level_Manager.Instance.currentXP;
        }

        if (IngredientDatabase.Instance != null)
        {
            saveData.UnlockedItemIDs.Clear();

            foreach (var item in IngredientDatabase.Instance.ingredientList)
            {
                if (item.isUnlocked)
                    saveData.UnlockedItemIDs.Add(item.id);
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("저장 완료: " + SavePath);
        Debug.Log(json);
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("저장 파일 없음. 기본값 사용");
            hasLoadedSave = false;
            return;
        }

        string json = File.ReadAllText(SavePath);
        loadedSaveData = JsonUtility.FromJson<PlayerSaveData>(json);

        if (loadedSaveData == null)
            loadedSaveData = new PlayerSaveData();

        currentNickname = string.IsNullOrWhiteSpace(loadedSaveData.Nickname)
            ? "Player"
            : loadedSaveData.Nickname;

        hasLoadedSave = true;

        Debug.Log("불러오기 완료");
        Debug.Log(json);
    }

    private void ApplyCoreLoadedDataOnce()
    {
        if (!hasLoadedSave || loadedSaveData == null || hasAppliedCoreData)
            return;

        if (Gold_Manager.Instance != null)
            Gold_Manager.Instance.totalGold = loadedSaveData.Gold;

        if (Day_Manager.Instance != null)
            Day_Manager.Instance.LoadDayData(loadedSaveData.Day);

        if (Level_Manager.Instance != null)
            Level_Manager.Instance.LoadLevelData(loadedSaveData.Level, loadedSaveData.XP);

        hasAppliedCoreData = true;
    }

    private void ApplyUnlockDataToDatabase()
    {
        if (!hasLoadedSave || loadedSaveData == null)
            return;

        if (IngredientDatabase.Instance == null)
            return;

        HashSet<int> unlockedSet = new HashSet<int>(
            loadedSaveData.UnlockedItemIDs ?? new List<int>());

        foreach (var item in IngredientDatabase.Instance.ingredientList)
        {
            bool shouldUnlock = unlockedSet.Contains(item.id);
            IngredientDatabase.Instance.UpdateUnlockState(item.id, shouldUnlock);
        }

        if (Shop_Manager.Instance != null)
            Shop_Manager.Instance.UpdateShopUI();

        if (ToppingManager.Instance != null)
            ToppingManager.Instance.RefreshToppingUI();

    }

    public void ResetAllProgress()
    {
        currentNickname = "Player";

        if (Gold_Manager.Instance != null)
            Gold_Manager.Instance.totalGold = 0f;

        if (Day_Manager.Instance != null)
            Day_Manager.Instance.LoadDayData(0);

        if (Level_Manager.Instance != null)
            Level_Manager.Instance.LoadLevelData(1, 0f);

        if (IngredientDatabase.Instance != null)
            IngredientDatabase.Instance.ResetToDefaultFromCSV();

        DeleteSave();

        loadedSaveData = new PlayerSaveData();
        hasLoadedSave = false;
        hasAppliedCoreData = false;

        Debug.Log("저장 데이터 전체 초기화 완료");
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("세이브 삭제 완료");
        }

        if (Shop_Manager.Instance != null)
            Shop_Manager.Instance.UpdateShopUI();

        if (ToppingManager.Instance != null)
            ToppingManager.Instance.RefreshToppingUI();
    }    
}