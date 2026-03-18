using System.Collections;
using UnityEngine;

public class Shader_Spread : MonoBehaviour
{
    [SerializeField] private SpriteRenderer oilRenderer;
    [SerializeField] private float spreadDuration = 0.45f;

    private Material runtimeMat;
    private Coroutine spreadRoutine;

    private static readonly int RevealID = Shader.PropertyToID("_Reveal");

    private void Awake()
    {
        runtimeMat = new Material(oilRenderer.sharedMaterial);
        oilRenderer.material = runtimeMat;

        runtimeMat.SetFloat(RevealID, 0f);
        oilRenderer.gameObject.SetActive(false);
    }

    public void PlayOilSpread()
    {
        if (spreadRoutine != null)
            StopCoroutine(spreadRoutine);

        oilRenderer.gameObject.SetActive(true);
        spreadRoutine = StartCoroutine(CoSpread());
    }

    public void HideOil()
    {
        if (spreadRoutine != null)
            StopCoroutine(spreadRoutine);

        runtimeMat.SetFloat(RevealID, 0f);
        oilRenderer.gameObject.SetActive(false);
    }

    private IEnumerator CoSpread()
    {
        float t = 0f;
        runtimeMat.SetFloat(RevealID, 0f);

        while (t < spreadDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / spreadDuration);

            float reveal = Mathf.SmoothStep(0f, 0.85f, p);
            runtimeMat.SetFloat(RevealID, reveal);

            yield return null;
        }

        runtimeMat.SetFloat(RevealID, 0.85f);
        spreadRoutine = null;
    }
}