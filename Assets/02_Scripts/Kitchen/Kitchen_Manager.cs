using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Kitchen_Manager : MonoBehaviour
{
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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        IInteractable clicked = hit.collider.GetComponent<IInteractable>();
        if (clicked == null) return;

        Debug.Log(hit.collider.gameObject.name);

        // 1️. 이미 손에 뭔가 들고 있을 때
        if (currentSelected != null)
        {
            // 같은 거 다시 클릭 → 무시
            if (currentSelected == clicked)
            {
                return;
            }
                
            bool used = clicked.Interact(currentSelected);
            // 사용당하는 쪽이 판단

            if (used)
            {
                // 정상 사용
                currentSelected.Cancel();
                currentSelected = null;
            }
            else
            {
                if (clicked.CanBeSelected)
                {
                    // 재료 ↔ 재료 : 교체 선택
                    currentSelected.Cancel();
                    clicked.Interact(null);
                    currentSelected = clicked;
                }
                else
                {
                    // 재료 ↔ 잘못된 기기 : 취소
                    currentSelected.Cancel();
                    currentSelected = null;
                }
            }

            return;
        }

        // 2️. 아무것도 손에 안 들고 있을 때
        if (clicked.CanBeSelected)
        {
            // 선택 가능한 대상만 손에 든다
            clicked.Interact(null);   // 선택 연출
            currentSelected = clicked;
        }
        else
        {
            // 선택 불가 기기 → 그냥 행동만
            clicked.Interact(null);
        }
    }

    void HandleRightClick()
    {
        if (!Input.GetMouseButtonDown(1)) return;

        Debug.Log("실행취소");

        currentSelected?.Cancel(); 
        currentSelected = null;
    }
}


