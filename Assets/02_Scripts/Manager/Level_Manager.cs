using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level_Manager : MonoBehaviour
{
    public static Level_Manager Instance;

    public Order_Manager orderManager;
    public LevelData levelData;

    public ProfileUI profileUI;     

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

        while (currentLevel < levelData.levelXPRequirements.Count &&
               currentXP >= levelData.levelXPRequirements[currentLevel])
        {
            currentLevel++;
        }


        UpdateUI();
    }

    public void RegisterXPUI(ProfileUI ui)
    {
        profileUI = ui;
    }

    public void UpdateUI()
    {
        if (profileUI == null || levelData == null || levelData.levelXPRequirements.Count == 0) return;

        // 이전 레벨 XP (레벨 1이면 0)
        int prevLevelXP = (currentLevel > 1) ? levelData.levelXPRequirements[currentLevel - 1] : 0;
        // 현재 레벨 XP (리스트 범위 체크)
        int nextLevelXP = (currentLevel < levelData.levelXPRequirements.Count)
                          ? levelData.levelXPRequirements[currentLevel]
                          : prevLevelXP + 1;

        // UI 표시
        profileUI.lvText.text = $"Lv {currentLevel}";          // 그대로 출력
        profileUI.xpText.text = $"{currentXP - prevLevelXP}/{nextLevelXP - prevLevelXP}";
        profileUI.xpImage.fillAmount = (currentXP - prevLevelXP) / (float)(nextLevelXP - prevLevelXP);
    }

}
