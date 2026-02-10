using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static IInteractableScript;

public class Cooker_PastaCooker : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform cookedNoodleSpawnPoint;
    [SerializeField] public GameObject cookedNoodlePrefab;

    private SpriteRenderer sr;
    private Collider cookerCollider;

    public bool CanBeSelected => false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        cookerCollider = GetComponent<Collider>();
    }

    public bool Interact(IInteractable target)
    {
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
    public void Cancel()
    {

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

        Instantiate(
            cookedNoodlePrefab,
            transform.position,
            Quaternion.identity,
            cookedNoodleSpawnPoint
        );

        StopBowling();
    }

    public void OnBowling()
    {
        sr.color = Color.cyan;
    }

    public void StopBowling()
    {
        sr.color = Color.white;
        cookerCollider.enabled = false;
        Debug.Log("면이 다 익었습니다 !");
    }

}
