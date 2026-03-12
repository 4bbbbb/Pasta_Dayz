using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static IInteractableScript;

public class Sauces : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;

    [Header("국자 연출")]
    [SerializeField] private SpriteRenderer ladleRenderer;
    [SerializeField] private Sprite emptyLadleSprite;     
    [SerializeField] private Sprite scoopedLadleSprite;

    public SauceType sauceType;
    public enum SauceType
    {
        None,
        Tomato,
        Cream,
        Rose,    
        Vongole,
    }

    private float riseDistance = 0.8f;
    private float riseDuration = 0.25f;
    private float returnDuration = 0.2f;

    private Vector3 ladleOriginalLocalPos;
    private Color ladleOriginalColor;
    private Sequence ladleSequence;
    private bool isAnimating = false;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (ladleRenderer != null)
        {
            ladleOriginalLocalPos = ladleRenderer.transform.localPosition;
            ladleOriginalColor = ladleRenderer.color;

            Color c = ladleRenderer.color;
            c.a = 0f;
            ladleRenderer.color = c;

            ladleRenderer.gameObject.SetActive(false);
        }
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
        if (isAnimating || isSelected)
        {
            return;
        }

        isSelected = true;

        PlayLadleSelectAnimation();
    }

    void PlayLadleSelectAnimation()
    {
        if (ladleRenderer == null || emptyLadleSprite == null || scoopedLadleSprite == null)
        {
            return;
        }

        isAnimating = true;

        if (ladleSequence != null)
        {
            ladleSequence.Kill();
        }

        ladleRenderer.gameObject.SetActive(true);
        ladleRenderer.transform.localPosition = ladleOriginalLocalPos + Vector3.down * riseDistance;
        ladleRenderer.sprite = emptyLadleSprite;

        Color startColor = ladleOriginalColor;
        startColor.a = 0f;
        ladleRenderer.color = startColor;

        ladleSequence = DOTween.Sequence();

        ladleSequence.Append(
            ladleRenderer.transform.DOLocalMove(ladleOriginalLocalPos, riseDuration).SetEase(Ease.OutSine)
        );

        ladleSequence.Join(
            ladleRenderer.DOFade(1f, riseDuration)
        );

        ladleSequence.AppendCallback(() =>
        {
            ladleRenderer.sprite = scoopedLadleSprite;
        });

        ladleSequence.Append(
            ladleRenderer.transform.DOLocalMove(
                ladleOriginalLocalPos + new Vector3(0f, 0.08f, 0f),
                0.12f
            ).SetEase(Ease.OutQuad)
        );

        ladleSequence.Append(
            ladleRenderer.transform.DOLocalMove(
                ladleOriginalLocalPos,
                0.1f
            ).SetEase(Ease.InOutSine)
        );

        ladleSequence.OnComplete(() =>
        {
            isAnimating = false;
        });
    }

    public void Cancel()
    {
        if (isAnimating)
        {
            if (ladleSequence != null)
            {
                ladleSequence.Kill();
            }
        }

        isSelected = false;
        isAnimating = true;

        // 이미 꺼져있으면 바로 종료
        if (!ladleRenderer.gameObject.activeSelf)
        {
            isAnimating = false;
            return;
        }

        if (ladleRenderer == null)
        {
            return;
        }

        ladleRenderer.sprite = emptyLadleSprite;

        ladleRenderer.transform.DOLocalMove(
            ladleOriginalLocalPos + Vector3.down * riseDistance,
            returnDuration
        ).SetEase(Ease.InSine);

        ladleRenderer.DOFade(0f, returnDuration).OnComplete(() =>
        {
            ladleRenderer.transform.localPosition = ladleOriginalLocalPos;
            Color c = ladleRenderer.color;
            c.a = 0f;
            ladleRenderer.color = c;
            isAnimating = false;
        });
    }
}