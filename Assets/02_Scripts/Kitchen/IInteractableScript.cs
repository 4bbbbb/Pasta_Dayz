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

        void Cancel();
        // 선택 취소

        bool CanBeSelected { get; }
        // true : 손에 들 수 있는 대상
        // false : 손에 들 수 없는 대상
    }
}
