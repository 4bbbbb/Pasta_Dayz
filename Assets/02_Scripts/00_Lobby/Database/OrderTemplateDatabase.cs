using System.Collections.Generic;
using UnityEngine;

public class OrderTemplateDatabase : MonoBehaviour
{
    public static OrderTemplateDatabase Instance;

    [System.Serializable]
    public class TemplateData
    {
        public string category;
        public List<string> templates;
    }

    public List<TemplateData> templateList = new List<TemplateData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public string GetRandomTemplate(string category)
    {
        TemplateData data = templateList.Find(x => x.category == category);

        if (data == null)
        {
            Debug.LogError($"OrderTemplateDatabase: {category} 카테고리를 찾을 수 없음");
            return "";
        }

        if (data.templates == null || data.templates.Count == 0)
        {
            Debug.LogError($"OrderTemplateDatabase: {category} 템플릿이 비어 있음");
            return "";
        }

        return data.templates[Random.Range(0, data.templates.Count)];
    }
}