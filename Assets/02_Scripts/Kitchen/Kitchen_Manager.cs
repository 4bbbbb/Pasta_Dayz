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

        if (Physics.Raycast(ray, out RaycastHit hit))
        {                       
            IInteractable clicked = hit.collider.GetComponent<IInteractable>();

            if (clicked == null) return;

            Debug.Log(hit.collider.gameObject.name);

            // 이미 뭔가 선택 중일 때
            if (currentSelected != null)
            {
                // 같은 재료를 한번 더 선택했을 때
                if (currentSelected == clicked)
                {                    
                    return; // 아무것도 안함
                }

                bool used = currentSelected.Interact(clicked);

                if (used)
                {
                    // 사용 완료 : 손 비우기
                    currentSelected = null;
                }
                else
                {
                    // 사용 안함 : 선택중인 재료 교체
                    bool selected = clicked.Interact(null);

                    if (selected)
                    {
                        currentSelected.Cancel();
                        currentSelected = clicked;
                    }
                }

            }

            // 아무것도 선택 안하고 있을 때
            else
            {
                bool selected = clicked.Interact(null);

                if (selected)
                {                    
                    currentSelected = clicked;
                }

                // 재료를 알맞지 않은 기구에 가져가면 실행 취소됨
                else
                {                    
                    currentSelected.Cancel();
                    currentSelected = null;
                }

            }
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


