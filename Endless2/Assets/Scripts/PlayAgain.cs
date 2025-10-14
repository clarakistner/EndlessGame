using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
public class PlayAgain : MonoBehaviour
{

    public void ReiniciarJogo()
    {
        Debug.Log("Reiniciando...");
        Time.timeScale = 1;
        SceneManager.LoadScene("SampleScene");
    }
}