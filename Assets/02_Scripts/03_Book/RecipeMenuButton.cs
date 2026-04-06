using UnityEngine;
using UnityEngine.UI;

public class RecipeMenuButton : MonoBehaviour
{
    [SerializeField] private int menuID;
    [SerializeField] private Button button;

    [Header("버튼 안 음식 썸네일")]
    [SerializeField] private Image menuImage;

    [Header("잠금 표시")]
    [SerializeField] private GameObject lockObject;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickButton);
    }

    private void Start()
    {
        RefreshState();
    }

    public void RefreshState()
    {
        if (RecipeBook_UI.Instance == null)
            return;

        bool unlocked = RecipeBook_UI.Instance.IsMenuUnlocked(menuID);

        button.interactable = unlocked;

        if (menuImage != null)
            menuImage.enabled = unlocked;

        if (lockObject != null)
            lockObject.SetActive(!unlocked);
    }

    private void OnClickButton()
    {
        if (RecipeBook_UI.Instance == null)
            return;

        if (!RecipeBook_UI.Instance.IsMenuUnlocked(menuID))
            return;

        RecipeBook_UI.Instance.ShowRecipeByID(menuID);
    }
}