using UnityEngine;

public class Level_Manager : MonoBehaviour
{
    public static Level_Manager Instance;

    public LevelData levelData;

    [Header("LV, XP")]
    public int currentLevel = 1;
    public float currentXP = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EarnXP(float amount)
    {
        currentXP += amount;

        while (currentLevel < levelData.levelXPRequirements.Count && currentXP >= levelData.levelXPRequirements[currentLevel])
        {
            currentLevel++;
        }
    }

    public void GetXPInfo(out int level, out float current, out float max)
    {
        level = currentLevel;

        int prevLevelXP;
        int nextLevelXP;

        if (currentLevel > 1)
            prevLevelXP = levelData.levelXPRequirements[currentLevel - 1];        
        else
            prevLevelXP = 0; 

        if (currentLevel < levelData.levelXPRequirements.Count)
            nextLevelXP = levelData.levelXPRequirements[currentLevel];       
        else
            nextLevelXP = prevLevelXP + 1;       

        current = currentXP - prevLevelXP;
        max = nextLevelXP - prevLevelXP;
    }

    public void LoadLevelData(int savedLevel, float savedXP)
    {
        currentLevel = Mathf.Max(1, savedLevel);
        currentXP = Mathf.Max(0f, savedXP);
    }
}
