using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Kitchen_Manager : MonoBehaviour
{

    [SerializeField] private LayerMask interactableMask;

    public static Kitchen_Manager Instance;

    private IInteractable currentSelected;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        HandleLeftClick();
        HandleRightClick();
    }

    void HandleLeftClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (!IsValidInteractable(currentSelected))
            currentSelected = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        IInteractable clicked = hit.collider.GetComponentInParent<IInteractable>();

        if (clicked == null) return;

        if (currentSelected != null)
        {
            if (currentSelected == clicked)
            {
                return;
            }

            bool used = clicked.Interact(currentSelected);

            if (used)
            {
                SafeCancelCurrentSelected();
                currentSelected = null;
            }
            else
            {
                if (clicked.CanBeSelected)
                {
                    SafeCancelCurrentSelected();

                    if (IsValidInteractable(clicked))
                    {
                        clicked.Interact(null);
                        currentSelected = clicked;
                    }
                    else
                    {
                        currentSelected = null;
                    }
                }
                else
                {
                    SafeCancelCurrentSelected();
                    currentSelected = null;
                }
            }

            return;
        }

        if (clicked.CanBeSelected)
        {
            clicked.Interact(null);
            currentSelected = clicked;
        }
        else
        {
            clicked.Interact(null);
        }
    }

    private void HandleRightClick()
    {
        if (!Input.GetMouseButtonDown(1))
            return;

        if (currentSelected is UnityEngine.Object unityObj && unityObj == null)
        {
            currentSelected = null;
            return;
        }

        currentSelected?.Cancel();
        currentSelected = null;
    }

    private bool IsValidInteractable(IInteractable target)
    {
        if (target == null)
            return false;

        if (target is UnityEngine.Object unityObj && unityObj == null)
            return false;

        return true;
    }

    private void SafeCancelCurrentSelected()
    {
        if (!IsValidInteractable(currentSelected))
        {
            currentSelected = null;
            return;
        }

        currentSelected.Cancel();
    }

    public void ClearSelectionForTrashed(IInteractable trashedTarget)
    {
        if (currentSelected == null)
            return;

        // 이미 Destroy된 유니티 오브젝트면 그냥 참조만 제거
        if (currentSelected is UnityEngine.Object unityObj && unityObj == null)
        {
            currentSelected = null;
            return;
        }

        // 지금 버려지는 대상이 현재 선택된 대상이면 Cancel 호출하지 말고 바로 참조만 제거
        if (ReferenceEquals(currentSelected, trashedTarget))
        {
            currentSelected = null;
            return;
        }

        // 다른 게 선택돼 있으면 정상 취소
        SafeCancelCurrentSelected();
        currentSelected = null;
    }

    public void ClearSelection(IInteractable target)
    {
        if (currentSelected == target)
            currentSelected = null;
    }
}


