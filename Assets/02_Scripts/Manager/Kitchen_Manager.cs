using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static IInteractableScript;

public class Kitchen_Manager : MonoBehaviour
{
    [SerializeField] private LayerMask interactableMask;

    public static Kitchen_Manager Instance;

    private IInteractable currentSelected;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        HandleLeftClick();
        HandleRightClick();
    }

    private void HandleLeftClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        // UI 위를 클릭한 경우 월드 오브젝트 클릭 막기
        if (IsPointerOverUI())
            return;

        if (!IsValidInteractable(currentSelected))
            currentSelected = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableMask))
            return;

        IInteractable clicked = hit.collider.GetComponentInParent<IInteractable>();

        if (clicked == null)
            return;

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

        // UI 위를 클릭한 경우 우클릭 취소도 막기
        if (IsPointerOverUI())
            return;

        if (currentSelected is UnityEngine.Object unityObj && unityObj == null)
        {
            currentSelected = null;
            return;
        }

        currentSelected?.Cancel();
        currentSelected = null;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
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

        if (currentSelected is UnityEngine.Object unityObj && unityObj == null)
        {
            currentSelected = null;
            return;
        }

        if (ReferenceEquals(currentSelected, trashedTarget))
        {
            currentSelected = null;
            return;
        }

        SafeCancelCurrentSelected();
        currentSelected = null;
    }

    public void ClearSelection(IInteractable target)
    {
        if (currentSelected == target)
            currentSelected = null;
    }
}