using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Topping_Parsley : MonoBehaviour, IInteractable
{
    [Header("<<파슬리 스프라이트>>")]
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite selectedSprite;

    private SpriteRenderer sr;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = originalSprite;
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
        sr.sprite = selectedSprite;
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
            sr.sprite = selectedSprite;
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
            sr.sprite = originalSprite;
        });
    }

    public void Cancel()
    {
        isSelected = false;
        sr.sprite = originalSprite;
    }
}
