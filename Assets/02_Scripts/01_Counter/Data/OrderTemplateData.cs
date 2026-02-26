using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderTemplateData : MonoBehaviour
{
    [System.Serializable]
    public class OrderTemplate
    {
        public string type;      // "Menu", "Noodle", "Topping"
        public string template;  // "{menu}하나 주세요."
    }
}
