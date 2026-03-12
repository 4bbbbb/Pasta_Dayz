using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Plate_BakedPane : MonoBehaviour, IInteractable
{
    [Header("<<완성된 파스타 스폰위치>>")]
    [SerializeField] private Transform pastaSpawnPoint;

    private SpriteRenderer sr;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    private bool hasPasta = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("구워진 빠네 선택!");
            Select();
            return true;
        }

        if (target is FinishedPasta finishedPasta)
        {
            if (hasPasta)
            {
                Debug.Log("이미 파스타가 담겨 있어요!");
                return false;
            }

            finishedPasta.OnMovedToPlate();

            finishedPasta.transform.SetParent(pastaSpawnPoint);
            finishedPasta.transform.position = pastaSpawnPoint.position;
            hasPasta = true;

            return true;
        }

        return false;
    }
    void Select()
    {
        isSelected = true;
        sr.color = Color.red;
    }
    public void Cancel()
    {
        isSelected = false;
        sr.color = Color.white;
    }
}
