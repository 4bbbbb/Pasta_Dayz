using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noodles : MonoBehaviour
{
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void OnCLickNoodles()
    {
        sr.color = Color.red;
    }


    
}
