using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kitchen_Manager : MonoBehaviour
{
    [SerializeField] private Noodles noodles;
    [SerializeField] private Cooker_GasStove gasStove;
    
        
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                switch (hit.collider.gameObject.name)
                {
                    case "Noodle_Spaghetti":
                        noodles.OnCLickNoodles();
                        break;

                    case "Cooker_GasStove":
                        gasStove.OnCLickStove();
                        break;
                }
            }
        }

        if(Input.GetMouseButtonDown(1))
        {            

        }
        
    }
}
