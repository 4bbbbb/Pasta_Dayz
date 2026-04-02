using UnityEngine;
using UnityEngine.UI;

public class RecipeMenuButton : MonoBehaviour
{
    [SerializeField] private int menuID;
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        RecipeBook_UI.Instance.ShowRecipeByID(menuID);
    }
}