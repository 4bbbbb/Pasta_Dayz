using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OrderTemplateData;

public class OrderTemplateDatabase : MonoBehaviour
{
    public List<OrderTemplate> templateList = new List<OrderTemplate>();

    void Awake()
    {
        LoadTemplateData();
    }

    void LoadTemplateData()
    {
        var data = CSVReader.Read("Data/OrderTemplateData");

        foreach (var row in data)
        {
            OrderTemplate temp = new OrderTemplate();
            temp.type = row["OrderType"].ToString();
            temp.template = row["Template"].ToString();

            templateList.Add(temp);
        }
    }

    public string GetRandomTemplate(string type)
    {
        List<OrderTemplate> filtered = templateList.FindAll(t => t.type == type);

        if (filtered.Count == 0)
            return "";

        int rand = Random.Range(0, filtered.Count);
        return filtered[rand].template;
    }
}
