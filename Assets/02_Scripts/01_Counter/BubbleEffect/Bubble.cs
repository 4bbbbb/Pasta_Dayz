using UnityEngine;
using DG.Tweening;

public class Bubble : MonoBehaviour
{
    SpriteRenderer sr;

    Vector3 minScale;
    Vector3 maxScale;

    float growTime;
    float delay;

    bool isActive = false;
    Vector3 originalPos;

    Tween delayTween;
    Tween vibrateTween;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalPos = transform.localPosition;

        SetupBubble();
    }


    public void StartBubble()
    {
        if (isActive) return;

        isActive = true;
        SetupBubble();
        Respawn();
    }

    public void StopBubble()
    {
        isActive = false;

        if (vibrateTween != null)
            vibrateTween.Kill();

        if (delayTween != null)
            delayTween.Kill();

        transform.DOKill();

        transform.DOScale(transform.localScale * 0.8f, 0.3f);
        sr.DOFade(0f, 0.3f);
    }

    void SetupBubble()
    {
        float type = Random.value;

        if (type < 0.2f)
        {
            minScale = new Vector3(0.1f, 0.1f, 1f);
            maxScale = new Vector3(0.25f, 0.25f, 1f);
            growTime = Random.Range(0.2f, 0.35f);
        }
        else if (type < 0.6f)
        {
            minScale = new Vector3(0.15f, 0.15f, 1f);
            maxScale = new Vector3(0.35f, 0.35f, 1f);
            growTime = Random.Range(0.4f, 0.65f);
        }
        else
        {
            minScale = new Vector3(0.2f, 0.2f, 1f);
            maxScale = new Vector3(0.5f, 0.5f, 1f);
            growTime = Random.Range(0.7f, 1.0f);
        }

        delay = Random.Range(0.2f, 1.2f);
    }

    void Respawn()
    {
        transform.localPosition = originalPos; 

        sr.color = new Color(1, 1, 1, 0);

        transform.localScale = minScale;

        StartVibration();

        delayTween = DOVirtual.DelayedCall(delay, PlayBubble);

    }


    void PlayBubble()
    {
        StopVibration();

        Sequence seq = DOTween.Sequence();

        seq.Append(sr.DOFade(1f, 0.1f));  

        seq.Append(transform.DOScale(maxScale, growTime)
            .SetEase(Ease.OutSine));

        seq.Append(transform.DOScale(maxScale * 1.2f, 0.1f));

        seq.Join(sr.DOFade(0f, 0.15f));

        seq.OnComplete(() =>
        {
            if (!isActive) return;

            SetupBubble();
            Respawn();
        });
    }


    void StartVibration()
    {
        float strength = maxScale.x * 0.05f;

        vibrateTween = transform.DOShakePosition(
            0.5f,
            new Vector3(strength, strength * 0.5f, 0),
            10,
            90,
            false,
            true
        ).SetLoops(-1, LoopType.Restart);
    }

    void StopVibration()
    {
        if (vibrateTween != null)
            vibrateTween.Kill();
    }
}
