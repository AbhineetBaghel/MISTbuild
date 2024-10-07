using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class changeWallet : MonoBehaviour
{
   public void changeSceneFromWallet()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
