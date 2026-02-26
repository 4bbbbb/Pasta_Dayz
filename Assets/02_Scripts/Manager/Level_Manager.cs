using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level_Manager : MonoBehaviour
{
    public static Level_Manager Instance;
    public OrderManager orderManager;
    public LevelData levelData;

    [Header("UI")]
    public Text xpText;

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
            currentXP -= levelData.levelXPRequirements[currentLevel];
            currentLevel++;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (xpText != null)
        {
            xpText.text = $"XP: {xpText}";
        }
    }

    //public float getxpratio()
    //{
    //    return currentXP / requiredXp;
    //}



}
