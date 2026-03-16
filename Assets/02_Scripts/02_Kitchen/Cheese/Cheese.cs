using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Cheese: MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite parmesanSprite;
    [SerializeField] private Sprite parmesanselectedSprite;

    private SpriteRenderer sr;
    public bool isSelected { get; private set; }

    public bool CanBeSelected => true;

    public CheeseType cheeseType;
    public enum CheeseType
    {
        Parmesan,
        Mozzarella,
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Select();
            return true;
        }

        return false;
    }

    void Select()
    {
        isSelected = true;
        if(cheeseType == CheeseType.Parmesan)
        {
            sr.sprite = parmesanselectedSprite;
        }
        else
        {
            sr.color = Color.red;
        }
    }

    public void Sprinkle(Transform pastaPoint, System.Action onSprinkle)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 targetPos = pastaPoint.position + new Vector3(2f, 2f, 0);
        Quaternion pourRot = Quaternion.Euler(0, 0, 120f);

        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            sr.sprite = parmesanselectedSprite;
        });

        // 파스타 쪽으로 이동
        seq.Append(transform.DOMove(targetPos, 0.35f).SetEase(Ease.OutQuad));

        // 기울기
        seq.Append(transform.DORotateQuaternion(pourRot, 0.2f));

        // 흔들기 + 파슬리 생성
        seq.AppendCallback(() =>
        {
            onSprinkle?.Invoke();
        });

        seq.Append(
            transform.DOMoveY(targetPos.y + 0.08f, 0.18f)
            .SetLoops(4, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
        );

        // 다시 세우기
        seq.Append(transform.DORotateQuaternion(startRot, 0.2f));

        // 원위치 복귀
        seq.Append(transform.DOMove(startPos, 0.35f).SetEase(Ease.InQuad));

        seq.AppendCallback(() =>
        {
            sr.sprite = parmesanSprite;
        });
    }

    public void Cancel()
    {
        isSelected = false;
        if (cheeseType == CheeseType.Parmesan)
        {
            sr.sprite = parmesanSprite;

        }
        else
        {
            sr.color = Color.white;

        }
    }
}
