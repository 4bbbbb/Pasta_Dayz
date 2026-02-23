using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomerButton : MonoBehaviour
{
    public void OnClickYesBtn()
    {
        SceneManager.LoadScene(2);
    }
}
