using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public List<int> levelXPRequirements = new List<int>()
    {
        0,   
        70,  
        180,  
        300,
        450,
        650,

    };
}
