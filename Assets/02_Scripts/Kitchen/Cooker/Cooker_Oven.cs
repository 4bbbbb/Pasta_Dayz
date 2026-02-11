using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_Oven : MonoBehaviour, IInteractable
{
    [Header("<<구운 빠네 >>")]
    [SerializeField] public GameObject bakedPanePrefab;

    [Header("<<구운 파스타 >>")]
    [SerializeField] public GameObject bakedPastaPrefab;

    [Header("<<구운 빠네,파스타 스폰 위치>>")]
    [SerializeField] private Transform bakedSpawnPoint;

    private SpriteRenderer sr;
    private Collider ovenCollider;

    public bool CanBeSelected => false;

    private bool isBaking = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        ovenCollider = GetComponent<Collider>();
    }

    public bool Interact(IInteractable target)
    {     
        if (target == null)
        {
            Debug.Log("어떤걸 구울건가요?");
            return false;
        }

        if (isBaking)
        {
            Debug.Log("오븐이 이미 작동 중입니다!");
            return false;
        }

        if (target is Plate_Pane pane)
        {
            StartBaking(pane);
            return true;
        }

        if (target is FinishedPasta pasta)
        {            
            if (!pasta.IsOnOvenPlate())
            {
                Debug.Log("오븐 전용 그릇에 담겨야 합니다!");
                return false;
            }
           
            if (!pasta.HasMozzarella())
            {
                Debug.Log("모짜렐라 치즈가 필요합니다!");
                return false;
            }

            StartBaking(pasta); 
            return true;
        }               

        return false;

    }
    public void StartBaking(Plate_Pane pane)
    {
        OnBaking();
        StartCoroutine(BakingRoutine(pane));
    }

    public void StartBaking(FinishedPasta pasta)
    {
        OnBaking();
        StartCoroutine(BakingRoutine(pasta));
    }

    IEnumerator BakingRoutine(Plate_Pane pane)
    { 
        for (int i = 1; i <= 8; i++)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log($"{i}초...");
        }

        Instantiate(
            bakedPanePrefab,
            bakedSpawnPoint.position,
            Quaternion.identity,
            bakedSpawnPoint
        );

        StopBaking();
    }

    IEnumerator BakingRoutine(FinishedPasta pasta)
    {
        Plates_OvenPlate plate = pasta.GetComponentInParent<Plates_OvenPlate>();

        Destroy(plate.gameObject);

        for (int i = 1; i <= 8; i++)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log($"{i}초...");
        }

        Instantiate(
            bakedPastaPrefab,
            bakedSpawnPoint.position,
            Quaternion.identity,
            bakedSpawnPoint
        );
        
        StopBaking();
    }

    public void OnBaking()
    {
        isBaking = true;
        sr.color = Color.cyan;
        Debug.Log("오븐이 작동중입니다 노릇노릇 ~~ !");
    }

    public void StopBaking()
    {
        isBaking = false;
        sr.color = Color.white;        
        Debug.Log("오븐 작동이 끝났습니다 !");
    }

    public void Cancel()
    {

    }

}
