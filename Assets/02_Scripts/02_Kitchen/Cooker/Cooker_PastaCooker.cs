using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static IInteractableScript;

public class Cooker_PastaCooker : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class NoodlePrefabData
    {
        public int id;
        public GameObject prefab;
    }

    [SerializeField] private List<NoodlePrefabData> noodlePrefabs;

    [Header("<<스폰위치>>")]
    [SerializeField] private Transform cookedNoodleSpawnPoint;
    

    private SpriteRenderer sr;

    private bool isCooking = false;

    public bool CanBeSelected => false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public bool Interact(IInteractable target)
    {
        if (isCooking)
        {
            Debug.Log($"{name}(이)가 이미 작동 중입니다!");
            return false;
        }

        if (target is Noodles noodles)
        {           
            StartBowling(noodles);
            return true; 
        }

        if (target == null)
        {
            Debug.Log("면을 선택해주세요");
            return false;
        }
        return false;
       
    }

    GameObject GetNoodlePrefab(int id)
    {
        foreach (var data in noodlePrefabs)
        {
            if (data.id == id)
                return data.prefab;
        }

        return null;
    }

    public void StartBowling(Noodles noodles)
    {
        OnBowling();
        StartCoroutine(BowlingRoutine(noodles));
    }

    IEnumerator BowlingRoutine(Noodles noodles)
    {       

        for (int i = 1; i <= 7; i++)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log($"{i}초...");
        }

        IngredientIDs id = noodles.GetComponent<IngredientIDs>();

        if (id != null)
        {
            GameObject prefab = GetNoodlePrefab(id.GetID());

            if (prefab != null)
            {
                Instantiate(
                    prefab,
                    cookedNoodleSpawnPoint.position,
                    Quaternion.identity,
                    cookedNoodleSpawnPoint
                );
            }
        }

        StopBowling();
    }

    public void OnBowling()
    {
        isCooking = true;
        Debug.Log("면이 삶아지고 있습니다. 보글보글 oOoOO ....");
        sr.color = Color.cyan;
    }

    public void StopBowling()
    {
        isCooking = false;
        sr.color = Color.white;
        Debug.Log("면이 다 익었습니다 !");
    }

    public void Cancel()
    {

    }
}
