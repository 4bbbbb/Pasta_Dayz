using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IInteractableScript
{
    public interface IInteractable
    {
        bool Interact(IInteractable target);
        // target == null : 선택 시도
        // target != null : 사용 시도 (재료 → 기기)
        // return true : 입력이 처리됨
        // return false : 처리할 상황X

        void Cancel();
        // 선택 취소

        bool CanBeSelected { get; }
        // true : 손에 들 수 있는 대상
        // false : 손에 들 수 없는 대상
    }
}
