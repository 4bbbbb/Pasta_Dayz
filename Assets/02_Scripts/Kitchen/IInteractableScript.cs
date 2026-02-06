using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IInteractableScript
{
    public interface IInteractable
    {
        bool Interact(IInteractable target);
        // true : 손에 들 수 있는 대상
        // false : 손에 들지는 않고 행동만 함
        void Cancel();
    }
}
