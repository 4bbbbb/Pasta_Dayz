using System.Collections;
using UnityEngine;

public class Shader_Spread : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sauceRenderer;    

    [SerializeField] private float spreadDuration = 0.8f;

    private Material runtimeMat;
    private Coroutine spreadRoutine;

    private static readonly int RevealID = Shader.PropertyToID("_Reveal");
    private static readonly int CenterID = Shader.PropertyToID("_Center");

    private void Awake()
    {
        runtimeMat = new Material(sauceRenderer.sharedMaterial);
        sauceRenderer.material = runtimeMat;

        runtimeMat.SetFloat(RevealID, 0f);

        runtimeMat.SetVector(CenterID, new Vector2(0.5f, 0.7f));
        sauceRenderer.gameObject.SetActive(false);
    }

    public void PlayOilSpread()
    {
        if (spreadRoutine != null)
        {
            StopCoroutine(spreadRoutine);
        }

        sauceRenderer.gameObject.SetActive(true);
        spreadRoutine = StartCoroutine(CoSpread());
    }

    public void HideOil()
    {
        if (spreadRoutine != null)
            StopCoroutine(spreadRoutine);

        runtimeMat.SetFloat(RevealID, 0f);
        sauceRenderer.gameObject.SetActive(false);
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