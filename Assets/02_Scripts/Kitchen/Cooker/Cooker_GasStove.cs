using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cooker_GasStove: MonoBehaviour
{
    public GameObject fryingPan;

    void Start()
    {        
        fryingPan.SetActive(false);
    }
    public void OnCLickStove()
    {
        fryingPan.SetActive(true);
    }
}
